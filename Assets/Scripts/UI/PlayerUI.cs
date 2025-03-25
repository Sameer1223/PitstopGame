using UnityEngine;
using TMPro;
using Unity.Netcode;


public class PlayerUI : NetworkBehaviour
{
    private const float MAGNITUDE_TO_MPH = 2.237f;
    private const float MAGNITUDE_TO_KPH = 3.6f;
    private float currentSpeed = 0.0f;
    [SerializeField]
    private TMP_Text speedometerText;
    private Rigidbody rb;

    private void Awake()
    {
        if (!IsOwner) {speedometerText.text = "";}
        rb = GetComponent<Rigidbody>();
    }
    private void UpdateUI() {
        float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
        currentSpeed = speed * MAGNITUDE_TO_KPH;
        speedometerText.text = currentSpeed.ToString("N0") + " kph";
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        UpdateUI();
    }
}
