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
            Debug.LogWarning("a");
            cameraHolder.SetActive(true);
        }
        base.OnNetworkSpawn();
    }
}
