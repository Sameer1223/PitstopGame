using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    private float currentSpeed = 0.0f;
    private TMP_Text speedometerText;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        speedometerText = transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
    }
    private void UpdateUI() {
        currentSpeed = rb.linearVelocity.magnitude * 2.237f;
        speedometerText.text = currentSpeed.ToString("N0") + " mph";
    }
    private void FixedUpdate()
    {
        UpdateUI();
    }
}
