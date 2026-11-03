using UnityEngine;

public class LoadPlayerManager : MonoBehaviour
{
    public Transform levelSpawn;
    private PlayerController player;

    void Start()
    {
        

        player.transform.position = levelSpawn.position;
    }

    PlayerController FindPlayer()
    {
        PlayerController player = GetComponent<PlayerController>();
        return player;
    }
}
