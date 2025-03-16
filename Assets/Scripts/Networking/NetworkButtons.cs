using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class NetworkUIManager : MonoBehaviour {
    public Button clientButton;
    public Button hostButton;
    public Button serverButton;
    public TMP_InputField inputField;
    public TMP_Text hostText;

    void Start() {
        clientButton.onClick.AddListener(StartClient);
        hostButton.onClick.AddListener(StartHost);
        serverButton.onClick.AddListener(StartServer);
    }

    void StartClient() {
        Debug.Log("Starting Client...");
        NetworkManager.Singleton.StartClient();
        Debug.Log(inputField.text);
    }

    void StartHost() {
        Debug.Log("Starting Host...");
        NetworkManager.Singleton.StartHost();
        hostText.text = "some text";
        hostText.gameObject.SetActive(true);
    }

    void StartServer() {
        Debug.Log("Starting Server...");
        NetworkManager.Singleton.StartServer();
    }
}