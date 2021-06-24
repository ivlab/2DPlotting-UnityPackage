using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// Abstract class which declares variables and methods ubiquitous to all possible selection modes.
    /// </summary>
    public abstract class SelectionMode : MonoBehaviour
    {
        [Header("Ubiquitous Selection Mode Dependencies")]
        /// <summary> Camera attached to the screen space canvas plots are children of. </summary>
        [SerializeField] protected Camera plotsCamera;
        /// <summary> Data plot the selection is currently acting on. </summary>
        protected DataPlot currentDataPlot;
        /// <summary> Rect transform of the current data plot. </summary>
        protected RectTransform currentPlotRect;

        /// <summary>
        /// Grants the selection a reference to the data plot it is acting on, and begins performing the selection.
        /// </summary>
        /// <param name="dataPlot">Data plot the selection is now acting on.</param>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public abstract void StartSelection(DataPlot dataPlot, Vector2 mousePosition);

        /// <summary>
        /// Updates the selection based on mouse position.
        /// </summary>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public abstract void UpdateSelection(Vector2 mousePosition);

        /// <summary>
        /// Finalizes the selection based on mouse position.
        /// </summary>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public abstract void EndSelection(Vector2 mousePosition);

        /// <summary>
        /// Selection states.
        /// </summary>
        public enum State
        {
            /// <summary>Selection has just started. Could be used in <see cref="StartSelection(DataPlot, Vector2)"/>. </summary>
            Start,
            /// <summary>Selection is updating. Could be used in <see cref="UpdateSelection(Vector2)"/>. </summary>
            Update,
            /// <summary>Selection has just ended. Could be used in <see cref="EndSelection(Vector2)"/>. </summary>
            End
        }
    }
}
