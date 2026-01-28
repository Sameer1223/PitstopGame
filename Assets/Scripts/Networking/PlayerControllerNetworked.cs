using Racing;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerControllerNetworked: NetworkBehaviour {

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool isBraking;
    private Rigidbody rb;
    private bool controlsEnabled = false;
    private CarStatsRuntime carStats;

    [SerializeField]
    Transform FL, FR;
    
    private void Awake() {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        carStats = GetComponent<CarStatsRuntime>();
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) return;

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Brake.performed += ctx => isBraking = true;
        inputActions.Player.Brake.canceled += ctx => isBraking = false;

        inputActions.Enable();

        // Center of mass
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    private void FixedUpdate()  {
        if (!IsOwner || !controlsEnabled) return;

        MoveServerRpc(moveInput);
    }
    
    public void EnableControls() { controlsEnabled = true; }

    public void DisableControls() { controlsEnabled = false; }

    [ServerRpc]
    private void MoveServerRpc(Vector2 input) {
        MoveClientRpc(input);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector2 input) {
        ForwardAcceleration(input);
        RollingResistance(input);
        Turning(input);
        Braking();
        DragAndDownforce();
    }

    private void ForwardAcceleration(Vector2 input) {
        float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
        if (!isBraking && speed < carStats.MaxSpeed){
            float accelerationFactor = Mathf.Lerp(1f, 0.7f, (rb.linearVelocity.magnitude / carStats.MaxSpeed) * 0.9f) * carStats.Acceleration;
            
            Vector3 forwardSpeed = transform.forward * (input.y * accelerationFactor * Time.fixedDeltaTime);
            rb.AddForce(forwardSpeed, ForceMode.Acceleration);
        }
    }

    private void RollingResistance(Vector2 input) {
        if (!isBraking && rb.linearVelocity.magnitude > 1f && Mathf.Abs(input.y) < 0.1f){
            
            float resistanceFactor = Mathf.Lerp(0.002f, 0.02f, Mathf.Log10(rb.linearVelocity.magnitude + 1) / Mathf.Log10(carStats.MaxSpeed + 1));
            Vector3 resistanceForce = -rb.linearVelocity.normalized * resistanceFactor;
            rb.AddForce(resistanceForce, ForceMode.Force);
        }
    }

    private void Turning(Vector2 input) {
        if (Mathf.Abs(input.x) > 0.1f && rb.linearVelocity.magnitude > carStats.MinimumTurnSpeed) {
            float reverseMultiplier = Vector3.Dot(rb.linearVelocity, transform.forward) < 0 ? -1f : 1f;

            Quaternion rotation = Quaternion.Euler(Vector3.up * (input.x * carStats.Handling * Time.fixedDeltaTime * reverseMultiplier));
            rb.MoveRotation(rb.rotation * rotation);

            RotateWheels(input);
        } else {
            ResetWheels();
        }
    }

    
    private void Braking() {
        if (isBraking) {
            float brakeStrength = Mathf.Lerp(0.6f, 1f, 1 - (rb.linearVelocity.magnitude / carStats.MaxSpeed));
            
            rb.AddForce(-transform.forward * (carStats.BrakeForce * brakeStrength), ForceMode.Force);
        }
    }

    private void DragAndDownforce() {
        rb.AddForce(-transform.up * carStats.Downforce);

        Vector3 lateralVelocity = Vector3.Dot(rb.linearVelocity, transform.right) * transform.right;
        Vector3 gripForce = -lateralVelocity * (rb.mass * carStats.Traction);
        rb.AddForce(gripForce, ForceMode.Force);
    }

    private void RotateWheels(Vector2 input) {
        FL.localRotation = Quaternion.Lerp(FL.localRotation, Quaternion.Euler(0, input.x * 30, 0), 
                                            carStats.WheelTurnSpeed * Time.fixedDeltaTime);
        FR.localRotation = Quaternion.Lerp(FR.localRotation, Quaternion.Euler(0, input.x * 30, 0), 
            carStats.WheelTurnSpeed * Time.fixedDeltaTime);
    }

    private void ResetWheels() {
        FL.localRotation = Quaternion.Lerp(FL.localRotation, Quaternion.identity, carStats.WheelTurnSpeed * Time.fixedDeltaTime);
        FR.localRotation = Quaternion.Lerp(FR.localRotation, Quaternion.identity, carStats.WheelTurnSpeed * Time.fixedDeltaTime);
    }
}