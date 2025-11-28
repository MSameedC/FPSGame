using UnityEngine;

[CreateAssetMenu(fileName = "EnemyProfile", menuName = "Create Data/New Enemy profile")]
public class EnemyData : ScriptableObject
{
    public int maxHealth;
    public float moveSpeed;
    public float detectRange;
    [Space]
    public int damage;
    public float attackRate;
    public Vector2 attackRange;
    [Space]
    public int scoreValue;
}