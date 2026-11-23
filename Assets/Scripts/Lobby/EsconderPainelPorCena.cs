using UnityEngine;
using UnityEngine.SceneManagement;

// Para o Canvas que SOBREVIVE às trocas de cena (DontDestroyOnLoad).
// Esconde um painel filho dele (ex: o TimerPanel) quando a cena carregada for a indicada (ex: Lobby),
// e devolve o painel quando entrar em outra cena. Quem decide se o timer aparece nas fases
// continua sendo o CountdownTimer.
public class EsconderPainelPorCena : MonoBehaviour
{
    [Tooltip("Nome exato da cena em que o painel deve ficar escondido.")]
    [SerializeField] private string nomeDaCena = "Lobby";

    [Tooltip("O painel a esconder (o TimerPanel, filho deste Canvas).")]
    [SerializeField] private GameObject painel;

    private bool escondidoPorMim;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
    }

    private void Start()
    {
        Aplicar(SceneManager.GetActiveScene());
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        Aplicar(cena);
    }

    private void Aplicar(Scene cena)
    {
        if (painel == null) return;

        if (cena.name == nomeDaCena)
        {
            if (painel.activeSelf)
            {
                painel.SetActive(false);
                escondidoPorMim = true;
            }
        }
        else if (escondidoPorMim)
        {
            // Só devolve o que ESTE script escondeu; o CountdownTimer decide depois se mostra ou não
            painel.SetActive(true);
            escondidoPorMim = false;
        }
    }
}
