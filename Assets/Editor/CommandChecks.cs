using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class CommandChecks
{
    static CommandChecks(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("BB8CommandCheck",false)){SessionState.SetBool("BB8CommandCheck",false);new GameObject("Command verification").AddComponent<CommandCheckDriver>();}};}
    public static void Run(){BB8Setup.OpenDemo();SessionState.SetBool("BB8CommandCheck",true);EditorApplication.isPlaying=true;}
}
