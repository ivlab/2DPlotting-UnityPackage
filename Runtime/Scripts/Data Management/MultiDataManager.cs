using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace IVLab.Plotting
{
    /// <summary> 
    /// Container class used to make the linkage between Data Managers and 
    /// Data Plot Managers more strict in the inspector. Used by <see cref="MultiDataManager"/>.
    /// </summary>
    [System.Serializable]
    public class ManagerContainer
    {
        public DataManager dataManager;
        public DataPlotManager dataPlotManager;

        public ManagerContainer(DataManager dataManager, DataPlotManager dataPlotManager)
        {
            this.dataManager = dataManager;
            this.dataPlotManager = dataPlotManager;
        }
    }

    /// <summary>
    /// Manages multiple data managers, allowing for multiple data tables to be used in the 
    /// same visualization, each with it's own linked index space and set of data plots.
    /// </summary>
    public class MultiDataManager : MonoBehaviour
    {
        [Header("Data Configuration")]
        /// <summary> List of <see cref="ManagerContainer"/> objects, which contain both a <see cref="DataManager"/> 
        /// and a <see cref="DataPlotManager"/> since they are inextricably linked.</summary>
        [SerializeField] private List<ManagerContainer> managers = new List<ManagerContainer>();

        [Header("Selection")]
        /// <summary> Current selection mode all data plot managers initialized by this manager
        /// are set to use. </summary>
        [SerializeField] private SelectionMode currentSelectionMode;

        [Header("Dependencies/Data Stuff")]
        /// <summary> Default data plot manager. Used as template for instantiation. </summary>
        [SerializeField] private GameObject dataPlotManager;
        [SerializeField] private Transform dataManagerParent, dataPlotManagerParent;
        /// <summary> Dropdown used to select the active data source. </summary>
        [SerializeField] private TMP_Dropdown dataDropdown;
        
        /// <summary> Index of data currently in focus. </summary>
        private int focusedData = 0;
        /// <summary> Prevents dropdown callback from occuring when focusing data. </summary></summaryu>
        private bool focusingData = false;

        // Initialization
        void Awake()
        {
            // Initialize any data managers (with data plot managers) that have already been added in the inspector
            foreach (ManagerContainer managerContainer in managers)
            {
                managerContainer.dataPlotManager.Init();
                managerContainer.dataManager.Init(this, managerContainer.dataPlotManager);

                // Add callbacks to the data source dropdown to disable selection when the mouse is over it
                EventTrigger dropdownEventTrigger = dataDropdown.GetComponent<EventTrigger>();
                EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
                pointerEnter.eventID = EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener(delegate { managerContainer.dataPlotManager.DisableSelection(); });
                dropdownEventTrigger.triggers.Add(pointerEnter);
                EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                pointerExit.eventID = EventTriggerType.PointerExit;
                pointerExit.callback.AddListener(delegate { managerContainer.dataPlotManager.EnableSelection(); });
                dropdownEventTrigger.triggers.Add(pointerExit);
            }

            // Set the selection mode to default
            SetCurrentGlobalSelectionMode(currentSelectionMode);

            // Initialize the data dropdown
            UpdateDataDropdown();
            dataDropdown.onValueChanged.AddListener(delegate { if (!focusingData) FocusData(dataDropdown.value); });

            // Focus on the first data source
            FocusData(0);
        }

        /// <summary>
        /// Focuses the plotting view on a certain set of data and its related plots.
        /// </summary>
        /// <param name="i">Index to data manager that should be focused on.</param>
        public void FocusData(int i)
        {
            // Return if index out of bounds or already focused
            if (i >= managers.Count || i < 0) return;
            // Disable all data plots except for the focused
            focusingData = true;
            for (int j = 0; j < managers.Count; j++)
            {
                managers[j].dataPlotManager.Hide();
            }
            managers[i].dataPlotManager.Show();
            dataDropdown.value = i;
            focusedData = i;
            focusingData = false;
        }

        /// <summary>
        /// Forces a refocus on the current data in order to re-trigger plot manager hide/show methods.
        /// </summary>
        public void Refocus()
        {
            FocusData(focusedData);
        }

        /// <summary>
        /// Adds a new data source, and returns the data manager that is created.
        /// </summary>
        /// <param name="dataTable">New data source.</param>
        /// <param name="linkedData">List of any additional linked data that should be attached to the instantiated DataManager. </param>
        public DataManager AddDataSource(DataTable dataTable, List<LinkedData> linkedData = null)
        {
            // Add a new data plot manager
            GameObject newPlotManager = Instantiate(dataPlotManager, Vector3.zero, Quaternion.identity) as GameObject;
            DataPlotManager newPlotManagerScript = newPlotManager.GetComponent<DataPlotManager>();
            newPlotManagerScript.Init();
            newPlotManager.transform.SetParent(dataPlotManagerParent);
            newPlotManager.transform.localPosition = Vector3.zero;
            newPlotManager.transform.localScale = Vector3.one;
            newPlotManager.name = dataTable.Name + " Data Plot Manager";
            newPlotManagerScript.SetCurrentSelectionMode(currentSelectionMode);
            // Add callbacks to the data source dropdown to disable selection for this plot manager when the mouse is over it
            EventTrigger dropdownEventTrigger = dataDropdown.GetComponent<EventTrigger>();
            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener(delegate { newPlotManagerScript.DisableSelection(); });
            dropdownEventTrigger.triggers.Add(pointerEnter);
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener(delegate { newPlotManagerScript.EnableSelection(); });
            dropdownEventTrigger.triggers.Add(pointerExit);

            // Add a new data manager using this data table
            GameObject newDataManager = new GameObject(dataTable.Name + " Data Manager");
            DataManager newDataManagerScript = newDataManager.AddComponent<DataManager>();
            newDataManagerScript.Init(this, dataTable, newPlotManagerScript, linkedData);
            newDataManager.transform.SetParent(dataManagerParent);
            newDataManager.transform.localPosition = Vector3.zero;
            newDataManager.transform.localScale = Vector3.one;

            // Add the pair of managers to the list of managers
            managers.Add(new ManagerContainer(newDataManagerScript, newPlotManagerScript));

            // Update the dropdown to include this new data source
            UpdateDataDropdown();

            // Return the data manager that was created (which itself contains reference to the data plot manager)
            return newDataManagerScript;
        }

        /// <summary>
        /// Sets the selection mode of all of the data plot managers.
        /// </summary>
        /// <param name="selectionMode">Selection mode all data plot managers will be set to use.</param>
        public void SetCurrentGlobalSelectionMode(SelectionMode selectionMode)
        {
            currentSelectionMode = selectionMode;
            foreach (ManagerContainer managerContainer in managers)
            {
                managerContainer.dataPlotManager.SetCurrentSelectionMode(selectionMode);
            }
        }

        /// <summary>
        /// Increments focused data (with wrapping).
        /// </summary>
        public void IncrementFocusedData()
        {
            focusedData = (++focusedData) % managers.Count;
            FocusData(focusedData);
        }

        /// <summary>
        /// Decrements focused data (with wrapping).
        /// </summary>
        public void DecrementFocusedData()
        {
            focusedData = (--focusedData + managers.Count) % managers.Count;
            FocusData(focusedData);
        }

        /// <summary>
        /// Updates the data dropdown based on the each of the data managers.
        /// </summary>
        public void UpdateDataDropdown()
        {
            dataDropdown.options.Clear();
            foreach (ManagerContainer managerContainer in managers)
            {
                dataDropdown.options.Add(new TMP_Dropdown.OptionData() { text = managerContainer.dataManager.DataTable?.Name ?? "Null" });
            }
            focusedData %= managers.Count;
            dataDropdown.value = focusedData;
            dataDropdown.RefreshShownValue();
        }
    }
}
