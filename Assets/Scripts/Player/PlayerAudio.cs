using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private float stepInterval;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip slamSound;
    [SerializeField] private AudioClip landSound;
    
    private float stepTimer;
    private PlayerController player;
    private AudioManager AudioManager;

    // ---

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Start()
    {
        AudioManager = AudioManager.Instance;
        player.OnSlamImpact += () => AudioManager.PlaySFX(slamSound);
        player.OnLanded += () => AudioManager.PlaySFX(landSound);
        player.OnDash += () => AudioManager.PlaySFX(dashSound);
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
            AudioManager.PlaySFX(footstepSound, 1, 1, 1.2f);
        }
        else
        {
            stepTimer -= delta;
        }
    }
}