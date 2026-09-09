using UnityEngine;

public sealed class WorldDestinations:MonoBehaviour
{
    public static WorldDestinations Instance {get;private set;}
    public GameObject Desert,Arrival,DesertAir;
    public int ActiveIndex {get;private set;}
    public string AreaLabel => ActiveIndex==0?"KHEPRA / PUESTO 08":"TIERRA / DISTRITO CENTRAL";
    public GameObject City {get;private set;}
    ConversationDirector director;
    Color sky,equator,ground,fog,sunColor;float fogDensity,sunIntensity,reflection;
    void Awake(){Instance=this;director=GetComponent<ConversationDirector>();sky=RenderSettings.ambientSkyColor;equator=RenderSettings.ambientEquatorColor;ground=RenderSettings.ambientGroundColor;fog=RenderSettings.fogColor;fogDensity=RenderSettings.fogDensity;reflection=RenderSettings.reflectionIntensity;if(RenderSettings.sun){sunColor=RenderSettings.sun.color;sunIntensity=RenderSettings.sun.intensity;}}
    public bool Travel(int destination)
    {
        if(destination<0||destination>1)return false;
        if(destination==ActiveIndex)return true;
        if(director.Voice.Recording||(director.Voice.Ready&&director.Voice.Busy))return false;
        if(destination==1&&!City){var prefab=Resources.Load<GameObject>("CityDistrict");if(!prefab)return false;City=Instantiate(prefab);City.name="Tierra — Distrito Central";}
        director.Brain.CancelInteraction();director.Popup.Hide();
        Desert.SetActive(destination==0);if(Arrival)Arrival.SetActive(destination==0);if(DesertAir)DesertAir.SetActive(destination==0);if(City)City.SetActive(destination==1);
        ActiveIndex=destination;
        var bb=director.BB8;var body=bb.GetComponent<Rigidbody>();
        body.position=destination==0?new Vector3(0,.55f,-15):new Vector3(0,.70f,1);
        body.linearVelocity=body.angularVelocity=Vector3.zero;bb.Head.position=body.position;bb.Head.linearVelocity=bb.Head.angularVelocity=Vector3.zero;
        bb.ResetHeading(Vector3.forward);
        director.Minion.Reposition(destination==0?new Vector3(2.6f,.08f,-19):new Vector3(2.6f,.22f,-3),-20);
        director.View.ResetFraming();
        RenderSettings.ambientSkyColor=destination==0?sky:new Color(.54f,.62f,.69f);
        RenderSettings.ambientEquatorColor=destination==0?equator:new Color(.43f,.45f,.45f);
        RenderSettings.ambientGroundColor=destination==0?ground:new Color(.25f,.26f,.26f);
        RenderSettings.fogColor=destination==0?fog:new Color(.66f,.74f,.79f);RenderSettings.fogDensity=destination==0?fogDensity:.007f;
        RenderSettings.reflectionIntensity=destination==0?reflection:.25f;
        if(RenderSettings.sun){RenderSettings.sun.color=destination==0?sunColor:new Color(1,.97f,.90f);RenderSettings.sun.intensity=destination==0?sunIntensity:1.1f;}
        Physics.SyncTransforms();return true;
    }
    void OnDestroy(){if(Instance==this)Instance=null;}
}
