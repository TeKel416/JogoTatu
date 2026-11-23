using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LobbyRespawnManager : MonoBehaviour
{
    public static bool deveTocarAnimacaoRenascimento = false;

    // Mantem compatibilidade caso algum script ainda chame o nome antigo
    public static bool morreuParaInimigoEspecifico
    {
        get { return deveTocarAnimacaoRenascimento; }
        set { deveTocarAnimacaoRenascimento = value; }
    }

    [Header("Referencias do Lobby")]
    public Transform pontoSpawnLobby;
    public CinemachineCamera cineCameraLobby;

    private IEnumerator Start()
    {
        // Espera 1 frame para garantir que o PlayerTatu duplicado da cena ja foi destruido
        yield return null;

        PlayerController controller = PlayerController.Instance;
        if (controller == null)
        {
            controller = FindFirstObjectByType<PlayerController>();
        }

        if (controller == null)
        {
            Debug.LogError("[LobbyRespawnManager] Nao encontrei nenhum PlayerController no Lobby!");
            yield break;
        }

        GameObject player = controller.gameObject;
        player.SetActive(true);

        // 1. Posiciona exatamente no SpawnPlayer do Lobby
        if (pontoSpawnLobby != null)
        {
            player.transform.position = pontoSpawnLobby.position;
        }

        // 2. Zera e destrava o Rigidbody
        if (player.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3. Reativa todos os Renderers e modelos 3D do tatu para ele nunca ficar invisivel
        foreach (Renderer r in player.GetComponentsInChildren<Renderer>(true))
        {
            if (r.gameObject != player)
            {
                r.enabled = true;
            }
        }

        if (controller.normalModel != null)
        {
            controller.normalModel.SetActive(true);
        }
        if (controller.ballModel != null)
        {
            controller.ballModel.SetActive(false);
        }

        // 4. Restaura a vida e reativa os controles
        if (player.TryGetComponent(out PlayerHealth health))
        {
            if (health.EstaMorto)
            {
                deveTocarAnimacaoRenascimento = true;
            }
            health.RestaurarVidaCompleta();
        }

        controller.enabled = true;

        // 5. Conecta a FollowCamera do Lobby ao jogador
        if (cineCameraLobby != null)
        {
            Transform target = player.transform.Find("TrackingTarget");
            cineCameraLobby.Target.TrackingTarget = target != null ? target : player.transform;
        }

        // 6. Traz o Vagalume para junto do tatu
        Vagalume vagalume = FindFirstObjectByType<Vagalume>();
        if (vagalume != null)
        {
            vagalume.TeletransportarParaAlvo(player.transform);
        }

        // 7. Dispara a animacao do Espirito da Mata (se veio de morte ou coleta de lixo)
        if (deveTocarAnimacaoRenascimento)
        {
            deveTocarAnimacaoRenascimento = false;
            controller.Renascer();
        }
    }
}
