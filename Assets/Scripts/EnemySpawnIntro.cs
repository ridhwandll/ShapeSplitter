using UnityEngine;
using System.Collections;

public class EnemySpawnIntro : MonoBehaviour
{
    public float spawnTime = 0.3f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Enemy _enemyScript;
    private Vector3 _initialScale;
        
    void Awake()
    {
        _enemyScript = GetComponent<Enemy>();
    }

    public void Play()
    {
        _initialScale = transform.localScale;
        transform.localScale = Vector3.zero;

        if (_enemyScript != null)
            _enemyScript.enabled = false;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        float t = 0f;

        while (t < spawnTime)
        {
            t += Time.deltaTime;
            float normalized = t / spawnTime;

            float scale = scaleCurve.Evaluate(normalized);
            transform.localScale = _initialScale * scale;

            yield return null;
        }

        transform.localScale = _initialScale;

        if (_enemyScript != null)
            _enemyScript.enabled = true;
    }
}