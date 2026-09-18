using UnityEngine;
using UnityEngine.SceneManagement; // Linha obrigatória para controle de cenas

public class GerenciadorCena : MonoBehaviour
{
    // Função que o botão vai chamar
    public void MudarDeCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }
}