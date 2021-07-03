using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace IVLab.Plotting
{

    /// <summary>
    /// Creates "nice" axis labels by taking the min and max values that the axis needs
    /// to display and altering them to create an even tick spacing.
    /// </summary>
    public class NiceAxisLabel : MonoBehaviour
    {
        [SerializeField] private RectTransform axisRect;  // Rect of the main line making up the axis
        [SerializeField] private int tickCount;  // Max number of tick marks allowed
        [SerializeField] private float tickWidth;  // Width of the tick marks
        [SerializeField] private float tickHeight;  // Height of the tick marks
        [SerializeField] private string numberSortingLayerName = "2DPlots";
        [SerializeField] private int numberSortingOrder = 4;
        [SerializeField] private float numbersOffsetMag;  // Offset of the numbers away from the axis
        [SerializeField] private GameObject numberTextPrefab;  // Prefab used to instantiate number labels
        [SerializeField] private GameObject tickMarkPrefab;  // Prefab used to instantiate tick marks
        [SerializeField] private GameObject gridlinePrefab;  // Prefab used to instantiate gridlines
        [SerializeField] private Transform numbersParent;
        [SerializeField] private Transform tickMarksParent;
        [SerializeField] private Transform gridlinesParent;
        // Wilkinson labeling dependencies
        float[] Q = new float[] { 10f, 1f, 5f, 2f, 2.5f, 3f, 4f, 1.5f, 7f, 6f, 8f, 9f };
        float minCoverage = 0.8f;
        // Local variables set after calling GenerateNiceMinMax()
        private float niceMin;
        private float niceMax;
        private float niceTickSpacing;
        private float niceRange;
        // Local variables set after calling Generate_AxisLabel() method
        private Vector2 axisDirection;
        private Vector2 numbersOffset;
        private Vector2 tickMarkOffset;
        private Vector2 tickMarkScale;
        private Vector2 axisScale;
        private Vector2 gridlineOffset;
        private Vector2 gridlineScale;
        // Pooling lists for tick marks and axis numbers
        private List<GameObject> tickMarks = new List<GameObject>();
        private List<GameObject> axisNumbers = new List<GameObject>();
        private List<GameObject> gridlines = new List<GameObject>();
        // Private variables that can be set externally
        private bool inverted = false;  // Whether or not the axis should be inverted

        // Accessors
        public bool Inverted
        {
            get => inverted;
            set => inverted = value;
        }
        public float NiceMin
        {
            get => niceMin;
        }
        public float NiceMax
        {
            get => niceMax;
        }

        // Heckbert's Labeling algorithm, translated into C# from the revised version
        // in the following paper:
        // https://rdrr.io/cran/labeling/src/R/labeling.R
        private (float, float, float) Heckbert(float dmin, float dmax)
        {
            float range = HeckbertNicenum((dmax - dmin), false);
            float lstep = HeckbertNicenum(range / (tickCount - 1), true);
            float lmin = Mathf.Floor(dmin / lstep) * lstep;
            float lmax = Mathf.Ceil(dmax / lstep) * lstep;
            return (lmin, lmax, lstep * 10);
        }

        private float HeckbertNicenum(float x, bool round)
        {
            int e = Mathf.FloorToInt(Mathf.Log10(x));
            float f = -x / (Mathf.Pow(10, e));
            float nf;
            if (round)
            {
                if (f < 1.5) nf = 1;
                else if (f < 3) nf = 2;
                else if (f < 7) nf = 5;
                else nf = 10;
            }
            else
            {
                if (f <= 1) nf = 1;
                else if (f <= 2) nf = 2;
                else if (f <= 5) nf = 5;
                else nf = 10;
            }
            return nf * Mathf.Pow(10, e);
        }


        // Wilkinson's Labeling algorithm, translated into C# from the revised version
        // in the following paper:
        // https://rdrr.io/cran/labeling/src/R/labeling.R
        private (float, float, float) Wilkinson(float min, float max)
        {
            WilkinsonResult best = null;
            var range = new
            {
                start = Mathf.Max(Mathf.FloorToInt(tickCount / 2), 2),
                // In the original algorithm this was (6 * tickCount), but that occasionally created too many tick marks imo.
                end = Mathf.CeilToInt(3 * tickCount)
            };
            for (int k = range.start; k <= range.end; k++)
            {
                WilkinsonResult result = WilkinsonNiceScale(min, max, k);
                if (result != null && (best == null || result.score > best.score))
                {
                    best = result;
                }
            }
            // If a nice axis label could not be generated using Wilkinson,
            // just default to Heckbert for now. (This should only happen if
            // tickCount was set to something absurdly low).
            if (best == null)
            {
                return Heckbert(min, max);
            }
            else
            {
                return (best.min, best.max, best.step);
            }
        }

        private WilkinsonResult WilkinsonNiceScale(float min, float max, int k)
        {
            float range = max - min;
            int intervals = k - 1;
            float granularity = 1 - Mathf.Abs(k - tickCount) / min;

            float delta = range / intervals;
            float dbase = Mathf.Pow(10, Mathf.Floor(Mathf.Log10(delta)));

            WilkinsonResult best = null;
            for (int i = 0; i < Q.Length; i++)
            {
                float tdelta = Q[i] * dbase;
                float tmin = Mathf.Floor(min / tdelta) * tdelta;
                float tmax = tmin + intervals * tdelta;

                if (tmin <= min && tmax >= max)
                {
                    float roundness = 1 - ((i) - ((tmin <= 0 && tmax >= 0) ? 1 : 0)) / Q.Length;
                    float coverage = (max - min) / (tmax - tmin);
                    if (coverage > minCoverage)
                    {
                        float tnice = granularity + roundness + coverage;

                        if (tmin == -tmax || tmin == 0 || tmax == 1 || tmax == 100)
                            tnice += 1;
                        if (tmin == 0 && tmax == 1 || tmin == 0 && tmax == 100)
                            tnice += 1;

                        if (best == null || tnice > best.score)
                        {
                            best = new WilkinsonResult();
                            best.score = tnice;
                            best.min = tmin;
                            best.max = tmax;
                            best.step = tdelta;
                        }
                    }
                }
            }

            return best;
        }

        class WilkinsonResult
        {
            public float score;
            public float min;
            public float max;
            public float step;
        }

        // Determine a tick size that is "nice" given the current data range,
        // then use that to calculate and return "nice" min and max values.
        // NOTE: This function should be called prior to generating axis labels.
        /// <summary>
        /// Uses Wilkinson's algorithm (or Heckbert's if Wilkinson's fails) to generate a 
        /// "nice" set of min, max and tick spacing values for the given data range.
        /// </summary>
        /// <remarks>
        /// Must be called prior to generating axis labels.
        /// </remarks>
        /// <param name="min">Minimum value in the data range.</param>
        /// <param name="max">Maximum value in the data range. </param>
        /// <returns>
        /// Both the adjusted "nice" min and the "nice" max, in tuple form.
        /// </returns>
        public (float, float) GenerateNiceMinMax(float min, float max)
        {
            // Account for special case when min and max are equal
            if (min == max)
            {
                min -= 1;
                max += 1;
            }
            (niceMin, niceMax, niceTickSpacing) = Wilkinson(min, max);
            niceRange = niceMax - niceMin;
            return (niceMin, niceMax);
        }

        /// <summary>
        /// Sets the necessary local variables before calling <see cref="GenerateAxisLabel(Vector2, float, bool)"/>
        /// in order to generate the axis labels in the x-direction.
        /// </summary>
        /// <param name="sourcePos">Start position of the axis (where the minimum value is located)</param>
        /// <param name="bounds">Bounds of the plot.</param>
        /// <param name="drawGridlines">Whether or not to draw gridlines as part of the axis labels.</param>
        public void GenerateXAxisLabel(Vector2 sourcePos, Vector2 bounds, bool drawGridlines = false)
        {
            // The x-axis points in either the right or left direction
            if (!inverted)
            {
                axisDirection = Vector2.right;
            }
            else
            {
                axisDirection = Vector2.left;
            }
            // Label numbers are offset below the x-axis
            tickMarkOffset = Vector2.down * tickHeight / 2;
            numbersOffset = Vector2.down * numbersOffsetMag;
            gridlineOffset = Vector2.up * bounds.y / 2;
            // Tick marks are scaled in the y direction and axis is scaled in the x
            tickMarkScale = new Vector2(tickWidth, tickHeight);
            axisScale = new Vector2(bounds.x, tickWidth);
            gridlineScale = new Vector2(tickWidth, bounds.y);

            // Set the alignment of the numbers text
            numberTextPrefab.GetComponent<TextMeshProUGUI>().horizontalAlignment = HorizontalAlignmentOptions.Center;
            numberTextPrefab.GetComponent<TextMeshProUGUI>().verticalAlignment = VerticalAlignmentOptions.Top;

            // Generate the actual axis labels now that necessary local variables have been set
            GenerateAxisLabel(sourcePos, bounds.x, drawGridlines);
        }

        /// <summary>
        /// Sets the necessary local variables before calling <see cref="GenerateAxisLabel(Vector2, float, bool)"/>
        /// in order to generate the axis labels in the y-direction.
        /// </summary>
        /// <param name="sourcePos">Start position of the axis (where the minimum value is located)</param>
        /// <param name="bounds">Bounds of the plot.</param>
        /// <param name="drawGridlines">Whether or not to draw gridlines as part of the axis labels.</param>
        public void GenerateYAxisLabel(Vector2 sourcePos, Vector2 bounds, bool drawGridlines = false)
        {
            // The y-axis points in either the up or down direction
            if (!inverted)
            {
                axisDirection = Vector2.up;
            }
            else
            {
                axisDirection = Vector2.down;
            }
            // Label numbers are offset to the left of the y-axis
            tickMarkOffset = Vector2.left * tickHeight / 2;
            numbersOffset = Vector2.left * numbersOffsetMag;
            gridlineOffset = Vector2.right * bounds.x / 2;
            // Tick marks are scaled in the x direction and axis is scaled in the y
            tickMarkScale = new Vector2(tickHeight, tickWidth);
            axisScale = new Vector2(tickWidth, bounds.y);
            gridlineScale = new Vector2(bounds.x, tickWidth);

            // Set the alignment of the numbers text
            numberTextPrefab.GetComponent<TextMeshProUGUI>().horizontalAlignment = HorizontalAlignmentOptions.Right;
            numberTextPrefab.GetComponent<TextMeshProUGUI>().verticalAlignment = VerticalAlignmentOptions.Middle;

            // Generate the actual axis labels now that necessary local variables have been set
            GenerateAxisLabel(sourcePos, bounds.y, drawGridlines);
        }

        /// <summary>
        /// Called internally after either GenerateXAxisLabel() or GenerateYAxisLabel() in order to actually generate
        /// and display the axis labels.
        /// </summary>
        /// <param name="sourcePos">Start position of the axis.</param>
        /// <param name="length">Length of the axis.</param>
        /// <param name="drawGridlines">Whether or not to draw gridlines.</param>
        private void GenerateAxisLabel(Vector2 sourcePos, float length, bool drawGridlines = false)
        {
            // Position and scale the main line of the axis
            axisRect.anchoredPosition = sourcePos + axisDirection * length / 2;
            axisRect.sizeDelta = axisScale;

            // Determine tick spacing scaled to actual axis length
            float scaledTickSpacing = length / niceRange * niceTickSpacing;

            // Reset and then iterate through all tick marks
            int i;
            float curTickOffset = 0;
            for (i = 0; i <= Mathf.CeilToInt(niceRange / niceTickSpacing); i++, curTickOffset = i * scaledTickSpacing)
            {
                // Instantiate new tick marks and numbers if there aren't enough
                if (i >= tickMarks.Count)
                {
                    GameObject tickMarkInst = Instantiate(tickMarkPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    tickMarks.Add(tickMarkInst);

                    GameObject numberTextInst = Instantiate(numberTextPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    axisNumbers.Add(numberTextInst);
                }
                // Update old tick marks and numbers
                tickMarks[i].SetActive(true);
                RectTransform tickMarkRect = tickMarks[i].GetComponent<RectTransform>();
                tickMarkRect.SetParent(tickMarksParent);
                tickMarks[i].transform.localScale = Vector3.one;
                tickMarkRect.anchoredPosition3D = sourcePos + axisDirection * curTickOffset + tickMarkOffset;
                tickMarkRect.sizeDelta = tickMarkScale;

                axisNumbers[i].SetActive(true);
                axisNumbers[i].GetComponent<TextMeshProUGUI>().text = "" + (niceMin + i * niceTickSpacing);
                RectTransform numberRect = axisNumbers[i].GetComponent<RectTransform>();
                numberRect.SetParent(numbersParent);
                Canvas axisNumberCanvas = axisNumbers[i].GetComponent<Canvas>() != null ? axisNumbers[i].GetComponent<Canvas>() : axisNumbers[i].AddComponent<Canvas>();
                axisNumberCanvas.overrideSorting = true;
                axisNumberCanvas.sortingLayerName = numberSortingLayerName;
                axisNumberCanvas.sortingOrder = numberSortingOrder;
                axisNumbers[i].transform.localScale = Vector3.one;
                numberRect.anchoredPosition3D = sourcePos + axisDirection * curTickOffset + numbersOffset;

                if (drawGridlines)
                {
                    if (i > 0)
                    {
                        if (i - 1 >= gridlines.Count)
                        {
                            GameObject gridlineInst = Instantiate(gridlinePrefab, Vector3.zero, Quaternion.identity) as GameObject;
                            gridlines.Add(gridlineInst);
                        }

                        gridlines[i - 1].SetActive(true);
                        RectTransform gridlineRect = gridlines[i - 1].GetComponent<RectTransform>();
                        gridlineRect.SetParent(gridlinesParent);
                        gridlines[i - 1].transform.localScale = Vector3.one;
                        gridlineRect.anchoredPosition3D = sourcePos + axisDirection * curTickOffset + gridlineOffset;
                        gridlineRect.sizeDelta = gridlineScale;
                    }
                }
            }
            // Deactivate any remaining tick marks.
            for (; i < tickMarks.Count; i++)
            {
                tickMarks[i].SetActive(false);
                axisNumbers[i].SetActive(false);
                if (drawGridlines && i > 0)
                {
                    gridlines[i - 1].SetActive(false);
                }
            }
        }
    }
}
