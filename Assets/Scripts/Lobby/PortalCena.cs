using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Anexe este script num objeto com um Collider marcado como 'Is Trigger' (ex: a boca do poço).
public class PortalCena : MonoBehaviour
{
    [Header("Destino")]
    [Tooltip("Nome exato da cena (ela precisa estar na lista de cenas do Build Settings)")]
    [SerializeField] private string nomeDaCena = "Game";

    [Header("Transição")]
    [Tooltip("Duração do escurecimento da tela antes de trocar de cena")]
    [SerializeField] private float tempoDoFade = 1f;

    [Header("Ativação")]
    [Tooltip("Desmarcado: a cena carrega assim que o tatu entra. Marcado: só ao apertar a tecla em cima do poço")]
    [SerializeField] private bool exigirTecla = false;
    [SerializeField] private KeyCode teclaInteracao = KeyCode.E;

    private bool carregando = false;
    private bool jogadorEmCima = false;
    private PlayerController jogador;

    // Roda uma vez, quando o script é adicionado no Editor: deixa o Collider como trigger
    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Jogador entrou no portal");

        if (!other.CompareTag("Player") || carregando) return;

        jogador = other.GetComponentInParent<PlayerController>();

        if (exigirTecla)
        {
            jogadorEmCima = true;
            return;
        }

        IniciarTransicao();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) jogadorEmCima = false;
    }

    private void Update()
    {
        if (exigirTecla && jogadorEmCima && !carregando && Input.GetKeyDown(teclaInteracao))
        {
            IniciarTransicao();
        }
    }

    private void IniciarTransicao()
    {
        carregando = true; // impede disparar duas vezes
        StartCoroutine(TransicaoRoutine());
    }

    private IEnumerator TransicaoRoutine()
    {
        // Trava o tatu para ele não andar durante a transição
        if (jogador != null)
        {
            Rigidbody rb = jogador.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            jogador.enabled = false;
        }

        // Escurece a tela com o Fader do professor, se existir na cena
        if (GDavini.Fader.Instance != null)
        {
            GDavini.Fader.FadeIn(0f, tempoDoFade, true);
            yield return new WaitForSecondsRealtime(tempoDoFade);
        }

        SceneManager.LoadScene(nomeDaCena);
    }
}
