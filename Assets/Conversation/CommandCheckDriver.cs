#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
public sealed class CommandCheckDriver:MonoBehaviour
{
    bool passed=true;readonly List<string> lines=new List<string>();
    void Check(bool value,string name){passed&=value;lines.Add((value?"PASS ":"FAIL ")+name);Debug.Log(lines[lines.Count-1]);}
    IEnumerator Start()
    {
        Application.runInBackground=true;yield return new WaitForSeconds(1);
        var d=FindFirstObjectByType<ConversationDirector>();var brain=d.Brain;var body=d.BB8.GetComponent<Rigidbody>();
        d.enabled=false;d.Minion.Controlled=false;d.Minion.enabled=false;d.BB8.AcceptPlayerInput=false;brain.Listening=false;
        d.Minion.transform.position=new Vector3(23,.1f,-6);body.position=new Vector3(23,.58f,-12);body.linearVelocity=Vector3.zero;d.BB8.Head.position=body.position;Physics.SyncTransforms();
        yield return new WaitForSeconds(.3f);
        Check(brain.ExecuteCommand("follow")&&!brain.Busy,"follow permits another voice command");
        yield return new WaitForSeconds(3);
        Check(brain.Following&&Vector3.Distance(body.position,d.Minion.transform.position)<2.6f,"approaches partner and keeps distance");
        d.Minion.transform.position+=Vector3.forward*3;Physics.SyncTransforms();float before=body.position.z;
        yield return new WaitForSeconds(2);
        Check(body.position.z>before+1.5f,"follows moving partner");
        brain.Listening=true;yield return new WaitForSeconds(.5f);before=body.position.z;yield return new WaitForSeconds(.5f);
        Check(Mathf.Abs(body.position.z-before)<.15f,"pauses while listening");brain.Listening=false;
        float distance=Vector3.Distance(body.position,d.Minion.transform.position);
        brain.ExecuteCommand("away");Check(!brain.Following,"away cancels follow");
        yield return new WaitForSeconds(5);
        Check(Vector3.Distance(body.position,d.Minion.transform.position)>distance+4f&&!brain.Autonomous,"retires and stops");
        float affinity=d.Popup.Affinity;d.Popup.ShowCommand("away");Check(d.Popup.Visible&&d.Popup.Feeling=="Entendido"&&d.Popup.Affinity==affinity,"command acknowledgment preserves affinity");
        brain.ExecuteCommand("follow");d.SwitchCharacter();Check(!brain.Autonomous,"manual BB8 control cancels command");
        Directory.CreateDirectory("Logs");File.WriteAllLines("Logs/command-verification.txt",lines);EditorApplication.Exit(passed?0:1);
    }
}
#endif
