using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TVNoSignalEffect : MonoBehaviour
{
    [Header("Screen Colors")]
    [SerializeField] private Color primaryGreen = new Color(0.05f, 0.85f, 0.15f, 1f);
    [SerializeField] private Color darkStaticGreen = new Color(0.01f, 0.25f, 0.04f, 1f);

    [Header("Static Settings")]
    [SerializeField] private int textureWidth = 128;
    [SerializeField] private int textureHeight = 96;
    [SerializeField] private float updatesPerSecond = 24f;
    [SerializeField] private float scanlineSpeed = 2f;

    private Texture2D staticTexture;
    private Material screenMaterial;
    private Color[] pixels;
    private float timer;

    void Start()
    {
        screenMaterial = GetComponent<Renderer>().material;

        staticTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        staticTexture.filterMode = FilterMode.Point;
        staticTexture.wrapMode = TextureWrapMode.Repeat;

        pixels = new Color[textureWidth * textureHeight];
        GenerateNoiseFrame();

        screenMaterial.SetTexture("_BaseMap", staticTexture);
        screenMaterial.color = Color.white;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float interval = 1f / Mathf.Max(1f, updatesPerSecond);

        if (timer >= interval)
        {
            timer -= interval;
            GenerateNoiseFrame();
        }
    }

    private void GenerateNoiseFrame()
    {
        float scanBar = Mathf.Repeat(Time.time * scanlineSpeed, 1f);

        for (int y = 0; y < textureHeight; y++)
        {
            float normalizedY = (float)y / textureHeight;
            float barDist = Mathf.Abs(normalizedY - scanBar);
            float barBoost = barDist < 0.08f ? 0.35f : 0f;

            for (int x = 0; x < textureWidth; x++)
            {
                float noise = Mathf.Clamp01(Random.value + barBoost);
                pixels[y * textureWidth + x] = Color.Lerp(darkStaticGreen, primaryGreen, noise);
            }
        }

        staticTexture.SetPixels(pixels);
        staticTexture.Apply();
    }
}
