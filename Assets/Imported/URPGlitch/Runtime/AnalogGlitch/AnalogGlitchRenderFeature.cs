using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace URPGlitch
{
    public class AnalogGlitchRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader anaglogGlitchShader;
        [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        private AnalogGlitchPass analogGlitchPass;

        public override void Create()
        {
            analogGlitchPass = new AnalogGlitchPass(anaglogGlitchShader);
            analogGlitchPass.renderPassEvent = renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            if (anaglogGlitchShader == null)
            {
                Debug.LogWarning(this.name + " shader is null and will be skipped.");
                return;
            }

            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                renderer.EnqueuePass(analogGlitchPass);
            }
        }

         protected override void Dispose(bool disposing)
        {
            analogGlitchPass.Dispose();
        }
    }
}
