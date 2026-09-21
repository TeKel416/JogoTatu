using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Cena do jogo")]
    [Tooltip("Nome exato da cena a carregar ao clicar em Play (precisa estar em Build Settings)")]
    public string gameSceneName = "Main";

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
        // Garante que nenhum painel de sub-menu comece aberto
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        // Carrega os volumes salvos (ou 0.75 como padrão na primeira vez)
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

    // Botão Play
    public void PlayGame()
    {
        MusicManager.Instance.PlayMusic("Game");

     
        SceneManager.LoadScene("Game");
    }

    // Botão Options
    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    // Botão Créditos
    public void OpenCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    // Botão Voltar, usado tanto no OptionsPanel quanto no CreditsPanel
    public void CloseAllPanels()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    // Botão Sair
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
