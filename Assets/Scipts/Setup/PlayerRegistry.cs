using System.Collections.Generic;
using UnityEngine;

public class PlayerRegistry : MonoBehaviour
{
    public static PlayerRegistry Instance { get; private set; }

    private readonly Dictionary<int, PlayerData> players = new();

    private void Awake()
    {
        Instance = this;
    }

    // Add player to registry
    public void RegisterPlayer(PlayerProfile profile, PlayerData data)
    {
        if (!players.ContainsKey(profile.playerId))
            players.Add(profile.playerId, data);
    }

    // Remove player (in case of death/disconnect)
    public void UnregisterPlayer(PlayerProfile profile)
    {
        players.Remove(profile.playerId);
    }

    // Fetch a player by ID
    public PlayerData GetPlayer(int playerId)
    {
        players.TryGetValue(playerId, out var data);
        return data;
    }

    // Fetch the local player (singleplayer = just the first)
    public PlayerData GetLocalPlayer()
    {
        foreach (var kvp in players)
            return kvp.Value;

        return null;
    }
}