using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int vidaMaxima = 3;
    public float tempoInvencivel = 1.5f;

    public event System.Action<int, int> AoMudarVida;
    public event System.Action AoMorrer;

    public int VidaAtual { get; private set; }
    public bool EstaMorto { get; private set; }

    private float invencivelAte;

    void Awake()
    {
        VidaAtual = vidaMaxima;
    }

    public void TakeDamage(int quantidade)
    {
        if (EstaMorto) return;
        if (Time.time < invencivelAte) return;

        VidaAtual = Mathf.Max(VidaAtual - quantidade, 0);
        invencivelAte = Time.time + tempoInvencivel;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("DanoTatu");
        }

        AoMudarVida?.Invoke(VidaAtual, vidaMaxima);

        if (VidaAtual == 0)
        {
            Die();
        }
    }

    void Die()
    {
        EstaMorto = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("MorteTatu");
        }

        if (TryGetComponent(out PlayerController controle))
        {
            controle.enabled = false;
        }

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        AoMorrer?.Invoke();
    }

    public void RestaurarVidaCompleta()
    {
        EstaMorto = false;
        VidaAtual = vidaMaxima;
        invencivelAte = 0f;
        AoMudarVida?.Invoke(VidaAtual, vidaMaxima);
    }
}
