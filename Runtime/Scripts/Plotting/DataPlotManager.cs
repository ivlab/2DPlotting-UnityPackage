using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace IVLab.Plotting
{
    [System.Serializable]
    /// <summary>
    /// Container class used to make adding specific data plots along with their
    /// corresponding styling and creation buttons easier via the inspector.
    /// </summary>
    public class PlotSetupContainer
    {
        [Header("Prefab")]
        public GameObject plotPrefab;
        [Header("Styling")]
        public DataPlotSkin plotSkin;
        [Header("Buttons")]
        public Button newPlotButton;
        public Button newPlotFromSelectedButton;
    }

    /// <summary>
    /// Manages the visualization and control of multiple <see cref="DataPlot"/> objects simultaneously.
    /// </summary>
    public class DataPlotManager : MonoBehaviour
    {
        [Header("Styling")]
        /// <summary> Skin (stylesheet) for plots created by this plot manager. </summary>
        [SerializeField] private PlotsCanvasSkin plotsCanvasSkin;
        [SerializeField] private bool overrideIndividualPlotStyling = false;
#if UNITY_EDITOR
        [ConditionalHide("overrideIndividualPlotStyling", true)]
#endif
        /// <summary> Overrides individual plot styling used by each plot. </summary>
        [SerializeField] private DataPlotSkin overridePlotSkin;
        /// <summary> Padding around the plots rect. </summary>
        [SerializeField] private RectPadding plotsRectPadding;
        /// <summary> Spacing between plots. </summary>
        [SerializeField] private float plotSpacing = 25;
        [Header("Plot Setups")]
        [SerializeField] private PlotSetupContainer[] plotSetups;
        [Header("Callbacks")]
        [SerializeField] private UnityEvent refreshCallback;
        [SerializeField] private UnityEvent showCallback;
        [SerializeField] private UnityEvent hideCallback;
        [Header("Dependencies/Plot Canvas")]
        /// <summary> Camera attached to the screen space canvas plots are children of. </summary>
        [SerializeField] private Camera plotsCamera;
        /// <summary> Screen space canvas plots are children of. </summary>
        [SerializeField] private Canvas plotsCanvas;
        /// <summary> Rect that plots are placed within. </summary>
        [SerializeField] private RectTransform plotsRect;
        [Header("Dependencies/Styling")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image dividerImage;
        [SerializeField] private GameObject interactionPanel;
        [SerializeField] private Image[] selectionModeButtons;
        [SerializeField] private Image[] selectionModeIcons;
        /// <summary> Current selection mode being used to select data points. </summary>
        private SelectionMode curSelectionMode;
        /// <summary> Allows selection to be enabled and disabled. </summary>
        private bool selectionEnabled = true;
        /// <summary> Allows only valid selections to be started. </summary>
        private bool validSelection;
        /// <summary> Prefab used to instantiate cluster toggles. </summary>
        private List<DataPlot> dataPlots = new List<DataPlot>();
        private DataManager dataManager;

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
        /// <summary> Padding around the plots rect. </summary>
        public RectPadding PlotsRectPadding { get => plotsRectPadding; set => plotsRectPadding = value; }
        /// <summary> Spacing between plots. </summary>
        public float PlotSpacing { get => plotSpacing; set => plotSpacing = value; }


#if UNITY_EDITOR
		private PlotsCanvasSkin prevPlotsCanvasSkin;
        /// <summary>
        /// Applies styling whenever field is changed in the inspector for this MonoBehaviour.
        /// </summary>
        void OnValidate()
        {
            if (plotsCanvasSkin != prevPlotsCanvasSkin)
            {
                ApplyStyling();
            }
        }
#endif

        /// <summary>
        /// Initializes this plot manager by creating a parent object for all the plots
        /// it will control and initializing its plot creation callbacks.
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
            foreach (PlotSetupContainer plotSetup in plotSetups)
            {
                if (overrideIndividualPlotStyling)
                    (plotSetup.plotSkin = Instantiate(plotSetup.plotSkin)).ApplyOverrideStyling(overridePlotSkin);
                plotSetup.newPlotButton.onClick.AddListener(delegate { AddPlot(plotSetup.plotPrefab, plotSetup.plotSkin); });
                plotSetup.newPlotFromSelectedButton.onClick.AddListener(delegate { AddPlotFromSelected(plotSetup.plotPrefab, plotSetup.plotSkin); });
            }

            // Apply styling
            ApplyStyling();
        }

        /// <summary>
        /// Applies current styling.
        /// </summary>
        private void ApplyStyling()
        {
            backgroundImage.color = plotsCanvasSkin.backgroundColor;
            dividerImage.color = plotsCanvasSkin.dividerColor;

            interactionPanel.GetComponent<Image>().color = plotsCanvasSkin.interactionPanelColor;
            interactionPanel.GetComponent<Outline>().effectColor = plotsCanvasSkin.interactionPanelOutlineColor;

            for (int i = 0; i < selectionModeButtons.Length; i++)
            {
                selectionModeButtons[i].color = plotsCanvasSkin.selectionModeButtonColor;
                selectionModeIcons[i].color = plotsCanvasSkin.selectionModeIconColor;
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

            // Invoke callbacks
            refreshCallback.Invoke();

            // If already shown, hide then show again to rewire connections with new data table
            if (shown)
            {
                Hide();
                Show();
            }
        }

        void Update()
        {
#if UNITY_EDITOR
            prevPlotsCanvasSkin = plotsCanvasSkin;
#endif

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
        /// Hard-coded (aka bad) template for arranging 1-4 plots.
        /// </summary>
        public void ArrangePlots()
        {
            if (dataPlots.Count == 1)
            {
                Vector2 position = new Vector2(0, 0);
                Vector2 outerBounds = plotsRect.rect.size - new Vector2(plotSpacing, plotSpacing);
                dataPlots[0].transform.localPosition = position;
                dataPlots[0].SetPlotSize(outerBounds);
                dataPlots[0].Plot();
            }
            else if (dataPlots.Count == 2)
            {
                Vector2 position1 = new Vector2(0, plotsRect.rect.size.y / 4);
                Vector2 position2 = new Vector2(0, -plotsRect.rect.size.y / 4);
                Vector2 outerBounds = new Vector2(plotsRect.rect.size.x - plotSpacing, plotsRect.rect.size.y / 2 - plotSpacing);
                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].SetPlotSize(outerBounds);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].SetPlotSize(outerBounds);
                dataPlots[1].Plot();
            }
            else if (dataPlots.Count == 3)
            {
                Vector2 position1 = new Vector2(0, plotsRect.rect.size.y / 4);
                Vector2 position2 = new Vector2(0 - plotsRect.rect.size.x / 4 + 0, -plotsRect.rect.size.y / 4);
                Vector2 position3 = new Vector2(0 + plotsRect.rect.size.x / 4 - 0, -plotsRect.rect.size.y / 4);
                Vector2 outerBounds1 = new Vector2(plotsRect.rect.size.x - plotSpacing, plotsRect.rect.size.y / 2 - plotSpacing);
                Vector2 outerBounds23 = new Vector2(plotsRect.rect.size.x / 2 - plotSpacing, plotsRect.rect.size.y / 2 - plotSpacing);
                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].SetPlotSize(outerBounds1);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].SetPlotSize(outerBounds23);
                dataPlots[1].Plot();

                dataPlots[2].transform.localPosition = position3;
                dataPlots[2].SetPlotSize(outerBounds23);
                dataPlots[2].Plot();
            }
            else if (dataPlots.Count == 4)
            {
                Vector2 position1 = new Vector2(0 - plotsRect.rect.size.x / 4 + 0, +plotsRect.rect.size.y / 4);
                Vector2 position2 = new Vector2(0 + plotsRect.rect.size.x / 4 - 0, +plotsRect.rect.size.y / 4);
                Vector2 position3 = new Vector2(0 - plotsRect.rect.size.x / 4 + 0, -plotsRect.rect.size.y / 4);
                Vector2 position4 = new Vector2(0 + plotsRect.rect.size.x / 4 - 0, -plotsRect.rect.size.y / 4);
                Vector2 outerBounds = new Vector2(plotsRect.GetComponent<RectTransform>().rect.size.x / 2 - plotSpacing, plotsRect.GetComponent<RectTransform>().rect.size.y / 2 - plotSpacing);

                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].SetPlotSize(outerBounds);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].SetPlotSize(outerBounds);
                dataPlots[1].Plot();

                dataPlots[2].transform.localPosition = position3;
                dataPlots[2].SetPlotSize(outerBounds);
                dataPlots[2].Plot();

                dataPlots[3].transform.localPosition = position4;
                dataPlots[3].SetPlotSize(outerBounds);
                dataPlots[3].Plot();
            }
        }

        /// <summary>
        /// Adds a new plot using only the currently selected data points.
        /// </summary>
        /// <param name="dataPlotPrefab">Prefab GameObject containing the data plot.</param>
        public void AddPlotFromSelected(GameObject dataPlotPrefab, DataPlotSkin dataPlotSkin)
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
            dataPlotScript.Init(this, dataPlotSkin, Vector2.one * 500, selectedIndices.ToArray());
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
        public void AddPlot(GameObject dataPlotPrefab, DataPlotSkin dataPlotSkin)
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
            dataPlotScript.Init(this, dataPlotSkin, Vector2.one * 500);
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
                }

                CheckSelection();
            }
        }

        /// <summary>
        /// Shows this data plot manager, all of its plot, and enables the plot creation buttons.
        /// </summary>
        public void Show()
        {
            shown = true;

            // Apply styling
            ApplyStyling();

            // Set the plots to active
            PlotsParent.gameObject.SetActive(true);
            gameObject.SetActive(true);

            // Invoke show callback
            showCallback.Invoke();

            // Check to see if any points are selected
            CheckSelection();

            // Enable plot creation buttons
            EnablePlotCreationButtons();

            // Adjust the size of the plot rect
            plotsRect.offsetMin = new Vector2(plotsRectPadding.left, plotsRectPadding.bottom);
            plotsRect.offsetMax = -new Vector2(plotsRectPadding.right, plotsRectPadding.top);
        }

        /// <summary>
        /// Hides this data plot manager, all of its plot, and disables the plot creation buttons.
        /// </summary>
        public void Hide()
        {
            shown = false;

            // Hide the plots
            PlotsParent.gameObject.SetActive(false);
            gameObject.SetActive(false);

            // Invoke hide callback
            hideCallback.Invoke();

            // Disable plot creation buttons
            DisablePlotCreationButtons();
        }

        /// <summary>
        /// Enables/disables "new plot from selected" buttons depending on whether or not any linked indices are selected;
        /// </summary>
        public void CheckSelection()
        {
            foreach (PlotSetupContainer plotSetup in plotSetups)
            {
                plotSetup.newPlotFromSelectedButton.gameObject.SetActive(false);
            }
            for (int i = 0; i < dataManager.LinkedIndices.Size; i++)
            {
                if (dataManager.LinkedIndices[i].Highlighted)
                {
                    foreach (PlotSetupContainer plotSetup in plotSetups)
                    {
                        plotSetup.newPlotFromSelectedButton.gameObject.SetActive(true);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Enables the plot creation buttons for this data plot manager.
        /// </summary>
        private void EnablePlotCreationButtons()
        {
            foreach (PlotSetupContainer plotSetup in plotSetups)
            {
                plotSetup.newPlotButton.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Disables the plot creation buttons for this data plot manager.
        /// </summary>
        private void DisablePlotCreationButtons()
        {
            foreach (PlotSetupContainer plotSetup in plotSetups)
            {
                plotSetup.newPlotButton.gameObject.SetActive(false);
                plotSetup.newPlotFromSelectedButton.gameObject.SetActive(false);
            }
        }
    }
}
