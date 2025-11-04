public interface IPlayerState
{
    public bool IsGrounded { get; }
    public bool IsDashing { get; }
    public bool IsSlaming { get; }
    public float MoveMagnitude { get; }
}