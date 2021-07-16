using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace IVLab.Plotting
{
    /// <summary>
    /// Scatter plot <see cref="DataPlot"/> implementation that uses Unity's particle system to efficiently
    /// render many data points at once.
    /// <img src="../resources/ScatterPlot/Example.png"/>
    /// </summary>
    public class ScatterPlot : DataPlot
    {
        [Header("Scatter Plot Properties")]
        /// <summary> Size of the data points. </summary>
        [SerializeField] protected float pointSize;
        /// <summary> Controls whether or not the plot is scaled so that the point (0, 0) is visible. </summary>
        [SerializeField] protected bool scaleToOrigin;

        [Header("Scatter Plot Dependencies")]
        /// <summary> Prefab from which plot particles can be instantiated. </summary>
        [SerializeField] protected GameObject plotParticleSystemPrefab;
        /// <summary> Prefab from which axis labels can be instantiated. </summary>
        [SerializeField] protected GameObject axisLabelPrefab;
        /// <summary> Dropdowns used to select which columns should be compared </summary>
        [SerializeField] protected TMP_Dropdown xDropdown, yDropdown;
        [SerializeField] protected Canvas dropdownCanvas;
        [SerializeField] protected Transform plotParticlesParent;
        /// <summary> Parent used to store axes labels in the scene hierarchy. </summary>
        [SerializeField] protected Transform axisLabelsParent;
        /// <summary> Axis label generating scripts. </summary>
        protected NiceAxisLabel xAxisLabel, yAxisLabel;
        /// <summary> Indices into data table of currently selected columns that are being compared. </summary>
        protected int xColumnIdx, yColumnIdx;
        /// <summary> Array of positions of all the points on the plot. </summary>
        protected Vector2[] pointPositions;
        /// <summary> Array of whether or not each point is NaN. Allows for NaN values to be loaded into the
        /// data table, but ignored when plotting. </summary>
        protected bool[] pointIsNaN;
        /// <summary> Particle system instance used to render data points. </summary>
        protected ParticleSystem plotParticleSystem;
        /// <summary> Array of particles representing all the points on the plot. </summary>
        protected ParticleSystem.Particle[] pointParticles;
        /// <summary> Index into pointPositions array of the point currently selected by the click selection mode. </summary>
        protected int clickedPointIdx;

#if UNITY_EDITOR
        protected float screenHeight;
#endif  // UNITY_EDITOR

        // Self-initialization.
        void Awake()
        {
#if UNITY_EDITOR
            screenHeight = Screen.height;
#endif  // UNITY_EDITOR
        }

        /// <summary>
        /// Initializes the scatter plot by initializing its particle system, axis labeling scripts,
        /// and column selection dropdown menus.
        /// </summary>
        /// <param name="dataPlotManager"> Manager of the plot: contains references to the <see cref="DataTable"/> and 
        /// <see cref="LinkedIndices"/> that the plot works from. </param>
        /// <param name="outerBounds"> Size to set the outer bounds of the plot. </param>
        /// <param name="selectedDataPointIndices"> Array of data point indices the plot should display.
        /// If <c>null</c>, all data points will be displayed by default. </param>
        public override void Init(DataPlotManager dataPlotManager, Vector2 outerBounds, int[] selectedDataPointIndices = null)
        {
            // Perform generic data plot initialization
            base.Init(dataPlotManager, outerBounds, selectedDataPointIndices);

            // Create an instance of the point particle system
            GameObject plotParticleSystemInst = (GameObject)Instantiate(plotParticleSystemPrefab, Vector3.zero, Quaternion.identity);
            plotParticleSystemInst.transform.SetParent(plotParticlesParent);
            plotParticleSystemInst.transform.localScale = Vector3.one;
            plotParticleSystemInst.transform.localPosition = Vector3.zero;
            plotParticleSystem = plotParticleSystemInst.GetComponent<ParticleSystem>();
            plotParticleSystem.Pause();
            // Initialize point particle, position, and isNaN arrays
            pointPositions = new Vector2[this.selectedDataPointIndices.Length];
            pointParticles = new ParticleSystem.Particle[this.selectedDataPointIndices.Length];
            pointIsNaN = new bool[this.selectedDataPointIndices.Length];
            // Modify all data points according to current state of index space
            foreach (int i in this.selectedDataPointIndices)
            {
                UpdateDataPoint(i, linkedIndices[i]);
            }

            // Create instance of x and y axes labels
            GameObject xAxisLabelInst = Instantiate(axisLabelPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            GameObject yAxisLabelInst = Instantiate(axisLabelPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            // Reset their size and positions
            xAxisLabelInst.transform.SetParent(axisLabelsParent);
            xAxisLabelInst.transform.localScale = Vector3.one;
            xAxisLabelInst.transform.localPosition = Vector3.zero;
            yAxisLabelInst.transform.SetParent(axisLabelsParent);
            yAxisLabelInst.transform.localScale = Vector3.one;
            yAxisLabelInst.transform.localPosition = Vector3.zero;
            // Initialize references to their nice axis label scripts
            xAxisLabel = xAxisLabelInst.GetComponent<NiceAxisLabel>();
            yAxisLabel = yAxisLabelInst.GetComponent<NiceAxisLabel>();


            dropdownCanvas.sortingLayerName = "2DPlots";
            // Set the column names displayed in the dropdown menus
            DropdownSetColumnNames();
            // Reposition the dropdowns
            xDropdown.GetComponent<RectTransform>().anchoredPosition = new Vector2(outerBounds.x / 4, outerBounds.y / 2 - 20);
            yDropdown.GetComponent<RectTransform>().anchoredPosition = new Vector2(-outerBounds.x / 4, outerBounds.y / 2 - 20);
            // Add callbacks to the dropdown column-selection menus to update the
            // plot when they are changed.
            xDropdown.onValueChanged.AddListener(delegate { xDropdownUpdated(); });
            yDropdown.onValueChanged.AddListener(delegate { yDropdownUpdated(); });
            // Add callbacks to the dropdowns to disable selection when the mouse is over them
            EventTrigger xDropdownEventTrigger = xDropdown.GetComponent<EventTrigger>();
            EventTrigger yDropdownEventTrigger = yDropdown.GetComponent<EventTrigger>();
            EventTrigger.Entry xPointerEnter = new EventTrigger.Entry();
            xPointerEnter.eventID = EventTriggerType.PointerEnter;
            xPointerEnter.callback.AddListener(delegate { dataPlotManager.DisableSelection(); });
            xDropdownEventTrigger.triggers.Add(xPointerEnter);
            EventTrigger.Entry yPointerEnter = new EventTrigger.Entry();
            yPointerEnter.eventID = EventTriggerType.PointerEnter;
            yPointerEnter.callback.AddListener(delegate { dataPlotManager.DisableSelection(); });
            yDropdownEventTrigger.triggers.Add(yPointerEnter);
            EventTrigger.Entry xPointerExit = new EventTrigger.Entry();
            xPointerExit.eventID = EventTriggerType.PointerExit;
            xPointerExit.callback.AddListener(delegate { dataPlotManager.EnableSelection(); });
            xDropdownEventTrigger.triggers.Add(xPointerExit);
            EventTrigger.Entry yPointerExit = new EventTrigger.Entry();
            yPointerExit.eventID = EventTriggerType.PointerExit;
            yPointerExit.callback.AddListener(delegate { dataPlotManager.EnableSelection(); });
            yDropdownEventTrigger.triggers.Add(yPointerExit);
        }

        // For some reason particles are destroyed when disabled and then enabled
        // So we should refresh them whenever the data plot is enabled
        private void OnEnable()
        {
            if (plotParticleSystem != null)
            {
                RefreshPlotGraphics();
            }
        }

        void Update()
        {
            // Ensures the plot is always drawn and scaled correctly in editor mode even if the screen height changes
#if UNITY_EDITOR
            if (Screen.height != screenHeight)
            {
                Plot();
                screenHeight = Screen.height;
            }
#endif  // UNITY_EDITOR
        }

        /// <summary>
        /// Sets the plot size, as well as positioning the dropdown menus.
        /// </summary>
        protected override void SetPlotSize()
        {
            base.SetPlotSize();

            xDropdown.GetComponent<RectTransform>().anchoredPosition = new Vector2(outerBounds.x / 4, outerBounds.y / 2 - 20);
            yDropdown.GetComponent<RectTransform>().anchoredPosition = new Vector2(-outerBounds.x / 4, outerBounds.y / 2 - 20);
        }

        /// <summary>
        /// Updates a specified data point based on its linked index attributes, only if it is
        /// already within the selected subset of points that this graph plots.
        /// </summary>
        /// <param name="index">Index of data point that needs to be updated.</param>
        /// <param name="indexAttributes">Current attributes of the data point.</param>
        public override void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes indexAttributes)
        {
            if (selectedIndexDictionary.ContainsKey(index))
            {
                int i = selectedIndexDictionary[index];
                if (indexAttributes.Masked)
                {
                    pointParticles[i].startColor = maskedColor;
                }
                else if (indexAttributes.Highlighted)
                {
                    pointParticles[i].startColor = highlightedColor;
                    // Hack to ensure highlighted particle appears in front of non-highlighted particles
                    pointParticles[i].position = new Vector3(pointParticles[i].position.x, pointParticles[i].position.y, -0.01f);
                }
                else
                {
                    pointParticles[i].startColor = defaultColor;
                    // Hack to ensure non-highlighted particle appears behind of highlighted particles
                    pointParticles[i].position = new Vector3(pointParticles[i].position.x, pointParticles[i].position.y, 0f);
                }
            }
        }

        /// <summary>
        /// Updates the point particle system to reflect the current state of the 
        /// data point particles.
        /// </summary>
        /// <remarks>
        /// Usually called after a series of UpdateDataPoint() calls to ensure
        /// that those updates are visually reflected.
        /// </remarks>
        public override void RefreshPlotGraphics()
        {
            plotParticleSystem.SetParticles(pointParticles, pointParticles.Length);
        }

        /// <summary>
        /// Plots only the selected data in the data table based on the two currently selected columns.
        /// </summary>
        public override void Plot()
        {
            // Get the min/max values of the columns of interest
            float xMin = selectedDataPointMins[xColumnIdx];
            float xMax = selectedDataPointMaxes[xColumnIdx];
            float yMin = selectedDataPointMins[yColumnIdx];
            float yMax = selectedDataPointMaxes[yColumnIdx];
            // If scaleToOrigin is enabled, scale min/max values such that (0,0) is visible
            if (scaleToOrigin)
            {
                xMin = (xMin > 0) ? 0 : xMin;
                xMax = (xMax < 0) ? 0 : xMax;
                yMin = (yMin > 0) ? 0 : yMin;
                yMax = (yMax < 0) ? 0 : yMax;
            }
            // Generate adjusted "nice" min and max values before generating nice axes labels
            Vector2 axisSource = plotOuterRect.anchoredPosition - innerBounds / 2;
            (xMin, xMax) = xAxisLabel.GenerateNiceMinMax(xMin, xMax);
            (yMin, yMax) = yAxisLabel.GenerateNiceMinMax(yMin, yMax);
            xAxisLabel.GenerateXAxisLabel(axisSource, innerBounds, true);
            yAxisLabel.GenerateYAxisLabel(axisSource, innerBounds, true);
            // Determine scale factors for the plot given "nice" min/max values
            float xScale = innerBounds.x / (xMax - xMin);
            float yScale = innerBounds.y / (yMax - yMin);
            // Get the origin position relative to the canvas given this scaling
            Vector2 origin = new Vector2(-(xMax + xMin) / 2.0f * xScale, -(yMax + yMin) / 2.0f * yScale);
            // Position each data point
            for (int i = 0; i < selectedDataPointIndices.Length; i++)
            {
                // Get the index of the actual data point
                int dataPointIndex = selectedDataPointIndices[i];
                // If either the x or y coordinate of the point is NaN, 
                // flag it so that it will be unselectable and set its size to 0 so it will be invisible
                float xData = dataTable.Data(dataPointIndex, xColumnIdx);
                float yData = dataTable.Data(dataPointIndex, yColumnIdx);
                if (float.IsNaN(xData) || float.IsNaN(yData))
                {
                    pointIsNaN[i] = true;
                    pointParticles[i].startSize = 0;
                }
                // Otherwise just position and render it normally
                else
                {
                    pointIsNaN[i] = false;
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

        /// <summary>
        /// Callback to update the currently selected x-column index whenever a new selection is made in 
        /// the x-axis dropdown, and then replot the plot.
        /// </summary>
        /// <remarks>
        /// Relies on the fact that the "value" of a dropdown is also the index of that column in the data table.
        /// </remarks>
        protected virtual void xDropdownUpdated() { xColumnIdx = xDropdown.value; Plot(); }
        /// <summary>
        /// Callback to update the currently selected y-column index whenever a new selection is made in 
        /// the y-axis dropdown, and then replot the plot.
        /// </summary>
        /// /// <remarks>
        /// Relies on the fact that the "value" of a dropdown is also the index of that column in the data table.
        /// </remarks>
        protected virtual void yDropdownUpdated() { yColumnIdx = yDropdown.value; Plot(); }

        /// <summary>
        /// Clears and then adds the column names from the data table to the x and y dropdown menus.
        /// </summary>

        protected virtual void DropdownSetColumnNames()
        {
            // Clear and add new column names to dropdown selection menus
            xDropdown.options.Clear();
            yDropdown.options.Clear();
            foreach (string name in dataTable.ColumnNames)
            {
                xDropdown.options.Add(new TMP_Dropdown.OptionData() { text = name });
                yDropdown.options.Add(new TMP_Dropdown.OptionData() { text = name });
            }
            // If possible, ensure currently selected value on both dropdowns is not the same
            if (dataTable.ColumnNames.Length > 1) yDropdown.value = 1;
            // Update currently selected column indices
            xColumnIdx = xDropdown.value;
            yColumnIdx = yDropdown.value;
        }

        /// <summary>
        /// Selects the point within the point selection radius that is closest to the mouse selection position if the selection state
        /// is "Start", and otherwise simply checks to see if the initially selected point is still within the point selection radius,
        /// highlighting it if it is, unhighlighting it if it is not.
        /// </summary>
        /// <param name="selectionPosition">Current selection position.</param>
        /// <param name="selectionState">State of the selection, e.g. Start/Update/End.</param>
        public override void ClickSelection(Vector2 selectionPosition, SelectionMode.State selectionState)
        {
            // Square the selection radius to avoid square root computation in the future
            float selectionRadiusSqr = Mathf.Pow(clickSelectionRadius, 2);
            // If this is the initial click, i.e. selectionState is Start, find the closest particle to the mouse (within selection radius) 
            // and highlight it, unhighlighting all other points
            if (selectionState == SelectionMode.State.Start)
            {
                // Reset clicked point index to -1 to reflect that no data points have been clicked
                clickedPointIdx = -1;
                // Set the current minimum distance (squared) between mouse and any point to the selection radius (squared)
                float minDistSqr = selectionRadiusSqr;
                // Iterate through all points to see if any are closer to the mouse than the current min distance,
                // updating the min distance every time a closer point is found
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    // Only try to highlight this data point if is actually in the selection plotted by this plot
                    // (and not NaN)
                    if (selectedIndexDictionary.ContainsKey(i) && !pointIsNaN[selectedIndexDictionary[i]])
                    {
                        // Only highlight the point if it is truly the closest one to the mouse
                        float mouseToPointDistSqr = Vector2.SqrMagnitude(selectionPosition - pointPositions[selectedIndexDictionary[i]]);
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
            }
            // If this is not the initial click but their was previously a point that was selected/clicked,
            // check to see if that point is still within the point selection radius of the current mouse selection position
            else if (clickedPointIdx != -1)
            {
                int i = selectedIndexDictionary[clickedPointIdx];
                // If this point is NaN, don't even try to (un)select it
                if (!pointIsNaN[i])
                {
                    float mouseToPointDistSqr = Vector2.SqrMagnitude(selectionPosition - pointPositions[i]);
                    if (mouseToPointDistSqr < selectionRadiusSqr)
                    {
                        // Only highlight the data point if it isn't masked
                        if (!linkedIndices[clickedPointIdx].Masked)
                        {
                            linkedIndices[clickedPointIdx].Highlighted = true;
                        }
                    }
                    else
                    {
                        linkedIndices[clickedPointIdx].Highlighted = false;
                    }
                }
            }
        }

        /// <summary>
        /// Selects all of the data points inside the given selection rectangle.
        /// </summary>
        /// <param name="selectionRect">Transform of the selection rectangle.</param>
        public override void RectSelection(RectTransform selectionRect)
        {
            // Iterate through all data point indices
            for (int i = 0; i < linkedIndices.Size; i++)
            {
                // Only try to highlight this data point if is actually in the selection plotted by this plot
                // (and not NaN)
                if (selectedIndexDictionary.ContainsKey(i) && !pointIsNaN[selectedIndexDictionary[i]])
                {
                    // Must translate point position to anchored position space for rect.Contains() to work
                    if (selectionRect.rect.Contains(pointPositions[selectedIndexDictionary[i]] - selectionRect.anchoredPosition))
                    {
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
        }

        /// <summary>
        /// Selects all the data points that the brush has passed over.
        /// </summary>
        /// <param name="prevBrushPosition">Previous position of the brush.</param>
        /// <param name="brushDelta">Change in position from previous to current.</param>
        /// <param name="selectionState">State of the selection, e.g. Start/Update/End.</param>
        public override void BrushSelection(Vector2 prevBrushPosition, Vector2 brushDelta, SelectionMode.State selectionState)
        {
            // Square the brush radius to avoid square root computation in the future
            float brushRadiusSqr = Mathf.Pow(brushSelectionRadius, 2);
            // This only triggers when brush selection is first called, therefore we can use it as an indicator
            // that we should reset all points except for those currently within the radius of the brush
            if (selectionState == SelectionMode.State.Start)
            {
                // Iterate through all data point indices
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    // Only try to highlight the current index if it is within the selection that this plot plots
                    // (and not NaN)
                    if (selectedIndexDictionary.ContainsKey(i) && !pointIsNaN[selectedIndexDictionary[i]])
                    {
                        // Highlight any points within the radius of the brush and unhighlight any that aren't
                        float pointToBrushDistSqr = Vector2.SqrMagnitude(pointPositions[selectedIndexDictionary[i]] - prevBrushPosition);
                        if (pointToBrushDistSqr < brushRadiusSqr)
                        {
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
            }
            // If this isn't the start of the selection, iterate through all data point positions
            // to highlight those which have been selected by the brush (taking into account the full movement 
            // of the brush since the previous frame)
            else
            {
                // Only iterate through the selected indices that this plot plots
                for (int i = 0; i < selectedDataPointIndices.Length; i++)
                {
                    // Get the index of the actual data point
                    int dataPointIndex = selectedDataPointIndices[i];
                    // If this point is NaN, don't try to (un)select it
                    if (!pointIsNaN[i]) {
                        // Trick to parametrize the line segment that the brush traveled since last frame and find the closest
                        // point on it to the current plot point
                        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(pointPositions[i] - prevBrushPosition, brushDelta) / brushDelta.sqrMagnitude));
                        Vector2 closestPointOnLine = prevBrushPosition + t * brushDelta;
                        // Determine if point lies within the radius of the closest point to it on the line
                        float pointToBrushDistSqr = Vector2.SqrMagnitude(pointPositions[i] - closestPointOnLine);
                        if (pointToBrushDistSqr < brushRadiusSqr)
                        {
                            // Only highlight the data point if it isn't masked
                            if (!linkedIndices[dataPointIndex].Masked)
                            {
                                linkedIndices[dataPointIndex].Highlighted = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
