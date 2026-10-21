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
    private bool jogoLiberado = false;

    // static = pertence à classe, não ao objeto. O valor SOBREVIVE quando a cena é recarregada
    // (ex: o personagem morreu). Só volta a false quando o jogo é fechado ou se você chamar ResetarCutscene().
    private static bool cutsceneJaTocou = false;

    // Permite que outros scripts (ex: GameManager) consultem se a cutscene já foi vista
    public static bool JaTocou => cutsceneJaTocou;

    // Garante que o valor volte a false ao apertar Play no Editor, mesmo com o Domain Reload desligado
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarAoIniciar()
    {
        cutsceneJaTocou = false;
    }

    // Chame isto se quiser que a cutscene toque de novo (ex: ao começar um jogo novo pelo menu)
    public static void ResetarCutscene()
    {
        cutsceneJaTocou = false;
    }

    void Start()
    {
        if (meuVideoPlayer == null)
            meuVideoPlayer = GetComponent<VideoPlayer>();

        // Já tocou antes (ex: o personagem morreu e a cena recarregou): pula a cutscene
        if (cutsceneJaTocou)
        {
            meuVideoPlayer.Stop();
            meuVideoPlayer.gameObject.SetActive(false);
            return;
        }

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

        // Marca que a cutscene já foi vista, para não tocar de novo se a cena recarregar
        cutsceneJaTocou = true;

        CancelInvoke(nameof(ForcarTransicaoSegura)); // Cancela o timer de segurança se o vídeo funcionou

        // Executa o fade do professor de forma estática
        GDavini.Fader.FadeIn(0f, tempoDoFade);
    }

    // Certifique-se de que ESTA função esteja selecionada no evento 'On End ()' do Fader do seu professor
    public void IrParaOJogo()
    {
        // O Fader dispara o 'On End' sempre que QUALQUER fade termina, inclusive um fade "fantasma"
        // que ele faz sozinho logo que a cena começa. Só vale o primeiro fim DEPOIS que a cutscene
        // realmente terminou (transicaoIniciada), e só uma vez (jogoLiberado).
        if (!transicaoIniciada || jogoLiberado) return;
        jogoLiberado = true;

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
