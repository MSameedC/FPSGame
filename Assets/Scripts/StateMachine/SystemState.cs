public abstract class SystemState
{
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(float delta) { }
    public virtual void LateUpdate(float delta) { }
}
