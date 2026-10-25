using UnityEngine;

public class InteracaoNPC : MonoBehaviour
{
    [Header("Dados do NPC")]
    [SerializeField] private string nomeDoNPC = "NPC";

    [Tooltip("Rosto do NPC mostrado na caixa de diálogo (opcional).")]
    [SerializeField] private Sprite retrato;

    [Tooltip("Cada item é uma fala. Aperte a tecla de interação para passar para a próxima.")]
    [TextArea(2, 5)]
    [SerializeField] private string[] falas;

    [Header("Interação")]
    [SerializeField] private KeyCode teclaInteracao = KeyCode.E;

    [Tooltip("Objeto filho do NPC com o 'E' flutuando em cima da cabeça dele.")]
    [SerializeField] private GameObject iconeInteragir;

    private bool jogadorEstaPerto = false;
    private bool conversando = false;

    private void Start()
    {
        MostrarIcone(false);

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("Não existe um DialogueManager na cena. Crie um GameObject com esse script.", this);
        }
    }

    // O Tatu entrou na área do NPC (o Collider do NPC precisa estar com 'Is Trigger' marcado)
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entrou na área: " + other.name);

        if (other.GetComponentInParent<PlayerController>() == null) return;

        jogadorEstaPerto = true;
        MostrarIcone(true);
    }

    // O Tatu saiu da área do NPC
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null) return;

        jogadorEstaPerto = false;
        MostrarIcone(false);
    }

    private void Update()
    {
        if (!jogadorEstaPerto || !Input.GetKeyDown(teclaInteracao)) return;

        DialogueManager dialogo = DialogueManager.Instance;

        if (!dialogo.EmDialogo)
        {
            // Começa a conversa
            conversando = true;
            MostrarIcone(false);
            dialogo.Iniciar(nomeDoNPC, falas, retrato);
        }
        else if (conversando)
        {
            // Só o NPC que começou a conversa avança as falas
            dialogo.ProximaFala();

            // Se a última fala acabou, libera o NPC e mostra o "E" de novo
            if (!dialogo.EmDialogo)
            {
                conversando = false;
                MostrarIcone(true);
            }
        }
    }

    private void MostrarIcone(bool mostrar)
    {
        if (iconeInteragir != null) iconeInteragir.SetActive(mostrar);
    }
}
