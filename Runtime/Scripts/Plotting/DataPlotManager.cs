using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// Manages the visualization and control of multiple <see cref="DataPlot"/> objects simultaneously.
    /// </summary>
    public class DataPlotManager : MonoBehaviour
    {
        [Header("Selection Mode")]
        /// <summary> Current selection mode being used to select data points. </summary>
        [SerializeField] private SelectionMode curSelectionMode;

        [Header("Dependencies")]
        /// <summary> Camera attached to the screen space canvas plots are children of. </summary>
        [SerializeField] private Camera plotsCamera;
        /// <summary> Screen space canvas plots are children of. </summary>
        [SerializeField] private Canvas plotsCanvas;
        /// <summary> Parent GameObject of the "new plot from selected" buttons. Used to toggle them on/off. </summary>
        [SerializeField] private GameObject newFromSelectedParent;
        /// <summary> Allows selection to be enabled and disabled. </summary>
        private bool selectionEnabled = true;
        /// <summary> Allows only valid selections to be started. </summary>
        private bool validSelection;
        private List<DataPlot> dataPlots;
        private DataManager dataManager;

        /// <summary> Collection of plots that this class manages. </summary>
        public List<DataPlot> DataPlots { get => dataPlots; }
        /// <summary> Data manager that manages this data plot manager's data,
        /// i.e. provides the DataTable and LinkedIndices. </summary>
        public DataManager DataManager { get => dataManager; set => dataManager = value; }

        // Self-initialization
        void Awake()
        {
            // Initialize an empty list of managed data plots
            dataPlots = new List<DataPlot>();
        }

        // Update
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
                    for (int i = 0; i < dataPlots.Count; i++)
                    {
                        validSelection = RectTransformUtility.RectangleContainsScreenPoint(
                            dataPlots[i].PlotSelectionRect,
                            mousePosition,
                            plotsCamera
                        );
                        // Start the selection if it is valid
                        if (validSelection)
                        {
                            // Toggle masking to false so that selection within a selection is more natural
                            dataManager.Masking = false;
                            curSelectionMode.StartSelection(dataPlots[i], mousePosition);
                            break;
                        }
                    }
                }
            }
            // 2. Update the selection while mouse held down (if it was valid)
            else if (validSelection && Input.GetMouseButton(0))
            {
                curSelectionMode.UpdateSelection(Input.mousePosition);
            }
            // 3. End the selection on mouse up (if it was valid)
            else if (validSelection && Input.GetMouseButtonUp(0))
            {
                curSelectionMode.EndSelection(Input.mousePosition);

                // Enable/disable "new plot from selected" buttons depending on whether or not anything has been selected
                for (int i = 0; i < dataManager.LinkedIndices.Size; i++)
                {
                    if (dataManager.LinkedIndices[i].Highlighted)
                    {
                        newFromSelectedParent.SetActive(true);
                        break;
                    }
                    else
                    {
                        newFromSelectedParent.SetActive(false);
                    }
                }

                validSelection = false;
            }
        }

        /// <summary>
        /// Sets the current selection mode used by this plot manager.
        /// </summary>
        public void SetCurrentSelectionMode(SelectionMode selectionMode) { curSelectionMode = selectionMode; }
        /// <summary>
        /// Enables selection so that clicking the mouse once again starts a selection.
        /// </summary>
        public void EnableSelection() { selectionEnabled = true; }
        /// <summary>
        /// Disables selection so that clicking the mouse has no effect.
        /// </summary>
        public void DisableSelection() { selectionEnabled = false; }

        /// <summary>
        /// Hard-coded (aka bad) template for arranging 1-4 plots.
        /// </summary>
        private void ArrangePlots()
        {
            if (dataPlots.Count == 1)
            {
                Vector2 position = new Vector2(-25, 0);
                Vector2 outerBounds = plotsCanvas.GetComponent<RectTransform>().sizeDelta - new Vector2(100, 50);
                dataPlots[0].transform.localPosition = position;
                dataPlots[0].ResizePlot(outerBounds);
                dataPlots[0].Plot();
            }
            else if (dataPlots.Count == 2)
            {
                Vector2 position1 = new Vector2(-25, plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 4);
                Vector2 position2 = new Vector2(-25, -plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 4);
                Vector2 outerBounds = new Vector2(plotsCanvas.GetComponent<RectTransform>().sizeDelta.x - 100, plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 2 - 50);

                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].ResizePlot(outerBounds);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].ResizePlot(outerBounds);
                dataPlots[1].Plot();
            }
            else if (dataPlots.Count == 3)
            {
                Vector2 position1 = new Vector2(-25, plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 4);
                Vector2 position2 = new Vector2(-25 - plotsCanvas.GetComponent<RectTransform>().sizeDelta.x / 4 + 15, -plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 4);
                Vector2 position3 = new Vector2(-25 + plotsCanvas.GetComponent<RectTransform>().sizeDelta.x / 4 - 15, -plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 4);
                Vector2 outerBounds1 = new Vector2(plotsCanvas.GetComponent<RectTransform>().sizeDelta.x - 100, plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 2 - 50);
                Vector2 outerBounds23 = new Vector2(plotsCanvas.GetComponent<RectTransform>().sizeDelta.x / 2 - 70, plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 2 - 50);

                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].ResizePlot(outerBounds1);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].ResizePlot(outerBounds23);
                dataPlots[1].Plot();

                dataPlots[2].transform.localPosition = position3;
                dataPlots[2].ResizePlot(outerBounds23);
                dataPlots[2].Plot();
            }
            else if (dataPlots.Count == 4)
            {
                Vector2 position1 = new Vector2(-25 - plotsCanvas.GetComponent<RectTransform>().sizeDelta.x / 4 + 15, +plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 4);
                Vector2 position2 = new Vector2(-25 + plotsCanvas.GetComponent<RectTransform>().sizeDelta.x / 4 - 15, +plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 4);
                Vector2 position3 = new Vector2(-25 - plotsCanvas.GetComponent<RectTransform>().sizeDelta.x / 4 + 15, -plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 4);
                Vector2 position4 = new Vector2(-25 + plotsCanvas.GetComponent<RectTransform>().sizeDelta.x / 4 - 15, -plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 4);
                Vector2 outerBounds = new Vector2(plotsCanvas.GetComponent<RectTransform>().sizeDelta.x / 2 - 70, plotsCanvas.GetComponent<RectTransform>().sizeDelta.y / 2 - 50);

                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].ResizePlot(outerBounds);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].ResizePlot(outerBounds);
                dataPlots[1].Plot();

                dataPlots[2].transform.localPosition = position3;
                dataPlots[2].ResizePlot(outerBounds);
                dataPlots[2].Plot();

                dataPlots[3].transform.localPosition = position4;
                dataPlots[3].ResizePlot(outerBounds);
                dataPlots[3].Plot();
            }
        }

        /// <summary>
        /// Adds a new plot using only the currently selected data points.
        /// </summary>
        /// <param name="dataPlotPrefab">Prefab GameObject containing the data plot.</param>
        public void AddPlotFromSelected(GameObject dataPlotPrefab)
        {
            if (dataPlots.Count >= 4)
                return;

            // Determine which data point indices are currently selected
            List<int> selectedIndices = new List<int>();
            for (int i = 0; i < dataManager.LinkedIndices.Size; i++)
            {
                if (dataManager.LinkedIndices[i].Highlighted)
                {
                    selectedIndices.Add(i);
                }
            }
            // Instantiate a clone of the plot given by the prefab
            GameObject dataPlot = Instantiate(dataPlotPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            // Attach it to the canvas and reset its scale
            dataPlot.transform.SetParent(plotsCanvas.transform);
            dataPlot.transform.localScale = Vector3.one;
            // Initialize and plot the data plot using its attached script
            DataPlot dataPlotScript = dataPlot.GetComponent<DataPlot>();
            dataPlotScript.Init(this, Vector2.one * 500, selectedIndices.ToArray());
            // Add this script to the list of data plot scripts this manager manages
            dataPlots.Add(dataPlotScript);

            // Rearrange the plots
            ArrangePlots();
        }

        /// <summary>
        /// Adds a new plot.
        /// </summary>
        /// <param name="dataPlotPrefab">Prefab GameObject containing the data plot.</param>
        public void AddPlot(GameObject dataPlotPrefab)
        {
            if (dataPlots.Count >= 4)
                return;

            // Instantiate a clone of the plot given by the prefab
            GameObject dataPlot = Instantiate(dataPlotPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            // Attach it to the canvas and reset its scale
            dataPlot.transform.SetParent(plotsCanvas.transform);
            dataPlot.transform.localScale = Vector3.one;
            // Initialize and plot the data plot using its attached script
            DataPlot dataPlotScript = dataPlot.GetComponent<DataPlot>();
            dataPlotScript.Init(this, Vector2.one * 500);
            // Add this script to the list of data plot scripts this manager manages
            dataPlots.Add(dataPlotScript);

            ArrangePlots();
        }

        /// <summary>
        /// Removes and destroys the specified plot, if it is being managed by this class.
        /// </summary>
        /// <param name="dataPlot">Script attached to the data plot GameObject that we wish to remove.</param>
        public void RemovePlot(DataPlot dataPlot)
        {
            if (dataPlots.Contains(dataPlot)) {
                dataPlots.Remove(dataPlot);
                Destroy(dataPlot.gameObject);

                ArrangePlots();
            }
        }
    }
}
