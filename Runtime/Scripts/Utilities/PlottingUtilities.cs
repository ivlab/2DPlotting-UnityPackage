using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IVLab.Plotting {
    /// <summary>
    /// A collection of utilities intended to improve the usability of the 2DPlotting package.
    /// </summary>
    public class PlottingUtilities
    {
        /// <summary>
        /// Global access to constants used in the 2D plotting package.
        /// </summary>
        public static class Consts
        {
            public const string PlotsSortingLayerName = "2DPlots";

            public const string PlotsLayerName = "2DPlots";
        }

        /// <summary>
        /// Takes an array of data tables and clusters/concatenates them vertically into a single "cluster" data table if possible, returning null if not.
        /// This "cluster" data table has an additional column identifying the data table each row came from.
        /// </summary>
        /// <param name="tableDatas">Array of data tables to be concatenated.</param>
        /// <param name="clusterIds">Optional array of identifiers for each cluster.</param>
        /// <param name="clusterColors">Optional array of colors for each cluster</param>
        /// <param name="identifierColumnName">Optional name for the identifier column that will be generated. </param>
        /// <returns>Single concatenated "cluster" data table, or null if the tables could not be concatenated. </returns>
        public static ClusterTableData ClusterTableDatas(TableData[] tableDatas, float[] clusterIds = null, Color[] clusterColors = null, string identifierColumnName = "Cluster")
        {
            // Return null if no data tables were given
            if (tableDatas.Length == 0)
            {
                Debug.LogError("Failed to concatenate data tables:\nArray of data tables can not be empty.");
                return null;
            }

            // Initialize identifiers as 1 -> tableDatas.Length if they were not given
            if (clusterIds == null)
            {
                clusterIds = new float[tableDatas.Length];
                for (int t = 0; t < tableDatas.Length; t++)
                {
                    clusterIds[t] = t + 1;
                }
            }

            // Return null if the number of the identifiers doesn't equal number of data tables given 
            else if (tableDatas.Length != clusterIds.Length)
            {
                Debug.LogError("Failed to concatenate data tables:\nArray of data tables must be the same length as the array of identifiers.");
                return null;
            }

            // Ensure that all data tables have the same headers / columns
            string[] columnNames = tableDatas[0].ColumnNames;
            for (int i = 1; i < tableDatas.Length; i++)
            {
                string[] curColumnNames = tableDatas[i].ColumnNames;
                if (columnNames.Length != curColumnNames.Length || !Enumerable.SequenceEqual(columnNames, curColumnNames))
                {
                    Debug.LogError("Failed to concatenate data tables:\nData tables must have the same number of columns and share column names.");
                    return null;
                }
            }

            // Construct a new data array combining all of the data tables' data arrays,
            // adding an additional column to indicate the index of the original data table
            int tableSize = 0;
            foreach (TableData tableData in tableDatas)
            {
                tableSize += tableData.Height * (tableData.Width + 1);  // include size for additional column
            }
            float[] combinedData = new float[tableSize];
            int idx = 0;
            for (int t = 0; t < tableDatas.Length; t++)
            {
                // Add the index of the data table used to the first column
                for (int d = 0; d < tableDatas[t].Height; d++, idx++)
                {
                    combinedData[idx] = clusterIds[t];
                }
            }
            // Add the rest of the data
            for (int j = 0; j < columnNames.Length; j++)
            {
                for (int t = 0; t < tableDatas.Length; t++)
                {
                    for (int i = 0; i < tableDatas[t].Height; i++, idx++)
                    {
                        combinedData[idx] = tableDatas[t].Data(i, j);
                    }
                }
            }

            // Concatenate the row names and combine the table names
            string[] combinedRowNames = { };
            string combinedTableName = "";
            foreach (TableData tableData in tableDatas)
            {
                combinedRowNames = combinedRowNames.Concat(tableData.RowNames).ToArray();
                combinedTableName += tableData.Name + " / ";
            }
            combinedTableName = combinedTableName.Substring(0, combinedTableName.Length - 3);

            // Add the data table index (trials) column to the array of column names
            string[] combinedColumnNames = new string[columnNames.Length + 1];
            combinedColumnNames[0] = identifierColumnName;
            System.Array.Copy(columnNames, 0, combinedColumnNames, 1, columnNames.Length);

            // Return the newly constructed TableData
            return new ClusterTableData(combinedData, combinedRowNames, combinedColumnNames, combinedTableName, clusterColors);
        }

        /// <summary>
        /// Applies "plots" layer to a gameObject and all of its children.
        /// </summary>
        /// <param name="obj">Parent gameObject.</param>
        /// <param name="layer">Layer id.</param>
        public static void ApplyPlotsLayersRecursive(GameObject gameObject)
        {
            gameObject.layer = LayerMask.NameToLayer(Consts.PlotsLayerName);
            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component != null)
                {
                    var field = component.GetType().GetProperty("sortingLayerID");
                    field?.SetValue(component, SortingLayer.NameToID(Consts.PlotsSortingLayerName));
                }
            }
            foreach (Transform child in gameObject.transform)
            {
                ApplyPlotsLayersRecursive(child.gameObject);
            }
        }
    }
}
