public abstract class PlayerState : BaseState
{
    protected PlayerState(PlayerController player) : base(player) { }
    public virtual void OnDashInput() { }
    public virtual void OnSlamInput() { }
}