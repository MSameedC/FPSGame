public static class PlayerUI
{
    // Quick access to local player data for UI
    public static PlayerData LocalPlayer => PlayerRegistry.Instance?.GetLocalPlayer();
    
    // Helper properties for direct UI binding
    public static int Health => LocalPlayer?.CurrentHealth ?? 0;
    public static int MaxHealth => LocalPlayer?.MaxHealth ?? 100;
    public static float HealthPercent => (float)Health / MaxHealth;
}