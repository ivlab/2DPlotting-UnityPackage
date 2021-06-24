using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    public class ClickSelectionMode : SelectionMode
    {
        [Header("Click Selection Mode Dependencies")]
        private Vector2 selectionStartPos;
        private Vector2 curSelectionPos;

        // Set references to the data plot this selection is currently acting in and begin the selection.
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

        // Update the click selection by calling the current data plot's click selection method.
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

        // End the click selection. This is the same as updating the selection.
        public override void EndSelection(Vector2 mousePosition)
        {
            UpdateSelection(mousePosition);
        }
    }
}
