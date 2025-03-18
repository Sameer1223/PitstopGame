using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Racing
{
    public class RaceManager : NetworkBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [SerializeField] public GameObject[] checkpoints;
        [SerializeField] public TextMeshProUGUI raceMessageText;
        public GameObject playerPrefab;
        public NetworkList<NetworkObjectReference> Racers = new NetworkList<NetworkObjectReference>();
        public int totalLaps = 3;
        private bool raceStarted = false;

        // Singleton Pattern
        private void Awake() {
            if (Instance != null && Instance != this){
                Destroy(gameObject);
            } else {
                Instance = this;
            }
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        
        private void OnClientConnected(ulong clientId)
        {
            if (!IsOwner) return;
            SetSpawnServerRpc(clientId);
        }

        private void Update()
        {
            if (IsServer && !raceStarted && Input.GetKeyDown(KeyCode.Space))
            {
                raceStarted = true;
                StartRaceServerRpc();
            }
        }

        public void CheckPlayerFinished(Racer player, string playerName) {
            if (player.lapCount.Value > totalLaps) {
                EndRaceServerRpc(playerName);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartRaceServerRpc()
        {
            StartCoroutine(RaceCountdown());
        }

        private IEnumerator RaceCountdown()
        {
            for (int i = 3; i > 0; i--)
            {
                UpdateRaceMessageTextClientRpc(i.ToString());
                yield return new WaitForSeconds(1f);
            }

            UpdateRaceMessageTextClientRpc("Go!");
            EnablePlayerInputClientRpc();
            
            yield return new WaitForSeconds(1f);
            UpdateRaceMessageTextClientRpc("");
        }

        [ClientRpc]
        private void UpdateRaceMessageTextClientRpc(string text)
        {
            raceMessageText.text = text;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetSpawnServerRpc(ulong clientId)
        {
            Transform spawnPoint = SpawnManager.Instance.GetNextSpawnPoint();
            if (spawnPoint == null) return;
            
            GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation); 
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

            NetworkObjectReference playerReference = new NetworkObjectReference(player.GetComponent<NetworkObject>());
            Racers.Add(playerReference);
            DisablePlayerInputClientRpc(playerReference);
        }

        [ClientRpc]
        private void DisablePlayerInputClientRpc(NetworkObjectReference playerReference)
        {
            if (playerReference.TryGet(out NetworkObject playerObject))
            {
                PlayerControllerNetworked playerController = playerObject.GetComponent<PlayerControllerNetworked>();
                if (playerController == null) return;
                playerController.DisableControls();
            }
        }

        [ClientRpc]
        private void EnablePlayerInputClientRpc()
        {
            foreach (var racerRef in Racers)
            {
                if (racerRef.TryGet(out NetworkObject racerObject))
                {
                    PlayerControllerNetworked playerController = racerObject.GetComponent<PlayerControllerNetworked>();
                    Debug.Log("Enabling");
                    if (playerController != null) playerController.EnableControls();
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void EndRaceServerRpc(string playerName)
        { 
            EndRaceClientRpc(playerName);
        }

        [ClientRpc]
        private void EndRaceClientRpc(string playerName)
        {
            if (raceMessageText == null) return;
            UpdateRaceMessageTextClientRpc($"{playerName} has won!");
            Invoke(nameof(HideText), 5f);
        }

        private void HideText()
        {
            raceMessageText.gameObject.SetActive(false);
        }
    }
}