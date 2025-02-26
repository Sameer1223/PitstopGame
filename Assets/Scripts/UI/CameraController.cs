using UnityEngine;
using Unity.Netcode;

public class CameraController : NetworkBehaviour
{
    [SerializeField]
    private GameObject cameraHolder;
    public override void OnNetworkSpawn()
    {
        if(IsOwner && IsClient)
        {
            cameraHolder.SetActive(true);
        }
        base.OnNetworkSpawn();
    }
}
