using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace IVLab.Plotting
{
    [System.Serializable]
    /// <summary> 
    /// Container class used to make the linkage between Data Managers and 
    /// Data Plot Managers more strict in the inspector. Used by <see cref="MultiDataManager"/>.
    /// </summary>
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
        /// <summary> Default seleciton mode all data plot managers initialized by this manager
        /// are set to use. </summary>
        [SerializeField] private SelectionMode defaultSelectionMode;

        [Header("Dependencies/Data Stuff")]
        /// <summary> Default data plot manager. Used as template for instantiation. </summary>
        [SerializeField] private GameObject dataPlotManager;
        [SerializeField] private Transform dataManagerParent, dataPlotManagerParent;
        /// <summary> Dropdown used to select the active data source. </summary>
        [SerializeField] private TMP_Dropdown dataDropdown;
        
        /// <summary> Index of data currently in focus. </summary>
        private int focusedData = 0;
        private bool focusingData = false;

        void Awake()
        {
            // Initialize any data managers (with data plot managers) that have already been added in the inspector
            foreach (ManagerContainer managerContainer in managers)
            {
                managerContainer.dataManager.Init(this, managerContainer.dataPlotManager);
                managerContainer.dataPlotManager.Init();

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
            SetCurrentGlobalSelectionMode(defaultSelectionMode);

            // Initialize the data dropdown
            UpdateDataDropdown();
            dataDropdown.onValueChanged.AddListener(delegate { if (!focusingData) FocusData(dataDropdown.value); });

            // Focus on the first data source
            FocusData(0);
        }

        // Update is called once per frame
        void Update()
        {
            /*
            if (Input.GetMouseButtonDown(1))
            {
                AddData(new DataTable("iris"));
            }
            */
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
                if (i == j)
                {
                    managers[j].dataPlotManager.Show();
                    dataDropdown.value = i;
                    focusedData = i;
                } else
                {
                    managers[j].dataPlotManager.Hide();
                }
            }
            focusingData = false;
        }

        /// <summary>
        /// Adds a new data source.
        /// </summary>
        /// <param name="dataTable">New data source.</param>
        /// <param name="linkedData">List of any additional linked data that should be attached to the instantiated DataManager. </param>
        /*public void AddData(DataTable dataTable, List<LinkedData> linkedData = null)
        {
            if (dataTable.IsEmpty())
            {
                Debug.Log("Data table is empty and will not be added as a new data source.");
                return;
            }

            // Add a new data plot manager
            GameObject newPlotManager = Instantiate(dataPlotManager, Vector3.zero, Quaternion.identity) as GameObject;
            DataPlotManager newPlotManagerScript = newPlotManager.GetComponent<DataPlotManager>();
            newPlotManager.transform.SetParent(dataPlotManagerParent);
            newPlotManager.transform.localPosition = Vector3.zero;
            newPlotManager.transform.localScale = Vector3.one;
            newPlotManager.name = dataTable.Name + " Data Plot Manager";

            // Add a new data manager using this data table
            GameObject newDataManager = new GameObject(dataTable.Name + " Data Manager");
            DataManager newDataManagerScript = newDataManager.AddComponent<DataManager>();
            newDataManagerScript.Init(this, dataTable, newPlotManagerScript, linkedData);
            newDataManager.transform.SetParent(dataManagerParent);
            newDataManager.transform.localPosition = Vector3.zero;
            newDataManager.transform.localScale = Vector3.one;

            managers.Add(new ManagerContainer(newDataManagerScript, newPlotManagerScript));

            UpdateDataDropdown();
        }*/

        /// <summary>
        /// Sets the selection mode of all of the data plot managers.
        /// </summary>
        /// <param name="selectionMode">Selection mode all data plot managers will be set to use.</param>
        public void SetCurrentGlobalSelectionMode(SelectionMode selectionMode)
        {
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
                dataDropdown.options.Add(new TMP_Dropdown.OptionData() { text = managerContainer.dataManager.DataTable.Name });
            }
            focusedData %= managers.Count;
            dataDropdown.value = focusedData;
            dataDropdown.RefreshShownValue();
        }
    }
}
