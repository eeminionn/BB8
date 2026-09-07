using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class ExpressionChecks
{
    static ExpressionChecks(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("BB8ExpressionCheck",false)){SessionState.SetBool("BB8ExpressionCheck",false);new GameObject("Expression verification").AddComponent<ExpressionCheckDriver>();}};}
    [MenuItem("BB8/Verify Expressions")]
    public static void Run(){BB8Setup.OpenDemo();SessionState.SetBool("BB8ExpressionCheck",true);EditorApplication.isPlaying=true;}
}
