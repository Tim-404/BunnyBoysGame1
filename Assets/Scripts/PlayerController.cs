using UnityEngine;

/// <summary>
/// Handles everything related to player input and performs calculations before
/// physics are calculated.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public PlayerMovement motor;

    [SerializeField]
    private float moveSpeed = 10f;
    [SerializeField]
    private float mouseSensitivity = 10f;

    /// <summary>
    /// Processes all player inputs, like movement or attacks.
    /// </summary>
    private void Update()
    {
        this.motor.UpdateVelocity(CalcVelocity());
        this.motor.UpdatePlayerRot(CalcPlayerRot());
        this.motor.UpdateCamRot(CalcCamRot());
    }

    /// <summary>
    /// Calculates player movement.
    /// </summary>
    /// <returns>The vector that determines where the player will move.</returns>
    private Vector3 CalcVelocity()
    {
        // Uses inputs in Edit -> Project Settings -> Input Manager
        Vector3 xMov = transform.right * Input.GetAxisRaw("Horizontal");
        Vector3 zMov = transform.forward * Input.GetAxisRaw("Vertical");
        return (xMov + zMov).normalized * moveSpeed * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Calculates player rotation in the xz-plane.
    /// </summary>
    /// <returns>The vector that will change the player rotation.</returns>
    private Vector3 CalcPlayerRot()
    {
        float horizRot = Input.GetAxisRaw("Mouse X");
        return new Vector3(0f, horizRot, 0f) * mouseSensitivity;
    }

    /// <summary>
    /// Calculates vertical camera rotation.
    /// </summary>
    /// <returns>The vector that will change the camera rotation.</returns>
    private Vector3 CalcCamRot()
    {
        float vertRot = Input.GetAxisRaw("Mouse Y");
        return new Vector3(vertRot, 0f, 0f) * mouseSensitivity;
    }
}
