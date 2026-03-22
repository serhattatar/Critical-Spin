using UnityEngine;
using CriticalSpin.Core;

namespace CriticalSpin.Presenter
{
    /// <summary>
    /// This script handles saving and loading the player's gold.
    /// I used PlayerPrefs here because it's simple and fast for a demo project.
    /// </summary>
    public class CurrencyManager : IPresenter
    {
        private const string CURRENCY_KEY = "CriticalSpin_Currency";
        private int _currentCurrency;
        private readonly int _startingCurrency;

        public CurrencyManager(int startingCurrency)
        {
            _startingCurrency = startingCurrency;
        }

        public void Initialize()
        {
            _currentCurrency = PlayerPrefs.GetInt(CURRENCY_KEY, _startingCurrency);
            GameEvents.OnCurrencyChanged?.Invoke(_currentCurrency);
        }

        public void Dispose()
        {
            Save();
        }

        /// <summary>
        /// I use this to add gold to the player's wallet and instantly alert the UI.
        /// </summary>
        public void AddCurrency(int amount)
        {
            _currentCurrency += amount;
            Save();
            GameEvents.OnCurrencyChanged?.Invoke(_currentCurrency);
        }

        /// <summary>
        /// Tries to spend gold (like when buying a Revive).
        /// Returns false if the player is broke.
        /// </summary>
        public bool TrySpendCurrency(int amount)
        {
            if (_currentCurrency < amount) return false;

            _currentCurrency -= amount;
            Save();
            GameEvents.OnCurrencyChanged?.Invoke(_currentCurrency);
            return true;
        }

        public bool HasEnough(int amount)       => _currentCurrency >= amount;
        public int  GetCurrentCurrency()        => _currentCurrency;

        private void Save()
        {
            PlayerPrefs.SetInt(CURRENCY_KEY, _currentCurrency);
            PlayerPrefs.Save();
        }
    }
}
