#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
public sealed class ExpressionCheckDriver:MonoBehaviour
{
    readonly List<string> lines=new List<string>();
    ConversationDirector d;
    Rigidbody body;
    bool passed=true;
    void Check(bool value,string name){passed&=value;lines.Add((value?"PASS ":"FAIL ")+name);Debug.Log(lines.Last());}
    void Warp(){body.position=new Vector3(23,.58f,-12);body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;d.BB8.Head.position=body.position;d.BB8.Head.linearVelocity=Vector3.zero;d.BB8.Head.angularVelocity=Vector3.zero;Physics.SyncTransforms();}
    IEnumerator Start()
    {
        Application.runInBackground=true;
        yield return new WaitForSeconds(1);
        d=FindFirstObjectByType<ConversationDirector>();body=d.BB8.GetComponent<Rigidbody>();
        d.enabled=false;d.Minion.Controlled=false;d.Minion.enabled=false;d.Minion.transform.position=new Vector3(23,.1f,-6);
        d.Brain.Listening=false;d.BB8.AcceptPlayerInput=false;
        Check(d.Brain.Reactions.All(r=>r.IntensitySounds.Length==3&&r.IntensitySounds.All(c=>c)),"18 clips assigned");
        Check(d.Brain.Reactions.SelectMany(r=>r.IntensitySounds).Distinct().Count()==18,"18 distinct recordings");
        var distances=new float[3];var poses=new float[3];
        for(int level=1;level<=3;level++){
            Warp();yield return new WaitForSeconds(.35f);var start=body.position;
            d.Brain.React("retreat",level,"hostile");
            while(d.Brain.Busy){
                distances[level-1]=Mathf.Max(distances[level-1],Vector3.Distance(start,body.position));
                var pose=d.BB8.GetComponent<BB8Personality>().SocialHeadPose;
                poses[level-1]=Mathf.Max(poses[level-1],new Vector2(pose.x,pose.y).magnitude);
                yield return null;
            }
        }
        Check(distances[0]>.4f&&distances[1]>distances[0]*1.8f&&distances[2]>distances[1]*1.5f,"distinct physical distances: "+string.Join(", ",distances.Select(v=>v.ToString("0.00"))));
        Check(poses[2]>poses[0]*2.4f,"head intensity grows visibly: "+poses[0].ToString("0.0")+" to "+poses[2].ToString("0.0"));
        Check(d.Popup.Visible&&d.Popup.Feeling=="Aterrado"&&d.Popup.Affinity<.2f,"popup shows robot feeling and accumulated rejection");
        float rejected=d.Popup.Affinity;
        Warp();yield return new WaitForSeconds(.35f);d.Brain.React("celebrate",3,"friendly");
        Check(d.Popup.Affinity>rejected,"positive statement moves preference toward green");
        Check(d.Popup.Feeling=="Eufórico","maximum joy uses a distinct word");
        float lateral=0;var danceStart=body.position;
        while(d.Brain.Busy){lateral=Mathf.Max(lateral,Mathf.Abs(body.position.x-danceStart.x));yield return null;}
        Check(lateral>1f,"celebration follows a wide lateral path");
        yield return new WaitForSeconds(3.2f);Check(!d.Popup.Visible,"popup disappears three seconds after gesture");
        Warp();yield return new WaitForSeconds(.35f);
        var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.transform.position=new Vector3(23,1,-14);obstacle.transform.localScale=new Vector3(5,2,1);Physics.SyncTransforms();
        d.Brain.React("retreat",3,"hostile");while(d.Brain.Busy)yield return null;
        Check(body.position.z>-13.5f,"retreat stops in front of obstacle");Destroy(obstacle);
        Check(!float.IsNaN(body.position.x)&&body.position.y>0,"physics remains stable");
        d.View.Target=d.BB8.transform;d.View.AutoFollow=true;
        float startAngle=d.View.transform.eulerAngles.y;
        for(float t=0;t<2;t+=Time.deltaTime){d.BB8.Drive(new Vector2(.5f,0),false,false,Time.deltaTime);yield return null;}
        var viewForward=Vector3.ProjectOnPlane(d.View.transform.forward,Vector3.up).normalized;
        Check(Vector3.Dot(viewForward,d.BB8.FacingDirection)>.9f,"camera follows the steering heading");
        Check(Mathf.Abs(Mathf.DeltaAngle(startAngle,d.View.transform.eulerAngles.y))>40,"camera turns without mouse input");
        Directory.CreateDirectory("Logs");File.WriteAllLines("Logs/expression-verification.txt",lines);
        if(Application.isBatchMode)EditorApplication.Exit(passed?0:1);else EditorApplication.isPlaying=false;
    }
}
#endif
