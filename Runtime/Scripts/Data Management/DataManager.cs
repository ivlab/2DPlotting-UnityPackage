using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IVLab.Plotting
{
    /// <summary>
    /// Manages data, primarily by way of loading the <see cref="DataTable"/> used by the <see cref="DataPlotManager"/>,
    /// and by maintaining a reference to the <see cref="LinkedIndices"/> related to that data, from which
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        [Header("Data Table Initialization")]
        /// <summary> Inspector visible toggle for whether or not to initialize data table from csv. </summary>
        [Tooltip("Whether or not to initialize the data table from a csv.")]
        [SerializeField] private bool initializeFromCsv = true;
        /// <summary> Whether or not to load the csv from the Resources folder or from full path name. </summary>
        [Tooltip("Whether or not to load the csv from the \"Resources\" folder, or from full path name.")]
#if UNITY_EDITOR
        [ConditionalHide("initializeFromCsv", true)]
#endif
        [SerializeField] private bool loadFromResources = true;
        /// <summary> Name of the csv file to pull data from, excluding ".csv". </summary>
        /// <remarks> File must reside in "Resources" folder. </remarks>
        [Tooltip("Name of the csv file to pull data from, excluding \".csv\". File must reside in \"Resources\" folder.")]
#if UNITY_EDITOR
        [ConditionalHide(new string[] { "initializeFromCsv", "loadFromResources" }, new bool[] { false, false }, true, false)]
#endif
        [SerializeField] private string csvFilename;
        /// <summary> Full path and name of csv file located anywhere. </summary>
        [Tooltip("Full path and name of csv file located anywhere.")]
#if UNITY_EDITOR
        [ConditionalHide(new string[] { "initializeFromCsv", "loadFromResources" }, new bool[] { false, true }, true, false)]
#endif
        [SerializeField] private string csvFullPathName;
        /// <summary> Inspector visible toggle for whether or not the csv has row names in its first column. </summary>
        [Tooltip("Whether or not the csv has row names in its first column.")]
#if UNITY_EDITOR
        [ConditionalHide("initializeFromCsv", true)]
#endif
        [SerializeField] private bool csvHasRowNames = true;
        /// <summary> Inspector visible toggle for whether or not the data table loaded
        /// from the csv is in "clusters".</summary>
        [Tooltip("Whether or not the data table loaded from the csv is \"clustered\".")]
#if UNITY_EDITOR
        [ConditionalHide("initializeFromCsv", true)]
#endif
        [SerializeField] private bool csvDataIsClustered = false;
        /// <summary> Allows the user to set a color palette for the cluster plot
        /// to use in the inspector. </summary>
        [Tooltip("Clusters will be colored by sampling evenly across this gradient.")]
#if UNITY_EDITOR
        [ConditionalHide(new string[] { "csvDataIsClustered", "initializeFromCsv" }, true, false)]
#endif
        [SerializeField] private Gradient clusterColorGradient;
        private DataTable dataTable;
        private DataPlotManager dataPlotManager;
        private DataManagerManager manager;
        private bool inFocus = false;
        private bool usingClusterDataTable = false;
        /// <summary> Current implementation of how masking should be toggled on and off for the data this manager manages. </summary>
        private MaskingToggle maskingToggle = new DefaultMaskingToggle();

        [Header("Additional Linked Data")]
        /// <summary> List of any additional linked data that should be updated along with the plots. </summary>
        [SerializeField] private List<LinkedData> linkedData;
        private LinkedIndices linkedIndices;
        private bool maskingData = false;
        private bool nothingMasked = true;

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
                usingClusterDataTable = dataTable?.GetType() == typeof(ClusterDataTable);
                // Log a warning if the data table is empty
                if (dataTable?.IsEmpty() == true)
                {
                    Debug.LogWarning("Data table is empty.");
                }
                else if (usingClusterDataTable && (((ClusterDataTable)dataTable)?.IsEmpty() == true))
                {
                    Debug.LogWarning("Cluster data table is empty.");
                }
                // Reinitialize the linked indices
                linkedIndices = new LinkedIndices(dataTable?.Height ?? 0);
                // Remove any currently linked plots
                for (int i = dataPlotManager.DataPlots.Count - 1; i >= 0; i--)
                {
                    dataPlotManager.RemovePlot(dataPlotManager.DataPlots[i]);
                }
                // Refresh data plot manager with this data table
                if (dataTable != null)
                    dataPlotManager.Refresh();
                // Update the data source dropdowns to reflect the table (if using multiple data sources)
                if (manager?.GetType() == typeof(MultiDataManagerManager))
                {
                    ((MultiDataManagerManager)manager)?.UpdateDataDropdown();  // (?. avoids null ref calling when Init sets the data table before the manager)
                }
            }
        }

        /// <summary> Data plot manager this data manager manages data for. </summary>
        public DataPlotManager DataPlotManager
        {
            get => dataPlotManager;
        }

        /// <summary>
        /// Whether or not the data table this data manager is pulling data from is a "cluster" data table.
        /// </summary>
        public bool UsingClusterDataTable { get => usingClusterDataTable; }

        /// <summary>
        /// Gets the linked indices associated with the current data table the manager is using.
        /// Can also set the linked indices, though this will cause all current plots to be removed
        /// to avoid linkage issues.
        /// </summary>
        public LinkedIndices LinkedIndices
        {
            get => linkedIndices;
            set
            {
                // Remove any currently linked plots
                for (int i = dataPlotManager.DataPlots.Count - 1; i >= 0; i--)
                {
                    dataPlotManager.RemovePlot(dataPlotManager.DataPlots[i]);
                }
                // Set the linked indices
                linkedIndices = value;
            }
        }

        /// <summary>
        /// Gets and sets the masking toggle implementation used by this data manager.
        /// </summary>
        public MaskingToggle MaskingToggle { get => maskingToggle; set => maskingToggle = value; }

        /// <summary>
        /// Gets and sets the list of external data linked to this data manager.
        /// </summary>
        public List<LinkedData> LinkedData
        {
            get => linkedData;
            set => linkedData = value;
        }

        /// <summary> Toggle for whether or not unhighlighted data is masked. </summary>
        public bool MaskingData { get => maskingData; set => maskingData = value; }
        /// <summary> Whether or not no data points are currently being masked. </summary>
        public bool NothingMasked { get => nothingMasked; set => nothingMasked = value; }
        /// <summary> Whether or not this data manager is in focus. </summary>
        public bool InFocus { get => inFocus; set => inFocus = value; }

        /// <summary>
        /// Initializes the data manager with the csv file given in the inspector.
        /// </summary>
        /// <param name="dataPlotManager">Data plot manager for this data manager to control.</param>
        /// <remarks>
        /// <b>Must</b> be called after <see cref="DataPlotManager.Init()"/>.
        /// </remarks>
        public void Init(DataManagerManager manager, DataPlotManager dataPlotManager)
        {
            // Initialize this as the data manager of the data plot manager
            this.dataPlotManager = dataPlotManager;
            this.dataPlotManager.DataManager = this;

            // Initialize the data table all plots controlled by this data manager will use
            // (using the DataTable property setter here will also automatically updated linked indices)
            if (initializeFromCsv)
            {
                // If it's a cluster table, color the clusters based on the given gradient
                if (csvDataIsClustered)
                {
                    ClusterDataTable table = loadFromResources ?
                        new ClusterDataTable(csvFilename, csvHasRowNames, loadFromResources) :
                        new ClusterDataTable(csvFullPathName, csvHasRowNames, loadFromResources);
                    int clusterCount = table.Clusters.Count;
                    for (int i = 0; i < clusterCount; i++)
                    {
                        table.Clusters[i].Color = clusterColorGradient.Evaluate(((float)i) / clusterCount);
                    }
                    DataTable = table;
                }
                // Otherwise just load it as a default data table
                else
                {
                    DataTable = loadFromResources ?
                        new DataTable(csvFilename, csvHasRowNames, loadFromResources) :
                        new DataTable(csvFullPathName, csvHasRowNames, loadFromResources);
                }
            }
            else
            {
                DataTable = null;
            }

            // Intentionally set the manager after everything else so that setting the DataTable
            // doesn't trigger UpdateDataDropdown
            this.manager = manager;
        }

        /// <summary>
        /// Initializes the data manager with a data table, data plot manager and linked data.
        /// </summary>
        /// <param name="dataTable">Data table for this data manager to use.</param>
        /// <param name="dataPlotManager">Data plot manager for this data manager to control.</param>
        /// <param name="linkedData">Data to be linked with the data table and data plots.</param>
        /// <remarks>
        /// <b>Must</b> be called after <see cref="DataPlotManager.Init()"/>.
        /// </remarks>
        public void Init(DataManagerManager manager, DataTable dataTable, DataPlotManager dataPlotManager, List<LinkedData> linkedData = null)
        {
            // Establish two-way dataPlotManager <-> dataManager references
            this.dataPlotManager = dataPlotManager;
            this.dataPlotManager.DataManager = this;

            // Initialize the data table all plots controlled by this data manager will use
            DataTable = dataTable;

            // Establish linked data
            this.linkedData = linkedData;

            // Intentionally set the manager after everything else so that setting the DataTable
            // doesn't trigger UpdateDataDropdown
            this.manager = manager;
        }

        void Update()
        {
            // Toggle masking when the space bar is pressed
            if (inFocus && Input.GetKeyDown(maskingToggle.toggleKey))
            {
                maskingToggle.ToggleMasking(this);
            }
        }

        void LateUpdate()
        {
            UpdateLinkedData();
        }

        /// <summary>
        /// Updates the linked data attached to this data manager based on the current state of the
        /// linked indices space, where "linked data" includes all of the data plots managed by the
        /// data plot manager, as well as any other additional linked data that has been attached.
        /// </summary>
        /// <remarks>
        /// It is recommended to call this method in Unity's LateUpdate() so as to ensure that all
        /// changes to linked indices for the current frame have been accounted for (since those
        /// should occur in Update())
        /// </remarks>
        private void UpdateLinkedData()
        {
            // Only update linked data if a data point's linked index attribute has been changed
            if (linkedIndices.LinkedAttributesChanged)
            {
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    // Only update the data points that have been changed
                    if (linkedIndices[i].LinkedAttributeChanged)
                    {
                        // Update changed data points in all plots
                        for (int j = 0; j < dataPlotManager.DataPlots.Count; j++)
                        {
                            dataPlotManager.DataPlots[j].UpdateDataPoint(i, linkedIndices[i]);
                        }
                        // Update any other linked data
                        if (linkedData != null)
                        {
                            for (int k = 0; k < linkedData.Count; k++)
                            {
                                linkedData[k].UpdateDataPoint(i, linkedIndices[i]);
                            }
                        }

                        linkedIndices[i].LinkedAttributeChanged = false;
                    }
                }

                // Update the graphics on all plots to reflect most recent changes
                for (int j = 0; j < dataPlotManager.DataPlots.Count; j++)
                {
                    dataPlotManager.DataPlots[j].RefreshPlotGraphics();
                }

                // Reset the linked attributes changed flag
                linkedIndices.LinkedAttributesChanged = false;
            }
        }
        
        /// <summary>
        /// Prints the names of all the currently selected data points.
        /// </summary>
        public void PrintSelectedDataPointNames()
        {
            string selectedIDs = "Selected Data Points (ID):\n\n";
            for (int i = 0; i < linkedIndices.Size; i++)
            {
                if (linkedIndices[i].Highlighted)
                {
                    selectedIDs += dataTable.RowNames[i] + "\n";
                }
            }
            print(selectedIDs);
        }

        /// <summary>
        /// Saves all the currently selected data points to a new csv.
        /// </summary>
        /// <param name="filePath">Path and name of csv file to save data in.</param>
        /// <param name="saveRowNames">Whether or not to save a first column with row names.</param>
        /// <remarks>
        /// The csv this method creates can be used to create a data table with only that data!
        /// </remarks>
        public void SaveSelectedDataToCSV(string filePath, bool saveRowNames = true)
        {
            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                // Create the header
                string[] header;
                if (saveRowNames)
                {
                    header = new string[dataTable.ColumnNames.Length + 1];
                    header[0] = "Data Point ID";
                    dataTable.ColumnNames.CopyTo(header, 1);
                }
                else
                {
                    header = dataTable.ColumnNames;
                }
                // Write the header
                streamWriter.WriteLine(string.Join(",", header));

                // Write the rest of data in the table
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    string[] row;
                    if (saveRowNames)
                    {
                        row = new string[dataTable.Width + 1];
                        row[0] = dataTable.RowNames[i];
                    }
                    else
                    {
                        row = new string[dataTable.Width];
                    }

                    if (linkedIndices[i].Highlighted)
                    {
                        for (int j = 0; j < dataTable.Width; j++)
                        {
                            if (saveRowNames)
                            {
                                row[j + 1] = dataTable.Data(i, j).ToString();
                            }
                            else
                            {
                                row[j] = dataTable.Data(i, j).ToString();
                            }
                        }

                        streamWriter.WriteLine(string.Join(",", row));
                    }
                }
            }
        }
    }
}
