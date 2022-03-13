using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

        [Header("Dependencies")]
        [SerializeField] private PlotInteractionController plotInteractionController;
        /// <summary> Default data plot group object. Used as template for instantiation. </summary>
        [SerializeField] private GameObject dataPlotGroup;
        [SerializeField] private Transform dataPlotGroupsParent;
        /// <summary> Dropdown used to select the active data plot group. </summary>
        [SerializeField] private TMP_Dropdown dataPlotGroupDropdown = null;
        
        /// <summary> Index of data plot group currently in focus. </summary>
        private int focusedDataPlotGroup = 0;
        /// <summary> Prevents dropdown callback from occuring when focusing data. </summary>
        private bool isFocusing = false;

        // Initialization
        void Start()
        {
            // Initialize any data plot gruops that have already been added in the inspector
            foreach (DataPlotGroup dataPlotGroup in dataPlotGroups)
            {
                dataPlotGroup.Init(this);
            }

            // Initialize the dropdown
            UpdateDataDropdown();
            if (dataPlotGroupDropdown != null)
                dataPlotGroupDropdown.onValueChanged.AddListener(delegate { if (!isFocusing) FocusDataPlotGroup(dataPlotGroupDropdown.value); });

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
            isFocusing = true;
            for (int j = 0; j < dataPlotGroups.Count; j++)
            {
                dataPlotGroups[j].Hide();
            }
            dataPlotGroups[i].Show();
            if (dataPlotGroupDropdown != null)
                dataPlotGroupDropdown.value = i;
            focusedDataPlotGroup = i;
            plotInteractionController.ActiveDataPlotManager = dataPlotGroups[i];
            isFocusing = false;
        }

        /// <summary>
        /// Adds a new data source, and returns the data plot group that is created.
        /// </summary>
        /// <param name="dataTable">New data source.</param>
        public DataPlotGroup AddDataSource(DataTable dataTable)
        {
            // Add a new data plot group
            GameObject newPlotGroup = GameObject.Instantiate(dataPlotGroup, Vector3.zero, Quaternion.identity) as GameObject;
            DataPlotGroup newPlotGroupScript = newPlotGroup.GetComponent<DataPlotGroup>();
            newPlotGroupScript.Init(this, dataTable);
            newPlotGroup.transform.SetParent(dataPlotGroupsParent);
            newPlotGroup.transform.localPosition = Vector3.zero;
            newPlotGroup.transform.localScale = Vector3.one;
            newPlotGroup.name = dataTable.Name + " Data Plot Group";
            PlottingUtilities.ApplyPlotsLayersRecursive(newPlotGroup);
            dataPlotGroups.Add(newPlotGroupScript);

            // Update the dropdown to include this new data plot group
            UpdateDataDropdown();

            // Return the data plot group that was created
            return newPlotGroupScript;
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

        /// <summary>
        /// Updates the data dropdown based on the each of the data plot groups.
        /// </summary>
        public void UpdateDataDropdown()
        {
            if (dataPlotGroupDropdown != null)
            {
                dataPlotGroupDropdown.options.Clear();
                foreach (DataPlotGroup dataPlotGroup in dataPlotGroups)
                {
                    dataPlotGroupDropdown.options.Add(new TMP_Dropdown.OptionData() { text = dataPlotGroup.DataTable?.Name });
                }
                focusedDataPlotGroup %= dataPlotGroups.Count;
                dataPlotGroupDropdown.value = focusedDataPlotGroup;
                dataPlotGroupDropdown.RefreshShownValue();
            }
        }
    }
}
