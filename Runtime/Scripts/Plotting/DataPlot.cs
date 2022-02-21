using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IVLab.Plotting
{
    /// <summary>
    /// Describes padding around a rectangular object.
    /// </summary>
    [System.Serializable]
    public class RectPadding
    {
        public float right;
        public float left;
        public float top;
        public float bottom;

        public RectPadding(float padding)
        {
            this.right = padding;
            this.left = padding;
            this.top = padding;
            this.bottom = padding;
        }

        public RectPadding(float right, float left, float top, float bottom)
        {
            this.right = right;
            this.left = left;
            this.top = top;
            this.bottom = bottom;
        }
    }

    /// <summary>
    /// An abstract class that declares (and defines) variables and methods that are ubiquitous to all 
    /// data plot implementations, such as plotting, updating, resizing and selection functionalities.
    /// </summary>
    public abstract class DataPlot : MonoBehaviour, ILinkedData
    {
        /// <summary>
        /// Contains all the information necessary to format and layout a data plot.
        /// </summary>
        [System.Serializable]
        public class PlotLayout
        {
            //  +-------------------+   +
            //  |                   |   | -> padding.top
            //  |   ^     . ..  .   |   +
            //  |   | ..    .       |
            //  |   | .   .         |
            //  |   +---inner--->   |   +
            //  |                   |   | -> padding.bottom
            //  +----size/outer-----+   +
            //
            //  +---+           +---+
            //    |               |
            // padding.left   padding.right

            /// <summary>
            /// Size of the full plot object. Also referred to as its "outer bounds".
            /// </summary>
            public Vector2 size;
            /// <summary>
            /// Padding between the outer bounds and the plot itself.
            /// </summary>
            public RectPadding padding;

            /// <summary>
            /// Constructor sets the size and padding of the layout.
            /// </summary>
            /// <param name="size"></param>
            /// <param name="padding"></param>
            public PlotLayout(Vector2 size, RectPadding padding)
            {
                this.size = size;
                this.padding = padding;
            }
        }

        [Header("Ubiquitous Plot Properties")]
        /// <summary> Dimensions of the plot's outer bounding rect. </summary>
        protected Vector2 outerBounds;
        /// <summary> Dimensions of the plot's inner bounding rect (i.e. the dimensions of the plot itself). </summary>
        protected Vector2 innerBounds;
        /// <summary> Offset of the plot from the center, determined by padding. </summary>
        protected Vector2 centerOffset;
        /// <summary> Padding between the outer bounding rect and the inner bounding rect. </summary>
        [SerializeField] protected RectPadding padding;
        /// <summary> Padding between the inner bounding rect and the selection rect. Allows
        /// the selection rect to be slightly larger than the plot itself, allowing for more forgiving
        /// selection interactions. </summary>
        [SerializeField] protected float selectionPadding;
        /// <summary> Radius at which the plot's data points can be selected when in <see cref="ClickSelectionMode"/>. </summary>
        [SerializeField] protected float clickSelectionRadius;
        /// <summary> Radius of the <see cref="BrushSelectionMode"/> brush for the plot. </summary>
        [SerializeField] protected float brushSelectionRadius;

        [Header("Ubiquitous Plot Styling Dependencies")]
        /// <summary> The default color of data points in the plot. </summary>
        [SerializeField] protected Color32 defaultColor;
        /// <summary> The color of highlighted data points in the plot. </summary>
        [SerializeField] protected Color32 highlightedColor;
        /// <summary> The color of masked data points in the plot. </summary>
        [SerializeField] protected Color32 maskedColor;

        [Header("Ubiquitous Plot Dependencies")]
        /// <summary> Rect mask used to conceal elements outside of the plot's inner bounds. </summary>
        [SerializeField] protected RectTransform plotMask;
        /// <summary> Rect transform used to parent and offset selection graphics. </summary>
        [SerializeField] protected RectTransform selectionGraphicsRect;
        /// <summary> Rect transform that visually makes up the plot's outer bounds. </summary>
        [SerializeField] protected RectTransform plotOuterRect;
        /// <summary> Rect transform that visually makes up the plot's inner bounds. </summary>
        [SerializeField] protected RectTransform plotInnerRect;
        /// <summary> Slightly enlarged version of the inner bounding rect that makes up the valid selection area of the plot. 
        /// Generated by taking <c>innerBounds + Vector2.One * selectionPadding</c>.</summary>
        [SerializeField] protected RectTransform plotSelectionRect;
        /// <summary> Button used to delete the plot. </summary>
        [SerializeField] protected GameObject deleteButton;
        /// <summary> Reference to the data table that the plot plots data from. </summary>
        protected DataTable dataTable;
        /// <summary> Reference to the linked indices data structure that contains the current state of all of the data points. </summary>
        protected LinkedIndices linkedIndices;
        /// <summary> The canvas that all plots are children of. </summary>
        protected Canvas plotsCanvas;
        /// <summary> Maps indices from the complete data point index space (i.e. linked index space) into 
        /// the local index space used by the plot (i.e. the subset of the linked indices that the plot plots). </summary>
        protected Dictionary<int, int> dataPointIndexMap;
        /// <summary> Array of data point indices plotted by this plot. </summary>
        protected int[] plottedDataPointIndices;
        /// <summary> Minimum value in each column of the data table for only the data points the plot plots. </summary>
        protected float[] plottedDataPointMins;
        /// <summary> Maximum value in each column of the data table for only the data points the plot plots. </summary>
        protected float[] plottedDataPointMaxes;

        /// <summary> Gets <see cref="selectionGraphicsRect"/>. </summary>
        public RectTransform SelectionGraphicsRect { get => selectionGraphicsRect; }
        /// <summary> Gets <see cref="plotOuterRect"/>. </summary>
        public RectTransform PlotOuterRect { get => plotOuterRect; }
        /// <summary> Gets <see cref="plotSelectionRect"/>. </summary>
        public RectTransform PlotSelectionRect { get => plotSelectionRect; }
        /// <summary> Gets <see cref="brushSelectionRadius"/>. </summary>
        public float BrushRadius { get => brushSelectionRadius; }

        /// <summary>
        /// Initializes the plot. Ideally called immediately after the plot has been instantiated and before
        /// anything else.
        /// </summary>
        /// <param name="dataPlotManager"> Manager of the plot: contains reference to the <see cref="DataManager"/> which controls the
        /// <see cref="DataTable"/> and <see cref="LinkedIndices"/> that the plot works from. </param>
        /// <param name="plotLayout"> Stores information about the size and padding of the plot. </param>
        /// <param name="dataPointIndices"> Array of data point indices the plot should display.
        /// If <c>null</c>, all data points will be displayed by default. </param>
        public virtual void Init(DataPlotManager dataPlotManager, PlotLayout plotLayout, int[] dataPointIndices = null)
        {
            // Initialize member variables
            plotsCanvas = GetComponentInParent<Canvas>();
            plotMask.GetComponent<Canvas>().sortingLayerName = PlottingUtilities.Consts.PlotsSortingLayerName;
            this.dataTable = dataPlotManager.DataManager.DataTable;
            this.linkedIndices = dataPlotManager.DataManager.LinkedIndices;

            // Set the plot's size
            SetPlotSize(plotLayout);

            // Fill dataPointIndices with indices of all data points if it is null
            if (dataPointIndices == null || (dataPointIndices.Length == 0))
            {
                plottedDataPointIndices = new int[dataTable.Height];
                for (int i = 0; i < plottedDataPointIndices.Length; i++)
                {
                    plottedDataPointIndices[i] = i;
                }
            }
            else
            {
                plottedDataPointIndices = dataPointIndices;
            }

            // Construct the index dictionary based on the selected indices
            dataPointIndexMap = new Dictionary<int, int>();
            for (int i = 0; i < plottedDataPointIndices.Length; i++)
            {
                dataPointIndexMap[plottedDataPointIndices[i]] = i;
            }

            // Determine the min and max of each column for the selected data points
            // (taking into account that there might be NaN points in the table)
            plottedDataPointMins = new float[dataTable.Width];
            plottedDataPointMaxes = new float[dataTable.Width];
            // Iterate through each column of the table (since we want the min/max of each column)
            for (int j = 0; j < dataTable.Width; j++)
            {
                // Keep traversing down the column until we find a starting value for min/max that isn't NaN
                int i = 0;
                float min;
                float max;
                do
                {
                   min = dataTable.Data(plottedDataPointIndices[i++], j);
                } while (float.IsNaN(min) && i < plottedDataPointIndices.Length);
                // If the entire column is filled with NaNs, just set min and max to 0
                if (float.IsNaN(min))
                {
                    min = 0;
                    max = 0;
                }
                // Otherwise use these as starting values for min and max
                else
                {
                    max = min;
                }
                // Iterate through the remaining selected points in the column and update the min/max if not NaN
                for (; i < plottedDataPointIndices.Length; i++)
                {
                    float val = dataTable.Data(plottedDataPointIndices[i], j);
                    if (!float.IsNaN(val))
                    {
                        if (val < min) { min = val; }
                        else if (val > max) { max = val; }
                    }
                }
                // Set the min and max
                plottedDataPointMins[j] = min;
                plottedDataPointMaxes[j] = max;
            }

            // Initialize the delete button for this plot
            deleteButton.GetComponent<Button>().onClick.AddListener(delegate { dataPlotManager.RemovePlot(this); });
        }

        /// <summary>
        /// Applies UI skin to plot.
        /// </summary>
        public virtual void ApplySkin(PlotUISkin plotSkin)
        {
            plotInnerRect.GetComponent<Image>().color = plotSkin.plotColor;
            plotOuterRect.GetComponent<Image>().color = plotSkin.borderColor;
            plotOuterRect.GetComponent<Outline>().effectColor = plotSkin.outlineColor;

            deleteButton.GetComponent<Image>().color = plotSkin.deleteButtonColor;

            defaultColor = plotSkin.defaultColor;
            highlightedColor = plotSkin.highlightedColor;
            maskedColor = plotSkin.maskedColor;
        }

        /// <summary> Resizes the plot and sets its new size. </summary>
        public void ResizePlot(PlotLayout plotLayout)
        {
            SetPlotSize(plotLayout);
        }

        /// <summary> Sets the size of the inner, outer, and selection bounds of the plot, 
        /// along with the plot mask. </summary>
        protected virtual void SetPlotSize(PlotLayout plotLayout)
        {
            // Set the outer bounds and padding using the given plot layout
            outerBounds = plotLayout.size;
            if (plotLayout.padding != null) padding = plotLayout.padding;
            if (padding.right + padding.left > outerBounds.x || padding.top + padding.bottom > outerBounds.y)
            {
                Debug.LogWarning("Plot Layout Warning: Padding larger than plot size");
            }
            // Determine the inner bounds and offset based on the padding
            innerBounds = outerBounds - new Vector2(padding.right + padding.left, padding.top + padding.bottom);
            centerOffset = new Vector2(padding.left - padding.right, padding.bottom - padding.top) / 2;
            // Resize the plot and its mask
            plotOuterRect.sizeDelta = outerBounds;
            plotInnerRect.sizeDelta = innerBounds;
            plotInnerRect.anchoredPosition = centerOffset;
            plotSelectionRect.sizeDelta = innerBounds + Vector2.one * selectionPadding;
            selectionGraphicsRect.anchoredPosition = -centerOffset;
            plotMask.sizeDelta = innerBounds;
            plotMask.anchoredPosition = centerOffset;
        }

        /// <summary> Updates a specific data point according to that data point's current linked index attributes. </summary>
        /// <param name="index">Index of data point that needs to be updated.</param>
        /// <param name="indexAttributes">Current attributes of the data point.</param>
        public abstract void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes indexAttributes);

        /// <summary> Refreshes the plot graphics to reflect most recent changes to plot data points. </summary>
        /// <remarks> Often called after a series of UpdateDataPoint() calls. </remarks>
        public abstract void RefreshPlotGraphics();

        /// <summary> Plots the selected data points and refreshes the plot graphics to match. </summary>
        public abstract void Plot();

        /// <summary> Controls the plot's reaction to the <see cref="ClickSelectionMode"/>. </summary>
        /// <param name="selectionPosition"> Current mouse/selector position in Canvas space. </param>
        /// <param name="selectionState"> Current <see cref="SelectionMode.State"/> of the selection. </param>
        public abstract void ClickSelection(Vector2 selectionPosition, SelectionMode.State selectionState);

        /// <summary> Controls the plot's reaction to the <see cref="RectSelectionMode"/>. </summary>
        /// <param name="selectionRect"> Current rect transform of the selection rect. </param>
        public abstract void RectSelection(RectTransform selectionRect);

        /// <summary> Controls the plot's reaction to the <see cref="BrushSelectionMode"/>. </summary>
        /// <param name="prevBrushPosition"> Previous position of the brush in Canvas space. </param>
        /// <param name="brushDelta"> Change in position from the brush' previous position to its current. </param>
        /// <param name="selectionState"> Current <see cref="SelectionMode.State"/> of the selection. </param>
        public abstract void BrushSelection(Vector2 prevBrushPosition, Vector2 brushDelta, SelectionMode.State selectionState);

    }
}
