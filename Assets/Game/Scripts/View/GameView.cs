using CriticalSpin.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CriticalSpin.View
{
    public class GameView : MonoBehaviour, IView
    {
        [Title("Sub Views")]
        [Required, SerializeField] private WheelView view_wheel;
        [Required, SerializeField] private ZoneBarView view_zone_bar;
        [Required, SerializeField] private ZoneTrackerView view_zone_tracker;
        [Required, SerializeField] private RewardDisplayView view_reward_display;
        [Required, SerializeField] private BombView view_bomb;
        [Required, SerializeField] private ReviveView view_revive;
        [Required, SerializeField] private CashoutView view_cashout;
        [Required, SerializeField] private RewardCollectView view_reward_collect;

        [Title("Buttons")]
        [SerializeField] private Button btn_spin;
        [SerializeField] private Button btn_cashout;

        [Title("HUD")]
        [SerializeField] private TextMeshProUGUI txt_currency_value;

        [Title("Zone Badge Sprites")]
        [SerializeField] private Sprite sp_badge_bronze;
        [SerializeField] private Sprite sp_badge_silver;
        [SerializeField] private Sprite sp_badge_gold;

        private void OnValidate()
        {
            view_wheel ??= GetComponentInChildren<WheelView>();
            view_zone_bar ??= GetComponentInChildren<ZoneBarView>();
            view_zone_tracker ??= GetComponentInChildren<ZoneTrackerView>();
            view_reward_display ??= GetComponentInChildren<RewardDisplayView>();
            view_bomb ??= GetComponentInChildren<BombView>(true);
            view_revive ??= GetComponentInChildren<ReviveView>(true);
            view_cashout ??= GetComponentInChildren<CashoutView>(true);
            view_reward_collect ??= GetComponentInChildren<RewardCollectView>(true);
            btn_spin ??= transform.Find("ui_panel_wheel/btn_spin")?.GetComponent<Button>();
            btn_cashout ??= transform.Find("btn_cashout")?.GetComponent<Button>();
            txt_currency_value ??= transform.Find("ui_panel_hud/ui_panel_currency/txt_currency_value")?.GetComponent<TextMeshProUGUI>();
        }

        public void Initialize()
        {
            view_wheel?.Initialize();
            view_zone_bar?.Initialize();
            view_zone_tracker?.Initialize();
            view_reward_display?.Initialize();
            view_bomb?.Initialize();
            view_revive?.Initialize();
            view_cashout?.Initialize();
            view_reward_collect?.Initialize();
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);


        public void UpdateCurrencyDisplay(int amount)
        {
            if (txt_currency_value != null) txt_currency_value.text = amount.ToString();
        }

        public void SetSpinButtonInteractable(bool interactable)
        {
            if (btn_spin != null) btn_spin.interactable = interactable;
        }

        public void SetCashoutButtonInteractable(bool interactable)
        {
            if (btn_cashout != null) btn_cashout.interactable = interactable;
        }

        public WheelView GetWheelView() => view_wheel;
        public ZoneBarView GetZoneBarView() => view_zone_bar;
        public ZoneTrackerView GetZoneTrackerView() => view_zone_tracker;
        public RewardDisplayView GetRewardDisplayView() => view_reward_display;
        public BombView GetBombView() => view_bomb;
        public ReviveView GetReviveView() => view_revive;
        public CashoutView GetCashoutView() => view_cashout;
        public RewardCollectView GetRewardCollectView() => view_reward_collect;
        public Button GetSpinButton() => btn_spin;
        public Button GetCashoutButton() => btn_cashout;
    }
}
