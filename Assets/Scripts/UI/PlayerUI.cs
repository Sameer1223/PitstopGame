using Racing;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace UI
{
    public class PlayerUI : NetworkBehaviour
    {
        //private const float MAGNITUDE_TO_MPH = 2.237f;
        private const float MAGNITUDE_TO_KPH = 3.6f;
        private float currentSpeed = 0.0f;
    
        [Header("UI Components")]
        [SerializeField] private TMP_Text lapCountText;
        [SerializeField] private TMP_Text speedometerText;
        [SerializeField] private TMP_Text racePositionText;
        [SerializeField] private TMP_Text frontPositionText;
        [SerializeField] private TMP_Text backPositionText;
        [SerializeField] private TMP_Text firstPositionText;
        [SerializeField] private TMP_Text selfPositionText;
        [SerializeField] private GameObject playerUIObject;
    
        private Rigidbody rb;
        private Racer racer;

        private void Awake()
        {
            if (!IsOwner)
            {
                speedometerText.text = "";
                lapCountText.text = "";
            }
        
            rb = GetComponent<Rigidbody>();
            racer = GetComponent<Racer>();
        }

        public override void OnNetworkSpawn()
        {
            if(IsOwner && IsClient)
            {
                playerUIObject.SetActive(true);
            }
            base.OnNetworkSpawn();
            if (!IsOwner) return;
        
            lapCountText.text = $"Lap 1 / {RaceManager.Instance.totalLapsValue}";
            racer.lapCount.OnValueChanged += UpdateLapCountText;
        }

        private void FixedUpdate()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (!IsOwner) return;
            UpdateSpeedometerText();
        }

        private void UpdateLapCountText(int oldLapCount, int newLapCount)
        {
            if (oldLapCount == RaceManager.Instance.totalLapsValue) return;
            lapCountText.text = $"Lap {newLapCount} / {RaceManager.Instance.totalLapsValue}";
        }

        private void UpdateSpeedometerText() {
            float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
            currentSpeed = speed * MAGNITUDE_TO_KPH;
            speedometerText.text = currentSpeed.ToString("N0") + " kph";
        }

        public void UpdateRacePosition(string positionText, string selfName)
        {
            if (!IsOwner) return;
            racePositionText.text = positionText;
            selfPositionText.text = positionText + ") " + selfName;
        }

        public void UpdateFrontRacerName(string posNum, string posName)
        {
            if (!IsOwner) return;
            frontPositionText.text = posNum + ") " + posName;
        }
        public void UpdateBackRacerName(string posNum, string posName)
        {
            if (!IsOwner) return;
            backPositionText.text = posNum + ") " + posName;
        }
        public void UpdateFirstRacerName(string posName)
        {
            if (!IsOwner) return;
            firstPositionText.text = "1st) " + posName;
        }
        public void ClearFrontRacerName()
        {
            if (!IsOwner) return;
            frontPositionText.text = "";
        }
        public void ClearBackRacerName()
        {
            if (!IsOwner) return;
            backPositionText.text = "";
        }
    }
}