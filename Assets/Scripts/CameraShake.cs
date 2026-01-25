using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float moveAmount = 0.5f;
    public int cycleCount = 5;
    public float interval = 0.025f;
    
    private Vector3 _startPosition;
    private bool _isShaking = false;

    public void Shake()
    {
        if (_isShaking)
        {
            StopAllCoroutines();
            transform.position = _startPosition;
            _isShaking = false;
        }

        StartCoroutine(CameraShakeRoutine(moveAmount, cycleCount, interval));
    }
    
    public void Shake(float moveAmt, int cycle, float shakeInterval)
    {
        if (_isShaking)
        {
            StopAllCoroutines();
            transform.position = _startPosition;
            _isShaking = false;
        }

        StartCoroutine(CameraShakeRoutine(moveAmt, cycle, shakeInterval));
    }
    

    IEnumerator CameraShakeRoutine(float moveAmt, int cycle, float shakeInterval)
    {
        _startPosition = transform.position;
        float currentStrength = moveAmt;
        _isShaking = true;
        for (int i = 0; i < cycle; i++)
        {
            // Random offset
            Vector3 targetOffset = new Vector3(
                Random.Range(-currentStrength, currentStrength),
                Random.Range(-currentStrength, currentStrength),
                0f
            );

            Vector3 startPos = transform.position;
            Vector3 targetPos = _startPosition + targetOffset;

            float t = 0f;

            // Smooth move TO offset
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / shakeInterval;
                transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            // Smooth move BACK
            t = 0f;
            startPos = transform.position;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / shakeInterval;
                transform.position = Vector3.Lerp(startPos, _startPosition, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            // Falloff so it calms down
            currentStrength *= 0.85f;
        }

        transform.position = _startPosition;
        _isShaking = true;
    }
}