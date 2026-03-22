using UnityEngine;
using Sirenix.OdinInspector;

namespace CriticalSpin.Model
{
    /// <summary>
    /// This is my main configuration file for the game rules. 
    /// I keep all the important global settings here like zone intervals, 
    /// wheel timings, and revive costs so I can easily balance them from the Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "CriticalSpin/Data/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Title("Zone Settings")]
        [LabelText("Safe Zone Interval")]
        [MinValue(1)]
        [Tooltip("How often we get a safe zone. I usually set this to every 5 zones.")]
        public int safeZoneInterval = 5;

        [LabelText("Super Zone Interval")]
        [MinValue(1)]
        [Tooltip("How often a super golden zone appears. I usually set this to every 30 zones.")]
        public int superZoneInterval = 30;

        [Title("Revive Settings")]
        [LabelText("Enable Revive")]
        [Tooltip("Check this if we want to let the player revive after hitting a bomb.")]
        public bool reviveEnabled = true;

        [LabelText("Revive Cost")]
        [ShowIf("reviveEnabled")]
        [MinValue(0)]
        public int reviveCost = 100;

        [LabelText("Max Revive Count")]
        [ShowIf("reviveEnabled")]
        [MinValue(1)]
        [Tooltip("Maximum number of times a player can revive in a single run.")]
        public int maxReviveCount = 3;

        [Title("Currency")]
        [LabelText("Starting Currency")]
        [MinValue(0)]
        [Tooltip("The amount of gold the player starts with when they first play the game.")]
        public int startingCurrency = 500;

        [Title("Wheel Animation")]
        [LabelText("Min Spin Duration (s)")]
        [MinValue(1f)]
        public float minSpinDuration = 3f;

        [LabelText("Max Spin Duration (s)")]
        [MinValue(1f)]
        public float maxSpinDuration = 5f;

        [LabelText("Extra Full Rotations")]
        [MinValue(3)]
        [Tooltip("I add this many full spins before the wheel starts slowing down to build excitement.")]
        public int extraFullRotations = 5;

        [LabelText("Overshoot Degrees")]
        [Range(5f, 45f)]
        [Tooltip("How many degrees the wheel goes past the target slice before bouncing back. Adds a nice juice effect!")]
        public float overshootDegrees = 15f;

        [LabelText("Total Zone Count")]
        [MinValue(30)]
        [Tooltip("The final zone of the game. After this, I force the player to cashout.")]
        public int totalZoneCount = 50;

        [LabelText("Post Spin Delay (s)")]
        [MinValue(0f)]
        [Tooltip("How long to wait after the wheel completely stops before showing the reward popup.")]
        public float postSpinDelay = 1.5f;

        /// <summary>I use this to check if the current zone is a Safe Zone (Silver).</summary>
        public bool IsSafeZone(int zoneNumber)
        {
            return zoneNumber % safeZoneInterval == 0 && !IsSuperZone(zoneNumber);
        }

        /// <summary>I use this to check if the current zone is a Super Zone (Golden).</summary>
        public bool IsSuperZone(int zoneNumber)
        {
            return zoneNumber % superZoneInterval == 0;
        }

        /// <summary>Finds out which spin wheel type (Bronze, Silver, Golden) we should show.</summary>
        public SpinType GetSpinType(int zoneNumber)
        {
            if (IsSuperZone(zoneNumber)) return SpinType.Golden;
            if (IsSafeZone(zoneNumber))  return SpinType.Silver;
            return SpinType.Bronze;
        }

        /// <summary>Helper method to check if the zone contains a bomb. Only bronze zones have them.</summary>
        public bool ZoneHasBomb(int zoneNumber)
        {
            return GetSpinType(zoneNumber) == SpinType.Bronze;
        }
    }
}
