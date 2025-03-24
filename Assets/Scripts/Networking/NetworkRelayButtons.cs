using System.Collections;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
    public class NetworkRelayButtons : MonoBehaviour
    {
        [Header("UI Elements")] public Button startHostButton;
        public Button startClientButton;
        public TMP_Text statusText;
        public TMP_Text errorText;

        // Reference to the Input Field for the join code
        public TMP_InputField joinCodeInputField;

        private RelayManager relayManager;
        private string joinCode; // To store the join code entered by the user

        private void Start()
        {
            // Find the RelayManager in the scene
            relayManager = FindObjectOfType<RelayManager>();

            // Check if RelayManager was found
            if (relayManager == null)
            {
                Debug.LogError("RelayManager not found in the scene!");
                return;
            }

            // Set up button listeners
            startHostButton.onClick.AddListener(OnStartHostClicked);
            startClientButton.onClick.AddListener(OnStartClientClicked);
        }

        private void OnStartHostClicked()
        {
            statusText.SetText("Starting Host...");

            relayManager.StartHost(joinCode =>
            {
                statusText.SetText("Host started! Join code: " + joinCode);
                Debug.Log("Relay Join Code: " + joinCode);

                // After the host starts, you can update the input field or save the join code for later use.
                joinCodeInputField.text = joinCode; // Update the input field with the generated join code
            });
        }

        private void OnStartClientClicked()
        {
            // Get the join code from the input field
            joinCode = joinCodeInputField.text;

            // Check if the join code is valid
            if (string.IsNullOrEmpty(joinCode))
            {
                statusText.SetText("Please enter a valid join code!");
                return;
            }

            statusText.text = "Connecting to host...";
            relayManager.StartClient(joinCode, (success, errorMessage) =>
            {
                if (success)
                {
                    statusText.SetText("Client started, connecting to host...");
                }
                else
                {
                    errorText.SetText(errorMessage);
                }
            });
        }
    }
}