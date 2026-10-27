using System.Collections;
using UnityEngine;

// Anexe este script no objeto PAI "Vagalume" (solto na raiz da Hierarchy, fora do PlayerTatu).
// A luz e o visual ficam como FILHOS dele. Assim o script nunca é desligado junto com a luz.
public class Vagalume : MonoBehaviour
{
    [Header("Seguir")]
    [Tooltip("O que o vagalume segue. Se ficar vazio, procura sozinho o objeto com a tag Player")]
    public Transform alvo;
    [Tooltip("Posição em relação ao tatu. X é espelhado quando o tatu vira, para o vagalume ficar sempre atrás")]
    public Vector3 deslocamento = new Vector3(-1f, 2f, -1.5f);
    [Tooltip("Quanto maior, mais rápido ele alcança o tatu. Menor = mais atrasado e solto")]
    public float suavidade = 4f;

    [Header("Balanço (voo)")]
    public float amplitude = 0.3f;
    public float velocidadeBalanco = 2f;

    [Header("Luz")]
    [Tooltip("A Point Light filha deste objeto")]
    public Light luz;
    [Tooltip("Parte visível do vagalume (ex: a esfera brilhante). Opcional")]
    public GameObject visual;
    public float intensidadeLigada = 3f;
    public float tempoFade = 0.5f;
    [Tooltip("Desmarque nas áreas claras, como o lobby")]
    public bool ligadoAoIniciar = true;

    private Coroutine rotinaLuz;

    private void Start()
    {
        if (alvo == null)
        {
            GameObject jogador = GameObject.FindGameObjectWithTag("Player");
            if (jogador != null) alvo = jogador.transform;
        }

        // Começa já no estado certo, sem fade
        luz.enabled = ligadoAoIniciar;
        luz.intensity = ligadoAoIniciar ? intensidadeLigada : 0f;
        if (visual != null) visual.SetActive(ligadoAoIniciar);

        if (alvo != null) transform.position = DestinoDesejado();
    }

    private void LateUpdate()
    {
        if (alvo == null) return;

        // Suavização que não depende do FPS
        float t = 1f - Mathf.Exp(-suavidade * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, DestinoDesejado(), t);
    }

    private Vector3 DestinoDesejado()
    {
        // O PlayerController vira o tatu mudando o sinal do localScale.x
        float lado = alvo.localScale.x < 0f ? -1f : 1f;

        Vector3 destino = alvo.position + new Vector3(deslocamento.x * lado, deslocamento.y, deslocamento.z);
        destino.y += Mathf.Sin(Time.time * velocidadeBalanco) * amplitude;
        return destino;
    }

    // Liga ou desliga a luz com um fade suave. Pode ser chamado por outros scripts
    // (ex: uma zona clara no meio da caverna).
    public void Ligar(bool ligado)
    {
        if (rotinaLuz != null) StopCoroutine(rotinaLuz);
        rotinaLuz = StartCoroutine(MudarLuz(ligado));
    }

    private IEnumerator MudarLuz(bool ligado)
    {
        float inicio = luz.intensity;
        float fim = ligado ? intensidadeLigada : 0f;

        if (ligado)
        {
            luz.enabled = true;
            if (visual != null) visual.SetActive(true);
        }

        float tempo = 0f;
        while (tempo < tempoFade)
        {
            tempo += Time.deltaTime;
            luz.intensity = Mathf.Lerp(inicio, fim, tempo / tempoFade);
            yield return null;
        }

        luz.intensity = fim;

        if (!ligado)
        {
            luz.enabled = false;
            if (visual != null) visual.SetActive(false);
        }
    }
}
