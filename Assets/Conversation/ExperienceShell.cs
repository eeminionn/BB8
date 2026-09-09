using UnityEngine;

[DefaultExecutionOrder(-300)]
public sealed class ExperienceShell:MonoBehaviour
{
    public static ExperienceShell Instance {get;private set;}
    public bool MenuOpen {get;private set;}
    public bool Muted {get;private set;}=true;
    public bool HasEntered {get;private set;}
    ConversationDirector director;
    GalacticMenu menu=new GalacticMenu();
    public int SelectedDestination {get;private set;}
    public string TravelStatus {get;private set;}="";
    public void SelectDestination(int index){SelectedDestination=Mathf.Clamp(index,0,1);TravelStatus="";}
    public void ToggleSound(){Muted=!Muted;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void QuietStart(){AudioListener.volume=0;AudioListener.pause=true;}
    void Awake(){Instance=this;director=GetComponent<ConversationDirector>();MenuOpen=!Application.isBatchMode;Time.timeScale=MenuOpen?0:1;}
    void Update(){if(MenuOpen)menu.Tick(SelectedDestination);}
    void LateUpdate(){AudioListener.pause=Muted||MenuOpen;AudioListener.volume=Muted||MenuOpen||director.Voice.Recording?0:1;}
    public void SetMenu(bool open){MenuOpen=open;if(WorldDestinations.Instance)SelectedDestination=WorldDestinations.Instance.ActiveIndex;TravelStatus="";if(!open)HasEntered=true;Time.timeScale=open?0:1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
    public void Enter(){
        if(WorldDestinations.Instance&&!WorldDestinations.Instance.Travel(SelectedDestination)){TravelStatus="Espera a que termine la respuesta antes de viajar.";return;}
        HasEntered=true;SetMenu(false);
    }
    void OnGUI(){
        if(Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.Escape&&!director.Typing){SetMenu(!MenuOpen);Event.current.Use();}
        if(!MenuOpen)return;
        if(Event.current.type==EventType.KeyDown){
            if(Event.current.keyCode==KeyCode.LeftArrow||Event.current.keyCode==KeyCode.RightArrow){SelectDestination(1-SelectedDestination);Event.current.Use();}
            else if(Event.current.keyCode==KeyCode.Return||Event.current.keyCode==KeyCode.KeypadEnter){Enter();Event.current.Use();return;}
        }
        menu.Draw(this,director);
    }
    void OnDestroy(){menu.Dispose();if(Instance==this)Instance=null;Time.timeScale=1;}
}

public static class UITheme
{
    public static readonly Color Accent=new Color(.92f,.79f,.38f), Ink=new Color(.9f,.94f,.93f), Muted=new Color(.64f,.73f,.74f), Line=new Color(.22f,.32f,.34f);
    public static GUIStyle Hero,Body,Small,Kicker,Button,Primary,Field;
    static Texture2D button,hover,active,field;
    static Texture2D Texture(Color c){var t=new Texture2D(1,1);t.SetPixel(0,0,c);t.Apply();return t;}
    public static void Init(){if(Body!=null)return;
        Body=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("Fonts/Rajdhani-Medium"),fontSize=17,wordWrap=true,normal={textColor=Ink}};
        Small=new GUIStyle(Body){fontSize=14,normal={textColor=Muted}};
        Kicker=new GUIStyle(Body){fontSize=15,fontStyle=FontStyle.Bold,normal={textColor=Accent}};
        Hero=new GUIStyle(Body){fontSize=40,fontStyle=FontStyle.Bold};
        button=Texture(new Color(.12f,.21f,.24f));hover=Texture(new Color(.19f,.32f,.35f));active=Texture(new Color(.3f,.54f,.55f));field=Texture(new Color(.025f,.052f,.066f));
        Button=new GUIStyle(GUI.skin.button){fontSize=13,fontStyle=FontStyle.Bold,border=new RectOffset(),normal={background=button,textColor=Ink},hover={background=hover,textColor=Color.white},active={background=active,textColor=Color.white},focused={background=hover,textColor=Color.white}};
        Primary=new GUIStyle(Button){fontSize=15,normal={background=active,textColor=Color.white}};
        Field=new GUIStyle(GUI.skin.textField){fontSize=17,padding=new RectOffset(13,13,10,10),normal={background=field,textColor=Ink},focused={background=field,textColor=Color.white}};
    }
    public static void Fill(Rect r,Color c){if(Event.current.type!=EventType.Repaint)return;var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
    public static void Panel(Rect r){Fill(r,new Color(.028f,.062f,.077f,.94f));}
}
