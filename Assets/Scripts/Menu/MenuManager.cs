using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[AddComponentMenu("Audio/Menu Manager")]
public class MenuManager : MonoBehaviour
{
    [Header("Cena do jogo")]
    [Tooltip("Nome exato da cena a carregar ao clicar em Play (precisa estar em Build Settings)")]
    public string gameSceneName = "Lobby";

    [Header("Painéis")]
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Áudio")]
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private const string MUSIC_PARAM = "MusicVolume";
    private const string SFX_PARAM = "SFXVolume";
    private const string MUSIC_PREF_KEY = "MusicVolume";
    private const string SFX_PREF_KEY = "SFXVolume";

    private void Start()
    {
        // 🎵 Toca a música tema do Menu automaticamente ao iniciar
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("MainMenu");
        }

        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        float savedMusic = PlayerPrefs.GetFloat(MUSIC_PREF_KEY, 0.75f);
        float savedSfx = PlayerPrefs.GetFloat(SFX_PREF_KEY, 0.75f);

        if (musicSlider != null)
        {
            musicSlider.value = savedMusic;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = savedSfx;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSfx);
    }

    public void PlayGame()
    {
        // Avisa no Console se o nome da cena estiver errado ou fora da Build List
        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError("A cena '" + gameSceneName + "' não foi encontrada. " +
                           "Confira o nome no Inspector do MenuManager e se ela está em File > Build Profiles > Scene List.", this);
            return;
        }

        // Garante que o jogo não fique congelado caso o menu tenha sido aberto de uma pausa
        Time.timeScale = 1f;

        // Só carrega a cena (a música do Lobby vai ser chamada quando o diálogo acabar!)
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void CloseAllPanels()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetMusicVolume(float sliderValue)
    {
        SetGroupVolume(MUSIC_PARAM, MUSIC_PREF_KEY, sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        SetGroupVolume(SFX_PARAM, SFX_PREF_KEY, sliderValue);
    }

    private void SetGroupVolume(string mixerParam, string prefKey, float sliderValue)
    {
        float dB = sliderValue > 0.0001f ? Mathf.Log10(sliderValue) * 20f : -80f;
        if (audioMixer != null)
        {
            audioMixer.SetFloat(mixerParam, dB);
        }
        PlayerPrefs.SetFloat(prefKey, sliderValue);
    }
}
