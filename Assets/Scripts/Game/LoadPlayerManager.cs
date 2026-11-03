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
        Debug.Log("findplayer");
        player = GameObject.Find("PlayerTatu");
        Debug.Log(player);
        return player;
    }

    IEnumerator SpawnPlayer()
    {
        yield return new WaitForSeconds(0.1f);

        player.transform.position = levelSpawn.position;

        player.GetComponent<PlayerController>().enabled = true;
        cineCamera.Target.TrackingTarget = player.transform;
    }
}
