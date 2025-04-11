using System;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

namespace Racing
{
    public class Racer : NetworkBehaviour
    {
        public NetworkVariable<int> lapCount = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> checkpointIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> distToNextCheckpoint = new NetworkVariable<float>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        private GameObject[] points;
        public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public bool warningActive;
        public int penaltySeconds;
        
        private void Start()
        {
            if (!IsOwner) return;
            points = RaceManager.Instance.checkpoints;
        }

        public override void OnNetworkSpawn()
        {
            if(IsOwner)
            {
                string tempPlayerName = "Player " + (RaceManager.Instance.Racers.Count + 1);
                playerName.Value = tempPlayerName;
            }
            base.OnNetworkSpawn();
        }

        private void Update()
        {
            if (!IsOwner) return;
            var nextIndex = (checkpointIndex.Value + 1) % points.Length;
            distToNextCheckpoint.Value = Vector3.Distance(transform.position, points[nextIndex].transform.position);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsOwner || (!other.CompareTag("Checkpoint") && !other.CompareTag("Corner"))) return;
            
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            var idx = checkpoint.index;
            
            var nextCheckpoint = (checkpointIndex.Value + 1) % points.Length;
            if (nextCheckpoint != idx) return;
            
            if (other.CompareTag("Corner"))
            {
                if (warningActive) penaltySeconds += 5;
                warningActive = !warningActive;
                Debug.Log("Warning! Penalty Seconds: " + penaltySeconds);
            }
                
            if (nextCheckpoint == points.Length - 1)
            {
                RaceTimer.Instance.RestartLapTimer();
                lapCount.Value++;
            }
            checkpointIndex.Value = nextCheckpoint;

            RaceManager.Instance.CheckPlayerFinished(this, playerName.Value.ToString(), penaltySeconds);
            Debug.Log((lapCount.Value, checkpointIndex.Value));
        }
    }
}
