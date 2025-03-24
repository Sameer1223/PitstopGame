using System.Collections;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    private string joinCode;

    private async void Awake()
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
    }

    public async void StartHost(System.Action<string> onHostStarted)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(1);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            NetworkManager.Singleton.StartHost();
            onHostStarted?.Invoke(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Error starting host: " + e.Message);
        }
    }

    public async void StartClient(string joinCode)
    {
        try
        {
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Error starting client: " + e.Message);
        }
    }
}