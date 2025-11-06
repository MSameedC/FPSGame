using UnityEngine;

public static class EntityHelper
{
    public static float GetDistance2D(Vector3 from, Vector3 to)
    {
        from.y = 0;
        to.y = 0;
        return Vector3.Distance(from, to);
    }
    
    public static Vector3 GetKnockbackDirection(Vector3 from, Vector3 to, float upwardRatio = 0.3f)
    {
        Vector3 direction = (to - from).normalized;
        direction.y += upwardRatio;
        return direction.normalized;
    }
    
    public static bool IsGrounded(Vector3 position, float radius, out RaycastHit hit , float checkDistance = 0.2f, LayerMask groundLayer = default)
    {
        Vector3 startPosition = position + Vector3.up * radius;
        return Physics.SphereCast(startPosition, radius, Vector3.down, out hit, checkDistance, groundLayer);
    }
}