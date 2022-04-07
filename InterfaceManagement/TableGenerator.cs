using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace EBuy.InterfaceManagement
{
    public static class TableGenerator
    {
        /// <summary>
        /// The second point called once the data headers and value selectors content has been converted into arrays.
        /// Convert the enumerable to an array before filling content 
        /// </summary>
        /// <typeparam name="T">The given ambiguous object type</typeparam>
        /// <param name="values">The enumerable list of objects left untouched</param>
        /// <param name="columnHeaders">The column headers (The variable names)</param>
        /// <param name="valSelectors">The values and what column they belong to</param>
        /// <returns></returns>
        public static string ToConsoleDataGrid<T>(this IEnumerable<T> values, string[] columnHeaders, params Func<T, object>[] valSelectors)
        {
            return ToConsoleDataGrid(values.ToArray(), columnHeaders, valSelectors);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static string SplitPropertyHeader(string val)
        {
            var regEx = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
            return regEx.Replace(val, " ");
        }

        /// <summary>
        /// The third point of call in the data grid process once values, headers and selectors are split.
        /// The headers will be filled with data and split by capital letters as a method of making the variable names readable
        /// The rows of data will be populated here depending on the row index and column being appended
        /// </summary>
        /// <typeparam name="T">The given ambiguous object type</typeparam>
        /// <param name="values">The list of objects as an array to reference by index</param>
        /// <param name="colHeaders">The list of string to be referenced by the values</param>
        /// <param name="valSelectors">The list of value selectors to select the data being processed for the given row</param>
        /// <returns></returns>
        public static string ToConsoleDataGrid<T>(this T[] values, string[] colHeaders, params Func<T, object>[] valSelectors)
        {
            Debug.Assert(colHeaders.Length == valSelectors.Length);

            var arrValues = new string[values.Length + 1, valSelectors.Length];

            // Fill the table headers
            for (int indexCol = 0; indexCol < arrValues.GetLength(1); indexCol++)
            {
                arrValues[0, indexCol] = SplitPropertyHeader(colHeaders[indexCol]); // Split the header by capital letter
            }
            
            // Fill the table row with data based on the current header
            for (int indexRow = 1; indexRow < arrValues.GetLength(0); indexRow++)
            {
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                {
                    object val = valSelectors[colIndex].Invoke(values[indexRow - 1]);
                    arrValues[indexRow, colIndex] = val != null ? val.ToString() : "null";
                }
            }

            return ToConsoleDataGrid(arrValues);
        }

        /// <summary>
        /// The final method to convert the array of values into a physical stringed table data grid
        /// </summary>
        /// <param name="arrValues">The array of final values in order of column headers to convert to string</param>
        /// <returns>The table data grid as a string</returns>
        public static string ToConsoleDataGrid(this string[,] arrValues)
        {
            int[] colWidths = GetColumnWidths(arrValues); // Get the width of the columns beng appended
            var headerDivider = new string('-', colWidths.Sum(i => i + 3) - 1);

            var sb = new StringBuilder(); // Build the string to return
            for (int indexRow = 0; indexRow < arrValues.GetLength(0); indexRow++)
            {
                for (int indexCol = 0; indexCol < arrValues.GetLength(1); indexCol++)
                {
                    // Append the string with padding from right to the string builder
                    string cell = arrValues[indexRow, indexCol];
                    cell = cell.PadRight(colWidths[indexCol]);
                    sb.Append(" | ");
                    sb.Append(cell);
                }

                // Print the end of line
                sb.Append(" | ");
                sb.AppendLine();

                // Print the splitter between headers
                if (indexRow == 0)
                {
                    sb.AppendFormat(" |{0}| ", headerDivider);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get the width required for the column
        /// Calculated by the data length and header length and what ever is largest
        /// </summary>
        /// <param name="arrValues">The array of values to determine width for</param>
        /// <returns>The exact width for each header</returns>
        private static int[] GetColumnWidths(string[,] arrValues)
        {
            var maxColumnsWidth = new int[arrValues.GetLength(1)];
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
                {
                    int newLength = arrValues[rowIndex, colIndex].Length;
                    int oldLength = maxColumnsWidth[colIndex];

                    if (newLength > oldLength)
                    {
                        maxColumnsWidth[colIndex] = newLength;
                    }
                }
            }

            return maxColumnsWidth;
        }

        /// <summary>
        /// Inital point of entry to convert an enumerable list of grid values into a table
        /// </summary>
        /// <typeparam name="T">The given ambiguous object type to be converted into a table</typeparam>
        /// <param name="gridValues">The enumerable list of items</param>
        /// <param name="valSelectors">The value selectors (Table columns) to choose from the grid values</param>
        /// <returns></returns>
        public static string ToConsoleDataGrid<T>(this IEnumerable<T> gridValues, params Expression<Func<T, object>>[] valSelectors)
        {
            var gridHeaders = valSelectors.Select(func => GetProperty(func).Name).ToArray();
            var gridSelectors = valSelectors.Select(exp => exp.Compile()).ToArray();
            return ToConsoleDataGrid(gridValues, gridHeaders, gridSelectors);
        }

        /// <summary>
        /// Get the property type value names of the value selectors given
        /// </summary>
        /// <typeparam name="T">The given ambiguous object type</typeparam>
        /// <param name="objectExpression">The expression of the object type to get the meta data of</param>
        /// <returns>Returns the attributes of the property and provides access to the property meta data</returns>
        private static PropertyInfo GetProperty<T>(Expression<Func<T, object>> objectExpression)
        {
            if (objectExpression.Body is UnaryExpression)
            {
                if ((objectExpression.Body as UnaryExpression).Operand is MemberExpression)
                {
                    return ((objectExpression.Body as UnaryExpression).Operand as MemberExpression).Member as PropertyInfo;
                }
            }

            if ((objectExpression.Body is MemberExpression))
            {
                return (objectExpression.Body as MemberExpression).Member as PropertyInfo;
            }
            return null;
        }
    }
}
