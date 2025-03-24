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
            var allocation = await RelayService.Instance.CreateAllocationAsync(4);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            NetworkManager.Singleton.StartHost();
            onHostStarted?.Invoke(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Error starting host: " + e.Message);
        }
    }

    public async void StartClient(string joinCode, System.Action<bool, string> callback)
    {
        try
        {
            Debug.Log("Attempting to join relay with code: " + joinCode);

            // Attempt to join the relay session
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log("Successfully joined relay.");
            callback(true, null); // Success, no error message
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay Join Failed: " + e.Message);
            callback(false, e.Message); // Pass the error message to the UI
        }
    }
}