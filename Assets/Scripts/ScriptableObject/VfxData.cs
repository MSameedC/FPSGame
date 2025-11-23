using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "VfxData", menuName = "Create Data/New Vfx Data")]
public class VfxData : ScriptableObject
{
    public VisualEffectAsset spawn;
    public VisualEffectAsset hurt;
}
