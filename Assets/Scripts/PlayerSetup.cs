using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable;

    Camera lobbyCamera;

    /// <summary>
    /// Disables scripts attached to all clients except the host, otherwise anyone can control anyone.
    /// </summary>
    private void Start()
    {
        if (!isLocalPlayer)
        {
            foreach (Behaviour component in componentsToDisable)
            {
                component.enabled = false;
            }
        }
        else
        {
            lobbyCamera = Camera.main;
            if (lobbyCamera != null)
            {
                lobbyCamera.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Reenables lobby camera.
    /// </summary>
    private void OnDisable()
    {
        if (lobbyCamera != null)
        {
            lobbyCamera.gameObject.SetActive(true);
        }
    }
}
