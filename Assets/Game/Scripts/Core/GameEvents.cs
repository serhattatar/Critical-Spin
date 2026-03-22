using System;
using CriticalSpin.Model;

namespace CriticalSpin.Core
{
    /// <summary>
    /// This is my global event bus. 
    /// I use it so my View and Presenter layers don't have to know about each other securely.
    /// Whenever something big happens, I just broadcast it here.
    /// </summary>
    public static class GameEvents
    {
        // Spin lifecycle
        public static Action OnSpinStarted;
        public static Action<WheelSliceData> OnSpinCompleted;

        // Reward lifecycle
        public static Action<RewardData> OnRewardCollected;
        public static Action OnAllRewardsLost;

        // Bomb
        public static Action OnBombHit;

        // Zone
        public static Action<int, SpinType> OnZoneChanged;

        // Game flow
        public static Action OnReviveRequested;
        public static Action<bool> OnReviveResult;
        public static Action OnCashoutRequested;
        public static Action OnGameRestarted;
        public static Action OnGameOver;

        // Currency
        public static Action<int> OnCurrencyChanged;

        /// <summary>
        /// I call this when the scene unloads so old events don't get stuck in memory.
        /// </summary>
        public static void Reset()
        {
            OnSpinStarted      = null;
            OnSpinCompleted    = null;
            OnRewardCollected  = null;
            OnAllRewardsLost   = null;
            OnBombHit          = null;
            OnZoneChanged      = null;
            OnReviveRequested  = null;
            OnReviveResult     = null;
            OnCashoutRequested = null;
            OnGameRestarted    = null;
            OnGameOver         = null;
            OnCurrencyChanged  = null;
        }
    }
}
