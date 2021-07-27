using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace IVLab.Plotting
{
    /// <summary>
    /// An implementation of <see cref="ScatterPlot"/> that allows data points to be clustered
    /// together so that related data can be selected all at once.
    /// </summary>
    public class ClusterPlot : ScatterPlot
    {
        [Header("Cluster Plot Dependencies")]
        /// <summary> Prefab used to instantiate cluster toggles. </summary>
        [SerializeField] private GameObject togglePrefab;
        /// <summary> Parent transform of the cluster toggles. </summary>
        [SerializeField] private Transform clusterTogglesParent;
        /// <summary> Vertical spacing between toggles. </summary>
        private float clusterToggleSpacing = 30;
        /// <summary> Offset between edge of plot and toggles. </summary>
        private float clusterToggleOffset = 15;
        /// <summary>
        /// Replaces <see cref="DataPlot.dataTable"/> field to ensure that the cluster plot
        /// works with a properly formated "cluster" data table.
        /// </summary>
        private new ClusterDataTable dataTable;
        /// <summary> List of clusters that this plot manages. </summary>
        private List<Cluster> clusters = new List<Cluster>();
        /// <summary> Array of cluster toggles used to hide/show clusters. </summary>
        private Toggle[] clusterToggles = new Toggle[0];


        /// <summary>
        /// Initialize the plot by first initializing it as a scatter plot, 
        /// and then generating the list of clusters using the provided data table.
        /// </summary>
        /// <param name="dataPlotManager"> Manager of the plot: contains reference to the <see cref="DataManager"/> which controls the
        /// <see cref="DataTable"/> and <see cref="LinkedIndices"/> that the plot works from. </param>
        /// <param name="plotLayout"> Stores information about the size and padding of the plot. </param>
        /// <param name="dataPointIndices"> Array of data point indices the plot should display.
        /// If <c>null</c>, all data points will be displayed by default. </param>
        public override void Init(DataPlotManager dataPlotManager, PlotLayout plotLayout, int[] dataPointIndices = null)
        {
            // Set the data table
            dataTable = (ClusterDataTable)dataPlotManager.DataManager.DataTable;  // This cast should always work since the cluster plot creation button will only appear if a ClusterDataTable is in use

            // Construct the list of clusters from the data table
            foreach ((float, int, int, Color) cluster in dataTable.Clusters)
            {
                clusters.Add(new Cluster(cluster));
            }

            // Scatter plot initialization
            base.Init(dataPlotManager, plotLayout, dataPointIndices);

            // Initialize the cluster toggles for only the clusters that exist in the selected data point indices
            List<int> selectedClusters = new List<int>();
            foreach (int i in this.plottedDataPointIndices)
            {
                int clusterIdx = dataTable.DataIdxToClusterIdx(i);
                if (!selectedClusters.Contains(clusterIdx))
                    selectedClusters.Add(clusterIdx);
            }
            clusterToggles = new Toggle[selectedClusters.Count];
            for (int i = 0; i < selectedClusters.Count; i++)
            {
                // Instantiate the toggle
                Cluster cluster = clusters[selectedClusters[i]];
                GameObject toggleObject = Instantiate(togglePrefab, Vector3.zero, Quaternion.identity) as GameObject;
                // Position the toggle
                toggleObject.transform.SetParent(clusterTogglesParent);
                toggleObject.transform.localScale = Vector3.one;
                toggleObject.GetComponent<RectTransform>().anchoredPosition = centerOffset +
                    new Vector2(innerBounds.x / 2 + clusterToggleOffset, ((selectedClusters.Count - 1) / 2.0f - i) * clusterToggleSpacing);
                // Set the toggle's text and color
                Toggle toggle = toggleObject.GetComponent<Toggle>();
                toggle.GetComponentInChildren<TextMeshProUGUI>().text = dataTable.ColumnNames[0] + " " + cluster.Id;
                toggle.GetComponentInChildren<TextMeshProUGUI>().color = cluster.Color;
                toggle.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().color = cluster.Color;
                clusterToggles[i] = toggle;
                // Add a callback for when the toggle is... toggled
                int index = selectedClusters[i];
                toggle.onValueChanged.AddListener(delegate { ToggleCluster(index); });
            }
        }

        /// <summary>
        /// Toggles specified cluster's visibility.
        /// </summary>
        /// <param name="i"> Index into <see cref="clusters"/> list. </param>
        private void ToggleCluster(int i)
        {
            clusters[i].Enabled = !clusters[i].Enabled;
            Plot();
        }

        /// <summary>
        /// Sets the cluster plots size using <see cref="ScatterPlot.SetPlotSize(PlotLayout)"/> and
        /// positions cluster toggles.
        /// </summary>
        /// <param name="plotLayout"></param>
        protected override void SetPlotSize(PlotLayout plotLayout)
        {
            base.SetPlotSize(plotLayout);

            // Position the toggles
            for (int i = 0; i < clusterToggles.Length; i++)
            {
                clusterToggles[i].GetComponent<RectTransform>().anchoredPosition = centerOffset + 
                    new Vector2(innerBounds.x / 2 + clusterToggleOffset, ((clusterToggles.Length - 1) / 2.0f - i) * clusterToggleSpacing);
            }
        }

        /// <summary>
        /// Updates a specified data point based on its linked index attributes, only if it is
        /// already within the selected subset of points that this graph plots.
        /// </summary>
        /// <param name="index">Index of data point that needs to be updated.</param>
        /// <param name="indexAttributes">Current attributes of the data point.</param>
        public override void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes indexAttributes)
        {
            if (dataPointIndexMap.ContainsKey(index))
            {
                int i = dataPointIndexMap[index];
                if (indexAttributes.Masked)
                {
                    pointParticles[i].startColor = maskedColor;
                    // Make the point unselectable
                    pointIsHidden[i] = true;
                }
                else if (indexAttributes.Highlighted)
                {
                    pointParticles[i].startColor = highlightedColor;
                    // Hack to ensure highlighted particle appears in front of non-highlighted particles
                    pointParticles[i].position = new Vector3(pointParticles[i].position.x, pointParticles[i].position.y, -0.01f);
                    // Ensure the point is selectable
                    pointIsHidden[i] = false;
                }
                else
                {
                    pointParticles[i].startColor = clusters[dataTable.DataIdxToClusterIdx(index)].Color;
                    // Hack to ensure non-highlighted particle appears behind of highlighted particles
                    pointParticles[i].position = new Vector3(pointParticles[i].position.x, pointParticles[i].position.y, 0f);
                    // Ensure the point is selectable
                    pointIsHidden[i] = false;
                }
            }
        }

        /// <summary>
        /// Plots only the selected data in the data table based on the two currently selected columns.
        /// </summary>
        public override void Plot()
        {
            // Get the min/max values of the columns of interest
            float xMin = plottedDataPointMins[xColumnIdx];
            float xMax = plottedDataPointMaxes[xColumnIdx];
            float yMin = plottedDataPointMins[yColumnIdx];
            float yMax = plottedDataPointMaxes[yColumnIdx];
            // If scaleToOrigin is enabled, scale min/max values such that (0,0) is visible
            if (scaleToOrigin)
            {
                xMin = (xMin > 0) ? 0 : xMin;
                xMax = (xMax < 0) ? 0 : xMax;
                yMin = (yMin > 0) ? 0 : yMin;
                yMax = (yMax < 0) ? 0 : yMax;
            }
            // Generate adjusted "nice" min and max values before generating nice axes labels
            Vector2 axisSource = plotOuterRect.anchoredPosition - innerBounds / 2 + centerOffset;
            (xMin, xMax) = xAxisLabel.GenerateNiceMinMax(xMin, xMax);
            (yMin, yMax) = yAxisLabel.GenerateNiceMinMax(yMin, yMax);
            xAxisLabel.GenerateXAxisLabel(axisSource, innerBounds, true);
            yAxisLabel.GenerateYAxisLabel(axisSource, innerBounds, true, true);
            // Determine scale factors for the plot given "nice" min/max values
            float xScale = innerBounds.x / (xMax - xMin);
            float yScale = innerBounds.y / (yMax - yMin);
            // Get the origin position relative to the canvas given this scaling
            Vector2 origin = new Vector2(-(xMax + xMin) / 2.0f * xScale, -(yMax + yMin) / 2.0f * yScale) + centerOffset;
            // Position each data point
            for (int i = 0; i < plottedDataPointIndices.Length; i++)
            {
                // Get the index of the actual data point
                int dataPointIndex = plottedDataPointIndices[i];
                // If either the x or y coordinate of the point is NaN, 
                // flag it so that it will be unselectable and set its size to 0 so it will be invisible
                float xData = dataTable.Data(dataPointIndex, xColumnIdx);
                float yData = dataTable.Data(dataPointIndex, yColumnIdx);
                if (!clusters[dataTable.DataIdxToClusterIdx(dataPointIndex)].Enabled || float.IsNaN(xData) || float.IsNaN(yData))  // bit of a hack to hide disabled clusters
                {
                    pointIsHidden[i] = true;
                    pointParticles[i].startSize = 0;
                }
                // Otherwise just position and render it normally
                else
                {
                    pointIsHidden[i] = false;
                    // Determine the scaled position of the current point
                    float x = origin.x + xData * xScale;
                    float y = origin.y + yData * yScale;
                    // Save the position and then create a particle at that point
                    pointPositions[i] = new Vector2(x, y);
                    pointParticles[i].position = new Vector3(x, y, 0) * plotsCanvas.transform.localScale.y + Vector3.forward * pointParticles[i].position.z;  // scale by canvas size since particles aren't officially part of the canvas
                    pointParticles[i].startSize = pointSize * plotsCanvas.transform.localScale.y * Mathf.Max(outerBounds.x, outerBounds.y) / 300;
                }
            }
            // Render the points
            RefreshPlotGraphics();
        }

        // Since we ignore the first column (cluster ids) when plotting, we must adjust our
        // column indices by one.
        protected override void xDropdownUpdated() { xColumnIdx = xDropdown.value + 1; Plot(); }
        protected override void yDropdownUpdated() { yColumnIdx = yDropdown.value + 1; Plot(); }

        // Clears and then adds the column names to the x and y dropdowns.
        protected override void DropdownSetColumnNames()
        {
            // Clear and add new column names to dropdown selection menus
            xDropdown.options.Clear();
            yDropdown.options.Clear();
            // Skip the first column so as not to plot cluster ids.
            for (int i = 1; i < dataTable.ColumnNames.Length; i++)
            {
                string name = dataTable.ColumnNames[i];
                xDropdown.options.Add(new TMP_Dropdown.OptionData() { text = name });
                yDropdown.options.Add(new TMP_Dropdown.OptionData() { text = name });
            }
            // If possible, ensure currently selected value on both dropdowns is not the same
            if (dataTable.ColumnNames.Length > 2) yDropdown.value = 1;
            // Update currently selected column indices
            xColumnIdx = xDropdown.value + 1;
            yColumnIdx = yDropdown.value + 1;
            xAxisTitle.text = dataTable.ColumnNames[xColumnIdx];
            yAxisTitle.text = dataTable.ColumnNames[yColumnIdx];
        }


        /// <summary>
        /// Increments the column displayed on the x-axis and re-plots.
        /// </summary>
        public override void IncrementXColumn()
        {
            xColumnIdx = (++xColumnIdx) % dataTable.Width;
            if (xColumnIdx == 0) xColumnIdx = 1;
            xAxisTitle.text = dataTable.ColumnNames[xColumnIdx];
            Plot();
        }
        /// <summary>
        /// Decrements the column displayed on the x-axis and re-plots.
        /// </summary>
        public override void DecrementXColumn()
        {
            xColumnIdx = (--xColumnIdx + dataTable.Width) % dataTable.Width;
            if (xColumnIdx == 0) xColumnIdx = dataTable.Width - 1;
            xAxisTitle.text = dataTable.ColumnNames[xColumnIdx];
            Plot();
        }
        /// <summary>
        /// Increments the column displayed on the y-axis and re-plots.
        /// </summary>
        public override void IncrementYColumn()
        {
            yColumnIdx = (++yColumnIdx) % dataTable.Width;
            if (yColumnIdx == 0) yColumnIdx = 1;
            yAxisTitle.text = dataTable.ColumnNames[yColumnIdx];
            Plot();
        }
        /// <summary>
        /// Decrements the column displayed on the y-axis and re-plots.
        /// </summary>
        public override void DecrementYColumn()
        {
            yColumnIdx = (--yColumnIdx + dataTable.Width) % dataTable.Width;
            if (yColumnIdx == 0) yColumnIdx = dataTable.Width - 1;
            yAxisTitle.text = dataTable.ColumnNames[yColumnIdx];
            Plot();
        }

        // Selects the point within the point selection radius that is closest to the mouse selection position if "startSelection"
        // is true, and otherwise simply checks to see if the initially selected point is still within the point selection radius,
        // highlighting it if it is, unhighlighting it if it is not.
        public override void ClickSelection(Vector2 selectionPosition, SelectionMode.State selectionState)
        {
            // Square the selection radius to avoid square root computation in the future
            float selectionRadiusSqr = Mathf.Pow(clickSelectionRadius, 2);
            // If this is the initial click, i.e. selectionState is Start, find the closest particle to the mouse (within selection radius) 
            // and highlight it, unhighlighting all other points
            if (selectionState == SelectionMode.State.Start)
            {
                foreach (Cluster cluster in clusters)
                {
                    cluster.Highlighted = false;
                }
                // Reset clicked point index to -1 to reflect that no data points have been clicked
                clickedPointIdx = -1;
                // Set the current minimum distance (squared) between mouse and any point to the selection radius (squared)
                float minDistSqr = selectionRadiusSqr;
                // Iterate through all points to see if any are closer to the mouse than the current min distance,
                // updating the min distance every time a closer point is found
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (dataPointIndexMap.ContainsKey(i) && !pointIsHidden[dataPointIndexMap[i]])
                    {
                        float mouseToPointDistSqr = Vector2.SqrMagnitude(selectionPosition - pointPositions[dataPointIndexMap[i]]);
                        // Only highlight the point if it is truly the closest one to the mouse
                        if (mouseToPointDistSqr < selectionRadiusSqr && mouseToPointDistSqr < minDistSqr)
                        {
                            // Unhighlight the previous closest point to the mouse since it is no longer the closest
                            if (clickedPointIdx != -1)
                            {
                                linkedIndices[clickedPointIdx].Highlighted = false;
                            }
                            // Highlight the new closest point
                            minDistSqr = mouseToPointDistSqr;
                            clickedPointIdx = i;
                            linkedIndices[i].Highlighted = true;
                            continue;
                        }
                    }
                    linkedIndices[i].Highlighted = false;
                }
                // Based on the point that was clicked, if any, determine which cluster it is a part of and toggle
                // the highlighted flag for all of the data points in that cluster
                if (clickedPointIdx != -1)
                {
                    Cluster selectedCluster = clusters[dataTable.DataIdxToClusterIdx(clickedPointIdx)];
                    if (!selectedCluster.Highlighted && !pointIsHidden[dataPointIndexMap[clickedPointIdx]])
                    {
                        for (int i = selectedCluster.StartIdx; i < selectedCluster.EndIdx; i++)
                        {
                            if (dataPointIndexMap.ContainsKey(i))
                            {
                                linkedIndices[i].Highlighted = true;
                            }
                        }
                        selectedCluster.Highlighted = true;
                    }
                }
            }
            // If this is not the initial click but their was previously a point that was selected/clicked,
            // check to see if that point is still within the point selection radius of the current mouse selection position
            else if (clickedPointIdx != -1)
            {
                if (!pointIsHidden[dataPointIndexMap[clickedPointIdx]])
                {
                    float mouseToPointDistSqr = Vector2.SqrMagnitude(selectionPosition - pointPositions[dataPointIndexMap[clickedPointIdx]]);
                    if (mouseToPointDistSqr < selectionRadiusSqr)
                    {
                        // Only highlight the data point if it isn't masked
                        if (!linkedIndices[clickedPointIdx].Masked)
                        {
                            linkedIndices[clickedPointIdx].Highlighted = true;

                            Cluster selectedCluster = clusters[dataTable.DataIdxToClusterIdx(clickedPointIdx)];
                            if (!selectedCluster.Highlighted && dataPointIndexMap.ContainsKey(clickedPointIdx))
                            {
                                for (int i = selectedCluster.StartIdx; i < selectedCluster.EndIdx; i++)
                                {
                                    if (dataPointIndexMap.ContainsKey(i))
                                    {
                                        linkedIndices[i].Highlighted = true;
                                    }
                                }
                                selectedCluster.Highlighted = true;
                            }
                        }
                    }
                    else
                    {
                        linkedIndices[clickedPointIdx].Highlighted = false;

                        Cluster selectedCluster = clusters[dataTable.DataIdxToClusterIdx(clickedPointIdx)];
                        if (selectedCluster.Highlighted && dataPointIndexMap.ContainsKey(clickedPointIdx))
                        {
                            for (int i = selectedCluster.StartIdx; i < selectedCluster.EndIdx; i++)
                            {
                                if (dataPointIndexMap.ContainsKey(i))
                                {
                                    linkedIndices[i].Highlighted = false;
                                }
                            }
                            selectedCluster.Highlighted = false;
                        }
                    }
                }
            }
        }

        // Selects all data points inside the given selection rect.
        /*public override void RectSelection(RectTransform selectionRect)
        {    
            // Iterate through all data point indices
            for (int i = 0; i < linkedIndices.Size; i++)
            {
                // Only try to highlight this data point if is actually in the selection plotted by this plot
                if (selectedIndexDictionary.ContainsKey(i))
                {
                    // Must translate point position to anchored position space space for rect.Contains() to work
                    if (selectionRect.rect.Contains(pointPositions[selectedIndexDictionary[i]] - selectionRect.anchoredPosition))
                    {
                        // Only highlight the data point if it isn't masked
                        if (!linkedIndices[i].Masked)
                        {
                            linkedIndices[i].Highlighted = true;
                            foreach (Trial trial in trials)
                            {
                                if (!trial.highlighted && trial.Contains(i))
                                {
                                    for (int idx = trial.startIdx; idx < trial.endIdx; idx++)
                                    {
                                        if (selectedIndexDictionary.ContainsKey(idx))
                                        {
                                            linkedIndices[idx].Highlighted = true;
                                        }
                                    }
                                    trial.highlighted = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {

                    }
                }
                else
                {
                    linkedIndices[i].Highlighted = false;
                    foreach (Trial trial in trials)
                    {
                        if (trial.highlighted && trial.Contains(i) && selectedIndexDictionary.ContainsKey(i))
                        {
                            for (int idx = trial.startIdx; idx < trial.endIdx; idx++)
                            {
                                if (selectedIndexDictionary.ContainsKey(idx))
                                {
                                    linkedIndices[idx].Highlighted = false;
                                }
                            }
                            trial.highlighted = false;
                            break;
                        }
                    }
                }
            }
        }*/

        // Selects all data points in the cluster that the brush has already most selected.
        public override void BrushSelection(Vector2 prevBrushPosition, Vector2 brushDelta, SelectionMode.State selectionState)
        {
            // Call scatter plot's base brush selection method.
            base.BrushSelection(prevBrushPosition, brushDelta, selectionState);

            bool masked = false;
            for (int i = 0; i < linkedIndices.Size; i++)
            {
                if (linkedIndices[i].Masked)
                {
                    masked = true;
                    break;
                }
            }

            // If the selection is ending, determine which cluster should be selected
            // based on which one currently has the most selected points
            if (selectionState == SelectionMode.State.End && !masked)
            {
                foreach (Cluster cluster in clusters)
                {
                    cluster.NumSelected = 0;
                }

                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (linkedIndices[i].Highlighted)
                    {
                        clusters[dataTable.DataIdxToClusterIdx(i)].NumSelected++;
                    }
                }
                Cluster selectedCluster = clusters[0];
                for (int i = 1; i < clusters.Count; i++)
                {
                    if (clusters[i].NumSelected > selectedCluster.NumSelected)
                    {
                        selectedCluster = clusters[i];
                    }
                }
                if (selectedCluster.NumSelected == 0)
                {
                    return;
                }

                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (selectedCluster.Contains(i))
                    {
                        if (dataPointIndexMap.ContainsKey(i))
                        {
                            linkedIndices[i].Highlighted = true;
                        }
                    }
                    else
                    {
                        linkedIndices[i].Highlighted = false;
                    }
                }
                selectedCluster.Highlighted = true;
            }
        }

        /// <summary>
        /// A collection of (consecutive) data point indices that should be treated as related.
        /// </summary>
        class Cluster
        {
            /// <summary>
            /// Identifier for this cluster.
            /// </summary>
            public float Id { get; set; }
            /// <summary>
            /// Start index of the cluster in the data table (inclusive).
            /// </summary>
            public int StartIdx { get; set; }
            /// <summary>
            /// End index of the cluster in the data table (exclusive).
            /// </summary>
            public int EndIdx { get; set; }
            /// <summary>
            /// Whether or not this cluster is enabled and should be plotted/selected.
            /// </summary>
            public bool Enabled { get; set; } = true;
            /// <summary>
            /// Whether or not this entire cluster is currently highlighted.
            /// </summary>
            public bool Highlighted { get; set; } = false;
            /// <summary>
            /// Whether or not this entire cluster is currently masked.
            /// </summary>
            public bool Masked { get; set; } = false;  
            /// <summary>
            /// Used in brush selection to track the total number of currently selected points
            /// within this cluster.
            /// </summary>
            public int NumSelected { get; set; }
            /// <summary>
            /// Color this trial uses when plotted.
            /// </summary>
            public Color32 Color { get; set; }

            /// <summary>
            /// Constructs a cluster using its start (inclusive) and end (exclusive) indices.
            /// </summary>
            /// <param name="id">Cluster identifier.</param>
            /// <param name="startIdx">Cluster start index (inclusive).</param>
            /// <param name="endIdx">Cluster end index (exclusive).</param>
            /// <param name="color">Cluster color.</param>
            public Cluster(float id, int startIdx, int endIdx, Color color)
            {
                Id = id;
                StartIdx = startIdx;
                EndIdx = endIdx;
                Color = color;
            }

            /// <summary>
            /// Constructs a cluster using a tuple of its start (inclusive) and end (exclusive) indices.
            /// </summary>
            /// <param name="startEndIdx">"Tuple" representation of cluster in which first item is the id, the second is the
            /// start index (inclusive), the third is the end index (exclusive), and the fourth is the color of the cluster.</param>
            public Cluster((float, int, int, Color) startEndIdx)
            {
                (Id, StartIdx, EndIdx, Color) = startEndIdx;
            }

            /// <summary>
            /// Returns whether or not the cluster contains a particular index.
            /// </summary>
            public bool Contains(int index)
            {
                return (index >= StartIdx && index < EndIdx);
            }

            // Highlights all the indices in the trial.
            /*public void Highlight()
            {
                if (!highlighted)
                {
                    for (int i = startIdx; i < endIdx; i++)
                    {
                        if (selectedIndexDictionary.ContainsKey(i))
                        {
                            linkedIndices[i].Highlighted = false;
                        }
                    }
                    trial.highlighted = false;
                    break;
                }
            }

            // Unhighlights all the indices in the trial.
            public void Unhighlight()
            {
                if (highlighted)
                {

                }
            }*/
        }
    }
}
