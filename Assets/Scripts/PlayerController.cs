using UnityEngine;

/// <summary>
/// Receives and interprets player input before passing it to the PlayerSupervisor.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public PlayerSupervisor supervisor;

    /// <summary>
    /// Processes all player inputs, like movement or attacks.
    /// </summary>
    private void Update()
    {
        CheckSprint();
        this.supervisor.UpdateVelocity(CalcLateralVelocity());
        this.supervisor.UpdatePlayerRot(CalcPlayerRot());
        this.supervisor.UpdateCamRot(CalcCamRot());
        ReadJump();
        ReadAttack();
    }

    /// <summary>
    /// Calculates player movement.
    /// </summary>
    /// <returns>The vector that determines where the player will move.</returns>
    private Vector3 CalcLateralVelocity()
    {
        // Uses inputs in Edit -> Project Settings -> Input Manager
        Vector3 xMov = transform.right * Input.GetAxisRaw("Horizontal");
        Vector3 zMov = transform.forward * Input.GetAxisRaw("Vertical");
        return (xMov + zMov).normalized * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Checks if sprint is enabled
    /// </summary>
    private void CheckSprint()
    {
        // Uses inputs in Edit -> Project Settings -> Input Manager
        this.supervisor.UpdateSprint(Input.GetAxisRaw("Sprint") == 1);
    }

    /// <summary>
    /// Calculates player rotation in the xz-plane.
    /// </summary>
    /// <returns>The vector that will change the player rotation.</returns>
    private Vector3 CalcPlayerRot()
    {
        float horizRot = Input.GetAxisRaw("Mouse X");
        return new Vector3(0f, horizRot, 0f);
    }

    /// <summary>
    /// Calculates vertical camera rotation.
    /// </summary>
    /// <returns>The vector that will change the camera rotation.</returns>
    private Vector3 CalcCamRot()
    {
        float vertRot = Input.GetAxisRaw("Mouse Y");
        return new Vector3(vertRot, 0f, 0f);
    }

    /// <summary>
    /// Determines when the player wants to jump.
    /// </summary>
    private void ReadJump()
    {   // Input.GetKeyDown() only fires once per press
        if (Input.GetKeyDown("space"))
        {
            this.supervisor.ScheduleJump();
        }
    }

    /// <summary>
    /// Determines when the player wants to attack.
    /// </summary>
    private void ReadAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.supervisor.ScheduleAttack();
        }
    }
}
