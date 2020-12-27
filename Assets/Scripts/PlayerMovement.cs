using UnityEngine;

/// <summary>
/// Handles everything related to player movement.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public Camera cam;

    private Vector3 velocity;
    private Vector3 playerRot;
    private Vector3 cameraRot;

    /// <summary>
    /// Updates player.
    /// </summary>
    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    /// <summary>
    /// Handles player movement.
    /// </summary>
    private void Move()
    {
        // RigidBody.MovePosition(float) does automatic physics checking
        this.rb.MovePosition(this.rb.position + velocity);
    }

    /// <summary>
    /// Handles player rotation.
    /// </summary>
    private void Rotate()
    {
        this.rb.MoveRotation(this.rb.rotation * Quaternion.Euler(this.playerRot));
        if (this.cam != null)
        {
            this.cam.transform.Rotate(-cameraRot);
        }
    }

    /// <summary>
    /// Updates the velocity of the player.
    /// </summary>
    /// <param name="vel">The new velocity.</param>
    public void UpdateVelocity(Vector3 vel)
    {
        this.velocity = vel;
    }

    /// <summary>
    /// Updates the rotation of the player.
    /// </summary>
    /// <param name="newRot">The new player rotation.</param>
    public void UpdatePlayerRot(Vector3 newRot)
    {
        this.playerRot = newRot;
    }

    /// <summary>
    /// Updates the rotation of the camera attached to the player if it exists.
    /// </summary>
    /// <param name="newRot">The new camera rotation.</param>
    public void UpdateCamRot(Vector3 newRot)
    {
        this.cameraRot = newRot;
    }
}
