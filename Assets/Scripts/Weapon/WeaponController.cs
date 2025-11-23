using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public enum FireMode { Single, Burst, Auto }

public class WeaponController : MonoBehaviour
{
    #region Events
    public event Action<float, float> OnWeaponHeatChanged;
    #endregion
    
    [SerializeField] private FireMode currentFireMode = FireMode.Single;
    
    [Header("Weapon Data")]
    [SerializeField] private UpgradeableWeaponData upgradedWeaponData;
    
    [Header("Visual Effects")]
    [SerializeField] private VisualEffect muzzleFlash;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private SliderHandler sliderHandler;
    [SerializeField] private Animator animator;
    [SerializeField] private LineRenderer lineRenderer;
    
    private const string poolId = PoolName.BulletTrailPool;
    
    // Weapon Heating
    private bool canCooldown;
    public float CurrentHeat { get; private set; }
    public bool OverHeating => CurrentHeat >= upgradedWeaponData.MaxHeat;
    
    // Audio
    private float lastAudioTime;
    private bool CanPlayAudio => !audioSource.isPlaying || Time.time > lastAudioTime + 0.1f;
    private AudioSource audioSource;

    // Weapon Shoot
    public float FireTimer { get; private set; }
    public bool IsShooting { get; private set; }
    public FireMode CurrentFireMode => currentFireMode;

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
    }

    private void Start()
    {
        PoolSystem.CreatePool(poolId, lineRenderer.gameObject, 10);
        SetState(new IdleState(this));
        canCooldown = true;
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        
        if (FireTimer > 0f)
            FireTimer -= delta;

        UpdateHeatCooldown(delta);
        proceduralManager.SetShooting(IsShooting);
        currentState?.Update(delta);
    }

    #endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
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
        
        CamTransform = Cam.transform;
        baseData = upgradedWeaponData.baseData;
        proceduralManager.SetWeaponData(baseData);
        upgradedWeaponData.Initialize();
        lineRenderer.gameObject.SetActive(false);
    }
    
    #endregion

    #region State Machine

    public void SetState(WeaponState newState)
    {
        currentState?.OnReloadPressed();
        currentState = newState;
        currentState.Enter();
    }

    #endregion

    #region Shooting Logic
    
    public void OnShoot()
    {
        if (!upgradedWeaponData.baseData) return;
        
        IsShooting = true;

        FireTimer = upgradedWeaponData.FireRate;
        CurrentHeat += upgradedWeaponData.baseData.heatAddAmount;

        PlayGunShotFeedback();

        Vector3 origin = CamTransform.position;
        Vector3 direction = CamTransform.forward;
        Vector3 hitPoint = origin + direction * baseData.range;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, baseData.range))
        {
            hitPoint = hit.point;
            
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            
            if (damageable != null)
            {
                damageable.TakeDamage(baseData.damage);
            }
        }
        
        ShowTrail(muzzlePoint.position, hitPoint);
    }

    private void ShowTrail(Vector3 start, Vector3 end)
    {
        GameObject trail = PoolSystem.Get(poolId, start, Quaternion.identity);
        LineRenderer line = trail.GetComponent<LineRenderer>();
        
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    
        // Instead of coroutine, use simple destroy
        StartCoroutine(ReturnTrailAfterDelay(trail, 0.05f));
    }
    
    private IEnumerator ReturnTrailAfterDelay(GameObject trail, float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolSystem.Return(poolId, trail);
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
        
        sliderHandler?.AnimateSlide();
        
        muzzleFlash.transform.position = muzzlePoint.position;
        muzzleFlash.Play();
        
        animator.SetTrigger(AnimationName.Shoot);
    }
    
    #endregion
    
    #region Heat Management
    
    private void UpdateHeatCooldown(float delta)
    {
        if (!canCooldown || CurrentHeat <= 0) return;

        float cooldownRate = upgradedWeaponData.HeatDecayRate;
        if (!IsShooting) cooldownRate *= 1.5f;

        CurrentHeat -= upgradedWeaponData.baseData.heatDecayAmount * cooldownRate * delta;
        CurrentHeat = Mathf.Max(0, CurrentHeat);

        OnWeaponHeatChanged?.Invoke(CurrentHeat, upgradedWeaponData.MaxHeat);
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
    
    #region Helpers

    private void PlayOneShot(AudioClip clip)
    {
        audioSource.pitch = Random.Range(1.0f, 1.13f);
        audioSource.PlayOneShot(clip);
    }

    #endregion

    #region Bools
    
    public bool CanShoot() => FireTimer <= 0f && !OverHeating;

    #endregion

    #region Accessors
    
    public WeaponData GetWeaponData() => upgradedWeaponData.baseData;
    public UpgradeableWeaponData GetUpgradedData() => upgradedWeaponData;

    #endregion
}