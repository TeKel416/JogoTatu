using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class EfeitoRenascimento : MonoBehaviour
{
    [Header("Som")]
    [SerializeField] private string nomeDoSom = "EspiritoDaMata";
    [SerializeField] private AudioClip somReserva;

    [Header("Duracao")]
    [SerializeField] private float duracao = 5f;
    [SerializeField] private float tempoSubida = 1.2f;
    [SerializeField] private float tempoDescida = 1.4f;

    [Header("Cores")]
    [SerializeField] private Color corBrilho = new Color(1f, 0.93f, 0.55f);
    [SerializeField] private Color corLuz = new Color(0.65f, 1f, 0.55f);

    [Header("Intensidade")]
    [SerializeField] private float intensidadeLuz = 5f;
    [SerializeField] private float brilhosPorSegundo = 60f;
    [SerializeField] private bool brilharModelo = true;
    [SerializeField] private float intensidadeEmissao = 2f;
    [SerializeField] private bool surgirComPulinho = true;
    [SerializeField] private Vector3 deslocamento = new Vector3(0f, 0.8f, 0f);

    private PlayerController jogador;
    private Coroutine rotina;
    private GameObject raiz;

    private Texture2D texCirculo;
    private Texture2D texBrilho;
    private Material matBrilho;
    private Material matCirculo;

    private Transform modeloAtual;
    private Vector3 escalaBase;
    private bool modeloAnimando;
    private Transform modeloDosMateriais;
    private List<Material> mats = new List<Material>();
    private List<Color> emissaoOriginal = new List<Color>();
    private List<bool> emissaoLigadaOriginal = new List<bool>();

    private void Awake()
    {
        jogador = GetComponent<PlayerController>();
    }

    private void OnDisable()
    {
        Limpar();
    }

    private void OnDestroy()
    {
        if (texCirculo != null) Destroy(texCirculo);
        if (texBrilho != null) Destroy(texBrilho);
        if (matBrilho != null) Destroy(matBrilho);
        if (matCirculo != null) Destroy(matCirculo);
    }

    public void Tocar()
    {
        if (!gameObject.activeInHierarchy) return;

        Limpar();
        rotina = StartCoroutine(Efeito());
    }

    private void Limpar()
    {
        if (rotina != null)
        {
            StopCoroutine(rotina);
            rotina = null;
        }
        if (raiz != null)
        {
            Destroy(raiz);
            raiz = null;
        }
        RestaurarModelo();
    }

    private IEnumerator Efeito()
    {
        TocarSom();
        PrepararRecursos();

        raiz = new GameObject("EfeitoRenascimento_VFX");
        raiz.transform.position = transform.position + deslocamento;

        ParticleSystem brilhos = CriarBrilhos(raiz.transform);
        CriarOnda(raiz.transform);
        CriarAura(raiz.transform);
        Light luz = CriarLuz(raiz.transform);

        PrepararModelo();

        float t = 0f;
        while (t < duracao)
        {
            if (raiz == null) break;

            t += Time.unscaledDeltaTime;
            float env = Envelope(t);

            raiz.transform.position = transform.position + deslocamento;
            DefinirTaxa(brilhos, brilhosPorSegundo * env);
            luz.intensity = intensidadeLuz * env;
            AtualizarModelo(t, env);

            yield return null;
        }

        RestaurarModelo();

        if (raiz != null)
        {
            DefinirTaxa(brilhos, 0f);
            luz.intensity = 0f;
            Destroy(raiz, 2.5f);
            raiz = null;
        }
        rotina = null;
    }

    private void TocarSom()
    {
        if (SoundManager.Instance != null && !string.IsNullOrEmpty(nomeDoSom))
        {
            SoundManager.Instance.PlaySound2D(nomeDoSom);
        }
        else if (somReserva != null)
        {
            AudioSource.PlayClipAtPoint(somReserva, transform.position);
        }
    }

    private float Envelope(float t)
    {
        float subida = Mathf.Max(tempoSubida, 0.01f);
        float inicioDescida = Mathf.Max(duracao - tempoDescida, subida);

        if (t < subida)
            return Mathf.SmoothStep(0f, 1f, t / subida);
        if (t > inicioDescida)
            return Mathf.SmoothStep(1f, 0f, (t - inicioDescida) / Mathf.Max(duracao - inicioDescida, 0.01f));
        return 1f;
    }

    private void PrepararModelo()
    {
        modeloAtual = null;
        if (jogador != null)
        {
            GameObject go = (jogador.ballModel != null && jogador.ballModel.activeInHierarchy)
                ? jogador.ballModel
                : jogador.normalModel;
            if (go != null) modeloAtual = go.transform;
        }

        if (modeloAtual == null)
        {
            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null) modeloAtual = rend.transform;
        }

        if (modeloAtual == null) return;

        escalaBase = modeloAtual.localScale;
        modeloAnimando = true;

        if (!brilharModelo) return;

        if (modeloDosMateriais != modeloAtual)
        {
            mats.Clear();
            emissaoOriginal.Clear();
            emissaoLigadaOriginal.Clear();

            foreach (Renderer r in modeloAtual.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.materials)
                {
                    mats.Add(m);
                    emissaoOriginal.Add(m.HasProperty("_EmissionColor") ? m.GetColor("_EmissionColor") : Color.black);
                    emissaoLigadaOriginal.Add(m.IsKeywordEnabled("_EMISSION"));
                }
            }
            modeloDosMateriais = modeloAtual;
        }

        foreach (Material m in mats)
        {
            if (m == null || !m.HasProperty("_EmissionColor")) continue;
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }

    private void AtualizarModelo(float t, float env)
    {
        if (!modeloAnimando || modeloAtual == null) return;

        if (surgirComPulinho)
        {
            float p = Mathf.Clamp01(t / 0.6f);
            float f = Mathf.LerpUnclamped(0.55f, 1f, EaseOutBack(p));
            modeloAtual.localScale = escalaBase * f;
        }

        if (brilharModelo)
        {
            Color e = corBrilho * (env * intensidadeEmissao);
            foreach (Material m in mats)
            {
                if (m != null && m.HasProperty("_EmissionColor"))
                    m.SetColor("_EmissionColor", e);
            }
        }
    }

    private void RestaurarModelo()
    {
        if (modeloAnimando && modeloAtual != null)
            modeloAtual.localScale = escalaBase;
        modeloAnimando = false;

        for (int i = 0; i < mats.Count; i++)
        {
            Material m = mats[i];
            if (m == null || !m.HasProperty("_EmissionColor")) continue;
            m.SetColor("_EmissionColor", emissaoOriginal[i]);
            if (emissaoLigadaOriginal[i]) m.EnableKeyword("_EMISSION");
            else m.DisableKeyword("_EMISSION");
        }
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    private ParticleSystem NovoSistema(Transform pai, string nome, Material mat, int ordem, Vector3 posLocal)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.transform.localPosition = posLocal;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.useUnscaledTime = true;

        ParticleSystemRenderer rend = go.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.sharedMaterial = mat;
        rend.sortingOrder = ordem;

        return ps;
    }

    private ParticleSystem CriarBrilhos(Transform pai)
    {
        ParticleSystem ps = NovoSistema(pai, "Brilhos", matBrilho, 30, new Vector3(0f, 0f, -0.8f));

        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startColor = new ParticleSystem.MinMaxGradient(corBrilho, Color.white);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
        main.maxParticles = 300;

        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = 0f;

        ParticleSystem.ShapeModule sh = ps.shape;
        sh.enabled = true;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius = 1.2f;
        sh.radiusThickness = 1f;

        ParticleSystem.VelocityOverLifetimeModule vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.y = new ParticleSystem.MinMaxCurve(1.0f, 2.5f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = Fade(0.15f, 0.6f);

        ParticleSystem.SizeOverLifetimeModule siz = ps.sizeOverLifetime;
        siz.enabled = true;
        siz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ParticleSystem.RotationOverLifetimeModule rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);

        ps.Play();
        return ps;
    }

    private ParticleSystem CriarOnda(Transform pai)
    {
        ParticleSystem ps = NovoSistema(pai, "Onda", matCirculo, 25, new Vector3(0f, 0f, -0.5f));

        ParticleSystem.MainModule main = ps.main;
        main.loop = false;
        main.duration = 2f;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4.0f, 4.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.35f);
        main.startColor = corBrilho;
        main.maxParticles = 150;

        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = 0f;
        em.burstCount = 2;
        em.SetBurst(0, new ParticleSystem.Burst(0f, 45));
        em.SetBurst(1, new ParticleSystem.Burst(1.3f, 35));

        ParticleSystem.ShapeModule sh = ps.shape;
        sh.enabled = true;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = 0.3f;
        sh.radiusThickness = 0f;

        ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = Fade(0.05f, 0.35f);

        ParticleSystem.SizeOverLifetimeModule siz = ps.sizeOverLifetime;
        siz.enabled = true;
        siz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));

        ps.Play();
        return ps;
    }

    private ParticleSystem CriarAura(Transform pai)
    {
        ParticleSystem ps = NovoSistema(pai, "Aura", matCirculo, 5, new Vector3(0f, 0f, 1.0f));

        ParticleSystem.MainModule main = ps.main;
        main.loop = false;
        main.duration = 0.5f;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = duracao * 0.95f;
        main.startSpeed = 0f;
        main.startSize = 5.0f;
        main.startColor = new Color(corLuz.r, corLuz.g, corLuz.b, 0.5f);
        main.maxParticles = 4;

        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = 0f;
        em.burstCount = 1;
        em.SetBurst(0, new ParticleSystem.Burst(0f, 1));

        ParticleSystem.ShapeModule sh = ps.shape;
        sh.enabled = false;

        ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = Fade(0.2f, 0.7f);

        ParticleSystem.SizeOverLifetimeModule siz = ps.sizeOverLifetime;
        siz.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 0.7f);
        curve.AddKey(0.3f, 1.05f);
        curve.AddKey(1f, 1f);
        siz.size = new ParticleSystem.MinMaxCurve(1f, curve);

        ps.Play();
        return ps;
    }

    private Light CriarLuz(Transform pai)
    {
        GameObject go = new GameObject("Luz");
        go.transform.SetParent(pai, false);
        go.transform.localPosition = new Vector3(0f, 0.6f, -2.2f);

        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = corLuz;
        l.range = 10f;
        l.intensity = 0f;
        l.shadows = LightShadows.None;
        return l;
    }

    private static void DefinirTaxa(ParticleSystem ps, float taxa)
    {
        if (ps == null) return;
        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = taxa;
    }

    private static Gradient Fade(float subida, float descida)
    {
        Gradient g = new Gradient();

        List<GradientColorKey> listaCores = new List<GradientColorKey>();
        listaCores.Add(new GradientColorKey(Color.white, 0f));
        listaCores.Add(new GradientColorKey(Color.white, 1f));

        List<GradientAlphaKey> listaAlfas = new List<GradientAlphaKey>();
        listaAlfas.Add(new GradientAlphaKey(0f, 0f));
        listaAlfas.Add(new GradientAlphaKey(1f, subida));
        listaAlfas.Add(new GradientAlphaKey(1f, descida));
        listaAlfas.Add(new GradientAlphaKey(0f, 1f));

        g.SetKeys(listaCores.ToArray(), listaAlfas.ToArray());
        return g;
    }

    private void PrepararRecursos()
    {
        if (texCirculo != null) return;
        texCirculo = CriarTextura(64, false);
        texBrilho = CriarTextura(64, true);
        matCirculo = CriarMaterial(texCirculo);
        matBrilho = CriarMaterial(texBrilho);
    }

    private static Material CriarMaterial(Texture tex)
    {
        Shader sh = Shader.Find("Sprites/Default");
        Material m = new Material(sh);
        m.mainTexture = tex;
        m.renderQueue = 3100;
        return m;
    }

    private static Texture2D CriarTextura(int n, bool estrela)
    {
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f) / n * 2f - 1f;
                float dy = (y + 0.5f) / n * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                float a;
                if (!estrela)
                {
                    a = Mathf.Clamp01(1f - d);
                    a = a * a * (3f - 2f * a);
                }
                else
                {
                    float miolo = Mathf.Exp(-d * d * 22f);
                    float raioH = Mathf.Exp(-dy * dy * 700f) * Mathf.Clamp01(1f - Mathf.Abs(dx));
                    float raioV = Mathf.Exp(-dx * dx * 700f) * Mathf.Clamp01(1f - Mathf.Abs(dy));
                    a = Mathf.Clamp01(miolo + 0.9f * raioH + 0.9f * raioV);
                }

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return tex;
    }
}
