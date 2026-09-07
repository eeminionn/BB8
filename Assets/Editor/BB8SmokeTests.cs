using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BB8SmokeTests
{
    static BB8SmokeTests() { EditorApplication.playModeStateChanged += Entered; }
    [MenuItem("BB8/Verify Scrapyard")]
    public static void Run()
    {
        if(EditorApplication.isPlaying) return;
        BB8Setup.OpenDemo();
        SessionState.SetBool("BB8.RunChecks",true);
        EditorApplication.isPlaying=true;
    }
    static void Entered(PlayModeStateChange state)
    {
        if(state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("BB8.RunChecks",false)) return;
        SessionState.SetBool("BB8.RunChecks",false);
        new GameObject("Temporary BB8 checks").AddComponent<BB8SmokeDriver>();
    }
}
