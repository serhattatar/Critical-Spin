using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CriticalSpin.View
{
    /// <summary>
    /// I created this struct to easily design the colors and sprites for 
    /// every possible zone state directly from the Unity Inspector!
    /// </summary>
    [Serializable]
    public class ZoneAppearanceConfig
    {
        public Sprite bgSprite;
        public Color  bgColor   = Color.white;
        public Color  textColor = Color.white;

        public void ApplyBgTo(Image img)
        {
            if (img == null) return;
            if (bgSprite != null) img.sprite = bgSprite;
            img.color = bgColor;
        }
    }

    /// <summary>
    /// This is a single square slot in the zone bar strip at the top of the game.
    /// </summary>
    public class ZoneItemView : MonoBehaviour
    {
        [SerializeField] private Image           img_zone_item_bg;
        [SerializeField] private TextMeshProUGUI txt_zone_number_value;

        [Header("Normal Zone")]
        [SerializeField] private ZoneAppearanceConfig _normalPassed   = new ZoneAppearanceConfig { bgColor = new Color(0.35f, 0.35f, 0.35f), textColor = new Color(0.5f, 0.5f, 0.5f) };
        [SerializeField] private ZoneAppearanceConfig _normalCurrent  = new ZoneAppearanceConfig { bgColor = Color.white, textColor = Color.black };
        [SerializeField] private ZoneAppearanceConfig _normalUpcoming = new ZoneAppearanceConfig { bgColor = new Color(0.82f, 0.82f, 0.82f), textColor = Color.black };

        [Header("Safe Zone")]
        [SerializeField] private ZoneAppearanceConfig _safePassed   = new ZoneAppearanceConfig { bgColor = new Color(0.15f, 0.45f, 0.15f), textColor = new Color(0.5f, 0.5f, 0.5f) };
        [SerializeField] private ZoneAppearanceConfig _safeCurrent  = new ZoneAppearanceConfig { bgColor = new Color(0.35f, 1.0f, 0.45f), textColor = Color.black };
        [SerializeField] private ZoneAppearanceConfig _safeUpcoming = new ZoneAppearanceConfig { bgColor = new Color(0.25f, 0.85f, 0.35f), textColor = Color.black };

        [Header("Super Zone")]
        [SerializeField] private ZoneAppearanceConfig _superPassed   = new ZoneAppearanceConfig { bgColor = new Color(0.5f, 0.4f, 0.05f), textColor = new Color(0.5f, 0.5f, 0.5f) };
        [SerializeField] private ZoneAppearanceConfig _superCurrent  = new ZoneAppearanceConfig { bgColor = new Color(1.0f, 0.9f, 0.3f), textColor = Color.black };
        [SerializeField] private ZoneAppearanceConfig _superUpcoming = new ZoneAppearanceConfig { bgColor = new Color(1.0f, 0.8f, 0.1f), textColor = Color.black };

        private void OnValidate()
        {
            img_zone_item_bg      ??= transform.Find("img_zone_item_bg")?.GetComponent<Image>();
            txt_zone_number_value ??= transform.Find("txt_zone_number_value")?.GetComponent<TextMeshProUGUI>();
        }

        public void Setup(int zoneNumber, ZoneItemState state)
        {
            ZoneAppearanceConfig cfg = state switch
            {
                ZoneItemState.NormalPassed   => _normalPassed,
                ZoneItemState.NormalCurrent  => _normalCurrent,
                ZoneItemState.NormalUpcoming => _normalUpcoming,
                ZoneItemState.SafePassed     => _safePassed,
                ZoneItemState.SafeCurrent    => _safeCurrent,
                ZoneItemState.SafeUpcoming   => _safeUpcoming,
                ZoneItemState.SuperPassed    => _superPassed,
                ZoneItemState.SuperCurrent   => _superCurrent,
                ZoneItemState.SuperUpcoming  => _superUpcoming,
                _                            => _normalUpcoming
            };

            if (txt_zone_number_value != null)
            {
                txt_zone_number_value.text  = zoneNumber > 0 ? zoneNumber.ToString() : "";
                txt_zone_number_value.color = cfg.textColor;
            }

            cfg.ApplyBgTo(img_zone_item_bg);
        }
    }
}
