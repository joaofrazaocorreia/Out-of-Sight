// refered to:
//     https://github.com/keijiro/KinoGlitch.git
//     Assets/Kino/Glitch/DigitalGlitch.cs

using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace URPGlitch
{
    [Serializable]
    public sealed class DigitalGlitchFeature : ScriptableRendererFeature
    {
        [SerializeField] Shader shader;
        [SerializeField] Shader compatShader;
        [SerializeField] private RenderPassEvent renderPassEvent;
        private DigitalGlitchRenderPass _scriptablePass;

        public override void Create()
        {
            _scriptablePass = new DigitalGlitchRenderPass(shader, compatShader);
            _scriptablePass.renderPassEvent = renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (shader == null || compatShader == null)
            {
                Debug.LogWarning(this.name + " shader is null and will be skipped.");
                return;
            }

            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                renderer.EnqueuePass(_scriptablePass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _scriptablePass.Dispose();
        }
    }
}