using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject painelDialogo;
    [SerializeField] private TextMeshProUGUI textoNome;
    [SerializeField] private TextMeshProUGUI textoFala;
    [Tooltip("Imagem do rosto do personagem (filha do painel de diálogo).")]
    [SerializeField] private Image imagemRetrato;
    [Tooltip("Aviso 'Pressione E para conversar'. Deve ficar FORA do painel de diálogo.")]
    [SerializeField] private GameObject avisoInteragir;

    [Header("Efeito de digitação")]
    [SerializeField] private float segundosPorLetra = 0.03f;

    [Header("Congelar durante a conversa")]
    [SerializeField] private PlayerController jogador;
    [SerializeField] private CountdownTimer timer;
    [Tooltip("Se marcado, o timer para enquanto o jogador conversa")]
    [SerializeField] private bool pausarTimer = true;

    public bool EmDialogo { get; private set; }

    private string[] falasAtuais;
    private int indice;
    private bool digitando;
    private Coroutine rotinaDigitacao;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        painelDialogo.SetActive(false);
        if (avisoInteragir != null) avisoInteragir.SetActive(false);
    }

    // Liga/desliga o aviso "Pressione E". Chamado pelo NPC.
    public void MostrarPrompt(bool mostrar)
    {
        if (avisoInteragir != null && avisoInteragir.activeSelf != mostrar)
        {
            avisoInteragir.SetActive(mostrar);
        }
    }

    public void Iniciar(string nome, string[] falas, Sprite retrato = null)
    {
        if (falas == null || falas.Length == 0) return;

        EmDialogo = true;
        falasAtuais = falas;
        indice = 0;

        textoNome.text = nome;

        // Mostra o retrato só se o NPC tiver um
        if (imagemRetrato != null)
        {
            imagemRetrato.gameObject.SetActive(retrato != null);
            imagemRetrato.sprite = retrato;
        }

        painelDialogo.SetActive(true);
        Congelar(true);
        MostrarFala();
    }

    // Se ainda está digitando, completa a frase na hora. Senão, passa para a próxima fala.
    public void ProximaFala()
    {
        if (digitando)
        {
            StopCoroutine(rotinaDigitacao);
            textoFala.maxVisibleCharacters = 99999;
            digitando = false;
            return;
        }

        indice++;

        if (indice >= falasAtuais.Length)
        {
            Encerrar();
            return;
        }

        MostrarFala();
    }

    private void MostrarFala()
    {
        rotinaDigitacao = StartCoroutine(Digitar(falasAtuais[indice]));
    }

    private IEnumerator Digitar(string frase)
    {
        digitando = true;

        // Põe o texto inteiro, mas mostra uma letra de cada vez (funciona também com tags de formatação)
        textoFala.text = frase;
        textoFala.maxVisibleCharacters = 0;
        textoFala.ForceMeshUpdate();
        int total = textoFala.textInfo.characterCount;

        for (int i = 1; i <= total; i++)
        {
            textoFala.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(segundosPorLetra);
        }

        digitando = false;
    }

    private void Encerrar()
    {
        EmDialogo = false;
        painelDialogo.SetActive(false);
        Congelar(false);
    }

    private void Congelar(bool congelar)
    {
        if (jogador != null)
        {
            if (congelar)
            {
                // Zera o deslize horizontal, senão o tatu continua andando sozinho com o script desligado
                Rigidbody rb = jogador.GetComponent<Rigidbody>();
                if (rb != null) rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            jogador.enabled = !congelar;
        }

        if (pausarTimer && timer != null)
        {
            timer.enabled = !congelar;
        }
    }
}
