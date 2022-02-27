using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXSystem : SingleTon
{
    #region [ 오브젝트 ]

    [SerializeField]
    private AudioSource soundPrefab;

    private AudioSource bgm;
    private List<AudioSource> sounds = new List<AudioSource>();

    #endregion

    #region [ SFX 데이터 ]

    [System.Serializable]
    private class SoundData
    {
        public AudioClip clip;
        public float volume = 1;
        public double[] trim = new double[2] { 0, -1 };
    }

    [SerializeField]
    private List<SoundData> bgmSources = new List<SoundData>();

    [SerializeField]
    private List<SoundData> soundSources = new List<SoundData>();

    #endregion

    private static readonly SoundData nullSound = new SoundData() { clip = null, volume = 1 };
    private readonly WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

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

    private void Start()
    {
        SetBgmVolume(1);
        SetSoundVolume(1);
    }

    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp(volume, 0, 1);
        bgm.volume = currentBgm.volume * BgmVolume;
    }

    public void SetSoundVolume(float volume) { SoundVolume = Mathf.Clamp(volume, 0, 1); }

    public void PlayBgm(int index = -1, float duration = 0.5f, bool replay = false)
    {
        if (index < -1 || index >= bgmSources.Count) return;

        if(bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
        }

        IEnumerator CoPlayBgm()
        {
            float progress = 0;
            while (currentBgm.clip != null && progress < duration)
            {
                bgm.volume = (1 - progress / duration) *
                    currentBgm.volume *
                    BgmVolume;

                yield return waitFrame;
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

                yield return waitFrame;
                progress += Time.deltaTime;
            }

            bgm.volume = currentBgm.volume * BgmVolume;
        }
        bgmCoroutine = StartCoroutine(CoPlayBgm());
    }

    public void PlaySound(int index = -1)
    {
        if (index < 0 || index >= soundSources.Count) return;

        AudioSource medium = null;
        foreach(AudioSource audio in sounds)
        {
            if (audio.isPlaying) continue;

            medium = audio;
            break;
        }

        if(medium == null) 
            medium = Instantiate(soundPrefab, transform);

        SoundData sound = soundSources[index];

        medium.volume = sound.volume * SoundVolume;
        medium.clip = sound.clip;

        medium.SetScheduledStartTime(sound.trim[0]);
        if (sound.trim[1] > sound.trim[0])
            medium.SetScheduledEndTime(sound.trim[1]);

        medium.Play();
    }
}
