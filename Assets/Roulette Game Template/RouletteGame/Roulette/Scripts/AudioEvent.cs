using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Sound/SoundEvent")]
public class AudioEvent : ScriptableObject
{
    public AudioClip clip;
    public AudioMixerGroup mixerGroup;
    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0f, 1f)]
    public float pitch = 1f;

    public bool loop = false;

    public void PlayIn(AudioSource source)
    {
        if (clip == null)
            return;

        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.outputAudioMixerGroup = mixerGroup;
        source.Play();
    }

    public void PlayIn(AudioSource source, float volume)
    {
        if (clip == null)
            return;

        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.outputAudioMixerGroup = mixerGroup;
        source.Play();
    }
}
