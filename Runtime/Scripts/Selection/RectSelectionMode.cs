using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    public class RectSelectionMode : SelectionMode
    {
        [Header("Rect Selection Mode Dependencies")]
        [SerializeField] private RectTransform selectionRect;  // Rect used to visualize the selection
        [SerializeField] private RectTransform rightBoundary, leftBoundary, topBoundary, bottomBoundary;  // Selection rect's boundaries
        [SerializeField] protected Transform selectionGraphicsParent;  // Parent object to store selection graphics under
        private Vector2 selectionStartPos;
        private Vector2 curSelectionPos;

        // Set references to the data plot this selection is currently acting in, then
        // reset, activate, and determine the starting position of the selection rectangle.
        public override void StartSelection(DataPlot dataPlot, Vector2 mousePosition)
        {
            // Establish references to the current data plot
            currentDataPlot = dataPlot;
            currentPlotRect = dataPlot.PlotOuterRect;
            // Position the selection rect under the mask of that plot
            selectionRect.SetParent(dataPlot.PlotMask);
            // Reset the selection rect
            selectionRect.sizeDelta = Vector2.zero;
            rightBoundary.sizeDelta = Vector2.zero;
            leftBoundary.sizeDelta = Vector2.zero;
            topBoundary.sizeDelta = Vector2.zero;
            bottomBoundary.sizeDelta = Vector2.zero;
            // Activate the selection rect
            selectionRect.gameObject.SetActive(true);
            // Determine starting position in canvas/rect space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                currentPlotRect,
                mousePosition,
                plotsCamera,
                out selectionStartPos
            );
            // Let the current data plot handle actual selection of data points based on the 
            // current selection rect
            currentDataPlot.RectSelection(selectionRect);
        }

        // Update the size of the selection rectangle and let the current plot selection anything
        // within it.
        public override void UpdateSelection(Vector2 mousePosition)
        {
            // Determine the current mouse position in canvas/rect space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                currentPlotRect,
                mousePosition,
                plotsCamera,
                out curSelectionPos
            );
            // Update the width/height/position of the selection rect
            float width = curSelectionPos.x - selectionStartPos.x;
            float height = curSelectionPos.y - selectionStartPos.y;
            selectionRect.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
            rightBoundary.sizeDelta = new Vector2(1, Mathf.Abs(height));
            leftBoundary.sizeDelta = new Vector2(1, Mathf.Abs(height));
            topBoundary.sizeDelta = new Vector2(Mathf.Abs(width), 1);
            bottomBoundary.sizeDelta = new Vector2(Mathf.Abs(width), 1);
            selectionRect.anchoredPosition = selectionStartPos + new Vector2(width / 2, height / 2);
            // Let the current data plot handle actual selection of data points based on the 
            // current selection rect
            currentDataPlot.RectSelection(selectionRect);
        }

        // Finalize the selection by updating the selection rect one last time and then
        // deactivating it.
        public override void EndSelection(Vector2 mousePosition)
        {
            UpdateSelection(mousePosition);
            // Deactivate the selection rect
            selectionRect.gameObject.SetActive(false);
            // Reset the selection rect's parent so that it is no longer attached to a data plot
            // (and therefore cannot be accidentally deleted with the plot)
            selectionRect.SetParent(selectionGraphicsParent);
        }
    }
}
