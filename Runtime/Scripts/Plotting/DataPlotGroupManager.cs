using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

namespace IVLab.Plotting
{
    /// <summary>
    /// Manages data plot groups, allowing for multiple data plot groups to exist at the same time.
    /// </summary>
    public class DataPlotGroupManager : MonoBehaviour
    {
        /// <summary> Data plot groups this manages. </summary>
        [SerializeField] private List<DataPlotGroup> dataPlotGroups = new List<DataPlotGroup>();
        [Header("Interaction")]
        /// <summary> Controls selection and masking for all plot groups this manages. </summary>
        [SerializeField] private PlotInteractionController plotInteractionController;
        [Header("Callbacks")]
        /// <summary> Callback for when a new data source is added. </summary>
        [SerializeField] private UnityEvent onDataSourceAdded;
        [Header("Dependencies")]
        /// <summary> Default data plot group, used as template to instantiate new ones. </summary>
        [SerializeField] private GameObject defaultDataPlotGroup;
        
        private int focusedDataPlotGroup = 0;

        /// <summary> Index of data plot group currently in focus. </summary>
        public int FocusedDataPlotGroup { get => focusedDataPlotGroup; }

        // Initialization
        void Start()
        {
            // Initialize any data plot groups that have already been added in the inspector
            foreach (DataPlotGroup dataPlotGroup in dataPlotGroups)
            {
                dataPlotGroup.Init();
            }

            // Focus on the first data source
            FocusDataPlotGroup(0);
        }

        /// <summary>
        /// Focuses the plotting view on a certain group of data plots.
        /// </summary>
        /// <param name="i">Index to data plot group that should be focused on.</param>
        public void FocusDataPlotGroup(int i)
        {
            // Return if index out of bounds or already focused
            if (i >= dataPlotGroups.Count || i < 0) return;
            // Disable all data plots except for the focused
            for (int j = 0; j < dataPlotGroups.Count; j++)
            {
                if (j != i)
                    dataPlotGroups[j].Hide();
            }
            focusedDataPlotGroup = i;
            plotInteractionController.ActiveDataPlotManager = dataPlotGroups[i];
            dataPlotGroups[i].Show();
        }

        /// <summary>
        /// Adds a new data source, and returns the data plot group that is created.
        /// </summary>
        /// <param name="dataTable">New data source.</param>
        public DataPlotGroup AddDataSource(DataTable dataTable)
        {
            // Add a new data plot group
            GameObject newDataPlotGroup = GameObject.Instantiate(defaultDataPlotGroup, Vector3.zero, Quaternion.identity) as GameObject;
            DataPlotGroup newDataPlotGroupScript = newDataPlotGroup.GetComponent<DataPlotGroup>();
            newDataPlotGroupScript.Init(dataTable);
            newDataPlotGroup.transform.SetParent(this.transform);
            newDataPlotGroup.transform.localPosition = Vector3.zero;
            newDataPlotGroup.transform.localScale = Vector3.one;
            newDataPlotGroup.name = dataTable.Name + " Data Plot Group";
            PlottingUtilities.ApplyPlotsLayersRecursive(newDataPlotGroup);
            dataPlotGroups.Add(newDataPlotGroupScript);

            // Invoke the data source added callback
            onDataSourceAdded.Invoke();

            // Return the data plot group that was created
            return newDataPlotGroupScript;
        }

        /// <summary>
        /// Increments focused data plot group (with wrapping).
        /// </summary>
        public void IncrementFocusedDataPlotGroup()
        {
            focusedDataPlotGroup = (++focusedDataPlotGroup) % dataPlotGroups.Count;
            FocusDataPlotGroup(focusedDataPlotGroup);
        }

        /// <summary>
        /// Decrements focused data plot group (with wrapping).
        /// </summary>
        public void DecrementFocusedDataPlotGroup()
        {
            focusedDataPlotGroup = (--focusedDataPlotGroup + dataPlotGroups.Count) % dataPlotGroups.Count;
            FocusDataPlotGroup(focusedDataPlotGroup);
        }
    }
}
