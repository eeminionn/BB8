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
    Transform origin, head, previousTarget, menuRoot, globe;
    TextMesh heading, destinations, action, instructions, status, hud;
    Material fontMaterial, titleMaterial, panelMaterial, globeMaterial;
    Material goldMaterial, blueMaterial, rowMaterial;
    Font font, titleFont;
    float headBaseline, nextRetry;
    bool calibrated, menuWasOpen, selectLatched, placeMenu=true;
    float followDistance=3.4f;
    float placeMenuAfter;
    bool trackingReported;
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
        head.gameObject.AddComponent<QuestViewMirror>();
        foreach(bool left in new[]{true,false}){
            var hand=new GameObject(left?"Quest · left controller":"Quest · right controller");
            hand.transform.SetParent(origin,false);hand.AddComponent<QuestControllerVisual>().Initialize(left,head,shell);
        }
        Debug.Log("BB8_QUEST_READY: left stick advance/yaw; right stick yaw; A jump; RT talk; LT boost; X character; Y view; B menu. Native 3D planets and tracked controller models.");
    }
    void Update()
    {
        if(!calibrated&&QuestInput.IsTracked(XRNode.Head)){
            headBaseline=QuestInput.HeadPosition.ReadValue<Vector3>().y;calibrated=true;
            placeMenu=true;placeMenuAfter=Time.unscaledTime+.25f;
        }
        if(QuestInput.Menu.WasPressedThisFrame())shell.SetMenu(!shell.MenuOpen);
        bool open=shell.MenuOpen;
        if(open&&!menuWasOpen){placeMenu=true;placeMenuAfter=Time.unscaledTime+.25f;}
        menuRoot.gameObject.SetActive(open);hud.gameObject.SetActive(!open);
        if(open){
            float x=QuestInput.Movement.x;
            if(Mathf.Abs(x)<.35f)selectLatched=false;
            if(Mathf.Abs(x)>.7f&&!selectLatched){selectLatched=true;shell.SelectDestination(1-shell.SelectedDestination);}
            if(QuestInput.Jump.WasPressedThisFrame()){QuestInput.SuppressJumpFrame=Time.frameCount;shell.Enter();}
            if(QuestInput.Sound.WasPressedThisFrame())shell.ToggleSound();
            globeMaterial.SetFloat("_Earth",shell.SelectedDestination==1?1:0);
            globe.localRotation=Quaternion.Euler(0,10+(float)(Time.realtimeSinceStartupAsDouble*6%360),-12);
            destinations.text=(shell.SelectedDestination==0?"› KHEPRA":"  KHEPRA")+"\n\n"+(shell.SelectedDestination==1?"› TIERRA":"  TIERRA");
            action.text="A  ·  "+(shell.HasEntered&&WorldDestinations.Instance.ActiveIndex==shell.SelectedDestination?"CONTINUAR":"VIAJAR")+"\nY  ·  SONIDO "+(shell.Muted?"APAGADO":"ENCENDIDO");
            status.text=string.IsNullOrEmpty(shell.TravelStatus)?(shell.SelectedDestination==1?"Planeta Tierra":"Khepra · Puesto 08"):shell.TravelStatus;
        }else{
            float yaw=QuestInput.Yaw*60*Time.unscaledDeltaTime;
            if(Mathf.Abs(yaw)>.001f){
                var localHead=Vector3.ProjectOnPlane(head.position-origin.position,Vector3.up);
                origin.Rotate(Vector3.up,yaw,Space.World);
                roomOffset+=localHead-Quaternion.AngleAxis(yaw,Vector3.up)*localHead;
            }
            if(QuestInput.Sound.WasPressedThisFrame())director.SwitchPOV();
            hud.text=(director.ControlBB8?"BB–8":"MINION")+"   ·   B MENÚ   ·   X PERSONAJE   ·   Y VISTA\n"+
                (director.Voice.Recording?"MIC QUEST  ["+new string('|',Mathf.RoundToInt(director.Voice.MicrophoneLevel*10)).PadRight(10,'·')+"]  ·  SUELTA EL GATILLO":director.Voice.Status)+
                (director.ControlBB8?"\nBATERÍA "+Mathf.RoundToInt(director.BB8.Battery*100)+"%":"");
        }
        menuWasOpen=open;
        if(!director.Voice.Ready&&!director.Voice.Busy&&Time.unscaledTime>nextRetry){nextRetry=Time.unscaledTime+5;director.Voice.Retry();}
        // Dynamic fonts can replace their atlas when another world label requests glyphs.
        fontMaterial.mainTexture=font.material.mainTexture;
        titleMaterial.mainTexture=titleFont.material.mainTexture;
#if DEVELOPMENT_BUILD
        if(!trackingReported&&Time.unscaledTime>8){trackingReported=true;Debug.Log($"BB8_QUEST_TRACKING head={QuestInput.IsTracked(XRNode.Head)} left={QuestInput.IsTracked(XRNode.LeftHand)} right={QuestInput.IsTracked(XRNode.RightHand)} headPose={QuestInput.HeadPosition.ReadValue<Vector3>().sqrMagnitude>.0001f} leftPose={QuestInput.LeftPosition.ReadValue<Vector3>().sqrMagnitude>.0001f} rightPose={QuestInput.RightPosition.ReadValue<Vector3>().sqrMagnitude>.0001f}");}
        if(QuestInput.Jump.WasPressedThisFrame()||QuestInput.Talk.WasPressedThisFrame()||QuestInput.Boost.WasPressedThisFrame()||QuestInput.Switch.WasPressedThisFrame()||QuestInput.Menu.WasPressedThisFrame())
            Debug.Log($"BB8_QUEST_INPUT A={QuestInput.Jump.IsPressed()} talk={QuestInput.Talk.IsPressed()} boost={QuestInput.Boost.IsPressed()} X={QuestInput.Switch.IsPressed()} menu={shell.MenuOpen} tracked={calibrated}");
#endif
    }
    void LateUpdate()
    {
        var target=director.View.Target;if(!target)return;
        if(target!=previousTarget){previousTarget=target;roomOffset=Vector3.zero;}
        Vector3 anchor=target.position+roomOffset+Vector3.up*(director.View.EyeHeight-headBaseline);
        if(!director.View.FirstPerson){
            var focus=target.position+Vector3.up*1.2f;
            var direction=(-origin.forward+Vector3.up*.2f).normalized;
            float distance=3.4f;
            // Ignore the two actor layers; scenery stops the observer going through walls.
            if(Physics.SphereCast(focus,.22f,direction,out var hit,distance,~((1<<8)|(1<<9)),QueryTriggerInteraction.Ignore))distance=Mathf.Max(.3f,hit.distance-.12f);
            followDistance=distance<followDistance?distance:Mathf.MoveTowards(followDistance,distance,4*Time.unscaledDeltaTime);
            anchor=focus+direction*followDistance+roomOffset-Vector3.up*headBaseline;
        }
        origin.position=anchor;
        if(shell.MenuOpen&&placeMenu&&Time.unscaledTime>=placeMenuAfter){PlaceMenu();placeMenu=false;}
    }
    void PlaceMenu()
    {
        var forward=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;
        if(forward.sqrMagnitude<.1f)forward=Vector3.forward;
        var facing=Quaternion.LookRotation(forward);
        menuRoot.SetPositionAndRotation(head.position+forward*3.6f+facing*Vector3.right*.4f,facing);
        globeMaterial.SetVector("_SunDirection",menuRoot.rotation*new Vector3(-.6f,.55f,-.7f));
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
        var opaque=Resources.Load<Shader>("QuestConsole");
        panelMaterial=new Material(opaque){color=new Color(.045f,.085f,.145f,1)};
        goldMaterial=new Material(shader){color=new Color(.95f,.73f,.3f)};
        blueMaterial=new Material(shader){color=new Color(.15f,.56f,.85f)};
        rowMaterial=new Material(opaque){color=new Color(.07f,.18f,.28f)};
        globeMaterial=new Material(Resources.Load<Shader>("QuestPlanet"));
        globeMaterial.SetTexture("_Land",Resources.Load<Texture2D>("EarthLand"));
        menuRoot=new GameObject("Quest · destination console").transform;
        Quad(menuRoot,"Console",Vector3.zero,new Vector2(2.8f,2.05f),panelMaterial);
        Quad(menuRoot,"Upper gold rail",new Vector3(0,1.025f,-.015f),new Vector2(2.8f,.012f),goldMaterial);
        Quad(menuRoot,"Lower blue rail",new Vector3(0,-1.025f,-.015f),new Vector2(2.8f,.008f),blueMaterial);
        Quad(menuRoot,"Destination upper",new Vector3(-.61f,.32f,-.012f),new Vector2(1.18f,.22f),rowMaterial);
        Quad(menuRoot,"Destination lower",new Vector3(-.61f,.065f,-.012f),new Vector2(1.18f,.22f),rowMaterial);
        heading=Label(menuRoot,"BB–8",new Vector3(-1.17f,.89f,-.025f),.032f);
        titleFont=Resources.Load<Font>("Fonts/Anton-Regular");titleFont.RequestCharactersInTexture("BB–8",64);
        titleMaterial=new Material(shader){mainTexture=titleFont.material.mainTexture};titleMaterial.SetFloat("_Font",1);
        heading.font=titleFont;heading.GetComponent<MeshRenderer>().sharedMaterial=titleMaterial;
        Label(menuRoot,"E N C U E N T R O S",new Vector3(-.18f,.80f,-.025f),.010f);
        Quad(menuRoot,"Title rule",new Vector3(0,.55f,-.012f),new Vector2(2.34f,.003f),goldMaterial);
        destinations=Label(menuRoot,"",new Vector3(-1.13f,.42f,-.025f),.017f);
        action=Label(menuRoot,"",new Vector3(-1.13f,-.12f,-.025f),.012f);
        status=Label(menuRoot,"",new Vector3(-1.13f,-.40f,-.025f),.009f);
        instructions=Label(menuRoot,"ELIGE CON EL JOYSTICK IZQUIERDO · A PARA ENTRAR\n\nY · PRIMERA / TERCERA PERSONA     X · PERSONAJE\nMIRA TUS MANDOS PARA CONSULTAR LOS CONTROLES",new Vector3(-1.13f,-.59f,-.025f),.0084f);
        instructions.color=new Color(.78f,.89f,1);
        var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);sphere.name="Planet · true stereo sphere";sphere.layer=9;
        Destroy(sphere.GetComponent<Collider>());globe=sphere.transform;globe.SetParent(menuRoot,false);
        globe.localPosition=new Vector3(.67f,.12f,-.44f);globe.localScale=Vector3.one*.91f;
        sphere.GetComponent<MeshRenderer>().sharedMaterial=globeMaterial;
        hud=Label(head,"",new Vector3(-.48f,-.40f,1.5f),.0048f);
        PlaceMenu();
    }
    TextMesh Label(Transform parent,string text,Vector3 position,float size)
    {
        var label=new GameObject("Console text").AddComponent<TextMesh>();label.gameObject.layer=9;
        label.transform.SetParent(parent,false);label.transform.localPosition=position;
        label.font=font;label.fontSize=64;label.characterSize=size;label.anchor=TextAnchor.UpperLeft;
        label.text=text;label.color=new Color(1,.9f,.58f);
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
        if(menuRoot)Destroy(menuRoot.gameObject);
        Destroy(fontMaterial);Destroy(titleMaterial);Destroy(panelMaterial);Destroy(globeMaterial);
        Destroy(goldMaterial);Destroy(blueMaterial);Destroy(rowMaterial);
        if(origin)Destroy(origin.gameObject);
    }
}
