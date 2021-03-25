using UnityEngine;
using Mirror;

/// <summary>
/// Tracks when the player takes damage.
/// </summary>
public class PlayerHealth : NetworkBehaviour
{
    public PlayerSupervisor supervisor;

    // Needs to be synced with the server manually.
    private int health;

    private const int maxHealth = 100;

    /// <summary>
    /// Initializes the health to maxHealth.
    /// </summary>
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
        RpcDamage(hp);
    }

    /// <summary>
    /// Deducts health on the client.
    /// </summary>
    /// <param name="hp">Hitpoints to deduct.</param>
    [ClientRpc]
    private void RpcDamage(int hp)
    {
        if (isServer) return;
        health -= hp;
    }
}
