using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip hurtSound;
    
    private EnemyBase enemy;
    private AudioSource audioSource;
    
    // ---
    
    private void Awake()
    {
        enemy = GetComponent<EnemyBase>();
        audioSource = GetComponent<AudioSource>();
    }
    
    private void Start()
    {
        enemy.OnSpawned += () => PlayOneShot(spawnSound);
        enemy.OnAttack += () => PlayOneShot(attackSound);
        enemy.OnHurt += () => PlayOneShot(hurtSound);
        enemy.OnDeath += () => PlayOneShot(deathSound);
    }
        
    private void PlayOneShot(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
