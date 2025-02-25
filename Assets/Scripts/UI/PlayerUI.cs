using UnityEngine;
using TMPro;


public class PlayerUI : MonoBehaviour
{
    private const float MAGNITUDE_TO_MPH = 2.237f;
    private float currentSpeed = 0.0f;
    [SerializeField]
    private TMP_Text speedometerText;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void UpdateUI() {
        currentSpeed = rb.linearVelocity.magnitude * MAGNITUDE_TO_MPH;
        speedometerText.text = currentSpeed.ToString("N0") + " mph";
    }
    private void FixedUpdate()
    {
        UpdateUI();
    }
}
