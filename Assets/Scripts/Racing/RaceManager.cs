using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Racing
{
    public class RaceManager : NetworkBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [SerializeField]
        public GameObject[] checkpoints;
        public List<Racer> racers = new();
        public int totalLaps = 3;

        // Singleton Pattern
        private void Awake() {
            if (Instance != null && Instance != this){
                Destroy(gameObject);
            } else {
                Instance = this;
            }
        }

        public void CheckPlayerFinished(Racer player) {
            if (player.lapCount.Value > totalLaps) {
                Debug.Log("Player finished!");
                EndRaceServerRpc(player.gameObject.name);
            }
        }
        
        [ServerRpc]
        private void EndRaceServerRpc(string winnerId)
        {
            Debug.Log($"Player {winnerId} wins! Ending race...");
            //Time.timeScale = 0; // Stop the race
        }
    }
}