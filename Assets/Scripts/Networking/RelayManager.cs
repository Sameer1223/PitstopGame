using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
    public class RelayManager : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button startHostButton;
        public Button startClientButton;
        public TMP_InputField joinCodeInputField;
        public TMP_Text statusText;

        private async void Start()
        {
            if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
            {
                try
                {
                    await UnityServices.InitializeAsync();

                    // Sign into Authentication Service
                    if (!AuthenticationService.Instance.IsSignedIn)
                    {
                        await AuthenticationService.Instance.SignInAnonymouslyAsync();
                        Debug.Log("Signed in anonymously!");
                    }

                    Debug.Log("Unity Services Initialized Successfully");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to initialize Unity Services: " + e.Message);
                }
            }
        
            startHostButton.onClick.AddListener(OnStartHostClicked);
            startClientButton.onClick.AddListener(OnStartClientClicked);
        }

        private async void OnStartHostClicked()
        {
            statusText.text = "Starting Host...";
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(7);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                Debug.Log("Join Code: " + joinCode);
                joinCodeInputField.text = joinCode;

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort) allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                NetworkManager.Singleton.StartHost();
                statusText.text = "Host started! Share this code: " + joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
                statusText.text = "Error starting host: " + e.Message;
            }
        }

        private async void OnStartClientClicked()
        {
            string joinCode = joinCodeInputField.text.Trim();
        
            if (string.IsNullOrEmpty(joinCode))
            {
                statusText.text = "Please enter a valid join code.";
                return;
            }
        
            statusText.text = "Joining Host...";
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort) joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );

                NetworkManager.Singleton.StartClient();
                statusText.text = "Client started, connecting to host...";
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
                statusText.text = "Error joining host: " + e.Message;
            }
        }
    }
}