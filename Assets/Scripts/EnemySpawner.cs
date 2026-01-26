using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public enum DifficultyLevel
{
    Easy = 0,
    Medium = 1,
    Hard = 2,
    Impossible = 3
}

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
    public float difficultyInterval = 25.0f; // seconds per level
    public float spawnDelayDecreasePerWave = 0.1f;
    public int maxEnemiesIncreasePerWave = 2;

    private int _currentEnemies;
    private bool _paused;
    private int _wave = 1;
    private float _waveTimer;
    public Action<int> OnDifficultyChanged;
    public Action OnEnemyKilled;
    
    private bool _canSpawnBigChonk = true;
    
    private int _score;
    
    void Start()
    {
        _score = 0;
        
        GameManager.Instance.OnPauseChanged += OnGamePaused;
        StartCoroutine(SpawnRoutine());
        _canSpawnBigChonk = true;

        switch (Globals.Difficulty)
        {
            case DifficultyLevel.Easy:
                difficultyInterval = 30.0f;
                maxEnemiesIncreasePerWave = 1;
                break;
            case DifficultyLevel.Medium:
                difficultyInterval = 25.0f;
                maxEnemiesIncreasePerWave = 2;
                break;
            case DifficultyLevel.Hard:
                difficultyInterval = 25.0f;
                maxEnemiesIncreasePerWave = 4;
                break;
            case DifficultyLevel.Impossible:
                difficultyInterval = 20.0f;
                maxEnemiesIncreasePerWave = 5;
                break;
        }
        Debug.Log("Playing in: " + Globals.Difficulty + "Mode");
    }

    void Update()
    {
        if (_paused || !GameManager.Instance.IsPlayerAlive)
            return;

        _waveTimer += Time.deltaTime;

        if (_waveTimer >= difficultyInterval)
        {
            _waveTimer = 0f;
            _wave++;
            OnDifficultyChanged?.Invoke(_wave);
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (GameManager.Instance.IsPlayerAlive)
        {
            yield return new WaitWhile(() => _paused);

            int scaledMaxEnemies = baseMaxEnemies + (_wave - 1) * maxEnemiesIncreasePerWave;

            if (_currentEnemies < scaledMaxEnemies)
                SpawnEnemy();

            float minDelay = Mathf.Max(0.2f, baseMinSpawnDelay - (_wave - 1) * spawnDelayDecreasePerWave);
            float maxDelay = Mathf.Max(minDelay + 0.1f, baseMaxSpawnDelay - (_wave - 1) * spawnDelayDecreasePerWave);
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

        if (rand < 0.5f)
        {
            enemyPrefab = shortRangeEnemyPrefab;
            enemyType = EnemyType.ShortRanged;
        }
        else if (rand < 0.9f)
        {
            enemyPrefab = longRangeEnemyPrefab;
            enemyType = EnemyType.LongRanged;
        }
        else
        {
            if (_canSpawnBigChonk)
            {
                enemyPrefab = bigChonkEnemyPrefab;
                enemyType = EnemyType.BigChonk;
                _canSpawnBigChonk = false;
            }
            else
            {
                enemyPrefab = longRangeEnemyPrefab;
                enemyType = EnemyType.LongRanged;
            }
        }
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);
        enemy.GetComponent<Enemy>().Initiate(this, enemyType);
        enemy.GetComponent<EnemySpawnIntro>()?.Play();
        _currentEnemies++;
    }

    public int GetWaveNumber()
    {
        return _wave;
    }
    
    public void OnEnemyDied(EnemyType type)
    {
        if (type == EnemyType.BigChonk)
            _canSpawnBigChonk = true;

        switch (type)
        {
            case EnemyType.ShortRanged:
                _score += Globals.KillUnitShortRangedScore;
                break;
            case EnemyType.LongRanged:
                _score += Globals.KillUnitLongRangedScore;
                break;
            case EnemyType.BigChonk:
                _score += Globals.KillUnitBigChonkScore;
                break;
        }
        
        OnEnemyKilled?.Invoke();
        _currentEnemies--;
    }

    public int GetEnemyKillScore()
    {
        return _score;
    }
    
    public int GetFinalScore()
    {
        _score += GetDifficultyScore(Globals.Difficulty);
        return _score;
    }

    public int GetDifficultyScore(DifficultyLevel difficultyLevel)
    {
        int result = 0;
        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                result += 150;
                break;  
            case DifficultyLevel.Medium:
                result += 150 * 3;
                break;
            case DifficultyLevel.Hard:
                result += 150 * 6;
                break;
            case DifficultyLevel.Impossible:
                result += 150 * 16;
                break;
        }
        return result;
    }
    
    private void OnGamePaused(bool isPaused)
    {
        _paused = isPaused;
    }
}