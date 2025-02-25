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

    private void Turning(Vector2 input) {
        if (Mathf.Abs(input.x) > 0.1f && rb.linearVelocity.magnitude > carTuning.minimumTurnSpeed) {
            //transform.Rotate(0, input.x * tiltAngle * Time.fixedDeltaTime, 0, Space.Self);
            Quaternion rotation = Quaternion.Euler(Vector3.up * input.x * carTuning.tiltAngle * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation * rotation);

            FL.localRotation = Quaternion.Lerp(FL.localRotation, Quaternion.Euler(0, input.y * input.x * 30, 0), 
                                                carTuning.wheelTurnSpeed * Time.fixedDeltaTime);
            FR.localRotation = Quaternion.Lerp(FR.localRotation, Quaternion.Euler(0, input.y * input.x * 30, 0), 
                                                carTuning.wheelTurnSpeed * Time.fixedDeltaTime);
        } else {
            FL.localRotation = Quaternion.Lerp(FL.localRotation, Quaternion.identity, 
                                                carTuning.wheelTurnSpeed * Time.fixedDeltaTime);
            FR.localRotation = Quaternion.Lerp(FR.localRotation, Quaternion.identity, 
                                                carTuning.wheelTurnSpeed * Time.fixedDeltaTime);
        }
    }

    private void Braking() {
        //rb.linearDamping = isBraking? carTuning.brakeForce: 0.1f;
        if (isBraking) rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, carTuning.brakeForce * Time.fixedDeltaTime);
    }

    private void DragAndDownforce() {
        rb.AddForce(-transform.up * carTuning.downforce);
    }

}