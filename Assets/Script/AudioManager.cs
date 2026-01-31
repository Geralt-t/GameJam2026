using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSfxSource;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float defaultMusicVolume = 0.8f;
    [SerializeField] private float defaultSfxVolume = 1f;

    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    private Coroutine currentFadeCoroutine;

    // Playlist
    private List<string> musicPlaylist = new List<string>();
    private int currentTrackIndex = -1;
    private bool isAutoPlayNext = true;

    private string currentLoopingSFX;

    // PlayerPrefs keys
    private const string MUSIC_KEY = "MusicEnabled";
    private const string SFX_KEY = "SFXEnabled";
    private const string MUSIC_VOLUME_KEY = "MusicVolumeValue";
    private const string SFX_VOLUME_KEY = "SfxVolumeValue";


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (loopSfxSource == null) loopSfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = false;

        bool musicEnabled = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        bool sfxEnabled = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;

        // Load volume 0 → 1
        defaultMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, defaultMusicVolume);
        defaultSfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSfxVolume);

        musicSource.volume = musicEnabled ? defaultMusicVolume : 0f;
        sfxSource.volume = sfxEnabled ? defaultSfxVolume : 0f;
        loopSfxSource.volume = sfxEnabled ? defaultSfxVolume : 0f;

        StartCoroutine(CheckMusicCompletion());
    }

    #region === PLAYLIST ===
    public void AddToPlaylist(string clipName)
    {
        if (!musicPlaylist.Contains(clipName))
            musicPlaylist.Add(clipName);
    }

    public void PlayPlaylist(bool autoPlayNext = true)
    {
        if (musicPlaylist.Count == 0) return;
        isAutoPlayNext = autoPlayNext;
        currentTrackIndex = 0;
        PlayMusic(musicPlaylist[currentTrackIndex]);
    }

    private IEnumerator CheckMusicCompletion()
    {
        while (true)
        {
            if (IsMusicEnabled() && isAutoPlayNext &&
                musicSource.clip != null &&
                !musicSource.isPlaying &&
                musicPlaylist.Count > 0)
            {
                PlayNextTrack();
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void PlayNextTrack()
    {
        if (musicPlaylist.Count == 0) return;
        currentTrackIndex = (currentTrackIndex + 1) % musicPlaylist.Count;
        PlayMusic(musicPlaylist[currentTrackIndex]);
    }
    #endregion

    #region === MUSIC CONTROL ===
    public void PlayMusic(string clipName, bool fade = true)
    {
        if (!IsMusicEnabled()) return;

        AudioClip clip = GetAudioClip(clipName, "Music");
        if (clip == null) return;

        if (fade && musicSource.isPlaying)
            FadeMusic(clip);
        else
        {
            musicSource.clip = clip;
            musicSource.volume = defaultMusicVolume;
            musicSource.Play();
        }
    }

    public void StopMusic(bool fade = true)
    {
        if (!musicSource.isPlaying) return;
        if (fade) StartFade(0f, () => musicSource.Stop());
        else musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (IsMusicEnabled())
            musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (IsMusicEnabled())
            musicSource.UnPause();
    }

    private void FadeMusic(AudioClip newClip)
    {
        if (!IsMusicEnabled()) return;

        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeMusicCoroutine(newClip));
    }

    private IEnumerator FadeMusicCoroutine(AudioClip newClip)
    {
        yield return StartFade(0f);
        musicSource.clip = newClip;
        musicSource.Play();
        yield return StartFade(defaultMusicVolume);
    }
    #endregion

    #region === SFX ===
    public void PlaySFX(string clipName, float volumeScale = 1f, bool loop = false)
    {
        if (!IsSFXEnabled()) return;

        AudioClip clip = GetAudioClip(clipName, "SFX");
        if (clip == null) return;

        if (loop)
        {
            // D�nh cho �m l?p ??c bi?t
            loopSfxSource.clip = clip;
            loopSfxSource.loop = true;
            loopSfxSource.volume = defaultSfxVolume * volumeScale;
            loopSfxSource.Play();
            currentLoopingSFX = clipName;
        }
        else
        {
            sfxSource.PlayOneShot(clip, defaultSfxVolume * volumeScale);
        }
    }

    public void StopSFX(string clipName = "")
    {
        if (!string.IsNullOrEmpty(clipName) && clipName == currentLoopingSFX)
            loopSfxSource.Stop();

        sfxSource.Stop();
        currentLoopingSFX = null;
    }
    #endregion

    #region === TOGGLE & SETTINGS ===
    
    public void ToggleMusic()
    {
        bool newState = !IsMusicEnabled();
        PlayerPrefs.SetInt(MUSIC_KEY, newState ? 1 : 0);
        PlayerPrefs.Save();

        if (!newState)
        {
            StopMusic();
            musicSource.volume = 0f;
        }
        else
        {
            SetMusicVolume(defaultMusicVolume);
            if (musicSource.clip != null && !musicSource.isPlaying)
                musicSource.Play();
        }
    }

    public void ToggleSFX()
    {
        bool newState = !IsSFXEnabled();
        PlayerPrefs.SetInt(SFX_KEY, newState ? 1 : 0);
        PlayerPrefs.Save();

        float volume = newState ? defaultSfxVolume : 0f;
        sfxSource.volume = volume;
        loopSfxSource.volume = volume;
    }

    public bool IsMusicEnabled() => PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
    public bool IsSFXEnabled() => PlayerPrefs.GetInt(SFX_KEY, 1) == 1;
    #endregion

    #region === VOLUME & FADE ===

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, defaultMusicVolume);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSfxVolume);
    }
    public void SetMusicVolume(float volume)
    {
        defaultMusicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, defaultMusicVolume);
        PlayerPrefs.Save();

        if (IsMusicEnabled())
            musicSource.volume = defaultMusicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        defaultSfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, defaultSfxVolume);
        PlayerPrefs.Save();

        if (IsSFXEnabled())
        {
            sfxSource.volume = defaultSfxVolume;
            loopSfxSource.volume = defaultSfxVolume;
        }
    }


    private Coroutine StartFade(float targetVolume, System.Action onComplete = null)
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeCoroutine(targetVolume, onComplete));
        return currentFadeCoroutine;
    }

    private IEnumerator FadeCoroutine(float targetVolume, System.Action onComplete = null)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        onComplete?.Invoke();
    }
    #endregion

    #region === AUDIO CLIP LOAD ===
    private AudioClip GetAudioClip(string clipName, string path)
    {
        if (audioClips.TryGetValue(clipName, out var clip))
            return clip;

        clip = Resources.Load<AudioClip>($"Audio/{path}/{clipName}");
        if (clip != null)
            audioClips[clipName] = clip;
        else
            Debug.LogWarning($"Audio clip {clipName} not found in Resources/Audio/{path}/");

        return clip;
    }
    #endregion
}

