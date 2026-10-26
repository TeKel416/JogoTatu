using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GameOverManager : MonoBehaviour
{
  
    public GameObject painelGameOver;

    public string Lobby = "Vila Ubiratinga";
   
    public float tempoAteVoltar = 3f;

    private PlayerHealth vidaJogador;

    void Start()
    {
        
        if (painelGameOver != null)
        {
            painelGameOver.SetActive(false);
        }

        vidaJogador = FindFirstObjectByType<PlayerHealth>();

        if (vidaJogador == null)
        {
            Debug.LogError("GameOverManager: não encontrei nenhum PlayerHealth na cena!");
            return;
        }

      
        vidaJogador.AoMorrer += IniciarGameOver;
    }

    void OnDestroy()
    {
        if (vidaJogador != null)
        {
            vidaJogador.AoMorrer -= IniciarGameOver;
        }
    }

    void IniciarGameOver()
    {
        
        StartCoroutine(SequenciaGameOver());
    }

    
   
    
    IEnumerator SequenciaGameOver()
    {
        if (painelGameOver != null)
        {
            painelGameOver.SetActive(true);
        }

        yield return new WaitForSeconds(tempoAteVoltar); 

        
        if (!Application.CanStreamedLevelBeLoaded(Lobby))
        {
            Debug.LogError("GameOverManager: a cena '" + Lobby + "' não está no Build Profiles ou o nome está errado.");
            yield break; // encerra a rotina
        }

        SceneManager.LoadScene(Lobby);
    }
}
