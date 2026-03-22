using System;
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
    /// This is my full-screen reward collection popup.
    /// It opens from the center with a bounce, shows the reward with a cool spinning background,
    /// and closes when the player taps Collect (or anywhere else).
    /// </summary>
    public class RewardCollectView : MonoBehaviour, IView
    {
        [Title("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image img_reward_collect_icon;
        [SerializeField] private TextMeshProUGUI txt_reward_collect_name;
        [SerializeField] private TextMeshProUGUI txt_reward_collect_amount;
        [SerializeField] private Button btn_collect;
        [SerializeField] private Button btn_collect_bg;
        [SerializeField] private RectTransform rt_icon_container;

        [Tooltip("Optional rotating shine / rays image placed behind the reward icon.")]
        [SerializeField] private Transform tf_shine;

        [Title("Animation Config")]
        [Tooltip("How fast the popup opens up.")]
        [SerializeField] private float _openDuration  = 0.45f;
        [Tooltip("How fast the popup closes.")]
        [SerializeField] private float _closeDuration = 0.3f;
        [SerializeField] private float _fadeDuration  = 0.2f;
        [SerializeField] private Ease  _openEase      = Ease.OutBack;
        [SerializeField] private Ease  _closeEase     = Ease.InBack;

        [Title("Shine Settings")]
        [Tooltip("How fast the cool radial rays spin in the background.")]
        [SerializeField] private float _shineRotationSpeed = 30f;

        private Action _onCollectConfirmed;

        private void OnValidate()
        {
            canvasGroup               ??= GetComponent<CanvasGroup>();
            img_reward_collect_icon   ??= transform.Find("rt_icon_container/img_reward_collect_icon")?.GetComponent<Image>();
            txt_reward_collect_name   ??= transform.Find("rt_icon_container/txt_reward_collect_name")?.GetComponent<TextMeshProUGUI>();
            txt_reward_collect_amount ??= transform.Find("rt_icon_container/txt_reward_collect_amount")?.GetComponent<TextMeshProUGUI>();
            btn_collect               ??= transform.Find("btn_reward_collect")?.GetComponent<Button>();
            btn_collect_bg            ??= transform.Find("btn_reward_collect_bg")?.GetComponent<Button>();
            rt_icon_container         ??= transform.Find("rt_icon_container")?.GetComponent<RectTransform>();
            tf_shine                  ??= transform.Find("rt_icon_container/tf_shine");
        }

        public void Initialize()
        {
            gameObject.SetActive(false);
            btn_collect?.onClick.AddListener(OnCollectClicked);
            btn_collect_bg?.onClick.AddListener(OnCollectClicked);
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        /// <summary>
        /// Opens the popup from screen center with a scale-in animation.
        /// onCollectConfirmed is invoked after the close animation finishes.
        /// </summary>
        public void ShowReward(RewardData reward, Action onCollectConfirmed)
        {
            if (reward == null) { onCollectConfirmed?.Invoke(); return; }

            _onCollectConfirmed = onCollectConfirmed;

            gameObject.SetActive(true);

            // Populate content.
            if (img_reward_collect_icon != null)
            {
                img_reward_collect_icon.sprite  = reward.icon;
                img_reward_collect_icon.enabled = reward.icon != null;
            }

            if (txt_reward_collect_name   != null) txt_reward_collect_name.text   = reward.rewardName;
            if (txt_reward_collect_amount != null)
            {
                txt_reward_collect_amount.text = reward.rewardType == RewardType.Bomb
                    ? string.Empty
                    : reward.amount > 0 ? $"x{reward.amount}" : string.Empty;
            }

            // Reset to initial state.
            if (rt_icon_container != null) rt_icon_container.localScale = Vector3.zero;

            if (canvasGroup != null)
            {
                canvasGroup.alpha          = 0f;
                canvasGroup.interactable   = false;
                canvasGroup.blocksRaycasts = true;
            }

            // Fade in overlay.
            if (canvasGroup != null)
            {
                DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, _fadeDuration)
                    .SetLink(gameObject);
            }

            // Start rotating immediately so it's already spinning while scaling up!
            StartShineRotation();

            // Scale icon container from zero to full at screen center.
            if (rt_icon_container != null)
            {
                rt_icon_container
                    .DOScale(Vector3.one, _openDuration)
                    .SetEase(_openEase)
                    .SetLink(gameObject)
                    .OnComplete(() =>
                    {
                        if (canvasGroup != null) canvasGroup.interactable = true;
                    });
            }
        }

        private void OnCollectClicked() => AnimateClose();

        /// <summary>
        /// Starts an infinite clockwise DOTween rotation on the shine object.
        /// </summary>
        private void StartShineRotation()
        {
            if (tf_shine == null) return;

            DOTween.Kill(tf_shine);
            tf_shine.localRotation = Quaternion.identity;

            float duration = 360f / Mathf.Max(1f, _shineRotationSpeed);

            // Using DOLocalRotate properly links tf_shine as the target for DOTween.Kill()
            tf_shine.DOLocalRotate(new Vector3(0f, 0f, -360f), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(tf_shine.gameObject);
        }

        private void AnimateClose()
        {
            if (canvasGroup != null) canvasGroup.interactable = false;

            Sequence seq = DOTween.Sequence().SetLink(gameObject);

            // Shrink back to zero at screen center.
            if (rt_icon_container != null)
            {
                seq.Join(rt_icon_container
                    .DOScale(Vector3.zero, _closeDuration)
                    .SetEase(_closeEase));
            }

            // Fade out overlay.
            if (canvasGroup != null)
            {
                seq.Join(DOTween.To(
                    () => canvasGroup.alpha,
                    x  => canvasGroup.alpha = x,
                    0f,
                    _closeDuration
                ));
            }

            seq.OnComplete(() =>
            {
                if (tf_shine != null) DOTween.Kill(tf_shine); // Kill the spin ONLY when fully closed
                gameObject.SetActive(false);
                _onCollectConfirmed?.Invoke();
                _onCollectConfirmed = null;
            });
        }
    }
}
