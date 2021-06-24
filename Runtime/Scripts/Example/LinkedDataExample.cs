using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// An example class for how one might implement <see cref="LinkedData"/>.
    /// </summary>
    public class LinkedDataExample : LinkedData
    {
        [SerializeField] private GameObject[] dataObjects;
        [SerializeField] private Material defaultMat, highlightedMat;

        private MeshRenderer[] renderers;

        private void Awake()
        {
            renderers = new MeshRenderer[dataObjects.Length];
            for (int i = 0; i < dataObjects.Length; i++)
            {
                renderers[i] = dataObjects[i].GetComponent<MeshRenderer>();
            }
        }

        public override void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes linkedAttributes)
        {
            if (linkedAttributes.Masked)
            {
                renderers[index].enabled = false;
            }
            else if (linkedAttributes.Highlighted)
            {
                renderers[index].enabled = true;
                renderers[index].material = highlightedMat;
            }
            else
            {
                renderers[index].enabled = true;
                renderers[index].material = defaultMat;
            }
        }
    }
}
