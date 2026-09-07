using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class ConversationBuilder
{
    const string Root="Assets/Conversation";
    [MenuItem("BB8/Install Conversation")]
    public static void Install(){
        BB8Setup.OpenDemo();
        var old=UnityEngine.Object.FindFirstObjectByType<ConversationDirector>();
        if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var previous=UnityEngine.Object.FindFirstObjectByType<MinionController>();if(previous)UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var bb=UnityEngine.Object.FindFirstObjectByType<BbRigidbodyController>();
        var view=UnityEngine.Object.FindFirstObjectByType<DragMouseOrbit>();
        var root=new GameObject("Minion");root.transform.position=new Vector3(2.5f,.2f,-15.5f);root.transform.rotation=Quaternion.Euler(0,210,0);
        var motor=root.AddComponent<CharacterController>();motor.height=1.7f;motor.radius=.38f;motor.center=new Vector3(0,.85f,0);motor.stepOffset=.3f;motor.slopeLimit=50;
        var minion=root.AddComponent<MinionController>();minion.Camera=view.transform;
        string modelPath=Root+"/Minion/source/Minion_Poly_Asset.fbx";
        var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
        importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=false;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root.transform);
        visual.name="Minion visual";visual.transform.localRotation=Quaternion.Euler(0,90,0);
        var anim=visual.GetComponent<Animator>();if(anim)UnityEngine.Object.DestroyImmediate(anim);
        minion.Visual=visual.transform;
        var renderers=visual.GetComponentsInChildren<Renderer>();
        Bounds bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        visual.transform.localScale*=1.7f/bounds.size.y;
        bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        visual.transform.position+=root.transform.position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
        var bones=visual.GetComponentsInChildren<Transform>();
        minion.LeftArm=bones.FirstOrDefault(t=>t.name=="Bone.003");minion.RightArm=bones.FirstOrDefault(t=>t.name=="Bone.015");
        minion.LeftLeg=bones.FirstOrDefault(t=>t.name=="Bone.025");minion.RightLeg=bones.FirstOrDefault(t=>t.name=="Bone.033");
        Directory.CreateDirectory(Root+"/Minion/Materials");
        foreach(var r in renderers){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++){
            if(!mats[i])continue;var name=mats[i].name;string path=Root+"/Minion/Materials/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat){mat=new Material(Shader.Find("Standard")){name=name,color=mats[i].color};
                mat.SetFloat("_Glossiness",.25f);
                if(name=="jeans"||name=="Material.003"){mat.color=Color.white;mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Minion/textures/Fabric_denim_01_basecolor.jpeg");}
                if(name=="metalic"){mat.SetFloat("_Metallic",.72f);mat.SetFloat("_Glossiness",.55f);}
                AssetDatabase.CreateAsset(mat,path);
            }
            if(name=="Material"){mat.mainTextureScale=new Vector2(.2f,.2f);mat.mainTextureOffset=new Vector2(.45f,.4f);mat.color=Color.white;mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Minion/textures/bake1.png");}
            if(name=="body") mat.color=new Color(1f,.76f,.055f);
            if(name=="Material.001") {mat.color=new Color(1,1,1,.08f);mat.SetFloat("_Mode",3);mat.SetInt("_SrcBlend",1);mat.SetInt("_DstBlend",10);mat.SetInt("_ZWrite",0);mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");mat.renderQueue=3000;}
            EditorUtility.SetDirty(mat);mats[i]=mat;
        }r.sharedMaterials=mats;}
        foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=9;
        var brain=bb.GetComponent<BB8SocialBrain>();if(!brain)brain=bb.gameObject.AddComponent<BB8SocialBrain>();brain.Partner=root.transform;
        Directory.CreateDirectory(Root+"/Reactions");
        brain.Reactions=new[]{Profile("celebrate","jump-01",3,15,7,0,1.15f,true),Profile("comfort","recharge",3.8f,6,1.8f,.14f,.8f,false),Profile("retreat","bump-02",2.8f,20,8,-.25f,.8f,false),Profile("cautious","bump-01",3.4f,9,4,-.08f,1.1f,false),Profile("attentive","recharge",2.4f,7,3,0,.95f,false),Profile("puzzled","jump-03",2.7f,20,2,0,.75f,false)};
        var system=new GameObject("Conversación local");var voice=system.AddComponent<LocalVoiceClient>();var director=system.AddComponent<ConversationDirector>();
        director.BB8=bb;director.Minion=minion;director.View=view;director.Voice=voice;director.Brain=brain;
        bb.AcceptPlayerInput=false;view.Target=root.transform;
        PlayerSettings.iOS.microphoneUsageDescription="Habla con BB-8. El audio se procesa solo en este Mac y no se guarda.";
        PrefabUtility.SaveAsPrefabAsset(root,Root+"/Minion.prefab");
        EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        Debug.Log("BB8_CONVERSATION_INSTALLED minionHeight="+bounds.size.y+" bones="+bones.Length);
    }
    static SocialReaction Profile(string id,string clip,float duration,float tilt,float speed,float movement,float pitch,bool bounce){
        string path=Root+"/Reactions/"+id+".asset";var p=AssetDatabase.LoadAssetAtPath<SocialReaction>(path);if(p)return p;
        p=ScriptableObject.CreateInstance<SocialReaction>();p.Id=id;p.Sound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Scrapyard/Audio/"+clip+".wav");p.Duration=duration;p.HeadTilt=tilt;p.HeadSpeed=speed;p.Movement=movement;p.Pitch=pitch;p.Bounce=bounce;AssetDatabase.CreateAsset(p,path);return p;
    }
}
