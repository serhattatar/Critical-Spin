using CriticalSpin.Core;
using CriticalSpin.Model;
using System.Collections.Generic;
using UnityEngine;

namespace CriticalSpin.Presenter
{
    /// <summary>
    /// I use this manager to hold the rewards that the player collects during the current run.
    /// It keeps them in a temporary list until the player decides to cash out.
    /// </summary>
    public class RewardManager : IPresenter, IRewardDataExporter
    {
        private readonly List<RewardData> _collectedRewards = new List<RewardData>();
        private readonly CurrencyManager  _currencyManager;
        private int _totalValue;

        public RewardManager(CurrencyManager currencyManager)
        {
            _currencyManager = currencyManager;
        }

        public void Initialize()
        {
            _collectedRewards.Clear();
            _totalValue = 0;
        }

        public void Dispose() => _collectedRewards.Clear();

        public void AddReward(RewardData reward)
        {
            if (reward == null) return;
            _collectedRewards.Add(reward);
            _totalValue += reward.amount;
            GameEvents.OnRewardCollected?.Invoke(reward);
        }

        public void ClearAllRewards()
        {
            _collectedRewards.Clear();
            _totalValue = 0;
            GameEvents.OnAllRewardsLost?.Invoke();
        }

        /// <summary>
        /// This is called when the player chooses to leave the game. 
        /// I export all temporary session rewards to the actual inventory and save them.
        /// </summary>
        public void ExportRewards()
        {
            foreach (var reward in _collectedRewards)
                ApplyReward(reward);

            _collectedRewards.Clear();
            _totalValue = 0;
        }

        private void ApplyReward(RewardData reward)
        {
            if (reward == null) return;

            switch (reward.rewardType)
            {
                case RewardType.Gold:
                    _currencyManager?.AddCurrency(reward.amount);
                    break;

                default:
                    // TODO: I will add full inventory support here later when I have 3D items or weapons.
                    Debug.Log($"[RewardManager] Inventory reward added: {reward.rewardName} x{reward.amount} ({reward.rewardType})");
                    break;
            }
        }

        // IRewardDataExporter Implementation for UI mapping
        public IReadOnlyList<RewardData> GetSessionRewards() => _collectedRewards;
        public int GetSessionTotalValue() => _totalValue;

        public List<RewardData> GetCollectedRewards() => new List<RewardData>(_collectedRewards);
        public int  GetTotalValue()  => _totalValue;
        public bool HasRewards()     => _collectedRewards.Count > 0;
    }
}
