using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting 
{
    [CreateAssetMenu(menuName = "Plotting/Data Plots/Scatter Plot Skin")]
    public class ScatterPlotSkin : DataPlotSkin
    {
        [Header("Scatter Plot Settings")]
        public float pointSize = 3;
    }
}
