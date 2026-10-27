using UnityEngine;
using UnityEngine.UI; 


public class BarraDeCoracoes : MonoBehaviour
{
 
    public Image[] coracoes;

    
    public Color corCheio = new Color(0.9f, 0.15f, 0.2f, 1f);
    [Tooltip("Cor do coração perdido")]
    public Color corVazio = new Color(0.2f, 0.2f, 0.2f, 0.7f);

    private PlayerHealth vidaJogador;

    void Start()
    {
       
        vidaJogador = FindFirstObjectByType<PlayerHealth>();

        if (vidaJogador == null)
        {
            Debug.LogError("BarraDeCoracoes: não encontrei nenhum PlayerHealth na cena!");
            return;
        }

        
        vidaJogador.AoMudarVida += AtualizarCoracoes;

        AtualizarCoracoes(vidaJogador.VidaAtual, vidaJogador.vidaMaxima);
    }

    void OnDestroy()
    {
        if (vidaJogador != null)
        {
            vidaJogador.AoMudarVida -= AtualizarCoracoes;
        }
    }

    void AtualizarCoracoes(int vidaAtual, int vidaMaxima)
    {
        for (int i = 0; i < coracoes.Length; i++)
        {
            if (coracoes[i] == null) continue;

            coracoes[i].color = i < vidaAtual ? corCheio : corVazio;

            
            coracoes[i].enabled = i < vidaMaxima;
        }
    }
}
