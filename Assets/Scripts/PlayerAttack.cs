using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Handles the player attack hitbox and synchronizes it with the server and the other players.
/// </summary>
public class PlayerAttack : NetworkBehaviour
{
    public PlayerSupervisor supervisor;

    [SerializeField] private float maxAttackCooldown = 1f;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private int attackStrength = 10;
    
    // These vars are updated on the server (client updates are only for testing).
    private GameObject attackHitbox;
    private HashSet<PlayerHealth> currentVictims;

    // These vars are updated only on clients.
    private bool attackScheduled;
    private Dictionary<string, MutableFloatPair> cooldowns;
    

    /// <summary>
    /// Inner class to represent maximum cooldown, actual cooldown pairs. The class uses pair such that the Key is
    /// the maximum cooldown while the Value is the current cooldown timer.
    /// </summary>
    /// <remarks>This could probably be a struct.</remarks>
    private class MutableFloatPair
    {
        public float Key { get; set; }
        public float Value { get; set; }

        public MutableFloatPair(float val1, float val2)
        {
            Key = val1;
            Value = val2;
        }
    }

    /// <summary>
    /// Initialize other variables like the cooldowns list.
    /// </summary>
    public override void OnStartClient()
    {
        attackHitbox = supervisor.attackHitbox;
        attackScheduled = false;
        cooldowns = new Dictionary<string, MutableFloatPair>();
        InitializeCooldownList();
        currentVictims = new HashSet<PlayerHealth>();
    }

    private void InitializeCooldownList()
    {
        cooldowns.Add("attackCooldown", new MutableFloatPair(maxAttackCooldown, 0));
        cooldowns.Add("attackDuration", new MutableFloatPair(attackDuration, 0));
    }


    /* Client only */

    // Update is called once per frame
    [ClientCallback]
    private void FixedUpdate()
    {
        LaunchAttack();
        UpdateMultiframeProcesses();
        TickCooldowns();
    }

    /// <summary>
    /// Makes the player attack if an attack is scheduled.
    /// </summary>
    [Client]
    private void LaunchAttack()
    {
        if (attackScheduled)
        {
            CmdClearVictims();
            attackScheduled = false;
            cooldowns["attackCooldown"].Value = maxAttackCooldown;
            cooldowns["attackDuration"].Value = attackDuration;
            CmdActivateHitbox();
        }
    }

    
    /// <summary>
    /// Updates all processes that span multiple frames. This includes managing attack hitboxes
    /// and animations (if we get there)
    /// </summary>
    [Client]
    private void UpdateMultiframeProcesses()
    {
        if (attackHitbox.activeSelf && cooldowns["attackDuration"].Value == 0)
        {
            CmdDeactivateHitbox();
        }
    }

    /// <summary>
    /// Decrements all the cooldown timers by 1 unless they are equal to 0.
    /// </summary>
    [Client]
    private void TickCooldowns()
    {
        foreach (KeyValuePair<string, MutableFloatPair> cooldownStat in cooldowns)
        {
            cooldownStat.Value.Value = Math.Max(cooldownStat.Value.Value - Time.fixedDeltaTime, 0);
        }
    }

    /// <returns>Returns the amount of time until the player can attack again.</returns>
    [Client]
    public float GetAttackCooldown()
    {
        return cooldowns["attackCooldown"].Value;
    }


    /* Server only */

    /// <summary>
    /// Determines if the hitbox is an attack from another player and takes damage.
    /// </summary>
    /// <param name="other">The player affected by the trigger</param>
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        PlayerSupervisor otherSupervisor = other.gameObject.GetComponent<PlayerSupervisor>();
        if (other.CompareTag("Player")
            && otherSupervisor != supervisor
            && !PreviouslyHit(otherSupervisor.healthMonitor))
        {
            InflictDamageOn(otherSupervisor.healthMonitor);
        }
    }

    /// <summary>
    /// Checks if the given player has already been hit by attack.
    /// </summary>
    /// <param name="victim">The player to check</param>
    /// <returns></returns>
    [Server]
    private bool PreviouslyHit(PlayerHealth victim)
    {
        return currentVictims.Contains(victim);
    }

    /// <summary>
    /// Inflicts damage on the given player.
    /// </summary>
    /// <param name="victim">The player to injure</param>
    [Server]
    private void InflictDamageOn(PlayerHealth victim)
    {
        victim.Damage(attackStrength);
        AddVictim(victim);
    }
    
    /// <summary>
    /// Adds victim to the list on the server.
    /// </summary>
    /// <param name="victim">The victim to add</param>
    [Server]
    private void AddVictim(PlayerHealth victim)
    {
        currentVictims.Add(victim);
        RpcAddVictim(victim);
    }


    /* Client <=> Server */

    /// <summary>
    /// Determines if the player should attack or not using the server vars.
    /// </summary>
    /// <remarks>Conditions include attack cooldown, hitstun ... (to be implemented)</remarks>
    [Command]
    public void CmdAttemptAttack()
    {
        if (cooldowns["attackCooldown"].Value == 0)
        {
            RpcScheduleAttack();
        }
    }

    /// <summary>
    /// Schedules the client to attack.
    /// </summary>
    [TargetRpc]
    public void RpcScheduleAttack()
    {
        attackScheduled = true;
    }

    /// <summary>
    /// Places the attackHitbox on the server.
    /// </summary>
    [Command]
    private void CmdActivateHitbox()
    {
        attackHitbox.SetActive(true);
        RpcActivateHitbox();
    }

    /// <summary>
    /// Places the attackHitbox on the client.
    /// </summary>
    /// <remarks>Optional, useful for visualizing hitboxes.</remarks>
    [ClientRpc]
    private void RpcActivateHitbox()
    {
        if (isServer) return;
        attackHitbox.SetActive(true);
    }

    /// <summary>
    /// Removes the attackHitbox on the server.
    /// </summary>
    [Command]
    private void CmdDeactivateHitbox()
    {
        attackHitbox.SetActive(false);
        RpcDeactivateHitbox();
    }

    /// <summary>
    /// Removes the attackHitbox on the client.
    /// </summary>
    /// <remarks>Optional, but must be present if hitbox is activated too.</remarks>
    [ClientRpc]
    private void RpcDeactivateHitbox()
    {
        if (isServer) return;
        attackHitbox.SetActive(false);
    }

    /// <summary>
    /// Adds victim to the list on the clients.
    /// </summary>
    /// <param name="victim">The victim to add</param>
    /// <remarks>Optional</remarks>
    [ClientRpc]
    private void RpcAddVictim(PlayerHealth victim)
    {
        if (isServer) return;
        currentVictims.Add(victim);
    }

    /// <summary>
    /// Clears the victims list on the server.
    /// </summary>
    [Command]
    private void CmdClearVictims()
    {
        currentVictims.Clear();
        RpcClearVictims();
    }

    /// <summary>
    /// Clears the victims list on the client.
    /// </summary>
    /// <remarks>Optional, but must be present if victims are added too.</remarks>
    [ClientRpc]
    private void RpcClearVictims()
    {
        if (isServer) return;
        currentVictims.Clear();
    }
}