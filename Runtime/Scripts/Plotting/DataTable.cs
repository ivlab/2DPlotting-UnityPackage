﻿using UnityEngine;
using System.Text.RegularExpressions;

namespace IVLab.Plotting
{
    /// <summary>
    /// Column-major data table that can be initialized from a basic CSV.
    /// </summary>
    public class DataTable
    {
        // Private member variables
        private int height;
        private int width;
        private float[][] data;  // NOTE: Data is stored in column-major order! (e.g. to access element ij, use data[j][i])
        private string[] rowIDs;  // Each row of the data table represents a "DataPoint" with a unique ID
        private string[] columnNames;
        private float[] columnMins;  // Tracks the minimum value in each column
        private float[] columnMaxes;  // Tracks the maximum value in each column

        // Public accessors 
        public int Height { get => height; }
        public int Width { get => width; }
        public float[][] Data { get => data; }
        public string[] RowIDs { get => rowIDs; }
        public string[] ColumnNames { get => columnNames; }
        public float[] ColumnMins { get => columnMins; }
        public float[] ColumnMaxes { get => columnMaxes; }

        // REGEX delimiters used for reading CSV files
        private string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        private char[] TRIM_CHARS = { '\"' };
        private float PARSE_ERROR = -9999999f;

        /// <summary> Default constructor initializes random table with 10,000 datapoints. </summary>
        public DataTable()
        {
            InitializeRandomTable();
        }
        /// <summary> Initializes a random table with a specified number of datapoints. </summary>
        public DataTable(int numDataPoints)
        {
            InitializeRandomTable(numDataPoints);
        }
        /// <summary> Filename constructor that attempts to initialize a table from the given csv filename. </summary>
        /// <param name="filename">Name of the csv file, excluding .csv.</param>
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
        /// Initializes a data table of random size populated with random numeric data.
        /// </summary>
        /// <param name="dataPointsCount">Number of data points to add to the random table.</param>
        public void InitializeRandomTable(int dataPointsCount = 10000)
        {
            // Generate random table height and width
            height = dataPointsCount;
            width = Random.Range(3, 10);
            // Initialize necessary data arrays using generated height and width
            rowIDs = new string[height];
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
                rowIDs[i] = "Row #" + i;
            }
        }

        /// <summary>
        /// Reads a csv file and loads it into corresponding data table arrays.
        /// </summary>
        /// <remarks>
        /// This is a modified version of the CSVReader written here: https://bravenewmethod.com/2014/09/13/lightweight-csv-reader-for-unity/.
        /// </remarks>
        public void InitializeTableFromCSV(string filename)
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
            rowIDs = new string[height];
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
                rowIDs[i - 1] = dataValues[0];

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
        /// Returns the min and max of a specified column.
        /// </summary>
        /// <param name="j">Index of the specified column.</param>
        public (float, float) ColumnDataMinMax(int j)
        {
            return (columnMins[j], columnMaxes[j]);
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
