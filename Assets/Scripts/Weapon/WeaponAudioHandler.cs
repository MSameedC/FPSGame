using System;
using UnityEngine;

public class WeaponAudioHandler : MonoBehaviour
{
    [SerializeField] private WeaponBase weapon;
    
    [Header("Clips")]
    [SerializeField] private AudioClip shoot;
    [SerializeField] private AudioClip overheat;
    
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        weapon.OnWeaponShoot += () => audioSource.PlayOneShot(shoot);
        weapon.OnOverheat += () => audioSource.PlayOneShot(overheat);
    }
}
