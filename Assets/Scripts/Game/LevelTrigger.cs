using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    public enum Action { Preload, Enter }
    [SerializeField] Action action;
    [SerializeField] int levelIndex;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (action == Action.Preload) LevelStreamer.Instance.Preload(levelIndex);
        else LevelStreamer.Instance.Enter(levelIndex);
    }
}
