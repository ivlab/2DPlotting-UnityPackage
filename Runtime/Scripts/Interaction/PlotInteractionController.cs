using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// Controls selection for all <see cref="DataPlot"/> managed by a list of <see cref="DataPlotGroup"/>.
    /// </summary>
    public class PlotInteractionController : MonoBehaviour
    {
        [Header("Selection Mode")]
        /// <summary> Current selection mode being used to select data points. </summary>
        [SerializeField] private SelectionMode currentSelectionMode;
        [Header("Dependencies")]
        /// <summary> Camera attached to the screen space canvas plots are children of. </summary>
        [SerializeField] private Camera plotsCamera;

        private DataPlotGroup activeDataPlotManager;
        /// <summary> Allows selection to be enabled and disabled. </summary>
        private bool selectionEnabled = true;
        /// <summary> Allows only valid selections to be started. </summary>
        private bool validSelection;

        /// <summary> Data plot manager currently in focus. </summary>
        public DataPlotGroup ActiveDataPlotManager { get => activeDataPlotManager; set => activeDataPlotManager = value; }

        void Update()
        {
            // Toggle masking when the space bar is pressed
            if (Input.GetKeyDown(activeDataPlotManager.MaskingMode.ToggleKey))
            {
                activeDataPlotManager.MaskingMode.ToggleMasking();
            }

            // Selection mode mouse interaction:
            // 1. Start the selection (mouse pressed down)
            if (Input.GetMouseButtonDown(0))
            {
                // Only try to start a selection if selection is enabled
                if (!selectionEnabled)
                {
                    validSelection = false;
                }
                // Otherwise only allow selection to begin if a plot's selection rect is actually clicked on
                else
                {
                    Vector2 mousePosition = Input.mousePosition;
                    foreach (DataPlot dataPlot in activeDataPlotManager.DataPlots)
                    {
                        validSelection = RectTransformUtility.RectangleContainsScreenPoint(
                            dataPlot.PlotSelectionRect,
                            mousePosition,
                            plotsCamera
                        );
                        // Start the selection if it is valid
                        if (validSelection)
                        {
                            currentSelectionMode.StartSelection(dataPlot, mousePosition);
                            break;
                        }
                    }
                }
            }
            // 2. Update the selection while mouse held down (if it was valid)
            else if (validSelection && Input.GetMouseButton(0))
            {
                currentSelectionMode.UpdateSelection(Input.mousePosition);
            }
            // 3. End the selection on mouse up (if it was valid)
            else if (validSelection && Input.GetMouseButtonUp(0))
            {
                currentSelectionMode.EndSelection(Input.mousePosition);

                // Check if anything was selected
                activeDataPlotManager.CheckAnySelected();

                validSelection = false;
            }
        }

        /// <summary>
        /// Sets the current selection mode.
        /// </summary>
        public void SetCurrentSelectionMode(SelectionMode selectionMode) { currentSelectionMode = selectionMode; }
        /// <summary>
        /// Enables selection so that clicking the mouse once again starts a selection.
        /// </summary>
        public void EnableSelection() { selectionEnabled = true; }
        /// <summary>
        /// Disables selection so that clicking the mouse has no effect.
        /// </summary>
        public void DisableSelection() { selectionEnabled = false; }
    }
}
