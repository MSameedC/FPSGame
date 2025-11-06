public abstract class PlayerState : BaseState
{
    public PlayerState(PlayerController player) : base(player) { }
    public virtual void OnDashInput() { }
    public virtual void OnSlamInput() { }
}