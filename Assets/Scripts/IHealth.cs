public interface IHealth
{
    void TakeDamage(int amount, bool isDamagingByOwnBullet = false);
    void Heal(int amount);
    
    int GetCurrentHealth();
    int GetMaxHealth();
}