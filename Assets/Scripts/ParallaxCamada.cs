using UnityEngine;

// Parallax para o fundo da caverna (jogo 2.5D com câmera em perspectiva).
// Cada camada é um Quad que fica "preso" à câmera, sempre cobrindo a tela inteira,
// e a textura vai deslizando em velocidade diferente conforme a câmera anda.
[DefaultExecutionOrder(1000)]               // roda depois da Cinemachine mexer na câmera (evita tremida)
[RequireComponent(typeof(MeshRenderer))]
public class ParallaxCamada : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Vazio = usa a Main Camera.")]
    [SerializeField] private Camera cam;
    [Tooltip("Opcional: o tatu. Serve para medir a distância do plano do jogo. Vazio = usa 'Distancia Do Jogo'.")]
    [SerializeField] private Transform jogador;
    [SerializeField] private float distanciaDoJogo = 15f;

    [Header("Camada")]
    [Tooltip("Distância da câmera até esta camada. Camadas ao fundo: maior que a distância do jogo (ex: 40 e 27). Camada na frente do jogo: menor (ex: 7).")]
    [SerializeField] private float distancia = 40f;
    [Tooltip("Velocidade em relação ao cenário do jogo. 0 = parada na tela, 1 = igual às plataformas, abaixo de 1 = mais lenta (longe), acima de 1 = mais rápida (perto).")]
    [SerializeField] private float velocidade = 0.2f;
    [Tooltip("1 = imagem inteira na altura da tela. Maior que 1 = imagem mais ampliada.")]
    [SerializeField] private float zoom = 1f;

    private Material mat;
    private float tilingX, tilingY;
    private float ultimoAspecto, ultimoFov;

    private void Start()
    {
        // Procura a câmera: a que você arrastou, a Main Camera (Tag MainCamera) ou qualquer câmera da cena
        if (cam == null) cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();

        if (cam == null)
        {
            Debug.LogError("ParallaxCamada (" + name + "): nenhuma câmera encontrada na cena. O fundo não vai aparecer.", this);
            enabled = false;
            return;
        }

        mat = GetComponent<MeshRenderer>().material;     // cópia só desta camada

        transform.SetParent(cam.transform, false);
        Ajustar();

        Debug.Log("ParallaxCamada (" + name + "): OK. Câmera = '" + cam.name + "', distância = " + distancia +
                  ", escala = " + transform.localScale.x.ToString("F1") + " x " + transform.localScale.y.ToString("F1"), this);
    }

    // Posiciona o Quad na frente da câmera e dimensiona para cobrir a tela
    private void Ajustar()
    {
        ultimoAspecto = cam.aspect;
        ultimoFov = cam.fieldOfView;

        transform.localPosition = new Vector3(0f, 0f, distancia);
        transform.localRotation = Quaternion.identity;

        float h = 2f * distancia * Mathf.Tan(ultimoFov * 0.5f * Mathf.Deg2Rad);
        float w = h * ultimoAspecto;
        transform.localScale = new Vector3(w * 1.02f, h * 1.02f, 1f);

        Texture tex = mat.mainTexture;
        float aspectoTextura = tex != null ? (float)tex.width / tex.height : 2f;

        tilingY = 1f / zoom;
        tilingX = ultimoAspecto / aspectoTextura / zoom;
        mat.mainTextureScale = new Vector2(tilingX, tilingY);
    }

    private void LateUpdate()
    {
        if (!Mathf.Approximately(cam.aspect, ultimoAspecto) ||
            !Mathf.Approximately(cam.fieldOfView, ultimoFov))
        {
            Ajustar();
        }

        float distJogo = jogador != null
            ? Mathf.Abs(jogador.position.z - cam.transform.position.z)
            : distanciaDoJogo;

        // Largura que a tela mostra no plano onde o tatu anda
        float larguraVisivel = 2f * distJogo * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * cam.aspect;

        // Quanto a textura desliza: câmera andou X unidades -> imagem desliza proporcionalmente
        float x = cam.transform.position.x / larguraVisivel * tilingX * velocidade;
        mat.mainTextureOffset = new Vector2(x, (1f - tilingY) * 0.5f);
    }

    private void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }
}
