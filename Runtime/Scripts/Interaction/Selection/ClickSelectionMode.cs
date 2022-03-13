using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// A click-based <see cref="SelectionMode"/> that asks the current <see cref="DataPlot"/> 
    /// it is working on to select the nearest data point that lies within the click position.
    /// </summary>
    public class ClickSelectionMode : SelectionMode
    {
        [Header("Click Selection Mode Dependencies")]
        /// <summary> Start position of the selection. </summary>
        private Vector2 selectionStartPos;
        /// <summary> Current selection position. </summary>
        private Vector2 curSelectionPos;

        /// <summary>
        /// Set references to the data plot this selection is currently acting in and begin the selection.
        /// </summary>
        /// <param name="dataPlot">Data plot the selection is now acting on.</param>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public override void StartSelection(DataPlot dataPlot, Vector2 mousePosition)
        {
            // Establish references to the current data plot
            currentDataPlot = dataPlot;
            currentPlotRect = dataPlot.PlotOuterRect;
            // Determine starting mouse position in canvas/rect space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                currentPlotRect,
                mousePosition,
                plotsCamera,
                out selectionStartPos
            );
            // Call the current data plot's click selection method and pass true to tell it
            // that this is the initial click
            currentDataPlot.ClickSelection(selectionStartPos, SelectionMode.State.Start);
        }

        /// <summary>
        /// Update the click selection by calling the current data plot's click selection method.
        /// </summary>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public override void UpdateSelection(Vector2 mousePosition)
        {
            // Determine current mouse position in canvas/rect space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                currentPlotRect,
                mousePosition,
                plotsCamera,
                out curSelectionPos
            );
            // Call the current data plot's click selection method and pass false to tell it
            // that this isn't the initial click
            currentDataPlot.ClickSelection(curSelectionPos, SelectionMode.State.Update);
        }

        /// <summary>
        /// End the click selection. This is the same as updating the selection.
        /// </summary>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public override void EndSelection(Vector2 mousePosition)
        {
            UpdateSelection(mousePosition);
        }
    }
}
