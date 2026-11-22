using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class LoadPlayerManager : MonoBehaviour
{
    public Transform levelSpawn;
    public CinemachineCamera cineCamera;
    private GameObject player;

    void Awake()
    {
        player = FindPlayer();
        if (player) StartCoroutine(SpawnPlayer());
    }

    GameObject FindPlayer()
    {
        player = GameObject.Find("PlayerTatu");
        return player;
    }

    IEnumerator SpawnPlayer()
    {
        yield return null;

        if (levelSpawn != null)
        {
            player.transform.position = levelSpawn.position;
        }

        // Faz o vagalume encontrar o tatu e aparecer junto dele no checkpoint
        Vagalume vagalume = FindFirstObjectByType<Vagalume>();
        if (vagalume != null)
        {
            vagalume.TeletransportarParaAlvo(player.transform);
        }

        yield return null;

        if (cineCamera != null)
        {
            cineCamera.Target.TrackingTarget = player.transform.Find("TrackingTarget");
        }

        if (player.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        if (player.TryGetComponent(out PlayerHealth health))
        {
            health.RestaurarVidaCompleta();
        }

        yield return null;

        if (player.TryGetComponent(out PlayerController controller))
        {
            controller.enabled = true;
            controller.Renascer();
        }
    }
}