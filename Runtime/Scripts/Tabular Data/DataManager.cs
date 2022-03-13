using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IVLab.Plotting
{
    /// <summary>
    /// Manages data, primarily by way of loading the <see cref="DataTable"/> used by the <see cref="DataPlotGroup"/>,
    /// and by maintaining a reference to the <see cref="LinkedIndices"/> related to that data, from which
    /// </summary>
    public class DataManager : MonoBehaviour
    {




        
        // /// <summary>
        // /// Prints the names of all the currently selected data points.
        // /// </summary>
        // public void PrintSelectedDataPointNames()
        // {
        //     string selectedIDs = "Selected Data Points (ID):\n\n";
        //     for (int i = 0; i < linkedIndices.Size; i++)
        //     {
        //         if (linkedIndices[i].Highlighted)
        //         {
        //             selectedIDs += dataTable.RowNames[i] + "\n";
        //         }
        //     }
        //     print(selectedIDs);
        // }

        // /// <summary>
        // /// Saves all the currently selected data points to a new csv.
        // /// </summary>
        // /// <param name="filePath">Path and name of csv file to save data in.</param>
        // /// <param name="saveRowNames">Whether or not to save a first column with row names.</param>
        // /// <remarks>
        // /// The csv this method creates can be used to create a data table with only that data!
        // /// </remarks>
        // public void SaveSelectedDataToCSV(string filePath, bool saveRowNames = true)
        // {
        //     using (StreamWriter streamWriter = new StreamWriter(filePath))
        //     {
        //         // Create the header
        //         string[] header;
        //         if (saveRowNames)
        //         {
        //             header = new string[dataTable.ColumnNames.Length + 1];
        //             header[0] = "Data Point ID";
        //             dataTable.ColumnNames.CopyTo(header, 1);
        //         }
        //         else
        //         {
        //             header = dataTable.ColumnNames;
        //         }
        //         // Write the header
        //         streamWriter.WriteLine(string.Join(",", header));

        //         // Write the rest of data in the table
        //         for (int i = 0; i < linkedIndices.Size; i++)
        //         {
        //             string[] row;
        //             if (saveRowNames)
        //             {
        //                 row = new string[dataTable.Width + 1];
        //                 row[0] = dataTable.RowNames[i];
        //             }
        //             else
        //             {
        //                 row = new string[dataTable.Width];
        //             }

        //             if (linkedIndices[i].Highlighted)
        //             {
        //                 for (int j = 0; j < dataTable.Width; j++)
        //                 {
        //                     if (saveRowNames)
        //                     {
        //                         row[j + 1] = dataTable.Data(i, j).ToString();
        //                     }
        //                     else
        //                     {
        //                         row[j] = dataTable.Data(i, j).ToString();
        //                     }
        //                 }

        //                 streamWriter.WriteLine(string.Join(",", row));
        //             }
        //         }
        //     }
        // }
    }
}
