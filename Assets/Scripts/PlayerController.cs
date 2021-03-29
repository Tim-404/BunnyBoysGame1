using UnityEngine;
using Mirror;

/// <summary>
/// Receives and interprets player input before passing it to the PlayerSupervisor.
/// </summary>
public class PlayerController : NetworkBehaviour
{
    public PlayerSupervisor supervisor;

    private Vector3 currPlayerRot;
    private Vector3 currCamRot;

    /// <summary>
    /// Processes all player inputs, like movement or attacks.
    /// </summary>
    [Client]
    private void Update()
    {
        CheckSprint();
        supervisor.UpdateDirection(CalcLateralDirection());
        supervisor.UpdatePlayerRot(CalcPlayerRot());
        supervisor.UpdateCamRot(CalcCamRot());
        ReadJump();
        ReadAttack();
    }

    /// <summary>
    /// Calculates player movement.
    /// </summary>
    /// <returns>The vector that determines where the player will move.</returns>
    [Client]
    private Vector3 CalcLateralDirection()
    {
        // Uses inputs in Edit -> Project Settings -> Input Manager
        Vector3 xMov = transform.right * Input.GetAxisRaw("Horizontal");
        Vector3 zMov = transform.forward * Input.GetAxisRaw("Vertical");
        return (xMov + zMov).normalized * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Checks if sprint is enabled
    /// </summary>
    [Client]
    private void CheckSprint()
    {
        // Uses inputs in Edit -> Project Settings -> Input Manager
        supervisor.UpdateSprint(Input.GetAxisRaw("Sprint") == 1);
    }

    /// <summary>
    /// Calculates player rotation in the xz-plane.
    /// </summary>
    /// <returns>The vector that will change the player rotation.</returns>
    [Client]
    private Vector3 CalcPlayerRot()
    {
        float horizRot = Input.GetAxisRaw("Mouse X");
        currPlayerRot.Set(0f, horizRot, 0f);
        return currPlayerRot;
    }

    /// <summary>
    /// Calculates vertical camera rotation.
    /// </summary>
    /// <returns>The vector that will change the camera rotation.</returns>
    [Client]
    private Vector3 CalcCamRot()
    {
        float vertRot = Input.GetAxisRaw("Mouse Y");
        currCamRot.Set(vertRot, 0f, 0f);
        return currCamRot;
    }

    /// <summary>
    /// Determines when the player wants to jump.
    /// </summary>
    [Client]
    private void ReadJump()
    {   // Input.GetKeyDown() only fires once per press
        if (Input.GetKeyDown("space"))
        {
            supervisor.ScheduleJump();
        }
    }

    /// <summary>
    /// Determines when the player wants to attack.
    /// </summary>
    [Client]
    private void ReadAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            supervisor.ScheduleAttack();
        }
    }
}
