using UnityEngine;
using CriticalSpin.Presenter;
using CriticalSpin.Model;
using DG.Tweening;
using CriticalSpin.Core;
using Sirenix.OdinInspector;

namespace CriticalSpin.View
{
    /// <summary>
    /// I built this Tracker to calculate the distance to the next milestones (Safe/Super).
    /// It uses DOTween to smoothly slide the closer milestone to the top, like a racing leaderboard!
    /// </summary>
    public class ZoneTrackerView : MonoBehaviour, IView
    {
        [Title("Tracker Slots")]
        [Required, SerializeField] private ZoneTrackerItemView item_safe_zone;
        [Required, SerializeField] private ZoneTrackerItemView item_super_zone;
        
        [Title("Animation Settings")]
        [Tooltip("The local Y position for the slot that is closest to the player (Winner position).")]
        [SerializeField] private float topY = 0f;
        
        [Tooltip("The local Y position for the slot that is further away (Loser position).")]
        [SerializeField] private float bottomY = -120f;
        
        [Tooltip("How fast the slots swap places when their distances change.")]
        [SerializeField] private float animationDuration = 0.5f;

        private ZoneManager _zoneManager;

        private void OnValidate()
        {
            item_safe_zone  ??= transform.Find("item_safe_zone")?.GetComponent<ZoneTrackerItemView>();
            item_super_zone ??= transform.Find("item_super_zone")?.GetComponent<ZoneTrackerItemView>();
        }

        public void Initialize()
        {
            GameEvents.OnZoneChanged += HandleZoneChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnZoneChanged -= HandleZoneChanged;
            
            if (item_safe_zone != null) DOTween.Kill(item_safe_zone.GetRectTransform());
            if (item_super_zone != null) DOTween.Kill(item_super_zone.GetRectTransform());
        }

        public void Setup(ZoneManager zoneManager)
        {
            _zoneManager = zoneManager;
            RefreshDisplay(zoneManager.GetCurrentZone(), false);
        }

        private void HandleZoneChanged(int newZone, SpinType spinType)
        {
            if (_zoneManager != null)
                RefreshDisplay(newZone, true);
        }

        private void RefreshDisplay(int currentZone, bool animate)
        {
            int nextSafe  = _zoneManager.GetNextSafeZone();
            int nextSuper = _zoneManager.GetNextSuperZone();

            bool showSafe  = nextSafe != -1;
            bool showSuper = nextSuper != -1;

            int safeLeft  = showSafe ? nextSafe - currentZone : 999;
            int superLeft = showSuper ? nextSuper - currentZone : 999;

            bool safeIsCloser = safeLeft <= superLeft;

            Vector2 safeTargetPos  = new Vector2(0, (!showSuper || safeIsCloser) ? topY : bottomY);
            Vector2 superTargetPos = new Vector2(0, (!showSafe || !safeIsCloser) ? topY : bottomY);

            if (item_safe_zone != null)
            {
                var rt = item_safe_zone.GetRectTransform();
                if (showSafe)
                {
                    item_safe_zone.Setup($"S A F E  Z O N E  {nextSafe}", safeLeft);
                    
                    if (animate)
                    {
                        DOTween.Kill(rt); // CRITICAL FIX: I must kill old animations before starting new lambdas!
                        rt.DOScale(1f, animationDuration).SetLink(gameObject);
                        DOTween.To(() => rt.anchoredPosition, x => rt.anchoredPosition = x, safeTargetPos, animationDuration).SetEase(Ease.OutBack).SetLink(gameObject);
                    }
                    else
                    {
                        rt.localScale = Vector3.one;
                        rt.anchoredPosition = safeTargetPos;
                    }
                }
                else
                {
                    if (animate) 
                    {
                        DOTween.Kill(rt);
                        rt.DOScale(0f, animationDuration).SetLink(gameObject);
                    }
                    else rt.localScale = Vector3.zero;
                }
            }

            if (item_super_zone != null)
            {
                var rt = item_super_zone.GetRectTransform();
                if (showSuper)
                {
                    item_super_zone.Setup($"S U P E R  Z O N E  {nextSuper}", superLeft);
                    
                    if (animate)
                    {
                        DOTween.Kill(rt); // Prevent memory leaks and tween conflicts
                        rt.DOScale(1f, animationDuration).SetLink(gameObject);
                        DOTween.To(() => rt.anchoredPosition, x => rt.anchoredPosition = x, superTargetPos, animationDuration).SetEase(Ease.OutBack).SetLink(gameObject);
                    }
                    else
                    {
                        rt.localScale = Vector3.one;
                        rt.anchoredPosition = superTargetPos;
                    }
                }
                else
                {
                    if (animate) 
                    {
                        DOTween.Kill(rt);
                        rt.DOScale(0f, animationDuration).SetLink(gameObject);
                    }
                    else rt.localScale = Vector3.zero;
                }
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
