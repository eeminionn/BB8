using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable] public sealed class VoiceResult { public string emotion, attitude, reaction, text, error, command; public int intensity; public float seconds; }
public sealed class LocalVoiceClient : MonoBehaviour
{
    [Serializable] class Health { public string service, error; public bool ready; }
    [Serializable] class TextRequest { public string text; public string[] history; }
    public bool Ready { get; private set; }
    public bool Busy { get; private set; }
    public bool Recording { get; private set; }
    public float MicrophoneLevel { get; private set; }
    public string Status { get; private set; } = "Preparando voz local…";
    public string Transcript { get; private set; } = "";
    public VoiceResult LastResult { get; private set; }
    public event Action<VoiceResult> Result;
    public int MicrophoneIndex;
    const string Url="http://127.0.0.1:8768";
    string project,token,device;
    AudioClip recording;
    float recordStart,oldVolume=1,releaseDeadline=-1;
    readonly float[] meterSamples=new float[2048];
    System.Diagnostics.Process service;
    readonly List<string> history=new List<string>();
    public string TalkHint => QuestInput.Active?"Mantén el gatillo derecho para hablar":"Mantén E para hablar con BB-8";
    void Start(){ StartCoroutine(Connect()); }
    public void Retry(){ if(!Busy && !Recording) StartCoroutine(Connect()); }
    IEnumerator Connect(){
        Ready=false; Busy=true; Status="Preparando voz local…";
        if(QuestInput.Active){yield return ConnectQuest();Busy=false;yield break;}
        var installed=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Library/Application Support/BB8Voice");
        if(Directory.Exists(Path.Combine(installed,".local-voice"))) project=installed;
        var root=new DirectoryInfo(Application.dataPath);
        for(int i=0;i<6 && root!=null && project==null;i++,root=root.Parent){
            if(Directory.Exists(Path.Combine(root.FullName,".local-voice"))) {project=root.FullName; break;}
            if(Directory.Exists(Path.Combine(root.FullName,"BB8/.local-voice"))) {project=Path.Combine(root.FullName,"BB8"); break;}
        }
        if(project==null){Status="No encuentro la instalación local. Mantén BB8.app junto a la carpeta BB8.";Busy=false;yield break;}
        bool launched=false;
        for(int i=0;i<150;i++){
            using(var req=UnityWebRequest.Get(Url+"/health")){
                req.timeout=2; yield return req.SendWebRequest();
                if(req.result==UnityWebRequest.Result.Success){
                    var health=JsonUtility.FromJson<Health>(req.downloadHandler.text);
                    if(health.service!="bb8-local-voice"){Status="El puerto de voz está ocupado."; break;}
                    if(health.ready){token=File.ReadAllText(Path.Combine(project,".local-voice/token")).Trim();Ready=true;Status="Mantén E para hablar con BB-8"; break;}
                    if(!string.IsNullOrEmpty(health.error)){Status="No se pudo iniciar la voz. Revisa .local-voice/service.log";break;}
                }else if(!launched){
                    launched=true;
                    try{
                        var info=new System.Diagnostics.ProcessStartInfo(Path.Combine(project,".local-voice/venv/bin/python"));
                        var script=Path.Combine(project,"VoiceService/service.py");
                        if(!File.Exists(script)) script=Path.Combine(project,"Assets/StreamingAssets/VoiceService/service.py");
                        info.Arguments="\""+script+"\"";
                        info.WorkingDirectory=project;info.UseShellExecute=false;info.CreateNoWindow=true;
                        info.EnvironmentVariables["BB8_PROJECT"]=project;
                        service=System.Diagnostics.Process.Start(info);
                    }catch(Exception e){Debug.LogWarning(e.Message);Status="No se pudo iniciar Python local.";break;}
                }
            }
            yield return new WaitForSecondsRealtime(1);
        }
        Busy=false;
        if(!Ready && Status=="Preparando voz local…") Status="La voz tardó demasiado. Pulsa Reintentar.";
    }
    IEnumerator ConnectQuest(){
        var path=Path.Combine(Application.persistentDataPath,"quest-voice-token");
        if(!File.Exists(path)){Status="Conecta el USB y abre Iniciar BB8 VR en el Mac.";yield break;}
        token=File.ReadAllText(path).Trim();
        using(var req=UnityWebRequest.Get(Url+"/health")){
            req.timeout=3;yield return req.SendWebRequest();
            if(req.result==UnityWebRequest.Result.Success){
                var health=JsonUtility.FromJson<Health>(req.downloadHandler.text);
                Ready=health.service=="bb8-local-voice"&&health.ready;
                Status=Ready?TalkHint:"El Mac está preparando la voz…";
                if(Ready)Debug.Log("BB8_QUEST_VOICE_READY");
            }else Status="Voz desconectada. Conecta el USB y abre Iniciar BB8 VR.";
        }
    }
    public void BeginRecording(){if(Ready&&!Busy&&!Recording) StartCoroutine(Begin());}
    IEnumerator Begin(){
        Busy=true;
#if UNITY_ANDROID && !UNITY_EDITOR
        if(!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone)){
            bool answered=false;var callbacks=new UnityEngine.Android.PermissionCallbacks();
            callbacks.PermissionGranted+=_=>answered=true;callbacks.PermissionDenied+=_=>answered=true;
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone,callbacks);
            float deadline=Time.realtimeSinceStartup+45;
            while(!answered&&Time.realtimeSinceStartup<deadline)yield return null;
        }
        if(!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone)){Status="Permite el micrófono en los permisos de BB8 del visor.";Busy=false;yield break;}
#else
        if(!Application.HasUserAuthorization(UserAuthorization.Microphone)) yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if(!Application.HasUserAuthorization(UserAuthorization.Microphone)){Status="Permite el micrófono en Ajustes del Sistema > Privacidad > Micrófono.";Busy=false;yield break;}
#endif
        var devices=Microphone.devices;
        if(devices.Length==0){Status="No se detectó un micrófono.";Busy=false;yield break;}
        if(!QuestInput.Talking){Status=TalkHint;Busy=false;yield break;}
        device=devices[Mathf.Clamp(MicrophoneIndex,0,devices.Length-1)];
        Microphone.GetDeviceCaps(device,out int minimum,out int maximum);
        int rate=QuestInput.Active?48000:16000;
        if(maximum>0)rate=Mathf.Clamp(rate,Mathf.Max(8000,minimum),maximum);
        Status="Preparando micrófono…";
        recording=Microphone.Start(device,false,15,rate);
        if(!recording){Status="No se pudo abrir el micrófono.";Busy=false;yield break;}
        oldVolume=AudioListener.volume;AudioListener.volume=0;
        recordStart=Time.realtimeSinceStartup;releaseDeadline=-1;MicrophoneLevel=0;Recording=true;
        // Do not announce readiness before Android actually starts delivering samples.
        while(Recording&&Microphone.GetPosition(device)<=0&&Time.realtimeSinceStartup-recordStart<3)yield return null;
        if(!Recording){Busy=false;yield break;}
        if(Microphone.GetPosition(device)<=0){CancelRecording();Status="El micrófono no está entregando audio. Vuelve a intentarlo.";yield break;}
        Busy=false;Status=QuestInput.Active?"Te escucho… suelta el gatillo para responder":"Te escucho… suelta E para responder";
    }
    void Update(){
        if(!Recording||Busy)return;
        int position=Microphone.GetPosition(device);
        if(position>meterSamples.Length){
            recording.GetData(meterSamples,position-meterSamples.Length);
            float sum=0;foreach(float sample in meterSamples)sum+=sample*sample;
            float rms=Mathf.Sqrt(sum/meterSamples.Length);
            MicrophoneLevel=Mathf.Lerp(MicrophoneLevel,Mathf.Clamp01((20*Mathf.Log10(Mathf.Max(rms,.00001f))+60)/48),.4f);
        }
        if(QuestInput.Talking)releaseDeadline=-1;
        else if(releaseDeadline<0)releaseDeadline=Time.realtimeSinceStartup+.12f;
        if((releaseDeadline>=0&&Time.realtimeSinceStartup>=releaseDeadline)||Time.realtimeSinceStartup-recordStart>14.7f)EndRecording();
    }
    void OnApplicationFocus(bool focused){if(!focused && Recording) CancelRecording();}
    void CancelRecording(){if(!Recording)return;Microphone.End(device);Recording=false;Busy=false;MicrophoneLevel=0;AudioListener.volume=oldVolume;if(recording)Destroy(recording);Status="Grabación cancelada";}
    void EndRecording(){
        int count=Microphone.GetPosition(device);Microphone.End(device);Recording=false;AudioListener.volume=oldVolume;
        if(count<recording.frequency*.3f){Destroy(recording);Status="Habla durante al menos un segundo. "+TalkHint;return;}
        var samples=new float[count*recording.channels];recording.GetData(samples,0);
        var wav=Wav(samples,recording.frequency,recording.channels);Destroy(recording);
        StartCoroutine(Send(wav,false));
    }
    public void SendText(string text){if(!Ready||Busy||Recording||string.IsNullOrWhiteSpace(text))return;
        StartCoroutine(Send(Encoding.UTF8.GetBytes(JsonUtility.ToJson(new TextRequest{text=text,history=history.ToArray()})),true));}
    IEnumerator Send(byte[] bytes,bool isText){
        Busy=true;Status=isText?"BB-8 está interpretando…":"BB-8 está escuchando e interpretando…";
        using(var req=new UnityWebRequest(Url+(isText?"/classify":"/voice"),"POST")){
            req.uploadHandler=new UploadHandlerRaw(bytes);req.downloadHandler=new DownloadHandlerBuffer();req.timeout=90;
            if(!isText) req.SetRequestHeader("X-BB8-Context",Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonUtility.ToJson(new TextRequest{history=history.ToArray()}))));
            req.SetRequestHeader("Content-Type",isText?"application/json":"audio/wav");req.SetRequestHeader("X-BB8-Token",token);
            yield return req.SendWebRequest();
            VoiceResult answer=null;
            try{answer=JsonUtility.FromJson<VoiceResult>(req.downloadHandler.text);}catch(Exception){ }
            if(req.result!=UnityWebRequest.Result.Success || answer==null || !string.IsNullOrEmpty(answer.error)){
                Status=answer!=null&&!string.IsNullOrEmpty(answer.error)?answer.error:"No se pudo contactar la voz local. Pulsa Reintentar.";
                if(req.result==UnityWebRequest.Result.ConnectionError)Ready=false;
            }else{
                LastResult=answer;Transcript=answer.text;history.Add(answer.text);if(history.Count>3)history.RemoveAt(0);
                Status=TalkHint;Result?.Invoke(answer);
            }
        }
        Busy=false;
    }
    public static byte[] Wav(float[] samples,int rate,int channels){
        using(var stream=new MemoryStream())using(var writer=new BinaryWriter(stream)){
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples.Length*2);writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)channels);writer.Write(rate);writer.Write(rate*channels*2);writer.Write((short)(channels*2));writer.Write((short)16);writer.Write(Encoding.ASCII.GetBytes("data"));writer.Write(samples.Length*2);foreach(var sample in samples)writer.Write((short)(Mathf.Clamp(sample,-1,1)*32767));return stream.ToArray();
        }
    }
    void OnDestroy(){CancelRecording();if(service!=null){try{if(!service.HasExited)service.Kill();}catch(Exception){}service.Dispose();}}
}
