using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

// WebXR grip-space models, at real physical scale, attached to the tracked origin.
public sealed class QuestControllerVisual : MonoBehaviour
{
    bool left;
    Transform head, model;
    ExperienceShell shell;
    TextMesh legend;
    Material material, fontMaterial;
    Font font;
    InputAction tracked;
    readonly Dictionary<string,Transform> nodes=new Dictionary<string,Transform>();
    MaterialPropertyBlock highlight;
    public void Initialize(bool isLeft,Transform viewer,ExperienceShell experience)
    {
        left=isLeft;head=viewer;shell=experience;
        highlight=new MaterialPropertyBlock();
        string hand=left?"left":"right";
        model=InstantiateModel(hand,transform);
        foreach(var node in model.GetComponentsInChildren<Transform>()) {nodes[node.name]=node;node.gameObject.layer=9;}
        // FBX's axis conversion preserves the WebXR grip origin and metre units.
        material=new Material(Resources.Load<Shader>("QuestController")){mainTexture=Resources.Load<Texture2D>("QuestControllers/"+hand+"-Texture")};
        foreach(var renderer in model.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=material;
        var pose=gameObject.AddComponent<TrackedPoseDriver>();
        pose.positionInput=new InputActionProperty(left?QuestInput.LeftPosition:QuestInput.RightPosition);
        pose.rotationInput=new InputActionProperty(left?QuestInput.LeftRotation:QuestInput.RightRotation);
        pose.ignoreTrackingState=true;pose.updateType=TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
        tracked=left?QuestInput.LeftTracked:QuestInput.RightTracked;
        font=Resources.Load<Font>("Fonts/Rajdhani-Medium");
        legend=new GameObject("Controller guide").AddComponent<TextMesh>();
        legend.transform.SetParent(transform,false);legend.transform.localPosition=new Vector3(left?-.10f:.10f,.17f,0);
        legend.gameObject.layer=9;legend.font=font;legend.fontSize=64;legend.characterSize=.0025f;
        legend.anchor=TextAnchor.MiddleCenter;legend.alignment=TextAlignment.Center;legend.color=new Color(1,.91f,.61f);
        fontMaterial=new Material(Resources.Load<Shader>("BB8WorldPopup")){mainTexture=font.material.mainTexture};fontMaterial.SetFloat("_Font",1);
        legend.GetComponent<MeshRenderer>().sharedMaterial=fontMaterial;
    }
    public static Transform InstantiateModel(string hand,Transform parent)
    {
        var model=Instantiate(Resources.Load<GameObject>("QuestControllers/"+hand),parent,false).transform;
        // Preserve FBX's baked axis/units transform (including its ~-88° pitch).
        // Unity's FBX importer mirrors X; WebXR-to-Unity grip space mirrors Z.
        var correction=Quaternion.Euler(0,180,0);
        model.localPosition=correction*model.localPosition;
        model.localRotation=correction*model.localRotation;
        return model;
    }
    void LateUpdate()
    {
        bool visible=QuestInput.IsTracked(left?UnityEngine.XR.XRNode.LeftHand:UnityEngine.XR.XRNode.RightHand);model.gameObject.SetActive(visible);
        // Guides stay beside the hands; only appear when the user looks towards them.
        legend.gameObject.SetActive(visible&&Vector3.Dot(head.forward,(transform.position-head.position).normalized)>.65f);
        legend.transform.rotation=Quaternion.LookRotation(legend.transform.position-head.position,head.up);
        legend.text=left?(shell.MenuOpen?"X · PERSONAJE     Y · SONIDO\nPALANCA · DESTINO":"X · PERSONAJE     Y · VISTA\nPALANCA · MOVER / GIRAR\nGATILLO · TURBO BB–8"):
            (shell.MenuOpen?"A · ENTRAR     B · CERRAR":"A · SALTAR     B · MENÚ\nPALANCA · GIRAR\nGATILLO · HABLAR");
        fontMaterial.mainTexture=font.material.mainTexture;
        Button(left?"x_button":"a_button",QuestInput.Switch,QuestInput.Jump);
        Button(left?"y_button":"b_button",QuestInput.Sound,QuestInput.Menu);
        float trigger=(left?QuestInput.Boost:QuestInput.Talk).ReadValue<float>();
        Animate("xr_standard_trigger",trigger);Glow("trigger",trigger>.5f);
        var stick=(left?QuestInput.Move:QuestInput.Turn).ReadValue<Vector2>();
        Animate("xr_standard_thumbstick_xaxis",(stick.x+1)*.5f);
        Animate("xr_standard_thumbstick_yaxis",(stick.y+1)*.5f);
    }
    void Button(string name,InputAction leftAction,InputAction rightAction)
    {
        bool pressed=(left?leftAction:rightAction).IsPressed();Animate(name,pressed?1:0);Glow(name,pressed);
    }
    void Animate(string name,float amount)
    {
        if(!nodes.TryGetValue(name+"_pressed_value",out var value)||!nodes.TryGetValue(name+"_pressed_min",out var min)||!nodes.TryGetValue(name+"_pressed_max",out var max))return;
        value.localPosition=Vector3.Lerp(min.localPosition,max.localPosition,amount);
        value.localRotation=Quaternion.Slerp(min.localRotation,max.localRotation,amount);
    }
    void Glow(string name,bool active)
    {
        if(!nodes.TryGetValue(name,out var node))return;
        highlight.SetColor("_Glow",active?new Color(.55f,.32f,.05f):Color.black);
        foreach(var renderer in node.GetComponentsInChildren<Renderer>())renderer.SetPropertyBlock(highlight);
    }
    void OnDestroy(){Destroy(material);Destroy(fontMaterial);}
}
