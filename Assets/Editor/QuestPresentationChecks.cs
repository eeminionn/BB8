using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Offscreen, silent visual check of the same runtime geometry and shaders.
public static class QuestPresentationChecks
{
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public static void Run()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Preview camera").AddComponent<Camera>();
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.008f,.015f,.03f);
        camera.fieldOfView=48;camera.aspect=1.5f;
        var host=new GameObject("Menu preview").AddComponent<QuestExperience>();
        typeof(QuestExperience).GetField("head",Private).SetValue(host,camera.transform);
        typeof(QuestExperience).GetMethod("BuildMenu",Private).Invoke(host,null);
        Text(host,"destinations","› KHEPRA\n\n  TIERRA");
        Text(host,"action","A  ·  VIAJAR\nY  ·  SONIDO APAGADO");Text(host,"status","Khepra · Puesto 08");
        foreach(string hand in new[]{"left","right"}){
            var model=QuestControllerVisual.InstantiateModel(hand,null).gameObject;
            Debug.Log("BB8_CONTROLLER_ROOT "+hand+" rotation="+model.transform.localRotation.eulerAngles+" scale="+model.transform.localScale);
            var bounds=new Bounds(model.transform.position,Vector3.zero);
            var material=new Material(Resources.Load<Shader>("QuestController")){mainTexture=Resources.Load<Texture2D>("QuestControllers/"+hand+"-Texture")};
            foreach(var r in model.GetComponentsInChildren<Renderer>()){bounds.Encapsulate(r.bounds);r.sharedMaterial=material;}
            if(bounds.size.magnitude<.12f||bounds.size.magnitude>.45f)throw new Exception("Controller scale invalid: "+hand+" "+bounds.size);
            Debug.Log("BB8_CONTROLLER_BOUNDS "+hand+" "+bounds.size);
            foreach(var renderer in model.GetComponentsInChildren<Renderer>())Debug.Log("BB8_CONTROLLER_MESH "+hand+" "+renderer.name+" center="+renderer.bounds.center.ToString("F5")+" rotation="+renderer.transform.rotation.eulerAngles);
            var grip=new GameObject("Preview grip").transform;model.transform.SetParent(grip,false);
            grip.position=new Vector3(hand=="left"?-.40f:.40f,-.30f,1.4f);
            grip.rotation=Quaternion.Euler(0,hand=="left"?-15:15,0);
        }
        var rt=new RenderTexture(1200,800,24);camera.targetTexture=rt;
        camera.Render();RenderTexture.active=rt;
        var image=new Texture2D(1200,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1200,800),0,0);image.Apply();
        Directory.CreateDirectory("Logs");File.WriteAllBytes("Logs/quest-presentation.png",image.EncodeToPNG());
        var materialPlanet=(Material)typeof(QuestExperience).GetField("globeMaterial",Private).GetValue(host);
        materialPlanet.SetFloat("_Earth",1);Text(host,"status","Planeta Tierra");Text(host,"destinations","  KHEPRA\n\n› TIERRA");
        camera.Render();image.ReadPixels(new Rect(0,0,1200,800),0,0);image.Apply();File.WriteAllBytes("Logs/quest-earth.png",image.EncodeToPNG());
        Debug.Log("BB8_QUEST_PRESENTATION_OK");
    }
    static void Text(QuestExperience host,string field,string value)=>((TextMesh)typeof(QuestExperience).GetField(field,Private).GetValue(host)).text=value;
}
