using System.Collections.Generic;
using CriticalSpin.Model;

namespace CriticalSpin.Core
{
    /// <summary>
    /// I use this interface to connect my Wheel mini-game to the main shooter game.
    /// It lets the main game easily pull all the collected rewards after a successful cashout.
    /// </summary>
    public interface IRewardDataExporter
    {
        /// <summary>
        /// Returns a snapshot of all rewards collected during the current session.
        /// The list contains the original ScriptableObject references — do NOT mutate them.
        /// </summary>
        IReadOnlyList<RewardData> GetSessionRewards();

        /// <summary>
        /// Returns the cumulative numeric value of all collected rewards this session.
        /// </summary>
        int GetSessionTotalValue();

        /// <summary>
        /// This pushes the rewards to the player's permanent inventory.
        /// Right now it just logs to console, but I'll connect it to the main game database later.
        /// </summary>
        void ExportRewards();
    }
}
