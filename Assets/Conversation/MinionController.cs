using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public sealed class MinionController : MonoBehaviour
{
    public bool Controlled;
    public bool Speaking;
    public Transform Camera, Visual;
    public Transform LeftArm, RightArm, LeftLeg, RightLeg;
    public bool Grounded {get;private set;}
    public string MotionState {get;private set;}="Reposo";
    public Vector3 FacingDirection => transform.forward;
    public bool Steering {get;private set;}
    CharacterController motor;
    Quaternion[] rests;
    Vector3[] swingAxes, spreadAxes;
    Transform[] bones;
    float vertical,gait,speed,blend,landed,lastGround=-10,jumpUntil=-10,waveUntil;
    Vector3 spawn;
    const int GroundMask=~((1<<8)|(1<<9));
    void Awake(){
        motor=GetComponent<CharacterController>();spawn=transform.position;
        bones=new[]{LeftArm,RightArm,LeftLeg,RightLeg};rests=new Quaternion[4];swingAxes=new Vector3[4];spreadAxes=new Vector3[4];
        for(int i=0;i<4;i++)if(bones[i]){
            rests[i]=bones[i].localRotation;
            swingAxes[i]=bones[i].InverseTransformDirection(transform.right);
            spreadAxes[i]=bones[i].InverseTransformDirection(transform.forward);
        }
    }
    void Update(){
        if(QuestInput.Active){Step(Controlled?QuestInput.Movement:Vector2.zero,Controlled&&QuestInput.JumpPressed,Time.deltaTime,true);return;}
        var input=Controlled?Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),1):Vector2.zero;
        if(Controlled&&Input.GetKeyDown(KeyCode.H))Wave();
        Drive(input,Controlled&&Input.GetButtonDown("Jump"),Time.deltaTime);
    }
    public void Wave(){waveUntil=Time.time+1.8f;}
    public void Reposition(Vector3 position,float heading){motor.enabled=false;transform.SetPositionAndRotation(position,Quaternion.Euler(0,heading,0));motor.enabled=true;spawn=position;vertical=speed=blend=0;Grounded=false;lastGround=jumpUntil=-10;}
    public void Drive(Vector2 input,bool jump,float dt){
        Step(input,jump,dt,false);
    }
    void Step(Vector2 input,bool jump,float dt,bool relative){
        if(!motor.enabled||dt<=0)return;
        bool wasGrounded=Grounded;
        Grounded=motor.isGrounded||(vertical<=0&&Physics.SphereCast(transform.position+Vector3.up*.24f,.20f,Vector3.down,out _,.10f,GroundMask,QueryTriggerInteraction.Ignore));
        if(Grounded){lastGround=Time.time;if(vertical<0)vertical=-3;if(!wasGrounded)landed=1;}
        if(jump)jumpUntil=Time.time+.12f;
        if(Time.time<jumpUntil&&Time.time-lastGround<.12f){vertical=5.5f;jumpUntil=-10;lastGround=-10;Grounded=false;}
        Steering=input.sqrMagnitude>.01f;
        Vector3 direction=transform.forward;
        float throttle=input.y;
        if(relative){
            var forward=Vector3.ProjectOnPlane(Camera.forward,Vector3.up).normalized;
            direction=(forward*input.y+Vector3.Cross(Vector3.up,forward)*input.x).normalized;
            throttle=input.magnitude;
            if(direction.sqrMagnitude>.01f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(direction),360*dt);
        }else transform.Rotate(0,input.x*100f*dt,0,Space.World);
        if(!relative)direction=transform.forward;
        speed=Mathf.MoveTowards(speed,throttle*3.2f,dt*(Mathf.Abs(throttle)>.01f?8f:12f));
        vertical=Mathf.Max(-22,vertical-20f*dt);
        var flags=motor.Move((direction*speed+Vector3.up*vertical)*dt);
        if((flags&CollisionFlags.Above)!=0&&vertical>0)vertical=0;
        if((flags&CollisionFlags.Below)!=0&&vertical<0){Grounded=true;vertical=-3;}
        float actualSpeed=Vector3.ProjectOnPlane(motor.velocity,Vector3.up).magnitude;
        blend=Mathf.MoveTowards(blend,Grounded?Mathf.Clamp01(actualSpeed/3.2f):0,dt*7);
        gait+=dt*actualSpeed*3.7f;landed=Mathf.MoveTowards(landed,0,dt*5);
        bool waving=Time.time<waveUntil;
        MotionState=!Grounded?(vertical>0?"Salto":"Caída"):waving?"Saludo":Speaking?"Hablando":landed>.1f?"Aterrizaje":blend>.1f?"Caminando":"Reposo";
        for(int i=0;i<4;i++)if(bones[i]){
            float phase=gait+(i%2)*Mathf.PI+(i<2?Mathf.PI:0);
            float swing=Mathf.Sin(phase)*blend*(i<2?18:24);
            float spread=0;
            if(i<2){swing+=Mathf.Sin(Time.time*1.7f+i)*1.6f*(1-blend);if(!Grounded)spread=(i==0?-1:1)*24;}
            else if(!Grounded)swing=vertical>0?-22:10;
            if(i==1&&(waving||Speaking)){spread=waving?110:35;swing=(waving?Mathf.Sin(Time.time*11)*22:Mathf.Sin(Time.time*4)*5)-20;}
            if(i>=2)swing-=landed*8;
            var target=rests[i]*Quaternion.AngleAxis(swing,swingAxes[i])*Quaternion.AngleAxis(spread,spreadAxes[i]);
            bones[i].localRotation=Quaternion.Slerp(bones[i].localRotation,target,1-Mathf.Exp(-14*dt));
        }
        if(transform.position.y<-8){motor.enabled=false;transform.position=spawn;motor.enabled=true;vertical=speed=0;}
    }
}
