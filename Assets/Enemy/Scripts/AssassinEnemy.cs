// Targets core directly, ignores pieces
using UnityEngine;

public class AssassinEnemy : EnemyBase
{
    protected override void OnInit()
    {
    }

    protected override void UpdateBehaviour()
    {
        if (_core == null)
            return;

        // Move straight toward core
        Vector2 direction = (_core.position - transform.position).normalized;
        transform.Translate(direction * _moveSpeed * Time.deltaTime);
    }

    protected override void OnDamaged(float amount)
    {
        // Flash red, speed boost when hurt
        _moveSpeed += 5f;
    }
}