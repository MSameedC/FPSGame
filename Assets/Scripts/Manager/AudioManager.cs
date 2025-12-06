using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Library")]
    [SerializeField] private AudioLibrary library;
    public AudioLibrary Library => library;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource sfxSource;
    
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
        // Configure sources
        musicSource.loop = true;
        ambientSource.loop = true;
        
        // Start ambient music
        PlayAmbient();
    }
    
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
        StartCoroutine(ReturnVFXAfterDelay(audioObject, 5));
    }
    
    private IEnumerator ReturnVFXAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolSystem.Return(PoolName.AudioPool, obj);
    }
    
    public AudioClip GetRandomClip(AudioClip[] clips) => clips.Length > 0 ? clips[Random.Range(0, clips.Length)] : null;
    
    public void PlayExplosionSound(Vector3 pos) => PlaySFX(library.explosionSound, pos, Quaternion.identity);
    
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
    
    // Private helpers
    private void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }
}