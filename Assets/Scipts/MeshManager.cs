using System.Collections;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    [SerializeField] private Material hurtMaterial;
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    private Material baseMaterial;

    private void Start()
    {
        baseMaterial = meshRenderer.material;
    }

    public void RenderHurtMaterial()
    {
        StartCoroutine(HurtCoroutine());
    }
    
    private IEnumerator HurtCoroutine()
    {
        meshRenderer.material = hurtMaterial;
        yield return new WaitForSeconds(0.15f);
        meshRenderer.material = baseMaterial;
    }
}
