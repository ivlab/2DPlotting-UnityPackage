using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace IVLab.Plotting
{
    /// <summary>
    /// Parallel coordinates plot <see cref="DataPlot"/> implementation that uses Unity particle systems
    /// along with line renderers to efficiently render many data points at once.
    /// <img src="../resources/ParallelCoordsPlot/Example.png"/>
    /// </summary>
    public class ParallelCoordsPlot : DataPlot
    {
        // Editor-visible private variables
        [Header("Parallel Coords Plot Properties")]
        /// <summary> Size of the data points. </summary>
        [SerializeField] private float pointSize;
        /// <summary> Width of the line that connects the data points. </summary>
        [SerializeField] private float lineWidth;
        /// <summary> Controls whether or not the plot is scaled so that the 0 is visible in each column/axis. </summary>
        [SerializeField] private bool scaleToZero;
        /// <summary> The default color of lines in the plot. </summary>
        [SerializeField] protected Color32 defaultLineColor;
        /// <summary> The color of highlighted lines in the plot. </summary>
        [SerializeField] protected Color32 highlightedLineColor;
        /// <summary> The color of masked lines in the plot. </summary>
        [SerializeField] protected Color32 maskedLineColor;

        [Header("Parallel Coords Dependencies")]
        /// <summary> Prefab from which plot particles can be instantiated. </summary>
        [SerializeField] private GameObject plotParticleSystemPrefab;
        /// <summary> Prefab from which axis labels can be instantiated. </summary>
        [SerializeField] private GameObject axisLabelPrefab;
        /// <summary> Prefab from which line renderers can be instantiated. </summary>
        [SerializeField] private GameObject lineRendererPrefab;
        /// <summary> Prefab from which axis name label button can be instantiated. </summary>
        [SerializeField] private GameObject axisNameButtonPrefab;
        /// <summary> Parent used to store particle systems in the scene hierarchy. </summary>
        [SerializeField] private Transform plotParticlesParent;
        /// <summary> Parent used to store line renderers in the scene hierarchy. </summary>
        [SerializeField] private Transform lineRendererParent;
        /// <summary> Parent used to store axes labels in the scene hierarchy. </summary>
        [SerializeField] private Transform axisLabelsParent;

        // Editor-non-visible private variables
        /// <summary> Matrix (column-major) of point positions in each column/axis of the plot. </summary>
        private Vector2[][] pointPositions;
        /// <summary> Matrix (column-major) of whether or not each point is hidden (and therefore unselectable). </summary>
        /// <remarks> Allows for points to be unselectable when masked, and for NaN values to be loaded into the data table
        /// but be ignored when plotting/selecting. </remarks>
        protected bool[][] pointIsHidden;
        /// <summary> Array of particle systems used to render data points in each column/axis. </summary>
        private ParticleSystem[] plotParticleSystem;
        /// <summary> Matrix (column-major) of particles representing all the points on the plot. </summary>
        private ParticleSystem.Particle[][] pointParticles;
        /// <summary> Array of line renderers storing line renderer for each data point. Used when the data table
        /// contains no NaN data in order to improve performance. </summary>
        private LineRenderer[] defaultLineRenderers;
        /// <summary> Matrix of (column-major) line renderers for the connections between every point (particle) in each data point 
        /// (which are multiple point particles connected by a line). Capable of creating segmented lines as required when plotting NaN data. </summary>
        private LineRenderer[][] NaNsLineRenderers;
        /// <summary> Array of axis label scripts for each column/axis of the plot. </summary>
        private NiceAxisLabel[] axisLabels;
        /// <summary> Array of axis name buttons that display the names of each axis and can be clicked to flip them. </summary>
        private Button[] axisNameButtons;
        /// <summary> Offset between the edge of the plot and the buttons. </summary>
        private float buttonOffset = 20;
        /// <summary> Indices into pointPositions matrix of the point currently selected by the click selection mode. </summary>
        private (int, int) clickedPointIdx;

#if UNITY_EDITOR
        private float screenHeight;
#endif  // UNITY_EDITOR

        // Self-initialization.
        void Awake()
        {
#if UNITY_EDITOR
            screenHeight = Screen.height;
#endif  // UNITY_EDITOR
        }

        /// <summary>
        /// Initializes the parallel coords plot by initializing its particle systems, line renderers, axis labeling scripts,
        /// and axis-flipping buttons.
        /// </summary>
        /// <param name="dataPlotManager"> Manager of the plot: contains reference to the <see cref="DataManager"/> which controls the
        /// <see cref="DataTable"/> and <see cref="LinkedIndices"/> that the plot works from. </param>
        /// <param name="plotLayout"> Stores information about the size and padding of the plot. </param>
        /// <param name="dataPointIndices"> Array of data point indices the plot should display.
        /// If <c>null</c>, all data points will be displayed by default. </param>
        public override void Init(DataPlotManager dataPlotManager, PlotUISkin plotSkin, PlotLayout plotLayout, int[] dataPointIndices = null)
        {
            // Perform generic data plot initialization
            base.Init(dataPlotManager, plotSkin, plotLayout, dataPointIndices);

            // Initialize point position and particle matrices/arrays
            pointPositions = new Vector2[dataTable.Width][];
            pointParticles = new ParticleSystem.Particle[dataTable.Width][];
            pointIsHidden = new bool[dataTable.Width][];
            // Create an instance of the point particle system for each column/axis
            plotParticleSystem = new ParticleSystem[dataTable.Width];
            for (int j = 0; j < dataTable.Width; j++)
            {
                pointPositions[j] = new Vector2[this.plottedDataPointIndices.Length];
                pointParticles[j] = new ParticleSystem.Particle[this.plottedDataPointIndices.Length];
                pointIsHidden[j] = new bool[this.plottedDataPointIndices.Length];
                // Instantiate a point particle system GameObject
                GameObject plotParticleSystemInst = Instantiate(plotParticleSystemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                // Reset its size and position
                plotParticleSystemInst.transform.SetParent(plotParticlesParent);
                plotParticleSystemInst.transform.localScale = Vector3.one;
                plotParticleSystemInst.transform.localPosition = Vector3.zero;
                // Add its particle system component to the array of particle systems
                plotParticleSystem[j] = plotParticleSystemInst.GetComponent<ParticleSystem>();
                plotParticleSystem[j].Pause();
            }

            // If the data table does not contain any NaN data, only create an instance of the plot line renderer system for 
            // each selected data point
            if (!dataTable.ContainsNaNs)
            {
                defaultLineRenderers = new LineRenderer[this.plottedDataPointIndices.Length];
                for (int i = 0; i < defaultLineRenderers.Length; i++)
                {
                    // Instantiate a line render GameObject
                    GameObject lineRendererGO = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    // Reset its size and position
                    lineRendererGO.transform.SetParent(lineRendererParent);
                    lineRendererGO.transform.localScale = Vector3.one;
                    lineRendererGO.transform.localPosition = Vector3.zero;
                    // Add its line renderer component to the array of line renderers
                    defaultLineRenderers[i] = lineRendererGO.GetComponent<LineRenderer>();
                    defaultLineRenderers[i].positionCount = dataTable.Width;
                }
            }
            // Otherwise create an instance of the plot line renderer system for the connections between every point within every
            // selected data point
            else
            {
                NaNsLineRenderers = new LineRenderer[Mathf.FloorToInt(dataTable.Width - 1)][];
                for (int j = 0; j < NaNsLineRenderers.Length; j++)
                {
                    NaNsLineRenderers[j] = new LineRenderer[this.plottedDataPointIndices.Length];
                    for (int i = 0; i < this.plottedDataPointIndices.Length; i++)
                    {
                        // Instantiate a line render GameObject
                        GameObject lineRendererGO = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                        // Reset its size and position
                        lineRendererGO.transform.SetParent(lineRendererParent);
                        lineRendererGO.transform.localScale = Vector3.one;
                        lineRendererGO.transform.localPosition = Vector3.zero;
                        // Add its line renderer component to the array of line renderers
                        NaNsLineRenderers[j][i] = lineRendererGO.GetComponent<LineRenderer>();
                        NaNsLineRenderers[j][i].positionCount = 2;
                    }
                }
            }
            // Apply line renderer styling
            defaultLineColor = plotSkin.defaultColor;
            highlightedLineColor = plotSkin.highlightedColor;
            maskedLineColor = plotSkin.maskedColor;

            // Create an instance of an axis label and a axis name for each column/axis
            axisLabels = new NiceAxisLabel[dataTable.Width];
            axisNameButtons = new Button[dataTable.Width];
            for (int j = 0; j < axisLabels.Length; j++)
            {
                // Instantiate a axis label GameObject
                GameObject axisLabel = Instantiate(axisLabelPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                // Reset its size and position
                axisLabel.transform.SetParent(axisLabelsParent);
                axisLabel.transform.localScale = Vector3.one;
                axisLabel.transform.localPosition = Vector3.zero;
                // Add its nice axis label script component to the array of axis label scripts
                axisLabels[j] = axisLabel.GetComponent<NiceAxisLabel>();
                // Apply styling
                axisLabels[j].SetStyling(plotSkin.axisLabelTextColor, plotSkin.tickMarkColor, plotSkin.gridlineColor);

                // Instantiate a axis name GameObject
                GameObject axisNameButtonInst = Instantiate(axisNameButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                // Reset its size and position
                axisNameButtonInst.transform.SetParent(axisLabel.transform);
                axisNameButtonInst.transform.localScale = Vector3.one;
                axisNameButtonInst.transform.localPosition = Vector3.zero;
                // Add its button to the array of axis name buttons
                axisNameButtons[j] = axisNameButtonInst.GetComponent<Button>();
                // Add a callback to then button to flip its related axis
                int columnIdx = j;
                axisNameButtons[j].onClick.AddListener(delegate { FlipAxis(columnIdx); });

                // Add pointer enter and exit triggers to disable and enable selection when
                // buttons are being pressed
                EventTrigger eventTrigger = axisNameButtonInst.GetComponent<EventTrigger>();
                EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
                pointerEnter.eventID = EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener(delegate { dataPlotManager.DisableSelection(); });
                eventTrigger.triggers.Add(pointerEnter);
                EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                pointerExit.eventID = EventTriggerType.PointerExit;
                pointerExit.callback.AddListener(delegate { dataPlotManager.EnableSelection(); });
                eventTrigger.triggers.Add(pointerExit);
            }

            // Modify all data points according to current state of index space
            foreach (int i in this.plottedDataPointIndices)
            {
                UpdateDataPoint(i, linkedIndices[i]);
            }
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

        // Manages mouse input with current selection mode.
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
        /// Updates a specified data point (which for a parallel coords plot includes multiple 
        /// point particles and their line renderer) based on its linked index attributes, only if it is
        /// within the subset of points that this graph plots.
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
                    for (int j = 0; j < dataTable.Width; j++)
                    {
                        pointParticles[j][i].startColor = maskedColor;
                        if (dataTable.ContainsNaNs && j < NaNsLineRenderers.Length)
                        {
                            NaNsLineRenderers[j][i].startColor = maskedLineColor;
                            NaNsLineRenderers[j][i].endColor = maskedLineColor;
                        }
                        // Make the point unselectable
                        pointIsHidden[j][i] = true;
                    }
                    if (!dataTable.ContainsNaNs)
                    {
                        defaultLineRenderers[i].startColor = maskedLineColor;
                        defaultLineRenderers[i].endColor = maskedLineColor;
                    }
                }
                else if (indexAttributes.Highlighted)
                {

                    for (int j = 0; j < dataTable.Width; j++)
                    {
                        pointParticles[j][i].startColor = highlightedColor;
                        // Hack to ensure highlighted particle appears in front of non-highlighted particles
                        pointParticles[j][i].position = new Vector3(pointParticles[j][i].position.x, pointParticles[j][i].position.y, -0.01f);
                        if (dataTable.ContainsNaNs && j < NaNsLineRenderers.Length)
                        {
                            NaNsLineRenderers[j][i].startColor = highlightedLineColor;
                            NaNsLineRenderers[j][i].endColor = highlightedLineColor;
                            NaNsLineRenderers[j][i].sortingOrder = 3;
                        }
                        // Ensure the point is selectable
                        pointIsHidden[j][i] = false;
                    }
                    if (!dataTable.ContainsNaNs)
                    {
                        defaultLineRenderers[i].startColor = highlightedLineColor;
                        defaultLineRenderers[i].endColor = highlightedLineColor;
                        defaultLineRenderers[i].sortingOrder = 3;
                    }
                }
                else
                {
                    for (int j = 0; j < dataTable.Width; j++)
                    {
                        pointParticles[j][i].startColor = defaultColor;
                        // Hack to ensure highlighted particle appears in front of non-highlighted particles
                        pointParticles[j][i].position = new Vector3(pointParticles[j][i].position.x, pointParticles[j][i].position.y, 0);
                        if (dataTable.ContainsNaNs && j < NaNsLineRenderers.Length)
                        {
                            NaNsLineRenderers[j][i].startColor = defaultLineColor;
                            NaNsLineRenderers[j][i].endColor = defaultLineColor;
                            NaNsLineRenderers[j][i].sortingOrder = 1;
                        }
                        // Ensure the point is selectable
                        pointIsHidden[j][i] = false;
                    }
                    if (!dataTable.ContainsNaNs)
                    {
                        defaultLineRenderers[i].startColor = defaultLineColor;
                        defaultLineRenderers[i].endColor = defaultLineColor;
                        defaultLineRenderers[i].sortingOrder = 1;
                    }
                }
            }
        }


        /// <summary>
        /// Updates the point particle systems to reflect the current state of the 
        /// data point particles.
        /// </summary>
        /// <remarks>
        /// Usually called after a series of UpdateDataPoint() calls to ensure
        /// that those updates are visually reflected.
        /// </remarks>
        public override void RefreshPlotGraphics()
        {
            for (int j = 0; j < plotParticleSystem.Length; j++)
            {
                plotParticleSystem[j].SetParticles(pointParticles[j], pointParticles[j].Length);
            }
        }

        /// <summary>
        /// Flips the j'th axis of the plot.
        /// </summary>
        /// <param name="j">Index into the data table for the column/axis that should be flipped. </param>
        public void FlipAxis(int j)
        {
            // Toggle the inverted status of the axis
            axisLabels[j].Inverted = !axisLabels[j].Inverted;

            // Determine the axis source position based on inversion and offset
            Vector2 axisSource;
            Vector2 axisOffset;
            if (dataTable.Width == 1)  // Special case where we want the axis in the middle of the plot
            {
                axisOffset = Vector2.right * innerBounds.x / 2;
            }
            else  // Otherwise space axes evenly
            {
                axisOffset = Vector2.right * innerBounds.x / (dataTable.Width - 1) * j;
            }
            if (axisLabels[j].Inverted)
            {
                axisSource = plotOuterRect.anchoredPosition + axisOffset + new Vector2(-innerBounds.x, innerBounds.y) / 2 + centerOffset;
            }
            else
            {
                axisSource = plotOuterRect.anchoredPosition + axisOffset + new Vector2(-innerBounds.x, -innerBounds.y) / 2 + centerOffset;
            }

            // Regenerate the axis labels
            axisLabels[j].GenerateYAxisLabel(axisSource, innerBounds);

            // Reposition just the point particles and line renderer points in this column/axis to match the flip
            float columnMin = axisLabels[j].NiceMin;
            float columnMax = axisLabels[j].NiceMax;
            float columnScale = innerBounds.y / (columnMax - columnMin);
            for (int i = 0; i < plottedDataPointIndices.Length; i++)
            {
                // Get the index of the actual data point
                int dataPointIndex = plottedDataPointIndices[i];
                // Only try to flip the point if it isn't NaN
                float dataValue = dataTable.Data(dataPointIndex, j);
                if (!float.IsNaN(dataValue))
                {
                    float x = axisSource.x;
                    // Position points along the y axis depending on the inversion
                    float y;
                    if (axisLabels[j].Inverted)
                    {
                        y = axisSource.y - (dataTable.Data(dataPointIndex, j) - columnMin) * columnScale;
                    }
                    else
                    {
                        y = axisSource.y + (dataTable.Data(dataPointIndex, j) - columnMin) * columnScale;
                    }
                    pointPositions[j][i] = new Vector2(x, y);
                    pointParticles[j][i].position = new Vector3(x, y, 0) * plotsCanvas.transform.localScale.y + Vector3.forward * pointParticles[j][i].position.z;  // scale by canvas size since particles aren't officially part of the canvas 
                    if (!dataTable.ContainsNaNs)
                    {
                        defaultLineRenderers[i].SetPosition(j, new Vector3(x, y, 0));
                    }
                    else
                    {
                        if (j < NaNsLineRenderers.Length && NaNsLineRenderers.Length != 0 && NaNsLineRenderers[j][i].positionCount != 0)
                        {
                            NaNsLineRenderers[j][i].SetPosition(0, new Vector3(x, y, 0));
                        }
                        if (j > 0 && NaNsLineRenderers[j - 1][i].positionCount != 0)
                        {
                            NaNsLineRenderers[j - 1][i].SetPosition(1, new Vector3(x, y, 0));
                        }
                    }
                }
            }

            // Update the particles to match
            plotParticleSystem[j].SetParticles(pointParticles[j], pointParticles[j].Length);
        }

        /// <summary>
        /// Plots only the selected data in the data table, updating all particle systems and line renderers.
        /// </summary>
        public override void Plot()
        {
            // Iterate through each column/axis and plot it
            for (int j = 0; j < dataTable.Width; j++)
            {
                // Extract the min and max values for this column/axis from the data table
                float columnMin = plottedDataPointMins[j];
                float columnMax = plottedDataPointMaxes[j];
                if (scaleToZero)
                {
                    columnMin = (columnMin > 0) ? 0 : columnMin;
                    columnMax = (columnMax < 0) ? 0 : columnMax;
                }
                // Instantiate a new axis label, first by generating "nice" min and max values and then by generating
                // the actual axis
                // Determine the axis source position based on inversion and offset
                Vector2 axisSource;
                Vector2 axisOffset;
                if (dataTable.Width == 1)  // Special case where we want the axis in the middle of the plot
                {
                    axisOffset = Vector2.right * innerBounds.x / 2;
                }
                else  // Otherwise space axes evenly
                {
                    axisOffset = Vector2.right * innerBounds.x / (dataTable.Width - 1) * j;
                }
                if (axisLabels[j].Inverted)
                {
                    axisSource = plotOuterRect.anchoredPosition + axisOffset + new Vector2(-innerBounds.x, innerBounds.y) / 2 + centerOffset;
                }
                else
                {
                    axisSource = plotOuterRect.anchoredPosition + axisOffset + new Vector2(-innerBounds.x, -innerBounds.y) / 2 + centerOffset;
                }
                (columnMin, columnMax) = axisLabels[j].GenerateNiceMinMax(columnMin, columnMax);
                axisLabels[j].GenerateYAxisLabel(axisSource, innerBounds);

                // Set the position and text of column/axis name
                if (axisLabels[j].Inverted)
                {
                    axisNameButtons[j].GetComponent<RectTransform>().anchoredPosition3D = axisSource + Vector2.down * (innerBounds.y + buttonOffset);
                }
                else
                {
                    axisNameButtons[j].GetComponent<RectTransform>().anchoredPosition3D = axisSource + Vector2.down * buttonOffset;
                }
                axisNameButtons[j].GetComponentInChildren<TextMeshProUGUI>().text = dataTable.ColumnNames[j];

                // Determine a rescaling of this column/axis's data based on adjusted ("nice"-fied) min and max
                float columnScale = innerBounds.y / (columnMax - columnMin);

                // Iterate through all data points in this column/axis and position/scale particle and linerenderer points
                for (int i = 0; i < plottedDataPointIndices.Length; i++)
                {
                    // Get the index of the actual data point
                    int dataPointIndex = plottedDataPointIndices[i];

                    if (!dataTable.ContainsNaNs)
                    {
                        // Determine the x and y position of the current data point based on the adjusted rescaling
                        float x = axisSource.x;
                        float y;
                        if (axisLabels[j].Inverted)
                        {
                            y = axisSource.y - (dataTable.Data(dataPointIndex, j) - columnMin) * columnScale;
                        }
                        else
                        {
                            y = axisSource.y + (dataTable.Data(dataPointIndex, j) - columnMin) * columnScale;
                        }
                        // Position and scale the point particles and line renderers
                        pointPositions[j][i] = new Vector2(x, y);
                        pointParticles[j][i].position = new Vector3(x, y, 0) * plotsCanvas.transform.localScale.y + Vector3.forward * pointParticles[j][i].position.z;  // scale by canvas size since particles aren't officially part of the canvas
                        pointParticles[j][i].startSize = pointSize * plotsCanvas.transform.localScale.y * Mathf.Max(outerBounds.x, outerBounds.y) / 300;
                        defaultLineRenderers[i].SetPosition(j, new Vector3(x, y, 0));
                        if (j == 0)
                        {
                            defaultLineRenderers[i].startWidth = lineWidth * plotsCanvas.transform.localScale.y * Mathf.Max(outerBounds.x, outerBounds.y) / 300;
                            defaultLineRenderers[i].endWidth = lineWidth * plotsCanvas.transform.localScale.y * Mathf.Max(outerBounds.x, outerBounds.y) / 300;
                        }
                    }
                    else
                    {
                        // If the point is NaN, flag it so that it will be unselectable and set its size to 0 so it will be invisible
                        float dataValue = dataTable.Data(dataPointIndex, j);
                        if (float.IsNaN(dataValue))
                        {
                            pointIsHidden[j][i] = true;
                            // Hide the point by setting its size to 0
                            pointParticles[j][i].startSize = 0;
                        }
                        // Otherwise position and size the point normally
                        else
                        {
                            pointIsHidden[j][i] = false;
                            // Determine the x and y position of the current data point based on the adjusted rescaling
                            float x = axisSource.x;
                            float y;
                            if (axisLabels[j].Inverted)
                            {
                                y = axisSource.y - (dataTable.Data(dataPointIndex, j) - columnMin) * columnScale;
                            }
                            else
                            {
                                y = axisSource.y + (dataTable.Data(dataPointIndex, j) - columnMin) * columnScale;
                            }
                            // Position and scale the point particles and line renderers
                            pointPositions[j][i] = new Vector2(x, y);
                            pointParticles[j][i].position = new Vector3(x, y, 0) * plotsCanvas.transform.localScale.y + Vector3.forward * pointParticles[j][i].position.z;  // scale by canvas size since particles aren't officially part of the canvas
                            pointParticles[j][i].startSize = pointSize * plotsCanvas.transform.localScale.y * Mathf.Max(outerBounds.x, outerBounds.y) / 300;
                        }
                        // Construct the line renderer starting with the second column/axis (since it connects with the first)
                        if (j > 0)
                        {
                            // Set the width of the line renderer
                            NaNsLineRenderers[j - 1][i].startWidth = lineWidth * plotsCanvas.transform.localScale.y * Mathf.Max(outerBounds.x, outerBounds.y) / 300;
                            NaNsLineRenderers[j - 1][i].endWidth = lineWidth * plotsCanvas.transform.localScale.y * Mathf.Max(outerBounds.x, outerBounds.y) / 300;
                            // Only connect the points of the line renderer if neither of them are NaN
                            if (!pointIsHidden[j - 1][i] && !pointIsHidden[j][i])
                            {
                                NaNsLineRenderers[j - 1][i].positionCount = 2;
                                NaNsLineRenderers[j - 1][i].SetPosition(0, pointPositions[j - 1][i]);
                                NaNsLineRenderers[j - 1][i].SetPosition(1, pointPositions[j][i]);
                            }
                            // Otherwise connect them
                            else
                            {
                                NaNsLineRenderers[j - 1][i].positionCount = 0;
                            }
                        }
                    }
                }
            }
            // Refresh the plot graphics to match the plotting changes made
            RefreshPlotGraphics();
        }

        /// <summary>
        /// Selects the point within the point selection radius that is closest to the mouse selection position if the selection state
        /// is "Start", and otherwise simply checks to see if the initially selected point is still within the point selection radius,
        /// highlighting it if it is, unhighlighting it if it is not.
        /// </summary>
        /// <remarks>
        /// For a parallel coords plot, a "data point" consists of multiple point particles, any of which could be selected.
        /// </remarks>
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
                clickedPointIdx = (-1, -1);
                // Set the current minimum distance (squared) between mouse and any point to the selection radius (squared)
                float minDistSqr = selectionRadiusSqr;
                // Iterate through all points to see if any are closer to the mouse than the current min distance,
                // updating the min distance every time a closer point is found
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (dataPointIndexMap.ContainsKey(i))
                    {
                        for (int j = 0; j < pointPositions.Length; j++)
                        {
                            // Hidden points are unselectable
                            if (!pointIsHidden[j][dataPointIndexMap[i]])
                            {
                                float mouseToPointDistSqr = Vector2.SqrMagnitude(selectionPosition - pointPositions[j][dataPointIndexMap[i]]);
                                // Only highlight the point if it is truly the closest one to the mouse
                                if (mouseToPointDistSqr < selectionRadiusSqr && mouseToPointDistSqr < minDistSqr)
                                {
                                    // Unhighlight the previous closest point to the mouse since it is no longer the closest
                                    // (as long as it was not already relating to the same data point idx)
                                    if (clickedPointIdx != (-1, -1) && clickedPointIdx.Item1 != i)
                                    {
                                        linkedIndices[clickedPointIdx.Item1].Highlighted = false;
                                    }
                                    // Highlight the new closest point
                                    minDistSqr = mouseToPointDistSqr;
                                    clickedPointIdx = (i, j);
                                    // Only highlight the data point if it isn't masked
                                    if (!linkedIndices[i].Masked)
                                    {
                                        linkedIndices[i].Highlighted = true;
                                    }
                                }
                            }
                        }
                        // Since all the individual points in a "row" are related to a single "data point",
                        // if not a single point in this row was clicked on, make sure not to highlight
                        // this entire data point
                        if (clickedPointIdx.Item1 != i)
                        {
                            linkedIndices[i].Highlighted = false;
                        }
                    }
                    else
                    {
                        linkedIndices[i].Highlighted = false;
                    }
                }
            }
            // If this is not the initial click but their was previously a point that was selected/clicked,
            // check to see if that point is still within the point selection radius of the current mouse selection position
            else if (clickedPointIdx != (-1, -1))
            {
                int i = dataPointIndexMap[clickedPointIdx.Item1];
                int j = clickedPointIdx.Item2;
                // Hidden points are unselectable
                if (!pointIsHidden[j][i])
                {
                    float mouseToPointDistSqr = Vector2.SqrMagnitude(selectionPosition - pointPositions[j][i]);
                    if (mouseToPointDistSqr < selectionRadiusSqr)
                    {
                        // Only highlight the data point if it isn't masked
                        if (!linkedIndices[clickedPointIdx.Item1].Masked)
                        {
                            linkedIndices[clickedPointIdx.Item1].Highlighted = true;
                        }
                    }
                    else
                    {
                        linkedIndices[clickedPointIdx.Item1].Highlighted = false;
                    }
                }
            }
        }

        /// <summary>
        /// Selects all of the data points inside the given selection rectangle.
        /// </summary>
        /// <remarks>
        /// For a parallel coords plot, a "data point" consists of multiple point particles, any of which could be selected.
        /// </remarks>
        /// <param name="selectionRect">Transform of the selection rectangle.</param>
        public override void RectSelection(RectTransform selectionRect)
        {
            for (int i = 0; i < linkedIndices.Size; i++)
            {
                if (dataPointIndexMap.ContainsKey(i))
                {
                    bool rectContainsPoint = false;
                    // Check if any of the points that make up this "data point" (where for a parallel coords 
                    // plot a "data point" is a line renderer and series of points it connects) are inside
                    // the selection rect. If any of the individual points are inside the selection rect,
                    // highlight the entire "data point" that point is related to.
                    for (int j = 0; j < pointPositions.Length; j++)
                    {
                        // Hidden points are unselectable
                        if (!pointIsHidden[j][dataPointIndexMap[i]])
                        {
                            // Must translate point position to anchored position space space for rect.Contains() to work
                            rectContainsPoint = selectionRect.rect.Contains(pointPositions[j][dataPointIndexMap[i]] - selectionRect.anchoredPosition);
                            if (rectContainsPoint) break;
                        }
                    }
                    if (rectContainsPoint)
                    {
                        // Only highlight the data point if it isn't masked
                        if (!linkedIndices[i].Masked)
                        {
                            linkedIndices[i].Highlighted = true;
                        }
                    }
                    else
                    {
                        linkedIndices[i].Highlighted = false;
                    }
                }
                else
                {
                    linkedIndices[i].Highlighted = false;
                }
            }
        }

        /// <summary>
        /// Selects all the data points that the brush has passed over.
        /// </summary>
        /// <remarks>
        /// For a parallel coords plot, a "data point" consists of multiple point particles, any of which could be selected.
        /// </remarks>
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
                for (int i = 0; i < linkedIndices.Size; i++)
                {
                    if (dataPointIndexMap.ContainsKey(i))
                    {
                        for (int j = 0; j < pointPositions.Length; j++)
                        {
                            // Hidden points are unselectable
                            if (!pointIsHidden[j][dataPointIndexMap[i]])
                            {
                                // Highlight any points within the radius of the brush and unhighlight any that aren't
                                float pointToBrushDistSqr = Vector2.SqrMagnitude(pointPositions[j][dataPointIndexMap[i]] - prevBrushPosition);
                                if (pointToBrushDistSqr < brushRadiusSqr)
                                {
                                    // Only highlight the data point if it isn't masked
                                    if (!linkedIndices[i].Masked)
                                    {
                                        linkedIndices[i].Highlighted = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    linkedIndices[i].Highlighted = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        linkedIndices[i].Highlighted = false;
                    }
                }
            }
            // If this isn't the start of the selection, iterate through all data point positions
            // to highlight those which have been selected by the brush (taking into account the full movement 
            // of the brush since the previous frame)
            else
            {
                for (int i = 0; i < plottedDataPointIndices.Length; i++)
                {
                    // Get the index of the actual data point
                    int dataPointIndex = plottedDataPointIndices[i];
                    for (int j = 0; j < pointPositions.Length; j++)
                    {
                        // Hidden points are unselectable
                        if (!pointIsHidden[j][i])
                        {
                            // Trick to parametrize the line segment that the brush traveled since last frame and find the closest
                            // point on it to the current plot point
                            float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(pointPositions[j][i] - prevBrushPosition, brushDelta) / brushDelta.sqrMagnitude));
                            Vector2 closestPointOnLine = prevBrushPosition + t * brushDelta;
                            // Determine if point lies within the radius of the closest point to it on the line
                            float pointToBrushDistSqr = Vector2.SqrMagnitude(pointPositions[j][i] - closestPointOnLine);
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
}
