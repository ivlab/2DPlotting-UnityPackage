using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// A brush-like <see cref="SelectionMode"/> that asks the current <see cref="DataPlot"/> it
    /// is working on to select any data points that have been brushed over since it was last updated.
    /// </summary>
    public class BrushSelectionMode : SelectionMode
    {
        [Header("Brush Selection Mode Dependencies")]
        /// <summary> Transform of the selection brush GameObject. </summary>
        [SerializeField] private RectTransform selectionBrush;
        /// <summary> Parent object that stores selection graphics when they are not in use. </summary>
        [SerializeField] protected Transform selectionGraphicsParent;
        /// <summary> Tracks the brush position from the previous update. </summary>
        private Vector2 prevBrushPosition;
        /// <summary> Tracks the brush position from the current update. </summary>
        private Vector2 curBrushPosition;
        /// <summary> Current selection state the brush is in. </summary>
        private SelectionMode.State selectionState;

        /// <summary>
        /// Set reference to the data plot this selection is now acting in, then
        /// reset, activate, and determine the starting position of the selection brush
        /// before finally calling the current data plot's method to handle brush
        /// selection interaction.
        /// </summary>
        /// <param name="dataPlot">Data plot the selection is now acting on.</param>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public override void StartSelection(DataPlot dataPlot, Vector2 mousePosition)
        {
            // Selection is starting
            selectionState = SelectionMode.State.Start;
            // Establish references to the current data plot
            currentDataPlot = dataPlot;
            currentPlotRect = dataPlot.PlotOuterRect;
            // Position the selection brush under the mask of that plot
            selectionBrush.SetParent(dataPlot.PlotMask);
            // Get the position of the brush in canvas/rect space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                currentPlotRect,
                mousePosition,
                plotsCamera,
                out prevBrushPosition
            );
            // Reset and activate the brush
            selectionBrush.sizeDelta = Vector2.one * dataPlot.BrushRadius * 2;
            selectionBrush.gameObject.SetActive(true);
            // Let the current data plot handle actual selection of data points by the brush
            currentDataPlot.BrushSelection(prevBrushPosition, Vector2.zero, selectionState);
        }

        /// <summary>
        /// Update the current selection using the distance that the brush has traveled
        /// since it was last updated.
        /// </summary>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public override void UpdateSelection(Vector2 mousePosition)
        {
            // Set the current position of the brush in canvas/rect space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                currentPlotRect,
                mousePosition,
                plotsCamera,
                out curBrushPosition
            );
            selectionBrush.anchoredPosition = curBrushPosition;
            // Get the vector representing the direction and distance the brush has moved since previous update
            Vector2 brushDelta = curBrushPosition - prevBrushPosition;
            // If the mouse hasn't moved, don't update the selection
            if (selectionState == SelectionMode.State.End)
            {
                currentDataPlot.BrushSelection(prevBrushPosition, brushDelta, selectionState);
            }
            else if (!brushDelta.Equals(Vector2.zero))
            {
                // Selection is updating 
                selectionState = SelectionMode.State.Update;
                // Let the current data plot handle the actual selection based on the movement 
                // of the brush since previous update
                currentDataPlot.BrushSelection(prevBrushPosition, brushDelta, selectionState);
                // Update previous brush position
                prevBrushPosition = curBrushPosition;
            }
        }

        /// <summary>
        /// Finalize the selection by updating it one last time and
        /// then deactivating the selection brush.
        /// </summary>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public override void EndSelection(Vector2 mousePosition)
        {
            // Selection is ending
            selectionState = SelectionMode.State.End;
            // Update the selection brush one last time
            UpdateSelection(mousePosition);
            // Deactivate the brush
            selectionBrush.gameObject.SetActive(false);
            // Reset the selection brush's parent so that it is no longer attached to a data plot
            // (and therefore cannot be accidentally deleted with the plot)
            selectionBrush.SetParent(selectionGraphicsParent);
        }
    }
}
