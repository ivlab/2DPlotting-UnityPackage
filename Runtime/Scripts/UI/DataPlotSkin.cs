using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting 
{
    [CreateAssetMenu(menuName = "Plotting/Data Plots/Data Plot Skin (Override)")]
    public class DataPlotSkin : ScriptableObject
    {
        [Header("Data Plot Styling")]
        public Color plotColor = Color.white;
        public Color borderColor = new Color(0.8941177f, 0.9372549f, 0.9529412f, 1);
        public Color outlineColor = new Color(0, 0, 0, 0.5019608f);
        public Color axisLabelTextColor = Color.black;
        public Color gridlineColor = new Color(0, 0, 0, 0.1960784f);
        public Color tickMarkColor = Color.black;
        public Color deleteButtonColor = new Color(0.2666667f, 0.5843138f, 0.6980392f, 1);
        public RectPadding padding = new RectPadding(50);

        [Header("Data Plot Settings")]
        public bool scaleAxesToZero = false;

        [Header("Selection Styling")]
        public Color defaultColor = Color.black;
        public Color highlightedColor = new Color(0.2156863f, 0.7411765f, 0.9333333f, 1);
        public Color maskedColor = Color.clear;
        public Color rectSelectionColor = new Color(0.5254902f, 0.8745098f, 1, 0.2941177f);
        public Color rectSelectionOutlineColor = new Color(0.3215686f, 0.5294118f, 0.6039216f, 0.682353f);
        public Color brushSelectionColor = new Color(0.5254902f, 0.8745098f, 1, 0.5450981f);

        [Header("Selection Settings")]
        public float clickSelectionRadius = 10;
        public float brushSelectionRadius = 15;
        public float selectionPadding = 10;

        public void ApplyOverrideStyling(DataPlotSkin plotSkin)
        {
            plotColor = plotSkin.plotColor;
            borderColor = plotSkin.borderColor;
            outlineColor = plotSkin.outlineColor;
            axisLabelTextColor = plotSkin.axisLabelTextColor;
            gridlineColor = plotSkin.gridlineColor;
            tickMarkColor = plotSkin.tickMarkColor;
            deleteButtonColor = plotSkin.deleteButtonColor;
            padding = plotSkin.padding;
            
            scaleAxesToZero = plotSkin.scaleAxesToZero;

            defaultColor = plotSkin.defaultColor;
            highlightedColor = plotSkin.highlightedColor;
            maskedColor = plotSkin.maskedColor;
            rectSelectionColor = plotSkin.rectSelectionColor;
            rectSelectionOutlineColor = plotSkin.rectSelectionOutlineColor;
            brushSelectionColor = plotSkin.brushSelectionColor;

            clickSelectionRadius = plotSkin.clickSelectionRadius;
            brushSelectionRadius = plotSkin.brushSelectionRadius;
            selectionPadding = plotSkin.selectionPadding;
        }
    }
}
