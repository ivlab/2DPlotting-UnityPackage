using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    /// <summary>
    /// Abstract class providing template for the management of data managers.
    /// </summary>
    public abstract class DataManagerManager : MonoBehaviour
    {
        [Header("Selection")]
        /// <summary> 
        /// Current selection mode all data plot managers initialized by this manager are set to use. 
        /// </summary>
        [SerializeField] protected SelectionMode currentSelectionMode;

        /// <summary>
        /// Sets the selection mode of all of the data plot managers managed by this manager.
        /// </summary>
        /// <param name="selectionMode">Selection mode all data plot managers will be set to use.</param>
        public abstract void SetCurrentGlobalSelectionMode(SelectionMode selectionMode);
    }
}
