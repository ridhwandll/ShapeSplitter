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