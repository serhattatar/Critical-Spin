using CriticalSpin.Core;
using CriticalSpin.Model;
using CriticalSpin.View;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CriticalSpin.Presenter
{
    /// <summary>
    /// This is the core brain of my game. It connects all the Views (UI) 
    /// with the Managers (Logic) using the MVP pattern. 
    /// It handles the main gameplay loop: spinning the wheel, managing revives, and cashing out.
    /// </summary>
    public class GamePresenter : MonoBehaviour, IPresenter
    {
        [Title("Configuration")]
        [Required, Tooltip("The core settings for the game rules.")]
        [SerializeField] private GameConfig _gameConfig;

        [Title("Zone Data")]
        [Tooltip("List of specific zone data override files. If a zone is not here, I use the defaults below.")]
        [SerializeField] private List<ZoneData> _zoneDatas = new List<ZoneData>();
        
        [Tooltip("The default wheel layout for standard bronze zones.")]
        [SerializeField] private ZoneData _defaultBronzeZone;
        [Tooltip("The default wheel layout for risk-free silver zones.")]
        [SerializeField] private ZoneData _defaultSilverZone;
        
        [Tooltip("The default wheel layout for super golden zones.")]
        [SerializeField] private ZoneData _defaultGoldenZone;

        [Title("View")]
        [Required, Tooltip("Main view container that holds all UI panels.")]
        [SerializeField] private GameView _gameView;

        private ZoneManager _zoneManager;
        private WheelPresenter _wheelPresenter;
        private RewardManager _rewardManager;
        private CurrencyManager _currencyManager;

        private GameState _state = GameState.Idle;
        private int _revivesUsed;
        private RewardData _pendingReward;

#if UNITY_EDITOR || UNITY_STANDALONE
        // Auto-spin cheat state
        private int _autoSpinCount;
#endif

        private void OnValidate() => _gameView ??= FindObjectOfType<GameView>();

        private void Start() => Initialize();

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleCheatInput();
            ProcessAutoSpin();
#endif
        }

        public void Initialize()
        {
            // I create my managers manually here instead of using MonoBehaviour singletons 
            // to keep the architecture clean and testable.
            _zoneManager = new ZoneManager(_gameConfig);
            _currencyManager = new CurrencyManager(_gameConfig.startingCurrency);
            _rewardManager = new RewardManager(_currencyManager);
            _wheelPresenter = new WheelPresenter(_gameView.GetWheelView(), _gameConfig);

            _gameView.Initialize();

            // Register events and bind buttons BEFORE initializing managers, 
            // so I don't miss initial setup events like currency updates.
            RegisterEvents();
            BindButtons();

            _zoneManager.Initialize();
            _currencyManager.Initialize();
            _rewardManager.Initialize();
            _wheelPresenter.Initialize();

            SetupZone();

            // Build the zone bar and tracker on start so they are visible immediately.
            int startZone = _zoneManager.GetCurrentZone();
            _gameView.GetZoneBarView()?.Setup(startZone, _gameConfig);
            _gameView.GetZoneTrackerView()?.Setup(_zoneManager);
        }


        public void Dispose()
        {
            UnregisterEvents();
            _wheelPresenter?.Dispose();
            _currencyManager?.Dispose();
        }

        private void OnDestroy() => Dispose();

        private void RegisterEvents()
        {
            GameEvents.OnSpinCompleted += OnSpinCompleted;
            GameEvents.OnRewardCollected += OnRewardCollected;
            GameEvents.OnAllRewardsLost += OnAllRewardsLost;
            GameEvents.OnZoneChanged += OnZoneChanged;
            GameEvents.OnCurrencyChanged += OnCurrencyChanged;
        }

        private void UnregisterEvents()
        {
            GameEvents.OnSpinCompleted -= OnSpinCompleted;
            GameEvents.OnRewardCollected -= OnRewardCollected;
            GameEvents.OnAllRewardsLost -= OnAllRewardsLost;
            GameEvents.OnZoneChanged -= OnZoneChanged;
            GameEvents.OnCurrencyChanged -= OnCurrencyChanged;
        }

        private void BindButtons()
        {
            _gameView.GetSpinButton()?.onClick.AddListener(OnSpinClicked);
            _gameView.GetCashoutButton()?.onClick.AddListener(OnCashoutClicked);
            _gameView.GetBombView()?.GetRestartButton()?.onClick.AddListener(OnRestartClicked);
            _gameView.GetBombView()?.GetReviveButton()?.onClick.AddListener(OnReviveFromBombClicked);
            _gameView.GetReviveView()?.GetAcceptButton()?.onClick.AddListener(OnReviveAccepted);
            _gameView.GetReviveView()?.GetDeclineButton()?.onClick.AddListener(OnReviveDeclined);
            _gameView.GetCashoutView()?.GetConfirmButton()?.onClick.AddListener(OnCashoutConfirmed);
        }

        private void OnSpinClicked()
        {
            if (_state != GameState.Idle) return;
            _state = GameState.Spinning;
            _gameView.SetSpinButtonInteractable(false);
            _gameView.SetCashoutButtonInteractable(false);
            _wheelPresenter.Spin();
        }

        private void OnCashoutClicked()
        {
            if (_state != GameState.Idle || !_zoneManager.CanCashout() || !_rewardManager.HasRewards()) return;
            _state = GameState.Cashout;
            _gameView.SetSpinButtonInteractable(false);
            _gameView.SetCashoutButtonInteractable(false);
            _gameView.GetCashoutView().PopulateRewards(_rewardManager.GetCollectedRewards(), _rewardManager.GetTotalValue());
            _gameView.GetCashoutView().Show();
        }

        private void OnRestartClicked()
        {
            _gameView.GetBombView()?.Hide();
            Restart();
        }

        private void OnReviveFromBombClicked()
        {
            _gameView.GetBombView()?.Hide();
            var rv = _gameView.GetReviveView();
            rv.UpdateInfo(_gameConfig.reviveCost, _currencyManager.GetCurrentCurrency(), _revivesUsed, _gameConfig.maxReviveCount);
            rv.Show();
        }

        private void OnReviveAccepted()
        {
            if (!_currencyManager.TrySpendCurrency(_gameConfig.reviveCost)) return;
            _revivesUsed++;
            _gameView.GetReviveView()?.Hide();
            _state = GameState.Idle;
            _gameView.SetSpinButtonInteractable(true);
            RefreshCashoutButton();
        }

        private void OnReviveDeclined()
        {
            _gameView.GetReviveView()?.Hide();
            Restart();
        }

        private void OnCashoutConfirmed()
        {
            // Apply rewards to currency before resetting the session.
            _rewardManager.ExportRewards();
            _gameView.GetCashoutView()?.Hide();
            Restart();
        }

        private void OnSpinCompleted(WheelSliceData result)
        {
            _state = GameState.ShowingResult;

            if (result.IsBomb)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                StopAutoSpin(); // Cancel cheat on bomb
#endif
                _state = GameState.BombExploded;
                _pendingReward = null;
                // DO NOT clear rewards here! Give the player a chance to Revive and keep them.
                // Rewards are only cleared inside Restart() if they decline or don't have enough currency.

                bool canRevive = _gameConfig.reviveEnabled
                    && _revivesUsed < _gameConfig.maxReviveCount
                    && _currencyManager.HasEnough(_gameConfig.reviveCost);

                _gameView.GetBombView().Configure(canRevive, _gameConfig.reviveCost, _currencyManager.GetCurrentCurrency());
                _gameView.GetBombView().Show();
                GameEvents.OnBombHit?.Invoke();
            }
            else
            {
                _pendingReward = result.reward;

                DOVirtual.DelayedCall(_gameConfig.postSpinDelay, () =>
                {
                    if (_state != GameState.ShowingResult) return;
                    ShowCollectPopup(result);
                }).SetLink(_gameView.gameObject);
            }
        }

        private void ShowCollectPopup(WheelSliceData result)
        {
            if (result.IsBomb || _state == GameState.BombExploded) return;

            var collectView = _gameView.GetRewardCollectView();
#if UNITY_EDITOR || UNITY_STANDALONE
            if (collectView == null || result.reward == null || _autoSpinCount > 0)
#else
            if (collectView == null || result.reward == null)
#endif
            {
                // Bypass popup if missing OR if cheat is active
                AdvanceZoneAfterCollect();
                return;
            }

            collectView.ShowReward(result.reward, AdvanceZoneAfterCollect);
        }

        private void AdvanceZoneAfterCollect()
        {
            if (_pendingReward != null)
            {
                _rewardManager.AddReward(_pendingReward);
                _pendingReward = null;
            }

            _zoneManager.AdvanceToNextZone();
            _state = GameState.Idle;
            RefreshCashoutButton();

            DOVirtual.DelayedCall(1f, () =>
            {
                _gameView.SetSpinButtonInteractable(!_zoneManager.IsFinished());
            }).SetLink(_gameView.gameObject);
        }

        private void OnRewardCollected(RewardData reward) =>
            _gameView.GetRewardDisplayView()?.AddReward(reward);

        private void OnAllRewardsLost() =>
            _gameView.GetRewardDisplayView()?.ResetDisplay();

        private void OnZoneChanged(int zone, SpinType type)
        {
            SetupZone();
        }

        private void OnCurrencyChanged(int amount) =>
            _gameView.UpdateCurrencyDisplay(amount);

        private void SetupZone()
        {
            int zone = _zoneManager.GetCurrentZone();
            SpinType type = _zoneManager.GetCurrentSpinType();
            ZoneData data = ResolveZoneData(zone, type);

            if (data != null && data.slices.Count > 0)
                _wheelPresenter.SetupWheel(data.slices, type);

            RefreshCashoutButton();
        }

        private ZoneData ResolveZoneData(int zone, SpinType type)
        {
            foreach (var zd in _zoneDatas)
                if (zd != null && zd.zoneIndex == zone) return zd;

            return type switch
            {
                SpinType.Silver => _defaultSilverZone,
                SpinType.Golden => _defaultGoldenZone,
                _ => _defaultBronzeZone
            };
        }

        private void RefreshCashoutButton() =>
            _gameView.SetCashoutButtonInteractable(_zoneManager.CanCashout() && _rewardManager.HasRewards());

        private void Restart()
        {
            _state = GameState.Idle;
            _revivesUsed = 0;

            _zoneManager.ResetZone();
            _rewardManager.Initialize();
            _wheelPresenter.Initialize();

            _gameView.GetRewardDisplayView()?.ResetDisplay();
            _gameView.GetZoneBarView()?.Setup(1, _gameConfig);
            _gameView.SetSpinButtonInteractable(true);
            RefreshCashoutButton();
            SetupZone();

            GameEvents.OnGameRestarted?.Invoke();
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        // ── Auto-Spin Cheat / Debug ──────────────────────────────────────────
        // I wrote this section to instantly test mechanics without painful clicking.
        // It's only enabled in the Editor and PC build, so it won't be in the final mobile release.

        private void HandleCheatInput()
        {
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    StartAutoSpin(i * 10);
                    break;
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape)) StopAutoSpin();
        }

        private void StartAutoSpin(int count)
        {
            _autoSpinCount = count;
            Time.timeScale = 15f;
            Debug.Log($"[Cheat] AutoSpin started: {count} spins at 3x speed.");
        }

        private void StopAutoSpin()
        {
            if (_autoSpinCount > 0)
            {
                _autoSpinCount = 0;
                Time.timeScale = 1f;
                Debug.Log("[Cheat] AutoSpin stopped.");
            }
        }

        private void ProcessAutoSpin()
        {
            if (_autoSpinCount <= 0) return;

            // Wait until game is idle and ready for a new spin
            if (_state == GameState.Idle && _gameView.GetSpinButton() != null && _gameView.GetSpinButton().interactable)
            {
                _autoSpinCount--;
                if (_autoSpinCount == 0) Time.timeScale = 1f; // Revert time scale on the very last spin
                OnSpinClicked();
            }
        }
#endif
    }
}
