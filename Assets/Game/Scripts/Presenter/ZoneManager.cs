using UnityEngine;
using CriticalSpin.Core;
using CriticalSpin.Model;

namespace CriticalSpin.Presenter
{
    /// <summary>
    /// I use this class to track which zone the player is currently in.
    /// It handles all calculations related to safe zones, super zones, and checking if the game is finished.
    /// </summary>
    public class ZoneManager : IPresenter
    {
        private readonly GameConfig _config;
        private int _currentZone = 1;

        public ZoneManager(GameConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            _currentZone = 1;
        }

        public void Dispose() { }

        /// <summary>
        /// Moves the player to the next zone and triggers the OnZoneChanged event.
        /// I hooked this up to the GameEvents so the UI updates automatically.
        /// </summary>
        public void AdvanceToNextZone()
        {
            _currentZone++;
            SpinType spinType = _config.GetSpinType(_currentZone);
            GameEvents.OnZoneChanged?.Invoke(_currentZone, spinType);
        }

        /// <summary>
        /// Resets the zone back to 1. I call this when the player cashes out or loses all rewards.
        /// </summary>
        public void ResetZone()
        {
            _currentZone = 1;
            GameEvents.OnZoneChanged?.Invoke(_currentZone, SpinType.Bronze);
        }

        public int      GetCurrentZone()      => _currentZone;
        public SpinType GetCurrentSpinType()  => _config.GetSpinType(_currentZone);
        public bool     IsCurrentZoneSafe()   => _config.IsSafeZone(_currentZone);
        public bool     IsCurrentZoneSuper()  => _config.IsSuperZone(_currentZone);

        /// <summary>Checks if the current zone has a bomb ( Bronze Spin ).</summary>
        public bool CurrentZoneHasBomb() => _config.ZoneHasBomb(_currentZone);

        /// <summary>Checks if the player has reached the maximum allowed zones.</summary>
        public bool IsFinished() => _currentZone > _config.totalZoneCount;

        /// <summary>
        /// The player can only cash out if they are on a Safe Zone, Super Zone, or if they finished the game.
        /// I added this rule based on the game design document.
        /// </summary>
        public bool CanCashout() => IsCurrentZoneSafe() || IsCurrentZoneSuper() || IsFinished();

        /// <summary>Finds the next upcoming safe zone for my UI Navigation Tracker.</summary>
        public int GetNextSafeZone()
        {
            int next = ((_currentZone / _config.safeZoneInterval) + 1) * _config.safeZoneInterval;
            while (_config.IsSuperZone(next) && next <= _config.totalZoneCount)
            {
                next += _config.safeZoneInterval;
            }
            return next > _config.totalZoneCount ? -1 : next;
        }

        /// <summary>Finds the next upcoming super zone for my UI Navigation Tracker.</summary>
        public int GetNextSuperZone()
        {
            int next = ((_currentZone / _config.superZoneInterval) + 1) * _config.superZoneInterval;
            return next > _config.totalZoneCount ? -1 : next;
        }
    }
}
