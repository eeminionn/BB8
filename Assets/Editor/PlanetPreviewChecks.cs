using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PlanetPreviewChecks
{
    [MenuItem("BB8/Verify Rotating Planets")]
    public static void Run()
    {
        float previousScale = Time.timeScale;
        var report = new System.Text.StringBuilder();
        try
        {
            Time.timeScale = 0;
            using (var renderer = new MenuPlanetRenderer())
            {
                for (int destination = 0; destination < 2; destination++)
                {
                    var start = Read(renderer.Render(destination, 0));
                    var quarter = Read(renderer.Render(destination, 15));
                    var fullTurn = Read(renderer.Render(destination, 60));
                    double changed = 0, loopError = 0;
                    for (int i = 0; i < start.Length; i++)
                    {
                        changed += Difference(start[i], quarter[i]);
                        loopError += Difference(start[i], fullTurn[i]);
                        if (start[i].a != quarter[i].a)
                            throw new Exception("Planet silhouette changed during axial rotation.");
                    }
                    changed /= start.Length;
                    loopError /= start.Length;
                    if (changed < .005) throw new Exception("Planet surface does not visibly rotate.");
                    if (loopError > .001) throw new Exception("Rotation does not loop seamlessly.");
                    if (start[0].a != 0 || start[256 * 512 + 256].a != 255)
                        throw new Exception("Planet transparency is incorrect.");
                    report.AppendLine($"PASS {(destination == 0 ? "Khepra" : "Earth")}: surface rotation while paused (pixel difference {changed:F4}), fixed silhouette, seamless full turn, transparent corners / opaque centre.");
                }
            }
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/planet-preview-verification.txt", report.ToString());
            Debug.Log("BB8_PLANET_PREVIEW_OK\n" + report);
        }
        finally { Time.timeScale = previousScale; }
    }

    static double Difference(Color32 a, Color32 b) =>
        (Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b)) / (3.0 * 255);

    static Color32[] Read(Texture source)
    {
        var previous = RenderTexture.active;
        var pixels = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true);
        try
        {
            RenderTexture.active = (RenderTexture)source;
            pixels.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            pixels.Apply();
            return pixels.GetPixels32();
        }
        finally { RenderTexture.active = previous; UnityEngine.Object.DestroyImmediate(pixels); }
    }
}
