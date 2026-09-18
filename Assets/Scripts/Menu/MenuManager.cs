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
    public Slider volumeSlider;

    private const string VOLUME_PREF_KEY = "MasterVolume";

    private void Start()
    {
        // Garante que nenhum painel de sub-menu comece aberto
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        // Carrega o volume salvo (ou 0.75 como padrão na primeira vez)
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_PREF_KEY, 0.75f);

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        SetVolume(savedVolume);
    }

    // Botão Play
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
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

    // Chamado automaticamente pelo Slider (via onValueChanged) sempre que o valor muda
    public void SetVolume(float sliderValue)
    {
        // Slider vai de 0 a 1 (linear); o AudioMixer trabalha em decibéis (logarítmico)
        // Evita log(0), que resultaria em -infinito
        float dB = sliderValue > 0.0001f ? Mathf.Log10(sliderValue) * 20f : -80f;

        if (audioMixer != null)
        {
            audioMixer.SetFloat(VOLUME_PREF_KEY, dB);
        }

        PlayerPrefs.SetFloat(VOLUME_PREF_KEY, sliderValue);
    }
}
