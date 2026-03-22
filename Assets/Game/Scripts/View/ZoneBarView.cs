using CriticalSpin.Core;
using CriticalSpin.Model;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace CriticalSpin.View
{
    /// <summary>
    /// This is my custom horizontal zone strip UI.
    /// Instead of creating 50 different game objects, I built a physical slot recycling system (Object Pooling).
    /// As the container moves left, the leftmost slot teleports to the right. 
    /// This saves a huge amount of RAM and CPU!
    /// </summary>
    public class ZoneBarView : MonoBehaviour, IView
    {
        [Header("References")]
        [SerializeField] private RectTransform rect_zone_container;
        [SerializeField] private RectTransform rect_mask_area;
        [SerializeField] private ZoneItemView pf_zone_item;

        [Header("Layout")]
        [Tooltip("The actual width of my zone square (prefab width).")]
        [SerializeField] private float _itemWidth = 50f;
        
        [Tooltip("The space between each zone square.")]
        [SerializeField] private float _itemSpacing = 6f;
        
        [Tooltip("How many zone items are visible on each side of the center.")]
        [SerializeField] private int _sideCount = 6;
        
        [Tooltip("Hidden buffer slots on the right so the animation looks smooth when pulling new zones.")]
        [SerializeField] private int _bufferRight = 4;
        
        [Tooltip("Must perfectly match: (sideCount*2+1)*itemWidth + sideCount*2*itemSpacing = 722")]
        [SerializeField] private float _maskWidth = 722f;

        [Header("Animation")]
        [Tooltip("How fast the bar slides to the left when moving to the next zone.")]
        [SerializeField] private float _slideDuration = 0.35f;
        [SerializeField] private Ease _slideEase = Ease.OutBack;

        private readonly List<ZoneItemView> _items = new();
        private readonly List<RectTransform> _rts = new(); // cached RTs, parallel to _items

        private int _currentZone;
        private int _leftmostZone;   // zone shown by _items[0]
        private GameConfig _config;
        private float _stepSize;
        private float _containerX;     // tracks container position (drifts left over time)
        private bool _busy;

        private int TotalSlots => _sideCount * 2 + 1 + _bufferRight;

        private void OnValidate()
        {
            rect_zone_container ??= transform.Find("rect_mask_area/rect_zone_container")?.GetComponent<RectTransform>();
            rect_mask_area ??= transform.Find("rect_mask_area")?.GetComponent<RectTransform>();
        }

        public void Initialize()
        {
            GameEvents.OnZoneChanged += OnZoneChanged;
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void OnDestroy()
        {
            GameEvents.OnZoneChanged -= OnZoneChanged;
        }

        public void Setup(int startZone, GameConfig config)
        {
            _config = config;
            _currentZone = startZone;
            _stepSize = _itemWidth + _itemSpacing;

            // We offset the container so that the center item perfectly aligns with the UI mask.
            _containerX = _maskWidth * 0.5f - _sideCount * _stepSize - _itemWidth * 0.5f;

            // _items[0] shows the zone that is 'sideCount' steps behind the currentZone.
            _leftmostZone = startZone - _sideCount;

            BuildPool();
            ApplyContainerX();
            SetupAllSlots();
        }

        private void OnZoneChanged(int newZone, SpinType _)
        {
            if (_config == null || _busy) return;
            int diff = newZone - _currentZone;
            _currentZone = newZone;

            // Only animate left if we haven't exceeded the total zone count.
            // If we did, just update the colors so the last zone becomes 'Passed', but leave it centered.
            if (_currentZone > (_config != null ? _config.totalZoneCount : 999))
            {
                SetupAllSlots();
            }
            else
            {
                AnimateSlide(diff);
            }
        }

        // ── Animation ────────────────────────────────────────────────

        private void AnimateSlide(int steps)
        {
            if (rect_zone_container == null || steps == 0) return;
            _busy = true;

            float toX = _containerX - steps * _stepSize;

            DOTween.Kill(rect_zone_container);
            DOTween.To(
                    () => rect_zone_container.anchoredPosition.x,
                    x =>
                    {
                        var p = rect_zone_container.anchoredPosition;
                        p.x = x;
                        rect_zone_container.anchoredPosition = p;
                    },
                    toX,
                    _slideDuration)
                .SetEase(_slideEase)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _containerX = toX;
                    RecycleSlots(steps);
                    SetupAllSlots(); // refresh states (Current→Passed, Coming→Current etc.)
                    _busy = false;
                });
        }

        // ── Recycling (Object Pooling) ─────────────────────────────────────────────────

        /// <summary>
        /// I wrote this to automatically move the leftmost invisible slots to the right end 
        /// one at a time, keeping the UI infinitely scrollable without destroying Memory.
        /// </summary>
        private void RecycleSlots(int steps)
        {
            for (int s = 0; s < steps; s++)
            {
                // Leftmost slot is now off-screen left.
                ZoneItemView leftSlot = _items[0];
                RectTransform leftRt = _rts[0];

                // Position it to the right of the current rightmost slot.
                float newLocalX = _rts[_rts.Count - 1].anchoredPosition.x + _stepSize;
                leftRt.anchoredPosition = new Vector2(newLocalX, 0f);

                // The new zone this slot will show.
                int newZone = _leftmostZone + _items.Count;  // = old rightmost zone + 1
                SetupSlot(leftSlot, newZone);

                // Rotate the lists: leftmost becomes rightmost.
                _items.RemoveAt(0);
                _rts.RemoveAt(0);
                _items.Add(leftSlot);
                _rts.Add(leftRt);

                _leftmostZone++;
            }
        }

        // ── Slot setup ─────────────────────────────────────────────────

        private void SetupAllSlots()
        {
            for (int i = 0; i < _items.Count; i++)
                SetupSlot(_items[i], _leftmostZone + i);
        }

        private void SetupSlot(ZoneItemView slot, int zone)
        {
            bool visible = zone >= 1 && zone <= (_config != null ? _config.totalZoneCount : 999);
            slot.gameObject.SetActive(visible);
            if (!visible) return;

            ZoneItemState state;
            bool isSuper = _config.IsSuperZone(zone);
            bool isSafe  = _config.IsSafeZone(zone);

            if (zone < _currentZone)
                state = isSuper ? ZoneItemState.SuperPassed : (isSafe ? ZoneItemState.SafePassed : ZoneItemState.NormalPassed);
            else if (zone == _currentZone)
                state = isSuper ? ZoneItemState.SuperCurrent : (isSafe ? ZoneItemState.SafeCurrent : ZoneItemState.NormalCurrent);
            else
                state = isSuper ? ZoneItemState.SuperUpcoming : (isSafe ? ZoneItemState.SafeUpcoming : ZoneItemState.NormalUpcoming);

            slot.Setup(zone, state);
        }

        // ── Pool construction ──────────

        private void BuildPool()
        {
            if (pf_zone_item == null || rect_zone_container == null) return;

            foreach (var item in _items)
                if (item != null) UIObjectPool.Despawn(item, pf_zone_item);
            _items.Clear();
            _rts.Clear();

            for (int i = 0; i < TotalSlots; i++)
            {
                ZoneItemView slot = UIObjectPool.Spawn(pf_zone_item, rect_zone_container);
                slot.name = $"ui_zone_item_{i:D2}";

                RectTransform rt = (RectTransform)slot.transform;
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(i * _stepSize, 0f);
                rt.sizeDelta = new Vector2(_itemWidth, _itemWidth);

                _items.Add(slot);
                _rts.Add(rt);
            }
        }

        private void ApplyContainerX()
        {
            if (rect_zone_container == null) return;
            var p = rect_zone_container.anchoredPosition;
            p.x = _containerX;
            rect_zone_container.anchoredPosition = p;
        }
    }
}
