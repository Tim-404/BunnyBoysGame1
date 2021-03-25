using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display stats to the player screen.
/// </summary>
public class PlayerUIInfo : MonoBehaviour
{
    public PlayerSupervisor supervisor;
    public Text health;
    public Text attackCooldown;

    private void FixedUpdate()
    {
        UpdateUI();
    }

    /// <summary>
    /// Update the data being outputted to the screen.
    /// </summary>
    public void UpdateUI()
    {
        health.text = supervisor.healthMonitor.GetHealth().ToString();
        attackCooldown.text = supervisor.attacker.GetAttackCooldown().ToString();
    }
}
