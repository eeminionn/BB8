#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
public sealed class DestinationCheckDriver:MonoBehaviour
{
    bool passed=true;readonly List<string> results=new List<string>();
    void Check(bool value,string label){passed&=value;results.Add((value?"PASS ":"FAIL ")+label);Debug.Log(results[results.Count-1]);}
    IEnumerator Start(){
        Application.runInBackground=true;yield return new WaitForSeconds(1);
        var d=FindFirstObjectByType<ConversationDirector>();var worlds=WorldDestinations.Instance;var shell=ExperienceShell.Instance;var voice=d.Voice;
        Check(worlds&&worlds.ActiveIndex==0&&worlds.Desert.activeSelf,"Khepra is the initial destination");
        Check(Resources.Load<Font>("Fonts/Anton-Regular")&&Resources.Load<Font>("Fonts/Rajdhani-Medium"),"galactic fonts bundled locally");
        d.Brain.ExecuteCommand("follow");
        shell.SetMenu(true);shell.SelectDestination(1);shell.Enter();yield return new WaitForSeconds(1);
        Check(worlds.ActiveIndex==1&&worlds.City.activeSelf&&!worlds.Desert.activeSelf&&!worlds.Arrival.activeSelf,"Earth activates exclusively");
        Check(!d.Brain.Autonomous&&!d.Brain.Busy,"travel cancels old movement and gestures");
        Check(d.Minion.Grounded&&d.Minion.transform.position.y>.10f&&d.Minion.transform.position.y<.20f,"Minion lands on city plaza");
        Check(d.BB8.IsGrounded&&d.BB8.transform.position.y>.55f,"BB8 settles on city plaza");
        Check(FindObjectsByType<MinionController>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<BbRigidbodyController>(FindObjectsSortMode.None).Length==1,"actors are not duplicated");
        Check(d.Voice==voice&&FindObjectsByType<LocalVoiceClient>(FindObjectsSortMode.None).Length==1,"voice service is preserved");
        Check(worlds.City.GetComponentsInChildren<EnergyCell>().Length==2,"city has two recharge points");
        Check(Physics.Raycast(new Vector3(0,4,0),Vector3.down,out var hit,6,~((1<<8)|(1<<9)))&&hit.collider.transform.IsChildOf(worlds.City.transform),"ground belongs to Earth");
        d.enabled=false;d.Minion.enabled=false;var before=d.Minion.transform.position;
        for(float t=0;t<1;t+=Time.deltaTime){d.Minion.Drive(Vector2.up,false,Time.deltaTime);yield return null;}
        Check(Vector3.Distance(before,d.Minion.transform.position)>1.5f,"city is walkable");
        d.Minion.enabled=true;d.enabled=true;
        var city=worlds.City;float affinity=d.Popup.Affinity;
        Check(worlds.Travel(0),"return to Khepra");yield return new WaitForSeconds(.7f);
        Check(worlds.Desert.activeSelf&&!worlds.City.activeSelf&&d.Minion.Grounded,"Khepra restores environment and safe spawn");
        d.View.FirstPerson=true;Check(worlds.Travel(1),"second trip to Earth");yield return new WaitForSeconds(.7f);
        Check(worlds.City==city&&d.Popup.Affinity==affinity,"reuses city and preserves affinity");
        Check(Vector3.Distance(d.View.transform.position,d.Minion.transform.position+Vector3.up*d.View.EyeHeight)<.05f,"POV survives travel without stale camera position");
        shell.SetMenu(true);Check(AudioListener.pause&&shell.Muted,"menu and worlds remain silent");
        Check(!worlds.Travel(3),"invalid destination rejected");
        File.WriteAllLines("Logs/destination-verification.txt",results);EditorApplication.Exit(passed?0:1);
    }
}
#endif
