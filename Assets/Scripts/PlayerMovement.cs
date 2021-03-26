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
    [SerializeField] private const float aerialAgility = 0.004f;    // extremely sensitive
    [SerializeField] private const float maxAerialMobility = 8f;
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
        Rotate();
        Jump();
        UpdateIsGrounded();
    }

    /// <summary>
    /// Updates when the player is on the ground.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (CollisionIsGround(collision))
        {
            numAirJumps = 0;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (CollisionIsGround(collision))
        {
            isGrounded = true;
        }
    }

    /// <summary>
    /// Handles player movement.
    /// </summary>
    private void Move()
    {
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
            this.rb.velocity = new Vector3(this.rb.velocity.x, this.jumpPower, this.rb.velocity.z);
            this.jumpScheduled = false;
            if (!isGrounded)
            {
                ++this.numAirJumps;
            }
        }
    }

    private void UpdateIsGrounded()
    {
        if (rb.velocity.y != 0f)
        {
            isGrounded = false;
        }
    }

    /// <param name="collision">The collisions object</param>
    /// <returns>Whether the collision occurred with the ground.</returns>
    private bool CollisionIsGround(Collision collision)
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
            // Prevents jagged aerial movement
            Vector3 targetVelocity = direction * maxAerialMobility;
            Vector3 velocityDiff = targetVelocity - this.lateralVelocity;
            Vector3 acceleration = (velocityDiff).normalized * aerialAgility;
            if (acceleration.sqrMagnitude >= velocityDiff.sqrMagnitude)
            {
                this.lateralVelocity = targetVelocity;
            }
            else
            {
                this.lateralVelocity += acceleration;
            }
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
