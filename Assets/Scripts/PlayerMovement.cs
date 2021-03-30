using System;
using UnityEngine;
using Mirror;

/// <summary>
/// Handles everything related to player movement.
/// </summary>
public class PlayerMovement : NetworkBehaviour
{
    public PlayerSupervisor supervisor;

    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float sprintSpeed = 20f;
    [SerializeField] private float aerialAgility = 0.005f;    // extremely sensitive
    [SerializeField] private float maxAerialMobility = 5f;
    [SerializeField] private float cameraSensitivity = 6f;
    [SerializeField] private float jumpPower = 6f;

    private Rigidbody rb;
    
    /* Need to decide whether to control these vars on server or client. 
     * on server => client may be teleported when there is lag.
     * on client => other players may see the player teleporting when there is lag.
     * 
     * Currently these vars are controlled on the client.
     */
    private Vector3 lateralVelocity;
    private float moveSpeed = 10f;
    private bool jumpScheduled = false;
    private bool isGrounded = true;
    private int numAirJumps = 0;
    private int maxAirJumps = 1;
    private Camera cam;
    private Vector3 playerRot;
    private Vector3 cameraRot;
    private float camRotTracker = 0f;
    private const float maxCameraRot = 90f;

    // Used to correct lateralVelocity with aerial collisions
    private Vector3 prevLocation;

    private void Start()
    {
        rb = supervisor.rb;
        cam = supervisor.cam;
    }


    /* Client only */

    /// <summary>
    /// Updates player.
    /// </summary>
    [ClientCallback]
    private void FixedUpdate()
    {
        CalibrateRBVelocity();
        Move();
        Jump(); 
        Rotate();
        UpdateIsGrounded();
    }

    /// <summary>
    /// Updates when the player is on the ground.
    /// </summary>
    /// <param name="collision"></param>
    [ClientCallback]
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
    [ClientCallback]
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
    [Client]
    private void Move()
    {
        TrackPreviousLocation();
        // RigidBody.MovePosition(float) does automatic physics checking
        rb.MovePosition(rb.position + lateralVelocity);
    }

    /// <summary>
    /// Handles player and camera rotation.
    /// </summary>
    [Client]
    private void Rotate()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(playerRot));

        if (cam != null && Math.Abs(camRotTracker + cameraRot.x) < maxCameraRot)
        {
            cam.transform.Rotate(-cameraRot);
            camRotTracker += cameraRot.x;
        }
    }

    /// <summary>
    /// Makes the player jump if a jump is scheduled.
    /// </summary>
    [Client]
    private void Jump()
    {
        if (jumpScheduled)
        {
            rb.velocity = new Vector3(0f, this.jumpPower, 0f);
            jumpScheduled = false;
            if (!isGrounded)
            {
               ++numAirJumps;
            }
        }
    }

    /// <summary>
    /// Tracks the previous location to correct the player velocity with aerial collisions.
    /// </summary>
    [Client]
    private void TrackPreviousLocation()
    {
        prevLocation = rb.position;
    }

    /// <summary>
    /// Sets the x and z components of the Rigidbody velocity to 0.
    /// </summary>
    [Client]
    private void CalibrateRBVelocity()
    {
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }

    /// <summary>
    /// Tracks when the player leaves the ground.
    /// </summary>
    [Client]
    private void UpdateIsGrounded()
    {
        if (isGrounded && rb.velocity.y != 0f)
        {
            isGrounded = false;
        }
    }

    /// <param name="collision">The collisions object</param>
    /// <returns>Whether the collision occurred with the ground.</returns>
    [Client]
    private bool HasHitGround(Collision collision)
    {
        return collision.collider.tag == "Ground";
    }

    /// <summary>
    /// Determines if the player should jump or not.
    /// </summary>
    [Client]
    public void AttemptJump()
    {
        jumpScheduled = numAirJumps < maxAirJumps;
    }

    /// <summary>
    /// Updates the velocity of the player.
    /// </summary>
    /// <param name="vel">The new velocity.</param>
    [Client]
    public void UpdateVelocity(Vector3 direction)
    {
        if (isGrounded)
        {
            lateralVelocity = direction * moveSpeed;
        }
        else
        {
            // Corrects the velocity in the event of aerial collisions so that the player shoot in the direction
            // they are holding.
            lateralVelocity.x = rb.position.x - prevLocation.x;
            lateralVelocity.z = rb.position.z - prevLocation.z;

            // Prevents jagged aerial movement.
            // Cannot use RigidBody.AddForce() because we are already using Rigidbody.MovePosition()
            Vector3 targetVelocity = direction * maxAerialMobility;
            lateralVelocity = Vector3.MoveTowards(lateralVelocity, targetVelocity, aerialAgility);
        }
    }

    /// <summary>
    /// Updates the rotation of the player.
    /// </summary>
    /// <param name="newRot">The new player rotation.</param>
    [Client]
    public void UpdatePlayerRot(Vector3 newRot)
    {
        playerRot = newRot * cameraSensitivity;
    }

    /// <summary>
    /// Updates the rotation of the camera attached to the player if it exists.
    /// </summary>
    /// <param name="newRot">The new camera rotation.</param>
    [Client]
    public void UpdateCamRot(Vector3 newRot)
    {
        cameraRot = newRot * cameraSensitivity;
    }

    /// <summary>
    /// Toggles movespeed to reflect whether the player is sprinting or not.
    /// </summary>
    /// <param name="sprinting"></param>
    [Client]
    public void UpdateSprint(bool sprinting)
    {
        moveSpeed = sprinting && isGrounded ? sprintSpeed : walkSpeed;
    }

    /// <summary>
    /// Sends the player in a certain direction.
    /// </summary>
    /// <param name="velocity">The velocity at which to launch the player.</param>
    [ClientRpc]
    public void Launch(Vector3 velocity)
    {
        lateralVelocity.x = velocity.x;
        lateralVelocity.z = velocity.z;
        rb.velocity = new Vector3(0f, velocity.y, 0f);
        prevLocation.Set(rb.position.x - lateralVelocity.x, rb.position.y - rb.velocity.y, rb.position.z - lateralVelocity.z);
    }
}
