using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public sealed class MinionController : MonoBehaviour
{
    public bool Controlled;
    public Transform Camera, Visual;
    public Transform LeftArm, RightArm, LeftLeg, RightLeg;
    CharacterController motor;
    Quaternion[] rests;
    Transform[] bones;
    float vertical, gait;
    Vector3 spawn;
    void Awake() {
        motor = GetComponent<CharacterController>(); spawn = transform.position;
        bones = new[] { LeftArm, RightArm, LeftLeg, RightLeg };
        rests = new Quaternion[4]; for (int i=0;i<4;i++) if(bones[i]) rests[i]=bones[i].localRotation;
    }
    void Update() {
        var input = Controlled ? Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical")),1) : Vector2.zero;
        var forward=Vector3.ProjectOnPlane(Camera.forward,Vector3.up).normalized;
        var direction=forward*input.y+Vector3.Cross(Vector3.up,forward)*input.x;
        if(motor.isGrounded) { vertical=-2f; if(Controlled && Input.GetButtonDown("Jump")) vertical=6f; }
        vertical-=18f*Time.deltaTime;
        motor.Move((direction*4.5f+Vector3.up*vertical)*Time.deltaTime);
        if(direction.sqrMagnitude>.01f) transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction),1-Mathf.Exp(-12*Time.deltaTime));
        gait+=Time.deltaTime*10f;
        for(int i=0;i<4;i++) if(bones[i]) bones[i].localRotation=rests[i]*Quaternion.Euler(Mathf.Sin(gait+(i%2)*Mathf.PI)*input.magnitude*25f,0,0);
        if(transform.position.y < -8) { motor.enabled=false; transform.position=spawn; motor.enabled=true; vertical=0; }
    }
}
