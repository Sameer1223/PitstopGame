using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;

public class FourWDPlayerNetworkedController : NetworkBehaviour {

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool isBraking;
    private Rigidbody rb;

    // Car parameters
    public float maxSpeed = 200f;
    public float maxSteerAngle = 30f;
    public float motorTorque = 1500f;
    public float brakeTorque = 3000f;
    public float engineBrakeForce = 200f;
    public float driftFactor = 0.95f; // Lower value = more drifting
    
    // Wheel colliders
    public WheelCollider frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel;
    public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;

    private float horizontalInput;
    private float verticalInput;

    private void Awake() {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) return;

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Brake.performed += ctx => isBraking = true;
        inputActions.Player.Brake.canceled += ctx => isBraking = false;

        inputActions.Enable();
    }

    private void FixedUpdate()  {
        if (!IsOwner) return;

        MoveServerRpc(moveInput);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 input) {
        MoveClientRpc(input);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector2 input) {
        verticalInput = input.y;
        horizontalInput = input.x;

        // Apply engine and braking forces
        ApplyThrottleAndBrakes();

        // Apply steering based on input
        ApplySteering();

        // Optional: Apply engine braking for deceleration
        ApplyEngineBraking();

        UpdateWheelPositions();
    }

    void ApplyThrottleAndBrakes()
    {
        // Apply motor torque for acceleration
        if (verticalInput > 0)
        {
            frontLeftWheel.motorTorque = motorTorque * verticalInput;
            frontRightWheel.motorTorque = motorTorque * verticalInput;
        }
        else if (verticalInput < 0)
        {
            frontLeftWheel.motorTorque = motorTorque * verticalInput;
            frontRightWheel.motorTorque = motorTorque * verticalInput;
        }

        // Apply brake torque if braking
        if (isBraking)
        {
            rearLeftWheel.brakeTorque = brakeTorque;
            rearRightWheel.brakeTorque = brakeTorque;
            frontLeftWheel.brakeTorque = brakeTorque;
            frontRightWheel.brakeTorque = brakeTorque;
        }
        else
        {
            rearLeftWheel.brakeTorque = 0f;
            rearRightWheel.brakeTorque = 0f;
            frontLeftWheel.brakeTorque = 0f;
            frontRightWheel.brakeTorque = 0f;
        }
    }

    void ApplySteering()
    {
        // Apply steering angle to the front wheels based on horizontal input
        float steerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheel.steerAngle = steerAngle;
        frontRightWheel.steerAngle = steerAngle;
    }

    void ApplyEngineBraking()
    {
        if (Mathf.Abs(verticalInput) < 0.1f && !isBraking) // Engine braking when not accelerating or braking
        {
            frontLeftWheel.brakeTorque = engineBrakeForce;
            frontRightWheel.brakeTorque = engineBrakeForce;
            rearLeftWheel.brakeTorque = engineBrakeForce;
            rearRightWheel.brakeTorque = engineBrakeForce;
        }
    }

    void UpdateWheelPositions()
    {
        UpdateWheelPosition(frontLeftWheel, frontLeftTransform);
        UpdateWheelPosition(frontRightWheel, frontRightTransform);
        UpdateWheelPosition(rearLeftWheel, rearLeftTransform);
        UpdateWheelPosition(rearRightWheel, rearRightTransform);
    }

    void UpdateWheelPosition(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
}