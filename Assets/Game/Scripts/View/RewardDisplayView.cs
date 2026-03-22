using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CriticalSpin.Core;
using CriticalSpin.Model;
using Sirenix.OdinInspector;

namespace CriticalSpin.View
{
    // Shows all collected rewards in a single container.
    // Same reward type stacks onto the existing slot instead of adding a new one.
    public class RewardDisplayView : MonoBehaviour, IView
    {
        [Title("References")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform tf_reward_item_container;
        [SerializeField] private RewardItemView pf_reward_item;

        [Title("Spawn Animation")]
        [SerializeField] private float _popDuration = 0.3f;

        private readonly List<RewardItemView> _items = new List<RewardItemView>();

        private void OnValidate()
        {
            _scrollRect ??= transform.Find("ui_scrollrect_rewards")?.GetComponent<ScrollRect>();
            tf_reward_item_container ??= transform.Find("ui_scrollrect_rewards/Viewport/Content");
        }

        public void Initialize() => ResetDisplay();
        public void Show()       => gameObject.SetActive(true);
        public void Hide()       => gameObject.SetActive(false);

        public void AddReward(RewardData reward)
        {
            if (reward == null) return;

            // Stack onto an existing slot if the same reward type is already shown.
            foreach (var item in _items)
            {
                if (item != null && item.RewardType == reward.rewardType)
                {
                    item.AddAmount(reward.amount);
                    FocusOnItem((RectTransform)item.transform);
                    return;
                }
            }

            // No existing slot — create a new one.
            SpawnItem(reward);
        }

        public void ResetDisplay()
        {
            foreach (var item in _items)
                if (item != null) UIObjectPool.Despawn(item, pf_reward_item);
            _items.Clear();
        }

        private void SpawnItem(RewardData reward)
        {
            if (pf_reward_item == null || tf_reward_item_container == null) return;

            RewardItemView item = UIObjectPool.Spawn(pf_reward_item, tf_reward_item_container);
            item.name = $"ui_reward_{reward.rewardType}";
            _items.Add(item);

            item.Setup(reward);

            item.transform.localScale = Vector3.zero;
            item.transform.DOScale(Vector3.one, _popDuration)
                .SetEase(Ease.OutBack)
                .SetLink(item.gameObject);

            FocusOnItem((RectTransform)item.transform);
        }

        private void FocusOnItem(RectTransform targetItem)
        {
            if (_scrollRect == null || _scrollRect.content == null) return;
            if (!gameObject.activeInHierarchy) return;

            // Wait 1 frame to let layout group sort the new item
            DOVirtual.DelayedCall(0.05f, () =>
            {
                if (_scrollRect == null || !gameObject.activeInHierarchy) return;

                // Unity's built-in ScrollRect uses 0 to 1 scaling (1 is Top, 0 is Bottom).
                // It automatically clamps and handles bounds!
                float contentHeight = _scrollRect.content.rect.height;
                if (contentHeight <= 0) return;

                float targetNormY = 1f - (Mathf.Abs(targetItem.anchoredPosition.y) / contentHeight);

                DOTween.Kill(_scrollRect);
                DOTween.To(() => _scrollRect.verticalNormalizedPosition, x => _scrollRect.verticalNormalizedPosition = x, targetNormY, 0.4f)
                    .SetEase(Ease.OutCubic).SetTarget(_scrollRect).SetLink(gameObject);

            }).SetLink(gameObject);
        }
    }
}
