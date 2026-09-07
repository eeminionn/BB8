using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

public static class BB8Setup
{
    public const string ScenePath = "Assets/scrapyard.unity";

    [MenuItem("BB8/Open Demo")]
    public static void OpenDemo() => EditorSceneManager.OpenScene(System.IO.File.Exists(ScenePath) ? ScenePath : "Assets/demo.unity");

    public static void Prepare()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        foreach (var path in System.IO.Directory.GetFiles("Assets/BB8", "*.blend"))
            if (!AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().Any())
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        OpenDemo();
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = "BB8";
        PlayerSettings.companyName = "eeminionn";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 800;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.runInBackground = false;
        PlayerSettings.SetArchitecture(UnityEditor.Build.NamedBuildTarget.Standalone, 1);
        var controllers = UnityEngine.Object.FindObjectsByType<BbRigidbodyController>();
        if (controllers.Length != 1 || controllers[0].Head == null || controllers[0].Camera == null)
            throw new Exception("Demo controller or references are missing.");
        foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>())
        {
            if (mf.sharedMesh == null) throw new Exception("Missing mesh: " + mf.name);
            Debug.Log("BB8_MESH " + mf.name + " mesh=" + mf.sharedMesh.name);
        }
        foreach (var rb in UnityEngine.Object.FindObjectsByType<Rigidbody>())
            Debug.Log("BB8_BODY " + rb.name + " pos=" + rb.position);
        if (controllers[0].GetComponent<BB8DemoHelp>() == null)
            controllers[0].gameObject.AddComponent<BB8DemoHelp>();
        EditorSceneManager.MarkSceneDirty(controllers[0].gameObject.scene);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("BB8_PREPARE_OK");
    }

    [MenuItem("BB8/Refresh Lighting")]
    public static void RefreshLighting()
    {
        Lightmapping.Clear();
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.4f, 0.44f, 0.5f);
        RenderSettings.ambientEquatorColor = new Color(0.25f, 0.27f, 0.3f);
        RenderSettings.ambientGroundColor = new Color(0.14f, 0.13f, 0.12f);
        RenderSettings.reflectionIntensity = 0f;
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    [MenuItem("BB8/Build Mac App")]
    public static void Build()
    {
        Prepare();
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { ScenePath },
            locationPathName = "../BB8.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        });
        if (result.summary.result != BuildResult.Succeeded)
            throw new Exception("BB8 build failed: " + result.summary.result);
        Debug.Log("BB8_BUILD_OK " + result.summary.totalSize);
    }
}
