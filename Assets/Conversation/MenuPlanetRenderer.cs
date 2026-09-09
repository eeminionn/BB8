using UnityEngine;

// A small GPU preview, independent of the paused simulation and its cameras.
public sealed class MenuPlanetRenderer : System.IDisposable
{
    public const float DegreesPerSecond = 6f;
    readonly Material material;
    readonly RenderTexture preview;

    public MenuPlanetRenderer()
    {
        var shader = Resources.Load<Shader>("MenuPlanet");
        if (!shader || !shader.isSupported)
            throw new System.InvalidOperationException("Menu planet shader is unavailable.");
        material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        material.SetTexture("_Land", Resources.Load<Texture2D>("EarthLand"));
        preview = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        {
            name = "Rotating menu planet",
            hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        preview.Create();
    }

    public Texture Render(int destination, double unscaledSeconds)
    {
        float degrees = (float)(unscaledSeconds * DegreesPerSecond % 360.0);
        material.SetFloat("_Rotation", (degrees + 10f) * Mathf.Deg2Rad);
        material.SetFloat("_Earth", destination == 1 ? 1f : 0f);
        var previous = RenderTexture.active;
        try { Graphics.Blit(Texture2D.whiteTexture, preview, material); }
        finally { RenderTexture.active = previous; }
        return preview;
    }

    public void Dispose()
    {
        preview.Release();
        if (Application.isPlaying) { Object.Destroy(preview); Object.Destroy(material); }
        else { Object.DestroyImmediate(preview); Object.DestroyImmediate(material); }
    }
}
