using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using CriticalSpin.Core;
using CriticalSpin.Model;
using Sirenix.OdinInspector;

namespace CriticalSpin.View
{
    /// <summary>
    /// This is the final screen where the player claims their rewards.
    /// I added DOTween to dynamically spawn all the collected items one by one.
    /// </summary>
    public class CashoutView : MonoBehaviour, IView
    {
        [Title("References")]
        [SerializeField] private Image img_cashout_bg_overlay;
        [SerializeField] private Transform tf_cashout_content_animator;
        [SerializeField] private TextMeshProUGUI txt_cashout_total_value;
        [SerializeField] private Transform tf_cashout_reward_list;
        [SerializeField] private RewardItemView pf_reward_item;  // same prefab as the reward panel
        [SerializeField] private Button btn_cashout_confirm;

        [Title("Overlay Settings")]
        [Tooltip("The dark background color that appears behind the popup.")]
        [SerializeField] private Color _overlayTargetColor = new Color(0f, 0f, 0f, 0.85f);
        [SerializeField] private float _overlayFadeDuration = 0.3f;

        [Title("Panel Animation")]
        [SerializeField] private float _panelScaleDuration = 0.5f;
        [SerializeField] private float _panelScaleDelay    = 0.2f;

        [Title("Row Animation")]
        [Tooltip("How fast each individual reward item scales up when appearing.")]
        [SerializeField] private float _rowScaleDuration = 0.25f;
        
        [Tooltip("The time gap between each item spawning, creating a nice domino effect.")]
        [SerializeField] private float _rowStaggerDelay  = 0.1f;
        
        [Tooltip("How long to wait before the first item starts spawning.")]
        [SerializeField] private float _firstRowDelay    = 0.4f;

        private readonly List<RewardItemView> _rows = new List<RewardItemView>();

        private void OnValidate()
        {
            img_cashout_bg_overlay      ??= transform.Find("ui_image_cashout_overlay")?.GetComponent<Image>();
            tf_cashout_content_animator ??= transform.Find("ui_animator_cashout_content");
            txt_cashout_total_value     ??= transform.Find("ui_animator_cashout_content/txt_cashout_total_value")?.GetComponent<TextMeshProUGUI>();
            tf_cashout_reward_list      ??= transform.Find("ui_animator_cashout_content/ui_scrollrect_cashout/Viewport/Content");
            btn_cashout_confirm         ??= transform.Find("ui_animator_cashout_content/btn_cashout_confirm")?.GetComponent<Button>();
        }

        public void Initialize() => gameObject.SetActive(false);

        public void Show()
        {
            gameObject.SetActive(true);

            if (img_cashout_bg_overlay != null)
            {
                img_cashout_bg_overlay.color = Color.clear;
                DOTween.To(() => img_cashout_bg_overlay.color, x => img_cashout_bg_overlay.color = x, _overlayTargetColor, _overlayFadeDuration)
                    .SetLink(gameObject);
            }

            if (tf_cashout_content_animator != null)
            {
                tf_cashout_content_animator.localScale = Vector3.zero;
                tf_cashout_content_animator.DOScale(Vector3.one, _panelScaleDuration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(_panelScaleDelay)
                    .SetLink(gameObject);
            }
        }

        public void Hide()
        {
            DOTween.Kill(tf_cashout_content_animator);

            if (img_cashout_bg_overlay != null)
            {
                Color hidden = _overlayTargetColor;
                hidden.a = 0f;
                DOTween.To(() => img_cashout_bg_overlay.color, x => img_cashout_bg_overlay.color = x, hidden, _overlayFadeDuration)
                    .SetLink(gameObject)
                    .OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                        img_cashout_bg_overlay.color = _overlayTargetColor;
                        ClearRows();
                    });
            }
            else
            {
                gameObject.SetActive(false);
                ClearRows();
            }
        }

        public void PopulateRewards(List<RewardData> rewards, int totalValue)
        {
            ClearRows();
            if (txt_cashout_total_value != null) txt_cashout_total_value.text = totalValue.ToString();

            float delay = _firstRowDelay;
            foreach (var reward in rewards)
            {
                SpawnRow(reward, delay);
                delay += _rowStaggerDelay;
            }
        }

        private void SpawnRow(RewardData reward, float delay)
        {
            if (pf_reward_item == null || tf_cashout_reward_list == null) return;

            RewardItemView row = UIObjectPool.Spawn(pf_reward_item, tf_cashout_reward_list);
            row.name = $"ui_cashout_row_{reward.rewardName}";
            _rows.Add(row);

            row.Setup(reward);

            row.transform.localScale = Vector3.zero;
            row.transform.DOScale(Vector3.one, _rowScaleDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(delay)
                .SetLink(row.gameObject);
        }

        private void ClearRows()
        {
            foreach (var r in _rows) if (r != null) UIObjectPool.Despawn(r, pf_reward_item);
            _rows.Clear();
        }

        public Button GetConfirmButton() => btn_cashout_confirm;
    }
}
