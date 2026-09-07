using UnityEngine;
[DefaultExecutionOrder(-100)]
public sealed class BB8SocialBrain : MonoBehaviour
{
    public Transform Partner;
    public SocialReaction[] Reactions;
    public bool Busy { get; private set; }
    public bool Listening;
    BbRigidbodyController motor;
    BB8Personality personality;
    Rigidbody body;
    SocialReaction active;
    float started, intensity;
    bool bounced;
    void Awake() { motor=GetComponent<BbRigidbodyController>(); personality=GetComponent<BB8Personality>(); body=GetComponent<Rigidbody>(); }
    public void React(string id, int strength) {
        active=System.Array.Find(Reactions,r=>r && r.Id==id);
        if(!active) return;
        Busy=true; started=Time.time; intensity=Mathf.Lerp(.7f,1.25f,(Mathf.Clamp(strength,1,3)-1)/2f); bounced=false;
        motor.SetInput(Vector2.zero,false,false);
        personality.Speak(active.Sound,active.Pitch);
    }
    void FixedUpdate() {
        if(!Busy && !Listening) { personality.SocialHeadPose=Vector3.Lerp(personality.SocialHeadPose,Vector3.zero,.15f); return; }
        var toward=Vector3.ProjectOnPlane(Partner.position-transform.position,Vector3.up);
        float yaw=Vector3.SignedAngle(Vector3.ProjectOnPlane(motor.Head.transform.up,Vector3.up),toward,Vector3.up);
        float t=Time.time-started;
        personality.SocialHeadPose=new Vector3(Busy?Mathf.Sin(t*active.HeadSpeed)*active.HeadTilt*intensity:6,0,Mathf.Clamp(yaw,-65,65));
        Vector3 move=Vector3.zero;
        bool jump=false;
        if(Busy) {
            float envelope=Mathf.Sin(Mathf.Clamp01(t/active.Duration)*Mathf.PI);
            move=toward.normalized*active.Movement*envelope*intensity;
            if(active.Movement>0 && toward.magnitude<2f) move=Vector3.zero;
            int mask=~((1<<8)|(1<<9));
            if(move.sqrMagnitude>.001f && (Physics.SphereCast(transform.position,.48f,move.normalized,out _,1f,mask,QueryTriggerInteraction.Ignore) || !Physics.Raycast(transform.position+move.normalized*.9f+Vector3.up*.3f,Vector3.down,1.6f,mask,QueryTriggerInteraction.Ignore))) move=Vector3.zero;
            if(active.Bounce && !bounced && t>.35f && motor.IsGrounded) { jump=true; bounced=true; }
            if(t>=active.Duration) { Busy=false; move=Vector3.zero; }
        }
        var forward=Vector3.ProjectOnPlane(motor.Camera.forward,Vector3.up).normalized;
        motor.SetInput(new Vector2(Vector3.Dot(move,Vector3.Cross(Vector3.up,forward)),Vector3.Dot(move,forward)),jump,false);
        // Short expressive steps should not keep rolling when the gesture finishes.
        var horizontal=Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up);
        if(horizontal.magnitude>1.2f || move.sqrMagnitude<.001f) body.AddForce(-horizontal*5f,ForceMode.Acceleration);
    }
}
