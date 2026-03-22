using CriticalSpin.Core;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CriticalSpin.View
{
    /// <summary>
    /// This is the popup that appears when the player hits a Bomb.
    /// It gives them the bad news and offers a chance to Revive or Restart.
    /// </summary>
    public class BombView : MonoBehaviour, IView
    {
        [Title("References")]
        [SerializeField] private Image img_bomb_icon;
        [SerializeField] private Image img_bomb_bg_overlay;
        [SerializeField] private TextMeshProUGUI txt_bomb_description_value;
        [SerializeField] private Button btn_restart;
        [SerializeField] private Button btn_revive;
        [SerializeField] private TextMeshProUGUI txt_btn_revive_cost;
        [SerializeField] private Transform tf_bomb_content_animator;

        [Title("Overlay Settings")]
        [Tooltip("The dark background color that appears behind the popup.")]
        [SerializeField] private Color _overlayTargetColor = new Color(0f, 0f, 0f, 0.85f);
        
        [Tooltip("How fast the dark background fades in.")]
        [SerializeField] private float _overlayFadeDuration = 0.3f;

        [Title("Panel Animation")]
        [SerializeField] private float _panelScaleDuration = 0.5f;
        [SerializeField] private float _panelScaleDelay = 0.2f;
        [SerializeField] private float _iconShakeDuration = 0.5f;
        [SerializeField] private float _iconShakeDelay = 0.7f;

        [Title("Revive Text")]
        [Tooltip("Text shown if the player has enough gold to revive.")]
        [SerializeField] private string _reviveAvailableText = "All rewards lost! Continue?";
        
        [Tooltip("Text shown if the player is too broke to revive.")]
        [SerializeField] private string _reviveUnavailableText = "All rewards lost!";

        private void OnValidate()
        {
            img_bomb_bg_overlay ??= transform.Find("ui_image_bomb_overlay")?.GetComponent<Image>();
            img_bomb_icon ??= transform.Find("ui_animator_bomb_content/ui_image_bomb_icon")?.GetComponent<Image>();
            txt_bomb_description_value ??= transform.Find("ui_animator_bomb_content/txt_bomb_description_value")?.GetComponent<TextMeshProUGUI>();
            btn_restart ??= transform.Find("ui_animator_bomb_content/btn_restart")?.GetComponent<Button>();
            btn_revive ??= transform.Find("ui_animator_bomb_content/btn_revive")?.GetComponent<Button>();
            txt_btn_revive_cost ??= btn_revive?.GetComponentInChildren<TextMeshProUGUI>();
            tf_bomb_content_animator ??= transform.Find("ui_animator_bomb_content");
        }

        public void Initialize() => gameObject.SetActive(false);

        public void Show()
        {
            gameObject.SetActive(true);

            if (img_bomb_bg_overlay != null)
            {
                img_bomb_bg_overlay.color = Color.clear;
                DOTween.To(() => img_bomb_bg_overlay.color, x => img_bomb_bg_overlay.color = x, _overlayTargetColor, _overlayFadeDuration).SetLink(gameObject);
            }

            if (tf_bomb_content_animator != null)
            {
                tf_bomb_content_animator.localScale = Vector3.zero;
                tf_bomb_content_animator.DOScale(Vector3.one, _panelScaleDuration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(_panelScaleDelay)
                    .SetLink(gameObject);
            }

            if (img_bomb_icon != null)
                img_bomb_icon.transform.DOShakeScale(_iconShakeDuration, 0.3f, 10, 90f).SetDelay(_iconShakeDelay).SetLink(gameObject);
        }

        public void Hide()
        {
            DOTween.Kill(tf_bomb_content_animator);

            if (img_bomb_bg_overlay != null)
            {
                Color hidden = _overlayTargetColor;
                hidden.a = 0f;
                DOTween.To(() => img_bomb_bg_overlay.color, x => img_bomb_bg_overlay.color = x, hidden, _overlayFadeDuration)
                    .SetLink(gameObject)
                    .OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                        img_bomb_bg_overlay.color = _overlayTargetColor;
                    });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void Configure(bool showReviveButton, int reviveCost, int currency)
        {
            bool canRevive = showReviveButton && currency >= reviveCost;

            if (btn_revive != null)
            {
                btn_revive.gameObject.SetActive(canRevive);
                if (txt_btn_revive_cost != null)
                    txt_btn_revive_cost.text = $"<sprite=0>{reviveCost}\nREVIVE";
            }

            if (txt_bomb_description_value != null)
                txt_bomb_description_value.text = canRevive ? _reviveAvailableText : _reviveUnavailableText;
        }

        public Button GetRestartButton() => btn_restart;
        public Button GetReviveButton() => btn_revive;
    }
}
