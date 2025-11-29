using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "VfxLibrary", menuName = "Create Data/New Vfx Library")]
public class VfxLibrary : ScriptableObject
{
    public VisualEffect spawn;
    public VisualEffect entityHit;
    public VisualEffect wallHitLight;
    public VisualEffect wallHitHeavy;
    public VisualEffect explosion;
    public VisualEffect death;
    public VisualEffect muzzleFlash;
}
