using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BbRigidbodyController : MonoBehaviour
{
    public float Speed = 500f;
    public bool Oomph;
    public bool CanJump = true;
    public float JumpForce = 5000f;
    public float JumpDelay = 0.65f;
    public Vector3 Gravity = Vector3.down;
    public Transform Camera;
    public Rigidbody Head;
    public float TorqueScale = 0.3f;
    [Header("Turbo")]
    public float CruiseSpeed = 7f;
    public float BoostSpeed = 14f;
    public float BoostDuration = 1.4f;
    public float BatteryDrain = 0.5f;
    public float Battery { get; private set; } = 1f;
    public bool IsBoosting { get; private set; }
    public bool IsGrounded => Time.time - lastGroundContact < 0.08f;
    public event Action Jumped;
    public event Action BoostStarted;
    public event Action Recharged;

    Rigidbody body;
    Vector3 up, groundNormal = Vector3.up;
    Vector2 moveInput;
    bool jumpRequested, boostHeld, boostLatched;
    float lastGroundContact = -10f, lastJump = -10f, boostTime, nextBoost;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        up = -Gravity.normalized;
        body.maxAngularVelocity = 28f;
    }

    void Update() => SetInput(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")),
        Input.GetButtonDown("Jump"), Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

    // Shared input boundary also allows repeatable physics checks in the editor.
    public void SetInput(Vector2 movement, bool jump, bool boost)
    {
        moveInput = Vector2.ClampMagnitude(movement, 1f);
        jumpRequested |= jump;
        boostHeld = boost;
    }

    void FixedUpdate()
    {
        if (!boostHeld) boostLatched = false;
        if (boostHeld && !boostLatched && moveInput.sqrMagnitude > 0.1f && Battery >= 0.12f && Time.time >= nextBoost)
        {
            IsBoosting = true;
            boostLatched = true;
            boostTime = 0f;
            BoostStarted?.Invoke();
        }
        if (IsBoosting)
        {
            boostTime += Time.fixedDeltaTime;
            Battery = Mathf.Max(0f, Battery - BatteryDrain * Time.fixedDeltaTime);
            if (!boostHeld || boostTime >= BoostDuration || Battery <= 0f || moveInput.sqrMagnitude < 0.01f)
            {
                IsBoosting = false;
                nextBoost = Time.time + 0.35f;
            }
        }
        var forward = Vector3.ProjectOnPlane(Camera.forward, up).normalized;
        var right = Vector3.Cross(up, forward);
        var direction = forward * moveInput.y + right * moveInput.x;
        bool grounded = IsGrounded;
        if (grounded) direction = Vector3.ProjectOnPlane(direction, groundNormal).normalized * moveInput.magnitude;
        float traction = grounded ? 1f : (IsBoosting ? 0.45f : 0.12f);
        var horizontal = Vector3.ProjectOnPlane(body.linearVelocity, up);
        float maxSpeed = IsBoosting ? BoostSpeed : CruiseSpeed;
        float throttle = Mathf.Clamp01((maxSpeed - Vector3.Dot(horizontal, direction.normalized)) / 1.5f);
        body.AddForce(direction * Speed * (IsBoosting ? 2.5f : 1f) * traction * throttle);
        if (horizontal.magnitude > maxSpeed)
            body.linearVelocity = Vector3.MoveTowards(horizontal, horizontal.normalized * maxSpeed, 14f * Time.fixedDeltaTime)
                + Vector3.Project(body.linearVelocity, up);
        if (jumpRequested && grounded && CanJump && Time.time - lastJump >= JumpDelay)
        {
            body.AddForce(up * JumpForce);
            lastJump = Time.time;
            lastGroundContact = -10f;
            Jumped?.Invoke();
        }
        jumpRequested = false;
        Head.position = body.position;
        Head.AddTorque(body.angularVelocity);
        var axis = Vector3.Cross(Head.transform.forward, up);
        var theta = Mathf.Asin(Mathf.Clamp01(axis.magnitude));
        var angular = axis.normalized * (theta / Time.fixedDeltaTime * TorqueScale);
        var rotation = Head.rotation * Head.inertiaTensorRotation;
        Head.AddTorque(rotation * Vector3.Scale(Head.inertiaTensor, Quaternion.Inverse(rotation) * angular));
        float angle = Vector3.Dot(Head.transform.right, body.linearVelocity.normalized) * Mathf.Rad2Deg * Time.fixedDeltaTime * 12f;
        Head.rotation *= Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void LateUpdate() { Head.transform.position = transform.position; }

    void OnCollisionStay(Collision collision)
    {
        foreach (var contact in collision.contacts)
            if (Vector3.Dot(contact.normal, up) > 0.5f)
            {
                lastGroundContact = Time.time;
                groundNormal = contact.normal;
                break;
            }
    }

    public bool AddEnergy(float amount)
    {
        if (Battery >= 0.995f || amount <= 0f) return false;
        Battery = Mathf.Clamp01(Battery + amount);
        Recharged?.Invoke();
        return true;
    }
}
