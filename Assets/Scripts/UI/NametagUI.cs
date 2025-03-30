using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using Racing;
using Unity.Collections;

public class UIFaceCamera : NetworkBehaviour
{
    [SerializeField]
    private Camera activeCam;
    [SerializeField]
    private TMP_Text thisNameTag;


    private static List<Transform> allNametags = new List<Transform>();
    public void Start()
    {
        if(IsOwner)
        {
            Debug.Log("my name: " + transform.parent.GetComponent<Racer>().playerName.Value.ToString());
            thisNameTag.gameObject.SetActive(false);
        }
        else
        {
            allNametags.Add(transform);
            thisNameTag.text = transform.parent.GetComponent<Racer>().playerName.Value.ToString();
            Debug.Log("from nametag: " + transform.parent.GetComponent<Racer>().playerName.Value.ToString());
            transform.parent.GetComponent<Racer>().playerName.OnValueChanged += OnNameChanged;
        }
        base.OnNetworkSpawn();
    }
    private void OnNameChanged(FixedString32Bytes prev, FixedString32Bytes curr)
    {
        Debug.Log("name changed: " + curr);
        thisNameTag.text = transform.parent.GetComponent<Racer>().playerName.Value.ToString();
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
