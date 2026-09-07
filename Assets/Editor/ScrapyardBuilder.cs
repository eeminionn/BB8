using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class ScrapyardBuilder
{
    const string Root = "Assets/Scrapyard/";
    static Transform environment;
    static Material sand, hull, rust, dark, trim, teal, glow, amber;
    static int meshIndex;

    [MenuItem("BB8/Create Scrapyard Scene")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before rebuilding the scene.");
        AssetDatabase.Refresh();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        meshIndex = 0;
        environment = new GameObject("Desguace 08").transform;
        Materials();
        Lighting();
        Box("Arena", new Vector3(0,-0.35f,0), new Vector3(150,.7f,150), sand);
        // A clear arrival court frames the wreck, pipe passage and elevated route.
        Ship();
        Pipe("Conducto transitable", new Vector3(7.5f,1.65f,-1), 1.65f, 8f, 15f, rust);
        Pipe("Conducto secundario", new Vector3(17,1.1f,8), 1.1f, 5f, -28f, hull);
        Box("Plataforma de mantenimiento", new Vector3(.5f,2.2f,5.4f), new Vector3(3.6f,.28f,6f), dark);
        Ramp("Rampa de acceso", new Vector3(.5f,.05f,-6), new Vector3(.5f,2.32f,2.4f), 3.6f, hull);
        Box("Pasarela de salida", new Vector3(.5f,2.2f,12), new Vector3(3.6f,.28f,3f), hull);
        Ramp("Descenso", new Vector3(.5f,2.32f,13.5f), new Vector3(.5f,.05f,21), 3.6f, rust);
        for (int i = 0; i < 3; i++)
        {
            Box("Soporte de pasarela", new Vector3(.5f,1.05f,3.2f+i*2.4f), new Vector3(2.6f,2.1f,.25f), trim);
            Box("Borde ámbar", new Vector3(-1.17f,2.36f,3.2f+i*2.4f), new Vector3(.07f,.045f,1.4f), amber, false);
            Box("Borde ámbar", new Vector3(2.17f,2.36f,3.2f+i*2.4f), new Vector3(.07f,.045f,1.4f), amber, false);
        }
        // Small beacons identify the two lips; there is no race, score or objective.
        Beacon(new Vector3(-1.35f,2.3f,8.1f)); Beacon(new Vector3(2.35f,2.3f,10.6f));
        Wing("Ala enterrada", new Vector3(-13,.1f,-10), new Vector3(0,-32,7), 8,4,hull);
        Wing("Aleta partida", new Vector3(13,.1f,15), new Vector3(5,145,-9), 9,3,rust);
        Ramp("Placa caída", new Vector3(11,.08f,-10), new Vector3(14,1.4f,-6), 3f, rust);
        Container(new Vector3(16,0,-8), 18f, teal);
        Container(new Vector3(-17,0,15), -13f, hull);
        Container(new Vector3(-20,0,10), 80f, rust);
        // A few deliberately placed salvage piles, away from the traversable corridors.
        var random = new System.Random(808);
        Vector3[] piles = { new Vector3(-17,0,-4), new Vector3(15,0,3), new Vector3(-12,0,19), new Vector3(9,0,23) };
        foreach (var p in piles)
            for (int i=0;i<6;i++)
            {
                var pos=p+new Vector3((float)random.NextDouble()*4-2,.18f+(i%2)*.2f,(float)random.NextDouble()*3-1.5f);
                var piece=Box("Chatarra",pos,new Vector3(1.2f+(float)random.NextDouble(),.3f,.45f),i%2==0?rust:trim);
                piece.transform.rotation=Quaternion.Euler(i*6,random.Next(180),i*8);
            }
        // Silhouette boundary: sand banks and broken ribs rather than invisible walls.
        for(int i=0;i<22;i++)
        {
            float angle=i*Mathf.PI*2/22;
            var pos=new Vector3(Mathf.Cos(angle)*34,-1.7f,Mathf.Sin(angle)*34+4);
            var dune=Primitive("Banco de arena",PrimitiveType.Sphere,pos,new Vector3(12,4.5f+(i%3),9),sand);
            dune.transform.rotation=Quaternion.Euler(0,i*31,0);
        }
        for(int i=0;i<5;i++)
        {
            var rib=Box("Costilla de casco lejana",new Vector3(-25+i*2.4f,2.2f,23),new Vector3(.45f,7,.7f),rust);
            rib.transform.rotation=Quaternion.Euler(0,0,-22-i*6);
        }
        Crane(new Vector3(19,0,18));
        Sign("08",new Vector3(-5.2f,2.3f,-13.5f),new Vector3(0,15,0),1.2f);
        Beacon(new Vector3(-3,0,-13)); Beacon(new Vector3(3,0,-13));
        Energy(new Vector3(0,0,-10));
        Energy(new Vector3(7.5f,0,-1));
        Energy(new Vector3(.5f,2.35f,6.3f));
        Energy(new Vector3(-8,1.12f,7));
        Energy(new Vector3(8,0,16));
        Energy(new Vector3(-12,0,-6));
        Player();
        var air=new GameObject("Aire del desguace").AddComponent<AudioSource>();
        air.clip=Clip("yard-air"); air.loop=true; air.volume=.055f; air.spatialBlend=0; air.playOnAwake=true;
        QualitySettings.antiAliasing=4;
        QualitySettings.shadowDistance=65;
        QualitySettings.shadows=ShadowQuality.All;
        QualitySettings.shadowResolution=ShadowResolution.High;
        QualitySettings.vSyncCount=1;
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/scrapyard.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/scrapyard.unity",true)};
        AssetDatabase.SaveAssets();
        Debug.Log("SCRAPYARD_CREATED meshes="+meshIndex+" cells=6");
    }

    static void Materials()
    {
        sand=Mat("Arena cálida",new Color(.54f,.40f,.25f),0,.15f);
        hull=Mat("Cerámica envejecida",new Color(.58f,.57f,.49f),.25f,.28f);
        rust=Mat("Acero oxidado",new Color(.34f,.17f,.085f),.35f,.25f);
        dark=Mat("Acero oscuro",new Color(.09f,.13f,.14f),.45f,.35f);
        trim=Mat("Bordes mecanizados",new Color(.21f,.25f,.24f),.65f,.4f);
        teal=Mat("Pintura petróleo",new Color(.12f,.27f,.27f),.2f,.3f);
        glow=Mat("Energía cian",new Color(.15f,.72f,.85f),.25f,.5f,new Color(.12f,.9f,1f)*1.5f);
        amber=Mat("Baliza ámbar",new Color(.9f,.48f,.1f),.2f,.35f,new Color(1,.36f,.05f)*.8f);
        var texture=new Texture2D(128,128,TextureFormat.RGB24,false);
        for(int y=0;y<128;y++) for(int x=0;x<128;x++)
        {
            float n=Mathf.PerlinNoise(x*.11f,y*.11f)*.14f+Mathf.PerlinNoise(x*.7f,y*.7f)*.08f;
            texture.SetPixel(x,y,new Color(.79f+n,.79f+n,.79f+n));
        }
        texture.Apply();
        string path=Root+"Textures/surface-grain.png";
        File.WriteAllBytes(path,texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        var imported=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        foreach(var mat in new[]{sand,hull,rust,dark,trim,teal}) { mat.mainTexture=imported; mat.mainTextureScale=mat==sand?new Vector2(60,60):new Vector2(2,2); }
    }
    static Material Mat(string name,Color color,float metallic,float smooth,Color emission=default)
    {
        string path=Root+"Materials/"+name+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null) { mat=new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(mat,path); }
        mat.color=color; mat.SetFloat("_Metallic",metallic); mat.SetFloat("_Glossiness",smooth);
        if(emission.maxColorComponent>0) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor",emission); }
        return mat;
    }
    static void Lighting()
    {
        var sun=new GameObject("Sol bajo").AddComponent<Light>(); sun.type=LightType.Directional;
        sun.color=new Color(1f,.81f,.59f); sun.intensity=1.35f; sun.shadows=LightShadows.Soft;
        sun.shadowBias=.025f; sun.shadowNormalBias=.25f; sun.transform.rotation=Quaternion.Euler(32,-38,0);
        RenderSettings.sun=sun;
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.48f,.57f,.65f);
        RenderSettings.ambientEquatorColor=new Color(.42f,.40f,.34f);
        RenderSettings.ambientGroundColor=new Color(.24f,.19f,.14f);
        RenderSettings.fog=true; RenderSettings.fogMode=FogMode.ExponentialSquared;
        RenderSettings.fogDensity=.012f; RenderSettings.fogColor=new Color(.62f,.53f,.41f);
        var sky=new Material(Shader.Find("Skybox/Procedural"));
        sky.SetColor("_SkyTint",new Color(.44f,.5f,.55f)); sky.SetColor("_GroundColor",new Color(.5f,.39f,.27f));
        sky.SetFloat("_AtmosphereThickness",.8f); sky.SetFloat("_SunSize",.025f); sky.SetFloat("_Exposure",1.15f);
        SaveAsset(sky,Root+"Materials/Yard sky.mat"); RenderSettings.skybox=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Yard sky.mat");
        var cube=new Cubemap(32,TextureFormat.RGBA32,false);
        for(int face=0;face<6;face++)
        {
            var colors=new Color[32*32];
            for(int y=0;y<32;y++) for(int x=0;x<32;x++)
            {
                float u=(x+.5f)/16-1,v=(y+.5f)/16-1;
                Vector3 d=face==0?new Vector3(1,-v,-u):face==1?new Vector3(-1,-v,u):face==2?new Vector3(u,1,v):face==3?new Vector3(u,-1,-v):face==4?new Vector3(u,-v,1):new Vector3(-u,-v,-1);
                d.Normalize(); Color c=d.y>0?Color.Lerp(new Color(.65f,.57f,.43f),new Color(.38f,.51f,.65f),d.y):Color.Lerp(new Color(.65f,.57f,.43f),new Color(.23f,.16f,.10f),-d.y);
                c+=new Color(1f,.83f,.57f)*Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,-sun.transform.forward)),180f)*1.7f;
                colors[y*32+x]=c;
            }
            cube.SetPixels(colors,(CubemapFace)face);
        }
        cube.Apply(); SaveAsset(cube,Root+"Textures/Yard reflection.cubemap");
        RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;
        RenderSettings.customReflectionTexture=AssetDatabase.LoadAssetAtPath<Cubemap>(Root+"Textures/Yard reflection.cubemap");
        RenderSettings.reflectionIntensity=.8f;
    }
    static void Player()
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BB8/bb8.prefab");
        var root=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        root.name="BB8"; root.transform.position=new Vector3(0,.55f,-18);
        var controller=root.GetComponentInChildren<BbRigidbodyController>();
        controller.CruiseSpeed=7; controller.BoostSpeed=14; controller.JumpDelay=.65f;
        var camera=new GameObject("Main Camera").AddComponent<Camera>(); camera.tag="MainCamera";
        camera.nearClipPlane=.08f; camera.farClipPlane=180; camera.fieldOfView=59;
        camera.transform.position=controller.transform.position+new Vector3(0,2.8f,-5.8f);
        camera.transform.LookAt(controller.transform.position+Vector3.up*.48f);
        camera.gameObject.AddComponent<AudioListener>();
        var orbit=camera.gameObject.AddComponent<DragMouseOrbit>(); orbit.Target=controller.transform; orbit.Distance=6.2f;
        controller.Camera=camera.transform;
        controller.gameObject.AddComponent<BB8DemoHelp>();
        var personality=controller.gameObject.AddComponent<BB8Personality>();
        var head=controller.Head.transform;
        // Animate the render geometry independently of the stabilising rigidbody.
        var expression=new GameObject("Gesto de cabeza").transform; expression.SetParent(head,false);
        var mesh=expression.gameObject.AddComponent<MeshFilter>(); mesh.sharedMesh=head.GetComponent<MeshFilter>().sharedMesh;
        var renderer=expression.gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterials=head.GetComponent<MeshRenderer>().sharedMaterials;
        head.GetComponent<MeshRenderer>().enabled=false;
        var pivots=new List<Transform>();
        foreach(Transform antenna in head.Cast<Transform>().Where(t=>t.name.Contains("antenna")).ToArray())
        {
            var bounds=antenna.GetComponent<MeshFilter>().sharedMesh.bounds;
            var pivot=new GameObject(antenna.name+" pivot").transform; pivot.SetParent(expression,false);
            pivot.position=antenna.TransformPoint(new Vector3(bounds.center.x,bounds.center.y,bounds.min.z));
            pivot.rotation=antenna.rotation;
            antenna.SetParent(pivot,true); pivots.Add(pivot);
        }
        personality.HeadVisual=expression; personality.Antennas=pivots.ToArray();
        personality.JumpSounds=new[]{Clip("jump-01"),Clip("jump-02"),Clip("jump-03")};
        personality.BumpSounds=new[]{Clip("bump-01"),Clip("bump-02"),Clip("bump-03")};
        personality.BoostSound=Clip("boost"); personality.RechargeSound=Clip("recharge");
        // The original metallic paint reads correctly with an explicit environment reflection.
        foreach(var name in new[]{"body","head"})
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/BB8/Materials/"+name+".mat");
            mat.SetFloat("_Metallic",.42f); mat.SetFloat("_Glossiness",.38f);
        }
    }
    static AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/"+name+".wav");
    static GameObject Primitive(string name,PrimitiveType type,Vector3 p,Vector3 s,Material mat,bool collision=true)
    {
        var go=GameObject.CreatePrimitive(type); go.name=name; go.transform.SetParent(environment,false);
        go.transform.localPosition=p; go.transform.localScale=s; go.GetComponent<Renderer>().sharedMaterial=mat;
        if(!collision) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.isStatic=true; return go;
    }
    static GameObject Box(string n,Vector3 p,Vector3 s,Material m,bool c=true)=>Primitive(n,PrimitiveType.Cube,p,s,m,c);
    static void Ramp(string name,Vector3 a,Vector3 b,float width,Material mat)
    {
        var go=Box(name,(a+b)*.5f,new Vector3(width,.22f,Vector3.Distance(a,b)),mat);
        go.transform.rotation=Quaternion.FromToRotation(Vector3.forward,b-a);
        for(int i=0;i<6;i++)
        {
            var line=Box("Junta de panel",Vector3.Lerp(a,b,(i+.5f)/6)+Vector3.up*.12f,new Vector3(width,.025f,.045f),trim,false);
            line.transform.rotation=go.transform.rotation;
        }
    }
    static void Pipe(string name,Vector3 center,float radius,float length,float yaw,Material mat)
    {
        MeshObject(name,Tube(radius,.15f,length,0,360),center,Quaternion.Euler(0,yaw,0),mat);
        var rot=Quaternion.Euler(0,yaw,0);
        for(int i=0;i<4;i++)
            MeshObject("Abrazadera",Tube(radius+.09f,.13f,.14f,0,360),center+rot*new Vector3(0,0,-length/2+length*i/3),rot,trim,false);
    }
    static void Ship()
    {
        var center=new Vector3(-8,2.2f,4);
        var rotation=Quaternion.Euler(0,-12,0);
        MeshObject("Casco de carguero abierto",Tube(3,.18f,17,150,390),center,rotation,hull);
        for(int i=0;i<7;i++)
            MeshObject("Cuaderna de carguero",Tube(3.12f,.24f,.18f,150,390),center+rotation*new Vector3(0,0,-8.2f+i*2.65f),rotation,rust,false);
        var deck=Box("Suelo de bodega",new Vector3(-8,1,4),new Vector3(4.7f,.24f,16),dark); deck.transform.rotation=rotation;
        Ramp("Acceso a bodega",new Vector3(-6.4f,.05f,-9),new Vector3(-6.4f,1.12f,-4.5f),3.1f,hull);
        for(int side=-1;side<=1;side+=2)
        {
            Vector3 p=center+rotation*new Vector3(side*3.4f,-.3f,6.1f);
            var engine=MeshObject("Motor desmantelado",Tube(1.05f,.24f,3.6f,0,360),p,rotation,trim);
            MeshObject("Anillo de motor",Tube(1.16f,.17f,.25f,0,360),p+rotation*new Vector3(0,0,1.8f),rotation,rust,false);
            for(int i=0;i<8;i++)
            {
                float a=i*45*Mathf.Deg2Rad;
                var blade=Box("Álabe",p+rotation*new Vector3(Mathf.Cos(a)*.58f,Mathf.Sin(a)*.58f,1.4f),new Vector3(.55f,.12f,.25f),dark,false);
                blade.transform.rotation=rotation*Quaternion.Euler(0,0,i*45+28);
            }
        }
        Wing("Panel lateral del carguero",new Vector3(-12,.25f,3),new Vector3(0,-12,12),8,4,rust);
    }
    static void Wing(string name,Vector3 pos,Vector3 angles,float length,float width,Material mat)
    {
        Vector3[] shape={new Vector3(-width/2,0,-length/2),new Vector3(width/2,0,-length/2),new Vector3(width*.18f,0,length/2),new Vector3(-width*.18f,0,length/2)};
        var verts=new List<Vector3>(); var indices=new List<int>();
        void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d) { int i=verts.Count; verts.AddRange(new[]{a,b,c,d}); indices.AddRange(new[]{i,i+1,i+2,i,i+2,i+3}); }
        var h=Vector3.up*.22f;
        Quad(shape[3]+h,shape[2]+h,shape[1]+h,shape[0]+h); Quad(shape[0],shape[1],shape[2],shape[3]);
        for(int i=0;i<4;i++) Quad(shape[i],shape[i]+h,shape[(i+1)%4]+h,shape[(i+1)%4]);
        MeshObject(name,Mesh(verts,indices),pos,Quaternion.Euler(angles),mat);
    }
    static void Container(Vector3 pos,float yaw,Material mat)
    {
        var root=Box("Módulo de carga",pos+Vector3.up*1.05f,new Vector3(3,2.1f,4.7f),mat); root.transform.rotation=Quaternion.Euler(0,yaw,0);
        for(int i=0;i<6;i++) for(int side=-1;side<=1;side+=2)
        {
            var rib=Box("Nervadura",pos+root.transform.rotation*new Vector3(side*1.53f,1.05f,-2+i*.8f),new Vector3(.08f,2.15f,.09f),trim,false);
            rib.transform.rotation=root.transform.rotation;
        }
    }
    static void Crane(Vector3 pos)
    {
        Box("Base de grúa",pos+Vector3.up*.3f,new Vector3(3,.6f,3),dark);
        Box("Mástil de grúa",pos+Vector3.up*4,new Vector3(.65f,8,.65f),teal);
        var beam=Box("Brazo de grúa",pos+new Vector3(-3,7.8f,0),new Vector3(7,.6f,.6f),rust);
        var cable=Primitive("Cable",PrimitiveType.Cylinder,pos+new Vector3(-5.6f,5.3f,0),new Vector3(.055f,2.2f,.055f),dark,false);
        MeshObject("Gancho",Tube(.3f,.09f,.13f,20,290),pos+new Vector3(-5.6f,3.1f,0),Quaternion.identity,trim,false);
    }
    static void Beacon(Vector3 p)
    {
        Primitive("Baliza",PrimitiveType.Cylinder,p+Vector3.up*.36f,new Vector3(.16f,.36f,.16f),dark);
        Primitive("Luz de baliza",PrimitiveType.Cylinder,p+Vector3.up*.75f,new Vector3(.2f,.05f,.2f),amber,false);
    }
    static void Sign(string text,Vector3 p,Vector3 rotation,float size)
    {
        Box("Placa de sector",p,new Vector3(2.6f,1.5f,.15f),teal).transform.rotation=Quaternion.Euler(rotation);
        var go=new GameObject("Identificación "+text); go.transform.SetParent(environment);
        go.transform.position=p+Quaternion.Euler(rotation)*new Vector3(0,0,-.085f); go.transform.rotation=Quaternion.Euler(rotation);
        var label=go.AddComponent<TextMesh>(); label.text=text; label.fontSize=100; label.characterSize=.12f*size;
        label.anchor=TextAnchor.MiddleCenter; label.color=new Color(.9f,.78f,.55f);
        Box("Poste de sector",p+Vector3.down*1.2f,new Vector3(.16f,2.4f,.16f),rust);
    }
    static void Energy(Vector3 pos)
    {
        var root=new GameObject("Celda de energía"); root.transform.SetParent(environment); root.transform.position=pos;
        var trigger=root.AddComponent<SphereCollider>(); trigger.isTrigger=true; trigger.radius=.85f; trigger.center=Vector3.up*.65f;
        var cell=root.AddComponent<EnergyCell>();
        var visual=new GameObject("Núcleo").transform; visual.SetParent(root.transform,false); visual.localPosition=Vector3.up*.7f;
        var core=Primitive("Carga luminosa",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.23f,.23f,.23f),glow,false);
        core.isStatic=false; core.transform.SetParent(visual,false); core.transform.localPosition=Vector3.zero;
        for(int side=-1;side<=1;side+=2)
        {
            var cap=Primitive("Terminal",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.32f,.045f,.32f),trim,false);
            cap.isStatic=false; cap.transform.SetParent(visual,false); cap.transform.localPosition=new Vector3(0,side*.25f,0);
        }
        var baseplate=Primitive("Base de celda",PrimitiveType.Cylinder,pos+Vector3.up*.035f,new Vector3(.8f,.035f,.8f),dark,false);
        cell.Visual=visual; cell.GlowRenderers=new[]{core.GetComponent<Renderer>()};
    }
    static GameObject MeshObject(string name,Mesh mesh,Vector3 p,Quaternion r,Material material,bool collision=true)
    {
        string path=Root+"Meshes/part-"+(meshIndex++).ToString("D3")+".asset";
        SaveAsset(mesh,path); mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        var go=new GameObject(name); go.transform.SetParent(environment); go.transform.SetPositionAndRotation(p,r);
        go.AddComponent<MeshFilter>().sharedMesh=mesh; go.AddComponent<MeshRenderer>().sharedMaterial=material;
        if(collision) go.AddComponent<MeshCollider>().sharedMesh=mesh;
        go.isStatic=true; return go;
    }
    static Mesh Tube(float radius,float wall,float length,float start,float end)
    {
        int segments=Mathf.CeilToInt((end-start)/7.5f);
        var v=new List<Vector3>(); var t=new List<int>();
        void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d) { int i=v.Count; v.AddRange(new[]{a,b,c,d}); t.AddRange(new[]{i,i+1,i+2,i,i+2,i+3}); }
        Vector3 Point(float r,float a,float z)=>new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,z);
        for(int i=0;i<segments;i++)
        {
            float a=Mathf.Lerp(start,end,(float)i/segments)*Mathf.Deg2Rad,b=Mathf.Lerp(start,end,(float)(i+1)/segments)*Mathf.Deg2Rad;
            float z=length/2,inner=radius-wall;
            Quad(Point(radius,a,-z),Point(radius,b,-z),Point(radius,b,z),Point(radius,a,z));
            Quad(Point(inner,a,z),Point(inner,b,z),Point(inner,b,-z),Point(inner,a,-z));
            Quad(Point(inner,a,-z),Point(inner,b,-z),Point(radius,b,-z),Point(radius,a,-z));
            Quad(Point(radius,a,z),Point(radius,b,z),Point(inner,b,z),Point(inner,a,z));
        }
        return Mesh(v,t);
    }
    static Mesh Mesh(List<Vector3> v,List<int> t)
    {
        var mesh=new Mesh(); mesh.SetVertices(v); mesh.SetTriangles(t,0);
        mesh.SetUVs(0,v.Select(p=>new Vector2(p.z,p.y+p.x)).ToList()); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
    }
    static void SaveAsset(Object asset,string path)
    {
        var existing=AssetDatabase.LoadAssetAtPath<Object>(path);
        if(existing!=null) { EditorUtility.CopySerialized(asset,existing); Object.DestroyImmediate(asset); }
        else AssetDatabase.CreateAsset(asset,path);
    }
}
