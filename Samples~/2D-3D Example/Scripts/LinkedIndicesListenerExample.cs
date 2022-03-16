using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// An example class for how one might implement <see cref="LinkedData"/>.
    /// </summary>
    public class LinkedIndicesListenerExample : LinkedIndicesListener
    {
        [SerializeField] private GameObject[] dataObjects;
        [SerializeField] private Material defaultMat, highlightedMat;

        private MeshRenderer[] renderers;

        private void Awake()
        {
            // Initialize references to the dataObjects' MeshRenderers
            renderers = new MeshRenderer[dataObjects.Length];
            for (int i = 0; i < dataObjects.Length; i++)
            {
                renderers[i] = dataObjects[i].GetComponent<MeshRenderer>();
            }
        }

        // Overrides the UpdateDataPoint() method inherited from LinkedData.
        public override void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes linkedAttributes)
        {
            // Perform different actions according to the current state of linked indices

            // If this data point is masked . . .
            if (linkedAttributes.Masked)
            {
                renderers[index].enabled = false;
            }
            // If this data point is highlighted . . .
            else if (linkedAttributes.Highlighted)
            {
                renderers[index].enabled = true;
                renderers[index].material = highlightedMat;
            }
            // Otherwise . . .
            else
            {
                renderers[index].enabled = true;
                renderers[index].material = defaultMat;
            }
        }
    }
}

