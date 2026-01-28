using UnityEngine;

namespace Racing
{
    [CreateAssetMenu(fileName = "NewCar", menuName = "Racing/Car Stats")]                                                                                                                                                                                                                                            
    public class CarStatsSO: ScriptableObject
    {
        [Header("Identity")]
        public string carName;
        
        // these are the base stats for every car
        [Header("Upgradeable Stats")]
        public float maxMovementSpeed = 20.0f;
        public float acceleration = 2000f;
        public float handling = 90.0f;
        public float traction = 4f;
        
        //fixed stats per car
        [Header("Fixed Stats")]
        public float minimumTurnSpeed = 0.2f;
        public float wheelTurnSpeed = 3f;
        public float drag = 0.98f;
        public float downforce = 500f;
        public float brakeForce = 3000f;
    }
}