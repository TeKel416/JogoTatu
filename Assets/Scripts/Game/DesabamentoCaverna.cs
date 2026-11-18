using UnityEngine;

// Efeito de "caverna desabando" quando o timer está acabando:
// a tela treme (cada vez mais forte) e caem poeira e pedrinhas do teto.
// Tudo é criado por código: basta colocar este script num objeto vazio da cena.
// Lê o tempo do CountdownTimer. Se o timer for desligado (Alt+T) ou pausado, o efeito para.
[DefaultExecutionOrder(2000)]    // roda depois da Cinemachine e do parallax
public class DesabamentoCaverna : MonoBehaviour
{
    [Header("Quando começa")]
    [Tooltip("O efeito começa quando faltarem estes segundos e vai aumentando até o tempo acabar.")]
    [SerializeField] private float segundosParaComecar = 30f;

    [Header("Tremor da tela")]
    [Tooltip("Tamanho do tremor no máximo (em unidades do mundo).")]
    [SerializeField] private float tremorMaximo = 0.25f;
    [SerializeField] private float frequencia = 18f;
    [Tooltip("Inclinação máxima da câmera, em graus.")]
    [SerializeField] private float inclinacaoMaxima = 0.8f;
    [Tooltip("Sacudidas fortes de vez em quando, junto com uma chuva de pedras.")]
    [SerializeField] private bool sacudidas = true;

    [Header("Partículas")]
    [SerializeField] private float poeiraPorSegundo = 40f;
    [SerializeField] private float pedrasPorSegundo = 10f;
    [SerializeField] private Color corPoeira = new Color(0.72f, 0.58f, 0.42f, 0.55f);
    [SerializeField] private Color corPedras = new Color(0.32f, 0.22f, 0.15f, 1f);

    private Camera cam;
    private CountdownTimer timer;
    private float proximaBusca;
    private ParticleSystem poeiraFrente, poeiraFundo, pedras;
    private Material materialParticulas;
    private float intensidade;     // 0 a 1
    private float pulso;           // sacudida forte, decai sozinha
    private float proximoPulso;

    private void Start()
    {
        cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();

        if (cam == null)
        {
            Debug.LogError("DesabamentoCaverna: nenhuma câmera encontrada na cena.", this);
            enabled = false;
            return;
        }

        materialParticulas = new Material(Shader.Find("Sprites/Default"));
        Texture2D circulo = CriarTexturaCirculo(64);

        poeiraFrente = CriarParticulas("Poeira_Frente", 10f, circulo, corPoeira,
            new Vector2(0.6f, 1.8f), new Vector2(3f, 5f), new Vector2(1f, 2.5f), 0.12f, 20, false);
        poeiraFundo = CriarParticulas("Poeira_Fundo", 22f, circulo, new Color(corPoeira.r, corPoeira.g, corPoeira.b, corPoeira.a * 0.8f),
            new Vector2(1.2f, 3.2f), new Vector2(3.5f, 6f), new Vector2(1f, 2.5f), 0.12f, 5, false);
        pedras = CriarParticulas("Pedras", 10f, Texture2D.whiteTexture, corPedras,
            new Vector2(0.10f, 0.28f), new Vector2(1.6f, 2.6f), new Vector2(2f, 5f), 0.9f, 21, true);

        proximoPulso = 2f;
    }

    private void LateUpdate()
    {
        // Acha o timer da cena (e procura de novo de vez em quando, caso ele ainda não exista)
        if (timer == null && Time.unscaledTime >= proximaBusca)
        {
            timer = FindFirstObjectByType<CountdownTimer>(FindObjectsInactive.Include);
            proximaBusca = Time.unscaledTime + 1f;
        }

        // O efeito só existe com o timer ligado e rodando (não no Lobby, não desligado, não em diálogo)
        // e perto do fim do tempo.
        float restante = CountdownTimer.TempoRestante;
        bool timerRodando = timer != null && timer.isActiveAndEnabled;
        bool ativo = timerRodando && CountdownTimer.Iniciado && !CountdownTimer.Desativado &&
                     restante > 0f && restante <= segundosParaComecar && Time.timeScale > 0f;

        float alvo = ativo ? Mathf.Clamp01(1f - restante / segundosParaComecar) : 0f;
        intensidade = Mathf.MoveTowards(intensidade, alvo, Time.unscaledDeltaTime * 0.8f);

        AtualizarSacudidas();
        AtualizarEmissao();
        AplicarTremor();
    }

    private void AtualizarSacudidas()
    {
        pulso = Mathf.MoveTowards(pulso, 0f, Time.unscaledDeltaTime * 2.2f);

        if (!sacudidas || intensidade < 0.2f || Time.timeScale <= 0f) return;

        proximoPulso -= Time.deltaTime;
        if (proximoPulso <= 0f)
        {
            pulso = 1f;
            proximoPulso = Mathf.Lerp(4.5f, 1.3f, intensidade) + Random.Range(0f, 1.5f);

            if (pedras != null) pedras.Emit(Mathf.RoundToInt(10f + 25f * intensidade));
            if (poeiraFrente != null) poeiraFrente.Emit(Mathf.RoundToInt(6f + 10f * intensidade));
        }
    }

    private void AtualizarEmissao()
    {
        DefinirTaxa(poeiraFrente, poeiraPorSegundo * intensidade);
        DefinirTaxa(poeiraFundo, poeiraPorSegundo * 0.7f * intensidade);
        DefinirTaxa(pedras, pedrasPorSegundo * intensidade);
    }

    private static void DefinirTaxa(ParticleSystem ps, float taxa)
    {
        if (ps == null) return;
        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = taxa;
    }

    // O tremor é somado à posição da câmera DEPOIS da Cinemachine, a cada quadro
    // (a Cinemachine recalcula a posição do zero todo quadro, então o tremor não acumula).
    private void AplicarTremor()
    {
        if (intensidade <= 0.001f) return;

        float t = Time.unscaledTime * frequencia;
        float ox = (Mathf.PerlinNoise(t, 0.3f) - 0.5f) * 2f;
        float oy = (Mathf.PerlinNoise(0.7f, t) - 0.5f) * 2f;
        float rz = (Mathf.PerlinNoise(t, 5.1f) - 0.5f) * 2f;

        float forca = intensidade * (1f + pulso * 1.8f);
        float amp = tremorMaximo * forca;

        Transform c = cam.transform;
        c.position += c.right * (ox * amp) + c.up * (oy * amp);
        c.Rotate(0f, 0f, rz * inclinacaoMaxima * forca, Space.Self);
    }

    // Cria um sistema de partículas preso à câmera, que sorteia pontos no topo da tela e deixa cair.
    private ParticleSystem CriarParticulas(string nome, float distancia, Texture tex, Color cor,
        Vector2 tamanho, Vector2 vida, Vector2 velocidade, float gravidade, int ordem, bool girar)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(cam.transform, false);

        float h = 2f * distancia * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float w = h * cam.aspect;
        go.transform.localPosition = new Vector3(0f, h * 0.55f, distancia);
        go.transform.localRotation = Quaternion.identity;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startLifetime = new ParticleSystem.MinMaxCurve(vida.x, vida.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(velocidade.x, velocidade.y);
        main.startSize = new ParticleSystem.MinMaxCurve(tamanho.x, tamanho.y);
        main.startColor = cor;
        main.gravityModifier = gravidade;
        main.maxParticles = 600;
        if (girar) main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);

        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = 0f;

        ParticleSystem.ShapeModule sh = ps.shape;
        sh.enabled = true;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale = new Vector3(w * 1.1f, 0.1f, 0.1f);
        sh.rotation = new Vector3(90f, 0f, 0f);        // emite para baixo

        ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        if (girar)
        {
            ParticleSystem.RotationOverLifetimeModule rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
        }

        ParticleSystemRenderer rend = go.GetComponent<ParticleSystemRenderer>();
        Material m = new Material(materialParticulas);
        m.mainTexture = tex;
        rend.material = m;
        rend.sortingOrder = ordem;

        ps.Play();
        return ps;
    }

    private static Texture2D CriarTexturaCirculo(int n)
    {
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f) / n * 2f - 1f;
                float dy = (y + 0.5f) / n * 2f - 1f;
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                a = a * a * (3f - 2f * a);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply();
        return tex;
    }
}
