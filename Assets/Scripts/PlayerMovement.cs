using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const float moveSpeed = 10f;

    public Transform player;

    /// <summary>
    /// Handles player movement in the xz-plane
    /// </summary>
    private void FixedUpdate()
    {
        // Uses inputs in Edit -> Project Settings -> Input Manager
        Vector3 xMov = transform.right * Input.GetAxisRaw("Horizontal");
        Vector3 zMov = transform.forward * Input.GetAxisRaw("Vertical");

        Vector3 velocity = (xMov + zMov).normalized * moveSpeed * Time.deltaTime;

        player.position += velocity;
    }
}
