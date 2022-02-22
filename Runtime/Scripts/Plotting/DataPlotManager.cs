using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace IVLab.Plotting
{
    /// <summary>
    /// Manages the visualization and control of multiple <see cref="DataPlot"/> objects simultaneously.
    /// </summary>
    public class DataPlotManager : MonoBehaviour
    {
        [Header("Styling")]
        /// <summary> Skin (stylesheet) for plots created by this plot manager. </summary>
        [SerializeField] private PlotUISkin plotSkin;
        [Header("Dependencies/Plot Stuff")]
        /// <summary> Camera attached to the screen space canvas plots are children of. </summary>
        [SerializeField] private Camera plotsCamera;
        /// <summary> Screen space canvas plots are children of. </summary>
        [SerializeField] private Canvas plotsCanvas;
        /// <summary> Rect that plots are placed within. </summary>
        [SerializeField] private RectTransform plotsRect;
        /// <summary> Padding around the plots rect. </summary>
        [SerializeField] private RectPadding plotsRectPadding;
        [Header("Dependencies/Styling")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image dividerImage;
        [SerializeField] private GameObject interactionPanel;
        [SerializeField] private Image[] selectionModeButtons;
        [SerializeField] private Image[] selectionModeIcons;
        [Header("Dependencies/Buttons")]
        /// <summary> Parent GameObject of the "new plot from selected" buttons. Used to toggle them on/off. </summary>
        [SerializeField] private GameObject newFromSelectedParent;
        [SerializeField] private Button newScatterPlotButton, newParallelCoordsPlotButton, newClusterPlotButton,
            selectedScatterPlotButton, selectedParallelCoordsPlotButton, selectedClusterPlotButton;
        [Header("Dependencies/Prefabs")]
        [SerializeField] private GameObject scatterPlotPrefab;
        [SerializeField] private GameObject parallelCoordsPlotPrefab;
        [SerializeField] private GameObject clusterPlotPrefab;
        [SerializeField] private GameObject togglePrefab;
        /// <summary> Array of cluster toggles used to hide/show clusters. </summary>
        private Toggle[] clusterToggles = new Toggle[0];
        /// <summary> Saves a copy of each clusters linked attributes before they are toggled off
        /// so that they can easily return to it when toggled on. </summary>
        private LinkedIndices.LinkedAttributes[][] savedClusterLinkedAttributes;
        /// <summary> Parent gameobject of cluster toggles. </summary>
        private Transform clusterToggleParent;
        /// <summary> Current selection mode being used to select data points. </summary>
        private SelectionMode curSelectionMode;
        /// <summary> Allows selection to be enabled and disabled. </summary>
        private bool selectionEnabled = true;
        /// <summary> Allows only valid selections to be started. </summary>
        private bool validSelection;
        /// <summary> Prefab used to instantiate cluster toggles. </summary>
        private List<DataPlot> dataPlots = new List<DataPlot>();
        private DataManager dataManager;
        private UnityAction createScatter;
        private UnityAction createParallelCoords;
        private UnityAction createCluster;
        private UnityAction createScatterFromSelected;
        private UnityAction createParallelCoordsFromSelected;
        private UnityAction createClusterFromSelected;
        /// <summary>
        /// Whether or not this data plot manager and its plots are currently shown (focused).
        /// </summary>
        private bool shown = false;

        /// <summary> Gets the parent transform of all of the plots managed by this manager. </summary>
        public Transform PlotsParent { get; private set; }
        /// <summary> Collection of plots that this class manages. </summary>
        public List<DataPlot> DataPlots { get => dataPlots; }
        /// <summary> Data manager that manages this data plot manager's data,
        /// i.e. provides the DataTable and LinkedIndices. </summary>
        public DataManager DataManager { get => dataManager; set => dataManager = value; }
        /// <summary> Gets the cluster toggles created by this plot manager. </summary>
        public Toggle[] ClusterToggles { get => clusterToggles; }

        /// <summary>
        /// Applies styling whenever field is changed in the inspector for this MonoBehaviour.
        /// </summary>
        void OnValidate()
        {
            ApplyStyling();
        }

        /// <summary>
        /// Initializes this plot manager by creating a parent object for all the plots
        /// it will control and initializing its plot creations callback.
        /// </summary>
        /// <remarks>
        /// <b>Must</b> be called before <see cref="DataManager.Init(MultiDataManager, DataPlotManager)"/> or <see cref="DataManager.Init(MultiDataManager, DataTable, DataPlotManager, List{LinkedData})"/>.
        /// </remarks>
        public void Init()
        {
            // Create a parent for all plots managed by this plot manager
            PlotsParent = new GameObject("Plots Parent").AddComponent<RectTransform>();
            PlotsParent.SetParent(plotsRect);
            PlotsParent.transform.localPosition = Vector3.zero;
            PlotsParent.transform.localScale = Vector3.one;
            // Stretch it to the size of the canvas
            ((RectTransform)PlotsParent).anchorMin = new Vector2(0, 0);
            ((RectTransform)PlotsParent).anchorMax = new Vector2(1, 1);
            ((RectTransform)PlotsParent).pivot = new Vector2(0.5f, 0.5f);
            ((RectTransform)PlotsParent).offsetMax = Vector2.zero;
            ((RectTransform)PlotsParent).offsetMin = Vector2.zero;

            // Initialize plot creation callbacks
            createScatter = () => { AddPlot(scatterPlotPrefab); };
            createParallelCoords = () => { AddPlot(parallelCoordsPlotPrefab); };
            createCluster = () => { AddPlot(clusterPlotPrefab); };
            createScatterFromSelected = () => { AddPlotFromSelected(scatterPlotPrefab); };
            createParallelCoordsFromSelected = () => { AddPlotFromSelected(parallelCoordsPlotPrefab); };
            createClusterFromSelected = () => { AddPlotFromSelected(clusterPlotPrefab); };

            // Apply styling
            ApplyStyling();
        }

        /// <summary>
        /// Applies current styling.
        /// </summary>
        private void ApplyStyling()
        {
            backgroundImage.color = plotSkin.backgroundColor;
            dividerImage.color = plotSkin.dividerColor;

            interactionPanel.GetComponent<Image>().color = plotSkin.interactionPanelColor;
            interactionPanel.GetComponent<Outline>().effectColor = plotSkin.interactionPanelOutlineColor;

            newScatterPlotButton.GetComponent<Image>().color = plotSkin.createPlotButtonColor;
            newParallelCoordsPlotButton.GetComponent<Image>().color = plotSkin.createPlotButtonColor;
            newClusterPlotButton.GetComponent<Image>().color = plotSkin.createPlotButtonColor;

            selectedScatterPlotButton.GetComponent<Image>().color = plotSkin.createPlotFromSelectedButtonColor;
            selectedParallelCoordsPlotButton.GetComponent<Image>().color = plotSkin.createPlotFromSelectedButtonColor;
            selectedClusterPlotButton.GetComponent<Image>().color = plotSkin.createPlotFromSelectedButtonColor;

            for (int i = 0; i < selectionModeButtons.Length; i++)
            {
                selectionModeButtons[i].color = plotSkin.selectionModeButtonColor;
                selectionModeIcons[i].color = plotSkin.selectionModeIconColor;
            }
        }

        /// <summary>
        /// Refreshes the data plot manager to use the most current <see cref="DataTable"/>
        /// in <see cref="DataManager"/>.
        /// </summary>
        public void Refresh()
        {
            // Name the plots parent using the data table
            PlotsParent.name = dataManager.DataTable.Name;

            // Delete any previous cluster toggles
            foreach (Toggle toggle in clusterToggles)
            {
                Destroy(toggle.gameObject);
            }
            // Create new cluster toggles if using a clustered data table
            if (dataManager.UsingClusterDataTable)
            {
                // Define a uniform spacing between toggles
                float clusterToggleSpacing = 85;

                // Allow space at bottom of canvas for cluster toggles
                plotsRectPadding.bottom = 25;

                // Destroy old / create new cluster toggle parent
                if (clusterToggleParent?.gameObject != null) 
                    Destroy(clusterToggleParent.gameObject);
                clusterToggleParent = new GameObject("Cluster Toggles").AddComponent<RectTransform>();
                clusterToggleParent.SetParent(PlotsParent.transform);
                clusterToggleParent.localScale = Vector3.one;
                clusterToggleParent.localPosition = Vector3.zero;
                // Stretch it to the size of its parent (plot parent)
                ((RectTransform)clusterToggleParent).anchorMin = new Vector2(0, 0);
                ((RectTransform)clusterToggleParent).anchorMax = new Vector2(1, 1);
                ((RectTransform)clusterToggleParent).pivot = new Vector2(0.5f, 0.5f);
                ((RectTransform)clusterToggleParent).offsetMax = Vector2.zero;
                ((RectTransform)clusterToggleParent).offsetMin = Vector2.zero;
                
                // Initialize relevant cluster arrays
                List<Cluster> clusters = ((ClusterDataTable)dataManager.DataTable).Clusters;
                clusterToggles = new Toggle[clusters.Count];
                savedClusterLinkedAttributes = new LinkedIndices.LinkedAttributes[clusters.Count][];

                // Create the cluster background image
                RectTransform backgroundRect = new GameObject("Toggles Background").AddComponent<RectTransform>();
                backgroundRect.SetParent(clusterToggleParent);
                backgroundRect.transform.localScale = Vector3.one;
                backgroundRect.transform.localPosition = Vector3.zero;
                backgroundRect.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
                backgroundRect.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
                backgroundRect.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                backgroundRect.sizeDelta = new Vector2(clusters.Count * (clusterToggleSpacing) + 25, 25);
                backgroundRect.gameObject.AddComponent<Image>().color = new Color32(228, 239, 243, 255);
                backgroundRect.gameObject.AddComponent<Outline>();

                // Create cluster toggles
                for (int i = 0; i < clusterToggles.Length; i++)
                {
                    // Instantiate the toggle
                    GameObject toggleObject = Instantiate(togglePrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    // Position the toggle
                    toggleObject.transform.SetParent(clusterToggleParent);
                    toggleObject.transform.localScale = Vector3.one;
                    toggleObject.transform.localPosition = Vector3.zero;
                    toggleObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
                    toggleObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
                    toggleObject.GetComponent<RectTransform>().anchoredPosition = Vector2.right * ((i - (clusters.Count - 1) / 2.0f) * clusterToggleSpacing - 20);
                    PlottingUtilities.ApplyPlotsLayersRecursive(toggleObject);
                    // Set the toggle's text and color
                    Toggle toggle = toggleObject.GetComponent<Toggle>();
                    toggle.GetComponentInChildren<TextMeshProUGUI>().text = dataManager.DataTable.ColumnNames[0] + " " + clusters[i].Id;
                    toggle.GetComponentInChildren<TextMeshProUGUI>().color = clusters[i].Color;
                    toggle.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().color = clusters[i].Color;
                    clusterToggles[i] = toggle;
                    // Add a callback for when the toggle is... toggled
                    int clusterIdx = i;
                    toggle.onValueChanged.AddListener(delegate { ToggleCluster(clusterIdx); });

                    // Initialize saved linked attributes for this cluster
                    savedClusterLinkedAttributes[i] = new LinkedIndices.LinkedAttributes[clusters[i].EndIdx - clusters[i].StartIdx];
                }
            }
            else
            {
                // With no toggles, no need for padding at the bottom of the canvas
                plotsRectPadding.bottom = 0;
            }

            // If already shown, hide then show again to rewire connections with new data table
            if (shown)
            {
                Hide();
                Show();
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
                            dataManager.MaskingData = false;
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
        /// Toggles specified cluster's visibility.
        /// </summary>
        /// <param name="clusterIdx"> Index into <see cref="clusterToggles"/> array. </param>
        private void ToggleCluster(int clusterIdx)
        {
            // Determine the start/end indices of the cluster
            int clusterStartIdx = ((ClusterDataTable)dataManager.DataTable).Clusters[clusterIdx].StartIdx;
            int clusterEndidx = ((ClusterDataTable)dataManager.DataTable).Clusters[clusterIdx].EndIdx;
            // Toggled on
            if (clusterToggles[clusterIdx].isOn)
            {
                // If nothing else is masked, it makes most sense to simply fully unmask this cluster
                if (dataManager.NothingMasked)
                {
                    for (int i = clusterStartIdx; i < clusterEndidx; i++)
                    {
                        dataManager.LinkedIndices[i].Masked = false;
                    }
                }
                // Otherwise, let's return all of the points in the cluster to their saved configuration
                else
                {
                    for (int i = clusterStartIdx; i < clusterEndidx; i++)
                    {
                        dataManager.LinkedIndices[i] =
                            new LinkedIndices.LinkedAttributes(savedClusterLinkedAttributes[clusterIdx][i - clusterStartIdx]);
                    }
                    dataManager.LinkedIndices.LinkedAttributesChanged = true;
                }
            }
            // Toggled off
            else
            {
                // Save the cluster's linked state and mask all of its points
                for (int i = clusterStartIdx; i < clusterEndidx; i++)
                {
                    dataManager.LinkedIndices[i].Highlighted = false;
                    savedClusterLinkedAttributes[clusterIdx][i - clusterStartIdx] = 
                        new LinkedIndices.LinkedAttributes(dataManager.LinkedIndices[i]);
                    dataManager.LinkedIndices[i].Masked = true;
                }
                // Toggling the cluster off unhighlighted some data points
                // so we should check to see if anything is still selected
                CheckSelection();
            }
        }

        /// <summary>
        /// Hard-coded (aka bad) template for arranging 1-4 plots.
        /// </summary>
        private void ArrangePlots()
        {
            if (dataPlots.Count == 1)
            {
                Vector2 position = new Vector2(0, 0);
                Vector2 outerBounds = plotsRect.rect.size - new Vector2(50, 50);
                DataPlot.PlotLayout plotLayout = new DataPlot.PlotLayout(outerBounds, null);
                dataPlots[0].transform.localPosition = position;
                dataPlots[0].ResizePlot(plotLayout);
                dataPlots[0].Plot();
            }
            else if (dataPlots.Count == 2)
            {
                Vector2 position1 = new Vector2(0, plotsRect.rect.size.y / 4);
                Vector2 position2 = new Vector2(0, -plotsRect.rect.size.y / 4);
                Vector2 outerBounds = new Vector2(plotsRect.rect.size.x - 50, plotsRect.rect.size.y / 2 - 50);
                DataPlot.PlotLayout plotLayout = new DataPlot.PlotLayout(outerBounds, null);
                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].ResizePlot(plotLayout);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].ResizePlot(plotLayout);
                dataPlots[1].Plot();
            }
            else if (dataPlots.Count == 3)
            {
                Vector2 position1 = new Vector2(0, plotsRect.rect.size.y / 4);
                Vector2 position2 = new Vector2(0 - plotsRect.rect.size.x / 4 + 0, -plotsRect.rect.size.y / 4);
                Vector2 position3 = new Vector2(0 + plotsRect.rect.size.x / 4 - 0, -plotsRect.rect.size.y / 4);
                Vector2 outerBounds1 = new Vector2(plotsRect.rect.size.x - 50, plotsRect.rect.size.y / 2 - 50);
                Vector2 outerBounds23 = new Vector2(plotsRect.rect.size.x / 2 - 50, plotsRect.rect.size.y / 2 - 50);
                DataPlot.PlotLayout plotLayout1 = new DataPlot.PlotLayout(outerBounds1, null);
                DataPlot.PlotLayout plotLayout23 = new DataPlot.PlotLayout(outerBounds23, null);
                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].ResizePlot(plotLayout1);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].ResizePlot(plotLayout23);
                dataPlots[1].Plot();

                dataPlots[2].transform.localPosition = position3;
                dataPlots[2].ResizePlot(plotLayout23);
                dataPlots[2].Plot();
            }
            else if (dataPlots.Count == 4)
            {
                Vector2 position1 = new Vector2(0 - plotsRect.rect.size.x / 4 + 0, +plotsRect.rect.size.y / 4);
                Vector2 position2 = new Vector2(0 + plotsRect.rect.size.x / 4 - 0, +plotsRect.rect.size.y / 4);
                Vector2 position3 = new Vector2(0 - plotsRect.rect.size.x / 4 + 0, -plotsRect.rect.size.y / 4);
                Vector2 position4 = new Vector2(0 + plotsRect.rect.size.x / 4 - 0, -plotsRect.rect.size.y / 4);
                Vector2 outerBounds = new Vector2(plotsRect.GetComponent<RectTransform>().rect.size.x / 2 - 50, plotsRect.GetComponent<RectTransform>().rect.size.y / 2 - 50);
                DataPlot.PlotLayout plotLayout = new DataPlot.PlotLayout(outerBounds, null);

                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].ResizePlot(plotLayout);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].ResizePlot(plotLayout);
                dataPlots[1].Plot();

                dataPlots[2].transform.localPosition = position3;
                dataPlots[2].ResizePlot(plotLayout);
                dataPlots[2].Plot();

                dataPlots[3].transform.localPosition = position4;
                dataPlots[3].ResizePlot(plotLayout);
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
            // Set the layout of the plot
            DataPlot.PlotLayout plotLayout = new DataPlot.PlotLayout(Vector2.one * 500, null);
            // Initialize and plot the data plot using its attached script
            DataPlot dataPlotScript = dataPlot.GetComponent<DataPlot>();
            dataPlotScript.Init(this, plotSkin, plotLayout, selectedIndices.ToArray());
            PlottingUtilities.ApplyPlotsLayersRecursive(dataPlot);
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
            // Set the layout of the plot
            DataPlot.PlotLayout plotLayout = new DataPlot.PlotLayout(Vector2.one * 500, null);
            // Initialize and plot the data plot using its attached script
            DataPlot dataPlotScript = dataPlot.GetComponent<DataPlot>();
            dataPlotScript.Init(this, plotSkin, plotLayout);
            PlottingUtilities.ApplyPlotsLayersRecursive(dataPlot);
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

                // If the final plot was deleted and there is no linked data, reset linked indices
                if (dataPlots.Count == 0 && dataManager.LinkedData?.Count == 0)
                {
                    dataManager.LinkedIndices.Reset();
                    newFromSelectedParent.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Shows this data plot manager, all of its plot, and rewires the plot creation buttons.
        /// </summary>
        public void Show()
        {
            shown = true;

            // Set the plots to active
            PlotsParent.gameObject.SetActive(true);
            gameObject.SetActive(true);

            // Check to see if any points are selected
            CheckSelection();

            // Rewire plot creation buttons
            RewirePlotCreationButtons();

            // Adjust the size of the plot rect
            plotsRect.offsetMin = new Vector2(plotsRectPadding.left, plotsRectPadding.bottom);
            plotsRect.offsetMax = -new Vector2(plotsRectPadding.right, plotsRectPadding.top);
        }

        /// <summary>
        /// Hides this data plot manager, all of its plot, and unwires the plot creation buttons.
        /// </summary>
        public void Hide()
        {
            shown = false;

            // Hide the plots
            PlotsParent.gameObject.SetActive(false);
            gameObject.SetActive(false);

            // Unwire plot creation buttons
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
            // Make the plot creation buttons uninteractable if the data table is empty
            // or null
            if (dataManager.DataTable?.IsEmpty() != false)
            {
                newScatterPlotButton.interactable = false;
                selectedScatterPlotButton.interactable = false;
                newParallelCoordsPlotButton.interactable = false;
                selectedParallelCoordsPlotButton.interactable = false;
            }
            else
            {
                newScatterPlotButton.interactable = true;
                selectedScatterPlotButton.interactable = true;
                newParallelCoordsPlotButton.interactable = true;
                selectedParallelCoordsPlotButton.interactable = true;
            }

            newScatterPlotButton.onClick.AddListener(createScatter);
            selectedScatterPlotButton.onClick.AddListener(createScatterFromSelected);
            newParallelCoordsPlotButton.onClick.AddListener(createParallelCoords);
            selectedParallelCoordsPlotButton.onClick.AddListener(createParallelCoordsFromSelected);
            // Enable/disable cluster plot creation buttons depending on whether or not these plots are
            // using a cluster data table
            if (dataManager.UsingClusterDataTable)
            {
                newClusterPlotButton.gameObject.SetActive(true);
                selectedClusterPlotButton.gameObject.SetActive(true);
                newClusterPlotButton.onClick.AddListener(createCluster);
                selectedClusterPlotButton.onClick.AddListener(createClusterFromSelected);

                // Make the cluster plot creation buttons uninteractable if the cluster data table is empty
                // or null
                if (((ClusterDataTable)dataManager.DataTable)?.IsEmpty() != false)
                {
                    newClusterPlotButton.interactable = false;
                    selectedClusterPlotButton.interactable = false;
                }
                else
                {
                    newClusterPlotButton.interactable = true;
                    selectedClusterPlotButton.interactable = true;
                }
            }
            else
            {
                newClusterPlotButton.gameObject.SetActive(false);
                selectedClusterPlotButton.gameObject.SetActive(false);
            }
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
