using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriterCsv
{
    public class WriterCsv
    {
        private int col;
        private StringBuilder newLine;
        private StringBuilder csv;
        private StringBuilder csvHeader;
        private string path;

        public WriterCsv(string _path, int _col)
        {
            col = _col;
            path = _path;
            csv = new StringBuilder();
            csvHeader = new StringBuilder();
            newLine = new StringBuilder();
            createNewLine();
            //  Clear();
        }

        private void createNewLine()
        {
            for (int i = 0; i < col - 1; i++)
            {
                newLine.Append("{" + i + "};");
            }
            newLine.Append("{" + (col - 1) + "}");
        }

        public void writeRow(string[] elements)
        {
            string line = string.Format(newLine.ToString(), elements);
            csv.AppendLine(line);
        }

        public void writeHeader(string[] elements)
        {
            string line = string.Format(newLine.ToString(), elements);
            csvHeader.AppendLine(line);
        }

        public void ChangeRow(string[] elements)
        {
            string line = string.Format(newLine.ToString(), elements);
            csv.Clear();
            csv.AppendLine(line);
        }

        public void writeToFile()
        {
            File.AppendAllText(path, csv.ToString());
        }

        private void Clear()
        {
            File.WriteAllText(path, string.Empty);
        }

        public void appendColumn(List<string> newColumnData)
        {
            /*
             * OLD data:
            * Column1,Column2,Column3
            * A,B,C
            */
            //total number of records should be same
            //List<string> newColumnData = new List<string>() { "D" };
            List<string> lines = File.ReadAllLines(path).ToList();
            //add new column to the header row
            //  lines[0] += ",Column 4";
            int index = 0;
            //add new column value for each row.
            lines.ToList().ForEach(line =>
            {
                //-1 for header
                //       lines[index] += "," + newColumnData[index - 1];
                lines[index] += "," + newColumnData[index];
                index++;
            });
            //write the new content
            File.WriteAllLines(path, lines);
            /*
                * NEW data:
                * Column1,Column2,Column3,Column 4
                * A,B,C,D
                */
        }
    }
}