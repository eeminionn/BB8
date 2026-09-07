using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class ConversationChecks
{
    static ConversationChecks(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("BB8ConversationCheck",false)){SessionState.SetBool("BB8ConversationCheck",false);new GameObject("Conversation verification").AddComponent<ConversationCheckDriver>();}};}
    [MenuItem("BB8/Verify Conversation")]
    public static void Run(){SessionState.SetBool("BB8ConversationCheck",true);EditorApplication.isPlaying=true;}
}
