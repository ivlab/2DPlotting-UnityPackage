using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{   
    /// <summary>
    /// An abstract container for tabular data. Derived classes are tabular data container
    /// components which can be attached to GameObjects and then dragged and dropped
    /// directly in to a <see cref="DataPlotGroup"/> via the inspector.
    /// </summary>
    /// <remarks>
    /// Must construct data table in awake to ensure that it is initialized by the time 
    /// a <see cref="DataPlotGroup"/> uses it.
    /// </remarks>
    public abstract class TabularDataContainer : MonoBehaviour
    {
        protected bool initialized = false;
        protected DataTable dataTable;
        public DataTable DataTable {
            get
            {
                if (!initialized) Init();
                return dataTable;
            }
        }
        protected abstract void Init();
    }  
}
