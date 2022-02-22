using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting {
    [CreateAssetMenu(menuName = "Plot UI Skin")]
    public class PlotUISkin : ScriptableObject
    {
        [Header("Plot Styling")]
        public Color plotColor;
        public Color borderColor;
        public Color outlineColor;
        public Color axisLabelTextColor;
        public Color gridlineColor;
        public Color tickMarkColor;
        public Color deleteButtonColor;

        [Header("Selection Styling")]
        public Color defaultColor;
        public Color highlightedColor;
        public Color maskedColor;
    }
}
