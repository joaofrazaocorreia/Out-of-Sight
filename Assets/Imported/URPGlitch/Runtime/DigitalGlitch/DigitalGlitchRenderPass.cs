using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace URPGlitch
{
    public class DigitalGlitchRenderPass : ScriptableRenderPass
    {
        const string k_DigitalPassName = "DigitalGlitch RenderPass";

        static readonly int MainTexID = Shader.PropertyToID("_MainTex");
        static readonly int NoiseTexID = Shader.PropertyToID("_NoiseTex");
        static readonly int TrashTexID = Shader.PropertyToID("_TrashTex");
        static readonly int IntensityID = Shader.PropertyToID("_Intensity");

        private readonly System.Random _random;
        private readonly Texture2D _noiseTexture;

        private Material material;
        private Material CompatMaterial;

        private RenderTextureDescriptor textureDescriptor;

        private RTHandle _mainFrame;
        private RTHandle _trashFrame1;
        private RTHandle _trashFrame2;

        private readonly DigitalGlitchVolume _volume;
        private static Vector4 scaleBias = new Vector4(1, 1, 0, 0);

        public DigitalGlitchRenderPass(Shader shader, Shader compatShader)
        {
            if (shader != null) material = CoreUtils.CreateEngineMaterial(shader);
            if (compatShader != null) CompatMaterial = CoreUtils.CreateEngineMaterial(compatShader);
            requiresIntermediateTexture = true;

            textureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);

            _random = new System.Random();

            _noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false)
            {
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            var volumeStack = VolumeManager.instance.stack;

            if (volumeStack == null)
            {
                return;
            }
            else
            {
                _volume = volumeStack.GetComponent<DigitalGlitchVolume>();
            }

            UpdateNoiseTexture();
        }

        #region Non-render graph code

        [System.Obsolete]
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var r = (float)_random.NextDouble();
            if (r > Mathf.Lerp(0.9f, 0.5f, _volume.intensity.value))
            {
                UpdateNoiseTexture();
            }

            textureDescriptor.width = cameraTextureDescriptor.width;
            textureDescriptor.height = cameraTextureDescriptor.height;

            RenderingUtils.ReAllocateIfNeeded(ref _mainFrame, textureDescriptor);
            RenderingUtils.ReAllocateIfNeeded(ref _trashFrame1, textureDescriptor);
            RenderingUtils.ReAllocateIfNeeded(ref _trashFrame2, textureDescriptor);
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var isPostProcessEnabled = renderingData.cameraData.postProcessEnabled;
            var isSceneViewCamera = renderingData.cameraData.isSceneViewCamera;

            if (!isPostProcessEnabled || isSceneViewCamera)
            {
                return;
            }

            var cmd = CommandBufferPool.Get(k_DigitalPassName);
            cmd.Clear();

            RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            cameraTargetDescriptor.depthBufferBits = 0;

            cmd.Blit(source, _mainFrame);

            var frameCount = Time.frameCount;
            if (frameCount % 13 == 0) cmd.Blit(source, _trashFrame1);
            if (frameCount % 73 == 0) cmd.Blit(source, _trashFrame2);

            var r = (float)_random.NextDouble();

            if (r > Mathf.Lerp(0.9f, 0.5f, _volume.intensity.value))
            {
                UpdateNoiseTexture();
            }

            var blitTrashHandle = r > 0.5f ? _trashFrame1 : _trashFrame2;

            CompatMaterial.SetFloat(IntensityID, _volume.intensity.value);
            CompatMaterial.SetTexture(NoiseTexID, _noiseTexture);
            CompatMaterial.SetTexture(MainTexID, _mainFrame);
            CompatMaterial.SetTexture(TrashTexID, blitTrashHandle);

            cmd.Blit(_mainFrame, source, CompatMaterial);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        #endregion

        #region Render graph code

        private class PassData
        {
            internal TextureHandle src;
        }

        private class TextureData
        {
            internal TextureHandle src;
            internal TextureHandle dst;
            internal int mainTexID;
            internal int TrashTexID;
            internal TextureHandle mainTex;
            internal TextureHandle trashTex;
            internal Material material;
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            Blitter.BlitTexture(context.cmd, data.src, scaleBias, 0, false);
        }

        static void ExecutePass(TextureData data, RasterGraphContext context, int pass)
        {
            data.material.SetTexture(data.mainTexID, data.mainTex);
            data.material.SetTexture(data.TrashTexID, data.trashTex);
            Blitter.BlitTexture(context.cmd, data.src, scaleBias, data.material, pass);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            var isPostProcessEnabled = frameData.Get<UniversalCameraData>().postProcessEnabled;
            var isSceneViewCamera = frameData.Get<UniversalCameraData>().isSceneViewCamera;
            if (!isPostProcessEnabled || isSceneViewCamera)
            {
                return;
            }

            if (resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogError($"Skipping render pass. BlitAndSwapColorRendererFeature requires an intermediate ColorTexture, we can't use the BackBuffer as a texture input.");
                return;
            }

            var r = (float)_random.NextDouble();

            if (r > Mathf.Lerp(0.9f, 0.5f, _volume.intensity.value))
            {
                UpdateNoiseTexture();
            }


            TextureHandle src = resourceData.activeColorTexture;
            TextureDesc destinationDesc = renderGraph.GetTextureDesc(src);
            destinationDesc.name = $"CameraColor-{k_DigitalPassName}";
            destinationDesc.clearBuffer = false;

            RenderTextureDescriptor renderTexDesc = frameData.Get<UniversalCameraData>().cameraTargetDescriptor;
            renderTexDesc.depthBufferBits = 0;

            TextureHandle mainFrame = UniversalRenderer.CreateRenderGraphTexture(renderGraph, renderTexDesc, "_MainFrame", false);
            TextureHandle trashFrame1 = UniversalRenderer.CreateRenderGraphTexture(renderGraph, renderTexDesc, "_TrashFrame1", false);
            TextureHandle trashFrame2 = UniversalRenderer.CreateRenderGraphTexture(renderGraph, renderTexDesc, "_TrashFrame2", false);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(k_DigitalPassName + "1", out PassData passData, profilingSampler))
            {
                passData.src = src;
                builder.UseTexture(src);
                builder.SetRenderAttachment(mainFrame, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
            var frameCount = Time.frameCount;
            if (frameCount % 13 == 0)
            {
                using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(k_DigitalPassName + "2", out PassData passData, profilingSampler))
                {
                    passData.src = src;
                    builder.UseTexture(src);
                    builder.SetRenderAttachment(trashFrame1, 0);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                }
            }
            if (frameCount % 73 == 0)
            {
                using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(k_DigitalPassName + "3", out PassData passData, profilingSampler))
                {
                    passData.src = src;
                    builder.UseTexture(src);
                    builder.SetRenderAttachment(trashFrame2, 0);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                }
            }

            var blitTrashHandle = r > 0.5f ? trashFrame1 : trashFrame2;

            material.SetFloat(IntensityID, _volume.intensity.value);
            material.SetTexture(NoiseTexID, _noiseTexture);


            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(k_DigitalPassName + "4", out TextureData data, profilingSampler))
            {
                data.src = mainFrame;
                data.dst = src;
                data.material = material;

                data.mainTex = mainFrame;
                data.mainTexID = MainTexID;

                data.trashTex = blitTrashHandle;
                data.TrashTexID = TrashTexID;

                builder.UseTexture(mainFrame);
                builder.UseTexture(blitTrashHandle);
                builder.SetRenderAttachment(src, 0);
                builder.SetRenderFunc((TextureData data, RasterGraphContext context) => ExecutePass(data, context, 0));
            }

        }

        #endregion

        void UpdateNoiseTexture()
        {
            var color = randomColor;

            for (var y = 0; y < _noiseTexture.height; y++)
            {
                for (var x = 0; x < _noiseTexture.width; x++)
                {
                    var r = (float)_random.NextDouble();
                    if (r > 0.89f)
                    {
                        color = randomColor;
                    }

                    _noiseTexture.SetPixel(x, y, color);
                }
            }

            _noiseTexture.Apply();
        }
        Color randomColor
        {
            get
            {
                var r = (float)_random.NextDouble();
                var g = (float)_random.NextDouble();
                var b = (float)_random.NextDouble();
                var a = (float)_random.NextDouble();
                return new Color(r, g, b, a);
            }
        }
        public void Dispose()
        {
            CoreUtils.Destroy(material);
            CoreUtils.Destroy(_noiseTexture);

            if (_mainFrame != null) _mainFrame.Release();
            if (_trashFrame1 != null) _trashFrame1.Release();
            if (_trashFrame2 != null) _trashFrame2.Release();

        }
    }
}
