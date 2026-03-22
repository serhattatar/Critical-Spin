using UnityEngine;
using TMPro;

namespace CriticalSpin.View
{
    /// <summary>
    /// This is a small UI slot that shows up on the right side of the screen 
    /// to tell the player how many steps are left until a big zone.
    /// </summary>
    public class ZoneTrackerItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txt_zone_name;
        [SerializeField] private TextMeshProUGUI txt_zones_left;
        
        private RectTransform _rectTransform;
        
        private void OnValidate()
        {
            txt_zone_name  ??= transform.Find("txt_zone_name")?.GetComponent<TextMeshProUGUI>();
            txt_zones_left ??= transform.Find("txt_zones_left")?.GetComponent<TextMeshProUGUI>();
        }

        public RectTransform GetRectTransform()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }

        public void Setup(string targetName, int zonesLeft)
        {
            if (txt_zone_name != null) txt_zone_name.text = targetName;
            
            if (txt_zones_left != null)
            {
                if (zonesLeft <= 0) 
                    txt_zones_left.text = "ARRIVED";
                else 
                    txt_zones_left.text = $"{zonesLeft} ZONES LEFT";
            }
        }
    }
}
