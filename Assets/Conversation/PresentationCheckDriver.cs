#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
public sealed class PresentationCheckDriver:MonoBehaviour
{
    bool passed=true;readonly List<string> lines=new List<string>();
    void Check(bool value,string name){passed&=value;lines.Add((value?"PASS ":"FAIL ")+name);Debug.Log(lines[lines.Count-1]);}
    float Feet(MinionController minion){
        float bottom=float.PositiveInfinity;
        foreach(var renderer in minion.Visual.GetComponentsInChildren<SkinnedMeshRenderer>()){
            var mesh=renderer.sharedMesh;var vertices=mesh.vertices;var bind=mesh.bindposes;var weights=mesh.boneWeights;var matrices=new Matrix4x4[renderer.bones.Length];
            for(int n=0;n<matrices.Length;n++)matrices[n]=renderer.bones[n].localToWorldMatrix*bind[n];
            for(int n=0;n<vertices.Length;n++){var w=weights[n];var v=vertices[n];
                float y=matrices[w.boneIndex0].MultiplyPoint3x4(v).y*w.weight0+matrices[w.boneIndex1].MultiplyPoint3x4(v).y*w.weight1+matrices[w.boneIndex2].MultiplyPoint3x4(v).y*w.weight2+matrices[w.boneIndex3].MultiplyPoint3x4(v).y*w.weight3;
                bottom=Mathf.Min(bottom,y);
            }
        }
        return bottom;
    }
    IEnumerator Start(){
        Application.runInBackground=true;yield return new WaitForSeconds(1);
        var d=FindFirstObjectByType<ConversationDirector>();var m=d.Minion;var cc=m.GetComponent<CharacterController>();var shell=ExperienceShell.Instance;
        Check(shell&&shell.Muted&&AudioListener.pause&&AudioListener.volume==0,"silent startup");
        Check(m.Grounded,"stable ground contact");
        float feet=Feet(m);Check(feet>-.035f&&feet<.045f,"actual visible feet meet ground: "+feet.ToString("0.000"));
        var start=m.transform.position;shell.SetMenu(true);yield return new WaitForSecondsRealtime(.2f);
        Check(Vector3.Distance(start,m.transform.position)<.001f&&Time.timeScale==0,"menu pauses movement");shell.Enter();
        d.enabled=false;m.enabled=false;d.BB8.AcceptPlayerInput=false;d.View.SuspendInput=false;cc.enabled=false;m.transform.position=new Vector3(23,.08f,-12);m.transform.rotation=Quaternion.identity;cc.enabled=true;
        start=m.transform.position;var leg=m.LeftLeg.localRotation;
        for(float t=0;t<2;t+=Time.deltaTime){m.Drive(Vector2.up,false,Time.deltaTime);yield return null;}
        Check(m.transform.position.z>start.z+4&&m.MotionState=="Caminando","accelerated walking travels forward");
        Check(Quaternion.Angle(leg,m.LeftLeg.localRotation)>2,"walk animates legs");
        for(float t=0;t<.5f;t+=Time.deltaTime){m.Drive(Vector2.zero,false,Time.deltaTime);yield return null;}
        Check(Vector3.ProjectOnPlane(cc.velocity,Vector3.up).magnitude<.05f,"stops without sliding");
        float baseline=m.transform.position.y,maxHeight=baseline;
        m.Drive(Vector2.zero,true,Time.deltaTime);
        for(float t=0;t<1.5f;t+=Time.deltaTime){m.Drive(Vector2.zero,false,Time.deltaTime);maxHeight=Mathf.Max(maxHeight,m.transform.position.y);yield return null;}
        Check(maxHeight>baseline+.5f&&m.Grounded&&Mathf.Abs(m.transform.position.y-baseline)<.07f,"jump and landing return to ground");
        m.Wave();m.Drive(Vector2.zero,false,Time.deltaTime);Check(m.MotionState=="Saludo","manual wave animation");
        yield return new WaitForSeconds(2);m.Speaking=true;m.Drive(Vector2.zero,false,Time.deltaTime);Check(m.MotionState=="Hablando","speaking animation");m.Speaking=false;
        for(float t=0;t<2;t+=Time.deltaTime){m.Drive(new Vector2(.5f,0),false,Time.deltaTime);yield return null;}
        var facing=Vector3.ProjectOnPlane(d.View.transform.forward,Vector3.up).normalized;
        Check(Vector3.Dot(facing,m.FacingDirection)>.9f,"camera follows turns comfortably");
        var camera=d.View.GetComponent<Camera>();Check(Mathf.Abs(camera.fieldOfView-65)<.01f,"stable field of view");
        d.View.FirstPerson=true;yield return null;yield return null;
        Check(Vector3.Distance(d.View.transform.position,m.transform.position+Vector3.up*d.View.EyeHeight)<.02f,"first-person switch has no camera travel");
        d.View.FirstPerson=false;yield return null;
        d.Brain.React("celebrate",1,"friendly");yield return null;
        var popup=GameObject.Find("BB8 · emoción");Check(popup&&popup.transform.position.y>d.BB8.transform.position.y+1,"world-space emotion stays above BB8");
        Check(AudioListener.pause&&AudioListener.volume==0,"reactions remain silent");
        File.WriteAllLines("Logs/presentation-verification.txt",lines);
        var importer=(ModelImporter)AssetImporter.GetAtPath("Assets/Conversation/Minion/source/Minion_Poly_Asset.fbx");
        importer.isReadable=SessionState.GetBool("PresentationReadable",false);importer.SaveAndReimport();
        EditorApplication.Exit(passed?0:1);
    }
}
#endif
