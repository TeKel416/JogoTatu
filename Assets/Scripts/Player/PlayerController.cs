
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //movimento
    public float velocidade = 5f;

    [Tooltip("Multiplicador de velocidade quando o tatu estiver em forma de bolinha pelo botão F")]
    public float multiplicadorBolinha = 1.5f;

    //pulo
    public float jumpForce = 7f;
    public float floorDistance = 0.7f;
    public LayerMask floorLayer;

    //stomp
    public float stompForce = 15f;
    public float stompDamage = 1f;
    public LayerMask camadaQuebravel;

    [Header("Visual da bolinha")]
    [Tooltip("GameObject com o modelo normal do tatu (visível fora do stomp)")]
    public GameObject normalModel;
    [Tooltip("GameObject com o modelo/esfera representando o tatu enrolado (visível durante o stomp)")]
    public GameObject ballModel;

    [Header("Animator (opcional, para quando tiver animação de verdade)")]
    public Animator animator;

    private Rigidbody rb;
    private bool Isfloor;
    public bool IsStomp;

    private bool isFacingRight = true;

    // Controle do modo bolinha manual (Tecla F)
    private bool isManualBall = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        AtualizarVisual();
    }

    void Update()
    {
        Isfloor = Physics.Raycast(
           transform.position,
           Vector3.down,
           floorDistance,
           floorLayer);

        // --- ALTERNAR MODO BOLINHA (TECLA F) ---
        // Só permite ativar/desativar se estiver no chão e não estiver dando stomp
        if (Input.GetKeyDown(KeyCode.F) && Isfloor && !IsStomp)
        {
            isManualBall = !isManualBall;
            AtualizarVisual();
        }

        // --- PULO (W ou Seta para Cima) ---
        // Bloqueado se estiver transformado em bolinha
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && Isfloor && !isManualBall)
        {
            Jump();
        }

        // --- ATAQUE STOMP (Espaço) ---
        if (Input.GetKeyDown(KeyCode.Space) && !Isfloor && !IsStomp)
        {
            Stomp();
        }

        if (IsStomp && Isfloor)
        {
            FinishStomp();
        }
    }

    void FixedUpdate()
    {
        // O movimento lateral só trava completamente durante a queda do Stomp
        if (!IsStomp)
        {
            float entradaMovimento = Input.GetAxis("Horizontal");

            // Define qual velocidade usar com base no estado do tatu
            float velocidadeAtual = velocidade;
            if (isManualBall)
            {
                velocidadeAtual = velocidade * multiplicadorBolinha;
            }

            Vector3 novaVelocidade = new Vector3(entradaMovimento * velocidadeAtual, rb.linearVelocity.y, 0f);
            rb.linearVelocity = novaVelocidade;



            if (novaVelocidade.x > 0)
            {
                isFacingRight = true;
            }
            if (novaVelocidade.x < 0)
            {
                isFacingRight = false;
            }

            if (!isFacingRight) 
            {
                transform.localScale = new Vector3(-2, 2, 2);
            }
            else
            {
                transform.localScale = new Vector3(2, 2, 2);
            }

        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        SoundManager.Instance.PlaySound3D("Jump", transform.position);
    }

    void Stomp()
    {
        IsStomp = true;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(Vector3.down * stompForce, ForceMode.Impulse);

        AtualizarVisual();
    }

    void FinishStomp()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);

        foreach (var colisor in hitColliders)
        {
            if (colisor.gameObject.CompareTag("Breakable"))
            {
                SoundManager.Instance.PlaySound3D("Break", transform.position);
                Destroy(colisor.gameObject);
                rb.linearVelocity = Vector3.zero;
            }
        }

        IsStomp = false;

        // Reseta o modo bolinha manual caso ele estivesse ativo antes do pulo/stomp
        isManualBall = false;

        AtualizarVisual();
    }

    // Gerencia a troca de modelos 3D e variáveis do Animator
    void AtualizarVisual()
    {
        bool viradoBolinha = IsStomp || isManualBall;

        if (normalModel != null) normalModel.SetActive(!viradoBolinha);
        if (ballModel != null) ballModel.SetActive(viradoBolinha);

        if (animator != null)
        {
            animator.SetBool("IsBall", viradoBolinha);
            rb.freezeRotation = !viradoBolinha;

            if (!viradoBolinha)
            {
                transform.rotation = Quaternion.identity;
            }
        }
    }

    void BreakForce()
    {
        Vector3 tamanhoCaixa = new Vector3(1f, 0.3f, 1f);
        Vector3 centroCaixa = transform.position + Vector3.down * 0.5f;

        Collider[] objetosAtingidos = Physics.OverlapBox(
            centroCaixa,
            tamanhoCaixa * 0.5f,
            Quaternion.identity,
            camadaQuebravel
            );

        Debug.Log(objetosAtingidos.Length);

        foreach (Collider colisor in objetosAtingidos)
        {
            Debug.Log(colisor.gameObject.tag);
        }
    }
}