using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    public class BrushSelectionMode : SelectionMode
    {
        [Header("Brush Selection Mode Dependencies")]
        [SerializeField] private RectTransform selectionBrush;  // Transform of selection brush gameobject
        [SerializeField] protected Transform selectionGraphicsParent;  // Parent object to store selection graphics under
        private Vector2 prevBrushPosition;  // Tracks the brush position from the previous frame
        private Vector2 curBrushPosition;  // Tracks the brush position from the current frame
        private SelectionMode.State selectionState;

        // Set references to the data plot this selection is currently acting in, then
        // reset, activate, and determine the starting position of the selection brush
        // before calling the current data plot's method to handle interaction
        public override void StartSelection(DataPlot dataPlot, Vector2 mousePosition)
        {
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

        // Update the current selection using the distance that the brush has traveled
        // since previous update.
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
                selectionState = SelectionMode.State.Update;
                // Let the current data plot handle the actual selection based on the movement 
                // of the brush since previous update
                currentDataPlot.BrushSelection(prevBrushPosition, brushDelta, selectionState);
                // Update previous brush position
                prevBrushPosition = curBrushPosition;
            }
        }

        // Finalize the selection by updating the selection brush one last time
        // and then deactivating it.
        public override void EndSelection(Vector2 mousePosition)
        {
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
