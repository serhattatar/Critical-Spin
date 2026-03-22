using UnityEngine;
using Sirenix.OdinInspector;

namespace CriticalSpin.Model
{
    /// <summary>
    /// I use this ScriptableObject to create individual rewards for the wheel.
    /// It makes it super easy to add new guns, chests, or currency types from the Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "RewardData_New", menuName = "CriticalSpin/Data/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [Title("Reward Info")]
        [LabelText("Reward Type")]
        public RewardType rewardType;

        [LabelText("Display Name")]
        public string rewardName;

        [LabelText("Amount / Value")]
        [Tooltip("The actual value given to the player (e.g. 500 Gold or 1x Rifle).")]
        [MinValue(0)]
        public int amount;

        [Title("Visuals")]
        [PreviewField(55, ObjectFieldAlignment.Left)]
        public Sprite icon;

        [LabelText("Tint Color")]
        public Color displayColor = Color.white;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(rewardName) && rewardType != RewardType.Bomb)
                rewardName = rewardType.ToString();
        }
    }
}
