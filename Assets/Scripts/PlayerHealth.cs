using UnityEngine;
using Mirror;

/// <summary>
/// Tracks when the player takes damage.
/// </summary>
public class PlayerHealth : NetworkBehaviour
{
    public PlayerSupervisor supervisor;

    // Needs to be synced with the server manually.
    [SyncVar] private int health;

    private const int maxHealth = 100;

    /// <summary>
    /// Initializes the health to maxHealth.
    /// </summary>
    [Server]
    public override void OnStartClient()
    {
        health = maxHealth;
    }

    /// <returns>The value of health.</returns>
    [Client]
    public int GetHealth()
    {
        return health;
    }

    /// <summary>
    /// Deducts health on the server.
    /// </summary>
    /// <param name="hp">Hitpoints to deduct.</param>
    [Server]
    public void Damage(int hp)
    {
        health -= hp;
    }
}
