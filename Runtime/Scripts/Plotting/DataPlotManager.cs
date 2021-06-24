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
        [Header("Data Table Control")]
        /// <summary> Name of csv file to pull data from. </summary>
        [SerializeField] private string csvFilename;
        /// <summary> If enabled, a random table will be generated instead of one using the provided csv. </summary>
        [SerializeField] private bool useRandomTable;
        /// <summary> Number of data points to generate random table with. </summary>
        [SerializeField] private int randomTableHeight;

        [Header("Additional Linked Data")]
        /// <summary> List of additional linked data that will be updated along with the plots. </summary>
        [SerializeField] private List<LinkedData> linkedData;

        [Header("Selection Mode")]
        /// <summary> Current selection mode being used to select data points. </summary>
        [SerializeField] private SelectionMode curSelectionMode;

        [Header("Dependencies")]
        /// <summary> Camera attached to the screen space canvas plots are children of. </summary>
        [SerializeField] private Camera plotsCamera;
        /// <summary> Screen space canvas plots are children of. </summary>
        [SerializeField] private Canvas plotsCanvas;
        /// <summary> Parent gameobject of the "new plot from selected" buttons. Used to toggle them on/off. </summary>
        [SerializeField] private GameObject newFromSelectedParent;
        /// <summary> Allows selection to be enabled and disabled. </summary>
        private bool selectionEnabled = true;
        /// <summary> Allows only valid selections to be started. </summary>
        private bool validSelection;
        /// <summary> Toggles whether or not unhighlighted data should be masked. </summary>
        private bool masking = false;
        /// <summary> Data table all plots managed by this class use. </summary>
        private DataTable dataTable;
        /// <summary> Collection of "data point" indices, linked with other key attributes. </summary>
        private LinkedIndices linkedIndices;
        /// <summary> Collection of plots that this class manages. </summary>
        private List<DataPlot> dataPlots;

        // Accesors
        public DataTable DataTable { get => dataTable; }
        public LinkedIndices LinkedIndices { get => linkedIndices; }
        public List<DataPlot> DataPlots { get => dataPlots; }

        // Initializtion
        void Awake()
        {
            // Initialize the data table all plots controlled by this data manager will use
            dataTable = useRandomTable ? new DataTable(randomTableHeight) : new DataTable(csvFilename);
            if (dataTable.Empty())
            {
                Debug.LogError("Data table is empty.");
            }
            // Initialize the linked indices array based on number of data points (table height)
            linkedIndices = new LinkedIndices(dataTable.Height);
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
                            masking = false;
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
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (linkedIndices[i].Highlighted)
                    {
                        newFromSelectedParent.SetActive(true);
                        break;
                    }
                    else
                    {
                        newFromSelectedParent.SetActive(false);
                    }
                }
            }

            // Toggle masking when the spacebar is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleMasking();
            }

            // After performing the current selection operations for all the plots,
            // update them to match these most recent changes
            UpdatePlots();
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
        /// Disables selection so that clickling the mouse has no effect.
        /// </summary>
        public void DisableSelection() { selectionEnabled = false; }

        /// <summary>
        /// Hardcoded (aka bad) template for arranging 1-4 plots.
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
            if (dataPlots.Count > 4)
                return;

            // Determine which data point indices are currently selected
            List<int> selectedIndices = new List<int>();
            for (int i = 0; i < linkedIndices.Size; i++)
            {
                if (linkedIndices[i].Highlighted)
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

            // Rearrage the plots
            ArrangePlots();
        }

        /// <summary>
        /// Adds a new plot.
        /// </summary>
        /// <param name="dataPlotPrefab">Prefab GameObject containing the data plot.</param>
        public void AddPlot(GameObject dataPlotPrefab)
        {
            if (dataPlots.Count > 4)
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

        /// <summary>
        /// Toggles masking.
        /// </summary>
        public void ToggleMasking()
        {
            masking = !masking;
            if (masking)
            {
                int unhighlightedCount = 0;
                // Mask all unhighlighted particles
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (!linkedIndices[i].Highlighted)
                    {
                        linkedIndices[i].Masked = true;
                        unhighlightedCount++;
                    }
                }
                // Unmask the particles if all of them were unhighlighted
                if (unhighlightedCount == linkedIndices.Size)
                {
                    for (int i = 0; i < linkedIndices.Size; i++)
                    {
                        linkedIndices[i].Masked = false;
                    }
                }
            }
            else
            {
                // Unmask all currently masked particles
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (linkedIndices[i].Masked)
                    {
                        linkedIndices[i].Masked = false;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the plots to match the most recent selection, highlighting, and filtering.
        /// </summary>
        public void UpdatePlots()
        {
            // Only update plots if a data point's linked index attribute has been changed
            if (linkedIndices.LinkedAttributesChanged)
            {
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    // Only update the data points that have been changed
                    if (linkedIndices[i].LinkedAttributeChanged)
                    {
                        // Update changed data points in all plots
                        for (int j = 0; j < dataPlots.Count; j++)
                        {
                            dataPlots[j].UpdateDataPoint(i, linkedIndices[i]);
                        }
                        // Update any other linked data
                        for (int k = 0; k < linkedData.Count; k++)
                        {
                            linkedData[k].UpdateDataPoint(i, linkedIndices[i]);
                        }

                        linkedIndices[i].LinkedAttributeChanged = false;
                    }
                }

                // Update the graphics on all plots to reflect most recent changes
                for (int j = 0; j < dataPlots.Count; j++)
                {
                    dataPlots[j].RefreshPlotGraphics();
                }

                // Reset the linked attributes changed flag
                linkedIndices.LinkedAttributesChanged = false;
            }
        }

        /// <summary>
        /// Prints the IDs of all the selected data.
        /// </summary>
        private void PrintSelectedDataIDs()
        {
            string selectedIDs = "Selected Data Points (ID):\n\n";
            for (int i = 0; i < linkedIndices.Size; i++)
            {
                if (linkedIndices[i].Highlighted)
                {
                    selectedIDs += dataTable.RowIDs[i] + "\n";
                }
            }
            print(selectedIDs);
        }
    }
}
