using System.Collections;
using UnityEngine;

public class Vagalume : MonoBehaviour
{
    [Header("Seguir")]
    public Transform alvo;
    public Vector3 deslocamento = new Vector3(-1f, 2f, -1.5f);
    public float suavidade = 4f;

    [Header("Balanco (voo)")]
    public float amplitude = 0.3f;
    public float velocidadeBalanco = 2f;

    [Header("Luz")]
    public Light luz;
    public GameObject visual;
    public float intensidadeLigada = 3f;
    public float tempoFade = 0.5f;
    public bool ligadoAoIniciar = true;

    private Coroutine rotinaLuz;

    private void Start()
    {
        BuscarAlvo();

        if (luz != null)
        {
            luz.enabled = ligadoAoIniciar;
            luz.intensity = ligadoAoIniciar ? intensidadeLigada : 0f;
        }
        if (visual != null) visual.SetActive(ligadoAoIniciar);

        if (alvo != null) transform.position = DestinoDesejado();
    }

    private void BuscarAlvo()
    {
        if (alvo != null) return;

        if (PlayerController.Instance != null)
        {
            alvo = PlayerController.Instance.transform;
            return;
        }

        GameObject jogador = GameObject.FindGameObjectWithTag("Player");
        if (jogador != null)
        {
            alvo = jogador.transform;
            return;
        }

        GameObject tatu = GameObject.Find("PlayerTatu");
        if (tatu != null) alvo = tatu.transform;
    }

    private void LateUpdate()
    {
        if (alvo == null)
        {
            BuscarAlvo();
            if (alvo == null) return;
        }

        float t = 1f - Mathf.Exp(-suavidade * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, DestinoDesejado(), t);
    }

    private Vector3 DestinoDesejado()
    {
        float lado = alvo.localScale.x < 0f ? -1f : 1f;

        Vector3 destino = alvo.position + new Vector3(deslocamento.x * lado, deslocamento.y, deslocamento.z);
        destino.y += Mathf.Sin(Time.time * velocidadeBalanco) * amplitude;
        return destino;
    }

    // Chame no respawn para o vagalume aparecer imediatamente ao lado do tatu
    public void TeletransportarParaAlvo(Transform novoAlvo = null)
    {
        if (novoAlvo != null) alvo = novoAlvo;
        else BuscarAlvo();

        if (alvo != null)
        {
            transform.position = DestinoDesejado();
        }
    }

    public void Ligar(bool ligado)
    {
        if (rotinaLuz != null) StopCoroutine(rotinaLuz);
        rotinaLuz = StartCoroutine(MudarLuz(ligado));
    }

    private IEnumerator MudarLuz(bool ligado)
    {
        if (luz == null) yield break;

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