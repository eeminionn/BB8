#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

// Editor-only integration check; never instantiated in the saved scene or included in the Mac app.
[DefaultExecutionOrder(-500)]
public sealed class BB8SmokeDriver : MonoBehaviour
{
    BbRigidbodyController controller;
    Rigidbody body;
    DragMouseOrbit orbit;
    BB8Personality personality;
    readonly List<string> results = new List<string>();
    int stage, ticks;
    float normalSpeed, boostSpeed, batteryAfterBurst, maxRampY, landingY;
    Vector3 origin;
    Quaternion idleRotation;
    int jumps;
    bool jumpedGap, heardBoost, heardCollision;
    float antennaMotion;
    Quaternion antennaRest;
    EnergyCell cell;
    public static string ReportPath => Path.GetFullPath("Logs/scrapyard-verification.txt");
    public static bool Done;
    public static bool Passed;

    void Start()
    {
        Done = false;
        Application.runInBackground = true;
        controller = FindAnyObjectByType<BbRigidbodyController>();
        body = controller.GetComponent<Rigidbody>();
        personality = controller.GetComponent<BB8Personality>();
        orbit = Camera.main.GetComponent<DragMouseOrbit>();
        orbit.enabled = false;
        Camera.main.transform.rotation = Quaternion.Euler(18,0,0);
        controller.Jumped += () => jumps++;
        antennaRest = personality.Antennas[0].localRotation;
        Assert(personality.Antennas.Length == 2 && Array.TrueForAll(personality.Antennas, a => a.childCount > 0), "Both antenna pivots contain their visible meshes");
        Assert(personality.JumpSounds.Length == 3 && personality.BumpSounds.Length == 3 && personality.RechargeSound != null, "Droid audio clips are assigned");
        Assert(FindObjectsByType<EnergyCell>().Length == 6, "Six energy cells exist");
        Warp(new Vector3(23,.58f,-12));
    }
    void Warp(Vector3 p)
    {
        body.position = p; body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero;
        controller.Head.position = p; controller.Head.linearVelocity = Vector3.zero; controller.Head.angularVelocity = Vector3.zero;
        Physics.SyncTransforms();
        controller.SetInput(Vector2.zero,false,false);
    }
    void FixedUpdate()
    {
        if (controller == null || Done) return;
        ticks++;
        controller.SetInput(Vector2.zero,false,false);
        if (stage == 0)
        {
            if (ticks == 60) idleRotation = personality.HeadVisual.localRotation;
            if (ticks < 120) return;
            Assert(Quaternion.Angle(idleRotation,personality.HeadVisual.localRotation) > .2f,"Head moves subtly while idle");
            Assert(controller.IsGrounded,"Body settles on ground");
            Next();
        }
        else if (stage == 1)
        {
            controller.SetInput(Vector2.up,false,false);
            normalSpeed = Mathf.Max(normalSpeed,Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude);
            if(ticks<60)return;
            Warp(new Vector3(23,.58f,-12)); Next();
        }
        else if (stage == 2)
        {
            controller.SetInput(Vector2.up,false,true);
            boostSpeed=Mathf.Max(boostSpeed,Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude);
            heardBoost |= controller.GetComponent<AudioSource>().isPlaying;
            antennaMotion = Mathf.Max(antennaMotion, Quaternion.Angle(antennaRest, personality.Antennas[0].localRotation));
            if(ticks<145)return;
            batteryAfterBurst=controller.Battery;
            Assert(boostSpeed <= controller.BoostSpeed + .5f,"Turbo respects its speed limit",boostSpeed.ToString());
            Assert(heardBoost && antennaMotion > .2f,"Boost produces sound and antenna motion",antennaMotion.ToString());
            Assert(boostSpeed>normalSpeed*1.3f,"Turbo exceeds cruising speed",normalSpeed+" -> "+boostSpeed);
            Assert(!controller.IsBoosting && batteryAfterBurst>.20f && batteryAfterBurst<.40f,"A held Shift produces one short burst",batteryAfterBurst.ToString());
            Next();
        }
        else if(stage==3)
        {
            if(ticks<25)return;
            Warp(new Vector3(23,.58f,-12)); Next();
        }
        else if(stage==4)
        {
            controller.SetInput(Vector2.up,false,true);
            if(ticks<80)return;
            Assert(controller.Battery==0 && !controller.IsBoosting,"Battery reaches zero and turbo stops");
            Next();
            cell=Array.Find(FindObjectsByType<EnergyCell>(),c=>Mathf.Abs(c.transform.position.z+10)<.1f);
            cell.RespawnDelay = .8f;
            Warp(cell.transform.position+Vector3.up*.58f);
        }
        else if(stage==5)
        {
            if(ticks<20)return;
            Assert(controller.Battery>.5f && controller.Battery<.6f && !cell.Visual.gameObject.activeSelf,"A pickup adds charge and becomes unavailable",controller.Battery.ToString());
            controller.AddEnergy(10f);
            Assert(Mathf.Approximately(controller.Battery,1f),"Recharge clamps at full battery");
            Warp(new Vector3(.5f,1.2f,-5)); Next();
        }
        else if(stage==6)
        {
            if(ticks<30)return;
            Next();
        }
        else if(stage==7)
        {
            controller.SetInput(Vector2.up,false,true);
            maxRampY=Mathf.Max(maxRampY,body.position.y);
            if(ticks<72)return;
            Assert(maxRampY>2.65f,"Turbo climbs the maintenance ramp",maxRampY.ToString());
            controller.AddEnergy(1f);
            Warp(new Vector3(.5f,2.97f,5.5f)); Next();
        }
        else if(stage==8)
        {
            if(ticks<35)return;
            origin=body.position; Next();
        }
        else if(stage==9)
        {
            bool jump=!jumpedGap && body.position.z>7.2f;
            if(jump)jumpedGap=true;
            controller.SetInput(Vector2.up,jump,true);
            if(body.position.z>10.7f && body.position.z<12.8f)landingY=Mathf.Max(landingY,body.position.y);
            if(ticks<70)return;
            Assert(jumps>0 && landingY>2.6f,"Boosted jump crosses the catwalk gap", "landing Y="+landingY+" jumps="+jumps);
            Assert(float.IsFinite(body.position.x) && float.IsFinite(controller.Head.position.y),"Body and head stay finite");
            Assert(cell.Visual.gameObject.activeSelf,"A collected energy cell respawns");
            Warp(new Vector3(23,.58f,-12));
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Temporary impact target"; wall.transform.position = new Vector3(23,1,-7); wall.transform.localScale = new Vector3(3,2,.3f);
            Next();
        }
        else if(stage==10)
        {
            controller.SetInput(Vector2.up,false,false);
            if(ticks>35) heardCollision |= controller.GetComponent<AudioSource>().isPlaying;
            if(ticks<110)return;
            Assert(heardCollision,"A physical collision triggers a droid reaction");
            Passed=results.TrueForAll(r=>r.StartsWith("PASS")); Done=true;
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllLines(ReportPath,results);
            Debug.Log("SCRAPYARD_TESTS "+(Passed?"PASS":"FAIL")+"\n"+string.Join("\n",results));
            EditorApplication.delayCall += () => EditorApplication.isPlaying=false;
        }
    }
    void Next(){stage++;ticks=0;}
    void Assert(bool ok,string title,string evidence=""){results.Add((ok?"PASS ":"FAIL ")+title+(evidence.Length>0?" | "+evidence:""));}
}
#endif
