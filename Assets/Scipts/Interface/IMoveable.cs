using UnityEngine;

public interface IMoveable
{
    public float MoveMagnitude { get; }
    public bool IsGrounded { get; }
    public void MoveTo(Vector3 direction, float delta);
}
