using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting 
{
    [CreateAssetMenu(menuName = "Plotting/Plots Canvas Skin")]
    public class PlotsCanvasSkin : ScriptableObject
    {
        [Header("General UI Styling")]
        public Color backgroundColor = Color.white;
        public Color dividerColor = Color.black;
        public Color interactionPanelColor = new Color(0.854902f, 0.9490196f, 1, 1);
        public Color interactionPanelOutlineColor = new Color(0.7294118f, 0.854902f, 0.9254902f, 1);
    }
}
