using UnityEngine;

public sealed class GalacticMenu
{
    static readonly Color Gold=new Color(.92f,.79f,.38f),Blue=new Color(.35f,.62f,.75f),Dim=new Color(.28f,.36f,.4f);
    GUIStyle logo,title,small,body,nav,eyebrow;
    Vector3[] stars;
    MenuPlanetRenderer planet;
    Texture planetPreview;
    double previewStarted;
    Texture2D dot;
    public void Tick(int destination){
        if(planet==null){planet=new MenuPlanetRenderer();previewStarted=Time.realtimeSinceStartupAsDouble;}
        // Blit before IMGUI starts collecting draw commands, never inside OnGUI.
        planetPreview=planet.Render(destination,Time.realtimeSinceStartupAsDouble-previewStarted);
    }
    void Init(){
        if(logo!=null)return;var heading=Resources.Load<Font>("Fonts/Anton-Regular");var text=Resources.Load<Font>("Fonts/Rajdhani-Medium");
        logo=new GUIStyle(GUI.skin.label){font=heading,fontSize=105,normal={textColor=Gold},padding=new RectOffset(),alignment=TextAnchor.UpperLeft};
        title=new GUIStyle(logo){fontSize=40};nav=new GUIStyle(logo){fontSize=27};
        body=new GUIStyle(GUI.skin.label){font=text,fontSize=20,wordWrap=true,normal={textColor=new Color(.78f,.82f,.83f)}};
        small=new GUIStyle(body){fontSize=16,normal={textColor=new Color(.53f,.64f,.69f)}};
        eyebrow=new GUIStyle(body){fontSize=17,normal={textColor=Gold}};
        var random=new System.Random(808);stars=new Vector3[360];for(int i=0;i<stars.Length;i++)stars[i]=new Vector3((float)random.NextDouble(),(float)random.NextDouble(),(float)random.NextDouble());
        dot=new Texture2D(16,16);for(int y=0;y<16;y++)for(int x=0;x<16;x++){float r=Vector2.Distance(new Vector2(x,y),new Vector2(7.5f,7.5f))/8;dot.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),3)));}dot.Apply();
    }
    public static Texture2D CreatePlanet(bool earth){
        const int size=384;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.wrapMode=TextureWrapMode.Clamp;
        var light=new Vector3(-.6f,.55f,.7f).normalized;
        var land=earth?Resources.Load<Texture2D>("EarthLand"):null;
        for(int y=0;y<size;y++)for(int x=0;x<size;x++){
            float u=(x+.5f)*2/size-1,v=(y+.5f)*2/size-1,r=u*u+v*v;
            if(r>1){texture.SetPixel(x,y,Color.clear);continue;}
            var n=new Vector3(u,v,Mathf.Sqrt(1-r));float noise=Mathf.PerlinNoise(u*3.8f+4,v*3.8f+8)*.65f+Mathf.PerlinNoise(u*10+9,v*10+3)*.35f;
            float lat=Mathf.Asin(v)*Mathf.Rad2Deg,lon=Mathf.Atan2(u,n.z)*Mathf.Rad2Deg+10;
            float continent=land?land.GetPixelBilinear((lon+180)/360,(lat+90)/180).r:0;
            Color soil=Color.Lerp(new Color(.16f,.32f,.14f),new Color(.65f,.52f,.30f),Mathf.Clamp01(1-Mathf.Abs(lat-23)/16));
            Color c=earth?Color.Lerp(new Color(.035f,.16f,.33f),soil*(.8f+noise*.4f),continent):Color.Lerp(new Color(.30f,.16f,.075f),new Color(.72f,.53f,.29f),noise);
            if(earth){float clouds=Mathf.Clamp01((Mathf.PerlinNoise(u*8+23,v*8+12)-.64f)*2);c=Color.Lerp(c,new Color(.83f,.86f,.84f),clouds);if(Mathf.Abs(v)>.96f)c=Color.Lerp(c,Color.white,.7f);}
            c*=.13f+.87f*Mathf.Max(0,Vector3.Dot(n,light));
            float rim=Mathf.Pow(1-n.z,4);c+= (earth?new Color(.12f,.42f,.7f):new Color(.6f,.38f,.15f))*rim*.6f;c.a=Mathf.Clamp01((1-r)*170);texture.SetPixel(x,y,c);
        }
        texture.Apply();return texture;
    }
    static void Line(Vector2 from,Vector2 to,Color color,float thickness=1){var old=GUI.matrix;var delta=to-from;GUI.matrix=old*Matrix4x4.TRS(new Vector3(from.x,from.y,0),Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg),Vector3.one);UITheme.Fill(new Rect(0,0,delta.magnitude,thickness),color);GUI.matrix=old;}
    static void Frame(Rect r,Color c){float cut=15;Line(new Vector2(r.x+cut,r.y),new Vector2(r.xMax,r.y),c);Line(new Vector2(r.xMax,r.y),new Vector2(r.xMax,r.yMax-cut),c);Line(new Vector2(r.xMax,r.yMax-cut),new Vector2(r.xMax-cut,r.yMax),c);Line(new Vector2(r.xMax-cut,r.yMax),new Vector2(r.x,r.yMax),c);Line(new Vector2(r.x,r.yMax),new Vector2(r.x,r.y+cut),c);Line(new Vector2(r.x,r.y+cut),new Vector2(r.x+cut,r.y),c);}
    void OutlineTitle(Rect rect,string text){var previous=logo.normal.textColor;logo.normal.textColor=Gold;foreach(var d in new[]{Vector2.left,Vector2.right,Vector2.up,Vector2.down})GUI.Label(new Rect(rect.position+d*2,rect.size),text,logo);logo.normal.textColor=new Color(.025f,.028f,.03f);GUI.Label(rect,text,logo);logo.normal.textColor=previous;}
    bool Action(Rect r,string label,bool selected=false){
        bool hover=r.Contains(Event.current.mousePosition);UITheme.Fill(r,selected?new Color(.18f,.15f,.065f,.95f):new Color(.025f,.055f,.073f,.94f));Frame(r,selected||hover?Gold:Dim);
        var style=new GUIStyle(body){alignment=TextAnchor.MiddleCenter,normal={textColor=selected||hover?Gold:new Color(.70f,.79f,.84f)}};
        if(GUI.Button(r,GUIContent.none,GUIStyle.none))return true;GUI.Label(r,label,style);return false;
    }
    public void Draw(ExperienceShell shell,ConversationDirector director){
        Init();UITheme.Init();var old=GUI.matrix;float scale=Mathf.Min(Screen.width/1280f,Screen.height/900f);var origin=new Vector2((Screen.width-1280*scale)/2,(Screen.height-900*scale)/2);
        UITheme.Fill(new Rect(0,0,Screen.width,Screen.height),new Color(.012f,.018f,.026f));GUI.matrix=Matrix4x4.TRS(origin,Quaternion.identity,Vector3.one*scale);
        var oldColor=GUI.color;
        if(Event.current.type==EventType.Repaint)foreach(var s in stars){float a=.18f+s.z*.5f;GUI.color=new Color(.7f,.82f,.91f,a);float size=2+s.z*5;GUI.DrawTexture(new Rect(s.x*1280,s.y*900,size,size),dot);}GUI.color=oldColor;
        // Angular hull frame, restrained scan lines and a large navigational globe.
        Frame(new Rect(35,30,1210,840),Dim);Line(new Vector2(60,78),new Vector2(1220,78),Dim);
        GUI.Label(new Rect(72,42,700,30),"B B – 8     /     A R C H I V O   D E   E N C U E N T R O S",eyebrow);
        GUI.Label(new Rect(986,43,220,26),"NAVEGACIÓN  /  02 DESTINOS",small);
        OutlineTitle(new Rect(78,104,480,145),"BB–8");
        GUI.Label(new Rect(82,252,490,65),"ENCUENTROS",title);
        GUI.Label(new Rect(84,323,460,28),"SELECCIONA TU DESTINO",eyebrow);
        for(int i=0;i<2;i++){
            var r=new Rect(82,365+i*98,440,82);bool chosen=shell.SelectedDestination==i,hover=r.Contains(Event.current.mousePosition);
            UITheme.Fill(r,new Color(.018f,.041f,.059f,.94f));Frame(r,chosen||hover?Gold:Dim);
            if(chosen)UITheme.Fill(new Rect(r.x+1,r.y+17,3,48),Gold);
            if(GUI.Button(r,GUIContent.none,GUIStyle.none))shell.SelectDestination(i);
            nav.normal.textColor=chosen?Gold:new Color(.76f,.82f,.85f);
            GUI.Label(new Rect(r.x+22,r.y+7,350,37),i==0?"KHEPRA":"TIERRA",nav);
            GUI.Label(new Rect(r.x+23,r.y+47,360,28),i==0?"PUESTO 08   /   FRONTERA DESÉRTICA":"DISTRITO CENTRAL   /   ENTORNO URBANO",small);
            GUI.Label(new Rect(r.x+388,r.y+23,35,30),chosen?"◁":"",eyebrow);
        }
        int selected=shell.SelectedDestination;var center=new Vector2(905,320);float radius=198;
        for(int i=0;i<100;i++){
            float angle=i*Mathf.PI*2/100;var d=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
            Line(center+d*(radius+18),center+d*(radius+(i%5==0?27:21)),i%5==0?Blue:Dim);
        }
        if(Event.current.type==EventType.Repaint&&planetPreview)GUI.DrawTexture(new Rect(center.x-radius,center.y-radius,radius*2,radius*2),planetPreview);
        Line(new Vector2(670,566),new Vector2(1160,566),Dim);
        title.normal.textColor=Gold;GUI.Label(new Rect(680,577,480,55),selected==0?"KHEPRA":"TIERRA",title);
        GUI.Label(new Rect(680,638,470,66),selected==0?"Un puesto de recuperación entre dunas,\nrestos de naves y rutas de chatarra.":"Planeta Tierra.",body);
        string action=shell.HasEntered&&WorldDestinations.Instance&&selected==WorldDestinations.Instance.ActiveIndex?"CONTINUAR ENCUENTRO":"VIAJAR A "+(selected==0?"KHEPRA":"LA TIERRA");
        if(Action(new Rect(82,578,440,55),action,true))shell.Enter();
        if(Action(new Rect(82,652,212,40),shell.Muted?"SONIDO: APAGADO":"SONIDO: ENCENDIDO"))shell.ToggleSound();
        if(Action(new Rect(308,652,214,40),director.View.AutoFollow?"CÁMARA: AUTO":"CÁMARA: MANUAL"))director.View.AutoFollow=!director.View.AutoFollow;
        if(!string.IsNullOrEmpty(shell.TravelStatus))GUI.Label(new Rect(82,706,540,28),shell.TravelStatus,small);
        Line(new Vector2(72,754),new Vector2(1208,754),Dim);
        GUI.Label(new Rect(82,770,365,57),"EXPLORAR\nW/S avanzar · A/D girar · Espacio saltar",small);
        GUI.Label(new Rect(473,770,390,57),"CONVERSAR\nE hablar · T escribir · Enter enviar",small);
        GUI.Label(new Rect(886,770,315,57),"OBSERVAR\nTab personaje · V vista · H saludo",small);
        GUI.Label(new Rect(82,836,820,27),"← →  Destino     /     ENTER  Viajar     /     ESC  Volver     /     Mouse derecho  Mirar",small);
        GUI.Label(new Rect(1005,836,220,27),"VOZ LOCAL  ·  SIN COSTO",small);
        GUI.matrix=old;
    }
    public void Dispose(){planet?.Dispose();if(dot)Object.Destroy(dot);}
}
