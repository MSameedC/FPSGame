public abstract class EnemyState
{
    protected EnemyBase enemy;
    protected EnemyState(EnemyBase enemy)
    {
        this.enemy = enemy;
    }
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(float delta) { }
}
