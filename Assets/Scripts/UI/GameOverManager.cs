using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Interface")]
    public GameObject painelGameOver;

    [Header("Configuracao de Cena")]
    [Tooltip("Nome exato da cena do lobby no Build Profiles.")]
    public string nomeCenaLobby = "Lobby";
    public float tempoAteVoltar = 3f;

    private PlayerHealth vidaJogador;
    private bool gameOverIniciado = false;

    void Start()
    {
        if (painelGameOver != null)
        {
            painelGameOver.SetActive(false);
        }

        if (PlayerController.Instance != null)
        {
            vidaJogador = PlayerController.Instance.GetComponent<PlayerHealth>();
        }
        else
        {
            vidaJogador = FindFirstObjectByType<PlayerHealth>();
        }

        if (vidaJogador == null)
        {
            Debug.LogError("GameOverManager: nao encontrei nenhum PlayerHealth na cena!");
            return;
        }

        vidaJogador.AoMorrer += IniciarGameOver;
    }

    void OnDestroy()
    {
        if (vidaJogador != null)
        {
            vidaJogador.AoMorrer -= IniciarGameOver;
        }
    }

    void IniciarGameOver()
    {
        if (gameOverIniciado) return;
        gameOverIniciado = true;

        StartCoroutine(SequenciaGameOver());
    }

    IEnumerator SequenciaGameOver()
    {
        if (painelGameOver != null)
        {
            painelGameOver.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(tempoAteVoltar);

        if (painelGameOver != null)
        {
            painelGameOver.SetActive(false);
        }

        Time.timeScale = 1f;

        if (!Application.CanStreamedLevelBeLoaded(nomeCenaLobby))
        {
            Debug.LogError("GameOverManager: a cena '" + nomeCenaLobby + "' nao esta no Build Profiles ou o nome esta errado!");
            yield break;
        }

        LobbyRespawnManager.deveTocarAnimacaoRenascimento = true;
        SceneManager.LoadScene(nomeCenaLobby);
    }
}
