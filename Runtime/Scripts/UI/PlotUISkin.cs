using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting {
    [CreateAssetMenu(menuName = "Plot UI Skin")]
    public class PlotUISkin : ScriptableObject
    {
        [Header("General UI Styling")]
        public Color backgroundColor;
        public Color dividerColor;
        public Color interactionPanelColor;
        public Color interactionPanelOutlineColor;
        public Color createPlotButtonColor;
        public Color createPlotFromSelectedButtonColor;
        public Color selectionModeButtonColor;
        public Color selectionModeIconColor;

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

        [Header("Parallel Coords Styling")]
        public Color defaultLineColor;
        public Color highlightedLineColor;
        public Color maskedLineColor;
    }
}
