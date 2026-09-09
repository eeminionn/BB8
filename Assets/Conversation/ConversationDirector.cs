using UnityEngine;
[DefaultExecutionOrder(-200)]
public sealed class ConversationDirector : MonoBehaviour
{
    public BbRigidbodyController BB8;
    public MinionController Minion;
    public DragMouseOrbit View;
    public LocalVoiceClient Voice;
    public BB8SocialBrain Brain;
    public bool ControlBB8 { get; private set; }
    public bool Typing { get; private set; }
    bool diagnostic;
    bool suppressOpeningT;
    string typed="";
    Renderer[] hidden=new Renderer[0];
    public BB8ReactionPopup Popup { get; private set; }
    void Start(){Popup=gameObject.AddComponent<BB8ReactionPopup>();Popup.Initialize(Brain);Voice.Result+=React;ApplyView();}
    void React(VoiceResult result){if(Brain.ExecuteCommand(result.command))Popup.ShowCommand(result.command);else Brain.React(result.reaction,result.intensity,result.attitude);}
    public void SwitchCharacter(){ControlBB8=!ControlBB8;if(ControlBB8)Brain.CancelCommand();BB8.SetInput(Vector2.zero,false,false);ApplyView();}
    public void SwitchPOV(){View.FirstPerson=!View.FirstPerson;ApplyView();}
    void ApplyView(){
        foreach(var r in hidden)if(r)r.enabled=true;
        View.Target=ControlBB8?BB8.transform:Minion.transform;View.EyeHeight=ControlBB8?1.02f:1.43f;
        hidden=new Renderer[0];
        if(View.FirstPerson){
            var all=ControlBB8?BB8.transform.parent.GetComponentsInChildren<Renderer>():Minion.GetComponentsInChildren<Renderer>();
            hidden=System.Array.FindAll(all,r=>r.enabled);foreach(var r in hidden)r.enabled=false;
        }
    }
    void Update(){
        if(ExperienceShell.Instance&&ExperienceShell.Instance.MenuOpen){BB8.AcceptPlayerInput=false;BB8.SetInput(Vector2.zero,false,false);Minion.Controlled=false;View.SuspendInput=true;return;}
        if(Input.GetKeyDown(KeyCode.F3))diagnostic=!diagnostic;
        if(!Typing){
            if(Input.GetKeyDown(KeyCode.Tab))SwitchCharacter();
            if(Input.GetKeyDown(KeyCode.V))SwitchPOV();
            if(Input.GetKeyDown(KeyCode.E)&&!Brain.Busy)Voice.BeginRecording();
        }
        bool conversational=Voice.Busy||Voice.Recording||Brain.Busy;
        Brain.Listening=Voice.Ready&&(Voice.Busy||Voice.Recording);
        Minion.Speaking=Voice.Recording;
        bool manual=ControlBB8&&!conversational&&!Typing&&!Brain.Autonomous;
        if(BB8.AcceptPlayerInput && !manual) BB8.SetInput(Vector2.zero,false,false);
        BB8.AcceptPlayerInput=manual;Minion.Controlled=!ControlBB8&&!Typing;
        View.SuspendInput=Typing;
    }
    void OnGUI(){
        if(ExperienceShell.Instance&&ExperienceShell.Instance.MenuOpen)return;
        // Handle shortcuts before TextField consumes Return.
        var e=Event.current;
        if(suppressOpeningT && e.keyCode==KeyCode.None && (e.character=='t'||e.character=='T')){
            suppressOpeningT=false;e.Use();
        }
        if(e.type==EventType.KeyUp && e.keyCode==KeyCode.T)suppressOpeningT=false;
        if(!Typing && e.keyCode==KeyCode.T && e.type==EventType.KeyDown){
            if(!Voice.Busy&&!Voice.Recording){Typing=true;suppressOpeningT=true;}
            e.Use();
        }
        if(Typing && e.type==EventType.KeyDown){
            if(e.keyCode==KeyCode.Escape){Typing=false;e.Use();}
            else if(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.KeypadEnter){SubmitText();e.Use();}
        }
        UITheme.Init();var previousMatrix=GUI.matrix;
        float scale=Mathf.Max(.65f,Mathf.Min(Screen.height/900f,Screen.width/1280f)),width=Screen.width/scale,height=Screen.height/scale;
        GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
        UITheme.Panel(new Rect(24,24,300,64));
        GUI.Label(new Rect(40,32,275,22),WorldDestinations.Instance?WorldDestinations.Instance.AreaLabel:"KHEPRA / PUESTO 08",UITheme.Kicker);
        GUI.Label(new Rect(40,57,265,20),(ControlBB8?"BB–8":"MINION")+"   ·   "+(View.FirstPerson?"Primera persona":"Tercera persona"),UITheme.Small);
        string state=Voice.Recording?"ESCUCHANDO":Voice.Busy?(Voice.Ready?"PREPARANDO RESPUESTA":"INICIANDO VOZ"):Voice.Ready?"VOZ LISTA":"VOZ NO DISPONIBLE";
        UITheme.Panel(new Rect(width-270,24,246,64));
        GUI.Label(new Rect(width-254,32,215,22),state,UITheme.Kicker);
        GUI.Label(new Rect(width-254,57,215,20),ExperienceShell.Instance&&ExperienceShell.Instance.Muted?"Sonido desactivado":"Sonido activado",UITheme.Small);
        float w=Mathf.Min(620,width-48),x=(width-w)/2;
        if(Typing){
            UITheme.Panel(new Rect(x,height-185,w,126));UITheme.Fill(new Rect(x,height-185,3,126),UITheme.Accent);
            GUI.Label(new Rect(x+18,height-172,w-36,23),"MENSAJE PARA BB–8",UITheme.Kicker);
            GUI.SetNextControlName("utterance");typed=GUI.TextField(new Rect(x+18,height-140,w-135,44),typed,600,UITheme.Field);GUI.FocusControl("utterance");
            if(GUI.Button(new Rect(x+w-103,height-140,85,44),"ENVIAR",UITheme.Primary))SubmitText();
            GUI.Label(new Rect(x+18,height-88,w-36,22),"Enter envía · Esc cierra · Prueba «sígueme» o «aléjate»",UITheme.Small);
        }else{
            string prompt=Voice.Ready&&!Voice.Busy&&!Voice.Recording?"E  Mantén para hablar    /    T  Escribe a BB–8":Voice.Status;
            if(Popup&&Popup.Visible&&!string.IsNullOrEmpty(Voice.Transcript))prompt="“"+Voice.Transcript+"”";
            UITheme.Panel(new Rect(x,height-123,w,56));GUI.Label(new Rect(x+18,height-112,w-36,40),prompt,UITheme.Body);
            if(!Voice.Ready&&!Voice.Busy&&GUI.Button(new Rect(width-168,100,144,34),"REINTENTAR VOZ",UITheme.Button))Voice.Retry();
        }
        if(ControlBB8){
            UITheme.Panel(new Rect(24,height-135,210,68));
            GUI.Label(new Rect(40,height-127,180,22),"IMPULSO   "+Mathf.RoundToInt(BB8.Battery*100)+"%",UITheme.Kicker);
            UITheme.Fill(new Rect(40,height-99,178,4),UITheme.Line);
            UITheme.Fill(new Rect(40,height-99,178*BB8.Battery,4),BB8.Battery<.15f?new Color(.94f,.62f,.29f):UITheme.Accent);
            GUI.Label(new Rect(40,height-87,180,18),"Shift · Celdas para recargar",UITheme.Small);
        }
        UITheme.Fill(new Rect(0,height-48,width,48),new Color(.018f,.029f,.038f,.9f));
        GUI.Label(new Rect(24,height-35,width-48,22),"W / S avanzar   ·   A / D girar   ·   Espacio saltar   ·   Tab personaje   ·   V vista   ·   Mouse derecho mirar   ·   Esc menú",UITheme.Small);
        if(diagnostic){
            UITheme.Panel(new Rect(width-340,108,316,175));var r=Voice.LastResult;
            GUI.Label(new Rect(width-324,121,285,90),r==null?"Sin interpretación todavía.":r.emotion+" / "+r.attitude+"\nIntensidad "+r.intensity+" · "+r.seconds.ToString("0.0")+" s\n"+r.reaction,UITheme.Body);
            var devices=Microphone.devices;
            if(GUI.Button(new Rect(width-324,209,284,28),devices.Length==0?"Sin micrófono":devices[Mathf.Clamp(Voice.MicrophoneIndex,0,devices.Length-1)],UITheme.Button)&&!Voice.Recording)Voice.MicrophoneIndex=(Voice.MicrophoneIndex+1)%Mathf.Max(1,devices.Length);
            for(int level=1;level<=3;level++)if(GUI.Button(new Rect(width-324+(level-1)*96,245,92,24),"Alegría "+level,UITheme.Button)&&!Voice.Busy&&!Brain.Busy)Brain.React("celebrate",level,"friendly");
        }
        GUI.matrix=previousMatrix;
    }
    void SubmitText(){
        if(!Voice.Ready||Voice.Busy||Voice.Recording||string.IsNullOrWhiteSpace(typed))return;
        Voice.SendText(typed);typed="";Typing=false;GUI.FocusControl(null);
    }
    void OnDestroy(){if(Voice)Voice.Result-=React;foreach(var r in hidden)if(r)r.enabled=true;}
}
