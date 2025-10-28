using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using URPGlitch;

public class CameraEffects : MonoBehaviour
{
    public static CameraEffects Instance;
    [SerializeField] private Volume volume;

    private bool glitchEnabled = false;
    private AnalogGlitchVolume analogGlitchVolume;
    private DigitalGlitchVolume digitalGlitchVolume;

    private void Start()
    {
        if (Instance != null || (GetComponentInChildren<Camera>() == null && GetComponentInChildren<CinemachineCamera>() == null))
        {
            Destroy(gameObject);
        }

        Instance = this;

        
        volume.profile.TryGet<AnalogGlitchVolume>(out analogGlitchVolume);
        volume.profile.TryGet<DigitalGlitchVolume>(out digitalGlitchVolume);
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

    public void SetDigitalGlitchIntensity(float value)
    {
        digitalGlitchVolume.intensity.value = value;
    }

    public void RandomizeDigitalGlitchIntensity(float value)
    {
        digitalGlitchVolume.intensity.value = Random.value;
    }

    public void SetAnalogGlitchJitter(float value)
    {
        analogGlitchVolume.scanLineJitter.value = value;
    }

    public void RandomizeAnalogGlitchJitter(float value)
    {
        analogGlitchVolume.scanLineJitter.value = Random.value;
    }

    public void SetAnalogGlitchJump(float value)
    {
        analogGlitchVolume.scanLineJitter.value = value;
    }

    public void RandomizeAnalogGlitchJump(float value)
    {
        analogGlitchVolume.scanLineJitter.value = Random.value;
    }

    public void SetAnalogGlitchShake(float value)
    {
        analogGlitchVolume.scanLineJitter.value = value;
    }

    public void RandomizeAnalogGlitchShake(float value)
    {
        analogGlitchVolume.scanLineJitter.value = Random.value;
    }

    public void SetAnalogGlitchDrift(float value)
    {
        analogGlitchVolume.scanLineJitter.value = value;
    }

    public void RandomizeAnalogGlitchDrift(float value)
    {
        analogGlitchVolume.scanLineJitter.value = Random.value;
    }
}
