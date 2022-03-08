using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting 
{
    [CreateAssetMenu(menuName = "Plotting/Data Plots/Parallel Coords Plot Skin")]
    public class ParallelCoordsPlotSkin : DataPlotSkin
    {
        [Header("Parallel Coords Plot Settings")]
        public float pointSize = 3;
        public float lineWidth = 1;

        [Header("Parallel Coords Plot Styling")]
        public Color defaultLineColor = new Color(0, 0, 0, 0.2941177f);
        public Color highlightedLineColor = new Color(0.4117647f, 0.8431373f, 1, 0.627451f);
        public Color maskedLineColor;
        public Color flipAxisButtonColor = Color.white;
        public Color flipAxisButtonOutlineColor = new Color(0, 0, 0, 0.5f);
    }
}
