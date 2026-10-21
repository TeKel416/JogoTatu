using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public enum GameState
{
    Intro,       // cutscene rodando, jogo congelado
    Transition,  // fade entre a cutscene e o jogo
    Playing      // jogo normal
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Intro;

    [Header("Intro (cutscene)")]
    public VideoPlayer introVideo;
    [Tooltip("Objeto que mostra o vídeo (ex: a RawImage). Se o vídeo é desenhado direto na câmera, arraste o próprio objeto do VideoPlayer aqui.")]
    public GameObject videoDisplay;
    [Tooltip("Tecla para pular a cutscene")]
    public KeyCode skipKey = KeyCode.Return;

    [Header("Jogo")]
    [Tooltip("Tudo que só deve aparecer durante o jogo (ex: o TimerPanel)")]
    public GameObject hud;
    public PlayerController player;
    public CountdownTimer timer;

    [Header("Fade")]
    [Tooltip("Canvas Group da imagem preta que cobre a tela (FadeImage)")]
    public CanvasGroup fade;
    public float fadeDuration = 0.8f;

    private Coroutine introFadeRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BeginIntro();
    }

    private void Update()
    {
        if (State == GameState.Intro && Input.GetKeyDown(skipKey))
        {
            StartGame();
        }
    }

    private void OnDestroy()
    {
        if (introVideo != null)
        {
            introVideo.loopPointReached -= OnIntroEnded;
            introVideo.prepareCompleted -= OnIntroPrepared;
        }
    }

    private void BeginIntro()
    {
        // A cutscene já foi vista (ex: o personagem morreu e a cena recarregou): vai direto pro jogo
        if (CutsceneManager.JaTocou)
        {
            PularIntro();
            return;
        }

        State = GameState.Intro;

        // Cobre a tela de preto desde o primeiro frame, escondendo o jogo enquanto o vídeo carrega
        fade.alpha = 1f;

        // Congela tudo que é do jogo enquanto a cutscene roda
        player.enabled = false;
        timer.enabled = false;
        hud.SetActive(false);
        videoDisplay.SetActive(true);

        introVideo.isLooping = false;
        introVideo.loopPointReached += OnIntroEnded;

        // Prepara o vídeo primeiro e só dá o Play quando estiver carregado (evita travadas no começo)
        introVideo.prepareCompleted += OnIntroPrepared;
        introVideo.Prepare();
    }

    // Libera o jogo na hora, sem cutscene e sem fade
    private void PularIntro()
    {
        fade.alpha = 0f;
        if (videoDisplay != null) videoDisplay.SetActive(false);
        hud.SetActive(true);
        player.enabled = true;
        timer.enabled = true;
        State = GameState.Playing;
    }

    // Chamado automaticamente pelo VideoPlayer quando termina de carregar
    private void OnIntroPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnIntroPrepared;
        vp.Play();

        // Revela o vídeo com um fade a partir do preto
        introFadeRoutine = StartCoroutine(Fade(0f));
    }

    // Chamado automaticamente pelo VideoPlayer quando o vídeo termina
    private void OnIntroEnded(VideoPlayer vp)
    {
        StartGame();
    }

    public void StartGame()
    {
        // Evita rodar duas vezes (vídeo acabou ao mesmo tempo que apertou a tecla de pular)
        if (State != GameState.Intro) return;

        State = GameState.Transition;

        // Se pulou a cutscene durante o fade-in do vídeo, cancela esse fade para não brigar com o novo
        if (introFadeRoutine != null) StopCoroutine(introFadeRoutine);

        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        yield return Fade(1f);   // escurece a tela

        introVideo.Stop();
        videoDisplay.SetActive(false);
        hud.SetActive(true);

        yield return Fade(0f);   // clareia mostrando o jogo

        player.enabled = true;
        timer.enabled = true;
        State = GameState.Playing;
    }

    private IEnumerator Fade(float alvo)
    {
        float inicio = fade.alpha;
        float tempo = 0f;

        while (tempo < fadeDuration)
        {
            // unscaledDeltaTime: o fade continua funcionando mesmo com Time.timeScale = 0
            tempo += Time.unscaledDeltaTime;
            fade.alpha = Mathf.Lerp(inicio, alvo, tempo / fadeDuration);
            yield return null;
        }

        fade.alpha = alvo;
    }
}
