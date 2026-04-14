using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IHealth
{
    // Actual enemy data
    [SerializeField] private EnemyData _data;

    // Copied data to script from SO
    protected int _maxHealth = 100;
    protected float _moveSpeed = 3f;
    protected int _damage = 10;

    protected int _health;
    protected Transform _core;
    protected List<Transform> _pieces;

    // Called by spawner when spawning enemies
    public void Init(Transform playerCore, List<Transform> playerPieces)
    {
        _core = playerCore;
        _pieces = playerPieces;
        ApplyData();

        _health = _maxHealth;
        OnInit();
    }
    private void ApplyData()
    {
        _maxHealth = _data.MaxHealth;
        _moveSpeed = _data.MoveSpeed;
        _damage = _data.Damage;
    }

    private void Update()
    {
        UpdateBehaviour();
    }

    protected abstract void OnInit();

    protected abstract void UpdateBehaviour();

    protected abstract void OnDamaged(float amount);

    protected virtual void OnDeath()
    {
        Destroy(gameObject);
    }

    public int GetCurrentHealth() => _health;
    public int GetMaxHealth() => _maxHealth;

    public void TakeDamage(int amount, bool isDamagingByOwnBullet = false)
    {
        OnDamaged(amount);
        _health = Mathf.Max(0, _health - amount);
        if (_health == 0)
        {
            if (Camera.main != null)
                Camera.main.gameObject.GetComponent<CameraShake>().Shake();

            OnDeath();
        }
    }

    public void Heal(int amount)
    {
        _health = Mathf.Min(_maxHealth, _health + amount);
    }
}
