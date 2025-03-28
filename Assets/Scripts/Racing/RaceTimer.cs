using System;
using System.Collections;
using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;

namespace Racing
{
    public class RaceTimer : NetworkBehaviour
    {
        public static RaceTimer Instance { get; private set; }
        
        private Stopwatch raceStopwatch = new Stopwatch();
        
        public NetworkVariable<float> raceTimer = new NetworkVariable<float>(0f);
        private float lapStartTime = 0f;
        private bool isRunning;
        
        [Header("UI Components")]
        [SerializeField] public TextMeshProUGUI raceTimerText;
        [SerializeField] public TextMeshProUGUI lapTimerText;

        // Singleton Pattern
        private void Awake() {
            if (Instance != null && Instance != this){
                Destroy(gameObject);
            } else {
                Instance = this;
            }
        }
        
        private void Update()
        {
            if (isRunning && IsServer) raceTimer.Value = (float)raceStopwatch.Elapsed.TotalSeconds;
            
            float currentLapTime = raceTimer.Value - lapStartTime;
            UpdateTimerDisplay(raceTimer.Value, raceTimerText);
            UpdateTimerDisplay(currentLapTime, lapTimerText);
        }

        public void StartTimer()
        {
            if (!IsServer) return;
            raceStopwatch.Restart();
            isRunning = true;
            StartCoroutine(SyncRaceTime());
        }

        public void StopTimer()
        {
            if (!IsServer) return;
            raceStopwatch.Stop();
            isRunning = false;
        }

        public void RestartLapTimer()
        {
            if (!IsClient) return;
            lapStartTime = raceTimer.Value;
        }

        private IEnumerator SyncRaceTime()
        {
            while (isRunning)
            {
                raceTimer.Value = (float) raceStopwatch.Elapsed.TotalSeconds;
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        private void UpdateTimerDisplay(float time, TextMeshProUGUI displayText)
        {
            if (!displayText) return;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            int centiseconds = Mathf.FloorToInt((time * 100) % 100);
            displayText.text = $"{minutes:00}:{seconds:00}:{centiseconds:00}";
        }

        public string GetFormattedTime()
        {
            float time = (float) raceStopwatch.Elapsed.TotalSeconds;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            int centiseconds = Mathf.FloorToInt((time * 100) % 100);
            return $"{minutes:00}:{seconds:00}:{centiseconds:00}";
        }
    }
}