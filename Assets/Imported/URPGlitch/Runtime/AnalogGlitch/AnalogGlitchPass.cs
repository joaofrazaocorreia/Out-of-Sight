using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace URPGlitch
{
    public class AnalogGlitchPass : ScriptableRenderPass
    {
        private const string k_AnalogPassName = "AnalogRenderPass";

        static readonly int ScanLineJitterID = Shader.PropertyToID("_ScanLineJitter");
        static readonly int VerticalJumpID = Shader.PropertyToID("_VerticalJump");
        static readonly int HorizontalShakeID = Shader.PropertyToID("_HorizontalShake");
        static readonly int ColorDriftID = Shader.PropertyToID("_ColorDrift");

        private Material analogGlitchMat;
        private RenderTextureDescriptor textureDescriptor;
        private RTHandle textureHandle;
        private float _verticalJumpTime;
        private static Vector4 scaleBias = new Vector4(1, 1, 0, 0);

        public AnalogGlitchPass(Shader shader)
        {
            if (shader != null) analogGlitchMat = CoreUtils.CreateEngineMaterial(shader);

            textureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
            requiresIntermediateTexture = true;
        }

        private void UpdateSettings()
        {
            if (analogGlitchMat == null)
            {
                Debug.LogError("update settings material null");
                return;
            }

            var _volume = VolumeManager.instance.stack.GetComponent<AnalogGlitchVolume>();

            var scanLineJitter = _volume.scanLineJitter.value;
            var verticalJump = _volume.verticalJump.value;
            var horizontalShake = _volume.horizontalShake.value;
            var colorDrift = _volume.colorDrift.value;

            //Scanline Jitter
            if (scanLineJitter > 0f)
            {
                var slThresh = Mathf.Clamp01(1.0f - scanLineJitter * 1.2f);
                var slDisp = 0.002f + Mathf.Pow(scanLineJitter, 3) * 0.05f;
                analogGlitchMat.SetVector(ScanLineJitterID, new Vector2(slDisp, slThresh));
            }
            else
            {
                analogGlitchMat.SetVector(ScanLineJitterID, Vector2.zero);
            }

            //Vertical Jump
            if (verticalJump > 0f)
            {
                _verticalJumpTime += Time.deltaTime * verticalJump * 11.3f;
                var vj = new Vector2(verticalJump, _verticalJumpTime);
                analogGlitchMat.SetVector(VerticalJumpID, vj);
            }
            else
            {
                analogGlitchMat.SetVector(VerticalJumpID, Vector2.zero);
            }

            //Horizontal Shake
            if (horizontalShake > 0f)
            {
                analogGlitchMat.SetFloat(HorizontalShakeID, horizontalShake * 0.2f);
            }
            else
            {
                analogGlitchMat.SetFloat(HorizontalShakeID, 0f);
            }

            //Color Drift
            if (colorDrift > 0f)
            {
                var cd = new Vector2(colorDrift * 0.04f, Time.time * 606.11f);
                analogGlitchMat.SetVector(ColorDriftID, cd);
            }
            else
            {
                analogGlitchMat.SetVector(ColorDriftID, Vector2.zero);
            }
        }


        #region Non-render graph code

        [System.Obsolete]
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            textureDescriptor.width = cameraTextureDescriptor.width;
            textureDescriptor.height = cameraTextureDescriptor.height;

            RenderingUtils.ReAllocateIfNeeded(ref textureHandle, textureDescriptor);
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

            CommandBuffer cmd = CommandBufferPool.Get();

            RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            cameraTargetDescriptor.depthBufferBits = 0;

            UpdateSettings();

            Blit(cmd, source, textureHandle);
            Blit(cmd, textureHandle, source, analogGlitchMat);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            CoreUtils.Destroy(analogGlitchMat);
            if (textureHandle != null) textureHandle.Release();
        }

        #endregion

        #region Render graph code

        private class PassData
        {
            internal TextureHandle source;
            internal Material material;
        }
        private static void ExecutePass(PassData data, RasterGraphContext context, int pass)
        {
            Blitter.BlitTexture(context.cmd, data.source, scaleBias, data.material, pass);
        }
        public override void RecordRenderGraph(RenderGraph renderGraph,
        ContextContainer frameData)
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
            var src = resourceData.activeColorTexture;
            var destinationDesc = renderGraph.GetTextureDesc(src);
            destinationDesc.name = $"CameraColor-{k_AnalogPassName}";
            destinationDesc.clearBuffer = false;

            TextureHandle dst = renderGraph.CreateTexture(destinationDesc);

            UpdateSettings();

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(k_AnalogPassName, out PassData passData, profilingSampler))
            {
                passData.source = src;
                passData.material = analogGlitchMat;

                builder.UseTexture(src);
                builder.SetRenderAttachment(dst, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context, 0));
            }

            resourceData.cameraColor = dst;
        }

        #endregion
    }
}
