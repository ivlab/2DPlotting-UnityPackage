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
        [Header("Data Table Configuration")]
        /// <summary> Name of csv file to pull data from, excluding ".csv". </summary>
        [SerializeField] private string csvFilename;
        private DataTable dataTable;
        private DataPlotManager dataPlotManager;
        private MultiDataManager manager;
        private bool usingClusterDataTable = false;

        [Header("Additional Linked Data")]
        /// <summary> List of any additional linked data that should be updated along with the plots. </summary>
        [SerializeField] private List<LinkedData> linkedData;
        private LinkedIndices linkedIndices;
        private bool masking = false;

        /// <summary> 
        /// Gets the data table this data manager is currently using. Can also be used to set
        /// the data table, which automatically causes <see cref="LinkedIndices"/> to reinitialize
        /// and deletes any linked plots.
        /// </summary>
        public DataTable DataTable {
            get => dataTable;
            set
            {
                // Set the new data table
                dataTable = value;
                usingClusterDataTable = dataTable.GetType() == typeof(ClusterDataTable);
                if (dataTable.IsEmpty()) { Debug.LogError("Data table is empty."); }
                // Update the data source dropdowns to reflect the table
                manager?.UpdateDataDropdown();  // (?. avoids null ref calling when Init sets the data table before the manager)
                manager?.Refocus();
                // Reinitialize the linked indices
                linkedIndices = new LinkedIndices(dataTable.Height);
                // Remove any currently linked plots
                for (int i = dataPlotManager.DataPlots.Count - 1; i >= 0; i--)
                {
                    dataPlotManager.RemovePlot(dataPlotManager.DataPlots[i]);
                }
            }
        }

        /// <summary> Data plot manager this data manager manages data for. </summary>
        public DataPlotManager DataPlotManager
        {
            get => dataPlotManager;
        }

        /// <summary>
        /// Gets and sets the manager that manages this data manager.
        /// </summary>
        public MultiDataManager MultiDataManager
        {
            get => manager;
            set => manager = value;
        }

        /// <summary>
        /// Whether or not the data table this data manager is pulling data from is a "cluster" data table.
        /// </summary>
        public bool UsingClusterDataTable { get => usingClusterDataTable; }

        /// <summary>
        /// Gets the linked indices associated with the current data table the manager is using.
        /// Can also set the linked indices, but the new linked indices must be the same size as
        /// the old.
        /// </summary>
        public LinkedIndices LinkedIndices {
            get => linkedIndices;
            /*set
            {
                // Only update the linked indices if the new value given is of the same size as the current
                if (value.Size == linkedIndices.Size)
                {
                    linkedIndices = value;
                }
                else
                {
                    Debug.Log("New linked indices must be of same size as old.");
                }
            }*/
        }

        /// <summary>
        /// Gets and sets the list of external data linked to this data manager.
        /// </summary>
        public List<LinkedData> LinkedData
        {
            get => linkedData;
            set => linkedData = value;
        }

        /// <summary> Toggle for whether or not unhighlighted data should be masked. </summary>
        public bool Masking { get => masking; set => masking = value; }

        /// <summary>
        /// Initializes the data manager with the csv file given in the inspector.
        /// </summary>
        /// <param name="dataPlotManager">Data plot manager for this data manager to control.</param>
        /// /// <remarks>
        /// <b>Must</b> be called before <see cref="DataPlotManager.Init()"/>.
        /// </remarks>
        public void Init(MultiDataManager manager, DataPlotManager dataPlotManager)
        {
            // Initialize this as the data manager of the data plot manager
            this.dataPlotManager = dataPlotManager;
            this.dataPlotManager.DataManager = this;

            // Initialize the data table all plots controlled by this data manager will use
            // (using the DataTable property setter here will also automatically updated linked indices)
            DataTable = new DataTable(csvFilename);

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
        /// <b>Must</b> be called before <see cref="DataPlotManager.Init()"/>.
        /// </remarks>
        public void Init(MultiDataManager manager, DataTable dataTable, DataPlotManager dataPlotManager, List<LinkedData> linkedData = null)
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleMasking();
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
        /// Toggles masking of unhighlighted data points.
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

        public void SaveSelectedDataToCSV()
        {

        }

        public void LoadDataFromCSV()
        {

        }
    }
}
