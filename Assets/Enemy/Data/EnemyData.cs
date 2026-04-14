using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string EnemyName;
    public int MaxHealth;
    public float MoveSpeed;
    public int Damage;
    public GameObject Prefab;
}