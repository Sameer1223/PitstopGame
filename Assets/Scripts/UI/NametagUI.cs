using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using Racing;

public class UIFaceCamera : NetworkBehaviour
{
    [SerializeField]
    private Camera activeCam;
    [SerializeField]
    private TMP_Text ownNameTag;


    private static List<Transform> allNametags = new List<Transform>();
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
        OrientNametag();
    }
    public void OrientNametag()
    {
        if(!IsOwner) return;
        foreach(var nametag in allNametags)
        {
            nametag.LookAt(activeCam.transform); //looks at the camera but backwards
            nametag.Rotate(0f, 180f, 0f); //flip around correct orientation
        }
    }
}
