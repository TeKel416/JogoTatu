using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

// Coloque este script no mesmo objeto do Video Player (CutscenePlayer).
// Quando o vídeo termina: escurece a tela (fade) e carrega a próxima cena.
public class CutsceneFim : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("Image preta que cobre a tela toda (a FadeImage do FadeCanvas).")]
    [SerializeField] private Image fadeImage;

    [Header("Transição")]
    [Tooltip("Nome EXATO da cena que será carregada (precisa estar na Build List).")]
    [SerializeField] private string nomeDaCena = "Lobby";
    [SerializeField] private float duracaoFade = 1f;

    private bool terminou;

    private void Awake()
    {
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        videoPlayer.loopPointReached += AoTerminarVideo;
    }

    private void OnDisable()
    {
        videoPlayer.loopPointReached -= AoTerminarVideo;
    }

    // Chamado pelo Unity quando o vídeo chega no final
    private void AoTerminarVideo(VideoPlayer vp)
    {
        if (terminou) return;
        terminou = true;
        StartCoroutine(FadeECarregar());
    }

    private IEnumerator FadeECarregar()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);

            float tempo = 0f;
            Color cor = fadeImage.color;

            while (tempo < duracaoFade)
            {
                tempo += Time.unscaledDeltaTime;
                cor.a = Mathf.Clamp01(tempo / duracaoFade);
                fadeImage.color = cor;
                yield return null;
            }

            cor.a = 1f;
            fadeImage.color = cor;
        }

        SceneManager.LoadScene(nomeDaCena);
    }
}
