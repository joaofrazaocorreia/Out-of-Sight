using System;
using UnityEngine;
using UnityEngine.Audio;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private PlayAudio calmMusicPlayer;
    [SerializeField] private PlayAudio alarmMusicPlayer;
    private bool _isCalmPlaying = true;

    public void SwitchTrack()
    {
        if (_isCalmPlaying)
        {
            calmMusicPlayer.Stop();
            alarmMusicPlayer.Play();
            
            _isCalmPlaying = false;
        }
        else
        {
            alarmMusicPlayer.Stop();
            calmMusicPlayer.Play();
            _isCalmPlaying = true;
        }
    }
}
