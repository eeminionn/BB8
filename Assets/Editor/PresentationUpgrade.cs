using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class PresentationUpgrade
{
    static PresentationUpgrade(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("PresentationAudit",false)){SessionState.SetBool("PresentationAudit",false);new GameObject("Presentation checks").AddComponent<PresentationCheckDriver>();}};}
    public static void Check(){
        var importer=(ModelImporter)AssetImporter.GetAtPath("Assets/Conversation/Minion/source/Minion_Poly_Asset.fbx");
        SessionState.SetBool("PresentationReadable",importer.isReadable);importer.isReadable=true;importer.SaveAndReimport();
        BB8Setup.OpenDemo();SessionState.SetBool("PresentationAudit",true);EditorApplication.isPlaying=true;
    }
    public static void Upgrade()
    {
        BB8Setup.OpenDemo();var director=Object.FindFirstObjectByType<ConversationDirector>();var minion=director.Minion;
        var cc=minion.GetComponent<CharacterController>();cc.skinWidth=.025f;cc.minMoveDistance=0;cc.stepOffset=.24f;cc.slopeLimit=45;
        var bounds=VisibleBounds(minion.Visual);
        minion.Visual.localScale*=1.7f/bounds.size.y;
        bounds=VisibleBounds(minion.Visual);
        minion.Visual.position+=new Vector3(minion.transform.position.x-bounds.center.x,minion.transform.position.y-cc.skinWidth-bounds.min.y,minion.transform.position.z-bounds.center.z);
        minion.transform.SetPositionAndRotation(new Vector3(2.6f,.08f,-19),Quaternion.Euler(0,-20,0));
        director.BB8.transform.root.position+=new Vector3(0,.55f,-15)-director.BB8.transform.position;
        director.View.Distance=5.2f;director.View.DistanceMin=3.2f;director.View.DistanceMax=7.5f;
        if(!director.GetComponent<ExperienceShell>())director.gameObject.AddComponent<ExperienceShell>();
        var sun=RenderSettings.sun;if(sun){sun.intensity=1.05f;sun.color=new Color(1,.9f,.76f);sun.shadowNormalBias=.12f;sun.shadowStrength=.8f;}
        RenderSettings.ambientSkyColor=new Color(.42f,.51f,.57f);RenderSettings.ambientEquatorColor=new Color(.36f,.38f,.37f);RenderSettings.ambientGroundColor=new Color(.22f,.20f,.17f);
        RenderSettings.reflectionIntensity=.48f;RenderSettings.fogDensity=.0105f;RenderSettings.fogColor=new Color(.56f,.57f,.55f);
        SetColor("Arena cálida",new Color(.48f,.39f,.28f));SetColor("Cerámica envejecida",new Color(.57f,.59f,.55f));SetColor("Acero oxidado",new Color(.31f,.21f,.15f));SetColor("Pintura petróleo",new Color(.10f,.24f,.25f));
        var old=GameObject.Find("Zona de encuentro");if(old)Object.DestroyImmediate(old);
        var arrival=new GameObject("Zona de encuentro").transform;
        var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Scrapyard/Materials/Acero oscuro.mat");
        for(int i=0;i<7;i++){
            var slab=GameObject.CreatePrimitive(PrimitiveType.Cube);slab.name="Placa de acceso";slab.transform.SetParent(arrival,false);
            slab.transform.position=new Vector3(1,.006f,-20+i*1.15f);slab.transform.localScale=new Vector3(4.4f,.012f,.94f);
            slab.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(slab.GetComponent<Collider>());
        }
        QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=45;
        EditorSceneManager.MarkSceneDirty(minion.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        Debug.Log("PRESENTATION_UPGRADE_OK feet="+VisibleBounds(minion.Visual).min.y);
    }
    static void SetColor(string name,Color color){var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Scrapyard/Materials/"+name+".mat");if(mat){mat.color=color;EditorUtility.SetDirty(mat);}}
    public static Bounds VisibleBounds(Transform visual)
    {
        Bounds bounds=default;bool first=true;
        foreach(var renderer in visual.GetComponentsInChildren<SkinnedMeshRenderer>()){
            var mesh=renderer.sharedMesh;var weights=mesh.boneWeights;var bind=mesh.bindposes;var vertices=mesh.vertices;
            foreach(var index in mesh.triangles){var w=weights[index];var v=vertices[index];
                var p=(renderer.bones[w.boneIndex0].localToWorldMatrix*bind[w.boneIndex0]).MultiplyPoint3x4(v)*w.weight0
                    +(renderer.bones[w.boneIndex1].localToWorldMatrix*bind[w.boneIndex1]).MultiplyPoint3x4(v)*w.weight1
                    +(renderer.bones[w.boneIndex2].localToWorldMatrix*bind[w.boneIndex2]).MultiplyPoint3x4(v)*w.weight2
                    +(renderer.bones[w.boneIndex3].localToWorldMatrix*bind[w.boneIndex3]).MultiplyPoint3x4(v)*w.weight3;
                if(first){bounds=new Bounds(p,Vector3.zero);first=false;}else bounds.Encapsulate(p);}
        }
        return bounds;
    }
    public static void Audit()
    {
        BB8Setup.OpenDemo();var minion=Object.FindFirstObjectByType<MinionController>();
        var bounds=VisibleBounds(minion.Visual);var cc=minion.GetComponent<CharacterController>();
        var report=new StringBuilder();report.AppendLine("Root: "+minion.transform.position+" visual: "+minion.Visual.localPosition+" scale: "+minion.Visual.localScale);
        report.AppendLine("Visible geometry: "+bounds+" feet: "+bounds.min.y+" collider bottom: "+(minion.transform.position.y+cc.center.y-cc.height/2));
        foreach(var bone in new[]{minion.LeftArm,minion.RightArm,minion.LeftLeg,minion.RightLeg})report.AppendLine(bone.name+" position: "+bone.position+" rotation: "+bone.rotation.eulerAngles);
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/presentation-audit.txt",report.ToString());Debug.Log(report);
    }
}
