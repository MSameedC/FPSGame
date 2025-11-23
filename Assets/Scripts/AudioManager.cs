using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip combatMusic;
    
    [Header("Ambient")]
    [SerializeField] private AudioClip ambientSound;
    
    [Header("UI")]
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip uiHover;
    
    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource sfxSource;
    
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        InitializeAudioSources();
    }
    
    private void InitializeAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        ambientSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        
        // Configure sources
        musicSource.loop = true;
        ambientSource.loop = true;
        
        // Start ambient music
        PlayAmbient();
    }
    
    // === PUBLIC INTERFACE ===
    
    // One-shot sounds (for player, enemies, UI)
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitchMin = 1f, float pitchMax = 1f)
    {
        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, volume);
    }
    
    // Music management
    public void PlayBackgroundMusic() => PlayMusic(backgroundMusic);
    public void PlayCombatMusic() => PlayMusic(combatMusic);
    public void StopMusic() => musicSource.Stop();
    
    // Ambient management
    public void PlayAmbient() 
    { 
        ambientSource.clip = ambientSound;
        ambientSource.Play();
    }
    public void StopAmbient() => ambientSource.Stop();
    
    // Volume control
    public void SetMusicVolume(float volume) => musicSource.volume = volume;
    public void SetSFXVolume(float volume) => sfxSource.volume = volume;
    public void SetAmbientVolume(float volume) => ambientSource.volume = volume;
    
    // Private helpers
    private void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }
}