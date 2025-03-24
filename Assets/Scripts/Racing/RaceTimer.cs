using System;
using System.Collections;
using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;
using TMPro;

namespace Racing
{
    public class RaceTimer : NetworkBehaviour
    {
        private Stopwatch stopwatch = new Stopwatch();
        public NetworkVariable<float> raceTimer = new NetworkVariable<float>(0f);
        public TextMeshProUGUI timerText;
        private bool isRunning = false;

        private void Update()
        {
            if (isRunning && IsServer) raceTimer.Value = (float) stopwatch.Elapsed.TotalSeconds;
            UpdateTimerDisplay(raceTimer.Value);
        }

        public void StartTimer()
        {
            if (!IsServer) return;
            stopwatch.Restart();
            isRunning = true;
            StartCoroutine(SyncRaceTime());
        }

        public void StopTimer()
        {
            if (!IsServer) return;
            stopwatch.Stop();
            isRunning = false;
        }

        private IEnumerator SyncRaceTime()
        {
            while (isRunning)
            {
                raceTimer.Value = (float) stopwatch.Elapsed.TotalSeconds;
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        private void UpdateTimerDisplay(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            int centiseconds = Mathf.FloorToInt((time * 100) % 100);
            if (timerText != null) timerText.text = $"{minutes:00}:{seconds:00}:{centiseconds:00}";
        }

        public string GetFormattedTime()
        {
            float time = (float) stopwatch.Elapsed.TotalSeconds;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            int centiseconds = Mathf.FloorToInt((time * 100) % 100);
            return $"{minutes:00}:{seconds:00}:{centiseconds:00}";
        }
    }
}