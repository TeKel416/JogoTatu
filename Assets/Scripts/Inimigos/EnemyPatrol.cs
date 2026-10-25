using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyPatrol : MonoBehaviour
{
    public float velocidade = 2f;


    public LayerMask camadaChao;
   
    public float distanciaParede = 0.2f;
 
    public float distanciaBorda = 0.6f;


    public int dano = 1;
  
    public float toleranciaPisao = 0.3f;

    public float forcaQuique = 10f;

    public float tempoParaSumir = 0.3f;

  
    public Transform modelo;

    private Rigidbody rb;
    private Collider col;
    private int direcao = 1; 
    private bool morto;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

        AtualizarVisual();
    }


    void FixedUpdate()
    {
        if (morto) return;

        if (DeveVirar())
        {
            Virar();
        }

        rb.linearVelocity = new Vector3(direcao * velocidade, rb.linearVelocity.y, 0f);
    }

    bool DeveVirar()
    {
        Bounds caixa = col.bounds; 
        Vector3 frente = Vector3.right * direcao;

       
        float distParede = caixa.extents.x + distanciaParede;
        if (Physics.Raycast(caixa.center, frente, distParede, camadaChao, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

     
        Vector3 origemBorda = new Vector3(
            caixa.center.x + direcao * (caixa.extents.x + 0.1f),
            caixa.min.y + 0.1f,
            caixa.center.z);

        bool temChao = Physics.Raycast(origemBorda, Vector3.down, distanciaBorda, camadaChao, QueryTriggerInteraction.Ignore);
        return !temChao;
    }

    void Virar()
    {
        direcao *= -1;
        AtualizarVisual();
    }

    void AtualizarVisual()
    {
        if (modelo == null) return;
        modelo.localRotation = Quaternion.Euler(0f, direcao > 0 ? 90f : -90f, 0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        TratarContato(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        TratarContato(collision);
    }

    void TratarContato(Collision collision)
    {
        if (morto) return;

     
        PlayerHealth vida = collision.collider.GetComponentInParent<PlayerHealth>();
        if (vida == null) return;

        PlayerController jogador = collision.collider.GetComponentInParent<PlayerController>();
        bool estaNoStomp = jogador != null && jogador.IsStomp;

        bool jogadorEmCima = collision.collider.bounds.min.y >= col.bounds.max.y - toleranciaPisao;

        if (estaNoStomp)
        {
            Die();
        }
        else if (jogadorEmCima)
        {
            QuicarJogador(collision);
            Die(); 
        }
        else
        {
            vida.TakeDamage(dano);
        }
    }

    void QuicarJogador(Collision collision)
    {
        Rigidbody rbJogador = collision.rigidbody;
        if (rbJogador == null) return;

        Vector3 v = rbJogador.linearVelocity;
        rbJogador.linearVelocity = new Vector3(v.x, forcaQuique, v.z);
    }

    public void Die()
    {
        if (morto) return;
        morto = true;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;  
        col.enabled = false;    
        Destroy(gameObject, tempoParaSumir);
    }
}
