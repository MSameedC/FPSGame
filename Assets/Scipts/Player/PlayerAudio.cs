using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private float stepInterval;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip slamSound;

    private float timer;

    private AudioSource audioSource;

    // ---

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayFootStep(float delta)
    {
        if (timer < 0)
        {
            timer = stepInterval;
            audioSource.pitch = Random.Range(1, 1.15f);
            audioSource.clip = footstepSound;
            audioSource.Play();
        }
        else
        {
            timer -= delta;
        }
    }

    public void ResetFootStep()
    {
        timer = 0;
    }

    public void PlayDashSound()
    {
        audioSource.PlayOneShot(dashSound);
    }

    public void PlaySlamSound()
    {
        audioSource.PlayOneShot(slamSound);
    }
}
