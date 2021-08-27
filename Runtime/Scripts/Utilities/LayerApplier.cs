using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// Ensures layers are properly applied throughout the
    /// scene on awake.
    /// </summary>
    public class LayerApplier : MonoBehaviour
    {
        [SerializeField] private Camera cam2D;
        [SerializeField] private GameObject plottingParent;

        void Awake()
        {
            int plotsLayerID = LayerMask.NameToLayer(PlottingUtilities.Consts.PlotsLayerName);

            // Apply "plots" layer to camera
            cam2D.gameObject.layer = plotsLayerID;
            cam2D.cullingMask = 1 << plotsLayerID;

            // Apply "plots" layer to every object in the plotting hierarchy
            PlottingUtilities.ApplyPlotsLayersRecursive(plottingParent);
        }
    }
}
