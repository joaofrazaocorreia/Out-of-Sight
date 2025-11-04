using System;
using UnityEngine.Rendering;

namespace URPGlitch
{
    [Serializable]
    [VolumeComponentMenu("Analog Glitch")]
    public class AnalogGlitchVolume : VolumeComponent
    {
        public ClampedFloatParameter scanLineJitter = new(0f, 0f, 1f);
        public ClampedFloatParameter verticalJump = new(0f, 0f, 1f);
        public ClampedFloatParameter horizontalShake = new(0f, 0f, 1f);
        public ClampedFloatParameter colorDrift = new(0f, 0f, 1f);
    }
}

