using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using CriticalSpin.Core;
using Sirenix.OdinInspector;

namespace CriticalSpin.View
{
    /// <summary>
    /// This screen appears if the player clicks 'Revive' on the Bomb panel.
    /// It shows their current balance and how much the revive costs.
    /// </summary>
    public class ReviveView : MonoBehaviour, IView
    {
        [Title("References")]
        [SerializeField] private Image img_revive_bg_overlay;
        [SerializeField] private Transform tf_revive_content_animator;
        [SerializeField] private TextMeshProUGUI txt_revive_cost_value;
        [SerializeField] private TextMeshProUGUI txt_player_currency_value;
        [SerializeField] private TextMeshProUGUI txt_revive_count_value;
        [SerializeField] private Button btn_accept_revive;
        [SerializeField] private Button btn_decline_revive;

        [Title("Overlay Settings")]
        [SerializeField] private Color _overlayTargetColor = new Color(0f, 0f, 0f, 0.85f);
        [SerializeField] private float _overlayFadeDuration = 0.3f;

        [Title("Panel Animation")]
        [SerializeField] private float _panelScaleDuration = 0.45f;
        [SerializeField] private float _panelScaleDelay    = 0.15f;

        [Title("Format Settings")]
        [Tooltip("Format for the Revive Cost text. Use {0} for the cost amount.")]
        [SerializeField] private string _reviveCostFormat  = "Revive Cost = <sprite=0> {0}";
        
        [Tooltip("Format for the player's current balance text.")]
        [SerializeField] private string _currencyFormat    = "Balance = <sprite=0> {0}";
        
        [Tooltip("Format for the remaining revive count text.")]
        [SerializeField] private string _reviveCountFormat = "{0} Left";

        private void OnValidate()
        {
            img_revive_bg_overlay       ??= transform.Find("ui_image_revive_overlay")?.GetComponent<Image>();
            tf_revive_content_animator  ??= transform.Find("ui_animator_revive_content");
            txt_revive_cost_value       ??= transform.Find("ui_animator_revive_content/txt_revive_cost_value")?.GetComponent<TextMeshProUGUI>();
            txt_player_currency_value   ??= transform.Find("ui_animator_revive_content/txt_player_currency_value")?.GetComponent<TextMeshProUGUI>();
            txt_revive_count_value      ??= transform.Find("ui_animator_revive_content/txt_revive_count_value")?.GetComponent<TextMeshProUGUI>();
            btn_accept_revive           ??= transform.Find("ui_animator_revive_content/btn_accept_revive")?.GetComponent<Button>();
            btn_decline_revive          ??= transform.Find("ui_animator_revive_content/btn_decline_revive")?.GetComponent<Button>();
        }

        public void Initialize() => gameObject.SetActive(false);

        public void Show()
        {
            gameObject.SetActive(true);

            if (img_revive_bg_overlay != null)
            {
                img_revive_bg_overlay.color = Color.clear;
                DOTween.To(() => img_revive_bg_overlay.color, x => img_revive_bg_overlay.color = x, _overlayTargetColor, _overlayFadeDuration).SetLink(gameObject);
            }

            if (tf_revive_content_animator != null)
            {
                tf_revive_content_animator.localScale = Vector3.zero;
                tf_revive_content_animator.DOScale(Vector3.one, _panelScaleDuration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(_panelScaleDelay)
                    .SetLink(gameObject);
            }
        }

        public void Hide()
        {
            DOTween.Kill(tf_revive_content_animator);

            if (img_revive_bg_overlay != null)
            {
                Color hidden = _overlayTargetColor;
                hidden.a = 0f;
                DOTween.To(() => img_revive_bg_overlay.color, x => img_revive_bg_overlay.color = x, hidden, _overlayFadeDuration)
                    .SetLink(gameObject)
                    .OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                        img_revive_bg_overlay.color = _overlayTargetColor;
                    });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void UpdateInfo(int reviveCost, int currency, int used, int max)
        {
            if (txt_revive_cost_value != null)     txt_revive_cost_value.text = string.Format(_reviveCostFormat, reviveCost);
            if (txt_player_currency_value != null) txt_player_currency_value.text = string.Format(_currencyFormat, currency);
            if (txt_revive_count_value != null)    txt_revive_count_value.text = string.Format(_reviveCountFormat, max - used);
            if (btn_accept_revive != null)         btn_accept_revive.interactable = currency >= reviveCost;
        }

        public Button GetAcceptButton()  => btn_accept_revive;
        public Button GetDeclineButton() => btn_decline_revive;
    }
}
