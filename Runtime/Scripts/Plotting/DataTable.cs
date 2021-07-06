using UnityEngine;
using System.Text.RegularExpressions;

namespace IVLab.Plotting
{
    /// <summary>
    /// Column-major data table that can be initialized from a basic CSV.
    /// <img src="../resources/DataTable/Sample.png"/>
    /// <b>Remember: </b> In accordance with data being stored in column-major order, 
    /// in order to access the ij'th data element one must use <c>DataTable.Data[j][i]</c>.
    /// </summary>
    public class DataTable
    {
        private int height;
        private int width;
        private float[][] data;
        private string[] rowNames;
        private string[] columnNames;
        private float[] columnMins;
        private float[] columnMaxes;

        /// <summary> Height of the data table, also the number of rows. </summary>
        /// <remarks> If data table was created from csv, this does not take into account the header row! </remarks>
        public int Height { get => height; }
        /// <summary> Width of the data table, also the number of rows. </summary>
        /// <remarks> If data table was created from csv, this does not take into account the first column! </remarks>
        public int Width { get => width; }
        /// <summary> 2x2 matrix of the numeric data stored in the data table.</summary>
        /// <remarks> Data is stored in column-major order! </remarks>
        /// <example>
        /// Column-major order means that if we wish to access the ij'th element of the data matrix, 
        /// we must use
        /// <code>
        /// float element_ij = data[j][i];
        /// </code>
        /// (instead of <c>data[i][j]</c>)
        /// </example>
        public float[][] Data { get => data; }
        /// <summary> Name of each row in the data table, excluding the first row (which should be the header row). </summary>
        public string[] RowNames { get => rowNames; }
        /// <summary> Name of each column in the data table, excluding the first column (which should be the data point / row ID column).  </summary>
        public string[] ColumnNames { get => columnNames; }
        /// <summary> Tracks the minimum value in each column. </summary>
        public float[] ColumnMins { get => columnMins; }
        /// <summary> Tracks the maximum value in each column. </summary>
        public float[] ColumnMaxes { get => columnMaxes; }

        // REGEX delimiters used for reading CSV files
        private string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        private char[] TRIM_CHARS = { '\"' };
        private float PARSE_ERROR = -9999999f;

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
        /// <param name="filename">Name of the csv file, excluding .csv.</param>
        /// <remarks>
        /// The csv file should be of the following form:
        /// <img src="../resources/DataTable/Example.png"/>
        /// Namely, its first column should be made up of data point names/IDs,
        /// its first row should be made up of column names, and the rest should 
        /// be the actual numeric data values that will make up the table.
        /// </remarks>
        public DataTable(string filename)
        {
            try
            {
                InitializeTableFromCSV(filename);
            }
            catch
            {
                InitializeRandomTable();
                Debug.LogError("Failed to load CSV file \"" + filename + "\" from Resources folder");
            }
        }

        /// <summary>
        /// Initializes a data table by converting a row-major data matrix to a column-major one, 
        /// setting additional attributes as it does so.
        /// </summary>
        /// <param name="data"><b>Row-major order</b> numeric data in matrix form. </param>
        /// <param name="rowIDs">Name of each row of data given, should be the same
        /// length as each data column. </param>
        /// <param name="columnNames">Name of each column of data given, should be the same
        /// length as each data row. </param>
        /// <remarks>
        /// Refer to the image at the top of the <see cref="DataTable"/> page for clarification.
        /// </remarks>
        public DataTable(float[][] data, string[] rowNames, string[] columnNames)
        {
            // Set the row/column names
            this.rowNames = rowNames;
            this.columnNames = columnNames;

            // Set the height/width of the data table using the given columns/rows
            height = rowNames.Length;
            width = columnNames.Length;

            // Initialize empty data arrays
            this.data = new float[width][];
            columnMins = new float[width];
            columnMaxes = new float[width];
            for (int j = 0; j < width; j++)
            {
                this.data[j] = new float[height];
            }

            // Convert from row-major order to column-major order,
            // determining the column mins and maxes in the process
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    this.data[j][i] = data[i][j];
                    columnMins[j] = (j == 0 || data[i][j] < columnMins[j]) ? data[i][j] : columnMins[j];
                    columnMaxes[j] = (j == 0 || data[i][j] > columnMaxes[j]) ? data[i][j] : columnMaxes[j];
                }
            }
        }

        /// <summary>
        /// Initializes a data table of random size populated with random numeric data.
        /// </summary>
        /// <param name="dataPointsCount">Number of data points to add to the random table.</param>
        private void InitializeRandomTable(int dataPointsCount = 10000)
        {
            // Generate random table height and width
            height = dataPointsCount;
            width = Random.Range(3, 10);
            // Initialize necessary data arrays using generated height and width
            rowNames = new string[height];
            columnNames = new string[width];
            columnMins = new float[width];
            columnMaxes = new float[width];
            data = new float[width][];
            // Populate table with random data column-wise
            for (int j = 0; j < width; j++)
            {
                // Sequentially name the current column
                columnNames[j] = "Column " + j;
                // Populate the column with random data
                float min = 0;
                float max = 0;
                float[] column = new float[height];
                for (int i = 0; i < height; i++)
                {
                    float dataValue = Random.Range(-100f, 100f);
                    min = (i == 0 || dataValue < min) ? dataValue : min;
                    max = (i == 0 || dataValue > max) ? dataValue : max;
                    column[i] = dataValue;
                }
                // Add the column to the table
                data[j] = column;
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
        /// <remarks>
        /// This is a modified version of the CSVReader written here: https://bravenewmethod.com/2014/09/13/lightweight-csv-reader-for-unity/.
        /// </remarks>
        private void InitializeTableFromCSV(string filename)
        {
            // Load the csv file from the Resources folder as a TextAsset
            TextAsset csvData = Resources.Load(filename) as TextAsset;

            // Split the csv file into lines/rows of data, returning early if there is not data
            string[] rows = Regex.Split(csvData.text, LINE_SPLIT_RE);
            if (rows.Length <= 1) return;

            // Parse the first row as the header/column names of the data
            string[] header = Regex.Split(rows[0], SPLIT_RE);

            // Record the height and width of our data table and initialize necessary data arrays
            // (where we subtract 2 from height to exclude the header row and any "phantom" rows at the bottom of the table,
            //  and we subtract 1 from width to exclude and the id column from our data matrix)
            height = rows.Length - 2;
            width = header.Length - 1;
            rowNames = new string[height];
            columnNames = new string[width];
            columnMins = new float[width];
            columnMaxes = new float[width];
            data = new float[width][];

            // Initialize empty column arrays and set the column names
            for (int j = 0; j < width; j++)
            {
                data[j] = new float[height];
                columnNames[j] = header[j + 1];
            }

            // Extract the data row-by-row, skipping the header
            for (int i = 1; i < rows.Length; i++)
            {
                // Split the current row into an array of values and break early if there are no more
                string[] dataValues = Regex.Split(rows[i], SPLIT_RE);
                if (dataValues.Length == 0 || dataValues[0] == "") break;
                // Record the row's data point ID
                rowNames[i - 1] = dataValues[0];

                // Loop through the columns of the row, skipping the ID column
                for (int j = 1; j < header.Length; j++)
                {
                    // Extract the individual data value
                    string dataValue = dataValues[j];
                    dataValue = dataValue.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    // Attempt to parse the data value as a float
                    float parsedValue;
                    if (!float.TryParse(dataValue, out parsedValue))
                        parsedValue = PARSE_ERROR;
                    // Add the data value to the table
                    data[j - 1][i - 1] = parsedValue;
                    columnMins[j - 1] = (i == 1 || parsedValue < columnMins[j - 1]) ? parsedValue : columnMins[j - 1];
                    columnMaxes[j - 1] = (i == 1 || parsedValue > columnMaxes[j - 1]) ? parsedValue : columnMaxes[j - 1];
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
        public bool Empty()
        {
            return (height == 0 || width == 0);
        }
    }
}
