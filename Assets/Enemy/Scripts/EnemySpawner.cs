using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemyData> enemyPool;
    [SerializeField] private Transform playerCore;
    [SerializeField] private List<Transform> playerPieces;

    private void Start()
    {
        SpawnWave(enemyPool);
    }

    public void Spawn(EnemyData data, Vector2 position)
    {
        GameObject obj = Instantiate(data.Prefab, position, Quaternion.identity);
        obj.transform.SetParent(transform);

        EnemyBase enemy = obj.GetComponent<EnemyBase>();
        enemy.Init(playerCore, playerPieces);
    }

    public void SpawnWave(List<EnemyData> wave)
    {
        foreach (var data in wave)
        {
            Vector2 spawnPos = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            Spawn(data, spawnPos);
        }
    }
}