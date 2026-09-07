using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class BB8SocialBrain : MonoBehaviour
{
    public Transform Partner;
    public SocialReaction[] Reactions;
    public bool Busy { get; private set; }
    public bool Listening;
    public bool Autonomous => following || leaving;
    public bool Following => following;
    bool following, leaving;
    Vector3 departureTarget;
    float departureRemaining;
    public int Strength { get; private set; }
    public float ReactionDuration { get; private set; }
    public event Action<SocialReaction,int,string> ReactionStarted;
    BbRigidbodyController motor;
    BB8Personality personality;
    Rigidbody body;
    SocialReaction active;
    Vector3 origin, forward, right;
    float started, travel, approach, nextVoice, nextHop;
    int voiceCount, hops;
    const int EnvironmentMask = ~((1<<8)|(1<<9));

    void Awake() { motor=GetComponent<BbRigidbodyController>(); personality=GetComponent<BB8Personality>(); body=GetComponent<Rigidbody>(); }

    public bool ExecuteCommand(string command)
    {
        if(!Partner || (command!="follow" && command!="away"))return false;
        CancelCommand();Busy=false;personality.SocialSpeaking=false;personality.SocialAntenna=0;
        following=command=="follow";leaving=!following;
        var away=Vector3.ProjectOnPlane(body.position-Partner.position,Vector3.up).normalized;
        if(away.sqrMagnitude<.1f)away=-motor.FacingDirection;
        departureTarget=body.position+away*6f;departureRemaining=8f;
        var acknowledgment=Array.Find(Reactions,r=>r && r.Id=="attentive");
        if(acknowledgment)personality.Speak(acknowledgment.Clip(1),1f,.6f);
        return true;
    }
    public void CancelCommand()
    {
        if(Autonomous)body.linearVelocity=Vector3.Project(body.linearVelocity,Vector3.up);
        following=leaving=false;motor.SetInput(Vector2.zero,false,false);
    }
    bool SafeDirection(Vector3 direction,float distance)
    {
        return !Physics.SphereCast(body.position+Vector3.up*.08f,.43f,direction,out _,distance,EnvironmentMask,QueryTriggerInteraction.Ignore)
            && Physics.Raycast(body.position+direction*distance+Vector3.up*.45f,Vector3.down,2f,EnvironmentMask,QueryTriggerInteraction.Ignore);
    }
    void MoveCommand()
    {
        motor.SetInput(Vector2.zero,false,false);
        var toward=Vector3.ProjectOnPlane(Partner.position-body.position,Vector3.up);
        var error=following?toward:Vector3.ProjectOnPlane(departureTarget-body.position,Vector3.up);
        if(leaving){departureRemaining-=Time.fixedDeltaTime;if(error.magnitude<.4f||departureRemaining<=0){CancelCommand();return;}}
        float distance=following?Mathf.Max(0,error.magnitude-2f):error.magnitude;
        var desired=error.normalized*Mathf.Min(4.8f,distance*2.5f);
        float lookAhead=.7f+Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude*.28f;
        if(desired.sqrMagnitude>.01f && !SafeDirection(desired.normalized,lookAhead))
        {
            Vector3 alternate=Vector3.zero;
            foreach(float angle in new[]{35f,-35f,65f,-65f})
            {
                var direction=Quaternion.AngleAxis(angle,Vector3.up)*desired.normalized;
                if(SafeDirection(direction,lookAhead)){alternate=direction*Mathf.Min(2f,desired.magnitude);break;}
            }
            desired=alternate;
        }
        if(toward.magnitude<1.6f && Vector3.Dot(desired,toward)>0)desired=Vector3.zero;
        body.AddForce(Vector3.ClampMagnitude((desired-Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up))*12f,22f),ForceMode.Acceleration);
        float yaw=Vector3.SignedAngle(Vector3.ProjectOnPlane(motor.Head.transform.up,Vector3.up),following?toward:error,Vector3.up);
        personality.SocialHeadPose=Vector3.Lerp(personality.SocialHeadPose,new Vector3(3,0,Mathf.Clamp(yaw,-65,65)),.16f);
    }

    public void React(string id, int strength, string attitude = "neutral")
    {
        active=Array.Find(Reactions,r=>r && r.Id==id);
        if(!active || !Partner) return;
        Strength=Mathf.Clamp(strength,1,3);
        origin=body.position;
        forward=Vector3.ProjectOnPlane(Partner.position-origin,Vector3.up).normalized;
        if(forward.sqrMagnitude<.1f) forward=Vector3.forward;
        right=Vector3.Cross(Vector3.up,forward);
        travel=active.Travel[Strength-1];
        approach=Mathf.Min(travel,Mathf.Max(0,Vector3.Distance(Partner.position,origin)-1.8f));
        ReactionDuration=active.Durations[Strength-1];
        started=Time.time; Busy=true; hops=0; voiceCount=0; nextHop=.4f;
        motor.SetInput(Vector2.zero,false,false);
        personality.SocialSpeaking=true;
        Speak();
        ReactionStarted?.Invoke(active,Strength,attitude);
    }

    void Speak()
    {
        int level=voiceCount==0 ? Strength : Mathf.Max(1,Strength-1);
        var clip=active.Clip(level);
        personality.Speak(clip,active.Pitch,Mathf.Lerp(.48f,.8f,(Strength-1)/2f));
        voiceCount++;
        nextVoice=Time.time-started+Mathf.Max(1.1f,clip ? clip.length/active.Pitch+.35f : 1.2f);
    }

    public Vector3 PlannedOffset(float progress)
    {
        float u=Mathf.Clamp01(progress), wave=Mathf.Sin(u*Mathf.PI), settle=Mathf.SmoothStep(0,1,Mathf.Clamp01(u/.78f));
        switch(active.Id)
        {
            case "celebrate": return right*Mathf.Sin(u*Mathf.PI*2)*travel*.6f + forward*Mathf.Sin(u*Mathf.PI*4)*travel*.25f;
            case "retreat": return -forward*travel*settle + right*Mathf.Sin(u*Mathf.PI*(Strength==3?4:2))*wave*travel*.24f;
            case "comfort": return forward*approach*settle + right*wave*travel*.22f;
            case "cautious": return -forward*travel*.65f*settle + right*Mathf.Sin(u*Mathf.PI*6)*wave*travel*.2f;
            case "puzzled": return right*Mathf.Sin(u*Mathf.PI*2)*travel*.6f + forward*wave*travel*.16f;
            default: return forward*wave*approach*.55f;
        }
    }

    void FixedUpdate()
    {
        if(!Partner) return;
        if(!Busy && !Listening && Autonomous){MoveCommand();return;}
        if(!Busy && !Listening) { personality.SocialHeadPose=Vector3.Lerp(personality.SocialHeadPose,Vector3.zero,.16f);return; }
        var toward=Vector3.ProjectOnPlane(Partner.position-body.position,Vector3.up);
        float yaw=Vector3.SignedAngle(Vector3.ProjectOnPlane(motor.Head.transform.up,Vector3.up),toward,Vector3.up);
        float t=Time.time-started;
        if(!Busy) { personality.SocialHeadPose=Vector3.Lerp(personality.SocialHeadPose,new Vector3(5,0,Mathf.Clamp(yaw,-70,70)),.16f);motor.SetInput(Vector2.zero,false,false);Brake();return; }
        float u=Mathf.Clamp01(t/ReactionDuration), envelope=Mathf.Sin(u*Mathf.PI);
        float amplitude=Strength==1?9:Strength==2?19:31;
        Vector3 pose;
        switch(active.Id)
        {
            case "celebrate": pose=new Vector3(Mathf.Sin(t*8)*amplitude,Mathf.Sin(t*5)*amplitude*.55f,Mathf.Sin(t*6)*amplitude*.8f);break;
            case "retreat": pose=new Vector3(-amplitude*.7f,Mathf.Sin(t*13)*amplitude*.45f,Mathf.Sin(t*8)*amplitude);break;
            case "comfort": pose=new Vector3(amplitude*.55f+Mathf.Sin(t*2.8f)*amplitude*.3f,Mathf.Sin(t*1.8f)*amplitude*.5f,Mathf.Sin(t*2)*8);break;
            case "cautious": pose=new Vector3(-amplitude*.4f,Mathf.Sin(t*16)*amplitude*.35f,Mathf.Sin(t*4)*amplitude*1.2f);break;
            case "puzzled": pose=new Vector3(Mathf.Sin(t*2.2f)*amplitude*.3f,Mathf.Sin(t*2.4f)*amplitude,Mathf.Sin(t*2)*amplitude*.7f);break;
            default: pose=new Vector3(Mathf.Sin(t*4)*amplitude*.6f,0,Mathf.Sin(t*2)*amplitude*.3f);break;
        }
        pose*=envelope; pose.z+=Mathf.Clamp(yaw,-65,65);
        personality.SocialHeadPose=Vector3.Lerp(personality.SocialHeadPose,pose,.25f);
        personality.SocialAntenna=Strength*envelope;
        bool jump=active.Id=="celebrate" && hops<Strength && t>=nextHop && motor.IsGrounded && u<.8f;
        if(jump){hops++;nextHop=t+1.15f;}
        motor.SetInput(Vector2.zero,jump,false);
        if(t>=nextVoice && voiceCount<Strength && u<.8f) Speak();
        var error=Vector3.ProjectOnPlane(origin+PlannedOffset(u)-body.position,Vector3.up);
        var desired=Vector3.ClampMagnitude(error*4.5f,Strength==1?1.2f:Strength==2?2.6f:4.3f);
        // Keep physical collisions and stop before holes; never teleport through scenery.
        float lookAhead=.65f+Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude*.28f;
        if(desired.sqrMagnitude>.01f)
        {
            var direction=desired.normalized;
            bool blocked=Physics.SphereCast(body.position+Vector3.up*.08f,.43f,direction,out _,lookAhead,EnvironmentMask,QueryTriggerInteraction.Ignore);
            bool ground=Physics.Raycast(body.position+direction*lookAhead+Vector3.up*.45f,Vector3.down,2f,EnvironmentMask,QueryTriggerInteraction.Ignore);
            bool partnerClose=Vector3.Dot(direction,toward)>0 && toward.magnitude<1.6f;
            if(blocked||!ground||partnerClose) desired=Vector3.zero;
        }
        var velocity=Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up);
        body.AddForce(Vector3.ClampMagnitude((desired-velocity)*12f,22f),ForceMode.Acceleration);
        if(u>=1)
        {
            Busy=false;personality.SocialSpeaking=false;personality.SocialAntenna=0;
            body.linearVelocity=Vector3.Project(body.linearVelocity,Vector3.up);
            motor.SetInput(Vector2.zero,false,false);
        }
    }
    void Brake(){body.AddForce(-Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up)*8,ForceMode.Acceleration);}
}
