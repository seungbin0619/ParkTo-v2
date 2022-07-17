using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : SingleTon<SFXManager>
{
    [SerializeField]
    private AudioSource soundPrefab;

    private AudioSource bgm;
    private List<AudioSource> sounds = new List<AudioSource>();

    [System.Serializable]
    private class SoundData
    {
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1;
        public double[] trim = new double[2] { 0, -1 };
    }

    [SerializeField]
    private List<SoundData> bgmSources = new List<SoundData>();

    [SerializeField]
    private List<SoundData> soundSources = new List<SoundData>();

    public delegate void SFXEventHandler(float value);
    //public event SFXEventHandler OnMusicVolumeChanged = delegate { };
    public event SFXEventHandler OnSoundVolumeChanged = delegate { };

    private static readonly SoundData nullSound = new SoundData() { clip = null, volume = 1 };

    private SoundData currentBgm = null;
    private Coroutine bgmCoroutine = null;

    public float BgmVolume { private set; get; }
    public float SoundVolume { private set; get; }
    protected override void Awake()
    {
        base.Awake();

        bgm = GetComponent<AudioSource>();
        foreach (Transform child in transform)
            sounds.Add(child.GetComponent<AudioSource>());
    }

    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp(volume, 0, 1);

        if(currentBgm != null)
            bgm.volume = currentBgm.volume * BgmVolume;
        //OnMusicVolumeChanged?.Invoke(BgmVolume);
    }

    public void SetSoundVolume(float volume)
    {
        SoundVolume = Mathf.Clamp(volume, 0, 1);
        OnSoundVolumeChanged?.Invoke(SoundVolume);
    }

    public void PlayBgm(int index = -1, float duration = 0.5f, bool replay = false)
    {
        if (index < -1 || index >= bgmSources.Count) return;

        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
        }

        IEnumerator CoPlayBgm()
        {
            float progress = 0;
            while (currentBgm?.clip != null && progress < duration)
            {
                bgm.volume = (1 - progress / duration) *
                    currentBgm.volume *
                    BgmVolume;

                yield return YieldDictionary.WaitForEndOfFrame;
                progress += Time.deltaTime;
            }
            bgm.volume = 0;
            bgm.Stop();
            bgm.clip = null;

            progress = 0;

            if (index == -1) currentBgm = nullSound;
            else if (currentBgm == bgmSources[index] && !replay) yield break;
            else currentBgm = bgmSources[index];

            bgm.clip = currentBgm.clip;

            while (currentBgm.clip != null && progress < duration)
            {
                bgm.volume = progress / duration *
                    currentBgm.volume *
                    BgmVolume;

                yield return YieldDictionary.WaitForEndOfFrame;
                progress += Time.deltaTime;
            }

            bgm.volume = currentBgm.volume * BgmVolume;
            bgm.Play();
        }
        bgmCoroutine = StartCoroutine(CoPlayBgm());
    }

    public void PlaySound(int index = -1, float delay = 0)
    {
        if (index < 0 || index >= soundSources.Count) return;

        AudioSource medium = null;
        foreach (AudioSource audio in sounds)
        {
            if (audio.isPlaying) continue;

            medium = audio;
            break;
        }

        if (medium == null)
            medium = Instantiate(soundPrefab, transform);

        SoundData sound = soundSources[index];

        medium.volume = sound.volume * SoundVolume;
        medium.clip = sound.clip;

        medium.SetScheduledStartTime(sound.trim[0]);
        if (sound.trim[1] > sound.trim[0])
            medium.SetScheduledEndTime(sound.trim[1]);

        medium.PlayDelayed(delay);
    }
}