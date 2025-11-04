public interface IPlayerData
{
    float CurrentHeat { get; }
    float MaxHeat { get; }
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsAlive { get; }
}
