using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting {
    /// <summary>
    /// An abstract class to be inherited by any class that wants to listen for changes in linked indices.
    /// </summary>
    /// <remarks>
    /// Inheriting from MonoBehaviour forces derived classes to be treated as components in the 
    /// Unity inspector, so that they can then be directly dragged and dropped to a <see cref="LinkedIndicesGroup"/>
    /// listeners array.
    /// </remarks>
    public abstract class LinkedIndicesListener : MonoBehaviour
    {
        /// <summary>
        /// Notify listener of specific linked index change. 
        /// </summary>
        /// <remarks>
        /// Called in LateUpdate for each specific linked index that was changed in that frame.
        /// </remarks>
        /// <param name="index">Index into linked indices array.</param>
        /// <param name="linkedAttributes">State of current linked index attributes.</param>
        public virtual void LinkedIndexChanged(int index, LinkedIndices.LinkedAttributes linkedAttributes) { }
        /// <summary>
        /// Notify listener that any linked indices have changed.
        /// </summary>
        /// <remarks>
        /// Called once in LateUpdate after all individual <see cref="LinkedIndicesListener.LinkedIndexChanged(int, LinkedIndices.LinkedAttributes)"/> calls have been made,
        /// only if any linked indices were changed.
        /// 
        /// Since this method is guaranteed to be called after all <see cref="LinkedIndicesListener.LinkedIndexChanged(int, LinkedIndices.LinkedAttributes)"/>
        /// calls, it can also be used as a notification that changes can now be finalized (i.e. we can now call <see cref="DataPlot.RefreshPlotGraphics"/>).
        /// </remarks>
        public virtual void LinkedIndicesChanged() { }
        /// <summary>
        /// Notify listener that a new linked indices has been set.
        /// </summary>
        public virtual void NewLinkedIndicesSet(LinkedIndices newLinkedIndices) { }
    }
}
