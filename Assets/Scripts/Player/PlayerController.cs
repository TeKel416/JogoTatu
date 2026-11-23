using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Collider TatuCollider;
    public Collider BolaCollider; 

    public static PlayerController Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    //movimento
    public float velocidade = 5f;

    [Header("Configurações do Modo Bola")]
    [Tooltip("Velocidade máxima que a bolinha alcança (Substituiu o multiplicador para dar mais liberdade de tuning)")]
    public float velocidadeMaximaBola = 12f;
    [Tooltip("Quão rápido ela acelera até a velocidade máxima. Valores menores deixam mais pesado/lento para arrancar.")]
    public float aceleracaoBola = 8f;
    [Tooltip("Quão difícil é virar pro lado oposto. Valores menores fazem ele 'escorregar' mais metros antes de mudar de direção.")]
    public float inerciaCurvaBola = 4f;
    [Tooltip("Velocidade com que a bolinha gira visualmente conforme anda.")]
    public float velocidadeGiroBola = 120f;

    [Header("Configurações de Quebra de Parede")]
    [Tooltip("Velocidade horizontal mínima necessária para conseguir quebrar a parede de lado.")]
    public float velocidadeMinimaParaQuebrarParede = 10f;

    [Header("Peso no ar")]
    [Tooltip("Multiplicador de gravidade na subida do pulo")]
    public float gravidadeSubida = 1.5f;
    [Tooltip("Multiplicador de gravidade na queda. Maior = cai mais rápido")]
    public float gravidadeQueda = 2.5f;

    //pulo
    public float jumpForce = 12f;
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

    [Header("Renascimento")]
    [Tooltip("Efeito de luz + som quando o tatu volta da morte. Se ficar vazio, procura no próprio tatu.")]
    public EfeitoRenascimento efeitoRenascimento;

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
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Busca o Animator automaticamente nos filhos caso você esqueça de arrastar no Inspector
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (efeitoRenascimento == null)
        {
            efeitoRenascimento = GetComponent<EfeitoRenascimento>();
        }

        AtualizarVisual();
    }

    void Update()
    {
        Isfloor = Physics.Raycast(
           transform.position,
           Vector3.down,
           floorDistance,
           floorLayer);

        // --- CONTROLE DA ANIMAÇÃO DE ANDAR ---
        if (animator != null)
        {
            float entradaMovimento = Input.GetAxisRaw("Horizontal");
            bool estaAndando = Mathf.Abs(entradaMovimento) > 0.1f && Isfloor && !isManualBall && !IsStomp;
            animator.SetBool("isWalking", estaAndando);
        }

        // --- ALTERNAR MODO BOLINHA (TECLA F) ---
        if (Input.GetKeyDown(KeyCode.F) && Isfloor && !IsStomp)
        {
            Som2D("BolinhaTatu");
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

        // --- ROTAÇÃO VISUAL DA BOLINHA ---
        if ((isManualBall || IsStomp) && ballModel != null && ballModel.activeInHierarchy)
        {
            float giro = -rb.linearVelocity.x * velocidadeGiroBola * Time.deltaTime;
            ballModel.transform.Rotate(0f, 0f, giro, Space.World);
        }
    }

    void FixedUpdate()
    {
        // Salva a velocidade antes de aplicar a física do frame atual
        velocidadeUltimoFrameX = rb.linearVelocity.x;

        // Gravidade extra: pulo mais seco e queda mais pesada
        float multiplicador = rb.linearVelocity.y < 0f ? gravidadeQueda : gravidadeSubida;
        rb.AddForce(Physics.gravity * (multiplicador - 1f), ForceMode.Acceleration);

        if (!IsStomp)
        {
            float entradaMovimento = Input.GetAxisRaw("Horizontal");

            if (isManualBall)
            {
                float velocidadeAlvoX = entradaMovimento * velocidadeMaximaBola;

                float taxaMudancaVelocidade = aceleracaoBola;

                // Inércia ao trocar de direção
                if (entradaMovimento != 0 &&
                    rb.linearVelocity.x != 0 &&
                    Mathf.Sign(entradaMovimento) != Mathf.Sign(rb.linearVelocity.x))
                {
                    taxaMudancaVelocidade = inerciaCurvaBola;
                }

                float novoX = Mathf.MoveTowards(
                    rb.linearVelocity.x,
                    velocidadeAlvoX,
                    taxaMudancaVelocidade * Time.fixedDeltaTime
                );

                rb.linearVelocity = new Vector3(novoX, rb.linearVelocity.y, 0f);
            }
            else
            {
                // Movimento normal original
                float velocidadeX = entradaMovimento * velocidade;

                rb.linearVelocity = new Vector3(velocidadeX, rb.linearVelocity.y, 0f);
            }

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

    // --- DETECTAR IMPACTO LATERAL ---
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
                Destroy(collision.gameObject);
                Som3D("Break", transform.position);

                // Mantém um pouco da velocidade para ele não parar estagnado ao quebrar a parede
                rb.linearVelocity = new Vector3(velocidadeUltimoFrameX * 0.5f, rb.linearVelocity.y, 0f);
            }
        }
    }

    void Jump()
    {
        if (animator != null)
        {
            animator.SetTrigger("jump");
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        Som3D("Jump", transform.position);
    }

    void Stomp()
    {
        Som2D("Stomp");

        IsStomp = true;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(Vector3.down * stompForce, ForceMode.Impulse);

        AtualizarVisual();
    }

    void FinishStomp()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);

        foreach (var colisor in hitColliders)
        {
            Debug.Log(colisor.gameObject.tag);
            if (colisor.gameObject.CompareTag("Breakable"))
            {
                Destroy(colisor.gameObject);
                Som3D("Break", transform.position);
                rb.linearVelocity = Vector3.zero;
            }
        }

        IsStomp = false;
        isManualBall = false;
        AtualizarVisual();
    }

    void AtualizarVisual()
    {
        bool mostrarBola = IsStomp || isManualBall;

        rb.rotation = Quaternion.identity;

        rb.constraints = mostrarBola ? 
            RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY : 
            RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        
       
        
            
            normalModel.SetActive(!mostrarBola);
            TatuCollider.enabled = !mostrarBola;
        
            

       
            BolaCollider.enabled = mostrarBola;
            ballModel.SetActive(mostrarBola);
        
          
    }


    public void Renascer()
    {
        gameObject.SetActive(true);
        IsStomp = false;
        isManualBall = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        AtualizarVisual();

        if (efeitoRenascimento == null)
        {
            efeitoRenascimento = GetComponent<EfeitoRenascimento>();
        }

        if (efeitoRenascimento != null)
        {
            efeitoRenascimento.Tocar();
        }
    }

    void Som2D(string nome)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(nome);
        }
    }

    void Som3D(string nome, Vector3 pos)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound3D(nome, pos);
        }
    }
}
