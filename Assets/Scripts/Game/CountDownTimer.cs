using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CountdownTimer : MonoBehaviour
{
    [Tooltip("Tempo inicial em segundos. 180 = 3 minutos.")]
    public float startTime = 300f;
    public TextMeshProUGUI timerText;

    [Tooltip("Opcional: o painel inteiro do timer (TimerPanel). Se ficar vazio, esconde só o texto.")]
    [SerializeField] private GameObject painelDoTimer;

    // ---------------------------------------------------------------------
    // Estado COMPARTILHADO entre as cenas (static = sobrevive à troca de cena).
    // ---------------------------------------------------------------------
    private static float tempoRestante;
    private static bool iniciado;
    private static bool desativado;    // Controla se o timer deve estar ligado ou desligado no jogo
    private bool timerRunning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarAoIniciar()
    {
        iniciado = false;
        desativado = false;
    }

    public static void ResetarTempo()
    {
        iniciado = false;
    }

    // =========================================================================
    // COMANDOS ESTÁTICOS PARA SEUS OUTROS SCRIPTS (LIXO E POÇO)
    // =========================================================================

    public static void PararEDesativarTimer()
    {
        desativado = true;
        iniciado = false;

        CountdownTimer timer = Object.FindFirstObjectByType<CountdownTimer>();
        if (timer != null)
        {
            timer.enabled = false;
            timer.AplicarVisibilidade(false);
        }
    }

    public static void IniciarEAtivarTimer()
    {
        desativado = false;
        iniciado = false; // Garante que vai resetar para o startTime na nova fase

        CountdownTimer timer = Object.FindFirstObjectByType<CountdownTimer>();
        if (timer != null)
        {
            timer.enabled = true;
        }
    }

    // =========================================================================

    private void Start()
    {
        // VERIFICAÇÃO DE SEGURANÇA: Se estiver no Lobby, o timer NUNCA deve rodar ou aparecer
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            desativado = true;
            timerRunning = false;
            AplicarVisibilidade(false);
            enabled = false; // Desliga o Update deste script
            return;
        }

        if (desativado)
        {
            timerRunning = false;
            AplicarVisibilidade(false);
            enabled = false;
            return;
        }

        if (!iniciado)
        {
            tempoRestante = startTime;
            iniciado = true;
        }

        timerRunning = tempoRestante > 0f;
        AplicarVisibilidade(true);
        UpdateDisplay();
    }

    private void OnEnable()
    {
        // Se o objeto for ativado estando no Lobby, desliga imediatamente
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            desativado = true;
            timerRunning = false;
            AplicarVisibilidade(false);
            enabled = false;
            return;
        }

        if (desativado)
        {
            timerRunning = false;
            AplicarVisibilidade(false);
            enabled = false;
            return;
        }

        if (!iniciado)
        {
            tempoRestante = startTime;
            iniciado = true;
        }

        timerRunning = tempoRestante > 0f;
        AplicarVisibilidade(true);
        UpdateDisplay();
    }

    private void OnDisable()
    {
        timerRunning = false;
        AplicarVisibilidade(false);
    }

    private void Update()
    {
        // Alt + T liga/desliga o timer manualmente por teclado
        bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        if (alt && Input.GetKeyDown(KeyCode.T))
        {
            AlternarTimer();
        }

        if (desativado || !timerRunning) return;

        tempoRestante -= Time.deltaTime;

        if (tempoRestante <= 0f)
        {
            tempoRestante = 0f;
            timerRunning = false;
            UpdateDisplay();
            OnTimerEnd();
            return;
        }

        UpdateDisplay();
    }

    private void AlternarTimer()
    {
        desativado = !desativado;
        AplicarVisibilidade(!desativado);
    }

    private void AplicarVisibilidade(bool visivel)
    {
        // Se houver um painel inteiro associado, usa ele. Senão, usa o próprio texto.
        GameObject alvo = painelDoTimer != null ? painelDoTimer : (timerText != null ? timerText.gameObject : null);
        if (alvo == null) return;

        // Se você arrastou o Painel do Timer (GameObject), desativa/ativa ele diretamente para sumir tudo da tela
        if (painelDoTimer != null)
        {
            painelDoTimer.SetActive(visivel);
        }
        else
        {
            // Caso não tenha painel, desativa os componentes gráficos do texto
            foreach (Graphic g in alvo.GetComponentsInChildren<Graphic>(true))
            {
                g.enabled = visivel;
            }
        }
    }

    private void UpdateDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(tempoRestante / 60f);
        int seconds = Mathf.FloorToInt(tempoRestante % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnTimerEnd()
    {
        Debug.Log("Tempo esgotado! Destruindo jogador e recarregando o Lobby...");
        ResetarTempo();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Tatu");

        if (player != null)
        {
            Destroy(player);
        }

        SceneManager.LoadScene("Lobby");
    }

    public void PauseTimer()
    {
        timerRunning = false;
        AplicarVisibilidade(true);
        if (timerText != null) timerText.text = "PARABÉNS! VOCÊ VENCEU!";
        Invoke("GoToMenu", 5f);
    }

    void GoToMenu()
    {
        ResetarTempo();
        SceneManager.LoadScene("MainMenu");
    }
}