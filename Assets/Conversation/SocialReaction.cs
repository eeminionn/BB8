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
    [Header("Expresión por intensidad (1, 2, 3)")]
    public AudioClip[] IntensitySounds;
    public string[] DroidWords;
    public string[] Feelings;
    public Vector3 Travel = new Vector3(.6f, 1.8f, 3.6f);
    public Vector3 Durations = new Vector3(2.6f, 3.8f, 5.2f);
    public AudioClip Clip(int level) => IntensitySounds != null && IntensitySounds.Length >= level && IntensitySounds[level-1] ? IntensitySounds[level-1] : Sound;
    public string Words(int level) => DroidWords != null && DroidWords.Length >= level ? DroidWords[level-1] : "beep-boop";
    public string Feeling(int level) => Feelings != null && Feelings.Length >= level ? Feelings[level-1] : "Atento";
}
