using CriticalSpin.Core;
using CriticalSpin.Model;
using CriticalSpin.View;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace CriticalSpin.Presenter
{
    /// <summary>
    /// I created this Presenter to completely control how the wheel spins.
    /// It calculates random weights, degrees, and uses DOTween for a natural physics feel.
    /// </summary>
    public class WheelPresenter : IPresenter
    {
        private readonly WheelView _view;
        private readonly GameConfig _config;

        private List<WheelSliceData> _slices;
        private bool _isSpinning;
        private int _selectedIndex;

        public WheelPresenter(WheelView view, GameConfig config)
        {
            _view = view;
            _config = config;
        }

        public void Initialize()
        {
            _isSpinning = false;
            _selectedIndex = -1;
        }

        public void Dispose()
        {
            if (_view != null)
            {
                DOTween.Kill(_view.GetRotatingContainer());
                var slots = _view.GetWheelSlots();
                if (slots != null)
                    foreach (var s in slots) if (s != null) DOTween.Kill(s.transform);
            }
        }

        public void SetupWheel(List<WheelSliceData> slices, SpinType spinType)
        {
            _slices = slices;
            _view.SetSpinType(spinType);
            _view.PopulateSlices(slices);

            Transform container = _view.GetRotatingContainer();
            if (container != null)
                container.localRotation = Quaternion.identity;
        }

        public void Spin(int forcedIndex = -1)
        {
            if (_isSpinning || _slices == null || _slices.Count == 0) return;

            _isSpinning = true;
            _selectedIndex = forcedIndex >= 0 ? forcedIndex : WeightedRandom(_slices);

            GameEvents.OnSpinStarted?.Invoke();

            float duration    = Random.Range(_config.minSpinDuration, _config.maxSpinDuration);
            float targetAngle = SliceAngle(_selectedIndex, _slices.Count);
            float totalAngle  = 360f * _config.extraFullRotations + targetAngle;

            // Overshoot: I make the wheel spin slightly past the target, then bounce back like a real casino wheel.
            float overshoot     = _config.overshootDegrees;
            float settleDuration = Mathf.Clamp(duration * 0.12f, 0.2f, 0.5f);

            Transform container = _view.GetRotatingContainer();
            if (container == null) { FinishSpin(); return; }

            container.localRotation = Quaternion.identity;

            Sequence spinSeq = DOTween.Sequence().SetLink(_view.gameObject);

            // Phase 1 — the main fast spin, pushing slightly past the target angle.
            spinSeq.Append(
                container
                    .DORotate(new Vector3(0f, 0f, -(totalAngle + overshoot)), duration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic)
            );

            // Phase 2 — bounce back to the exact target with a gentle elastic/back feel.
            spinSeq.Append(
                container
                    .DOLocalRotate(new Vector3(0f, 0f, -(totalAngle % 360f)), settleDuration)
                    .SetEase(Ease.OutBack)
            );

            spinSeq.OnComplete(FinishSpin);
        }


        private void FinishSpin()
        {
            _isSpinning = false;
            if (_slices != null && _selectedIndex >= 0 && _selectedIndex < _slices.Count)
                GameEvents.OnSpinCompleted?.Invoke(_slices[_selectedIndex]);
        }

        /// <summary>
        /// Simple math formula I use to calculate the exact Z rotation angle for a specific slice index.
        /// </summary>
        private float SliceAngle(int index, int total)
        {
            return (360f / total) * index;
        }

        /// <summary>
        /// I use a weighted random algorithm here instead of a simple Random.Range.
        /// This ensures the bomb or big rewards accurately follow their percentage chances!
        /// </summary>
        private int WeightedRandom(List<WheelSliceData> slices)
        {
            float total = 0f;
            foreach (var s in slices) total += s.weight;

            float pick = Random.Range(0f, total);
            float cumulative = 0f;
            for (int i = 0; i < slices.Count; i++)
            {
                cumulative += slices[i].weight;
                if (pick <= cumulative) return i;
            }
            return slices.Count - 1;
        }

        public bool IsSpinning() => _isSpinning;
    }
}
