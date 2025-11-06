using System.Collections.Generic;
using UnityEngine;

public class PlayerRegistry : MonoBehaviour
{
    public static PlayerRegistry Instance { get; private set; }

    private readonly Dictionary<int, PlayerData> players = new();
    private int nextPlayerId = 1; // Start from 1, 0 can be "invalid"

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep between scenes if needed
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Add player to registry with auto-generated ID
    public void RegisterPlayer(PlayerData data)
    {
        // Generate new ID and assign it
        int newId = GeneratePlayerId();
        data.PlayerId = newId;
        
        players.Add(newId, data);
        
        Debug.Log($"Player registered with ID: {newId}");
    }
    
    // Remove player
    public void UnregisterPlayer(PlayerData data)
    {
        if (players.Remove(data.PlayerId))
        {
            Debug.Log($"Player {data.PlayerId} unregistered");
        }
    }
    
    // Fetch a player by ID
    // public PlayerData GetPlayer(int playerId)
    // {
    //     players.TryGetValue(playerId, out PlayerData data);
    //     return data;
    // }

    // Fetch the local player (for singleplayer - gets first player)
    public PlayerData GetLocalPlayer()
    {
        // In singleplayer, just return the first player
        foreach (var kvp in players)
            return kvp.Value;

        return null;
    }

    // Get all players (useful for multiplayer scenarios)
    // public List<PlayerData> GetAllPlayers()
    // {
    //     return new List<PlayerData>(players.Values);
    // }

    // Auto-generate unique player ID
    private int GeneratePlayerId()
    {
        // Simple incremental ID - in real multiplayer you'd want something more robust
        return nextPlayerId++;
    }
}