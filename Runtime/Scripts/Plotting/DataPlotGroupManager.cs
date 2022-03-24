using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

namespace IVLab.Plotting
{
    /// <summary>
    /// Manages data plot groups, allowing for multiple data plot groups to exist at the same time.
    /// </summary>
    public class DataPlotGroupManager : MonoBehaviour
    {
        /// <summary> Data plot groups this manages. </summary>
        [SerializeField] private List<DataPlotGroup> dataPlotGroups = new List<DataPlotGroup>();
        [Header("Interaction")]
        /// <summary> Current selection mode being used to select data points. </summary>
        [SerializeField] private SelectionMode currentSelectionMode;
        [Header("Callbacks")]
        /// <summary> Callback for when a new data source is added. </summary>
        [SerializeField] private UnityEvent onDataSourceAdded;
        [Header("Dependencies")]
        /// <summary> Camera attached to the screen space canvas plots are children of. </summary>
        [SerializeField] private Camera plotsCamera;
        
        /// <summary> Data plot group currently experiencing selection. </summary>
        private DataPlotGroup activeSelectionDataPlotGroup;
        /// <summary> Allows selection to be enabled and disabled. </summary>
        private bool selectionEnabled = true;
        /// <summary> Allows only valid selections to be started. </summary>
        private bool validSelection;

        // Initialization
        void Start()
        {
            // Initialize any data plot groups that have already been added in the inspector
            foreach (DataPlotGroup dataPlotGroup in dataPlotGroups)
            {
                dataPlotGroup.Init();
                dataPlotGroup.Show();
            }
        }

        void Update()
        {
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
                    foreach (DataPlotGroup dataPlotGroup in dataPlotGroups)
                    {
                        if (dataPlotGroup.Shown)
                        {
                            foreach (DataPlot dataPlot in dataPlotGroup.DataPlots)
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
                                    activeSelectionDataPlotGroup = dataPlotGroup;
                                    break;
                                }
                            }
                            if (validSelection) break;
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
