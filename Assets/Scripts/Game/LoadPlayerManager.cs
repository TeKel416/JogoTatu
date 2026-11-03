using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class LoadPlayerManager : MonoBehaviour
{
    public Transform levelSpawn;
    public CinemachineCamera cineCamera;
    private GameObject player;
    
    void Start()
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
        player.GetComponent<Rigidbody>().isKinematic = true;

        yield return new WaitForSeconds(1f);

        Debug.Log("old player: "+player.transform.position);
        player.transform.position = levelSpawn.position;
        Debug.Log("new player: " + player.transform.position);

        player.GetComponent<PlayerController>().enabled = true;
        cineCamera.Target.TrackingTarget = player.transform;

        player.GetComponent<Rigidbody>().isKinematic = false;

        Debug.Log("final player: " + player.transform.position);
    }
}
