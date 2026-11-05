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
        Debug.Log(player);
        return player;
    }

    IEnumerator SpawnPlayer()
    {
        yield return null;

        Debug.Log("old player: " + player.transform.position);
        player.transform.position = levelSpawn.position;
        Debug.Log("new player: " + player.transform.position);

        yield return null;

        cineCamera.Target.TrackingTarget = player.transform.Find("TrackingTarget");
        player.GetComponent<Rigidbody>().isKinematic = false;

        yield return null;

        player.GetComponent<PlayerController>().enabled = true;

        Debug.Log("final player: " + player.transform.position);
    }
}
