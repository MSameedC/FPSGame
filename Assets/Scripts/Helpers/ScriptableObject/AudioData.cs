using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "Create Data/New Audio Data")]
public class AudioData : ScriptableObject
{
    public AudioClip spawn;
    public AudioClip attack;
    public AudioClip hurt;
    public AudioClip death;
}
