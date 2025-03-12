using UnityEngine;

public class RaceManager : MonoBehaviour
{
    private static RaceManager _instance;

    public static RaceManager Instance { get { return _instance; }}

    [SerializeField]
    public GameObject[] checkpoints;
    public int totalLaps = 3;

    // Singleton Pattern
    private void Awake() {
        if (_instance != null && _instance != this){
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    public void CheckPlayerFinished(RaceLogic player) {
        if (player.lapCount > totalLaps) {
            Debug.Log("Player finished!");
        }
    }
}
