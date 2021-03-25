using UnityEngine;

/// <summary>
/// Keeps all public variables for the player in one file.
/// Redirects all calls from PlayerController to the appropriate subcomponents.
/// </summary>
public class PlayerSupervisor : MonoBehaviour
{
    public PlayerMovement motor;
    public Rigidbody rb;
    public Camera cam;

    public PlayerAttack attacker;
    public GameObject attackHitbox;

    public PlayerHealth healthMonitor;

    public PlayerUIInfo UI;


    /* Calls to motor */
    public void UpdateVelocity(Vector3 vel)
    {
        motor.UpdateVelocity(vel);
    }

    public void UpdateSprint(bool sprinting)
    {
        motor.UpdateSprint(sprinting);
    }

    public void UpdatePlayerRot(Vector3 newRot)
    {
        motor.UpdatePlayerRot(newRot);
    }

    public void UpdateCamRot(Vector3 newRot)
    {
        motor.UpdateCamRot(newRot);
    }

    public void ScheduleJump()
    {
        motor.ScheduleJump();
    }

    /* Calls to attacker */
    public void ScheduleAttack()
    {
        attacker.ScheduleAttack();
    }
}
