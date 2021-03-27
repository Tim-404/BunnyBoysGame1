using System;
using UnityEngine;

/// <summary>
/// Handles everything related to player movement.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    public PlayerSupervisor supervisor;

    [SerializeField] private const float walkSpeed = 10f;
    [SerializeField] private const float sprintSpeed = 20f;
    [SerializeField] private const float aerialAgility = 0.005f;    // extremely sensitive
    [SerializeField] private const float maxAerialMobility = 5f;
    [SerializeField] private float cameraSensitivity = 6f;
    [SerializeField] private float jumpPower = 6f;

    private Rigidbody rb;
    private Vector3 lateralVelocity;
    private float moveSpeed = walkSpeed;

    private bool jumpScheduled = false;
    private bool isGrounded = true;
    private int numAirJumps = 0;
    private int maxAirJumps = 1;

    private Camera cam;
    private Vector3 playerRot;
    private Vector3 cameraRot;
    private float camRotTracker = 0;
    private const float maxCameraRot = 90f;

    // Used to correct lateralVelocity with aerial collisions
    private Vector3 prevLocation;

    private void Start()
    {
        rb = supervisor.rb;
        cam = supervisor.cam;
    }

    /// <summary>
    /// Updates player.
    /// </summary>
    private void FixedUpdate()
    {
        Move();
        Jump(); 
        Rotate();
        UpdateIsGrounded();
    }

    /// <summary>
    /// Updates when the player is on the ground.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (HasHitGround(collision))
        {
            numAirJumps = 0;
        }
    }

    /// <summary>
    /// Updates when the player is on the ground.
    /// </summary>
    /// <param name="collision"></param>
    /// <remarks>This is the repeated call so that it is accurate even on a sloped surface.</remarks>
    private void OnCollisionStay(Collision collision)
    {
        if (HasHitGround(collision))
        {
            isGrounded = true;
        }
    }

    /// <summary>
    /// Handles player movement.
    /// </summary>
    private void Move()
    {
        TrackPreviousLocation();
        CalibrateRBVelocity();
        // RigidBody.MovePosition(float) does automatic physics checking
        this.rb.MovePosition(this.rb.position + this.lateralVelocity);
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
            this.rb.velocity = new Vector3(0, this.jumpPower, 0);
            this.jumpScheduled = false;
            if (!isGrounded)
            {
               ++this.numAirJumps;
            }
        }
    }

    /// <summary>
    /// Tracks the previous location to correct the player velocity with aerial collisions.
    /// </summary>
    private void TrackPreviousLocation()
    {
        prevLocation = this.rb.position;
    }

    private void CalibrateRBVelocity()
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }

    /// <summary>
    /// Tracks when the player leaves the ground.
    /// </summary>
    private void UpdateIsGrounded()
    {
        if (isGrounded && rb.velocity.y != 0f)
        {
            isGrounded = false;
        }
    }

    /// <param name="collision">The collisions object</param>
    /// <returns>Whether the collision occurred with the ground.</returns>
    private bool HasHitGround(Collision collision)
    {
        return collision.collider.tag == "Ground";
    }

    /// <summary>
    /// Determines if the player should jump or not.
    /// </summary>
    public void ScheduleJump()
    {
        this.jumpScheduled = this.numAirJumps < maxAirJumps;
    }

    /// <summary>
    /// Updates the velocity of the player.
    /// </summary>
    /// <param name="vel">The new velocity.</param>
    public void UpdateVelocity(Vector3 direction)
    {
        if (isGrounded)
        {
            this.lateralVelocity = direction * moveSpeed;
        }
        else
        {
            // Corrects the velocity in the event of aerial collisions so that the player shoot in the direction
            // they are holding.
            this.lateralVelocity.x = this.rb.position.x - this.prevLocation.x;
            this.lateralVelocity.z = this.rb.position.z - this.prevLocation.z;

            // Prevents jagged aerial movement.
            // Cannot use RigidBody.AddForce() because we are already using Rigidbody.MovePosition()
            Vector3 targetVelocity = direction * maxAerialMobility;
            this.lateralVelocity = Vector3.MoveTowards(this.lateralVelocity, targetVelocity, aerialAgility);
        }
    }

    /// <summary>
    /// Updates the rotation of the player.
    /// </summary>
    /// <param name="newRot">The new player rotation.</param>
    public void UpdatePlayerRot(Vector3 newRot)
    {
        this.playerRot = newRot * cameraSensitivity;
    }

    /// <summary>
    /// Updates the rotation of the camera attached to the player if it exists.
    /// </summary>
    /// <param name="newRot">The new camera rotation.</param>
    public void UpdateCamRot(Vector3 newRot)
    {
        this.cameraRot = newRot * cameraSensitivity;
    }

    /// <summary>
    /// Toggles movespeed to reflect whether the player is sprinting or not.
    /// </summary>
    /// <param name="sprinting"></param>
    public void UpdateSprint(bool sprinting)
    {
        moveSpeed = sprinting && isGrounded ? sprintSpeed : walkSpeed;
    }
}
