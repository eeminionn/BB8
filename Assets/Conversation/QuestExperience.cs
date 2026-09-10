using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

[DefaultExecutionOrder(-250)]
public sealed class QuestExperience : MonoBehaviour
{
    ConversationDirector director;
    ExperienceShell shell;
    Transform origin, head, previousTarget, menuRoot;
    TextMesh heading, destinations, action, instructions, status, hud;
    Material fontMaterial, panelMaterial, globeMaterial;
    MenuPlanetRenderer planet;
    Font font;
    float headBaseline, nextRetry;
    bool calibrated, menuWasOpen, turnLatched, selectLatched, placeMenu=true;
    Vector3 roomOffset;
    readonly List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();

    void Awake()
    {
        director=GetComponent<ConversationDirector>();shell=GetComponent<ExperienceShell>();
        head=director.View.transform;
        origin=new GameObject("Quest · tracked origin").transform;
        head.SetParent(origin,false);head.localPosition=Vector3.zero;head.localRotation=Quaternion.identity;
        var camera=head.GetComponent<Camera>();camera.nearClipPlane=.05f;camera.stereoTargetEye=StereoTargetEyeMask.Both;
        var pose=head.gameObject.AddComponent<TrackedPoseDriver>();
        pose.positionInput=new InputActionProperty(QuestInput.HeadPosition);
        pose.rotationInput=new InputActionProperty(QuestInput.HeadRotation);
        pose.ignoreTrackingState=true;pose.updateType=TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
        SubsystemManager.GetSubsystems(subsystems);
        foreach(var system in subsystems)system.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
        QualitySettings.vSyncCount=0;Application.targetFrameRate=72;
        QualitySettings.antiAliasing=2;QualitySettings.shadowDistance=18;QualitySettings.shadowCascades=0;
        QualitySettings.shadowResolution=ShadowResolution.Low;QualitySettings.pixelLightCount=1;
        XRSettings.eyeTextureResolutionScale=.9f;
        BuildMenu();
        Debug.Log("BB8_QUEST_READY: left stick move; A jump; right trigger talk; left trigger boost; X character; B menu; right stick snap turn.");
    }
    void Update()
    {
        if(!calibrated&&QuestInput.HeadTracked.IsPressed()){
            headBaseline=QuestInput.HeadPosition.ReadValue<Vector3>().y;calibrated=true;
            placeMenu=true;
        }
        if(QuestInput.Menu.WasPressedThisFrame())shell.SetMenu(!shell.MenuOpen);
        bool open=shell.MenuOpen;
        if(open&&!menuWasOpen)placeMenu=true;
        menuRoot.gameObject.SetActive(open);hud.gameObject.SetActive(!open);
        if(open){
            float x=QuestInput.Movement.x;
            if(Mathf.Abs(x)<.35f)selectLatched=false;
            if(Mathf.Abs(x)>.7f&&!selectLatched){selectLatched=true;shell.SelectDestination(1-shell.SelectedDestination);}
            if(QuestInput.Jump.WasPressedThisFrame()){QuestInput.SuppressJumpFrame=Time.frameCount;shell.Enter();}
            if(QuestInput.Sound.WasPressedThisFrame())shell.ToggleSound();
            globeMaterial.mainTexture=planet.Render(shell.SelectedDestination,Time.realtimeSinceStartupAsDouble);
            destinations.text=(shell.SelectedDestination==0?"› KHEPRA":"  KHEPRA")+"\n\n"+(shell.SelectedDestination==1?"› TIERRA":"  TIERRA");
            action.text="A  ·  "+(shell.HasEntered&&WorldDestinations.Instance.ActiveIndex==shell.SelectedDestination?"CONTINUAR":"VIAJAR")+"\nY  ·  SONIDO "+(shell.Muted?"APAGADO":"ENCENDIDO");
            status.text=string.IsNullOrEmpty(shell.TravelStatus)?(shell.SelectedDestination==1?"Planeta Tierra":"Khepra · Puesto 08"):shell.TravelStatus;
        }else{
            float x=QuestInput.Turn.ReadValue<Vector2>().x;
            if(Mathf.Abs(x)<.3f)turnLatched=false;
            if(Mathf.Abs(x)>.7f&&!turnLatched){turnLatched=true;var before=origin.position;origin.RotateAround(head.position,Vector3.up,Mathf.Sign(x)*30);roomOffset+=origin.position-before;}
            hud.text=(director.ControlBB8?"BB–8":"MINION")+"   ·   B MENÚ   ·   X PERSONAJE\n"+
                (director.Voice.Recording?"ESCUCHANDO · SUELTA EL GATILLO":director.Voice.Status)+
                (director.ControlBB8?"\nBATERÍA "+Mathf.RoundToInt(director.BB8.Battery*100)+"%":"");
        }
        menuWasOpen=open;
        if(!director.Voice.Ready&&!director.Voice.Busy&&Time.unscaledTime>nextRetry){nextRetry=Time.unscaledTime+5;director.Voice.Retry();}
        // Dynamic fonts can replace their atlas when another world label requests glyphs.
        fontMaterial.mainTexture=font.material.mainTexture;
#if DEVELOPMENT_BUILD
        if(QuestInput.Jump.WasPressedThisFrame()||QuestInput.Talk.WasPressedThisFrame()||QuestInput.Boost.WasPressedThisFrame()||QuestInput.Switch.WasPressedThisFrame()||QuestInput.Menu.WasPressedThisFrame())
            Debug.Log($"BB8_QUEST_INPUT A={QuestInput.Jump.IsPressed()} talk={QuestInput.Talk.IsPressed()} boost={QuestInput.Boost.IsPressed()} X={QuestInput.Switch.IsPressed()} menu={shell.MenuOpen} tracked={calibrated}");
#endif
    }
    void LateUpdate()
    {
        var target=director.View.Target;if(!target)return;
        if(target!=previousTarget){previousTarget=target;roomOffset=Vector3.zero;}
        origin.position=target.position+roomOffset+Vector3.up*(director.View.EyeHeight-headBaseline);
        if(shell.MenuOpen&&placeMenu){PlaceMenu();placeMenu=false;}
    }
    void PlaceMenu()
    {
        var forward=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;
        if(forward.sqrMagnitude<.1f)forward=Vector3.forward;
        menuRoot.SetPositionAndRotation(head.position+forward*3,Quaternion.LookRotation(forward));
    }
    void OnApplicationFocus(bool focused)
    {
        if(!focused&&shell){shell.SetMenu(true);director.BB8.SetInput(Vector2.zero,false,false);}
    }
    void BuildMenu()
    {
        font=Resources.Load<Font>("Fonts/Rajdhani-Medium");
        font.RequestCharactersInTexture("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÁÉÍÓÚáéíóúñÑ0123456789›·–%",64);
        var shader=Resources.Load<Shader>("BB8WorldPopup");
        fontMaterial=new Material(shader){mainTexture=font.material.mainTexture};fontMaterial.SetFloat("_Font",1);
        panelMaterial=new Material(shader){color=new Color(.012f,.025f,.04f,.97f)};
        globeMaterial=new Material(shader);
        menuRoot=new GameObject("Quest · destination console").transform;
        Quad(menuRoot,"Console",Vector3.zero,new Vector2(2.8f,2.05f),panelMaterial);
        heading=Label(menuRoot,"BB–8 / ENCUENTROS",new Vector3(-1.17f,.8f,-.025f),.021f);
        destinations=Label(menuRoot,"",new Vector3(-1.13f,.43f,-.025f),.02f);
        action=Label(menuRoot,"",new Vector3(-1.13f,-.12f,-.025f),.012f);
        status=Label(menuRoot,"",new Vector3(-1.13f,-.40f,-.025f),.009f);
        instructions=Label(menuRoot,"JOYSTICK IZQUIERDO · DESTINO     A · ENTRAR\n\nAL EXPLORAR: A SALTA · X CAMBIA PERSONAJE · B MENÚ\nGATILLO DERECHO: HABLAR · IZQUIERDO: TURBO BB–8\nJOYSTICK DERECHO: GIRO POR PASOS",new Vector3(-1.13f,-.59f,-.025f),.0076f);
        planet=new MenuPlanetRenderer();
        Quad(menuRoot,"Planet",new Vector3(.69f,.27f,-.025f),new Vector2(1.03f,1.03f),globeMaterial);
        hud=Label(head,"",new Vector3(-.48f,-.40f,1.5f),.0048f);
        PlaceMenu();
    }
    TextMesh Label(Transform parent,string text,Vector3 position,float size)
    {
        var label=new GameObject("Console text").AddComponent<TextMesh>();label.gameObject.layer=9;
        label.transform.SetParent(parent,false);label.transform.localPosition=position;
        label.font=font;label.fontSize=64;label.characterSize=size;label.anchor=TextAnchor.UpperLeft;
        label.text=text;label.color=new Color(.92f,.79f,.38f);
        label.GetComponent<MeshRenderer>().sharedMaterial=fontMaterial;
        return label;
    }
    static void Quad(Transform parent,string name,Vector3 position,Vector2 size,Material material)
    {
        var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name=name;quad.layer=9;
        Destroy(quad.GetComponent<Collider>());quad.transform.SetParent(parent,false);
        quad.transform.localPosition=position;quad.transform.localScale=new Vector3(size.x,size.y,1);
        quad.GetComponent<MeshRenderer>().sharedMaterial=material;
    }
    void OnDestroy()
    {
        planet?.Dispose();if(menuRoot)Destroy(menuRoot.gameObject);
        Destroy(fontMaterial);Destroy(panelMaterial);Destroy(globeMaterial);
        if(origin)Destroy(origin.gameObject);
    }
}
