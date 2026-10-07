using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //movimento
    public float velocidade = 5f;

    [Header("Configurações do Modo Bola")]
    [Tooltip("Velocidade máxima que a bolinha alcança (Substituiu o multiplicador para dar mais liberdade de tuning)")]
    public float velocidadeMaximaBola = 12f;
    [Tooltip("Quão rápido ela acelera até a velocidade máxima. Valores menores deixam mais pesado/lento para arrancar.")]
    public float aceleracaoBola = 8f;
    [Tooltip("Quão difícil é virar pro lado oposto. Valores menores fazem ele 'escorregar' mais metros antes de mudar de direção.")]
    public float inerciaCurvaBola = 4f;

    [Header("Configurações de Quebra de Parede")]
    [Tooltip("Velocidade horizontal mínima necessária para conseguir quebrar a parede de lado.")]
    public float velocidadeMinimaParaQuebrarParede = 10f;

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

    // Guarda a velocidade do frame anterior porque no frame da colisão a física da Unity pode zerar a velocidade antes do OnCollisionEnter rodar
    private float velocidadeUltimoFrameX;

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
        if (Input.GetKeyDown(KeyCode.F) && Isfloor && !IsStomp)
        {
            isManualBall = !isManualBall;
            AtualizarVisual();
        }

        // --- PULO (W ou Seta para Cima) ---
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && Isfloor)
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
        // Salva a velocidade antes de aplicar a física do frame atual
        velocidadeUltimoFrameX = rb.linearVelocity.x;

        if (!IsStomp)
        {
            float entradaMovimento = Input.GetAxisRaw("Horizontal");

            float velocidadeAlvoX = 0f;
            float taxaMudancaVelocidade = 0f;

            if (isManualBall)
            {
                velocidadeAlvoX = entradaMovimento * velocidadeMaximaBola;

                bool mudandoDeDirecao = (entradaMovimento != 0 && Mathf.Sign(rb.linearVelocity.x) != Mathf.Sign(entradaMovimento));

                if (mudandoDeDirecao)
                {
                    taxaMudancaVelocidade = inerciaCurvaBola;
                }
                else
                {
                    taxaMudancaVelocidade = aceleracaoBola;
                }
            }
            else
            {
                velocidadeAlvoX = entradaMovimento * velocidade;
                taxaMudancaVelocidade = 100f;
            }

            float novoX = Mathf.MoveTowards(rb.linearVelocity.x, velocidadeAlvoX, taxaMudancaVelocidade * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(novoX, rb.linearVelocity.y, 0f);

            // --- LÓGICA DE DIRECIONAMENTO VISUAL ---
            if (rb.linearVelocity.x > 0.1f)
            {
                isFacingRight = true;
            }
            if (rb.linearVelocity.x < -0.1f)
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

    // --- NOVA FUNÇÃO: DETECTAR IMPACTO LATERAL ---
    void OnCollisionEnter(Collision collision)
    {
        // Verifica se o objeto batido tem a tag da parede quebrável
        if (collision.gameObject.CompareTag("WallBreakable"))
        {
            // Pega o valor absoluto da velocidade do frame anterior (ignora se é esquerda ou direita)
            float impactoHorizontal = Mathf.Abs(velocidadeUltimoFrameX);

            // Se a velocidade do impacto foi maior ou igual ao limite definido
            if (impactoHorizontal >= velocidadeMinimaParaQuebrarParede)
            {
                SoundManager.Instance.PlaySound3D("Break", transform.position);
                Destroy(collision.gameObject);

                // Mantém um pouco da velocidade para ele não parar estagnado ao quebrar a parede
                rb.linearVelocity = new Vector3(velocidadeUltimoFrameX * 0.5f, rb.linearVelocity.y, 0f);
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
        isManualBall = false;
        AtualizarVisual();
    }

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

        foreach (Collider colisor in objetosAtingidos)
        {
            Debug.Log(colisor.gameObject.tag);
        }
    }
}
