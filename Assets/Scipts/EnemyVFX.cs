using System.Collections;
using UnityEngine;

public class EnemyVFX : MonoBehaviour
{
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    
    [SerializeField] private EnemyBase enemy;
    [Space]
    [SerializeField] private Material hurtMaterial;
    [Space]
    [SerializeField] private Color idleColor = Color.yellow;
    [SerializeField] private Color spottedColor = Color.red;
    
    private MaterialPropertyBlock propertyBlock;
    private MeshRenderer MeshRenderer;
    private Material baseMaterial;

    private float colorLerp;
    private bool isSpotted;
    
    // ---

    private void Awake()
    {
        MeshRenderer = GetComponentInChildren<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }
    
    private void Start()
    {
        baseMaterial = MeshRenderer.material;
        
        MeshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_EmissionColor", idleColor);
        MeshRenderer.SetPropertyBlock(propertyBlock);
        
        baseMaterial.EnableKeyword("_EMISSION");

        enemy.OnHurt += RenderHurtMaterial;
        enemy.OnPlayerSpotted += RenderHuntMaterial;
        enemy.OnPlayerLost += RenderDefaultMaterial;
    }

    private void OnDestroy()
    {
        enemy.OnHurt -= RenderHurtMaterial;
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

    public void RenderHuntMaterial() => isSpotted = true;
    public void RenderDefaultMaterial() => isSpotted = false;
    public void RenderHurtMaterial() => StartCoroutine(HurtCoroutine());
    
    private IEnumerator HurtCoroutine()
    {
        float delay = 0.15f;
        MeshRenderer.material = hurtMaterial;
        yield return new WaitForSeconds(delay);
        MeshRenderer.material = baseMaterial;
    }
}
