namespace HorrorGame.Interfaces
{
    /// <summary>INTERFACE: kontrak untuk objek yang bisa terkena damage (pemain).</summary>
    public interface IDamageable
    {
        int CurrentHealth { get; }
        bool IsDead { get; }
        void TakeDamage(int amount);
    }
}
