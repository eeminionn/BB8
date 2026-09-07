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
    public Vector3 FacingDirection => transform.forward;
    public bool Steering { get; private set; }
    void Awake() {
        motor = GetComponent<CharacterController>(); spawn = transform.position;
        if(Camera) transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(Camera.forward,Vector3.up).normalized);
        bones = new[] { LeftArm, RightArm, LeftLeg, RightLeg };
        rests = new Quaternion[4]; for (int i=0;i<4;i++) if(bones[i]) rests[i]=bones[i].localRotation;
    }
    void Update() {
        var input = Controlled ? Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical")),1) : Vector2.zero;
        Steering=input.sqrMagnitude>.01f;
        transform.Rotate(0,input.x*110f*Time.deltaTime,0,Space.World);
        var direction=transform.forward*input.y;
        if(motor.isGrounded) { vertical=-2f; if(Controlled && Input.GetButtonDown("Jump")) vertical=6f; }
        vertical-=18f*Time.deltaTime;
        motor.Move((direction*4.5f+Vector3.up*vertical)*Time.deltaTime);
        gait+=Time.deltaTime*10f;
        for(int i=0;i<4;i++) if(bones[i]) bones[i].localRotation=rests[i]*Quaternion.Euler(Mathf.Sin(gait+(i%2)*Mathf.PI)*Mathf.Abs(input.y)*25f,0,0);
        if(transform.position.y < -8) { motor.enabled=false; transform.position=spawn; motor.enabled=true; vertical=0; }
    }
}
