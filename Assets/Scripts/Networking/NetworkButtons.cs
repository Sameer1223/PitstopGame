using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkUIManager : MonoBehaviour {
    public Button clientButton;
    public Button hostButton;
    public Button serverButton;

    void Start() {
        clientButton.onClick.AddListener(StartClient);
        hostButton.onClick.AddListener(StartHost);
        serverButton.onClick.AddListener(StartServer);
    }

    void StartClient() {
        Debug.Log("Starting Client...");
        NetworkManager.Singleton.StartClient();
    }

    void StartHost() {
        Debug.Log("Starting Host...");
        NetworkManager.Singleton.StartHost();
    }

    void StartServer() {
        Debug.Log("Starting Server...");
        NetworkManager.Singleton.StartServer();
    }
}