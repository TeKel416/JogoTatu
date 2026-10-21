using UnityEngine;

public class InteracaoNPC : MonoBehaviour
{
    private bool jogadorEstaPerto = false;

    // Detecta quando o Tatu entra na área do NPC
    private void OnTriggerEnter(Collider other)
    {
        // Certifique-se de que o seu Tatu tem a Tag "Player" configurada no Inspector
        if (other.CompareTag("Player"))
        {
            jogadorEstaPerto = true;
            Debug.Log("Pressione E para interagir com o NPC.");
        }
    }

    // Detecta quando o Tatu sai da área do NPC
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorEstaPerto = false;
            Debug.Log("Saiu de perto do NPC.");
        }
    }

    void Update()
    {
        // Se o jogador estiver perto E pressionar a tecla E
        if (jogadorEstaPerto && Input.GetKeyDown(KeyCode.E))
        {
            Interagir();
        }
    }

    void Interagir()
    {
        // Aqui vai acontecer a mágica no futuro (abrir diálogo, dar missão, etc.)
        Debug.Log("Interagindo com o NPC: Olá, Tatu! Bem-vindo à Vila!");
    }
}
