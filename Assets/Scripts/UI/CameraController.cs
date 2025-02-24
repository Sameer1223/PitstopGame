using UnityEngine;
using Unity.Netcode;

public class CameraController : NetworkBehaviour
{
    private GameObject cameraHolder;
    public override void OnNetworkSpawn()
    {
        cameraHolder = transform.GetChild(2).gameObject;
        if(IsOwner && IsClient)
        {
            cameraHolder.SetActive(true);
        }
        base.OnNetworkSpawn();
    }
}
