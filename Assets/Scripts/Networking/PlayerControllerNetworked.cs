using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerNetworkedController : NetworkBehaviour {

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool isBraking;
    private Rigidbody rb;

    [SerializeField]
    Transform FL, FR;

    // Controller variables
    [System.Serializable]
    public class CarTuning {
        public float maxMovementSpeed = 20.0f;
        public float acceleration = 2000f;
        public float tiltAngle = 90.0f;
        public float minimumTurnSpeed = 0.2f;
        public float wheelTurnSpeed = 3f;
        public float drag = 0.98f;
        public float downforce = 500f;
        public float brakeForce = 3f;
    }
    
    public CarTuning carTuning = new CarTuning();

    private void Awake() {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

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
        if (!IsOwner) return;

        MoveServerRpc(moveInput);
    }

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
        Debug.Log(rb.linearVelocity.magnitude);
    }

    private void ForwardAcceleration(Vector2 input) {
        if (rb.linearVelocity.magnitude < carTuning.maxMovementSpeed){
            Vector3 forwardSpeed = transform.forward * input.y * carTuning.acceleration * Time.fixedDeltaTime;
            rb.AddForce(forwardSpeed, ForceMode.Acceleration);
        }
    }

    private void RollingResistance(Vector2 input) {
        if (rb.linearVelocity.magnitude > 1f && Mathf.Abs(input.y) < 0.1f){
            if (rb.linearVelocity.magnitude > 10f) return;
            rb.sleepThreshold = 0f; 
            
            //float resistanceFactor = Mathf.Clamp(rb.linearVelocity.magnitude * 0.005f, 0.01f, 0.1f);
            float resistanceFactor = Mathf.Lerp(0.001f, 0.1f, 1f - Mathf.Clamp01(rb.linearVelocity.magnitude / carTuning.maxMovementSpeed));
            Vector3 resistanceForce = -rb.linearVelocity.normalized * resistanceFactor;
            rb.AddForce(resistanceForce, ForceMode.Acceleration);
        }
    }

    private void Turning(Vector2 input) {
        if (Mathf.Abs(input.x) > 0.1f && rb.linearVelocity.magnitude > carTuning.minimumTurnSpeed) {
            float reverseMultiplier = Vector3.Dot(rb.linearVelocity, transform.forward) < 0 ? -1f : 1f;

            Quaternion rotation = Quaternion.Euler(Vector3.up * input.x * carTuning.tiltAngle * Time.fixedDeltaTime * reverseMultiplier);
            rb.MoveRotation(rb.rotation * rotation);

            RotateWheels(input);
        } else {
            ResetWheels();
        }
    }

    
    private void Braking() {
        rb.linearDamping = isBraking? carTuning.brakeForce: 0f;
        /*if (isBraking) {
            rb.linearDamping = Mathf.Lerp(rb.linearDamping, 10f, Time.fixedDeltaTime * 0.5f);
        } else {
            rb.linearDamping = 0.1f;
        }*/
    }

    private void DragAndDownforce() {
        rb.AddForce(-transform.up * carTuning.downforce);
    }

    private void RotateWheels(Vector2 input) {
        FL.localRotation = Quaternion.Lerp(FL.localRotation, Quaternion.Euler(0, input.x * 30, 0), 
                                            carTuning.wheelTurnSpeed * Time.fixedDeltaTime);
        FR.localRotation = Quaternion.Lerp(FR.localRotation, Quaternion.Euler(0, input.x * 30, 0), 
                                            carTuning.wheelTurnSpeed * Time.fixedDeltaTime);
    }

    private void ResetWheels() {
        FL.localRotation = Quaternion.Lerp(FL.localRotation, Quaternion.identity, carTuning.wheelTurnSpeed * Time.fixedDeltaTime);
        FR.localRotation = Quaternion.Lerp(FR.localRotation, Quaternion.identity, carTuning.wheelTurnSpeed * Time.fixedDeltaTime);
    }
}