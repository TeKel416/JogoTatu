using UnityEngine;

// Coloque num objeto da cena (ex: um objeto vazio chamado "MusicaDaCena").
// Ao iniciar a cena, toca a música escolhida (ou faz silêncio, se o nome ficar vazio).
public class TocarMusicaNaCena : MonoBehaviour
{
    [Tooltip("Nome da faixa na lista do MusicManager (campo Track Name). Vazio = silêncio.")]
    [SerializeField] private string nomeDaMusica = "Game";
    [SerializeField] private float duracaoFade = 0.5f;

    private void Start()
    {
        if (MusicManager.Instance == null) return;

        if (string.IsNullOrEmpty(nomeDaMusica))
            MusicManager.Instance.StopMusic(duracaoFade);
        else
            MusicManager.Instance.PlayMusic(nomeDaMusica, duracaoFade);
    }
}
