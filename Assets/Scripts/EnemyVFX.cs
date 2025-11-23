using System.Collections;
using UnityEngine;

public class EnemyVFX : MonoBehaviour
{
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    
    [SerializeField] private VfxData vfxData;
    [Header("Hurt Material")]
    [SerializeField] private Material hurtMaterial;
    [Space]
    [SerializeField] private Color idleColor = Color.yellow;
    [SerializeField] private Color spottedColor = Color.red;
    
    private MaterialPropertyBlock propertyBlock;
    private MeshRenderer MeshRenderer;
    private VFXManager VFXManager;
    private Material baseMaterial;
    private EnemyBase enemy;

    private float colorLerp;
    private bool isSpotted;
    
    // ---

    private void Awake()
    {
        enemy = GetComponent<EnemyBase>();
        MeshRenderer = GetComponentInChildren<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }
    
    private void Start()
    {
        VFXManager = VFXManager.Instance;
        baseMaterial = MeshRenderer.material;
        
        MeshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(EmissionColor, idleColor);
        MeshRenderer.SetPropertyBlock(propertyBlock);
        
        baseMaterial.EnableKeyword("_EMISSION");
        
        if (!enemy) return;
        
        enemy.OnSpawned += PerformSpawnEffect;
        enemy.OnHurt += PerformHurtEffect;
        enemy.OnPlayerSpotted += RenderHuntMaterial;
        enemy.OnPlayerLost += RenderDefaultMaterial;
    }

    private void OnDestroy()
    {
        if (!enemy) return;
        enemy.OnSpawned -= PerformSpawnEffect;
        enemy.OnHurt -= PerformHurtEffect;
        enemy.OnPlayerSpotted -= RenderHuntMaterial;
        enemy.OnPlayerLost -= RenderDefaultMaterial;
    }

    private void Update()
    {
        // Smoothly lerp the emission color
        colorLerp = Mathf.MoveTowards(colorLerp, isSpotted ? 1f : 0f, 3 * Time.deltaTime);
        
        Color currentEmission = Color.Lerp(idleColor, spottedColor, colorLerp);
        
        MeshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(EmissionColor, currentEmission);
        MeshRenderer.SetPropertyBlock(propertyBlock);
    }
    
    private void PerformHurtEffect()
    {
        StartCoroutine(HurtCoroutine());
        VFXManager?.PlayVFX(vfxData.hurt, transform.position, Quaternion.identity);
    }
    private void PerformSpawnEffect()
    {
        VFXManager?.PlayVFX(vfxData.spawn, transform.position, Quaternion.identity);
    }

    private void RenderHuntMaterial() => isSpotted = true;
    private void RenderDefaultMaterial() => isSpotted = false;
    
    private IEnumerator HurtCoroutine()
    {
        float delay = 0.15f;
        MeshRenderer.material = hurtMaterial;
        yield return new WaitForSeconds(delay);
        MeshRenderer.material = baseMaterial;
    }
}
