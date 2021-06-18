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

    [SerializeField] private int attackStrength1 = 10;
    [SerializeField] private Vector3 attackKnockback1 = new Vector3(0f, 1f, 2f);
    [SerializeField] private GameObject attackHitbox1;
    [SerializeField] private float attackDuration1 = 0.2f;
    [SerializeField] private float maxAttackCooldown1 = 1f;
    [SerializeField] private float hitstun1 = 0.5f;
    
    // These vars are updated on the server (client updates are only for testing).
    private HashSet<PlayerHealth> currentVictims;

    // These vars are updated only on clients.
    private bool attackScheduled;
    private Attack currAttack;
    private Dictionary<string, float> cooldowns;
    private List<string> activeCooldownsKeys;

    private Attack baseAttack;

    /// <summary>
    /// Inner class to hold the information on different attacks.
    /// </summary>
    private class Attack
    {
        public int Strength { get; }
        public Vector3 Knockback { get; }
        public GameObject Hitbox { get; }
        public float Duration { get; }
        public float Cooldown { get; }
        public float Hitstun { get; }

        public Attack(int strength, Vector3 knockback, GameObject hitbox, float duration, float cooldown, float hitstun)
        {
            Strength = strength;
            Knockback = knockback;
            Hitbox = hitbox;
            Duration = duration;
            Cooldown = cooldown;
            Hitstun = hitstun;
        }
    }

    /// <summary>
    /// Initialize other variables like the cooldowns list.
    /// </summary>
    public override void OnStartClient()
    {

        baseAttack = new Attack(attackStrength1, attackKnockback1, attackHitbox1, attackDuration1, maxAttackCooldown1, hitstun1);
        currAttack = baseAttack;
        attackScheduled = false;
        cooldowns = new Dictionary<string, float>();
        InitializeCooldownList();
        activeCooldownsKeys = new List<string>();
        currentVictims = new HashSet<PlayerHealth>();
    }

    private void InitializeCooldownList()
    {
        cooldowns.Add("attackCooldown", 0);
        cooldowns.Add("attackDuration", 0);
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
            currAttack = baseAttack;    // currAttack will depend on which type of attack is used. Right now only baseAttack is available.
            attackScheduled = false;
            cooldowns["attackCooldown"] = currAttack.Cooldown;
            cooldowns["attackDuration"] = currAttack.Duration;
            activeCooldownsKeys.Add("attackCooldown");
            activeCooldownsKeys.Add("attackDuration");
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
        if (currAttack.Hitbox.activeSelf && cooldowns["attackDuration"] == 0)
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
        List<string> inactiveCooldownKeys = new List<string>();
        foreach (string key in activeCooldownsKeys)
        {
            cooldowns[key] = Math.Max(cooldowns[key] - Time.fixedDeltaTime, 0);
            if (cooldowns[key] == 0)
            {
                inactiveCooldownKeys.Add(key);
            }
        }

        foreach (string key in inactiveCooldownKeys)
        {
            activeCooldownsKeys.Remove(key);
        }
    }

    /// <returns>Returns the amount of time until the player can attack again.</returns>
    [Client]
    public float GetAttackCooldown()
    {
        return cooldowns["attackCooldown"];
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
            // TODO: hitstun and launch
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
        victim.Damage(currAttack.Strength);
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
        if (cooldowns["attackCooldown"] == 0)
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
        currAttack.Hitbox.SetActive(true);
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
        currAttack.Hitbox.SetActive(true);
    }

    /// <summary>
    /// Removes the attackHitbox on the server.
    /// </summary>
    [Command]
    private void CmdDeactivateHitbox()
    {
        currAttack.Hitbox.SetActive(false);
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
        currAttack.Hitbox.SetActive(false);
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