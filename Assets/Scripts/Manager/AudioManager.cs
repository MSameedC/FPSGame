using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Library")]
    [SerializeField] private AudioLibrary library;
    public AudioLibrary Library => library;
    
    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource sfxSource;
    
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        InitializeAudioSources();
        PoolSystem.CreatePool(PoolName.AudioPool, sfxSource.gameObject, 10);
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
    public void PlaySFX(AudioClip clip, Vector3 pos, Quaternion rot, float volume = 1f, float pitchMin = 1f, float pitchMax = 1f)
    {
        if (!PoolSystem.HasAvailable(PoolName.AudioPool))
        {
            PoolSystem.Add(PoolName.AudioPool, sfxSource.gameObject, 5);
        }
        GameObject audioObject = PoolSystem.Get(PoolName.AudioPool, pos, rot);
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();
        audioSource.pitch = Random.Range(pitchMin, pitchMax);
        audioSource.PlayOneShot(clip, volume);
    }
    
    public AudioClip GetRandomClip(AudioClip[] clips) => clips.Length > 0 ? clips[Random.Range(0, clips.Length)] : null;
    
    // Music management
    public void PlayBackgroundMusic() => PlayMusic(library.backgroundMusic);
    public void PlayCombatMusic() => PlayMusic(library.combatMusic);
    public void StopMusic() => musicSource.Stop();
    
    // Ambient management
    public void PlayAmbient() 
    { 
        ambientSource.clip = library.ambientSound;
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