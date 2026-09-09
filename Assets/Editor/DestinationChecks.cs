using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class DestinationChecks
{
    static DestinationChecks(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("DestinationCheck",false)){SessionState.SetBool("DestinationCheck",false);new GameObject("Destination checks").AddComponent<DestinationCheckDriver>();}};}
    public static void Run(){BB8Setup.OpenDemo();SessionState.SetBool("DestinationCheck",true);EditorApplication.isPlaying=true;}
}
