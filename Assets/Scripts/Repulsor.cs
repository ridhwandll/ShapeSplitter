using UnityEngine;

public class Repulsor : MonoBehaviour
{
    public float growthDuration = 1f;
    public float maxScale = 15f;
    public float initialScale = 0.1f;

    private CircleCollider2D circleCollider;
    private float timer = 0f;

    void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Calculate scale based on growth
        float scaleFactor = Mathf.Lerp(initialScale, maxScale, timer / growthDuration);
        transform.localScale = Vector3.one * scaleFactor;

        // Optionally, destroy after fully grown
        if (timer >= growthDuration)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            IHealth health = other.GetComponent<IHealth>();
            health.TakeDamage(169);            
        }
    }
}