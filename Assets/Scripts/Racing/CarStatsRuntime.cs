using Unity.Netcode;
using UnityEngine;

namespace Racing
{
    public enum StatType {
        MaxSpeed,
        Acceleration,
        Handling,
        Traction
    }
    
    public class CarStatsRuntime : NetworkBehaviour {
        [SerializeField] private CarStatsSO baseStats;
        
        [Header("Upgrades")]
        private NetworkVariable<int> speedLevel = new NetworkVariable<int>(0);
        private NetworkVariable<int> accelLevel = new NetworkVariable<int>(0);
        private NetworkVariable<int> handlingLevel = new NetworkVariable<int>(0);
        private NetworkVariable<int> tractionLevel = new NetworkVariable<int>(0);
        
        // TODO: tune these values
        // how much each upgrade level increase each state
        private const float SPEED_PER_LEVEL = 2f;
        private const float ACCEL_PER_LEVEL = 200f;
        private const float HANDLING_PER_LEVEL = 5f;
        private const float TRACTION_PER_LEVEL = 0.4f;
        
        
        [Header("Stats")]
        // Final computed stats (upgradeable)
        public float MaxSpeed => CalculateStat(StatType.MaxSpeed);
        public float Acceleration => CalculateStat(StatType.Acceleration);
        public float Handling => CalculateStat(StatType.Handling);
        public float Traction => CalculateStat(StatType.Traction);
        // Pass-through stats (not upgradeable)
        public float MinimumTurnSpeed => baseStats.minimumTurnSpeed;
        public float WheelTurnSpeed => baseStats.wheelTurnSpeed;
        public float Drag => baseStats.drag;
        public float Downforce => baseStats.downforce;
        public float BrakeForce => baseStats.brakeForce;
        public string CarName => baseStats.carName;

        
        // TODO: use these when upgrading 
        [ServerRpc(RequireOwnership = false)]
        public void UpgradeStatServerRpc(StatType statType, int levels = 1)
        {
            UpgradeStat(statType, levels);
        }

        private void UpgradeStat(StatType statType, int levels = 1)
        {
            if (!IsServer) return;
    
            int currentLevel = GetStatLevel(statType);
            int newLevel = Mathf.Clamp(currentLevel + levels, 0, 10);
            SetStatLevel(statType, newLevel);
        }
        //TODO: use this when starting / selecting a car
        public void SetBaseStats(CarStatsSO stats)
        {
            baseStats = stats;
        }
        
        //TODO: use this when starting a new run
        public void ResetUpgrades()
        {
            if (!IsServer) return;
    
            speedLevel.Value = 0;
            accelLevel.Value = 0;
            handlingLevel.Value = 0;
            tractionLevel.Value = 0;
        }

        private void SetStatLevel(StatType statType, int level)
        {
            if (!IsServer) return;
            switch (statType)
            {
                case StatType.MaxSpeed:
                    speedLevel.Value = level;
                    break;
                case StatType.Acceleration:
                    accelLevel.Value = level;
                    break;
                case StatType.Handling:
                    handlingLevel.Value = level;
                    break;
                case StatType.Traction:
                    tractionLevel.Value = level;
                    break;
            }
        }

        /// <summary>
        /// Calculates the value of a stat.
        /// </summary>
        /// <param name="statType">The stat.</param>
        /// <returns>The stat's value after all upgrades.</returns>
        private float CalculateStat(StatType statType)  {
            float baseStat = GetBaseStat(statType);
            int level = GetStatLevel(statType);
            float increment = GetStatIncrement(statType);

            return baseStat + (level * increment);
        }
        
        /// <summary>
        /// Gets the base value of a stat
        /// </summary>
        /// <param name="statType">The stat.</param>
        /// <returns>The base value of statType</returns>
        private float GetBaseStat(StatType statType) {
            return statType switch
            {
                StatType.MaxSpeed => baseStats.maxMovementSpeed,
                StatType.Acceleration => baseStats.acceleration,
                StatType.Handling => baseStats.handling,
                StatType.Traction => baseStats.traction,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Gets the level of a stat
        /// </summary>
        /// <param name="statType">The stat.</param>
        /// <returns>The level of statType</returns>
        public int GetStatLevel(StatType statType)
        {
            return statType switch
            {
                StatType.MaxSpeed => speedLevel.Value,
                StatType.Acceleration => accelLevel.Value,
                StatType.Handling => handlingLevel.Value,
                StatType.Traction => tractionLevel.Value,
                _ => 0
            };
        }
        
        /// <summary>
        /// Gets the value to increment a stat by.
        /// </summary>
        /// <param name="statType">The stat.</param>
        /// <returns>The increment amount of statType</returns>
        private float GetStatIncrement(StatType statType)
        {
            return statType switch
            {
                StatType.MaxSpeed => SPEED_PER_LEVEL,
                StatType.Acceleration => ACCEL_PER_LEVEL,
                StatType.Handling => HANDLING_PER_LEVEL,
                StatType.Traction => TRACTION_PER_LEVEL,
                _ => 0f
            };
        }

    }
}