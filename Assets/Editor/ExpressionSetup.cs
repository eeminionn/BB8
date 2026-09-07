using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ExpressionSetup
{
    [MenuItem("BB8/Upgrade Expressions")]
    public static void Upgrade()
    {
        BB8Setup.OpenDemo();
        Configure("celebrate",new Vector3(.65f,1.8f,3.5f),new[]{"Contento","Feliz","Eufórico"},new[]{"beep-bwee!","beep-beep, bwoop!","bwee-beep-boop! beep-bwee!"},1.04f);
        Configure("comfort",new Vector3(.65f,1.7f,3.2f),new[]{"Atento","Preocupado","Conmovido"},new[]{"boop…","boop-bwooo…","boop-bwooo… boop-beep…"},.95f);
        Configure("retreat",new Vector3(.85f,2.4f,4.8f),new[]{"Incómodo","Asustado","Aterrado"},new[]{"brr-beep!","brrr-bweep!","brrr-bweep! brr-beep-beep!"},1f);
        Configure("cautious",new Vector3(.6f,1.8f,3.6f),new[]{"Inquieto","Nervioso","Alarmado"},new[]{"bwoo?","bwoo-brr…","bwoo-brr! boop-brr…"},1f);
        Configure("attentive",new Vector3(.5f,1.1f,2f),new[]{"Atento","Interesado","Expectante"},new[]{"beep.","beep-boop.","beep-boop, beep-beep."},1f);
        Configure("puzzled",new Vector3(.5f,1.2f,2.5f),new[]{"Intrigado","Confundido","Desconcertado"},new[]{"bweep?","bweep-boop?","bweep-boop? bweee?"},1f);
        var view=Object.FindFirstObjectByType<DragMouseOrbit>();view.AutoFollow=true;
        EditorSceneManager.MarkSceneDirty(view.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        Debug.Log("BB8_EXPRESSIONS_UPGRADED 18 sound variants, 3 distinct motion levels");
    }
    static void Configure(string id,Vector3 travel,string[] feelings,string[] words,float pitch)
    {
        var p=AssetDatabase.LoadAssetAtPath<SocialReaction>("Assets/Conversation/Reactions/"+id+".asset");
        p.Travel=travel;p.Durations=new Vector3(2.6f,3.8f,5.2f);p.Feelings=feelings;p.DroidWords=words;p.Pitch=pitch;
        p.IntensitySounds=new AudioClip[3];
        for(int i=0;i<3;i++)p.IntensitySounds[i]=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Conversation/Audio/"+id+"-"+(i+1)+".wav");
        p.Sound=p.IntensitySounds[0];EditorUtility.SetDirty(p);
    }
}
