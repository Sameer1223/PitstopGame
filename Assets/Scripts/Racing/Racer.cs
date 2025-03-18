using System;
using Unity.Netcode;
using UnityEngine;

namespace Racing
{
    public class Racer : NetworkBehaviour
    {
        public NetworkVariable<int> lapCount = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> checkpointIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private GameObject[] points;
        private string playerName;

        private void Start()
        {
            if (!IsOwner) return;
            points = RaceManager.Instance.checkpoints;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsOwner) return;
            playerName = "Player " + (RaceManager.Instance.Racers.Count + 1);
            Debug.Log(playerName);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsOwner || !other.CompareTag("Checkpoint")) return;
            
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            var idx = checkpoint.index;
            
            var nextCheckpoint = (checkpointIndex.Value + 1) % points.Length;
            if (nextCheckpoint == idx) {
                if (nextCheckpoint == points.Length - 1) lapCount.Value++;
                checkpointIndex.Value = nextCheckpoint;

                RaceManager.Instance.CheckPlayerFinished(this, playerName);
                Debug.Log((lapCount.Value, checkpointIndex.Value));
            }
        }
    }
}
