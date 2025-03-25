using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkUIManager : MonoBehaviour {
    [Header("UI Elements")] 
    public Button startHostButton;
    public Button startClientButton;
    public TMP_InputField joinCodeInputField;
    public TMP_Text statusText;
    private string joinCode;

    void Start() {
        startClientButton.onClick.AddListener(StartClient);
        startHostButton.onClick.AddListener(StartHost);
    }

    void StartClient() {
        Debug.Log("Starting Client...");
        NetworkManager.Singleton.StartClient();
    }

    void StartHost() {
        Debug.Log("Starting Host...");
        NetworkManager.Singleton.StartHost();
    }
}