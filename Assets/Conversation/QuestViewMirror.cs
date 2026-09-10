using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

// Local spectator output of our own scene, not an OS screen capture. No audio.
public sealed class QuestViewMirror : MonoBehaviour
{
    [System.Serializable] class Viewer { public bool active; }
    Camera source, spectator;
    RenderTexture target;
    Texture2D pixels;
    string token;
    bool active;
    const string Url="http://127.0.0.1:8769";
    IEnumerator Start()
    {
        source=GetComponent<Camera>();
        spectator=new GameObject("Quest · USB spectator camera").AddComponent<Camera>();
        spectator.CopyFrom(source);spectator.enabled=false;spectator.stereoTargetEye=StereoTargetEyeMask.None;
        target=new RenderTexture(640,704,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){antiAliasing=1};target.Create();
        pixels=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
        var raw=new byte[target.width*target.height*4];
        spectator.targetTexture=target;
        float nextPoll=0;
        while(true){
            if(Time.realtimeSinceStartup>=nextPoll){
                nextPoll=Time.realtimeSinceStartup+1;
                var path=Path.Combine(Application.persistentDataPath,"quest-voice-token");
                if(token==null&&File.Exists(path))token=File.ReadAllText(path).Trim();
                using(var status=UnityWebRequest.Get(Url+"/status")){
                    status.timeout=1;yield return status.SendWebRequest();
                    active=status.result==UnityWebRequest.Result.Success&&JsonUtility.FromJson<Viewer>(status.downloadHandler.text).active;
                }
            }
            if(!active||string.IsNullOrEmpty(token)){yield return new WaitForSecondsRealtime(.5f);continue;}
            float started=Time.realtimeSinceStartup;
            yield return new WaitForEndOfFrame();
            // Reuse the left-eye view/projection: tracked head movement, rig turning,
            // first/third person, world menu, controllers and HUD all match the visor.
            Matrix4x4 view=source.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
            var world=view.inverse;
            spectator.transform.SetPositionAndRotation(world.GetColumn(3),Quaternion.LookRotation(-world.GetColumn(2),world.GetColumn(1)));
            spectator.worldToCameraMatrix=view;spectator.projectionMatrix=source.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            spectator.cullingMask=source.cullingMask;
            spectator.Render();
            if(SystemInfo.supportsAsyncGPUReadback){
                var readback=AsyncGPUReadback.Request(target,0,TextureFormat.RGBA32);
                while(!readback.done)yield return null;
                if(readback.hasError){yield return new WaitForSecondsRealtime(1);continue;}
                readback.GetData<byte>().CopyTo(raw);
            }else{
                var previous=RenderTexture.active;RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);RenderTexture.active=previous;
                pixels.GetRawTextureData<byte>().CopyTo(raw);
            }
            // Encoding must not block a headset frame. Only one job is ever in flight;
            // the reusable raw buffer is untouched until that job completes.
            var encode=Task.Run(()=>ImageConversion.EncodeArrayToJPG(raw,GraphicsFormat.R8G8B8A8_UNorm,640,704,0,75));
            while(!encode.IsCompleted)yield return null;
            if(encode.IsFaulted){active=false;yield return new WaitForSecondsRealtime(2);continue;}
            byte[] jpeg=encode.Result;
            using(var upload=new UnityWebRequest(Url+"/frame","POST")){
                upload.uploadHandler=new UploadHandlerRaw(jpeg);upload.downloadHandler=new DownloadHandlerBuffer();upload.timeout=2;
                upload.SetRequestHeader("Content-Type","image/jpeg");upload.SetRequestHeader("X-BB8-Token",token);
                yield return upload.SendWebRequest();
                if(upload.result!=UnityWebRequest.Result.Success)active=false;
            }
            yield return new WaitForSecondsRealtime(Mathf.Max(.01f,.1f-(Time.realtimeSinceStartup-started)));
        }
    }
    void OnDestroy(){if(spectator)Destroy(spectator.gameObject);if(target){target.Release();Destroy(target);}if(pixels)Destroy(pixels);}
}
