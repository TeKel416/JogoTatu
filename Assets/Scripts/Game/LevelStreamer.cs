using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStreamer : MonoBehaviour
{
    public static LevelStreamer Instance { get; private set; }

    [SerializeField] string[] levelScenes; // "Level_01", "Level_02"...

    Rigidbody PlayerTatu;
    int current = -1;

    void Awake() => Instance = this;

    IEnumerator Start()
    {
        if (PlayerTatu) PlayerTatu.isKinematic = true;
        yield return Load(0);
        Enter(0);

        yield return new WaitForFixedUpdate();
        if (PlayerTatu) PlayerTatu.isKinematic = false;
    }

    bool IsLoaded(int i) => SceneManager.GetSceneByName(levelScenes[i]).isLoaded;

    IEnumerator Load(int index)
    {
        if (index < 0 || index >= levelScenes.Length || IsLoaded(index)) yield break;
        var op = SceneManager.LoadSceneAsync(levelScenes[index], LoadSceneMode.Additive);
        while (op != null && !op.isDone) yield return null;
    }

    public void Preload(int index) => StartCoroutine(Load(index));

    public void Enter(int index)
    {
        int previous = current;
        current = index;
        Preload(index + 1);

        if (previous >= 0 && previous != index && IsLoaded(previous))
            SceneManager.UnloadSceneAsync(levelScenes[previous]);
    }
}
