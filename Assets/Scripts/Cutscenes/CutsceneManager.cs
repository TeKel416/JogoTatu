using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private VideoPlayer meuVideoPlayer;

    [Header("Configurações de Transição")]
    [SerializeField] private float tempoDoFade = 1.5f;
    [SerializeField] private string nomeDaProximaCena = ""; // Deixe em branco se for na mesma cena

    private bool transicaoIniciada = false;

    void Start()
    {
        if (meuVideoPlayer == null)
            meuVideoPlayer = GetComponent<VideoPlayer>();

        // Evento padrão da Unity para quando o vídeo acabar sozinho
        meuVideoPlayer.loopPointReached += IniciarTransicaoPeloVideo;

        // TRAVA DE SEGURANÇA: Se o vídeo falhar, força o início após o tempo de duração dele
        Invoke(nameof(ForcarTransicaoSegura), (float)meuVideoPlayer.length);
    }

    void IniciarTransicaoPeloVideo(VideoPlayer vp)
    {
        ExecutarTransicao();
    }

    void ForcarTransicaoSegura()
    {
        if (!transicaoIniciada)
        {
            Debug.LogWarning("Segurança acionada: O vídeo travou ou não disparou o fim, forçando a transição.");
            ExecutarTransicao();
        }
    }

    void ExecutarTransicao()
    {
        if (transicaoIniciada) return; // Evita que rode duas vezes simultâneas
        transicaoIniciada = true;

        CancelInvoke(nameof(ForcarTransicaoSegura)); // Cancela o timer de segurança se o vídeo funcionou

        // Executa o fade do professor de forma estática
        GDavini.Fader.FadeIn(0f, tempoDoFade);
    }

    // Certifique-se de que ESTA função esteja selecionada no evento 'On End ()' do Fader do seu professor
    public void IrParaOJogo()
    {
        if (!string.IsNullOrEmpty(nomeDaProximaCena))
        {
            SceneManager.LoadScene(nomeDaProximaCena);
        }
        else
        {
            // Se for na mesma cena, desliga o vídeo e limpa a tela preta
            if (meuVideoPlayer != null)
            {
                meuVideoPlayer.Stop();
                meuVideoPlayer.gameObject.SetActive(false);
            }
            GDavini.Fader.FadeOut(0f, tempoDoFade);
        }
    }
}
