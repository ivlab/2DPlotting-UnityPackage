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

        [Header("Data Plot Manager")]
        /// <summary> Data plot manager this data manager manages data for. </summary>
        [SerializeField] private DataPlotManager dataPlotManager;

        [Header("Additional Linked Data")]
        /// <summary> List of any additional linked data that should be updated along with the plots. </summary>
        [SerializeField] private List<LinkedData> linkedData;
        private LinkedIndices linkedIndices;
        private bool masking = false;

        /// <summary> Reference to the data table this class loaded. </summary>
        public DataTable DataTable { get => dataTable; }
        /// <summary> Collection of "data point" indices, linked with other key attributes. </summary>
        public LinkedIndices LinkedIndices { get => linkedIndices; }
        /// <summary> Toggles whether or not unhighlighted data should be masked. </summary>
        public bool Masking { get => masking; set => masking = value; }

        // Initialization
        void Start()
        {
            // Initialize the data table all plots controlled by this data manager will use
            dataTable = new DataTable(csvFilename);
            if (dataTable.Empty())
            {
                Debug.LogError("Data table is empty.");
            }

            // Initialize this as the data manager of the data plot manager
            dataPlotManager.DataManager = this;

            // Initialize the linked indices array based on number of data points (table height)
            linkedIndices = new LinkedIndices(dataTable.Height);
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
                        for (int k = 0; k < linkedData.Count; k++)
                        {
                            linkedData[k].UpdateDataPoint(i, linkedIndices[i]);
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
        /// Prints the IDs of all the selected data.
        /// </summary>
        private void PrintSelectedDataIDs()
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
    }
}
