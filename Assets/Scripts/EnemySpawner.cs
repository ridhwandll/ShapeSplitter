using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public GameObject shortRangeEnemyPrefab;
    public GameObject longRangeEnemyPrefab;
    public GameObject bigChonkEnemyPrefab;
    
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("Base Spawning")]
    public float baseMinSpawnDelay = 1.5f;
    public float baseMaxSpawnDelay = 3f;
    public int baseMaxEnemies = 10;

    [Header("Difficulty Scaling")]
    public float difficultyInterval = 10.0f; // seconds per level
    public float spawnDelayDecreasePerLevel = 0.1f;
    public int maxEnemiesIncreasePerLevel = 2;

    private int _currentEnemies;
    private bool _paused;
    private int _difficultyLevel = 1;
    private float _difficultyTimer;
    public Action<int> OnDifficultyChanged;
    
    private bool _canSpawnBigChonk = true;
    
    void Start()
    {
        GameManager.Instance.OnPauseChanged += OnGamePaused;
        StartCoroutine(SpawnRoutine());
        _canSpawnBigChonk = true;
    }

    void Update()
    {
        if (_paused || !GameManager.Instance.IsPlayerAlive)
            return;

        _difficultyTimer += Time.deltaTime;

        if (_difficultyTimer >= difficultyInterval)
        {
            _difficultyTimer = 0f;
            _difficultyLevel++;
            OnDifficultyChanged?.Invoke(_difficultyLevel);
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (GameManager.Instance.IsPlayerAlive)
        {
            yield return new WaitWhile(() => _paused);

            int scaledMaxEnemies = baseMaxEnemies + (_difficultyLevel - 1) * maxEnemiesIncreasePerLevel;

            if (_currentEnemies < scaledMaxEnemies)
                SpawnEnemy();

            float minDelay = Mathf.Max(0.2f, baseMinSpawnDelay - (_difficultyLevel - 1) * spawnDelayDecreasePerLevel);
            float maxDelay = Mathf.Max(minDelay + 0.1f, baseMaxSpawnDelay - (_difficultyLevel - 1) * spawnDelayDecreasePerLevel);
            float delay = Random.Range(minDelay, maxDelay);
            
            yield return new WaitForSeconds(delay);
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = new Vector2(
            Random.Range(minBounds.x, maxBounds.x),
            Random.Range(minBounds.y, maxBounds.y)
        );
        
        float rand = Random.value; // 0 to 1
        GameObject enemyPrefab;
        EnemyType enemyType;

        if (rand < 0.65f) // 65% chance for short range
        {
            enemyPrefab = shortRangeEnemyPrefab;
            enemyType = EnemyType.ShortRanged;
        }
        else if (rand < 0.95f) // next 30% for long range
        {
            enemyPrefab = longRangeEnemyPrefab;
            enemyType = EnemyType.LongRanged;
        }
        else // last 5% for big chonk
        {
            if (_canSpawnBigChonk) // Allow only one big chonk
            {
                enemyPrefab = bigChonkEnemyPrefab;
                enemyType = EnemyType.BigChonk;
                _canSpawnBigChonk = false;
            }
            else // spawn a long range instead
            {
                enemyPrefab = longRangeEnemyPrefab;
                enemyType = EnemyType.LongRanged;
            }
        }
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);
        enemy.GetComponent<Enemy>().Initiate(this, enemyType);
        _currentEnemies++;
    }

    public int GetDifficultyLevel()
    {
        return _difficultyLevel;
    }
    
    public void OnEnemyDied(EnemyType type)
    {
        if (type == EnemyType.BigChonk)
            _canSpawnBigChonk = true;
        
        _currentEnemies--;
    }

    private void OnGamePaused(bool isPaused)
    {
        _paused = isPaused;
    }
}