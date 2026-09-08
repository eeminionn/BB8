using UnityEngine;
public class DragMouseOrbit:MonoBehaviour
{
    public Transform Target;
    public float Distance=5f,XSpeed=10f,YSpeed=10f,YMinLimit=-12f,YMaxLimit=70f,DistanceMin=2f,DistanceMax=12f,SmoothTime=6f;
    public bool FirstPerson,SuspendInput;
    public float EyeHeight=1f;
    public bool AutoFollow=true;
    float lastMouse=-10f,followVelocity,yaw,pitch=14,actualDistance;
    Vector3 smoothedFocus,focusVelocity;
    Transform previousTarget;
    bool previousPOV;
    Camera view;
    void Start(){view=GetComponent<Camera>();actualDistance=Distance;yaw=transform.eulerAngles.y;pitch=14;}
    void LateUpdate(){
        if(!Target)return;
        // A future XR rig owns tracked head pose; this orbit camera is desktop-only.
        if(UnityEngine.XR.XRSettings.enabled)return;
        float dt=Time.deltaTime;
        var motor=Target.GetComponent<BbRigidbodyController>();var minion=Target.GetComponent<MinionController>();
        var facing=motor?motor.FacingDirection:minion?minion.FacingDirection:Target.forward;
        bool changed=Target!=previousTarget||FirstPerson!=previousPOV;
        if(dt<=0&&!changed)return;
        if(changed){
            yaw=Mathf.Atan2(facing.x,facing.z)*Mathf.Rad2Deg;pitch=FirstPerson?0:14;followVelocity=0;
            previousTarget=Target;previousPOV=FirstPerson;
        }
        if(!SuspendInput&&Input.GetMouseButton(1)){
            float x=Input.GetAxis("Mouse X"),y=Input.GetAxis("Mouse Y");
            yaw+=x*2.4f;pitch=Mathf.Clamp(pitch-y*1.8f,FirstPerson?-55:4,FirstPerson?55:48);
            if(Mathf.Abs(x)+Mathf.Abs(y)>.01f)lastMouse=Time.time;
        }
        if(!SuspendInput&&!FirstPerson)Distance=Mathf.Clamp(Distance-Input.GetAxis("Mouse ScrollWheel")*3,3.2f,7.5f);
        bool steering=motor?motor.Steering:minion&&minion.Steering;
        if(AutoFollow&&!SuspendInput&&!Input.GetMouseButton(1)&&steering&&Time.time-lastMouse>.9f){
            yaw=Mathf.SmoothDampAngle(yaw,Mathf.Atan2(facing.x,facing.z)*Mathf.Rad2Deg,ref followVelocity,.28f,180,dt);
        }
        var focus=Target.position+Vector3.up*(FirstPerson?EyeHeight:motor?.65f:1.03f);
        if(changed||FirstPerson){smoothedFocus=focus;focusVelocity=Vector3.zero;}
        else smoothedFocus=Vector3.SmoothDamp(smoothedFocus,focus,ref focusVelocity,.09f,Mathf.Infinity,dt);
        var rotation=Quaternion.Euler(pitch,yaw,0);var back=rotation*Vector3.back;
        float desired=FirstPerson?0:Distance;
        if(!FirstPerson&&Physics.SphereCast(smoothedFocus,.25f,back,out var hit,Distance,~((1<<8)|(1<<9)),QueryTriggerInteraction.Ignore))desired=Mathf.Max(.25f,hit.distance-.08f);
        actualDistance=FirstPerson?0:changed||desired<actualDistance?desired:Mathf.Lerp(actualDistance,desired,1-Mathf.Exp(-5*dt));
        transform.SetPositionAndRotation(smoothedFocus+back*actualDistance,rotation);
        if(view){view.fieldOfView=65;view.nearClipPlane=.08f;}
    }
}
