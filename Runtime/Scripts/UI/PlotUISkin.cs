using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting {
    [CreateAssetMenu(menuName = "Plot UI Skin")]
    public class PlotUISkin : ScriptableObject
    {
        [Header("General UI Styling")]
        public Color backgroundColor = Color.white;
        public Color dividerColor = Color.black;
        public Color interactionPanelColor = new Color(0.854902f, 0.9490196f, 1, 1);
        public Color interactionPanelOutlineColor = new Color(0.7294118f, 0.854902f, 0.9254902f, 1);
        public Color createPlotButtonColor = Color.white;
        public Color createPlotFromSelectedButtonColor = new Color(0.2156863f, 0.7411765f, 0.9333333f, 1);
        public Color selectionModeButtonColor = Color.white;
        public Color selectionModeIconColor = new Color(0.854902f, 0.9490196f, 1, 1);

        [Header("Plot Styling")]
        public Color plotColor = Color.white;
        public Color borderColor = new Color(0.8941177f, 0.9372549f, 0.9529412f, 1);
        public Color outlineColor = new Color(0, 0, 0, 0.5019608f);
        public Color axisLabelTextColor = Color.black;
        public Color gridlineColor = new Color(0, 0, 0, 0.1960784f);
        public Color tickMarkColor = Color.black;
        public Color deleteButtonColor = new Color(0.2666667f, 0.5843138f, 0.6980392f, 1);

        [Header("Selection Styling")]
        public Color defaultColor = Color.black;
        public Color highlightedColor = new Color(0.2156863f, 0.7411765f, 0.9333333f, 1);
        public Color maskedColor;

        [Header("Parallel Coords Styling")]
        public Color defaultLineColor = new Color(0, 0, 0, 0.2941177f);
        public Color highlightedLineColor = new Color(0.4117647f, 0.8431373f, 1, 0.627451f);
        public Color maskedLineColor;
    }
}
