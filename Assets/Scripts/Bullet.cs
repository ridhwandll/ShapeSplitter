using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int _damage = 1;
    public float speed = 10f;
    public float lifetime = 2f;
    
    private Vector2 direction;
    private bool _isEnemyBullet;

    private void Start()
    {
    }
    
    public void Setup(Vector2 shootDirection, bool isEnemyBullet = false)
    {
        _isEnemyBullet = isEnemyBullet;
        gameObject.GetComponent<SpriteRenderer>().color = isEnemyBullet  ? Constants.EnemyColor : Constants.PlayerColor;        
        direction = shootDirection.normalized;
        Destroy(gameObject, lifetime); // auto destroy after lifetime
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        IHealth health = collision.gameObject.GetComponent<IHealth>();

        // Check for enemy hit
        if (collision.gameObject.CompareTag("Enemy") && !_isEnemyBullet)
        {
            health.TakeDamage(_damage);
            Destroy(gameObject); 
        }
        else if (collision.gameObject.CompareTag("Player") && _isEnemyBullet)
        {
            health.TakeDamage(_damage);
            Destroy(gameObject); 
        }
        
        //Destroy if hits walls
        if (collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}