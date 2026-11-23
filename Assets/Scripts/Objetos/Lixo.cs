using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Lixo : MonoBehaviour
{
    [Header("Cena de Destino")]
    [Tooltip("Nome exato da cena do Lobby no Build Profiles (ex: Lobby ou Vila Ubiratinga).")]
    public string nomeCenaLobby = "Lobby";
    public UnityEvent pararTimer;

    private bool coletado;

    void OnTriggerEnter(Collider collision)
    {
        if (coletado) return;

        PlayerController player = collision.GetComponentInParent<PlayerController>();
        if (player == null && !collision.CompareTag("Player")) return;

        coletado = true;

        if (ColetaUI.Instancia != null)
            ColetaUI.Instancia.MostrarColeta("Uma peça foi coletada!");

        CountdownTimer.PararEDesativarTimer();

        if (pararTimer != null)
        {
            pararTimer.Invoke();
        }

        LobbyRespawnManager.deveTocarAnimacaoRenascimento = true;

        if (Application.CanStreamedLevelBeLoaded(nomeCenaLobby))
        {
            Destroy(player.gameObject);
            SceneManager.LoadScene(nomeCenaLobby);
        }
        else
        {
            Debug.LogError("Lixo: A cena '" + nomeCenaLobby + "' nao esta no Build Profiles!");
        }
    }
}
