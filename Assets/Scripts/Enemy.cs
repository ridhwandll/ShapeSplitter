using UnityEngine;

public enum EnemyType
{
    ShortRanged,
    LongRanged,
    BigChonk
}

public class Enemy : MonoBehaviour, IHealth
{
    private EnemyType _enemyType;
    
    // Health
    private int _health;
    private int _maxHealth = 3;
    
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;
    public float smoothTime = 0.3f;
    public float fireRate = 1.0f;
    
    private Vector3 _velocity; // required for SmoothDamp
    
    public LayerMask enemyLayer;
    public GameObject bulletPrefab;
    public GameObject lifeShardPrefab;

    //On Death
    public ParticleSystem enemyDeathParticleSystem;
    public AudioClip deathClip;

    private SpriteRenderer _spriteRenderer;
    private Color _baseColor; 
    
    private EnemySpawner _mySpawner;
    public float personalSpace = 0.7f;
    private Transform _player;
    private float _nextFireTime;
    private float _currentDistanceFromTarget;
    
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseColor = _spriteRenderer.color;
        
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            _player = p.transform;
    }

    // Must Call after instantiating enemy
    public void Initiate(EnemySpawner mySpawner, EnemyType type)
    {
        _mySpawner = mySpawner;
        _enemyType = type;
        SetDifficultyLevelOfEnemy(Globals.Difficulty, type);
        _health = _maxHealth;
    }
    
    void Update()
    {
        if (!_player || GameManager.Instance.IsPaused)
            return;
        
        MoveEnemy();
        UpdateColor();
        
        if (Time.time >= _nextFireTime && _currentDistanceFromTarget <= stopDistance)
        {
            ShootAtPlayer();
            _nextFireTime = Time.time + fireRate;
        }
    }

    private void ShootAtPlayer()
    {
        // SHOOT
        Vector3 aimDir = _player.position - transform.position;
        var bullerSpawnPos = transform.position + (aimDir.normalized * 0.5f);
        GameObject bullet = Instantiate(bulletPrefab, bullerSpawnPos, Quaternion.identity);
        bullet.GetComponent<Bullet>().Setup(aimDir, true);
    }
    
    private void MoveEnemy()
    {
        Vector3 currentPos = transform.position;
        Vector3 targetPos = _player.position;

        Vector3 toPlayer = targetPos - currentPos;
        _currentDistanceFromTarget = toPlayer.magnitude;

        if (_currentDistanceFromTarget <= stopDistance)
        {
            _velocity = Vector3.zero; // clean stop
            return;
        }

        if (_enemyType == EnemyType.LongRanged)
        {
            //Rotate the enemy
            float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 720.0f * Time.deltaTime);
        }

        // Move the enemy
        Vector3 desiredPos = targetPos - toPlayer.normalized * stopDistance;
        Vector3 newPos = Vector3.SmoothDamp(currentPos, desiredPos, ref _velocity, smoothTime, moveSpeed);
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        
        // check nearby enemies
        Collider2D[] others = Physics2D.OverlapCircleAll(transform.position, personalSpace, enemyLayer);
        foreach (Collider2D other in others)
        {
            if (other.transform == transform) continue;
            Vector3 away = transform.position - other.transform.position;
            if (away.magnitude > 0f)
                toPlayer += away.normalized * 0.5f; // simple push
        }

        toPlayer.Normalize();
        transform.position += toPlayer * moveSpeed * Time.deltaTime;
    }
    
    public int GetCurrentHealth() => _health;
    public int GetMaxHealth() => _maxHealth;

    public void TakeDamage(int amount, bool isDamagingByOwnBullet = false)
    {
        _health = Mathf.Max(0, _health - amount);
        if (_health == 0)
        {
            if (Camera.main != null)
                Camera.main.gameObject.GetComponent<CameraShake>().Shake();

            SoundFXManager.instance.PlaySoundFXClip(deathClip, transform, 1.0f);
            
            // Play Enemy Death effect
            enemyDeathParticleSystem.transform.SetParent(null);
            enemyDeathParticleSystem.Play();
            Destroy(enemyDeathParticleSystem.gameObject, enemyDeathParticleSystem.main.duration + enemyDeathParticleSystem.main.startLifetime.constantMax);
            
            if (amount != 169) // DO NOT Give Life shard it killed by REPULSOR
            {
                GameObject lifeShard = Instantiate(lifeShardPrefab, transform.position, Quaternion.identity);
                lifeShard.GetComponent<LifeShard>().Initiate(_enemyType);
            }
            _mySpawner.OnEnemyDied(_enemyType);
            Destroy(gameObject);
        }
    }
    
    public void Heal(int amount)
    {
        _health = Mathf.Min(_maxHealth, _health + amount);
    }
    
    private void UpdateColor()
    {
        if (_spriteRenderer == null)
            return;

        float healthRatio = Mathf.Clamp01((float)_health / _maxHealth);
        
        Color.RGBToHSV(_baseColor, out float h, out float s, out float v);
        
        if (healthRatio > 0.8f) // 100% - 60%
            s = 1.0f;
        else if (healthRatio > 0.5f) // 60% - 30%
            s = 0.7f;
        else // 30% - 0%
            s = 0.4f;

        _spriteRenderer.color = Color.HSVToRGB(h, s, v);
    }
    
    private void SetDifficultyLevelOfEnemy(DifficultyLevel level, EnemyType type)
    {
        switch (level)
        {
            case DifficultyLevel.Easy:
                switch (type)
                {  
                    case EnemyType.ShortRanged:
                        _maxHealth = 1;
                        moveSpeed = 2.5f;
                        stopDistance = 3.5f;
                        fireRate = 1.4f;
                        break;
                    case EnemyType.LongRanged:
                        _maxHealth = 1;
                        moveSpeed = 1.5f;
                        stopDistance = 5f;
                        fireRate = 0.4f;
                        break;
                    case EnemyType.BigChonk:
                        _maxHealth = 10;
                        moveSpeed = 0.9f;
                        stopDistance = 3.5f;
                        fireRate = 0.8f;
                        break;
                }
                break;
            case DifficultyLevel.Medium:
                switch (type)
                {  
                    case EnemyType.ShortRanged:
                        _maxHealth = 2;
                        moveSpeed = 3.7f;
                        stopDistance = 3.8f;
                        fireRate = 1.2f;
                        break;
                    case EnemyType.LongRanged:
                        _maxHealth = 2;
                        moveSpeed = 1.5f;
                        stopDistance = 5.5f;
                        fireRate = 0.35f;
                        break;
                    case EnemyType.BigChonk:
                        _maxHealth = 18;
                        moveSpeed = 0.9f;
                        stopDistance = 4f;
                        fireRate = 0.6f;
                        break;
                }
                break;
            case DifficultyLevel.Hard:
                switch (type)
                {  
                    case EnemyType.ShortRanged:
                        _maxHealth = 2;
                        moveSpeed = 4f;
                        stopDistance = 4f;
                        fireRate = 0.9f;
                        break;
                    case EnemyType.LongRanged:
                        _maxHealth = 3;
                        moveSpeed = 1.9f;
                        stopDistance = 6f;
                        fireRate = 0.24f;
                        break;
                    case EnemyType.BigChonk:
                        _maxHealth = 20;
                        moveSpeed = 1f;
                        stopDistance = 4.5f;
                        fireRate = 0.5f;
                        break;
                }
                break;
            case DifficultyLevel.Impossible:
                switch (type)
                {  
                    case EnemyType.ShortRanged:
                        _maxHealth = 4;
                        moveSpeed = 4.2f;
                        stopDistance = 5f;
                        fireRate = 0.8f;
                        break;
                    case EnemyType.LongRanged:
                        _maxHealth = 3;
                        moveSpeed = 2.5f;
                        stopDistance = 6.7f;
                        fireRate = 0.22f;
                        break;
                    case EnemyType.BigChonk:
                        _maxHealth = 23;
                        moveSpeed = 1.4f;
                        stopDistance = 4.9f;
                        fireRate = 0.4f;
                        break;
                }
                break;
        }
    }
}
