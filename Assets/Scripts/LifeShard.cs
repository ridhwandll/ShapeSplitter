using UnityEngine;

public class LifeShard : MonoBehaviour
{
    public int healthAtSpawn = 1;
    public int healthAfter3s = 2;
    public int healthAfter6s = 4;
    
    public Color colorAtSpawn = Color.paleGoldenRod;
    public Color colorAfter3s = Color.yellow;
    public Color colorAfter6s = Color.orange;
    
    public float lifetime = 17.0f;

    private SpriteRenderer sr;
    private int healthValue;
    private float _spawnTime; 
    
    void Start()
    {
        _spawnTime = Time.time;
        
        sr = GetComponent<SpriteRenderer>();
        sr.color = colorAtSpawn;
        Destroy(gameObject, lifetime);
    }

    public void Initiate(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.BigChonk:
                healthAtSpawn = 8;
                healthAfter3s = 10;
                healthAfter6s = 15;
                transform.localScale = new Vector3(transform.localScale.x * 2.5f, transform.localScale.y * 2.5f, 1f);
                break;
            case EnemyType.LongRanged:
                healthAtSpawn = 2;
                healthAfter3s = 3;
                healthAfter6s = 5;
                transform.localScale = new Vector3(transform.localScale.x * 1.5f, transform.localScale.y * 1.5f, 1f);
                break;
            case EnemyType.ShortRanged:
                //Default
                break;
        }
    }
    
    void Update()
    {
        float elapsed = Time.time - _spawnTime;
        
        if (elapsed >= 6f)
            sr.color = colorAfter6s;
        else if (elapsed >= 3f)
            sr.color = colorAfter3s;
        else
            sr.color = colorAtSpawn;
    }
    
    // Give health to player on pickup
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IHealth playerHealth = other.GetComponent<IHealth>();
            if (playerHealth != null)
            {
                float elapsed = Time.time - _spawnTime;

                int healthToGive = healthAtSpawn;

                if (elapsed >= 6f)
                    healthToGive = healthAfter6s;
                else if (elapsed >= 3f)
                    healthToGive = healthAfter3s;

                playerHealth.Heal(healthToGive);
            }

            Destroy(gameObject);
        }
    }
}