using CriticalSpin.Core;
using CriticalSpin.Model;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CriticalSpin.View
{
    // Shows the spin wheel: rotating container, type images, and the slice slots.
    public class WheelView : MonoBehaviour, IView
    {
        [Title("Wheel Base Images")]
        [SerializeField] private Image img_spin_bronze_base;
        [SerializeField] private Image img_spin_silver_base;
        [SerializeField] private Image img_spin_golden_base;

        [Title("Indicators")]
        [SerializeField] private Image img_spin_bronze_indicator;
        [SerializeField] private Image img_spin_silver_indicator;
        [SerializeField] private Image img_spin_golden_indicator;

        [Title("Containers")]
        [Tooltip("The root container that rotates during spin: ui_animator_wheel_container")]
        [SerializeField] private Transform tf_wheel_rotating_base;

        [Title("Static Hierarchy")]
        [SerializeField] private WheelSliceView[] _wheelSlots;

        private void OnValidate()
        {
            tf_wheel_rotating_base ??= transform.Find("ui_animator_wheel_container");

            img_spin_bronze_base      ??= transform.Find("ui_animator_wheel_container/ui_image_spin_bronze_base")?.GetComponent<Image>();
            img_spin_silver_base      ??= transform.Find("ui_animator_wheel_container/ui_image_spin_silver_base")?.GetComponent<Image>();
            img_spin_golden_base      ??= transform.Find("ui_animator_wheel_container/ui_image_spin_golden_base")?.GetComponent<Image>();
            img_spin_bronze_indicator ??= transform.Find("ui_image_spin_bronze_indicator")?.GetComponent<Image>();
            img_spin_silver_indicator ??= transform.Find("ui_image_spin_silver_indicator")?.GetComponent<Image>();
            img_spin_golden_indicator ??= transform.Find("ui_image_spin_golden_indicator")?.GetComponent<Image>();
        }

        public void Initialize() => SetSpinType(SpinType.Bronze);
        public void Show()       => gameObject.SetActive(true);
        public void Hide()       => gameObject.SetActive(false);

        public void SetSpinType(SpinType spinType)
        {
            img_spin_bronze_base?.gameObject.SetActive(spinType == SpinType.Bronze);
            img_spin_silver_base?.gameObject.SetActive(spinType == SpinType.Silver);
            img_spin_golden_base?.gameObject.SetActive(spinType == SpinType.Golden);
            img_spin_bronze_indicator?.gameObject.SetActive(spinType == SpinType.Bronze);
            img_spin_silver_indicator?.gameObject.SetActive(spinType == SpinType.Silver);
            img_spin_golden_indicator?.gameObject.SetActive(spinType == SpinType.Golden);
        }

        // Sets each slot's data. Slots beyond the slice count are hidden.
        public void PopulateSlices(List<WheelSliceData> slices)
        {
            if (_wheelSlots == null || _wheelSlots.Length == 0) return;

            int total = slices.Count;

            for (int i = 0; i < total; i++)
            {
                if (i >= _wheelSlots.Length) break;
                WheelSliceView view = _wheelSlots[i];
                if (view != null)
                {
                    view.gameObject.SetActive(true);
                    view.Setup(slices[i]);
                }
            }

            for (int i = total; i < _wheelSlots.Length; i++)
            {
                if (_wheelSlots[i] != null)
                    _wheelSlots[i].gameObject.SetActive(false);
            }
        }

        public Transform        GetRotatingContainer() => tf_wheel_rotating_base;
        public WheelSliceView[] GetWheelSlots()        => _wheelSlots;
    }
}
