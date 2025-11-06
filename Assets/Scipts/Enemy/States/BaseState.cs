public abstract class BaseState
{
    protected readonly IMoveable entity;
    protected BaseState(IMoveable entity)
    {
        this.entity = entity;
    }
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(float delta) { }
}
