using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class PlayAudio : MonoBehaviour
{
	[SerializeField]
	private AudioClip[] _audioClip;

	[SerializeField]
	private AudioMixerGroup _audioMixer;

	[SerializeField]
	private AudioSource _audioSource;
	[SerializeField] private float _minPitchVariation = 0.8f;
	[SerializeField] private float _maxPitchVariation = 1.2f;

	[SerializeField]
	private bool _isLoop;

	[SerializeField]
	private bool _playOnStart;

	private System.Random rnd;

	public float Pitch { get; private set; }

	private void Start()
	{
		rnd = new System.Random();
		_audioSource.outputAudioMixerGroup = _audioMixer;
		ApplyLoop();
		if (_playOnStart)
		{
			Play();
		}
	}

	public void Play()
	{
		if(_audioSource == null) return;
		if(rnd == null) Start();
		if(_audioClip.Length == 0) return;
		ChooseAVariant();
		_audioSource.Play();
	}

	public void Stop()
	{
		_audioSource.Stop();
	}

	private void ChooseAVariant()
	{
		_audioSource.clip = _audioClip[rnd.Next(0, _audioClip.Length)];
		Pitch = UnityEngine.Random.Range(_minPitchVariation, _maxPitchVariation);
		UpdatePitch();
	}

	private void ApplyLoop()
	{
		_audioSource.loop = _isLoop;
	}

	private void UpdatePitch()
	{
		_audioSource.pitch = Pitch;
	}

	public void SetPitch(float newPitch)
	{
		Pitch = newPitch;
		UpdatePitch();
	}
}
