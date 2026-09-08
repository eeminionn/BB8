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
    string typed="";
    Renderer[] hidden=new Renderer[0];
    GUIStyle label,title;
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
        if(Input.GetKeyDown(KeyCode.F3))diagnostic=!diagnostic;
        if(!Typing){
            if(Input.GetKeyDown(KeyCode.Tab))SwitchCharacter();
            if(Input.GetKeyDown(KeyCode.V))SwitchPOV();
            if(Input.GetKeyDown(KeyCode.E)&&!Brain.Busy)Voice.BeginRecording();
        }
        bool conversational=Voice.Busy||Voice.Recording||Brain.Busy;
        Brain.Listening=Voice.Ready&&(Voice.Busy||Voice.Recording);
        bool manual=ControlBB8&&!conversational&&!Typing&&!Brain.Autonomous;
        if(BB8.AcceptPlayerInput && !manual) BB8.SetInput(Vector2.zero,false,false);
        BB8.AcceptPlayerInput=manual;Minion.Controlled=!ControlBB8&&!Typing;
        View.SuspendInput=Typing;
    }
    void OnGUI(){
        // Handle shortcuts before TextField consumes Return.
        var e=Event.current;
        if(e.keyCode==KeyCode.None && (e.character=='t'||e.character=='T'))e.Use();
        if(e.keyCode==KeyCode.T && (e.type==EventType.KeyDown||e.type==EventType.KeyUp)){
            if(e.type==EventType.KeyDown && (Typing||(!Voice.Busy&&!Voice.Recording)))Typing=!Typing;
            e.Use();
        }
        if(Typing && e.type==EventType.KeyDown){
            if(e.keyCode==KeyCode.Escape){Typing=false;e.Use();}
            else if(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.KeypadEnter){SubmitText();e.Use();}
        }
        var previousMatrix=GUI.matrix;
        float scale=Mathf.Max(.75f,Screen.height/900f),width=Screen.width/scale,height=Screen.height/scale;
        GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
        if(label==null){label=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=true,normal={textColor=new Color(.85f,.93f,.93f)}};title=new GUIStyle(label){fontSize=18,fontStyle=FontStyle.Bold};}
        float w=Mathf.Min(570,width-40);
        GUI.Box(new Rect(20,82,w,Typing?152:102),GUIContent.none);
        GUI.Label(new Rect(34,91,w-28,26),(ControlBB8?"BB-8":"MINION")+"  /  "+(View.FirstPerson?"PRIMERA PERSONA":"TERCERA PERSONA"),title);
        GUI.Label(new Rect(34,122,w-28,46),Voice.Status,label);
        if(!Voice.Ready&&!Voice.Busy&&GUI.Button(new Rect(34,156,110,24),"Reintentar"))Voice.Retry();
        if(Typing){GUI.SetNextControlName("utterance");typed=GUI.TextField(new Rect(34,166,w-128,28),typed,600);GUI.FocusControl("utterance");
            if(GUI.Button(new Rect(w-84,166,90,28),"Enviar"))SubmitText();
        }
        if(Popup && Popup.Visible && !string.IsNullOrEmpty(Voice.Transcript)){GUI.Box(new Rect(width/2f-w/2,height-174,w,54),GUIContent.none);GUI.Label(new Rect(width/2f-w/2+12,height-168,w-24,46),"“"+Voice.Transcript+"”",label);}
        GUI.Label(new Rect(24,height-32,width-48,25),"TAB personaje   ·   V vista   ·   E mantener para hablar   ·   T escribir   ·   F3 diagnóstico",label);
        if(diagnostic){
            GUI.Box(new Rect(width-330,90,310,168),GUIContent.none);
            var r=Voice.LastResult;
            GUI.Label(new Rect(width-315,102,280,100),r==null?"Aún no hay una interpretación.":"Interpretación del texto (estimación)\n"+r.emotion+" / "+r.attitude+"\nIntensidad: "+r.intensity+" · "+r.seconds.ToString("0.0")+" s\nReacción: "+r.reaction,label);
            var devices=Microphone.devices;
            if(GUI.Button(new Rect(width-315,203,280,28),devices.Length==0?"Sin micrófono":"Mic: "+devices[Mathf.Clamp(Voice.MicrophoneIndex,0,devices.Length-1)])&&!Voice.Recording)Voice.MicrophoneIndex=(Voice.MicrophoneIndex+1)%Mathf.Max(1,devices.Length);
            for(int level=1;level<=3;level++){
                if(GUI.Button(new Rect(width-315+(level-1)*94,235,90,22),"Alegría "+level)&&!Voice.Busy&&!Brain.Busy)Brain.React("celebrate",level,"friendly");
            }
        }
        GUI.matrix=previousMatrix;
    }
    void SubmitText(){
        if(!Voice.Ready||Voice.Busy||Voice.Recording||string.IsNullOrWhiteSpace(typed))return;
        Voice.SendText(typed);typed="";Typing=false;GUI.FocusControl(null);
    }
    void OnDestroy(){if(Voice)Voice.Result-=React;foreach(var r in hidden)if(r)r.enabled=true;}
}
