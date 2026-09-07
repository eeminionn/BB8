using UnityEngine;

[RequireComponent(typeof(BbRigidbodyController), typeof(AudioSource))]
public sealed class BB8Personality : MonoBehaviour
{
    public Vector3 SocialHeadPose;
    public Transform HeadVisual;
    public Transform[] Antennas;
    public AudioClip[] JumpSounds;
    public AudioClip[] BumpSounds;
    public AudioClip BoostSound;
    public AudioClip RechargeSound;
    BbRigidbodyController controller;
    Rigidbody body;
    AudioSource voice;
    float idleBlend, wobble, wobbleVelocity, nextBump;
    Vector3 lastVelocity;
    Quaternion[] antennaRest;

    void Awake()
    {
        controller = GetComponent<BbRigidbodyController>();
        body = GetComponent<Rigidbody>();
        voice = GetComponent<AudioSource>();
        voice.playOnAwake = false;
        voice.spatialBlend = 0.35f;
        voice.minDistance = 2f;
        voice.maxDistance = 22f;
        antennaRest = new Quaternion[Antennas.Length];
        for (int i = 0; i < Antennas.Length; i++) antennaRest[i] = Antennas[i].localRotation;
    }
    void OnEnable()
    {
        controller.Jumped += Jump;
        controller.BoostStarted += Boost;
        controller.Recharged += Recharge;
    }
    void OnDisable()
    {
        controller.Jumped -= Jump;
        controller.BoostStarted -= Boost;
        controller.Recharged -= Recharge;
    }
    void Play(AudioClip clip, float volume)
    {
        if (clip == null) return;
        voice.pitch = Random.Range(0.94f, 1.06f);
        voice.PlayOneShot(clip, volume);
    }
    public void Speak(AudioClip clip, float pitch = 1f) { if (!clip) return; voice.Stop(); voice.pitch = pitch; voice.PlayOneShot(clip, 0.55f); wobbleVelocity += 25f; }
    AudioClip Pick(AudioClip[] clips) => clips.Length == 0 ? null : clips[Random.Range(0, clips.Length)];
    void Jump() { wobbleVelocity += 35f; Play(Pick(JumpSounds), 0.52f); }
    void Boost() { wobbleVelocity += 22f; Play(BoostSound, 0.32f); }
    void Recharge() { Play(RechargeSound, 0.42f); }
    void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        if (impact < 2.6f || Time.time < nextBump) return;
        nextBump = Time.time + 0.6f;
        wobbleVelocity += Mathf.Min(impact * 6f, 50f);
        Play(Pick(BumpSounds), Mathf.Lerp(0.2f, 0.55f, Mathf.InverseLerp(2.6f, 9f, impact)));
    }
    void LateUpdate()
    {
        float dt = Mathf.Min(Time.deltaTime, 0.035f);
        float speed = body.linearVelocity.magnitude;
        idleBlend = Mathf.MoveTowards(idleBlend, speed < 0.25f && controller.IsGrounded ? 1f : 0f, dt * 1.5f);
        float t = Time.time;
        if (HeadVisual != null)
            HeadVisual.localRotation = Quaternion.Euler(idleBlend * Mathf.Sin(t * 0.8f) * 2.2f,
                idleBlend * Mathf.Sin(t * 1.1f + 1f) * 1.6f,
                idleBlend * (Mathf.Sin(t * 0.53f) * 12f + Mathf.Sin(t * 0.21f) * 5f)) * Quaternion.Euler(SocialHeadPose);
        float acceleration = Mathf.Clamp((body.linearVelocity - lastVelocity).magnitude, 0f, 2f);
        lastVelocity = body.linearVelocity;
        wobbleVelocity += (acceleration * 14f - wobble * 130f - wobbleVelocity * 9f) * dt;
        wobble = Mathf.Clamp(wobble + wobbleVelocity * dt, -10f, 10f);
        for (int i = 0; i < Antennas.Length; i++)
        {
            float sway = Mathf.Sin(t * (16f + i * 3f)) * Mathf.Min(speed * 0.18f, 2.4f);
            Antennas[i].localRotation = antennaRest[i] * Quaternion.Euler((wobble + sway) * (i == 0 ? 1f : 0.65f), sway * 0.6f, 0f);
        }
    }
}
