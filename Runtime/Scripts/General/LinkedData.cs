using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting {
    /// <summary>
    /// Provides an interface for any script that wants to be able to update data
    /// in sync with the data plots.
    /// </summary>
    public abstract class LinkedData : MonoBehaviour
    {
        public abstract void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes linkedAttributes);
    }
}
