using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IVLab.Plotting
{
    /// <summary>
    /// Column-major order data table that can be initialized from a basic CSV, another data table, or with
    /// random data.
    /// <img src="../resources/DataTable/Sample.png"/>
    /// </summary>
    public class DataTable
    {
        protected string name;
        protected int height;
        protected int width;
        protected float[] data;
        protected string[] rowNames;
        protected string[] columnNames;
        protected float[] columnMins;
        protected float[] columnMaxes;
        protected bool containsNaNs;

        /// <summary> The name of the data table, usually set to be the name of the csv used. </summary>
        public string Name { get => name; set => name = value; }
        /// <summary> Height of the data table, also the number of rows. </summary>
        /// <remarks> If data table was created from csv, this does not take into account the header row! </remarks>
        public int Height { get => height; }
        /// <summary> Width of the data table, also the number of rows. </summary>
        /// <remarks> If data table was created from csv, this does not take into account the first column! </remarks>
        public int Width { get => width; }
        /// <summary> Name of each row in the data table, excluding the first row (which should be the header row). </summary>
        public string[] RowNames { get => rowNames; }
        /// <summary> Name of each column in the data table, excluding the first column (which should be the data point / row ID column).  </summary>
        public string[] ColumnNames { get => columnNames; }
        /// <summary> Tracks the minimum value in each column. </summary>
        public float[] ColumnMins { get => columnMins; }
        /// <summary> Tracks the maximum value in each column. </summary>
        public float[] ColumnMaxes { get => columnMaxes; }
        /// <summary> Whether or not any of the data the table contains is NaN. </summary>
        public bool ContainsNaNs { get => containsNaNs; }

        // REGEX delimiters used for reading CSV files
        //private string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        protected string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        //private char[] TRIM_CHARS = { '\"' };

        /// <summary> Default constructor initializes a data table with 10,000 random data points. </summary>
        public DataTable()
        {
            InitializeRandomTable();
        }
        /// <summary> Initializes a data table with a specified number of random data points. </summary>
        public DataTable(int numDataPoints)
        {
            InitializeRandomTable(numDataPoints);
        }
        /// <summary> Attempts to initialize a data table from the given csv filename, and initializes
        /// a random data table instead on failure. </summary>
        /// <param name="csvFilename">Name of the csv file, excluding .csv.</param>
        /// <remarks>
        /// The csv file should be of the following form:
        /// <img src="../resources/DataTable/Example.png"/>
        /// Namely, its first column should be made up of data point names/IDs,
        /// its first row should be made up of column names, and the rest should 
        /// be the actual numeric data values that will make up the table.
        /// </remarks>
        public DataTable(string csvFilename, bool csvHasRowNames = true, bool loadFromResources = true)
        {
            try
            {
                InitializeTableFromCSV(csvFilename, csvHasRowNames, loadFromResources);
            }
            catch
            {
                InitializeRandomTable();
                Debug.LogError("Failed to load CSV file \"" + csvFilename + "\"." +
                    "\nInitializing random DataTable instead.");
            }
        }

        /// <summary>
        /// Initializes a data table by converting a row-major data matrix to a column-major array, 
        /// setting additional attributes as it does so.
        /// </summary>
        /// <param name="data"><b>Row-major order</b> numeric data in matrix form. </param>
        /// <param name="rowNames">Name of each row of data given, should be the same
        /// length as each data column. </param>
        /// <param name="columnNames">Name of each column of data given, should be the same
        /// length as each data row. </param>
        /// <remarks>
        /// Refer to the image at the top of the <see cref="DataTable"/> page for clarification.
        /// </remarks>
        public DataTable(float[][] data, string[] rowNames, string[] columnNames, string name = "foo")
        {
            // Return if a data table cannot be constructed from the given data
            if (data.Length == 0 || data[0].Length == 0)
            {
                Debug.LogError("Failed to construct data table: Data array cannot be empty.");
                return;
            }
            else if (data.Length != rowNames.Length || data[0].Length != columnNames.Length)
            {
                Debug.LogWarning("Failed to construct data table: Size mismatch.");
                return;
            }

            // Set the names
            this.name = name;
            this.rowNames = rowNames;
            this.columnNames = columnNames;

            // Set the height/width of the data table using the given columns/rows
            height = rowNames.Length;
            width = columnNames.Length;

            // Initialize empty data arrays
            this.data = new float[width * height];
            columnMins = new float[width];
            columnMaxes = new float[width];

            // Convert from row-major order to column-major order,
            // determining the column mins and maxes in the process
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (float.IsNaN(data[i][j])) containsNaNs = true;
                    this.data[ArrayIdx(i, j)] = data[i][j];
                    columnMins[j] = (j == 0 || data[i][j] < columnMins[j]) ? data[i][j] : columnMins[j];
                    columnMaxes[j] = (j == 0 || data[i][j] > columnMaxes[j]) ? data[i][j] : columnMaxes[j];
                }
            }
        }

        /// <summary>
        /// Initializes a data table by directly taking a column-major order array of data, along with
        /// row and header names.
        /// </summary>
        /// <param name="data"><b>COlumn-major order</b> numeric data in array form. </param>
        /// <param name="rowNames">Name of each row of data given, should be the same
        /// length as each data column. </param>
        /// <param name="columnNames">Name of each column of data given, should be the same
        /// length as each data row. </param>
        public DataTable(float[] data, string[] rowNames, string[] columnNames, string name = "foo")
        {
            // Return if a data table cannot be constructed from the given data
            if (data.Length == 0)
            {
                Debug.LogError("Failed to construct data table: Data array cannot be empty.");
                return;
            }
            else if (data.Length != rowNames.Length * columnNames.Length)
            {
                Debug.LogWarning("Failed to construct data table: Size mismatch.");
                return;
            }

            // Set the names
            this.name = name;
            this.rowNames = rowNames;
            this.columnNames = columnNames;

            // Set the height/width of the data table using the given columns/rows
            height = rowNames.Length;
            width = columnNames.Length;

            // Initialize data arrays
            this.data = data;
            columnMins = new float[width];
            columnMaxes = new float[width];

            // Determine the column mins and maxes
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (float.IsNaN(data[ArrayIdx(i, j)])) containsNaNs = true;
                    columnMins[j] = (j == 0 || data[ArrayIdx(i, j)] < columnMins[j]) ? data[ArrayIdx(i, j)] : columnMins[j];
                    columnMaxes[j] = (j == 0 || data[ArrayIdx(i, j)] > columnMaxes[j]) ? data[ArrayIdx(i, j)] : columnMaxes[j];
                }
            }
        }

        /// <summary>
        /// Gets the ij'th element of the data stored in the data table. 
        /// </summary>
        /// <param name="i">Row index.</param>
        /// <param name="j">Column index.</param>
        /// <returns>Data value stored at position ij.</returns>
        public float Data(int i, int j)
        {
            return data[ArrayIdx(i, j)];
        }

        /// <summary>
        /// Initializes a data table of random size populated with random numeric data.
        /// </summary>
        /// <param name="dataPointsCount">Number of data points to add to the random table.</param>
        protected void InitializeRandomTable(int dataPointsCount = 10000)
        {
            // Set the name
            name = "Random";
            // Generate random table height and width
            height = dataPointsCount;
            width = Random.Range(3, 10);
            // Initialize necessary data arrays using generated height and width
            rowNames = new string[height];
            columnNames = new string[width];
            columnMins = new float[width];
            columnMaxes = new float[width];
            data = new float[width * height];
            // Populate table with random data column-wise
            for (int j = 0; j < width; j++)
            {
                // Sequentially name the current column
                columnNames[j] = "Column " + j;
                // Populate the column with random data
                float min = 0;
                float max = 0;
                for (int i = 0; i < height; i++)
                {
                    float dataValue = Random.Range(-100f, 100f);
                    min = (i == 0 || dataValue < min) ? dataValue : min;
                    max = (i == 0 || dataValue > max) ? dataValue : max;
                    data[ArrayIdx(i, j)] = dataValue;
                }
                columnMins[j] = min;
                columnMaxes[j] = max;
            }
            // Populate rowIDs
            for (int i = 0; i < height; i++)
            {
                rowNames[i] = "Row #" + i;
            }
        }

        /// <summary>
        /// Reads a csv file and loads it into corresponding data table arrays.
        /// </summary>
        /// <param name="filename">Filename (excluding .csv) of csv file located in Resources folder.</param>
        /// <param name="csvHasRowNames">Whether or not the first column of the csv is row names.</param>
        /// <remarks>
        /// This is a modified version of the CSVReader written here: https://bravenewmethod.com/2014/09/13/lightweight-csv-reader-for-unity/.
        /// </remarks>
        protected void InitializeTableFromCSV(string filename, bool csvHasRowNames, bool loadFromResources)
        {
            // Read the file from resources or using a direct file path
            string csvText;
            if (loadFromResources)
            {
                name = filename;
                // Load the csv file from the Resources folder as a TextAsset
                TextAsset csvData = Resources.Load(filename) as TextAsset;
                csvText = csvData.text;
            }
            else
            {
                name = Path.GetFileNameWithoutExtension(filename);
                csvText = File.ReadAllText(filename);
            }

            // Split the csv file into lines/rows of data, returning early if there is not data
            string[] rows = Regex.Split(csvText, LINE_SPLIT_RE);
            if (rows.Length <= 1) return;

            // Parse the first row as the header/column names of the data
            string[] header = rows[0].Split(','); //Regex.Split(rows[0], SPLIT_RE);

            // Record the height and width of our data table and initialize necessary data arrays
            // (where we subtract 2 from height to exclude the header row and any "phantom" rows at the bottom of the table,
            //  and we subtract 1 from width to exclude and the row names column from our data matrix, unless the csv doesn't have them)
            height = rows.Length - 2;
            width = csvHasRowNames ? header.Length - 1 : header.Length;
            rowNames = new string[height];
            columnNames = new string[width];
            columnMins = new float[width];
            columnMaxes = new float[width];
            data = new float[width * height];

            // Set the column names
            if (csvHasRowNames)
            {
                for (int j = 0; j < width; j++)
                {
                    columnNames[j] = header[j + 1];
                }
            } 
            else
            {
                columnNames = header;
            }

            // Extract the data row-by-row, skipping the header
            for (int i = 1; i < rows.Length; i++)
            {
                // Split the current row into an array of values and break early if there are no more
                string[] dataValues = rows[i].Split(','); //Regex.Split(rows[i], SPLIT_RE);
                if (dataValues.Length == 0 || dataValues[0] == "") break;
                // Record the row's data point ID
                rowNames[i - 1] = dataValues[0];

                // Loop through the columns of the row, skipping the ID column
                for (int j = csvHasRowNames ? 1 : 0; j < header.Length; j++)
                {
                    // Extract the individual data value
                    string dataValue = dataValues[j];
                    //dataValue = dataValue.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    // Attempt to parse the data value as a float
                    float parsedValue;
                    if (!float.TryParse(dataValue, out parsedValue) || float.IsNaN(parsedValue) || float.IsInfinity(parsedValue))
                    {
                        parsedValue = float.NaN;
                        containsNaNs = true;
                    }
                    // Add the data value to the table
                    if (csvHasRowNames)
                    {
                        data[ArrayIdx(i - 1, j - 1)] = parsedValue;
                        columnMins[j - 1] = (i == 1 || parsedValue < columnMins[j - 1]) ? parsedValue : columnMins[j - 1];
                        columnMaxes[j - 1] = (i == 1 || parsedValue > columnMaxes[j - 1]) ? parsedValue : columnMaxes[j - 1];
                    }
                    else
                    {
                        data[ArrayIdx(i - 1, j)] = parsedValue;
                        columnMins[j] = (i == 1 || parsedValue < columnMins[j]) ? parsedValue : columnMins[j];
                        columnMaxes[j] = (i == 1 || parsedValue > columnMaxes[j]) ? parsedValue : columnMaxes[j];
                    }
                }
            }
        }

        /// <summary>
        /// Returns the min value of a specified column.
        /// </summary>
        /// <param name="j">Index of the specified column.</param>
        public float ColumnMin(int j)
        {
            return columnMins[j];
        }

        /// <summary>
        /// Returns the max value of a specified column.
        /// </summary>
        /// <param name="j">Index of the specified column.</param>
        public float ColumnMax(int j)
        {
            return columnMaxes[j];
        }

        /// <summary>
        /// Whether or not the table is empty.
        /// </summary>
        public bool IsEmpty()
        {
            return (height == 0 || width == 0);
        }

        /// <summary>
        /// Converts matrix indices (i, j) to single array accessor index.
        /// </summary>
        protected int ArrayIdx(int i, int j)
        {
            return i + j * height;
        }
    }

    /// <summary>
    /// Special type of <see cref="DataTable"/> where each row has an additional identifier
    /// to indicate which "cluster" that data point is a part of.
    /// </summary>
    public class ClusterDataTable : DataTable
    {
        private List<Cluster> clusters = new List<Cluster>();
        /// <summary> Maps cluster IDs (first column of data table) to the index of
        /// the cluster they are a part of in the <see cref="clusters"/> list.</summary>
        private Dictionary<float, int> clusterIdToClusterIdx = new Dictionary<float, int>();

        /// <summary>
        /// Stores the id, start index (inclusive), end index (exclusive) and color of each cluster in the data table.
        /// </summary>
        public List<Cluster> Clusters { get => clusters; }

        /// <summary> Calls base <see cref="DataTable()"/> and then initializes clusters. </summary>
        public ClusterDataTable(Color[] clusterColors = null) : base() {
            InitializeClusters(clusterColors);
        }

        /// <summary> Calls base <see cref="DataTable(int)"/> and then initializes clusters. </summary>
        public ClusterDataTable(int numDataPoints, Color[] clusterColors = null) : base(numDataPoints)
        {
            InitializeClusters(clusterColors);
        }

        /// <summary> Calls base <see cref="DataTable(string, bool, bool)"/> and then initializes clusters. </summary>
        public ClusterDataTable(string csvFilename, bool csvHasRowNames = true, bool loadFromResources = true, Color[] clusterColors = null) 
            : base(csvFilename, csvHasRowNames, loadFromResources)
        {
            InitializeClusters(clusterColors);
        }

        /// <summary> Calls base <see cref="DataTable(float[][], string[], string[], string)"/> and then initializes clusters. </summary>
        public ClusterDataTable(float[][] data, string[] rowNames, string[] columnNames, string name = "foo", Color[] clusterColors = null) 
            : base(data, rowNames, columnNames, name)
        {
            InitializeClusters(clusterColors);
        }

        /// <summary> Calls base <see cref="DataTable(float[], string[], string[], string)"/> and then initializes clusters. </summary>
        public ClusterDataTable(float[] data, string[] rowNames, string[] columnNames, string name = "foo", Color[] clusterColors = null)
            : base(data, rowNames, columnNames, name)
        {
            InitializeClusters(clusterColors);
        }

        /// <summary>
        /// Initializes the clusters by saving each cluster as a pair of start/end indices,
        /// and then creates a dictionary mapping cluster identifiers (first column of the data table)
        /// to their cluster index in the <see cref="clusters"/> list. 
        /// </summary>
        private void InitializeClusters(Color[] clusterColors = null)
        {
            // Can't initialize clusters if the table is empty.
            if (IsEmpty()) return;

            // If no cluster colors were given, set them to black
            if (clusterColors == null) clusterColors = new Color[] { Color.black };

            // Iterate through the first column, adding a new cluster whenever
            // the identifier changes.
            int clusterStartIdx = 0;
            float clusterId = Data(0, 0);
            int clusterIdx = 0;
            for (int i = 0; i < height; i++)
            {
                if (Data(i, 0) != clusterId)
                {
                    clusterIdToClusterIdx[clusterId] = clusterIdx;
                    clusters.Add(new Cluster(clusterId, clusterStartIdx, i, clusterColors[clusterIdx++ % clusterColors.Length]));
                    clusterStartIdx = i;
                    clusterId = Data(i, 0);
                }
            }
            clusterIdToClusterIdx[clusterId] = clusterIdx;
            clusters.Add(new Cluster(clusterId, clusterStartIdx, height, clusterColors[clusterIdx % clusterColors.Length]));
        }

        /// <summary>
        /// Whether or not the table is empty.
        /// </summary>
        /// <remarks>
        /// Will still return true even if the table has a first column of cluster ids and nothing else.
        /// </remarks>
        public new bool IsEmpty()
        {
            return (height == 0 || width <= 1);
        }

        /// <summary>
        /// Converts a data point index to the index of the cluster that it is a part of.
        /// </summary>
        /// <param name="i">Data point index.</param>
        /// <returns>Index of cluster that that data point is a part of.</returns>
        public int DataIdxToClusterIdx(int i)
        {
            return clusterIdToClusterIdx[Data(i, 0)];
        }
    }
}
