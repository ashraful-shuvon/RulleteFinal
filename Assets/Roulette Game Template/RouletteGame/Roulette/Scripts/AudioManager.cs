using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager _Instance;

    public int maxAudioSourcePool = 15;
    public AudioEvent[] audioEvent;
    private List<AudioSource> AudioSourcePool;
    public AudioSource AudioSourceBGM;
    private AudioSource auxiliarAS;

    public static float MusicVolume = 1;
    public static float SoundVolume = 1;

    public SceneRoulette scene;

    private AudioSource rolingSound;

    void Awake()
    {
        _Instance = this;

        AudioSourcePool = new List<AudioSource>();
        AudioSourceAlloc();
    }

    private void Start()
    {
        scene.musicToggle.onValueChanged.AddListener(ToggleVolumeSlider);
        scene.soundToggle.onValueChanged.AddListener(ToggleSound);
    }

    public void ToggleVolumeSlider(bool value)
    {
        scene.volumeSlider.gameObject.SetActive(value);
    }

    public void ToggleSound(bool value)
    {
        SoundVolume = value ? 0 : 1;
        if(auxiliarAS)
            auxiliarAS.volume = SoundVolume;
    }

    public void ChangeVolume()
    {
        MusicVolume = scene.volumeSlider.value;
        _Instance.AudioSourceBGM.volume = MusicVolume;
    }

    public void AudioSourceAlloc()
    {
        AudioSourcePool.Clear();
        
        for (int i = 0; i < maxAudioSourcePool; ++i)
        {
            AudioSource aS = gameObject.AddComponent<AudioSource>();
            aS.loop = false;
            AudioSourcePool.Add(aS);
        }
    }

    public AudioSource AudioSourcePop()
    {
        if (AudioSourcePool.Count <= 0) return null;
        
        AudioSource aS = AudioSourcePool[0];
        AudioSourcePool.RemoveAt(0);
        AudioSourcePool.Add(aS);

        return aS;
    }

    public static void SoundPlay(int iType)
    {
        AudioSource aS = _Instance.AudioSourcePop();
        if (iType == 2)
            _Instance.auxiliarAS = aS;
        _Instance.audioEvent[iType].PlayIn(aS, SoundVolume);
    }

    public static void StopAuxiliar()
    {
        _Instance.auxiliarAS.Stop();
    }

    public void SoundPlayCoroutine(int iType, float fDelay)
    {
        StartCoroutine(SoundPlayIn(iType, fDelay));
    }

    public IEnumerator SoundPlayIn(int iType, float fDelay)
    {

        if (fDelay > 0.0001f) yield return new WaitForSeconds(fDelay);

        SoundPlay(iType);
    }
    
    public static void MusicPlay()
    {
        if (MusicVolume == 0) return;
        _Instance.AudioSourceBGM.volume = MusicVolume;
        _Instance.AudioSourceBGM.Play();
    }

    public static bool MusicIsPlaying()
    {
        return _Instance.AudioSourceBGM.isPlaying;
    }

    public static void MusicStop()
    {
        _Instance.AudioSourceBGM.Stop();
    }
}
