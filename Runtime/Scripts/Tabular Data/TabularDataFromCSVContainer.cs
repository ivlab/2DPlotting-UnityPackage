using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Plotting
{
    public class TabularDataFromCSVContainer : TabularDataContainer
    {
        [Header("Data Table Initialization")]
        /// <summary> Whether or not to load the csv from the Resources folder or from full path name. </summary>
        [Tooltip("Whether or not to load the csv from the \"Resources\" folder, or from full path name.")]
        [SerializeField] private bool loadFromResources = true;
        /// <summary> Name of the csv file to pull data from, excluding ".csv". </summary>
        /// <remarks> File must reside in "Resources" folder. </remarks>
        [Tooltip("Name of the csv file to pull data from, excluding \".csv\". File must reside in \"Resources\" folder.")]
#if UNITY_EDITOR
        [ConditionalHide(new string[] { "loadFromResources" }, new bool[] { false }, true, false)]
#endif
        [SerializeField] private string csvFilename = "cars";
        /// <summary> Full path and name of csv file located anywhere. </summary>
        [Tooltip("Full path and name of csv file located anywhere.")]
#if UNITY_EDITOR
        [ConditionalHide(new string[] { "loadFromResources" }, new bool[] { true }, true, false)]
#endif
        [SerializeField] private string csvFullPathName;
        /// <summary> Inspector visible toggle for whether or not the csv has row names in its first column. </summary>
        [Tooltip("Whether or not the csv has row names in its first column.")]
        [SerializeField] private bool csvHasRowNames = true;
        /// <summary> Inspector visible toggle for whether or not the data table loaded
        /// from the csv is in "clusters".</summary>
        [Tooltip("Whether or not the data table loaded from the csv is \"clustered\".")]
        [SerializeField] private bool csvDataIsClustered = false;
        /// <summary> Allows the user to set a color palette for the cluster plot
        /// to use in the inspector. </summary>
        [Tooltip("Clusters will be colored by sampling evenly across this gradient.")]
#if UNITY_EDITOR
        [ConditionalHide(new string[] { "csvDataIsClustered" }, true, false)]
#endif
        [SerializeField] private Gradient clusterColorGradient;

        protected override void Init()
        {
            // If it's a cluster table, color the clusters based on the given gradient
            if (csvDataIsClustered)
            {
                ClusterDataTable table = loadFromResources ?
                    new ClusterDataTable(csvFilename, csvHasRowNames, loadFromResources) :
                    new ClusterDataTable(csvFullPathName, csvHasRowNames, loadFromResources);
                int clusterCount = table.Clusters.Count;
                for (int i = 0; i < clusterCount; i++)
                {
                    table.Clusters[i].Color = clusterColorGradient.Evaluate(((float)i) / clusterCount);
                }
                dataTable = table;
            }
            // Otherwise just load it as a default data table
            else
            {
                dataTable = loadFromResources ?
                    new DataTable(csvFilename, csvHasRowNames, loadFromResources) :
                    new DataTable(csvFullPathName, csvHasRowNames, loadFromResources);
            }

            initialized = true;
        }
    }
}
