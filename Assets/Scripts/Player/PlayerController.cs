using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //movimento
    public float velocidade = 5f;

    //pulo
    public float jumpForce = 7f;

    public float floorDistance = 0.6f;

    public LayerMask floorLayer;

    //stomp

    public float stompForce = 15f;
    public float stompDamage = 1f;
    public LayerMask camadaQuebravel;





    private Rigidbody rb;

    private bool Isfloor;

    public bool IsStomp;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        Isfloor = Physics.Raycast(
           transform.position,
           Vector3.down,
           floorDistance,
          floorLayer);


        float entradaMovimento = Input.GetAxis("Horizontal");

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && Isfloor)
        {
            Jump();
        }

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
        if (!IsStomp)
        {
            float entradaMovimento = Input.GetAxis("Horizontal");
            Vector3 novaVelocidade = new Vector3(entradaMovimento * velocidade, rb.linearVelocity.y, 0f);
            rb.linearVelocity = novaVelocidade;
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
    }

    void FinishStomp()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);

        foreach (var colisor in hitColliders)
        {
            if (colisor.gameObject.CompareTag("Breakable"))
            {
                Destroy(colisor.gameObject);
                rb.linearVelocity = Vector3.zero;

                SoundManager.Instance.PlaySound3D("Break", transform.position);
            }
        }

        IsStomp = false;

        //BreakForce();
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
            /*
            Breakable breakableObject = colisor.GetComponent<Breakable>();

            if (breakableObject != null)
            {
                breakableObject.ReceberDano(stompDamage);
            }
            */
        }

       

    }
}
