using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CriticalSpin.Model
{
    /// <summary>
    /// I use this to define what combinations of slices appear in specific zones.
    /// It automatically calculates whether it's a Safe, Super, or Normal zone based on its Index.
    /// </summary>
    [CreateAssetMenu(fileName = "ZoneData_New", menuName = "CriticalSpin/Data/Zone Data")]
    public class ZoneData : ScriptableObject
    {
        [Title("Zone Identity")]
        [LabelText("Zone Index")]
        [Tooltip("Which level number this zone represents. E.g. 5 is usually the first Safe Zone.")]
        [MinValue(1)]
        public int zoneIndex = 1;

        [LabelText("Spin Type")]
        [ReadOnly]
        public SpinType spinType = SpinType.Bronze;

        [Title("Wheel Slices")]
        [LabelText("Slices on Wheel")]
        [ListDrawerSettings(ShowIndexLabels = true, NumberOfItemsPerPage = 8)]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        public List<WheelSliceData> slices = new List<WheelSliceData>();

        [Title("Info")]
        [ShowInInspector, ReadOnly]
        public bool IsSafeZone  => zoneIndex % 5  == 0 && zoneIndex % 30 != 0;

        [ShowInInspector, ReadOnly]
        public bool IsSuperZone => zoneIndex % 30 == 0;

        [ShowInInspector, ReadOnly]
        public bool HasBomb     => spinType == SpinType.Bronze;

        private void OnValidate()
        {
            // Auto-calculate spin type from zone index.
            if (zoneIndex % 30 == 0)
                spinType = SpinType.Golden;
            else if (zoneIndex % 5 == 0)
                spinType = SpinType.Silver;
            else
                spinType = SpinType.Bronze;
        }
    }
}
