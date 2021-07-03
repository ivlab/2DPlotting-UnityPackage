using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace IVLab.Plotting
{
    /// <summary>
    /// An implementation of <see cref="ScatterPlot"/> that allows data points to be clustered
    /// together so that related data can be selected all at once.
    /// </summary>
    public class ClusterPlot : ScatterPlot
    {
        /// <summary> List of clusters that this plot manages. </summary>
        private List<Cluster> clusters = new List<Cluster>();

        /// <summary>
        /// Initialize the plot by first initializing it as a scatter plot, 
        /// and then generating the list of clusters using the provided data table.
        /// </summary>
        /// <param name="dataPlotManager"> Manager of the plot: contains references to the <see cref="DataTable"/> and 
        /// <see cref="LinkedIndices"/> that the plot works from. </param>
        /// <param name="outerBounds"> Size to set the outer bounds of the plot. </param>
        /// <param name="selectedDataPointIndices"> Array of data point indices the plot should display.
        /// If <c>null</c>, all data points will be displayed by default. </param>
        public override void Init(DataPlotManager dataPlotManager, Vector2 outerBounds, int[] selectedDataPointIndices = null)
        {
            // Scatter plot initialization
            base.Init(dataPlotManager, outerBounds, selectedDataPointIndices);
            // Construct the list of clusters from the data table
            // (assuming the table is formatted so that the first column consists exclusively of ordered cluster id #s)
            int startIdx = 0;
            int clusterID = (int)dataTable.Data[0][0];  // NOTE: This will end poorly if data table is empty
            for (int i = 1; i < dataTable.Height; i++)
            {
                if (dataTable.Data[0][i] != clusterID)
                {
                    clusters.Add(new Cluster(startIdx, i));
                    startIdx = i;
                    clusterID = (int)dataTable.Data[0][i];
                }
            }
            clusters.Add(new Cluster(startIdx, dataTable.Height));
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
            // Skip the first column so as not to plot trial ids.
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
                foreach (Cluster trial in clusters)
                {
                    trial.highlighted = false;
                }
                // Reset clicked point index to -1 to reflect that no data points have been clicked
                clickedPointIdx = -1;
                // Set the current minimum distance (squared) between mouse and any point to the selection radius (squared)
                float minDistSqr = selectionRadiusSqr;
                // Iterate through all points to see if any are closer to the mouse than the current min distance,
                // updating the min distance every time a closer point is found
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (selectedIndexDictionary.ContainsKey(i))
                    {
                        float mouseToPointDistSqr = Vector2.SqrMagnitude(selectionPosition - pointPositions[selectedIndexDictionary[i]]);
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
                            // Only highlight the data point if it isn't masked
                            if (!linkedIndices[i].Masked)
                            {
                                linkedIndices[i].Highlighted = true;
                            }
                            continue;
                        }
                    }
                    linkedIndices[i].Highlighted = false;
                }
                // Based on the point that was clicked, if any, determine which cluster it is a part of and toggle
                // the highlighted flag for all of the data points in that cluster
                if (clickedPointIdx != -1)
                {
                    foreach (Cluster trial in clusters)
                    {
                        if (!trial.highlighted && trial.Contains(clickedPointIdx))
                        {
                            for (int i = trial.startIdx; i < trial.endIdx; i++)
                            {
                                if (selectedIndexDictionary.ContainsKey(i))
                                {
                                    linkedIndices[i].Highlighted = true;
                                }
                            }
                            trial.highlighted = true;
                            break;
                        }
                    }
                }
            }
            // If this is not the initial click but their was previously a point that was selected/clicked,
            // check to see if that point is still within the point selection radius of the current mouse selection position
            else if (clickedPointIdx != -1)
            {
                float mouseToPointDistSqr = Vector2.SqrMagnitude(selectionPosition - pointPositions[selectedIndexDictionary[clickedPointIdx]]);
                if (mouseToPointDistSqr < selectionRadiusSqr)
                {
                    // Only highlight the data point if it isn't masked
                    if (!linkedIndices[clickedPointIdx].Masked)
                    {
                        linkedIndices[clickedPointIdx].Highlighted = true;

                        foreach (Cluster trial in clusters)
                        {
                            if (!trial.highlighted && trial.Contains(clickedPointIdx) && selectedIndexDictionary.ContainsKey(clickedPointIdx))
                            {
                                for (int i = trial.startIdx; i < trial.endIdx; i++)
                                {
                                    if (selectedIndexDictionary.ContainsKey(i))
                                    {
                                        linkedIndices[i].Highlighted = true;
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
                    linkedIndices[clickedPointIdx].Highlighted = false;

                    foreach (Cluster trial in clusters)
                    {
                        if (trial.highlighted && trial.Contains(clickedPointIdx) && selectedIndexDictionary.ContainsKey(clickedPointIdx))
                        {
                            for (int i = trial.startIdx; i < trial.endIdx; i++)
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

        // Selects all data points in the trial that the brush has already most selected.
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

            // If the selection is ending, determine which trial should be selected
            // based on which one currently has the most selected points
            if (selectionState == SelectionMode.State.End && !masked)
            {
                foreach (Cluster trial in clusters)
                {
                    trial.numSelected = 0;
                }

                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (linkedIndices[i].Highlighted)
                    {
                        foreach (Cluster trial in clusters)
                        {
                            if (trial.Contains(i))
                            {
                                trial.numSelected++;
                                break;
                            }
                        }
                    }
                }
                Cluster selectedTrial = clusters[0];
                for (int i = 1; i < clusters.Count; i++)
                {
                    if (clusters[i].numSelected > selectedTrial.numSelected)
                    {
                        selectedTrial = clusters[i];
                    }
                }
                if (selectedTrial.numSelected == 0)
                {
                    return;
                }

                for (int i = 0; i < linkedIndices.Size; i++)
                {

                    if (selectedTrial.Contains(i))
                    {
                        if (selectedIndexDictionary.ContainsKey(i))
                        {
                            linkedIndices[i].Highlighted = true;
                        }
                    }
                    else
                    {
                        linkedIndices[i].Highlighted = false;
                    }
                }
                selectedTrial.highlighted = true;
            }
        }

        // A trial is a collection of related data point indices.
        // Instead of containing all of the indices the trial pertains to,
        // it only stores the start and end indices (therefore assuming 
        // that the data table has been constructed in such a way that 
        // all trials are consecutive).
        class Cluster
        {
            public int startIdx;  // Start index of the trial (inclusive)
            public int endIdx;  // End index of the trial (exclusive)
            public bool highlighted = false;  // Whether or not this entire trial is currently highlighted
            public bool masked = false;  // Whether or not this entire trial is currently masked
            public int numSelected;  // Used in brush selection to count the total number of currently selected points.

            // Construct a trial using its start (inclusive) and end (exclusive) indices.
            public Cluster(int startIdx, int endIdx)
            {
                this.startIdx = startIdx;
                this.endIdx = endIdx;
            }

            // Returns whether or not the trial contains a certain index.
            public bool Contains(int index)
            {
                return (index >= startIdx && index < endIdx);
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
