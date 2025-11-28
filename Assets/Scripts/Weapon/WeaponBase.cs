using System;
using UnityEngine;
using static EnumManager;

public abstract class WeaponBase : MonoBehaviour
{
    public event Action<float, float> OnWeaponHeatChanged;
    public event Action OnOverheat;
    public event Action OnWeaponShoot;
    
    [SerializeField] protected FireMode fireMode = FireMode.Single;
    [Space]
    [SerializeField] protected WeaponData weaponData;
    [Space]
    [SerializeField] protected Transform muzzlePoint;
    
    // Weapon Heating
    private bool canCooldown;
    public float CurrentHeat { get; private set; }
    public float MaxHeat => weaponData.maxHeat;
    public bool OverHeating => CurrentHeat >= weaponData.maxHeat;

    // Weapon Shoot
    public float FireTimer { get; private set; }
    public bool IsShooting { get; private set; }
    public FireMode CurrentFireMode => fireMode;
    
    // Components
    private ProceduralManager proceduralManager;
    private WeaponState currentState;
    protected VFXManager vfxManager;
    protected AudioManager audioManager;

    // Camera
    protected Transform CamTransform;
    private Camera Cam;

    #region Unity

    private void Awake()
    {
        proceduralManager = GetComponent<ProceduralManager>();
        Initialize();
    }
    
    protected virtual void Start()
    {
        vfxManager = VFXManager.Instance;
        audioManager = AudioManager.Instance;
        
        SetState(new IdleState(this));
        canCooldown = true;
    }
    
    protected virtual void Update()
    {
        float delta = Time.deltaTime;
        
        if (FireTimer > 0f)
            FireTimer -= delta;

        UpdateHeatCooldown(delta);
        proceduralManager.SetShooting(IsShooting);
        currentState?.Update(delta);
    }

    #endregion

    private void Initialize()
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
        
        if (!weaponData)
        {
            Debug.LogError("WeaponData is missing!", this);
            return;
        }
        
        CamTransform = Cam.transform;
        proceduralManager.SetWeaponData(weaponData);
    }
    
    public void SetState(WeaponState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    #region Shoot

    public virtual void OnShoot()
    {
        if (!weaponData) return;
        IsShooting = true;

        FireTimer = weaponData.fireRate;
        CurrentHeat += weaponData.heatAddAmount;

        OnWeaponShoot?.Invoke();
    }

    public abstract void PerformShoot();
    
    public void ResetShooting()
    {
        IsShooting = false;
    }

    #endregion

    #region Heat System

    private void UpdateHeatCooldown(float delta)
    {
        if (!canCooldown || CurrentHeat <= 0) return;

        float cooldownRate = weaponData.heatDecayRate;
        if (!IsShooting) cooldownRate *= 1.5f;

        CurrentHeat -= weaponData.heatDecayAmount * cooldownRate * delta;
        CurrentHeat = Mathf.Max(0, CurrentHeat);

        OnWeaponHeatChanged?.Invoke(CurrentHeat, weaponData.maxHeat);
    }
    
    public void OnCoolDownEnter()
    {
        OnOverheat?.Invoke();
        canCooldown = false;
    }
    
    public void OnCoolDownExit()
    {
        canCooldown = true;
    }

    #endregion
    
    public bool CanShoot() => FireTimer <= 0f && !OverHeating;
    
    public WeaponData GetWeaponData() => weaponData;
}
