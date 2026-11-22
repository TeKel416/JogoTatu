using System.Collections;
using TMPro;
using UnityEngine;

public class ColetaUI : MonoBehaviour
{
    public static ColetaUI Instancia { get; private set; }

    [Header("Aviso na tela")]
    [SerializeField] private TextMeshProUGUI textoAviso;
    [SerializeField] private CanvasGroup grupoAviso; // no mesmo objeto do texto (ou no pai)
    [SerializeField] private float tempoVisivel = 1.5f;

    private Coroutine rotinaAtual;

    private void Awake()
    {
        Instancia = this;
        grupoAviso.alpha = 0f;
    }

    public void MostrarColeta(string mensagem)
    {
        textoAviso.text = mensagem;

        if (rotinaAtual != null) StopCoroutine(rotinaAtual);
        rotinaAtual = StartCoroutine(AnimarAviso());
    }

    private IEnumerator AnimarAviso()
    {
        Transform t = textoAviso.transform;

        // Entrada: fade in + "pop" de escala
        float duracaoEntrada = 0.25f;
        float tempo = 0f;
        while (tempo < duracaoEntrada)
        {
            tempo += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(tempo / duracaoEntrada);
            grupoAviso.alpha = p;
            t.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.2f, p);
            yield return null;
        }
        t.localScale = Vector3.one;

        // Fica visível
        yield return new WaitForSecondsRealtime(tempoVisivel);

        // Saída: fade out
        float duracaoSaida = 0.5f;
        tempo = 0f;
        while (tempo < duracaoSaida)
        {
            tempo += Time.unscaledDeltaTime;
            grupoAviso.alpha = 1f - Mathf.Clamp01(tempo / duracaoSaida);
            yield return null;
        }
        grupoAviso.alpha = 0f;
    }
}