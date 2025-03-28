using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

public class UIFaceCamera : NetworkBehaviour
{
    [SerializeField]
    private Camera activeCam;
    [SerializeField]
    private TMP_Text ownNameTag;

    private List<Transform> allNametags = new List<Transform>();
    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            ownNameTag.gameObject.SetActive(false);
        }
        else
        {
            allNametags.Add(transform);
        }
        base.OnNetworkSpawn();
    }
    public void Update()
    {
        OrientNametagClientRpc();
    }
    [ClientRpc]
    public void OrientNametagClientRpc()
    {
        // Debug.Log("looking at: " + activeCam.name);
        for(int i = 0; i < allNametags.Count; i++)
        {
            allNametags[i].LookAt(activeCam.transform); //looks at the camera but backwards
            allNametags[i].RotateAround(allNametags[i].position, allNametags[i].up, 180f); //flip around correct orientation
        }
    }
}
