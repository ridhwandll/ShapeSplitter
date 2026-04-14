using UnityEngine;

public class PlayerSplitShape : MonoBehaviour
{
    public bool CollidedWithOthersOnce;

    // Resets the collider to not trigger
    public void ResetCollider()
    {
        CollidedWithOthersOnce = false;
        GetComponent<Collider2D>().isTrigger = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollidedWithOthersOnce = true;

        if(collision.gameObject.CompareTag("Enemy"))
        {
            IHealth health = collision.gameObject.GetComponent<IHealth>();
            Debug.Log(health.GetMaxHealth());
            health.TakeDamage(5);
        }

    }
}
