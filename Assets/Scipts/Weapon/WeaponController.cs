using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class WeaponController : MonoBehaviour
{
    #region Events

    // Weapon Heat
    public event Action<float, float> OnWeaponHeatChanged;
    public event Action OnWeaponOverheated;
    public event Action OnWeaponCooldown;

    // Weapon Shoot
    public event Action<float> OnRecoilRequested;
    public event Action OnWeaponFired;

    // Weapon Hit
    public event Action<IDamageable> OnTargetHit;

    #endregion
    
    [SerializeField] private TrailRenderer trailRenderer;
    [Space]
    [SerializeField] private UpgradeableWeaponData upgradedWeaponData;
    [Space]
    [SerializeField] private VisualEffect muzzleFlash;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private SliderHandler sliderHandler;

    [Header("Flags")]
    public bool isAutomatic;

    // Weapon Heating
    private bool canCooldown;
    public float CurrentHeat { get; private set; }
    public bool OverHeating => CurrentHeat >= upgradedWeaponData.MaxHeat;
    
    // Bullet Trail
    private const int trailPoolSize = 20;
    private readonly Queue<TrailRenderer> trailPool = new();
    
    // Audio
    private float lastAudioTime;
    private bool CanPlayAudio => !audioSource.isPlaying || Time.time > lastAudioTime + 0.1f;
    private AudioSource audioSource;

    // Weapon Shoot
    public float FireTimer { get; private set; }
    public bool IsShooting { get; private set; }

    // Components
    private ProceduralManager proceduralManager;
    private WeaponState currentState;
    private WeaponData baseData;

    // Camera
    private Transform CamTransform;
    private Camera Cam;

    // ---

    #region Unity Methods

    private void Awake()
    {
        proceduralManager = GetComponent<ProceduralManager>();
        audioSource = GetComponent<AudioSource>();
        InitializeComponents();
        trailRenderer.gameObject.SetActive(false);
    }
    private void Start()
    {
        InitializeTrailPool();
        
        SetState(new IdleState(this));
        canCooldown = true;
    }
    private void Update()
    {
        float delta = Time.deltaTime;
        // Timer
        if (FireTimer > 0f)
            FireTimer -= delta;

        UpdateHeatCooldown(delta);

        proceduralManager.SetShooting(IsShooting);
        currentState?.Update(delta);
    }

    #endregion
    
    #region Methods
    
    private void InitializeComponents()
    {
        // Confirm components are present
        if (!Cam)
        {
            Cam = Camera.main;
            
            if (!Cam)
            {
                Debug.LogError("No camera found!", this);
                return;
            }
        }
        
        if (!upgradedWeaponData.baseData)
        {
            Debug.LogError("WeaponData is missing!", this);
            return;
        }
        // Initialize
        CamTransform = Cam.transform;
        baseData = upgradedWeaponData.baseData;
        proceduralManager.SetWeaponData(baseData);
        upgradedWeaponData.Initialize();
    }
    private void InitializeTrailPool()
    {
        GameObject trailParent = new GameObject("BulletTrailPool");
        for (int i = 0; i < trailPoolSize; i++)
        {
            var trail = Instantiate(trailRenderer, trailParent.transform);
            trail.gameObject.SetActive(false);
            trailPool.Enqueue(trail);
        }
    }
    
    #endregion

    #region Input Calls

    public void OnShootPressed() => currentState?.OnShootPressed();
    public void OnShootCancelled() => currentState?.OnShootReleased();

    #endregion

    #region State Machine

    public void SetState(WeaponState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    #endregion

    #region Behaviour
    
    #region Shoot
    
    public void OnShoot()
    {
        if (!upgradedWeaponData.baseData) return;
        
        IsShooting = true;
        OnWeaponFired?.Invoke();

        FireTimer = upgradedWeaponData.FireRate;
        CurrentHeat += upgradedWeaponData.baseData.heatAddAmount;

        PlayGunShotFeedback();

        Vector3 origin = CamTransform.position;
        Vector3 direction = CamTransform.forward;
        Vector3 hitPoint = origin + direction * baseData.range;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, baseData.range))
        {
            hitPoint = hit.point;
            // Interact
            var damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage((int)upgradedWeaponData.Damage);
                OnTargetHit?.Invoke(damageable);
            }
        }
        // Bullet Trail
        ShowTrail(muzzlePoint.position, hitPoint);
        if (OverHeating) OnWeaponOverheated?.Invoke();
    }
    public void ResetShooting()
    {
        IsShooting = false;
    }
    private void PlayGunShotFeedback()
    {
        if (CanPlayAudio)
        {
            PlayOneShot(upgradedWeaponData.baseData.sound.fire);
            lastAudioTime = Time.time;
        }

        // Recoil
        OnRecoilRequested?.Invoke(upgradedWeaponData.baseData.recoil.kick);
        // Procedural Animations
        sliderHandler?.AnimateSlide();
        // VFX
        muzzleFlash.transform.position = muzzlePoint.position; // On aim position changes, will need to be updated
        muzzleFlash.Play();
    }
    
    #endregion
    
    #region CoolDown
    
    private void UpdateHeatCooldown(float delta)
    {
        if (!canCooldown || CurrentHeat <= 0) return;

        float cooldownRate = upgradedWeaponData.HeatDecayRate;

        // Faster cooldown when not shooting
        if (!IsShooting) cooldownRate *= 1.5f;

        CurrentHeat -= upgradedWeaponData.baseData.heatDecayAmount * cooldownRate * delta;
        CurrentHeat = Mathf.Max(0, CurrentHeat);

        OnWeaponHeatChanged?.Invoke(CurrentHeat, upgradedWeaponData.MaxHeat);
        OnWeaponCooldown?.Invoke();
    }
    public void OnCoolDownEnter()
    {
        PlayOneShot(upgradedWeaponData.baseData.sound.overheat);
        canCooldown = false;
    }
    public void OnCoolDownExit()
    {
        canCooldown = true;
    }
    
    #endregion
    
    #region Procedural Components
    
    public void SetSliderLocked(bool locked) => sliderHandler.SetLocked(locked);
    
    #endregion
    
    #region Effects
    
    private void ShowTrail(Vector3 start, Vector3 end)
    {
        if (trailPool.Count == 0) return;
        
        var trail = trailPool.Dequeue();
        trail.gameObject.SetActive(true);
        trail.transform.position = start;
        
        StartCoroutine(AnimateTrail(trail, end));
    }
    private IEnumerator AnimateTrail(TrailRenderer trail, Vector3 endPoint)
    {
        Vector3 startPos = trail.transform.position;
        float distance = Vector3.Distance(startPos, endPoint);
        float speed = 100f; // Constant speed in units per second
        float duration = distance / speed; // Time based on distance
        
        float time = 0;
    
        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPos, endPoint, time);
            time += Time.deltaTime / duration; // Now duration scales with distance
            yield return null;
        }
    
        trail.transform.position = endPoint;
    
        // Wait for trail to fade naturally
        yield return new WaitForSeconds(trail.time);
    
        // Return to pool
        trail.Clear();
        trail.gameObject.SetActive(false);
        trailPool.Enqueue(trail);
    }
    
    #endregion

    #endregion

    #region Bools

    public bool IsAiming() => InputManager.IsAiming;
    public bool CanShoot() => FireTimer <= 0f && !OverHeating && InputManager.IsShooting;

    #endregion

    #region Helpers

    private void PlayOneShot(AudioClip clip)
    {
        audioSource.pitch = Random.Range(1.0f, 1.13f);
        audioSource.PlayOneShot(clip);
    }

    #endregion

    #region Accessors
    
    public WeaponData GetWeaponData() => upgradedWeaponData.baseData;
    public UpgradeableWeaponData GetUpgradedData() => upgradedWeaponData;

    #endregion
}
