using UnityEngine;
[CreateAssetMenu(menuName="BB8/Reacción")]
public sealed class SocialReaction : ScriptableObject
{
    public string Id;
    public AudioClip Sound;
    public float Duration = 3f;
    public float HeadTilt = 12f;
    public float HeadSpeed = 3f;
    public float Movement = 0f;
    public float Pitch = 1f;
    public bool Bounce;
}
