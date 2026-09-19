using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Cena do menu principal")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Áudio (mesmos campos do MenuManager)")]
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private const string MUSIC_PARAM = "MusicVolume";
    private const string SFX_PARAM = "SFXVolume";
    private const string MUSIC_PREF_KEY = "MusicVolume";
    private const string SFX_PREF_KEY = "SFXVolume";

    private bool isPaused;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

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
    }

    private void Update()
    {
        // Esc alterna entre pausado e jogando (mas não fecha se o Options estiver aberto,
        // pra Esc não pausar "por cima" do painel de opções por engano)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                CloseOptions();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void QuitToMainMenu()
    {
        // Importante: volta o tempo ao normal antes de trocar de cena,
        // senão o menu principal carrega já congelado
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
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
