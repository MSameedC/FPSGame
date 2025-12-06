using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private float stepInterval;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip slamSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip hurtSound;
    
    private float stepTimer;
    private PlayerController player;
    private PlayerHealth health;
    private AudioManager AudioManager;

    // ---

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        health = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        AudioManager = AudioManager.Instance;
        player.OnSlamImpact += () => PlayOneShot(slamSound);
        player.OnJumped += () => PlayOneShot(jumpSound, 1, 1, 1.1f);
        player.OnLanded += () => PlayOneShot(landSound);
        player.OnDash += () => PlayOneShot(dashSound);
        health.OnHealed += () => PlayOneShot(healSound);
        health.OnHurt += () => PlayOneShot(hurtSound);
    }

    private void Update()
    {
        if (!player.IsGrounded) return;
        
        if (InputManager.MoveInput != Vector2.zero)
            PlayFootStep(Time.deltaTime);
        else
            stepTimer = 0;
    }

    private void PlayFootStep(float delta)
    {
        if (stepTimer < 0)
        {
            stepTimer = stepInterval;
            PlayOneShot(footstepSound, 1, 1, 1.2f);
        }
        else
        {
            stepTimer -= delta;
        }
    }

    private void PlayOneShot(AudioClip clip, float volume = 1f, float minPitch  = 1f, float maxPitch  = 1f)
    {
        AudioManager.PlaySFX(clip, transform.position, Quaternion.identity, volume, minPitch, maxPitch);
    }
}