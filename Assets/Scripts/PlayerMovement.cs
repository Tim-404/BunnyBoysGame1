using System;
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
    private float camRotTracker = 0;

    private bool jumpScheduled = false;
    private int numJumps = 0;

    private const float maxCameraRot = 90f;
    [SerializeField]
    private float jumpPower = 6f;
    [SerializeField]
    private int maxJumps = 2;

    /// <summary>
    /// Updates player.
    /// </summary>
    private void FixedUpdate()
    {
        Move();
        Rotate();
        Jump();
    }

    /// <summary>
    /// Updates when the player is on the ground.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Ground")
        {
            numJumps = 0;
        }
    }

    /// <summary>
    /// Handles player movement.
    /// </summary>
    private void Move()
    {
        // RigidBody.MovePosition(float) does automatic physics checking
        this.rb.MovePosition(this.rb.position + this.velocity);
    }

    /// <summary>
    /// Handles player and camera rotation.
    /// </summary>
    private void Rotate()
    {
        this.rb.MoveRotation(this.rb.rotation * Quaternion.Euler(this.playerRot));

        if (this.cam != null && Math.Abs(this.camRotTracker + this.cameraRot.x) < maxCameraRot)
        {
            this.cam.transform.Rotate(-this.cameraRot);
            this.camRotTracker += this.cameraRot.x;
        }
    }

    /// <summary>
    /// Makes the player jump if a jump is scheduled.
    /// </summary>
    private void Jump()
    {
        if (jumpScheduled)
        {
            this.rb.velocity = new Vector3(this.rb.velocity.x, 0, this.rb.velocity.z);
            this.rb.AddForce(new Vector3(0f, jumpPower, 0f), ForceMode.VelocityChange);
            ++this.numJumps;
            this.jumpScheduled = false;
        }
    }

    /// <summary>
    /// Determines if the player should jump or not.
    /// </summary>
    /// <remarks>Scheduling will make it easier to implement short hops later if we want to.</remarks>
    public void ScheduleJump()
    {
        this.jumpScheduled = this.numJumps < maxJumps;
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
