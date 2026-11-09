using System.Collections.Generic;
using UnityEngine;

// Transforma um bloco liso em algo com cara de terra ou pedra.
// Como funciona: cria um cubo subdividido em muitos pedaços pequenos, arredonda os cantos
// e empurra cada vértice para dentro ou para fora usando ruído (Perlin). Resultado: superfície irregular.
//
// COMO USAR: coloque este script no mesmo objeto que tem MeshFilter + MeshRenderer
// (por exemplo, o Cube do seu bloco). O BoxCollider continua o mesmo, então a física
// (pulo, stomp, quebrar bloco) não muda nada.
//
// Funciona também com blocos esticados (ex: Scale 10 x 0.4 x 1): a malha é gerada já no
// tamanho real do bloco e depois "desesticada", então o relevo e a textura saem uniformes.
// A forma aparece quando você dá Play (a malha é gerada no Awake).
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BlocoRochoso : MonoBehaviour
{
    [Header("Forma")]
    [Tooltip("Tamanho de referência do bloco. 1 = mesmo tamanho do Cube padrão da Unity.")]
    public float tamanho = 1f;
    [Tooltip("Subdivisões por unidade de tamanho. Mais = mais detalhe no relevo, mas mais pesado.")]
    [Range(4, 40)] public int subdivisoes = 16;
    [Tooltip("Máximo de subdivisões em cada direção (protege blocos muito compridos).")]
    [Range(8, 200)] public int limiteSubdivisoes = 80;
    [Tooltip("Arredondamento dos cantos (fração do tamanho). Terra: ~0.05. Pedra: ~0.15.")]
    [Range(0.02f, 0.5f)] public float arredondamento = 0.1f;

    [Header("Relevo (ruído)")]
    [Tooltip("Altura dos calombos e buracos. Terra: ~0.05. Pedra: ~0.12.")]
    [Range(0f, 0.4f)] public float intensidade = 0.1f;
    [Tooltip("Tamanho dos calombos. Valor maior = calombos menores e mais numerosos.")]
    public float escala = 3f;
    [Tooltip("Marcado: cada bloco sai diferente do outro.")]
    public bool sementeAleatoria = true;
    public int semente = 1;

    [Header("Textura")]
    [Tooltip("Quantas vezes a textura se repete por unidade de tamanho.")]
    public float repeticaoUV = 1f;

    [Header("Grama no topo (opcional)")]
    [Tooltip("Marcado: as partes do bloco viradas para cima usam o Material Grama. O resto usa o material normal do Mesh Renderer.")]
    public bool gramaNoTopo = false;
    [Tooltip("Material da grama. O material de terra/pedra continua no Element 0 do Mesh Renderer.")]
    public Material materialGrama;
    [Tooltip("Quão virada para cima uma área precisa estar para ganhar grama. Maior = menos grama.")]
    [Range(0.1f, 0.95f)] public float limiarGrama = 0.6f;
    [Tooltip("Deixa a borda da grama irregular, descendo pelas quinas em alguns pontos. Combine com Arredondamento maior.")]
    [Range(0f, 1f)] public float irregularidadeGrama = 0.5f;

    private Mesh malha;

    private void Awake()
    {
        Gerar();
    }

    private void OnDestroy()
    {
        if (malha != null) Destroy(malha);
    }

    public void Gerar()
    {
        // Escala real do objeto no mundo (blocos esticados)
        Vector3 s3 = transform.lossyScale;
        Vector3 escalaMundo = new Vector3(
            Mathf.Max(Mathf.Abs(s3.x), 0.0001f),
            Mathf.Max(Mathf.Abs(s3.y), 0.0001f),
            Mathf.Max(Mathf.Abs(s3.z), 0.0001f));
        Vector3 invEscala = new Vector3(1f / escalaMundo.x, 1f / escalaMundo.y, 1f / escalaMundo.z);

        // Meio-tamanho do bloco em metros, já com a Scale aplicada
        Vector3 H = escalaMundo * (tamanho * 0.5f);
        float menorMeio = Mathf.Min(H.x, Mathf.Min(H.y, H.z));
        float raio = Mathf.Clamp(arredondamento * tamanho, 0.005f, menorMeio * 0.95f);
        Vector3 limite = new Vector3(H.x - raio, H.y - raio, H.z - raio);

        // Quantidade de subdivisões em cada eixo, proporcional ao comprimento real
        int[] cnt = new int[3];
        for (int k = 0; k < 3; k++)
        {
            cnt[k] = Mathf.Clamp(Mathf.CeilToInt(subdivisoes * escalaMundo[k]), 4, limiteSubdivisoes);
        }

        // Deslocamento aleatório do ruído (faz cada bloco ter um relevo diferente)
        int s = sementeAleatoria ? Random.Range(0, 100000) : semente;
        System.Random rng = new System.Random(s);
        Vector3 off = new Vector3((float)rng.NextDouble() * 100f,
                                  (float)rng.NextDouble() * 100f,
                                  (float)rng.NextDouble() * 100f) + Vector3.one * 500f;

        Vector3[] faces = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        for (int f = 0; f < 6; f++)
        {
            Vector3 nF = faces[f];

            // Eixos desta face: u e w são as duas direções ao longo da face
            int eixoU = Mathf.Abs(nF.y) > 0.5f ? 0 : 1;
            int eixoN = Mathf.Abs(nF.x) > 0.5f ? 0 : (Mathf.Abs(nF.y) > 0.5f ? 1 : 2);
            int eixoW = 3 - eixoN - eixoU;

            Vector3 u = eixoU == 0 ? Vector3.right : Vector3.up;
            Vector3 w = Vector3.Cross(-nF, u);

            int nu = cnt[eixoU];
            int nw = cnt[eixoW];
            float compU = 2f * H[eixoU];
            float compW = 2f * H[eixoW];

            int baseIdx = verts.Count;

            for (int i = 0; i <= nu; i++)
            {
                for (int j = 0; j <= nw; j++)
                {
                    float a = (float)i / nu * 2f - 1f;
                    float b = (float)j / nw * 2f - 1f;

                    // Ponto na superfície da caixa, já em metros
                    Vector3 pu = nF + a * u + b * w;
                    Vector3 P = Vector3.Scale(pu, H);

                    // Cantos arredondados
                    Vector3 inner = new Vector3(
                        Mathf.Clamp(P.x, -limite.x, limite.x),
                        Mathf.Clamp(P.y, -limite.y, limite.y),
                        Mathf.Clamp(P.z, -limite.z, limite.z));
                    Vector3 d = P - inner;
                    Vector3 nrm = nF;
                    if (d.sqrMagnitude > 0.0000001f)
                    {
                        nrm = d.normalized;
                        P = inner + nrm * raio;
                    }

                    // Relevo: empurra o vértice para fora ou para dentro
                    Vector3 q = (P * (2f / tamanho)) * escala + off;
                    float r = 0.65f * Perlin3D(q) + 0.35f * Perlin3D(q * 2.3f);
                    P += nrm * ((r - 0.5f) * 3f * intensidade * tamanho * 0.5f);

                    // "Desestica": o Transform vai esticar de volta, e o resultado fica uniforme
                    verts.Add(Vector3.Scale(P, invEscala));

                    // UV proporcional ao tamanho real (a textura não estica em blocos compridos)
                    uvs.Add(new Vector2((a + 1f) * 0.5f * compU, (b + 1f) * 0.5f * compW) / tamanho * repeticaoUV);
                }
            }

            for (int i = 0; i < nu; i++)
            {
                for (int j = 0; j < nw; j++)
                {
                    int A = baseIdx + i * (nw + 1) + j;
                    int B = A + (nw + 1);
                    int C = A + 1;
                    int D = B + 1;

                    tris.Add(A); tris.Add(C); tris.Add(B);
                    tris.Add(B); tris.Add(C); tris.Add(D);
                }
            }
        }

        malha = new Mesh();
        malha.name = "BlocoRochoso";
        malha.indexFormat = verts.Count > 65000
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;
        malha.SetVertices(verts);
        malha.SetUVs(0, uvs);
        malha.SetTriangles(tris, 0);

        malha.RecalculateNormals();
        SuavizarNormais(malha);
        malha.RecalculateTangents();

        // Grama: separa os triângulos virados para cima num segundo material
        if (gramaNoTopo)
        {
            SepararGrama(malha, tris, escalaMundo, off);
            ConfigurarMateriais();
        }

        malha.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = malha;
    }

    // Divide a malha em dois submeshes: 0 = terra/pedra, 1 = grama (parte virada para cima)
    private void SepararGrama(Mesh mesh, List<int> todos, Vector3 escalaMundo, Vector3 off)
    {
        Vector3[] vs = mesh.vertices;
        Vector3[] ns = mesh.normals;
        List<int> terra = new List<int>();
        List<int> grama = new List<int>();

        for (int t = 0; t < todos.Count; t += 3)
        {
            int i0 = todos[t];
            int i1 = todos[t + 1];
            int i2 = todos[t + 2];

            float ny = (ns[i0].y + ns[i1].y + ns[i2].y) / 3f;

            // Ruído no centro do triângulo, só para a borda da grama não ficar reta
            Vector3 centro = (vs[i0] + vs[i1] + vs[i2]) / 3f;
            float ruido = Perlin3D(Vector3.Scale(centro, escalaMundo) * 6f + off) - 0.5f;

            bool temGrama = ny + ruido * irregularidadeGrama * 2f > limiarGrama;

            List<int> destino = temGrama ? grama : terra;
            destino.Add(i0);
            destino.Add(i1);
            destino.Add(i2);
        }

        mesh.subMeshCount = 2;
        mesh.SetTriangles(terra, 0);
        mesh.SetTriangles(grama, 1);
    }

    // Garante que o Mesh Renderer tenha 2 materiais: o normal e o da grama
    private void ConfigurarMateriais()
    {
        if (materialGrama == null)
        {
            Debug.LogWarning("BlocoRochoso: 'Grama No Topo' está marcado, mas o campo 'Material Grama' está vazio.", this);
        }

        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material[] mats = mr.sharedMaterials;

        if (mats.Length < 2)
        {
            System.Array.Resize(ref mats, 2);
            mats[1] = materialGrama != null ? materialGrama : mats[0];
        }
        else if (materialGrama != null)
        {
            mats[1] = materialGrama;
        }

        mr.sharedMaterials = mats;
    }

    // Vértices na mesma posição (nas emendas entre faces) ganham a mesma normal,
    // para não aparecer uma linha de iluminação quebrada nas arestas.
    private void SuavizarNormais(Mesh mesh)
    {
        Vector3[] vs = mesh.vertices;
        Vector3[] ns = mesh.normals;
        Dictionary<Vector3Int, Vector3> soma = new Dictionary<Vector3Int, Vector3>();

        for (int i = 0; i < vs.Length; i++)
        {
            Vector3Int k = Vector3Int.RoundToInt(vs[i] * 1000f);
            soma.TryGetValue(k, out Vector3 atual);
            soma[k] = atual + ns[i];
        }

        for (int i = 0; i < vs.Length; i++)
        {
            Vector3Int k = Vector3Int.RoundToInt(vs[i] * 1000f);
            ns[i] = soma[k].normalized;
        }

        mesh.normals = ns;
    }

    // Ruído 3D simples feito com três ruídos 2D do Unity
    private static float Perlin3D(Vector3 q)
    {
        float xy = Mathf.PerlinNoise(q.x, q.y);
        float yz = Mathf.PerlinNoise(q.y, q.z);
        float zx = Mathf.PerlinNoise(q.z, q.x);
        return (xy + yz + zx) / 3f;
    }
}
