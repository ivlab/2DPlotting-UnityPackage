using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IVLab.Plotting
{
    /// <summary>
    /// Manages multiple data managers, allowing for multiple data tables to be used in the 
    /// same visualization, each with it's own linked index space.
    /// </summary>
    public class MultiDataManager : MonoBehaviour
    {

        [SerializeField] private Transform dataManagerParent, dataPlotManagerParent;
        [SerializeField] private GameObject dataPlotManager;
        [SerializeField] private int dataCount = 1;
        [SerializeField] private List<DataManager> dataManagers = new List<DataManager>();
        [SerializeField] private List<DataPlotManager> dataPlotManagers = new List<DataPlotManager>();
        [SerializeField] private Button clickSelectionButton, rectSelectionButton, brushSelectionButton;
        [SerializeField] private Button newScatterPlotButton, newParallelCoordsPlotButton, newClusterPlotButton,
            selectedScatterPlotButton, selectedParallelCoordsPlotButton, selectedClusterPlotButton;
        [SerializeField] private GameObject scatterPlotPrefab, parallelCoordsPlotPrefab, clusterPlotPrefab;

        private int focusedData = 0;

        // Start is called before the first frame update
        void Awake()
        {
            dataManagers[0].Init();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(2))
            {
                AddData(new DataTable("iris"));
            }
            if (Input.GetMouseButtonDown(1))
            {
                FocusData((++focusedData) % dataManagers.Count);
            }
        }

        public void FocusData(int i)
        {
            print("focusing data: " + i);
            if (i >= dataManagers.Count || i < 0) return;
            for (int j = 0; j < dataManagers.Count; j++)
            {
                if (i == j)
                {
                    dataPlotManagers[j].PlotsParent.gameObject.SetActive(true);
                    dataPlotManagers[j].gameObject.SetActive(true);
                } else
                {
                    dataPlotManagers[j].PlotsParent.gameObject.SetActive(false);
                    dataPlotManagers[j].gameObject.SetActive(false);
                }
            }
        }

        private void OnValidate()
        {
            // Ensure the number of data sources doesn't drop below 1
            if (dataCount < 1) dataCount = 1;

            if (dataCount > dataManagers.Count)
            {

            }
        }

        /// <summary>
        /// Adds a new data source.
        /// </summary>
        /// <param name="dataTable"></param>
        public void AddData(DataTable dataTable, List<LinkedData> linkedData = null)
        {
            if (dataTable.IsEmpty())
            {
                Debug.Log("Data table is empty and will not be added as a new data source.");
                return;
            }

            // Add a new data plot manager
            GameObject newPlotManager = Instantiate(dataPlotManager, Vector3.zero, Quaternion.identity) as GameObject;
            DataPlotManager newPlotManagerScript = newPlotManager.GetComponent<DataPlotManager>();
            dataPlotManagers.Add(newPlotManagerScript);
            newPlotManager.transform.SetParent(dataPlotManagerParent);
            newPlotManager.transform.localPosition = Vector3.zero;
            newPlotManager.transform.localScale = Vector3.one;
            newPlotManager.name = dataTable.Name + " Data Plot Manager";

            // Wire up the data plot manager to work with creation and selection buttons
            clickSelectionButton.onClick.AddListener(delegate { newPlotManagerScript.SetCurrentSelectionMode(clickSelectionButton.GetComponent<ClickSelectionMode>()); });
            rectSelectionButton.onClick.AddListener(delegate { newPlotManagerScript.SetCurrentSelectionMode(rectSelectionButton.GetComponent<RectSelectionMode>()); });
            brushSelectionButton.onClick.AddListener(delegate { newPlotManagerScript.SetCurrentSelectionMode(brushSelectionButton.GetComponent<BrushSelectionMode>()); });

            newScatterPlotButton.onClick.AddListener(delegate { newPlotManagerScript.AddPlot(scatterPlotPrefab); });
            selectedScatterPlotButton.onClick.AddListener(delegate { newPlotManagerScript.AddPlot(scatterPlotPrefab); });
            newParallelCoordsPlotButton.onClick.AddListener(delegate { newPlotManagerScript.AddPlot(parallelCoordsPlotPrefab); });
            selectedParallelCoordsPlotButton.onClick.AddListener(delegate { newPlotManagerScript.AddPlot(parallelCoordsPlotPrefab); });
            newClusterPlotButton.onClick.AddListener(delegate { newPlotManagerScript.AddPlot(clusterPlotPrefab); });
            selectedClusterPlotButton.onClick.AddListener(delegate { newPlotManagerScript.AddPlot(clusterPlotPrefab); });     

            // Add a new data manager using this data table
            GameObject newDataManager = new GameObject(dataTable.Name + " Data Manager");
            DataManager newDataManagerScript = newDataManager.AddComponent<DataManager>();
            dataManagers.Add(newDataManagerScript);
            newDataManagerScript.Init(dataTable, newPlotManagerScript, linkedData);
            newDataManager.transform.SetParent(dataManagerParent);
            newDataManager.transform.localPosition = Vector3.zero;
            newDataManager.transform.localScale = Vector3.one;
        }
    }
}
