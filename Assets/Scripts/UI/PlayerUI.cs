using System;
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
        [SerializeField] private TMP_Text penaltyTimeText;
        [SerializeField] private TMP_Text warningText;
    
        private Rigidbody rb;
        private Racer racer;

        private void Awake()
        {
            if (!IsOwner)
            {
                speedometerText.text = "";
                lapCountText.text = "";
                penaltyTimeText.text = "";
                warningText.text = "";
            }
        
            rb = GetComponent<Rigidbody>();
            racer = GetComponent<Racer>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner) return;
        
            lapCountText.text = $"Lap 1 / {RaceManager.Instance.totalLapsValue}";
            racer.lapCount.OnValueChanged += UpdateLapCountText;
            penaltyTimeText.text = "";
        }

        private void FixedUpdate()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (!IsOwner) return;
            UpdateSpeedometerText();
            UpdatePenaltyTimeText();
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

        private void UpdatePenaltyTimeText()
        {
            warningText.text = racer.warningActive ? "!" : "";;
            if (racer.penaltySeconds == 0) return;
            penaltyTimeText.text = $"+{racer.penaltySeconds}s";
        }

        public void UpdateRacePosition(string positionText)
        {
            if (!IsOwner) return;
            racePositionText.text = positionText;
        }
    }
}