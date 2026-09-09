using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;

public static class CityDistrictBuilder
{
    public static void ConfigureMenu(){
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/EarthLand.png");importer.isReadable=true;importer.sRGBTexture=false;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        foreach(bool earth in new[]{false,true}){
            var image=GalacticMenu.CreatePlanet(earth);string path="Assets/Resources/"+(earth?"EarthPreview":"KhepraPreview")+".png";File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);AssetDatabase.ImportAsset(path);
            var texture=(TextureImporter)AssetImporter.GetAtPath(path);texture.alphaIsTransparency=true;texture.mipmapEnabled=false;texture.textureCompression=TextureImporterCompression.Uncompressed;texture.SaveAndReimport();
        }
    }
    static Transform root;
    static Material pavement,road,stone,brick,cream,teal,glass,metal,wood,leaf,white,red,grass;
    public static void Build()
    {
        BB8Setup.OpenDemo();Directory.CreateDirectory("Assets/City/Materials");
        pavement=Mat("Pavimento",new Color(.55f,.55f,.51f));road=Mat("Asfalto",new Color(.14f,.16f,.18f));stone=Mat("Piedra",new Color(.68f,.66f,.60f));
        brick=Mat("Ladrillo",new Color(.51f,.29f,.22f));cream=Mat("Estuco",new Color(.77f,.71f,.59f));teal=Mat("Comercio",new Color(.16f,.31f,.32f));
        glass=Mat("Vidrio",new Color(.14f,.25f,.31f),.35f);metal=Mat("Metal",new Color(.10f,.13f,.15f),.25f);wood=Mat("Madera",new Color(.38f,.23f,.13f));
        leaf=Mat("Copa",new Color(.22f,.35f,.16f));grass=Mat("Cesped",new Color(.31f,.39f,.22f));white=Mat("Senalizacion",new Color(.92f,.9f,.8f));red=Mat("Rojo",new Color(.55f,.11f,.07f));
        root=new GameObject("Distrito Central").transform;
        Box("Suelo urbano",new Vector3(0,-.3f,0),new Vector3(100,.6f,100),pavement);
        foreach(float axis in new[]{-16f,18f}){
            Box("Calle norte-sur",new Vector3(axis,.012f,0),new Vector3(8,.024f,96),road,false);
            Box("Calle este-oeste",new Vector3(0,.015f,axis),new Vector3(96,.024f,8),road,false);
            for(int j=-43;j<45;j+=5)if(Mathf.Abs(j+16)>5&&Mathf.Abs(j-18)>5){
                Box("Linea vial",new Vector3(axis,.035f,j),new Vector3(.13f,.01f,2.2f),white,false);
                Box("Linea vial",new Vector3(j,.038f,axis),new Vector3(2.2f,.01f,.13f),white,false);
            }
        }
        Box("Plaza Central",new Vector3(1,.06f,1),new Vector3(25,.12f,25),stone);
        // Wide approaches are flush enough for both the capsule and rolling droid.
        for(int i=-10;i<=12;i+=2){
            Box("Junta de losa",new Vector3(i,.123f,1),new Vector3(.025f,.006f,25),pavement,false);
            Box("Junta de losa",new Vector3(1,.123f,i),new Vector3(25,.006f,.025f),pavement,false);
        }
        foreach(float a in new[]{-16f,18f})for(int i=0;i<7;i++){
            Box("Paso peatonal",new Vector3(a-3.2f+i*1.05f,.04f,0),new Vector3(.55f,.015f,3),white,false);
            Box("Paso peatonal",new Vector3(0,.043f,a-3.2f+i*1.05f),new Vector3(3,.015f,.55f),white,false);
        }
        // Buildings frame the plaza with shops at street level and homes above.
        string[] shops={"LIBRERIA","CAFE CENTRAL","ALMACEN","PANADERIA","FARMACIA"};
        for(int i=0;i<5;i++){
            Building(new Vector3(-22+i*12,0,30),0,8.5f,8,3+i%2,shops[i],i%2==0?brick:cream);
            Building(new Vector3(-22+i*12,0,-31),180,8.5f,8,2+i%3,shops[(i+2)%5],i%2==0?cream:brick);
        }
        for(int i=0;i<3;i++){
            Building(new Vector3(-32,0,-7+i*12),-90,8,9,3+i%2,shops[i+1],cream);
            Building(new Vector3(33,0,-7+i*12),90,8,9,3,shops[(i+3)%5],brick);
        }
        foreach(var p in new[]{new Vector3(-8,0,-8),new Vector3(10,0,-8),new Vector3(-8,0,10),new Vector3(10,0,10)}){
            Box("Jardinera",p+Vector3.up*.28f,new Vector3(3.5f,.32f,3.5f),pavement);
            Box("Tierra vegetal",p+Vector3.up*.455f,new Vector3(3.1f,.04f,3.1f),grass,false);Tree(p+Vector3.up*.4f);
        }
        Bench(new Vector3(-6,.12f,6),90);Bench(new Vector3(8,.12f,6),-90);
        Bench(new Vector3(-6,.12f,-4),90);Bench(new Vector3(8,.12f,-4),-90);
        foreach(var p in new[]{new Vector3(-10,0,-10),new Vector3(12,0,-10),new Vector3(-10,0,12),new Vector3(12,0,12),new Vector3(-23,0,8),new Vector3(25,0,8)})Lamp(p);
        Box("Mapa de barrio",new Vector3(-4,1.25f,9),new Vector3(2.5f,1.7f,.18f),teal);
        Sign("PLAZA\nCENTRAL",new Vector3(-4,1.5f,8.89f),Quaternion.identity,.08f);
        BusStop(new Vector3(-23,0,-4));
        ChargingPoint(new Vector3(-8,.12f,3));ChargingPoint(new Vector3(10,.12f,3));
        Car(new Vector3(-18.5f,0,8),0,teal);Car(new Vector3(20.5f,0,-7),180,red);Car(new Vector3(9,0,-18.5f),90,cream);
        for(int i=0;i<8;i++){
            var p=new Vector3(-10+i*3.15f,.4f,-11.5f);Cylinder("Bolardo",p,.10f,.8f,metal);
        }
        var distantGlass=Mat("Vidrio lejano",new Color(.42f,.50f,.55f),.2f);
        for(int i=0;i<12;i++){
            var p=new Vector3(-43+i*8,-.1f,46);float h=16+i%4*4;Box("Silueta urbana",p+Vector3.up*h/2,new Vector3(7,h,7),i%2==0?pavement:cream);
            for(int j=-2;j<=2;j++)Box("Cristal torre",p+new Vector3(j*1.2f,h/2,-3.54f),new Vector3(.9f,h-2,.08f),distantGlass,false);
            for(int floor=3;floor<h;floor+=3)Box("Forjado torre",p+new Vector3(0,floor,-3.61f),new Vector3(7,.12f,.12f),pavement,false);
        }
        foreach(var side in new[]{-1,1}){
            Box("Limite del barrio",new Vector3(side*49,1.2f,0),new Vector3(.4f,2.4f,100),pavement);
            Box("Limite del barrio",new Vector3(0,1.2f,side*49),new Vector3(100,2.4f,.4f),pavement);
        }
        PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Resources/CityDistrict.prefab");Object.DestroyImmediate(root.gameObject);
        var d=Object.FindFirstObjectByType<ConversationDirector>();var destinations=d.GetComponent<WorldDestinations>();if(!destinations)destinations=d.gameObject.AddComponent<WorldDestinations>();
        destinations.Desert=GameObject.Find("Desguace 08");destinations.Arrival=GameObject.Find("Zona de encuentro");destinations.DesertAir=GameObject.Find("Aire del desguace");
        EditorSceneManager.MarkSceneDirty(d.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("CITY_DISTRICT_OK");
    }
    static Material Mat(string name,Color color,float gloss=.12f){string path="Assets/City/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.enableInstancing=true;m.SetFloat("_Glossiness",gloss);EditorUtility.SetDirty(m);return m;}
    static GameObject Box(string name,Vector3 p,Vector3 scale,Material mat,bool solid=true){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;if(!solid)Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
    static GameObject Cylinder(string name,Vector3 p,float radius,float height,Material mat){var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localScale=new Vector3(radius*2,height/2,radius*2);go.GetComponent<Renderer>().sharedMaterial=mat;return go;}
    static void Sign(string text,Vector3 p,Quaternion rotation,float size){var go=new GameObject(text);go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localRotation=rotation;var t=go.AddComponent<TextMesh>();t.text=text;t.font=Resources.Load<Font>("Fonts/Rajdhani-Medium");t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=Color.white;go.GetComponent<Renderer>().sharedMaterial=t.font.material;}
    static void Building(Vector3 p,float yaw,float width,float depth,int floors,string shop,Material facade){
        var previous=root;root=new GameObject(shop+" · edificio").transform;root.SetParent(previous,false);root.localPosition=p;root.localRotation=Quaternion.Euler(0,yaw,0);float h=floors*3.2f;
        Box("Edificio",new Vector3(0,h/2,0),new Vector3(width,h,depth),facade);
        Box("Zocalo",new Vector3(0,.4f,0),new Vector3(width+.15f,.8f,depth+.15f),pavement);
        Box("Cornisa",new Vector3(0,h,0),new Vector3(width+.5f,.25f,depth+.5f),stone);
        for(int f=1;f<floors;f++){
            Box("Forjado",new Vector3(0,f*3.2f,0),new Vector3(width+.25f,.12f,depth+.25f),stone,false);
            for(int j=-1;j<=1;j++){
                var q=new Vector3(j*width*.29f,f*3.2f+1.4f,-depth/2-.04f);
                Box("Marco ventana",q,new Vector3(1.65f,1.85f,.16f),stone,false);Box("Ventana",q+Vector3.back*.10f,new Vector3(1.4f,1.6f,.06f),glass,false);
                Box("Parteluz",q+Vector3.back*.15f,new Vector3(.06f,1.65f,.06f),metal,false);
                Box("Alfeizar",q+new Vector3(0,-.95f,-.12f),new Vector3(1.85f,.12f,.35f),stone,false);
            }
            foreach(int side in new[]{-1,1})for(int j=-1;j<=1;j++)Box("Ventana lateral",new Vector3(side*(width/2+.03f),f*3.2f+1.4f,j*2.2f),new Vector3(.07f,1.6f,1.3f),glass,false);
        }
        Box("Escaparate",new Vector3(0,1.25f,-depth/2-.06f),new Vector3(width-.6f,2.3f,.08f),glass,false);
        for(int j=-1;j<=1;j++)Box("Marco comercio",new Vector3(j*width*.32f,1.2f,-depth/2-.14f),new Vector3(.1f,2.4f,.1f),metal,false);
        Box("Rotulo",new Vector3(0,2.7f,-depth/2-.16f),new Vector3(width,.62f,.22f),teal,false);
        Sign(shop,new Vector3(0,2.7f,-depth/2-.3f),Quaternion.identity,.055f);
        Box("Marquesina",new Vector3(0,3.1f,-depth/2-.6f),new Vector3(width+.2f,.12f,1.3f),teal,false);
        Box("Acera comercial",new Vector3(0,.06f,-depth/2-1.4f),new Vector3(width+.6f,.12f,2.8f),pavement);
        root=previous;
    }
    static void Tree(Vector3 p){Cylinder("Tronco",p+Vector3.up*1.5f,.15f,3,wood);for(int i=0;i<3;i++){var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name="Copa de arbol";go.transform.SetParent(root,false);go.transform.localPosition=p+new Vector3((i-1)*.65f,3.2f+(i%2)*.6f,0);go.transform.localScale=new Vector3(2.4f,2.5f,2.6f);go.GetComponent<Renderer>().sharedMaterial=leaf;Object.DestroyImmediate(go.GetComponent<Collider>());}}
    static void Bench(Vector3 p,float yaw){var previous=root;root=new GameObject("Banco").transform;root.SetParent(previous,false);root.localPosition=p;root.localRotation=Quaternion.Euler(0,yaw,0);for(int j=0;j<3;j++)Box("Tablon asiento",new Vector3(0,.46f,(j-1)*.16f),new Vector3(1.9f,.09f,.13f),wood);Box("Respaldo",new Vector3(0,.8f,.25f),new Vector3(1.9f,.55f,.1f),wood);foreach(float x in new[]{-.7f,.7f})Box("Pata",new Vector3(x,.23f,0),new Vector3(.10f,.46f,.5f),metal);root=previous;}
    static void Lamp(Vector3 p){Cylinder("Farola",p+Vector3.up*2.5f,.075f,5,metal);Box("Luminaria",p+new Vector3(0,5,.35f),new Vector3(.5f,.14f,1.2f),metal);Box("Difusor",p+new Vector3(0,4.91f,.35f),new Vector3(.4f,.03f,1),white,false);}
    static void BusStop(Vector3 p){Box("Parada techo",p+new Vector3(0,2.7f,0),new Vector3(2,.16f,4.8f),teal);foreach(float z in new[]{-2f,2f})Cylinder("Poste parada",p+new Vector3(-.7f,1.35f,z),.065f,2.7f,metal);Box("Panel parada",p+new Vector3(-.7f,1.45f,0),new Vector3(.1f,2.2f,4),glass);Bench(p+new Vector3(0,0,.5f),90);Box("Senal bus",p+new Vector3(1.3f,2.3f,-2.4f),new Vector3(.12f,.8f,.7f),teal);}
    static void ChargingPoint(Vector3 p){
        Box("Cargador urbano",p+Vector3.up*.4f,new Vector3(.48f,.8f,.48f),metal);
        var station=new GameObject("Punto de recarga BB8");station.transform.SetParent(root,false);station.transform.localPosition=p;
        var trigger=station.AddComponent<SphereCollider>();trigger.radius=1.2f;trigger.isTrigger=true;
        var visual=Box("Indicador de energia",p+Vector3.up*.95f,new Vector3(.18f,.26f,.18f),teal,false);visual.transform.SetParent(station.transform,true);
        var cell=station.AddComponent<EnergyCell>();cell.Visual=visual.transform;cell.GlowRenderers=new[]{visual.GetComponent<Renderer>()};
    }
    static void Car(Vector3 p,float yaw,Material paint){var previous=root;root=new GameObject("Auto estacionado").transform;root.SetParent(previous,false);root.localPosition=p;root.localRotation=Quaternion.Euler(0,yaw,0);
        Box("Carroceria",new Vector3(0,.55f,0),new Vector3(1.7f,.6f,3.8f),paint);Box("Cabina",new Vector3(0,1.05f,-.2f),new Vector3(1.45f,.6f,1.9f),glass);Box("Techo",new Vector3(0,1.37f,-.2f),new Vector3(1.5f,.09f,1.95f),paint);
        foreach(float x in new[]{-.84f,.84f})foreach(float z in new[]{-1.2f,1.2f}){var wheel=Cylinder("Rueda",new Vector3(x,.34f,z),.34f,.18f,metal);wheel.transform.localRotation=Quaternion.Euler(0,0,90);}
        foreach(float x in new[]{-.55f,.55f}){Box("Faro",new Vector3(x,.62f,1.91f),new Vector3(.35f,.18f,.035f),white,false);Box("Piloto",new Vector3(x,.62f,-1.91f),new Vector3(.35f,.18f,.035f),red,false);}root=previous;
    }
}
