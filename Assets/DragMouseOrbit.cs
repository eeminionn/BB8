using UnityEngine;
public class DragMouseOrbit : MonoBehaviour
{
    public Transform Target;
    public float Distance=5f, XSpeed=10f, YSpeed=10f, YMinLimit=-12f, YMaxLimit=70f, DistanceMin=2f, DistanceMax=12f, SmoothTime=6f;
    public bool FirstPerson, SuspendInput;
    public float EyeHeight=1f;
    float yaw,pitch,velocityX,velocityY,actualDistance;
    Camera view;
    void Start(){ pitch=transform.eulerAngles.x; yaw=transform.eulerAngles.y; actualDistance=Distance; view=GetComponent<Camera>(); }
    void LateUpdate(){
        if(!Target) return;
        if(!SuspendInput){
            velocityX+=XSpeed*Input.GetAxis("Mouse X")*.02f;
            velocityY+=YSpeed*Input.GetAxis("Mouse Y")*.02f;
            Distance=Mathf.Clamp(Distance-Input.GetAxis("Mouse ScrollWheel")*5,DistanceMin,DistanceMax);
        }
        yaw+=velocityX; pitch=Mathf.Clamp(pitch-velocityY,FirstPerson?-65:YMinLimit,YMaxLimit);
        var rotation=Quaternion.Euler(pitch,yaw,0);
        var focus=Target.position+Vector3.up*(FirstPerson?EyeHeight:.65f);
        var direction=rotation*Vector3.back;
        float desired=FirstPerson?0:Distance;
        if(!FirstPerson && Physics.SphereCast(focus,.22f,direction,out var hit,Distance,~((1<<8)|(1<<9)),QueryTriggerInteraction.Ignore)) desired=Mathf.Max(.3f,hit.distance-.1f);
        actualDistance=desired<actualDistance?desired:Mathf.Lerp(actualDistance,desired,1-Mathf.Exp(-8*Time.deltaTime));
        transform.SetPositionAndRotation(focus+direction*actualDistance,rotation);
        velocityX=Mathf.Lerp(velocityX,0,Time.deltaTime*SmoothTime); velocityY=Mathf.Lerp(velocityY,0,Time.deltaTime*SmoothTime);
        var motor=Target.GetComponent<BbRigidbodyController>();
        if(view) view.fieldOfView=Mathf.Lerp(view.fieldOfView,FirstPerson?68: motor&&motor.IsBoosting?64:59,1-Mathf.Exp(-5*Time.deltaTime));
    }
}
