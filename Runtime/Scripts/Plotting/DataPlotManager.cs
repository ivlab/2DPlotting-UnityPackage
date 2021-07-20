using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace IVLab.Plotting
{
    /// <summary>
    /// Manages the visualization and control of multiple <see cref="DataPlot"/> objects simultaneously.
    /// </summary>
    public class DataPlotManager : MonoBehaviour
    {
        [Header("Dependencies/Plot Stuff")]
        /// <summary> Camera attached to the screen space canvas plots are children of. </summary>
        [SerializeField] private Camera plotsCamera;
        /// <summary> Screen space canvas plots are children of. </summary>
        [SerializeField] private Canvas plotsCanvas;
        /// <summary> Rect that plots are placed within. </summary>
        [SerializeField] private RectTransform plotsRect;
        [Header("Dependencies/Buttons")]
        /// <summary> Parent GameObject of the "new plot from selected" buttons. Used to toggle them on/off. </summary>
        [SerializeField] private GameObject newFromSelectedParent;
        [SerializeField] private Button newScatterPlotButton, newParallelCoordsPlotButton, newClusterPlotButton,
            selectedScatterPlotButton, selectedParallelCoordsPlotButton, selectedClusterPlotButton;
        [Header("Dependencies/Prefabs")]
        [SerializeField] private GameObject scatterPlotPrefab;
        [SerializeField] private GameObject parallelCoordsPlotPrefab;
        [SerializeField] private GameObject clusterPlotPrefab;
        /// <summary> Current selection mode being used to select data points. </summary>
        private SelectionMode curSelectionMode;
        /// <summary> Allows selection to be enabled and disabled. </summary>
        private bool selectionEnabled = true;
        /// <summary> Allows only valid selections to be started. </summary>
        private bool validSelection;
        private List<DataPlot> dataPlots = new List<DataPlot>();
        private DataManager dataManager;
        private UnityAction createScatter;
        private UnityAction createParallelCoords;
        private UnityAction createCluster;
        private UnityAction createScatterFromSelected;
        private UnityAction createParallelCoordsFromSelected;
        private UnityAction createClusterFromSelected;

        /// <summary> Gets the parent transform of all of the plots managed by this manager. </summary>
        public Transform PlotsParent { get; private set; }
        /// <summary> Collection of plots that this class manages. </summary>
        public List<DataPlot> DataPlots { get => dataPlots; }
        /// <summary> Data manager that manages this data plot manager's data,
        /// i.e. provides the DataTable and LinkedIndices. </summary>
        public DataManager DataManager { get => dataManager; set => dataManager = value; }

        /// <summary>
        /// Initializes this plot manager by creating a parent object for all the plots
        /// it will control and initializing its plot creations callback.
        /// </summary>
        /// <remarks>
        /// <b>Must</b> be called after <see cref="DataManager.Init(MultiDataManager, DataPlotManager)"/> or <see cref="DataManager.Init(MultiDataManager, DataTable, DataPlotManager, List{LinkedData})"/>.
        /// </remarks>
        public void Init()
        {
            // Create a parent for all plots managed by this plot manager
            PlotsParent = new GameObject(dataManager.DataTable.Name + " Plots").transform;
            PlotsParent.SetParent(plotsCanvas.transform);
            PlotsParent.transform.localPosition = Vector3.zero;
            PlotsParent.transform.localScale = Vector3.one;

            // Initialize plot creation callbacks
            createScatter = () => { AddPlot(scatterPlotPrefab); };
            createParallelCoords = () => { AddPlot(parallelCoordsPlotPrefab); };
            createCluster = () => { AddPlot(clusterPlotPrefab); };
            createScatterFromSelected = () => { AddPlotFromSelected(scatterPlotPrefab); };
            createParallelCoordsFromSelected = () => { AddPlotFromSelected(parallelCoordsPlotPrefab); };
            createClusterFromSelected = () => { AddPlotFromSelected(clusterPlotPrefab); };
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
                CheckSelection();

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
                Vector2 outerBounds = plotsRect.rect.size - new Vector2(100, 50);
                dataPlots[0].transform.localPosition = position;
                dataPlots[0].ResizePlot(outerBounds);
                dataPlots[0].Plot();
            }
            else if (dataPlots.Count == 2)
            {
                Vector2 position1 = new Vector2(-25, plotsRect.rect.size.y / 4);
                Vector2 position2 = new Vector2(-25, -plotsRect.rect.size.y / 4);
                Vector2 outerBounds = new Vector2(plotsRect.rect.size.x - 100, plotsRect.rect.size.y / 2 - 50);

                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].ResizePlot(outerBounds);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].ResizePlot(outerBounds);
                dataPlots[1].Plot();
            }
            else if (dataPlots.Count == 3)
            {
                Vector2 position1 = new Vector2(-25, plotsRect.rect.size.y / 4);
                Vector2 position2 = new Vector2(-25 - plotsRect.rect.size.x / 4 + 15, -plotsRect.rect.size.y / 4);
                Vector2 position3 = new Vector2(-25 + plotsRect.rect.size.x / 4 - 15, -plotsRect.rect.size.y / 4);
                Vector2 outerBounds1 = new Vector2(plotsRect.rect.size.x - 100, plotsRect.rect.size.y / 2 - 50);
                Vector2 outerBounds23 = new Vector2(plotsRect.rect.size.x / 2 - 70, plotsRect.rect.size.y / 2 - 50);

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
                Vector2 position1 = new Vector2(-25 - plotsRect.rect.size.x / 4 + 15, +plotsRect.rect.size.y / 4);
                Vector2 position2 = new Vector2(-25 + plotsRect.rect.size.x / 4 - 15, +plotsRect.rect.size.y / 4);
                Vector2 position3 = new Vector2(-25 - plotsRect.rect.size.x / 4 + 15, -plotsRect.rect.size.y / 4);
                Vector2 position4 = new Vector2(-25 + plotsRect.rect.size.x / 4 - 15, -plotsRect.rect.size.y / 4);
                Vector2 outerBounds = new Vector2(plotsRect.GetComponent<RectTransform>().rect.size.x / 2 - 70, plotsRect.GetComponent<RectTransform>().rect.size.y / 2 - 50);

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
            dataPlot.transform.SetParent(PlotsParent);
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
            dataPlot.transform.SetParent(PlotsParent);
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
        /// Shows this data plot manager, all of its plot, and rewires the plot creation buttons.
        /// </summary>
        public void Show()
        {
            PlotsParent.gameObject.SetActive(true);
            gameObject.SetActive(true);

            CheckSelection();

            RewirePlotCreationButtons();
        }

        /// <summary>
        /// Hides this data plot manager, all of its plot, and unwires the plot creation buttons.
        /// </summary>
        public void Hide()
        {
            PlotsParent.gameObject.SetActive(false);
            gameObject.SetActive(false);

            UnwirePlotCreationButtons();
        }

        /// <summary>
        /// Enables/disables "new plot from selected" buttons depending on whether or not anything has been selected;
        /// </summary>
        private void CheckSelection()
        {
            // Enable/disable "new plot from selected" buttons depending on whether or not anything has been selected
            newFromSelectedParent.SetActive(false);
            for (int i = 0; i < dataManager.LinkedIndices.Size; i++)
            {
                if (dataManager.LinkedIndices[i].Highlighted)
                {
                    newFromSelectedParent.SetActive(true);
                    break;
                }
            }
        }

        /// <summary>
        /// Rewires the plot creation buttons to use this data plot manager.
        /// </summary>
        private void RewirePlotCreationButtons()
        {
            newScatterPlotButton.onClick.AddListener(createScatter);
            selectedScatterPlotButton.onClick.AddListener(createScatterFromSelected);
            newParallelCoordsPlotButton.onClick.AddListener(createParallelCoords);
            selectedParallelCoordsPlotButton.onClick.AddListener(createParallelCoordsFromSelected);
            newClusterPlotButton.onClick.AddListener(createCluster);
            selectedClusterPlotButton.onClick.AddListener(createClusterFromSelected);
        }

        /// <summary>
        /// Unwires this data plot manager from using the plot creation buttons.
        /// </summary>
        private void UnwirePlotCreationButtons()
        {
            newScatterPlotButton.onClick.RemoveListener(createScatter);
            selectedScatterPlotButton.onClick.RemoveListener(createScatterFromSelected);
            newParallelCoordsPlotButton.onClick.RemoveListener(createParallelCoords);
            selectedParallelCoordsPlotButton.onClick.RemoveListener(createParallelCoordsFromSelected);
            newClusterPlotButton.onClick.RemoveListener(createCluster);
            selectedClusterPlotButton.onClick.RemoveListener(createClusterFromSelected);
        }
    }
}
