using System;
using UnityEngine;

public class Repulsor : MonoBehaviour
{
    public float growthDuration = 1f;
    public float maxScale = 15f;
    public float initialScale = 0.1f;
    public ParticleSystem spawnParticles;
    private float _timer = 0f;

    private void Start()
    {
        spawnParticles.Play();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        
        // Calculate scale based on growth
        float scaleFactor = Mathf.Lerp(initialScale, maxScale, _timer / growthDuration);
        transform.localScale = Vector3.one * scaleFactor;

        // Destroy after fully grown
        if (_timer >= growthDuration)
            Destroy(gameObject);
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