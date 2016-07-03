using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Csv
{/// <summary>
/// Writes data to the cv file in the path property.
/// </summary>
    public class CsvWriter
    {
        #region Private properties

        private int col;

        /// <summary>
        /// The formation of a csv line depends on the column count {0};{1};..;{col}.
        /// </summary>
        private StringBuilder newLine;

        /// <summary>
        /// The content for the csv.
        /// </summary>
        private StringBuilder csv;

        /// <summary>
        /// File name.
        /// </summary>
        private string path;

        #endregion Private properties

        public CsvWriter(string _path, int _col)
        {
            col = _col;
            Init(_path);
        }

        public CsvWriter(string _path, string[] header)
        {
            col = header.Length;
            Init(_path);
            WriteRow(header);
        }

        /// <summary>
        /// Initializes components and creates the file if it doesn't exist already.
        /// </summary>
        /// <param name="_path"></param>
        private void Init(string _path)
        {
            path = _path;
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            csv = new StringBuilder();
            newLine = new StringBuilder();
            CreateNewLine();
        }

        /// <summary>
        /// Creates the formation of a new csv line: {0};{1};..;{col}.
        /// </summary>
        private void CreateNewLine()
        {
            for (int i = 0; i < col - 1; i++)
            {
                newLine.Append("{" + i + "};");
            }
            //last one without the ';'
            newLine.Append("{" + (col - 1) + "}");
        }

        /// <summary>
        /// Writes a row with the amount of columns from the paramater and not the standard newLine formation.
        /// </summary>
        /// <param name="col"></param>
        /// <param name="elements"></param>
        public void WriteRow(int col, List<string> elements)
        {
            newLine.Clear();
            for (int i = 0; i < col - 1; i++)
            {
                newLine.Append("{" + i + "};");
            }
            newLine.Append("{" + (col - 1) + "}");
            csv.AppendLine(string.Format(newLine.ToString(), elements.ToArray()));
            newLine.Clear();
        }

        /// <summary>
        /// Writes a line to the csv the line must be ";" separated: {0};{1};..;{col}
        /// </summary>
        /// <param name="line"></param>
        public void writeSeperatedRow(string line)
        {
            csv.AppendLine(line);
        }

        /// <summary>
        /// Writes an array, the array length must be the same as the column count of the newLine format
        /// </summary>
        /// <param name="elements"></param>
        public void WriteRow(string[] elements)
        {
            csv.AppendLine(string.Format(newLine.ToString(), elements));
        }

        /// <summary>
        /// Writes one element, the newLine format must have one column.
        /// </summary>
        /// <param name="element"></param>
        public void WriteRow(string element)
        {
            csv.AppendLine(string.Format(newLine.ToString(), new string[] { element }));
        }

        /// <summary>
        /// Write the csv content string to the file.
        /// </summary>
        public void WriteToFile()
        {
            try
            {
                File.AppendAllText(path, csv.ToString());
            }
            catch (IOException)
            {
                Console.WriteLine("Csv already open.");
            }
            finally
            {
                csv.Clear();
            }
        }

        /// <summary>
        /// Clears the current file.
        /// </summary>
        private void Clear()
        {
            File.WriteAllText(path, string.Empty);
        }

        /// <summary>
        /// Appends a column to the csv
        /// </summary>
        /// <param name="newColumnData"></param>
        /// <param name="header"></param>
        public void AppendColumn(List<string> newColumnData, string header = "")
        {
            //Read the whole csv
            List<string> lines = File.ReadAllLines(path).ToList();
            //Count of lines in new column
            int newColumnLines = newColumnData.Count;
            if (!header.Equals(""))
            {
                newColumnLines++;
            }

            //total number of records should be same as the record count of the new column
            if (newColumnLines != lines.Count)
            {
                throw new Exception("The line count of the new column and the csv is not the same");
            }

            int index = 0;
            //add new column to the header row
            if (!header.Equals(""))
            {
                lines[index] += ";" + header;
                index++;
            }

            //add new column value for each row.
            lines.ToList().ForEach(line =>
            {
                lines[index] += ";" + newColumnData[index];
                index++;
            });

            //write the new content
            File.WriteAllLines(path, lines);
        }
    }
}