using UnityEngine;

public sealed class BB8ReactionPopup : MonoBehaviour
{
    public float Affinity { get; private set; } = .5f;
    public bool Visible => Time.time < expires;
    public string Feeling { get; private set; }
    BB8SocialBrain brain;
    string words;
    float expires=-1, shown, displayed=.5f;
    GUIStyle speech, feeling, small;

    public void Initialize(BB8SocialBrain value)
    {
        if(brain) brain.ReactionStarted-=Show;
        brain=value;brain.ReactionStarted+=Show;
    }
    void Show(SocialReaction profile,int level,string attitude)
    {
        words=profile.Words(level);Feeling=profile.Feeling(level);
        float change=attitude=="hostile" || profile.Id=="retreat" ? -.16f*level
            : profile.Id=="celebrate" ? .12f*level
            : attitude=="friendly" || attitude=="seeking_comfort" ? .05f*level
            : profile.Id=="cautious" ? -.065f*level : 0;
        Affinity=Mathf.Clamp01(Affinity+change);
        shown=Time.time;expires=shown+brain.ReactionDuration+3f;
    }
    public void ShowCommand(string command)
    {
        words=command=="follow"?"Beep-boop!":"Boop-beep!";Feeling="Entendido";
        shown=Time.time;expires=shown+4f;
    }
    void Update(){displayed=Mathf.MoveTowards(displayed,Affinity,Time.deltaTime*.35f);}
    void OnGUI()
    {
        if(!Visible)return;
        float scale=Mathf.Max(.75f,Screen.height/900f), width=Screen.width/scale;
        var matrix=GUI.matrix;var color=GUI.color;GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
        if(speech==null){
            speech=new GUIStyle(GUI.skin.label){fontSize=23,fontStyle=FontStyle.Bold,wordWrap=true,normal={textColor=Color.white}};
            feeling=new GUIStyle(speech){fontSize=19};
            small=new GUIStyle(GUI.skin.label){fontSize=11,normal={textColor=new Color(.73f,.83f,.85f)}};
        }
        float alpha=Mathf.Min(Mathf.Clamp01((Time.time-shown)/.18f),Mathf.Clamp01((expires-Time.time)/.6f));
        float w=Mathf.Min(350,width-40),x=width-w-25,y=265;
        Fill(new Rect(x,y,w,170),new Color(.025f,.07f,.09f,.94f),alpha);
        Fill(new Rect(x,y,4,170),new Color(.35f,.87f,.94f),alpha);
        GUI.color=new Color(1,1,1,alpha);
        GUI.Label(new Rect(x+18,y+10,w-36,19),"BB-8",small);
        GUI.Label(new Rect(x+18,y+31,w-36,57),words,speech);
        GUI.Label(new Rect(x+18,y+91,w-36,27),Feeling,feeling);
        float barX=x+18,barY=y+126,barW=w-36;
        for(int i=0;i<48;i++){
            float u=i/47f;
            var c=u<.5f?Color.Lerp(new Color(.2f,.85f,.45f),new Color(1,.8f,.25f),u*2):Color.Lerp(new Color(1,.8f,.25f),new Color(.95f,.24f,.22f),(u-.5f)*2);
            Fill(new Rect(barX+barW*i/48f,barY,barW/48f+1,8),c,alpha);
        }
        Fill(new Rect(barX+(1-displayed)*(barW-4),barY-4,4,16),Color.white,alpha);
        GUI.color=new Color(1,1,1,alpha);
        GUI.Label(new Rect(barX,barY+16,110,18),"LE GUSTA",small);
        var previous=small.alignment;small.alignment=TextAnchor.UpperRight;
        GUI.Label(new Rect(barX+barW-130,barY+16,130,18),"NO LE GUSTA",small);small.alignment=previous;
        GUI.color=color;GUI.matrix=matrix;
    }
    static void Fill(Rect rect,Color color,float alpha){color.a*=alpha;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);}
    void OnDestroy(){if(brain)brain.ReactionStarted-=Show;}
}
