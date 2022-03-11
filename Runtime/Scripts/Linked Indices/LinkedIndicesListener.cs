using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting {
    /// <summary>
    /// Interface for any class that contains data points which need to be updated
    /// based on the current linked index space.
    /// </summary>
    public interface ILinkedIndicesListener
    {
        void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes linkedAttributes);
    }

    /// <summary>
    /// Provides definitions for any script that wants to be able to update data
    /// in sync with the index space.
    /// </summary>
    public abstract class LinkedData : MonoBehaviour, ILinkedIndicesListener
    {
        public abstract void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes linkedAttributes);
    }
}
