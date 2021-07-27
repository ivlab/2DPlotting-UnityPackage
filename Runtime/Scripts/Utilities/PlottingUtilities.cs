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
        /// Takes an array of data tables and concatenates them into a single "cluster" data table which has an additional column
        /// identifying the index of the original data table each row came from, if possible, and returning null if not.
        /// </summary>
        /// <param name="dataTables">Array of data tables to be concatenated.</param>
        /// <returns>Single concatenated "cluster" data table, or null if the tables could not be concatenated.</returns>
        public static ClusterDataTable ClusterDataTables(DataTable[] dataTables, float[] clusterIds = null, Color[] clusterColors = null, string identifierColumnName = "Cluster")
        {
            // Return null if no data tables were given
            if (dataTables.Length == 0)
            {
                Debug.LogError("Failed to concatenate data tables:\nArray of data tables can not be empty.");
                return null;
            }

            // Initialize identifiers as 1 -> dataTables.Length if they were not given
            if (clusterIds == null)
            {
                clusterIds = new float[dataTables.Length];
                for (int t = 0; t < dataTables.Length; t++)
                {
                    clusterIds[t] = t + 1;
                }
            }

            // Return null if the number of the identifiers doesn't equal number of data tables given 
            else if (dataTables.Length != clusterIds.Length)
            {
                Debug.LogError("Failed to concatenate data tables:\nArray of data tables must be the same length as the array of identifiers.");
                return null;
            }

            // Ensure that all data tables have the same headers / columns
            string[] columnNames = dataTables[0].ColumnNames;
            for (int i = 1; i < dataTables.Length; i++)
            {
                string[] curColumnNames = dataTables[i].ColumnNames;
                if (columnNames.Length != curColumnNames.Length || !Enumerable.SequenceEqual(columnNames, curColumnNames))
                {
                    Debug.LogError("Failed to concatenate data tables:\nData tables must have the same number of columns and share column names.");
                    return null;
                }
            }

            // Construct a new data array combining all of the data tables' data arrays,
            // adding an additional column to indicate the index of the original data table
            int tableSize = 0;
            foreach (DataTable dataTable in dataTables)
            {
                tableSize += dataTable.Height * (dataTable.Width + 1);  // include size for additional column
            }
            float[] combinedData = new float[tableSize];
            int idx = 0;
            for (int t = 0; t < dataTables.Length; t++)
            {
                // Add the index of the data table used to the first column
                for (int d = 0; d < dataTables[t].Height; d++, idx++)
                {
                    combinedData[idx] = clusterIds[t];
                }
            }
            // Add the rest of the data
            for (int j = 0; j < columnNames.Length; j++)
            {
                for (int t = 0; t < dataTables.Length; t++)
                {
                    for (int i = 0; i < dataTables[t].Height; i++, idx++)
                    {
                        combinedData[idx] = dataTables[t].Data(i, j);
                    }
                }
            }

            // Concatenate the row names and combine the table names
            string[] combinedRowNames = { };
            string combinedTableName = "";
            foreach (DataTable dataTable in dataTables)
            {
                combinedRowNames = combinedRowNames.Concat(dataTable.RowNames).ToArray();
                combinedTableName += dataTable.Name + " / ";
            }
            combinedTableName = combinedTableName.Substring(0, combinedTableName.Length - 3);

            // Add the data table index (trials) column to the array of column names
            string[] combinedColumnNames = new string[columnNames.Length + 1];
            combinedColumnNames[0] = identifierColumnName;
            System.Array.Copy(columnNames, 0, combinedColumnNames, 1, columnNames.Length);

            // Return the newly constructed DataTable
            return new ClusterDataTable(combinedData, combinedRowNames, combinedColumnNames, combinedTableName, clusterColors);
        }
    }
}
