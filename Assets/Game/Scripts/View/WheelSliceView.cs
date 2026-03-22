using CriticalSpin.Model;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CriticalSpin.View
{
    /// <summary>
    /// I use this to fill out the UI graphics for a single slice on the spinning wheel.
    /// </summary>
    public class WheelSliceView : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private Image img_slice_icon;
        [SerializeField] private TextMeshProUGUI txt_slice_amount_value;

        private void OnValidate()
        {
            img_slice_icon         ??= transform.Find("ui_image_slice_icon")?.GetComponent<Image>();
            txt_slice_amount_value ??= transform.Find("txt_slice_amount_value")?.GetComponent<TextMeshProUGUI>();
        }

        public void Setup(WheelSliceData data)
        {
            if (data == null) return;


            if (data.reward == null) return;

            if (img_slice_icon != null)
            {
                img_slice_icon.enabled = data.reward.icon != null;
                if (data.reward.icon != null)
                {
                    img_slice_icon.sprite = data.reward.icon;
                    img_slice_icon.preserveAspect = true;
                }
            }

            if (txt_slice_amount_value != null)
            {
                txt_slice_amount_value.text = data.reward.rewardType == RewardType.Bomb
                    ? "BOMB"
                    : data.reward.amount > 0 ? $"x{data.reward.amount}" : data.reward.rewardName;
            }
        }
    }
}
