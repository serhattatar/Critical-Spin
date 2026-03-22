using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using CriticalSpin.Model;

namespace CriticalSpin.View
{
    /// <summary>
    /// This represents a single collected reward slot on the right panel or cashout screen.
    /// It automatically stacks amounts and does a punch animation when receiving duplicates.
    /// </summary>
    public class RewardItemView : MonoBehaviour
    {
        [SerializeField] private Image img_icon;
        [SerializeField] private TextMeshProUGUI txt_amount;
        [SerializeField] private TextMeshProUGUI txt_name; // optional — shown in cashout list

        [SerializeField] private float _punchScale    = 0.3f;
        [SerializeField] private float _punchDuration = 0.35f;

        private int _count;
        private RewardData _data;

        public RewardType RewardType => _data != null ? _data.rewardType : default;

        private void OnValidate()
        {
            img_icon   ??= transform.Find("ui_image_reward_icon")?.GetComponent<Image>();
            txt_amount ??= transform.Find("txt_reward_amount_value")?.GetComponent<TextMeshProUGUI>();
            txt_name   ??= transform.Find("txt_reward_name_value")?.GetComponent<TextMeshProUGUI>();
            if (img_icon != null) img_icon.preserveAspect = true;
        }

        // Called when a new slot is first created.
        public void Setup(RewardData reward)
        {
            _data  = reward;
            _count = reward.amount > 0 ? reward.amount : 1;

            if (img_icon != null && reward.icon != null)
            {
                img_icon.sprite = reward.icon;
                img_icon.preserveAspect = true;
            }

            if (txt_name   != null) txt_name.text  = reward.rewardName;
            if (txt_amount != null) txt_amount.text = $"x{_count}";
        }

        // Called when another reward of the same type arrives. Stacks the count.
        public void AddAmount(int amount)
        {
            _count += amount > 0 ? amount : 1;
            if (txt_amount != null) txt_amount.text = $"x{_count}";
            PunchAnimate();
        }

        private void PunchAnimate()
        {
            DOTween.Kill(transform);
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, 5, 0.5f)
                     .SetLink(gameObject);
        }
    }
}
