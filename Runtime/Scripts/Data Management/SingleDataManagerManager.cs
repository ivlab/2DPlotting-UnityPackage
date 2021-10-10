using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// Manages single data manager and data plat manager as opposed to <see cref="MultiDataManagerManager"/>.
    /// Used to create a simpler plotting setup in editor.
    /// </summary>
    public class SingleDataManagerManager : DataManagerManager
    {
        [Header("Data Configuration")]
        /// <summary>
        /// Single data manager this manager manages.
        /// </summary>
        public DataManager dataManager;
        /// <summary>
        /// Single data plot manager this manager manages.
        /// </summary>
        public DataPlotManager dataPlotManager;

        // Initialization
        void Awake()
        {
            // Initialize any data manager and plot manager that have been added in the inspector
            dataPlotManager.Init();
            dataManager.Init(this, dataPlotManager);

            // Set the selection mode to default
            SetCurrentGlobalSelectionMode(currentSelectionMode);

            // Ensure data plot manager is focused on
            dataPlotManager.Show();
        }

        /// <summary>
        /// Sets the selection mode of the data plot manager managed by this manager.
        /// </summary>
        /// <param name="selectionMode">Selection mode data plot manager will be set to use.</param>
        public override void SetCurrentGlobalSelectionMode(SelectionMode selectionMode)
        {
            currentSelectionMode = selectionMode;
            dataPlotManager.SetCurrentSelectionMode(selectionMode);
        }
    }
}
