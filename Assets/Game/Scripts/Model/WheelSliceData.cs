using UnityEngine;
using Sirenix.OdinInspector;

namespace CriticalSpin.Model
{
    /// <summary>
    /// This object defines a single slice on my wheel. 
    /// I can drag and drop a RewardData into it and adjust its drop chance weight.
    /// </summary>
    [CreateAssetMenu(fileName = "WheelSlice_New", menuName = "CriticalSpin/Data/Wheel Slice Data")]
    public class WheelSliceData : ScriptableObject
    {
        [Title("Reward Settings")]
        [LabelText("Reward Data (Include Bomb Here)")]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        public RewardData reward;

        [Title("Weight")]
        [LabelText("Spin Weight")]
        [MinValue(0.1f)]
        [Tooltip("Higher weight means this slice is more likely to be selected by my WeightedRandom algorithm.")]
        public float weight = 1f;

        /// <summary>Helper method to quickly check if this slice is the physical Bomb.</summary>
        public bool IsBomb => reward != null && reward.rewardType == RewardType.Bomb;

        /// <summary>Returns the display name for this slice (used for debugging and editor tooling).</summary>
        public string GetDisplayName()
        {
            if (IsBomb) return "BOMB";
            return reward != null ? reward.rewardName : "Empty";
        }
    }
}
