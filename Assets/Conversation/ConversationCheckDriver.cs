#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
public sealed class ConversationCheckDriver:MonoBehaviour
{
    readonly List<string> lines=new List<string>();
    void Check(bool value,string name){lines.Add((value?"PASS ":"FAIL ")+name);Debug.Log(lines[lines.Count-1]);}
    IEnumerator Start(){
        Application.runInBackground=true;
        yield return new WaitForSeconds(1);
        var d=FindFirstObjectByType<ConversationDirector>();
        Check(d&&d.Minion&&d.BB8&&d.Brain,"scene references");
        Check(d.Minion.LeftArm&&d.Minion.RightArm&&d.Minion.LeftLeg&&d.Minion.RightLeg,"Minion rig references");
        Check(!d.ControlBB8&&d.Minion.Controlled&&!d.BB8.AcceptPlayerInput,"starts controlling Minion");
        d.SwitchCharacter();yield return null;
        Check(d.ControlBB8&&d.View.Target==d.BB8.transform,"Tab switches character and target");
        d.SwitchPOV();yield return null;Check(d.View.FirstPerson,"first person");
        d.SwitchPOV();d.SwitchCharacter();yield return null;Check(!d.View.FirstPerson&&d.View.Target==d.Minion.transform,"restores third person Minion");
        float limit=Time.realtimeSinceStartup+150;while(!d.Voice.Ready&&Time.realtimeSinceStartup<limit)yield return null;
        Check(d.Voice.Ready,"local service connected");
        if(d.Voice.Ready){
            d.Voice.SendText("Estoy muy feliz de verte, BB8");yield return null;
            limit=Time.realtimeSinceStartup+30;while(d.Voice.Busy&&Time.realtimeSinceStartup<limit)yield return null;
            Check(d.Voice.LastResult!=null&&d.Voice.LastResult.reaction=="celebrate"&&d.Brain.Busy,"text to classifier to physical reaction");
            Check(!d.BB8.AcceptPlayerInput,"manual robot input suspended during reaction");
            yield return new WaitForSeconds(4);Check(!d.Brain.Busy,"reaction finishes");
            var before=d.BB8.transform.position;
            d.Brain.React("retreat",2);yield return new WaitForSeconds(3.5f);
            Check(Vector3.Distance(before,d.BB8.transform.position)>.12f,"retreat physically moves robot");
            Check(Vector3.Distance(before,d.BB8.transform.position)<4f,"reaction displacement bounded");
            Check(!float.IsNaN(d.BB8.transform.position.x)&&d.BB8.transform.position.y>-.2f,"robot stays on map");
        }
        Check(d.Brain.Reactions.Length==6&&System.Array.TrueForAll(d.Brain.Reactions,r=>r&&r.Sound),"six configured audio and motion profiles");
        Directory.CreateDirectory("Logs");File.WriteAllLines("Logs/conversation-verification.txt",lines);
        EditorApplication.isPlaying=false;
    }
}
#endif
