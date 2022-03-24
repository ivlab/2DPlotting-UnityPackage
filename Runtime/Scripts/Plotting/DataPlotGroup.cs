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

    [System.Serializable]
    public class DataPlotEvent : UnityEvent<DataPlot> {}

    /// <summary>
    /// Defines a group of <see cref="DataPlot"/> objects along with their associated <see cref="TableData"/>
    /// and <see cref="LinkedIndices"/>.
    /// </summary>
    public class DataPlotGroup : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Inspector Serialized Fields 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        [Header("Data Table")]
        /// <summary> Whether or not to initialize the data table from a tabular data container in the inspector. </summary>
        [Tooltip("Whether or not to initialize the data table from a tabular data container in the inspector.")]
        [SerializeField] private bool initializeFromInspector = true;
#if UNITY_EDITOR
        [ConditionalHide(new string[] { "initializeFromInspector" }, new bool[] { false }, true, false)]
#endif
        /// <summary> Container for the data table that this plot group will use. </summary>
        [Tooltip("Container for the data table that this plot group will use.")]
        [SerializeField] private TabularDataContainer tabularDataContainer;

        [Header("Linked Indices")]
        /// <summary> Linked indices group this plot group is a member of. </summary>
        [Tooltip("Linked indices group this plot group is a member of.")]
        [SerializeField] private LinkedIndices linkedIndices;

        [Header("Masking")]
        [SerializeField] private MaskingMode maskingMode;

        [Header("Styling")]
        /// <summary> Skin (stylesheet) for the canvas of this plot group. </summary>
        [Tooltip("Skin (stylesheet) for the canvas of this plot group.")]
        [SerializeField] private PlotsCanvasSkin plotsCanvasSkin;
        /// <summary> Whether or not individual plot styling should be overridden. </summary>
        [Tooltip("Whether or not individual plot styling should be overridden.")]
        [SerializeField] private bool overrideIndividualPlotStyling = false;
#if UNITY_EDITOR
        [ConditionalHide("overrideIndividualPlotStyling", true)]
#endif
        /// <summary> Overrides individual plot styling used by each plot. </summary>
        [Tooltip("Overrides individual plot styling used by each plot.")]
        [SerializeField] private DataPlotSkin overridePlotSkin;
        /// <summary> Padding used to re-size the plots container. </summary>
        [Tooltip("Padding used to re-size the plots container.")]
        [SerializeField] private RectPadding plotsContainerPadding;
        /// <summary> Spacing between plots. </summary>
        [Tooltip("Spacing between plots.")]
        [SerializeField] private float plotSpacing = 25;

        [Space(10)]
        /// <summary> Setups for each of the plots this plot group is allowed to create. </summary>
        [Tooltip("Setups for each of the plots this plot group is allowed to create.")]
        [SerializeField] private PlotSetupContainer[] plotSetups;

        [Header("Callbacks")]
        [SerializeField] private DataPlotEvent onDataPlotAdded;
        [SerializeField] private UnityEvent onNewTableDataSet;
        [SerializeField] private UnityEvent onShow;
        [SerializeField] private UnityEvent onHide;

        [Header("Dependencies/Plot Canvas")]
        /// <summary> Screen space canvas plots are children of. </summary>
        [SerializeField] private Canvas plotsCanvas;
        /// <summary> Rect that plots are contained within. </summary>
        [SerializeField] private RectTransform plotsContainer;

        [Header("Dependencies/Styling")]
        [SerializeField] private Image canvasBackgroundImage;
        [SerializeField] private Image dividerImage;
        [SerializeField] private GameObject interactionPanel;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Private Fields 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        private List<DataPlot> dataPlots = new List<DataPlot>();
        private Transform plotsParent;
        private bool shown = false;
        private TableData tableData;


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Public Properties 
        //////////////////////////////////////////////////////////////////////////////////////////////////// 
        /// <summary> Masking mode this plot group uses. </summary>
        public MaskingMode MaskingMode { get => maskingMode; set => maskingMode = value; }
        /// <summary> Gets the parent transform of all of the plots in this group. </summary>
        public Transform PlotsParent { get => plotsParent; }
        /// <summary> Collection of plots that this class manages. </summary>
        public List<DataPlot> DataPlots { get => dataPlots; }
        /// <summary> Padding around the plots container. </summary>
        public RectPadding PlotsContainerPadding { get => plotsContainerPadding; set => plotsContainerPadding = value; }
        /// <summary> Spacing between plots. </summary>
        public float PlotSpacing { get => plotSpacing; set => plotSpacing = value; }
        /// <summary> Whether or not this data plot group is currently shown. </summary>
        public bool Shown { get => shown; }
        public DataPlotEvent OnDataPlotAdded { get => onDataPlotAdded; }
        public UnityEvent OnNewTableDataSet { get => onNewTableDataSet; }
        public UnityEvent OnShow { get => onShow; }
        public UnityEvent OnHide { get => onHide; }
        /// <summary> Gets the linked indices group this data plot group is a member of. </summary>
        public LinkedIndices LinkedIndices { get => linkedIndices; }
        /// <summary> 
        /// Gets the data table this plot group is currently using. Can also be used to set
        /// the data table, which automatically causes <see cref="LinkedIndices"/> to reinitialize
        /// and deletes any linked plots, among other actions.
        /// </summary>
        public TableData TableData
        {
            get => tableData;
            set
            {
                // Set the new data table
                tableData = value;
                // Perform a refresh based on new data table
                RefreshWithNewTableData();
            }
        }


#if UNITY_EDITOR
		private PlotsCanvasSkin prevPlotsCanvasSkin;
        /// <summary>
        /// Applies styling whenever field is changed in the inspector for this MonoBehaviour.
        /// </summary>
        void OnValidate()
        {
            if (plotsCanvasSkin != prevPlotsCanvasSkin)
            {
                ApplyCanvasStyling();
            }
        }
#endif

        /// <summary>
        /// Initializes this plot group by assigning it a data table, creating a parent object for all the plots
        /// it will control and initializing its plot creation button callbacks.
        /// </summary>
        public void Init(TableData tableData = null)
        {
            // Create a parent for all plots added to this group
            plotsParent = new GameObject(tableData?.Name + " Group").AddComponent<RectTransform>();
            plotsParent.SetParent(plotsContainer);
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
            if (tableData != null)
            {
                TableData = tableData;
            }
            else
            {
                TableData = initializeFromInspector ? tabularDataContainer.TableData : this.tableData;
            }

            // Listen to linked indices so that we can check if anything is selected whenever changes to the indices occur
            linkedIndices.OnAnyIndexAttributeChanged.AddListener(CheckAnySelected);
            linkedIndices.OnIndicesReinitialized.AddListener(CheckAnySelected);

            // Apply canvas styling
            ApplyCanvasStyling();
        }

        /// <summary>
        /// Applies current plot group canvas styling.
        /// </summary>
        private void ApplyCanvasStyling()
        {
            if (canvasBackgroundImage != null)
                canvasBackgroundImage.color = plotsCanvasSkin.backgroundColor;
            if (dividerImage != null)
                dividerImage.color = plotsCanvasSkin.dividerColor;

            if (interactionPanel != null)
            {
                interactionPanel.GetComponent<Image>().color = plotsCanvasSkin.interactionPanelColor;
                interactionPanel.GetComponent<Outline>().effectColor = plotsCanvasSkin.interactionPanelOutlineColor;
            }
        }

        /// <summary>
        /// Refreshes this data plot group to use the most current <see cref="TableData"/>
        /// </summary>
        private void RefreshWithNewTableData()
        {
            // Log a warning if the data table is empty
            if (tableData?.IsEmpty() == true)
                Debug.LogWarning("Data table is empty.");
            
            // Reinitialize linked indices using this data table
            linkedIndices.Init(tableData?.Height ?? 0);

            // Remove all plots
            for (int i = dataPlots.Count - 1; i >= 0; i--)
                RemovePlot(dataPlots[i]);

            // Make plot creation buttons un/interactable depending on whether or not data table is null
            foreach (PlotSetupContainer plotSetup in plotSetups)
            {
                plotSetup.newPlotButton.interactable = tableData == null ? false : true;
                plotSetup.newPlotFromSelectedButton.interactable = tableData == null ? false : true;
            }

            // Name the plots parent using the data table
            plotsParent.name = tableData?.Name + " Group";

            // Invoke callbacks
            onNewTableDataSet.Invoke();
        }

        void Update()
        {
#if UNITY_EDITOR
            prevPlotsCanvasSkin = plotsCanvasSkin;
#endif
            if (Input.GetKeyDown(maskingMode.ToggleKey))
            {
                maskingMode.ToggleMasking();
            }
        }

        /// <summary>
        /// Hard-coded (aka bad) template for arranging 1-4 plots.
        /// </summary>
        public void ArrangePlots()
        {
            if (dataPlots.Count == 1)
            {
                Vector2 position = new Vector2(0, 0);
                Vector2 outerBounds = plotsContainer.rect.size - new Vector2(plotSpacing, plotSpacing);
                dataPlots[0].transform.localPosition = position;
                dataPlots[0].SetPlotSize(outerBounds);
                dataPlots[0].Plot();
            }
            else if (dataPlots.Count == 2)
            {
                bool splitVertically = plotsContainer.rect.height > plotsContainer.rect.width;
                Vector2 position1 = splitVertically ? new Vector2(0, plotsContainer.rect.size.y / 4) : new Vector2(plotsContainer.rect.size.x / 4, 0);
                Vector2 position2 = splitVertically ? new Vector2(0, -plotsContainer.rect.size.y / 4) : new Vector2(-plotsContainer.rect.size.x / 4, 0);
                Vector2 outerBounds = splitVertically ? new Vector2(plotsContainer.rect.size.x - plotSpacing, plotsContainer.rect.size.y / 2 - plotSpacing) : new Vector2(plotsContainer.rect.size.x / 2 - plotSpacing, plotsContainer.rect.size.y - plotSpacing);
                dataPlots[0].transform.localPosition = position1;
                dataPlots[0].SetPlotSize(outerBounds);
                dataPlots[0].Plot();

                dataPlots[1].transform.localPosition = position2;
                dataPlots[1].SetPlotSize(outerBounds);
                dataPlots[1].Plot();
            }
            else if (dataPlots.Count == 3)
            {
                bool splitVertically = plotsContainer.rect.height > plotsContainer.rect.width;
                Vector2 position1 = splitVertically ? new Vector2(0, plotsContainer.rect.size.y / 4) : new Vector2(-plotsContainer.rect.size.x / 4, 0);
                Vector2 position2 = splitVertically ? new Vector2(-plotsContainer.rect.size.x / 4, -plotsContainer.rect.size.y / 4) : new Vector2(plotsContainer.rect.size.x / 4, plotsContainer.rect.size.y / 4);
                Vector2 position3 = splitVertically ? new Vector2(plotsContainer.rect.size.x / 4, -plotsContainer.rect.size.y / 4) : new Vector2(plotsContainer.rect.size.x / 4, -plotsContainer.rect.size.y / 4);
                Vector2 outerBounds1 = splitVertically ? new Vector2(plotsContainer.rect.size.x - plotSpacing, plotsContainer.rect.size.y / 2 - plotSpacing) : new Vector2(plotsContainer.rect.size.x /2 - plotSpacing, plotsContainer.rect.size.y - plotSpacing);
                Vector2 outerBounds23 = new Vector2(plotsContainer.rect.size.x / 2 - plotSpacing, plotsContainer.rect.size.y / 2 - plotSpacing);
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
                Vector2 position1 = new Vector2(0 - plotsContainer.rect.size.x / 4 + 0, +plotsContainer.rect.size.y / 4);
                Vector2 position2 = new Vector2(0 + plotsContainer.rect.size.x / 4 - 0, +plotsContainer.rect.size.y / 4);
                Vector2 position3 = new Vector2(0 - plotsContainer.rect.size.x / 4 + 0, -plotsContainer.rect.size.y / 4);
                Vector2 position4 = new Vector2(0 + plotsContainer.rect.size.x / 4 - 0, -plotsContainer.rect.size.y / 4);
                Vector2 outerBounds = new Vector2(plotsContainer.GetComponent<RectTransform>().rect.size.x / 2 - plotSpacing, plotsContainer.GetComponent<RectTransform>().rect.size.y / 2 - plotSpacing);

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
        /// Adds a new plot to this group with given styling.
        /// </summary>
        /// <param name="dataPlotPrefab">Prefab GameObject containing the data plot.</param>
        /// <param name="dataPlotSkin">Skin containing styling information for the new data plot.</param>
        /// <param name="fromSelected">Whether or not the plot should be created from only the currently selected points.</param>
        public DataPlot AddPlot(GameObject dataPlotPrefab, DataPlotSkin dataPlotSkin, bool fromSelected = false)
        {
            if (dataPlots.Count >= 4)
                return null;

            int[] selectedIndices = null;
            if (fromSelected)
            {
                // Determine which indices are currently selected
                selectedIndices = new int[linkedIndices.HighlightedCount];
                for (int li = 0, si = 0; li < linkedIndices.Size; li++)
                {
                    if (linkedIndices[li].Highlighted)
                        selectedIndices[si++] = li;
                }
            }

            // Instantiate a clone of the plot given by the prefab
            GameObject dataPlot = Instantiate(dataPlotPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            // Add it to the plots hierarchy and reset its scale
            dataPlot.transform.SetParent(plotsParent);
            dataPlot.transform.localScale = Vector3.one;
            // Initialize the plot using its attached script
            DataPlot dataPlotScript = dataPlot.GetComponent<DataPlot>();
            dataPlotScript.Init(this, dataPlotSkin, Vector2.one * 500, selectedIndices);
            // Add this script to the list of data plot scripts in this group
            dataPlots.Add(dataPlotScript);

            // Add this plot to the group of linked indices it is a part of
            linkedIndices.OnIndexAttributeChanged.AddListener(dataPlotScript.UpdateDataPoint);
            linkedIndices.OnAnyIndexAttributeChanged.AddListener(dataPlotScript.RefreshPlotGraphics);
            linkedIndices.OnIndicesReinitialized.AddListener(dataPlotScript.UpdateAllDataPoints);

            // Rearrange the plots
            ArrangePlots();

            // Invoke data plot added callback
            onDataPlotAdded.Invoke(dataPlotScript);

            return dataPlotScript;
        }

        /// <summary>
        /// Removes the specified plot from this group and destroys it.
        /// </summary>
        /// <param name="dataPlot">DataPlot script attached to the data plot GameObject that we wish to remove.</param>
        public void RemovePlot(DataPlot dataPlot)
        {
            if (dataPlots.Contains(dataPlot))
            {
                // Remove plot from linked indices listener group
                linkedIndices.OnIndexAttributeChanged.RemoveListener(dataPlot.UpdateDataPoint);
                linkedIndices.OnAnyIndexAttributeChanged.RemoveListener(dataPlot.RefreshPlotGraphics);
                linkedIndices.OnIndicesReinitialized.RemoveListener(dataPlot.UpdateAllDataPoints);

                // Remove and delete plot from this group
                dataPlots.Remove(dataPlot);
                Destroy(dataPlot.gameObject);

                // Reset linked indices if this was the final plot removed 
                // (and there are no other linked indices on attribute changed event subscribers)
                if (linkedIndices.OnIndexAttributeChanged.ListenerCount == 0)
                    linkedIndices.Reset();
                // Rearrange the plots
                ArrangePlots();
            }
        }

        /// <summary>
        /// Shows this data plot group and enables its plot creation buttons.
        /// </summary>
        public void Show()
        {
            if (!shown) 
            {
                // Apply canvas styling
                ApplyCanvasStyling();

                // Set the plots to active
                plotsParent.gameObject.SetActive(true);
                gameObject.SetActive(true);

                // Invoke show callback
                onShow.Invoke();

                // Enable plot creation buttons
                EnablePlotCreationButtons();

                // Adjust the size of the plots container
                plotsContainer.offsetMin = new Vector2(plotsContainerPadding.left, plotsContainerPadding.bottom);
                plotsContainer.offsetMax = -new Vector2(plotsContainerPadding.right, plotsContainerPadding.top);

                shown = true;
            }
        }

        /// <summary>
        /// Hides this data plot group and disables its plot creation buttons.
        /// </summary>
        public void Hide()
        {
            if (shown)
            {
                // Hide the plots
                plotsParent.gameObject.SetActive(false);
                gameObject.SetActive(false);

                // Invoke hide callback
                onHide.Invoke();

                // Disable plot creation buttons
                DisablePlotCreationButtons();
                
                shown = false;
            }
        }

        /// <summary>
        /// Checks if any linked indices are "selected" (aka highlighted) and enables/disables "new plot from selected" buttons accordingly.
        /// </summary>
        private void CheckAnySelected()
        {
            bool anySelected = linkedIndices.HighlightedCount > 0 ? true : false;
            foreach (PlotSetupContainer plotSetup in plotSetups)
            {
                plotSetup.newPlotFromSelectedButton.gameObject.SetActive(anySelected);
            }
        }

        /// <summary>
        /// Enables the plot creation buttons for this data plot group.
        /// </summary>
        private void EnablePlotCreationButtons()
        {
            foreach (PlotSetupContainer plotSetup in plotSetups)
            {
                plotSetup.newPlotButton.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Disables the plot creation buttons for this data plot group.
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
