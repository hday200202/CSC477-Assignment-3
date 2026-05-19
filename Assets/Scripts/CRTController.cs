using UnityEngine;
using UnityEngine.UI;


[DisallowMultipleComponent]
public class CRTController : MonoBehaviour
{
    [Header("Scanlines")]
    [Range(0f, 1f)]    public float scanlineStrength    = 0.35f;
    [Range(50f, 800f)] public float scanlineCount       = 300f;

    [Header("Sweep Band")]
    [Range(0f, 4f)]    public float scrollSpeed         = 0.6f;
    [Range(0f, 3f)]    public float scrollBrightness    = 1.2f;
    [Range(5f, 80f)]   public float scrollBandSharpness = 25f;

    [Header("Vignette")]
    [Range(0f, 3f)]    public float vignetteStrength    = 1.8f;

    [Header("Distortion")]
    [Range(0f, 0.15f)] public float distortStrength     = 0.018f;

    [Header("Glitch")]
    [Range(0f, 1f)]    public float glitchStrength      = 0.25f;

    [Header("Phosphor")]
    public Color        phosphorColor    = new Color(0.2f, 1f, 0.3f, 1f);
    [Range(0f, 2f)]    public float bloomStrength       = 0.5f;
    [Range(0.002f, 0.03f)] public float bloomSpread    = 0.008f;

    [Header("Effects")]
    [Range(0f, 0.5f)]  public float noiseStrength       = 0.03f;
    [Range(0f, 0.2f)]  public float flickerStrength     = 0.025f;

    // Cached shader property IDs (avoid string lookups every frame)
    static readonly int ID_ScanlineStrength    = Shader.PropertyToID("_ScanlineStrength");
    static readonly int ID_ScanlineCount       = Shader.PropertyToID("_ScanlineCount");
    static readonly int ID_ScrollSpeed         = Shader.PropertyToID("_ScrollSpeed");
    static readonly int ID_ScrollBrightness    = Shader.PropertyToID("_ScrollBrightness");
    static readonly int ID_ScrollBandSharpness = Shader.PropertyToID("_ScrollBandSharpness");
    static readonly int ID_DistortStrength     = Shader.PropertyToID("_DistortStrength");
    static readonly int ID_GlitchStrength      = Shader.PropertyToID("_GlitchStrength");
    static readonly int ID_PhosphorColor       = Shader.PropertyToID("_PhosphorColor");
    static readonly int ID_BloomStrength       = Shader.PropertyToID("_BloomStrength");
    static readonly int ID_BloomSpread         = Shader.PropertyToID("_BloomSpread");
    static readonly int ID_VignetteStrength    = Shader.PropertyToID("_VignetteStrength");
    static readonly int ID_NoiseStrength       = Shader.PropertyToID("_NoiseStrength");
    static readonly int ID_FlickerStrength     = Shader.PropertyToID("_FlickerStrength");

    private Material instanceMaterial;

    void Awake()
    {
        var img    = GetComponent<Image>();
        var rawImg = GetComponent<RawImage>();

        Material src = img    != null ? img.material    :
                       rawImg != null ? rawImg.material : null;

        if (src == null) { enabled = false; return; }

        instanceMaterial = new Material(src);

        if (img    != null) img.material    = instanceMaterial;
        if (rawImg != null) rawImg.material = instanceMaterial;
    }

    void Update()
    {
        if (instanceMaterial == null) return;

        instanceMaterial.SetFloat(ID_ScanlineStrength,    scanlineStrength);
        instanceMaterial.SetFloat(ID_ScanlineCount,       scanlineCount);
        instanceMaterial.SetFloat(ID_ScrollSpeed,         scrollSpeed);
        instanceMaterial.SetFloat(ID_ScrollBrightness,    scrollBrightness);
        instanceMaterial.SetFloat(ID_ScrollBandSharpness, scrollBandSharpness);
        instanceMaterial.SetFloat(ID_DistortStrength,     distortStrength);
        instanceMaterial.SetFloat(ID_GlitchStrength,      glitchStrength);
        instanceMaterial.SetColor(ID_PhosphorColor,        phosphorColor);
        instanceMaterial.SetFloat(ID_BloomStrength,        bloomStrength);
        instanceMaterial.SetFloat(ID_BloomSpread,          bloomSpread);
        instanceMaterial.SetFloat(ID_VignetteStrength,    vignetteStrength);
        instanceMaterial.SetFloat(ID_NoiseStrength,       noiseStrength);
        instanceMaterial.SetFloat(ID_FlickerStrength,     flickerStrength);
    }

    void OnDestroy()
    {
        if (instanceMaterial != null)
            Destroy(instanceMaterial);
    }
}
