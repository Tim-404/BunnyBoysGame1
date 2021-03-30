using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// Display stats to the player screen.
/// </summary>
public class PlayerUIInfo : NetworkBehaviour
{
    public PlayerSupervisor supervisor;
    public Text health;
    public Text attackCooldown;

    [ClientCallback]
    private void FixedUpdate()
    {
        UpdateUI();
    }

    /// <summary>
    /// Update the data being outputted to the screen.
    /// </summary>
    [Client]
    public void UpdateUI()
    {
        health.text = supervisor.healthMonitor.GetHealth().ToString();
        attackCooldown.text = supervisor.attacker.GetAttackCooldown().ToString();
    }
}
