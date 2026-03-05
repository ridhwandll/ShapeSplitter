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
    }
}
