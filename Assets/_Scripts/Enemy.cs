using UnityEngine;

public enum EnemyType
{
    ShortRanged,
    LongRanged,
    BigChonk
}

public class Enemy : MonoBehaviour, IHealth
{
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;
    public float smoothTime = 0.3f;
    public float fireRate = 1.0f;

    public LayerMask enemyLayer;
    public GameObject bulletPrefab;

    public float personalSpace = 0.7f;

    // Health
    private int _health;
    private int _maxHealth = 3;
        
    private Vector3 _velocity; // required for SmoothDamp
    

    private SpriteRenderer _spriteRenderer;
    private Color _baseColor;     

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
    
    void Update()
    {
        if (!_player || GameManager.Instance.IsPaused)
            return;
        
        MoveEnemy();
        
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

            //SoundFXManager.instance.PlaySoundFXClip(deathClip, 1.0f);
            
            // Play Enemy Death effect
            //enemyDeathParticleSystem.transform.SetParent(null);
            //enemyDeathParticleSystem.Play();
            //Destroy(enemyDeathParticleSystem.gameObject, enemyDeathParticleSystem.main.duration + enemyDeathParticleSystem.main.startLifetime.constantMax);
            
            //if (amount != 169) // DO NOT Give Life shard it killed by REPULSOR
            //{
            //    GameObject lifeShard = Instantiate(lifeShardPrefab, transform.position, Quaternion.identity);
            //    lifeShard.GetComponent<LifeShard>().Initiate(_enemyType);
            //}

            //_mySpawner.OnEnemyDied(_enemyType);
            Destroy(gameObject);
        }
    }
    
    public void Heal(int amount)
    {
        _health = Mathf.Min(_maxHealth, _health + amount);
    }
}
