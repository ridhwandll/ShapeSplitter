using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;


    public Vector2 minBounds;
    public Vector2 maxBounds;

    public float minSpawnDelay = 1f;
    public float maxSpawnDelay = 3f;
    
    public int maxEnemies = 20;

    private int _currentEnemies;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (!Constants.IsPlayerAlive)
                break;
            
            if (_currentEnemies < maxEnemies)
                SpawnEnemy();

            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
        }
    }

    void SpawnEnemy()
    {
        
        Vector2 spawnPos = new Vector2(Random.Range(minBounds.x, maxBounds.x), Random.Range(minBounds.y, maxBounds.y));

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        enemy.transform.SetParent(transform);
        _currentEnemies++;
    }
}