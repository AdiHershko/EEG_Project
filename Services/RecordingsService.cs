using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EEG_Project.Services
{
    public class RecordingsService
    {
        public RecordingsService()
        {

        }

        public int[,] ReadRecordingFile(string path)
        {

            StreamReader sr = new StreamReader(path);
            var lines = new List<string[]>();
            int Row = 0;
            while (!sr.EndOfStream)
            {
                string[] Line = sr.ReadLine().Split(',');
                lines.Add(Line);
                Row++;
                Console.WriteLine(Row);
            }

            var data = lines.ToArray();
            int rows = data.GetLength(0);
            int cols = data[0].Length;
            int[,] arr = new int[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    arr[i, j] = int.Parse(data[i][j]);
                }
            }
            return arr;
        }



    }
}
