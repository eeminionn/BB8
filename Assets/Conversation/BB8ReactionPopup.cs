using UnityEngine;

public sealed class BB8ReactionPopup : MonoBehaviour
{
    public float Affinity { get; private set; } = .5f;
    public bool Visible => Time.time < expires;
    public string Feeling { get; private set; }
    BB8SocialBrain brain;
    float expires=-1, shown, displayed=.5f;
    Transform bubble, marker;
    TextMesh caption;
    Material panelMaterial, barMaterial, markerMaterial, textMaterial;
    Texture2D gradient;

    public void Initialize(BB8SocialBrain value)
    {
        if(brain) brain.ReactionStarted-=Show;
        brain=value;brain.ReactionStarted+=Show;
    }
    void Show(SocialReaction profile,int level,string attitude)
    {
        Feeling=profile.Feeling(level);
        float change=attitude=="hostile" || profile.Id=="retreat" ? -.16f*level
            : profile.Id=="celebrate" ? .12f*level
            : attitude=="friendly" || attitude=="seeking_comfort" ? .05f*level
            : profile.Id=="cautious" ? -.065f*level : 0;
        Affinity=Mathf.Clamp01(Affinity+change);
        shown=Time.time;expires=shown+brain.ReactionDuration+3f;
    }
    public void ShowCommand(string command)
    {
        Feeling="Entendido";
        shown=Time.time;expires=shown+4f;
    }
    void Start()
    {
        bubble=new GameObject("BB8 · emoción").transform;
        bubble.gameObject.layer=8;
        var shader=Resources.Load<Shader>("BB8WorldPopup");
        panelMaterial=new Material(shader){color=new Color(.025f,.07f,.09f,.92f)};
        barMaterial=new Material(shader);
        markerMaterial=new Material(shader){color=Color.white};
        gradient=new Texture2D(48,1){wrapMode=TextureWrapMode.Clamp};
        for(int i=0;i<48;i++){
            float u=i/47f;
            gradient.SetPixel(i,0,u<.5f?Color.Lerp(new Color(.2f,.85f,.45f),new Color(1,.8f,.25f),u*2):Color.Lerp(new Color(1,.8f,.25f),new Color(.95f,.24f,.22f),(u-.5f)*2));
        }
        gradient.Apply();barMaterial.mainTexture=gradient;
        Quad("Panel",Vector3.zero,new Vector3(1.55f,.49f,1),panelMaterial);
        Quad("Afinidad",new Vector3(0,-.135f,-.015f),new Vector3(1.27f,.045f,1),barMaterial);
        marker=Quad("Indicador",new Vector3(0,-.135f,-.025f),new Vector3(.025f,.095f,1),markerMaterial);
        caption=new GameObject("Emoción").AddComponent<TextMesh>();
        caption.transform.SetParent(bubble,false);caption.transform.localPosition=new Vector3(0,.055f,-.02f);
        caption.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        caption.fontSize=64;caption.characterSize=.026f;caption.anchor=TextAnchor.MiddleCenter;
        caption.alignment=TextAlignment.Center;caption.color=Color.white;
        textMaterial=new Material(shader){mainTexture=caption.font.material.mainTexture};
        textMaterial.SetFloat("_Font",1);
        caption.GetComponent<MeshRenderer>().sharedMaterial=textMaterial;
        bubble.gameObject.SetActive(false);
    }
    Transform Quad(string title,Vector3 position,Vector3 scale,Material material)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(go.GetComponent<Collider>());
        go.name=title;go.layer=8;go.transform.SetParent(bubble,false);
        go.transform.localPosition=position;go.transform.localScale=scale;
        go.GetComponent<MeshRenderer>().sharedMaterial=material;
        return go.transform;
    }
    void LateUpdate()
    {
        displayed=Mathf.MoveTowards(displayed,Affinity,Time.deltaTime*.35f);
        if(!bubble)return;
        var camera=Camera.main;
        bool show=Visible&&brain&&camera&&!(ExperienceShell.Instance&&ExperienceShell.Instance.MenuOpen);
        if(show){
            var orbit=camera.GetComponent<DragMouseOrbit>();
            show=!(orbit&&orbit.FirstPerson&&orbit.Target==brain.transform);
        }
        bubble.gameObject.SetActive(show);
        if(!show)return;
        bubble.position=brain.transform.position+Vector3.up*1.45f;
        bubble.localScale=Vector3.one*.72f;
        bubble.rotation=camera.transform.rotation;
        caption.text=Feeling;
        marker.localPosition=new Vector3((.5f-displayed)*1.245f,-.135f,-.025f);
        float alpha=Mathf.Min(Mathf.Clamp01((Time.time-shown)/.18f),Mathf.Clamp01((expires-Time.time)/.6f));
        panelMaterial.color=new Color(.025f,.07f,.09f,.92f*alpha);
        barMaterial.color=markerMaterial.color=textMaterial.color=new Color(1,1,1,alpha);
    }
    void OnDestroy(){
        if(brain)brain.ReactionStarted-=Show;
        if(bubble)Destroy(bubble.gameObject);
        Destroy(panelMaterial);Destroy(barMaterial);Destroy(markerMaterial);Destroy(textMaterial);Destroy(gradient);
    }
}
