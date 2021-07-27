using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// A rectangular <see cref="SelectionMode"/> that generates a selection rectangle
    /// and asks the current <see cref="DataPlot"/> it is working on to select any data
    /// points that lie within it.
    /// </summary>
    public class RectSelectionMode : SelectionMode
    {
        [Header("Rect Selection Mode Dependencies")]
        /// <summary> Transform of the rectangle used to visualize the selection. </summary>
        [SerializeField] private RectTransform selectionRect;
        /// <summary> Selection rectangle's boundaries (used to give it a visual outline). </summary>
        [SerializeField] private RectTransform rightBoundary, leftBoundary, topBoundary, bottomBoundary;
        /// <summary> Parent object that stores selection graphics when they are not in use. </summary>
        [SerializeField] protected Transform selectionGraphicsParent;
        /// <summary> Start position of the selection. </summary>
        private Vector2 selectionStartPos;
        /// <summary> Current selection position. </summary>
        private Vector2 curSelectionPos;

        /// <summary>
        /// Set reference to the data plot this selection is now acting in, then
        /// reset, activate, and determine the starting position of the selection rectangle.
        /// </summary>
        /// <param name="dataPlot">Data plot the selection is now acting on.</param>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public override void StartSelection(DataPlot dataPlot, Vector2 mousePosition)
        {
            // Establish references to the current data plot
            currentDataPlot = dataPlot;
            currentPlotRect = dataPlot.PlotOuterRect;
            // Position the selection rectangle under the mask of that plot
            selectionRect.SetParent(dataPlot.SelectionGraphicsRect);
            // Reset the selection rectangle
            selectionRect.sizeDelta = Vector2.zero;
            rightBoundary.sizeDelta = Vector2.zero;
            leftBoundary.sizeDelta = Vector2.zero;
            topBoundary.sizeDelta = Vector2.zero;
            bottomBoundary.sizeDelta = Vector2.zero;
            // Activate the selection rectangle
            selectionRect.gameObject.SetActive(true);
            // Determine starting position in canvas/rect space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                currentPlotRect,
                mousePosition,
                plotsCamera,
                out selectionStartPos
            );
            // Let the current data plot handle actual selection of data points based on the 
            // current selection rectangle
            currentDataPlot.RectSelection(selectionRect);
        }

        /// <summary>
        /// Update the size of the selection rectangle and let the current plot select anything
        /// within it.
        /// </summary>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public override void UpdateSelection(Vector2 mousePosition)
        {
            // Determine the current mouse position in canvas/rect space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                currentPlotRect,
                mousePosition,
                plotsCamera,
                out curSelectionPos
            );
            // Update the width/height/position of the selection rectangle
            float width = curSelectionPos.x - selectionStartPos.x;
            float height = curSelectionPos.y - selectionStartPos.y;
            selectionRect.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
            rightBoundary.sizeDelta = new Vector2(1, Mathf.Abs(height));
            leftBoundary.sizeDelta = new Vector2(1, Mathf.Abs(height));
            topBoundary.sizeDelta = new Vector2(Mathf.Abs(width), 1);
            bottomBoundary.sizeDelta = new Vector2(Mathf.Abs(width), 1);
            selectionRect.anchoredPosition = selectionStartPos + new Vector2(width / 2, height / 2);
            // Let the current data plot handle actual selection of data points based on the 
            // current selection rectangle
            currentDataPlot.RectSelection(selectionRect);
        }

        /// <summary>
        /// Finalize the selection by updating it one last time and then
        /// deactivating the selection rectangle.
        /// </summary>
        /// <param name="mousePosition">Current mouse position in pixel coordinates (as from Input.mousePosition).</param>
        public override void EndSelection(Vector2 mousePosition)
        {
            // Update the selection
            UpdateSelection(mousePosition);
            // Deactivate the selection rectangle
            selectionRect.gameObject.SetActive(false);
            // Reset the selection rectangle's parent so that it is no longer attached to a data plot
            // (and therefore cannot be accidentally deleted with the plot)
            selectionRect.SetParent(selectionGraphicsParent);
        }
    }
}
