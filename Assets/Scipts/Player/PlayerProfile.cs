using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProfile", menuName = "PlayerProfile", order = 0)]
public class PlayerProfile : ScriptableObject
{
    public new string name;
    public int playerId;
}
