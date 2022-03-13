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
    /// Defines a group of <see cref="DataPlot"/> objects along with their associated <see cref="DataTable"/>
    /// and <see cref="LinkedIndicesGroup"/>.
    /// </summary>
    public class DataPlotGroup : MonoBehaviour
    {
        [Header("Data Table")]
        [SerializeField] private bool initializeFromInspector = true;
#if UNITY_EDITOR
        [ConditionalHide(new string[] { "initializeFromInspector" }, new bool[] { false }, true, false)]
#endif
        [SerializeField] private TabularDataContainer tabularDataContainer;
        [Header("Linked Indices")]
        [SerializeField] private LinkedIndicesGroup linkedIndicesGroup;
        [Header("Masking")]
        [SerializeField] private MaskingMode maskingMode;
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
        [Space(10)]
        [SerializeField] private PlotSetupContainer[] plotSetups;
        [Header("Callbacks")]
        [SerializeField] private UnityEvent onNewDataTableSet;
        [SerializeField] private UnityEvent onShow;
        [SerializeField] private UnityEvent onHide;
        [Header("Dependencies/Plot Canvas")]
        /// <summary> Screen space canvas plots are children of. </summary>
        [SerializeField] private Canvas plotsCanvas;
        /// <summary> Rect that plots are placed within. </summary>
        [SerializeField] private RectTransform plotsRect;
        [Header("Dependencies/Styling")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image dividerImage;
        [SerializeField] private GameObject interactionPanel;
        /// <summary> Prefab used to instantiate cluster toggles. </summary>
        private List<DataPlot> dataPlots = new List<DataPlot>();
        private Transform plotsParent;
        private bool shown = false;
        private DataPlotGroupManager manager;
        private DataTable dataTable;

        /// <summary> Masking mode this plot group uses. </summary>
        public MaskingMode MaskingMode { get => maskingMode; set => maskingMode = value; }
        /// <summary> Gets the parent transform of all of the plots managed by this manager. </summary>
        public Transform PlotsParent { get => plotsParent; }
        /// <summary> Collection of plots that this class manages. </summary>
        public List<DataPlot> DataPlots { get => dataPlots; }
        /// <summary> Padding around the plots rect. </summary>
        public RectPadding PlotsRectPadding { get => plotsRectPadding; set => plotsRectPadding = value; }
        /// <summary> Spacing between plots. </summary>
        public float PlotSpacing { get => plotSpacing; set => plotSpacing = value; }
        /// <summary> Whether or not this data plot manager and its plots are currently shown (focused). </summary>
        public bool Shown { get => shown; }
        /// <summary> 
        /// Gets the data table this data manager is currently using. Can also be used to set
        /// the data table, which automatically causes <see cref="LinkedIndices"/> to reinitialize
        /// and deletes any linked plots.
        /// </summary>
        public DataTable DataTable
        {
            get => dataTable;
            set
            {
                // Set the new data table
                dataTable = value;
                // Perform a refresh based on new data table
                RefreshWithNewDataTable();
            }
        }
        public LinkedIndicesGroup LinkedIndicesGroup { get => linkedIndicesGroup; }


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
        public void Init(DataPlotGroupManager manager, DataTable dataTable = null)
        {
            this.manager = manager;

            // Create a parent for all plots managed by this plot manager
            plotsParent = new GameObject("Plots Parent").AddComponent<RectTransform>();
            plotsParent.SetParent(plotsRect);
            plotsParent.transform.localPosition = Vector3.zero;
            plotsParent.transform.localScale = Vector3.one;
            // Stretch it to the size of the canvas
            ((RectTransform)plotsParent).anchorMin = new Vector2(0, 0);
            ((RectTransform)plotsParent).anchorMax = new Vector2(1, 1);
            ((RectTransform)plotsParent).pivot = new Vector2(0.5f, 0.5f);
            ((RectTransform)plotsParent).offsetMax = Vector2.zero;
            ((RectTransform)plotsParent).offsetMin = Vector2.zero;

            // Initialize plot creation callbacks
            foreach (PlotSetupContainer plotSetup in plotSetups)
            {
                if (overrideIndividualPlotStyling)
                    (plotSetup.plotSkin = Instantiate(plotSetup.plotSkin)).ApplyOverrideStyling(overridePlotSkin);
                plotSetup.newPlotButton.onClick.AddListener(delegate { AddPlot(plotSetup.plotPrefab, plotSetup.plotSkin, false); });
                plotSetup.newPlotFromSelectedButton.onClick.AddListener(delegate { AddPlot(plotSetup.plotPrefab, plotSetup.plotSkin, true); });
            }

            // Initialize the data table
            if (dataTable != null)
            {
                DataTable = dataTable;
            }
            else
            {
                DataTable = initializeFromInspector ? tabularDataContainer.DataTable : this.dataTable;
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
        }

        /// <summary>
        /// Refreshes the data plot manager to use the most current <see cref="DataTable"/>
        /// </summary>
        private void RefreshWithNewDataTable()
        {
            // Log a warning if the data table is empty
            if (dataTable?.IsEmpty() == true)
                Debug.LogWarning("Data table is empty.");
            
            // Reinitialize linked indices using this data table
            linkedIndicesGroup.LinkedIndices = new LinkedIndices(dataTable?.Height ?? 0);

            // Remove all plots
            for (int i = dataPlots.Count - 1; i >= 0; i--)
                RemovePlot(dataPlots[i]);

            // Update the data source dropdowns to reflect the new data table
            manager.UpdateDataDropdown();

            // Make plot creation buttons un/interactable depending on whether or not data table is null
            foreach (PlotSetupContainer plotSetup in plotSetups)
            {
                plotSetup.newPlotButton.interactable = dataTable == null ? false : true;
                plotSetup.newPlotFromSelectedButton.interactable = dataTable == null ? false : true;
            }

            // Name the plots parent using the data table
            plotsParent.name = dataTable?.Name + " Group";

            // Invoke callbacks
            onNewDataTableSet.Invoke();
        }

        void Update()
        {
#if UNITY_EDITOR
            prevPlotsCanvasSkin = plotsCanvasSkin;
#endif
        }

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
        /// Adds a new plot with given styling.
        /// </summary>
        /// <param name="dataPlotPrefab">Prefab GameObject containing the data plot.</param>
        public DataPlot AddPlot(GameObject dataPlotPrefab, DataPlotSkin dataPlotSkin, bool fromSelected = false)
        {
            if (dataPlots.Count >= 4)
                return null;

            int[] selectedIndices = null;
            if (fromSelected)
            {
                // Determine which data point indices are currently selected
                selectedIndices = new int[linkedIndicesGroup.LinkedIndices.HighlightedCount];
                for (int li = 0, si = 0; li < linkedIndicesGroup.LinkedIndices.Size; li++)
                {
                    if (linkedIndicesGroup.LinkedIndices[li].Highlighted)
                        selectedIndices[si++] = li;
                }
            }

            // Instantiate a clone of the plot given by the prefab
            GameObject dataPlot = Instantiate(dataPlotPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            // Attach it to the canvas and reset its scale
            dataPlot.transform.SetParent(plotsParent);
            dataPlot.transform.localScale = Vector3.one;
            // Initialize and plot the data plot using its attached script
            DataPlot dataPlotScript = dataPlot.GetComponent<DataPlot>();
            dataPlotScript.Init(this, dataPlotSkin, Vector2.one * 500, selectedIndices);
            PlottingUtilities.ApplyPlotsLayersRecursive(dataPlot);
            // Add this script to the list of data plot scripts this manager manages
            dataPlots.Add(dataPlotScript);

            // Add this plot to the group of linked indices it is a part of
            linkedIndicesGroup.AddListener(dataPlotScript);

            // Rearrange the plots
            ArrangePlots();

            return dataPlotScript;
        }

        /// <summary>
        /// Removes and destroys the specified plot, if it is being managed by this class.
        /// </summary>
        /// <param name="dataPlot">Script attached to the data plot GameObject that we wish to remove.</param>
        public void RemovePlot(DataPlot dataPlot)
        {
            if (dataPlots.Contains(dataPlot))
            {
                // Remove plot from linked indices listener group
                linkedIndicesGroup.RemoveListener(dataPlot);

                // Remove and delete plot from this manager
                dataPlots.Remove(dataPlot);
                Destroy(dataPlot.gameObject);

                // Reset linked indices if this was the final plot removed 
                // (and there are no other linked indices listeners)
                if (linkedIndicesGroup.ListenerCount() == 0)
                {
                    linkedIndicesGroup.LinkedIndices.Reset();
                    CheckAnySelected();
                }

                // Rearrange the plots
                ArrangePlots();
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
            plotsParent.gameObject.SetActive(true);
            gameObject.SetActive(true);

            // Invoke show callback
            onShow.Invoke();

            // Check to see if any points are selected
            CheckAnySelected();

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
            plotsParent.gameObject.SetActive(false);
            gameObject.SetActive(false);

            // Invoke hide callback
            onHide.Invoke();

            // Disable plot creation buttons
            DisablePlotCreationButtons();
        }

        /// <summary>
        /// Checks if any linked indices are "selected" (aka highlighted) and enables/disables "new plot from selected" buttons accordingly.
        /// </summary>
        public void CheckAnySelected()
        {
            bool anySelected = linkedIndicesGroup.LinkedIndices.HighlightedCount > 0 ? true : false;
            foreach (PlotSetupContainer plotSetup in plotSetups)
                plotSetup.newPlotFromSelectedButton.gameObject.SetActive(anySelected);
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
