using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable;

    Camera lobbyCamera;

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

    private void OnDisable()
    {
        if (lobbyCamera != null)
        {
            lobbyCamera.gameObject.SetActive(true);
        }
    }
}
