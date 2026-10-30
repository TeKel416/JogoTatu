using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private VideoPlayer meuVideoPlayer;
    [Tooltip("Opcional: o tatu, para ficar parado enquanto a cutscene toca. Se ficar vazio, procura sozinho na cena")]
    [SerializeField] private PlayerController jogador;

    [Header("Configurações de Transição")]
    [SerializeField] private float tempoDoFade = 1.5f;
    [SerializeField] private string nomeDaProximaCena = ""; // Deixe em branco se for na mesma cena
    [Tooltip("Tecla para pular a cutscene")]
    [SerializeField] private KeyCode teclaPular = KeyCode.Return;

    private bool transicaoIniciada = false;
    private bool jogoLiberado = false;

    // static = pertence à classe, não ao objeto. O valor SOBREVIVE quando a cena é recarregada
    // (ex: o personagem morreu). Só volta a false quando o jogo é fechado ou se você chamar ResetarCutscene().
    private static bool cutsceneJaTocou = false;

    // Permite que outros scripts consultem se a cutscene já foi vista
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

        if (jogador == null)
            jogador = FindFirstObjectByType<PlayerController>();

        // Já tocou antes (ex: o personagem morreu e a cena recarregou): pula a cutscene
        if (cutsceneJaTocou)
        {
            meuVideoPlayer.Stop();
            meuVideoPlayer.gameObject.SetActive(false);
            return;
        }

        // Cobre a tela de preto já no começo, para o jogo não aparecer enquanto o vídeo carrega
        if (GDavini.Fader.Instance != null)
        {
            GDavini.Fader.FadeIn(0f, 0.01f, true);
        }

        // O tatu fica parado durante a cutscene
        TravarJogador(true);

        // Evento padrão da Unity para quando o vídeo acabar sozinho
        meuVideoPlayer.loopPointReached += IniciarTransicaoPeloVideo;

        // Play() já prepara o vídeo sozinho se ele ainda não estiver pronto
        meuVideoPlayer.Play();
        StartCoroutine(RevelarQuandoVideoComecar());
    }

    void Update()
    {
        if (!cutsceneJaTocou && !transicaoIniciada && Input.GetKeyDown(teclaPular))
        {
            ExecutarTransicao();
        }
    }

    // Espera o vídeo realmente começar a mostrar imagem e só então clareia a tela
    private IEnumerator RevelarQuandoVideoComecar()
    {
        float espera = 0f;

        while ((!meuVideoPlayer.isPlaying || meuVideoPlayer.frame < 1) && espera < 10f)
        {
            espera += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!meuVideoPlayer.isPlaying)
        {
            // Passou 10s e o vídeo não começou: segue para o jogo em vez de ficar preso na tela preta
            Debug.LogWarning("O vídeo não começou a tocar em 10 segundos. Pulando a cutscene.");
            ExecutarTransicao();
            yield break;
        }

        if (GDavini.Fader.Instance != null)
        {
            GDavini.Fader.FadeOut(0f, tempoDoFade * 0.5f, true);
        }

        // TRAVA DE SEGURANÇA: se o vídeo travar, força a transição depois da duração dele (+1s de folga)
        Invoke(nameof(ForcarTransicaoSegura), (float)meuVideoPlayer.length + 1f);
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
        // O Fader dispara o 'On End' sempre que QUALQUER fade termina. Só vale o primeiro fim
        // DEPOIS que a cutscene realmente terminou (transicaoIniciada), e só uma vez (jogoLiberado).
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

            // Devolve o controle ao tatu
            TravarJogador(false);
        }
    }

    private void TravarJogador(bool travar)
    {
        if (jogador == null) return;

        if (travar)
        {
            // Zera o movimento lateral para o tatu não sair deslizando com o script desligado
            Rigidbody rb = jogador.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        jogador.enabled = !travar;
    }
}
