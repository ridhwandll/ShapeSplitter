using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int _damage = 1;
    public float speed = 10f;
    public float lifetime = 2f;
    private Vector2 direction;
    private bool _isEnemyBullet;
    
    public void Setup(Vector2 shootDirection, bool isEnemyBullet = false)
    {
        _isEnemyBullet = isEnemyBullet;
        Color bulletColor = isEnemyBullet ? Globals.EnemyColor : Globals.PlayerColor;
        gameObject.GetComponent<SpriteRenderer>().color = bulletColor;
        direction = shootDirection.normalized;
        Destroy(gameObject, lifetime); // auto destroy after lifetime
        SetupTrailRenderer();
    }
    
    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        IHealth health = collision.gameObject.GetComponent<IHealth>();
        
        if (collision.gameObject.CompareTag("Enemy") && !_isEnemyBullet)
        {
            health.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision) // Player is a Trigger Only
    {
        IHealth health = collision.gameObject.GetComponent<IHealth>();
        if (collision.gameObject.CompareTag("Player") && _isEnemyBullet)
        {
            health.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }

    private void SetupTrailRenderer()
    {
        Color bulletColor = _isEnemyBullet ? Globals.EnemyColor : Globals.PlayerColor;
        
        // Set the color of the trail
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(bulletColor, 0.0f);
        colorKeys[1] = new GradientColorKey(bulletColor, 1.0f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(bulletColor.a, 1.0f);
        alphaKeys[1] = new GradientAlphaKey(bulletColor.a, 0.0f);

        // Set the keys in the gradient
        gradient.SetKeys(colorKeys, alphaKeys);

        // Assign the new gradient to the TrailRenderer's colorGradient property
        GetComponent<TrailRenderer>().colorGradient = gradient;
    }
}