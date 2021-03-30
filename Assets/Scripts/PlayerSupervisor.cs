using UnityEngine;
using Mirror;

/// <summary>
/// Keeps all public variables for the player in one file.
/// Redirects all calls from PlayerController to the appropriate subcomponents.
/// </summary>
public class PlayerSupervisor : NetworkBehaviour
{
    public PlayerMovement motor;
    public Rigidbody rb;
    public Camera cam;

    public PlayerAttack attacker;
    public GameObject attackHitbox;

    public PlayerHealth healthMonitor;

    public PlayerUIInfo UI;


    /* Calls to motor */
    [Client]
    public void UpdateDirection(Vector3 direction)
    {
        motor.UpdateVelocity(direction);
    }

    [Client]
    public void UpdateSprint(bool sprinting)
    {
        motor.UpdateSprint(sprinting);
    }

    [Client]
    public void UpdatePlayerRot(Vector3 newRot)
    {
        motor.UpdatePlayerRot(newRot);
    }

    [Client]
    public void UpdateCamRot(Vector3 newRot)
    {
        motor.UpdateCamRot(newRot);
    }

    [Client]
    public void ScheduleJump()
    {
        motor.AttemptJump();
    }

    [Client]
    /* Calls to attacker */
    public void ScheduleAttack()
    {
        attacker.CmdAttemptAttack();
    }
}
