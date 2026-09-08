using UnityEngine;

[DefaultExecutionOrder(-300)]
public sealed class ExperienceShell:MonoBehaviour
{
    public static ExperienceShell Instance {get;private set;}
    public bool MenuOpen {get;private set;}
    public bool Muted {get;private set;}=true;
    public bool HasEntered {get;private set;}
    ConversationDirector director;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void QuietStart(){AudioListener.volume=0;AudioListener.pause=true;}
    void Awake(){Instance=this;director=GetComponent<ConversationDirector>();MenuOpen=!Application.isBatchMode;Time.timeScale=MenuOpen?0:1;}
    void LateUpdate(){AudioListener.pause=Muted||MenuOpen;AudioListener.volume=Muted||MenuOpen||director.Voice.Recording?0:1;}
    public void SetMenu(bool open){MenuOpen=open;if(!open)HasEntered=true;Time.timeScale=open?0:1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
    public void Enter(){HasEntered=true;SetMenu(false);}
    void OnGUI(){
        if(Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.Escape&&!director.Typing){SetMenu(!MenuOpen);Event.current.Use();}
        if(!MenuOpen)return;
        if(Event.current.type==EventType.KeyDown&&(Event.current.keyCode==KeyCode.Return||Event.current.keyCode==KeyCode.KeypadEnter)){Enter();Event.current.Use();return;}
        UITheme.Init();var previous=GUI.matrix;float scale=Mathf.Max(.65f,Mathf.Min(Screen.height/900f,Screen.width/1280f));GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
        float width=Screen.width/scale,height=Screen.height/scale;UITheme.Fill(new Rect(0,0,width,height),new Color(.018f,.033f,.043f,.84f));
        float x=width/2-350,y=height/2-300;
        UITheme.Panel(new Rect(x,y,700,600));UITheme.Fill(new Rect(x,y,700,3),UITheme.Accent);
        GUI.Label(new Rect(x+40,y+32,620,24),"BB–8    /    DESGUACE 08",UITheme.Kicker);
        GUI.Label(new Rect(x+40,y+84,620,100),HasEntered?"Un momento de pausa.":"Un encuentro\ncon BB–8.",UITheme.Hero);
        GUI.Label(new Rect(x+40,y+198,600,55),"Explora con el Minion, habla con BB–8 y descubre cómo responde a tus palabras.",UITheme.Body);
        GUI.Label(new Rect(x+40,y+246,620,22),"Sobre BB–8: emoción + afinidad. Verde = agrado · Rojo = desagrado.",UITheme.Small);
        UITheme.Fill(new Rect(x+40,y+272,620,1),UITheme.Line);
        GUI.Label(new Rect(x+40,y+291,185,25),"01   EXPLORA",UITheme.Kicker);
        GUI.Label(new Rect(x+40,y+326,180,88),"W / S  avanzar\nA / D  girar\nEspacio  saltar\nH  saludar (Minion)",UITheme.Body);
        GUI.Label(new Rect(x+255,y+291,185,25),"02   CONVERSA",UITheme.Kicker);
        GUI.Label(new Rect(x+255,y+326,185,73),"E  mantener para hablar\nT  escribir · Enter  enviar\nEsc  cerrar texto",UITheme.Body);
        GUI.Label(new Rect(x+470,y+291,200,25),"03   OBSERVA",UITheme.Kicker);
        GUI.Label(new Rect(x+470,y+326,190,73),"Tab  cambiar personaje\nV  primera / tercera persona\nMouse derecho  mirar",UITheme.Body);
        if(GUI.Button(new Rect(x+40,y+422,295,38),Muted?"SONIDO   DESACTIVADO":"SONIDO   ACTIVADO",UITheme.Button))Muted=!Muted;
        if(GUI.Button(new Rect(x+350,y+422,310,38),director.View.AutoFollow?"CÁMARA   SIGUE EL GIRO":"CÁMARA   GIRO MANUAL",UITheme.Button))director.View.AutoFollow=!director.View.AutoFollow;
        if(GUI.Button(new Rect(x+40,y+485,620,50),HasEntered?"VOLVER AL DESGUACE   →":"ENTRAR AL DESGUACE   →",UITheme.Primary))Enter();
        GUI.Label(new Rect(x+40,y+550,620,24),"Enter para entrar · Esc para pausar · Voz procesada en este computador",UITheme.Small);
        GUI.matrix=previous;
    }
    void OnDestroy(){if(Instance==this)Instance=null;Time.timeScale=1;}
}

public static class UITheme
{
    public static readonly Color Accent=new Color(.4f,.84f,.82f), Ink=new Color(.9f,.94f,.93f), Muted=new Color(.64f,.73f,.74f), Line=new Color(.22f,.32f,.34f);
    public static GUIStyle Hero,Body,Small,Kicker,Button,Primary,Field;
    static Texture2D button,hover,active,field;
    static Texture2D Texture(Color c){var t=new Texture2D(1,1);t.SetPixel(0,0,c);t.Apply();return t;}
    public static void Init(){if(Body!=null)return;
        Body=new GUIStyle(GUI.skin.label){fontSize=15,wordWrap=true,normal={textColor=Ink}};
        Small=new GUIStyle(Body){fontSize=12,normal={textColor=Muted}};
        Kicker=new GUIStyle(Body){fontSize=13,fontStyle=FontStyle.Bold,normal={textColor=Accent}};
        Hero=new GUIStyle(Body){fontSize=40,fontStyle=FontStyle.Bold};
        button=Texture(new Color(.12f,.21f,.24f));hover=Texture(new Color(.19f,.32f,.35f));active=Texture(new Color(.3f,.54f,.55f));field=Texture(new Color(.025f,.052f,.066f));
        Button=new GUIStyle(GUI.skin.button){fontSize=13,fontStyle=FontStyle.Bold,border=new RectOffset(),normal={background=button,textColor=Ink},hover={background=hover,textColor=Color.white},active={background=active,textColor=Color.white},focused={background=hover,textColor=Color.white}};
        Primary=new GUIStyle(Button){fontSize=15,normal={background=active,textColor=Color.white}};
        Field=new GUIStyle(GUI.skin.textField){fontSize=17,padding=new RectOffset(13,13,10,10),normal={background=field,textColor=Ink},focused={background=field,textColor=Color.white}};
    }
    public static void Fill(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
    public static void Panel(Rect r){Fill(r,new Color(.028f,.062f,.077f,.94f));}
}
