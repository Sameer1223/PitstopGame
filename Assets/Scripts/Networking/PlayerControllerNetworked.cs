using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerNetworkedController : NetworkBehaviour {

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Rigidbody rb;

    // Variables for UI
    private float currentSpeed = 0.0f;
    private TMP_Text speedometerText;



    // Controller variables
    private float maxMovementSpeed = 15.0f;
    private float acceleration = 1000f;
    private float tiltAngle = 90.0f;

    private void Awake() {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        speedometerText = transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>();
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) return;

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Enable();
    }

    private void FixedUpdate()  {
        if (!IsOwner) return;

        MoveServerRpc(moveInput);
        UpdateUI();
    }

    private void UpdateUI() {
        currentSpeed = rb.linearVelocity.magnitude * 2.237f;
        speedometerText.text = currentSpeed.ToString("N0") + " mph";
    }
    [ServerRpc]
    private void MoveServerRpc(Vector2 input) {
        MoveClientRpc(input);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector2 input) {
        // Forward acceleration
        if (rb.linearVelocity.magnitude < maxMovementSpeed){
            Vector3 forwardSpeed = transform.forward * input.y * acceleration * Time.fixedDeltaTime;
            rb.AddForce(forwardSpeed, ForceMode.Acceleration);
        }

        // Turning
        if (rb.linearVelocity.magnitude > 0.3f) {
            transform.Rotate(0, input.x * tiltAngle * Time.fixedDeltaTime, 0, Space.Self);
        }
    }
}