using UnityEngine;

namespace CriticalSpin.Core
{
    /// <summary>
    /// I wrote this script to fit the UI inside the notch/safe area of modern mobile phones.
    /// It only calculates the anchors when the screen size changes, keeping performance at maximum!
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea = new Rect(0, 0, 0, 0);

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;

            // Only update if the safe area actually changed (e.g., rotating the phone)
            if (safeArea != _lastSafeArea)
            {
                _lastSafeArea = safeArea;
                
                Vector2 anchorMin = safeArea.position;
                Vector2 anchorMax = safeArea.position + safeArea.size;

                // Convert safe area pixel coordinates to normalized Anchor values (0 to 1)
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                _rectTransform.anchorMin = anchorMin;
                _rectTransform.anchorMax = anchorMax;
            }
        }

        /// <summary>
        /// Unity calls this automatically if the device is rotated (Landscape Left to Right).
        /// </summary>
        private void OnRectTransformDimensionsChange()
        {
            if (_rectTransform != null)
            {
                ApplySafeArea();
            }
        }
    }
}
