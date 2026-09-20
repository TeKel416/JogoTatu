using UnityEngine;

public class Breakable : MonoBehaviour
{
    [Header("Resistência")]
    public float vida = 1f;

    private bool foiQuebrado = false;

    

    public void ReceberDano(float dano)
    {
        // Se já foi quebrado, não faz nada
        if (foiQuebrado) return;

        vida -= dano;
        Debug.Log($"📉 Objeto sofreu dano! Vida restante: {vida}");

        if (vida <= 0)
        {
            foiQuebrado = true;
            Break();
        }
    }

    void Break()
    {
        Debug.Log("💥 Objeto quebrado!");
        Destroy(gameObject);
    }

    
}