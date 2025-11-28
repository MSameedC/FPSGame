using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Create Data/New Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [Header("Hit")]
    public AudioClip[] wallHitSound;
    public AudioClip[] criticalHitSound;
    public AudioClip explosionSound;
    
    [Header("Music")]
    public AudioClip backgroundMusic;
    public AudioClip combatMusic;
    
    [Header("Ambient")]
    public AudioClip ambientSound;
    
    [Header("UI")]
    public AudioClip uiClick;
    public AudioClip uiHover;
}
