using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using URPGlitch;

public class CameraEffects : MonoBehaviour
{
    public static CameraEffects Instance;
    [SerializeField] private Volume volume;
    [SerializeField] private float defaultIntensity = 0;
    [SerializeField] private float defaultJitter = 0;
    [SerializeField] private float defaultJump = 0;
    [SerializeField] private float defaultShake = 0;
    [SerializeField] private float defaultDrift = 0;

    private bool glitchEnabled = false;
    public bool GlitchEnabled { get => glitchEnabled; }
    private PlayAudio glitchSFXPlayer;
    private AnalogGlitchVolume analogGlitchVolume;
    private DigitalGlitchVolume digitalGlitchVolume;

    private void Start()
    {
        if (Instance != null || (GetComponentInChildren<Camera>() == null && GetComponentInChildren<CinemachineCamera>() == null))
        {
            Destroy(gameObject);
        }

        Instance = this;

        glitchSFXPlayer = GetComponent<PlayAudio>();
        volume.profile.TryGet<AnalogGlitchVolume>(out analogGlitchVolume);
        volume.profile.TryGet<DigitalGlitchVolume>(out digitalGlitchVolume);
    }
    
    public void PlayGlitchSound()
    {
        glitchSFXPlayer.Play();
    }

    public void Shake(float duration, float posStrength = 2f, float rotStrength = 0.15f)
    {
        StopAllCoroutines();
        StartCoroutine(CameraShake(duration, posStrength, rotStrength));
    }

    private IEnumerator CameraShake(float duration, float posStrength, float rotStrength)
    {
        float elapsed = 0f;
        float magnitude = 1f;

        while (elapsed < duration)
        {
            float x = (Random.value - 0.5f) * magnitude * posStrength;
            float y = (Random.value - 0.5f) * magnitude * rotStrength;

            float lerpAmount = magnitude * rotStrength;
            Vector3 directionVector = Vector3.Lerp(Vector3.forward, Random.insideUnitCircle, lerpAmount);

            transform.localPosition = new Vector3(x, y, 0);
            transform.localRotation = Quaternion.LookRotation(directionVector);

            elapsed += Time.deltaTime;
            magnitude = (1 - (elapsed / duration)) * (1 - (elapsed / duration));

            yield return null;
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    
    public void InvertedShake(float duration, float posStrength = 2f, float rotStrength = 0.15f)
    {
        StopAllCoroutines();
        StartCoroutine(CameraInvertedShake(duration, posStrength, rotStrength));
    }

    private IEnumerator CameraInvertedShake(float duration, float posStrength, float rotStrength)
    {
        float elapsed = 0f;
        float magnitude = 1f;

        while (elapsed < duration)
        {
            float x = (Random.value - 0.5f) * magnitude * posStrength;
            float y = (Random.value - 0.5f) * magnitude * rotStrength;

            float lerpAmount = magnitude * rotStrength;
            Vector3 directionVector = Vector3.Lerp(Vector3.forward, Random.insideUnitCircle, lerpAmount);

            transform.localPosition = new Vector3(x, y, 0);
            transform.localRotation = Quaternion.LookRotation(directionVector);

            elapsed += Time.deltaTime;
            magnitude = (elapsed / duration) * (elapsed / duration);

            yield return null;
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void ToggleGlitchEffects()
    {
        glitchEnabled = !glitchEnabled;
        analogGlitchVolume.active = glitchEnabled;
        digitalGlitchVolume.active = glitchEnabled;
    }

    public void ResetAllEffects()
    {
        ResetDigitalGlitchIntensity();

        ResetAnalogGlitchJitter();
        ResetAnalogGlitchJump();
        ResetAnalogGlitchShake();
        ResetAnalogGlitchDrift();
    }

    public void SetDigitalGlitchIntensity(float value)
    {
        digitalGlitchVolume.intensity.value = value;
    }

    public void RandomizeDigitalGlitchIntensity()
    {
        digitalGlitchVolume.intensity.value = Random.value;
    }

    public void ResetDigitalGlitchIntensity()
    {
        digitalGlitchVolume.intensity.value = defaultIntensity;
    }

    public void SetAnalogGlitchJitter(float value)
    {
        analogGlitchVolume.scanLineJitter.value = value;
    }

    public void RandomizeAnalogGlitchJitter()
    {
        analogGlitchVolume.scanLineJitter.value = Random.value;
    }

    public void ResetAnalogGlitchJitter()
    {
        analogGlitchVolume.scanLineJitter.value = defaultJitter;
    }

    public void SetAnalogGlitchJump(float value)
    {
        analogGlitchVolume.verticalJump.value = value;
    }

    public void RandomizeAnalogGlitchJump(float value)
    {
        analogGlitchVolume.verticalJump.value = Random.value;
    }

    public void ResetAnalogGlitchJump()
    {
        analogGlitchVolume.verticalJump.value = defaultJump;
    }

    public void SetAnalogGlitchShake(float value)
    {
        analogGlitchVolume.horizontalShake.value = value;
    }

    public void RandomizeAnalogGlitchShake(float value)
    {
        analogGlitchVolume.horizontalShake.value = Random.value;
    }

    public void ResetAnalogGlitchShake()
    {
        analogGlitchVolume.horizontalShake.value = defaultShake;
    }

    public void SetAnalogGlitchDrift(float value)
    {
        analogGlitchVolume.colorDrift.value = value;
    }

    public void RandomizeAnalogGlitchDrift(float value)
    {
        analogGlitchVolume.colorDrift.value = Random.value;
    }

    public void ResetAnalogGlitchDrift()
    {
        analogGlitchVolume.colorDrift.value = defaultDrift;
    }
}
