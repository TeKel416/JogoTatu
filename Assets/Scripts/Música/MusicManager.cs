using System.Collections;
using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;
}

[AddComponentMenu("Audio/Music Manager")]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Lista de Músicas")]
    public MusicTrack[] tracks; // Aqui está a aba de faixas que vai aparecer!

    [HideInInspector]
    public AudioSource musicSource;
    private Coroutine fadeCoroutine;
    private bool parando;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null) musicSource = GetComponent<AudioSource>();
    }

    public AudioClip GetClipFromName(string name)
    {
        if (tracks == null) return null;

        foreach (var track in tracks)
        {
            if (track.trackName == name)
            {
                return track.clip;
            }
        }
        return null;
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        AudioClip clip = GetClipFromName(trackName);

        if (clip == null)
        {
            Debug.LogWarning($"A música com o nome '{trackName}' não foi encontrada!");
            return;
        }

        // Se essa mesma música já está tocando, não reinicia
        if (!parando && musicSource.isPlaying && musicSource.clip == clip) return;

        parando = false;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(AnimateMusicCrossfade(clip, fadeDuration));
    }

    // NOVO: abaixa o volume aos poucos e para a música (silêncio)
    public void StopMusic(float fadeDuration = 0.5f)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        parando = true;
        fadeCoroutine = StartCoroutine(AnimateMusicStop(fadeDuration));
    }

    private IEnumerator AnimateMusicStop(float fadeDuration)
    {
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float percent = 0;

            while (percent < 1)
            {
                percent += Time.deltaTime / Mathf.Max(fadeDuration, 0.01f);
                musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
                yield return null;
            }
        }

        musicSource.Stop();
        musicSource.clip = null;
        parando = false;
    }

    private IEnumerator AnimateMusicCrossfade(AudioClip nextClip, float fadeDuration)
    {
        float percent = 0;

        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            while (percent < 1)
            {
                percent += Time.deltaTime / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
                yield return null;
            }
        }

        musicSource.clip = nextClip;
        musicSource.Play();

        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1f, percent);
            yield return null;
        }
    }
}
