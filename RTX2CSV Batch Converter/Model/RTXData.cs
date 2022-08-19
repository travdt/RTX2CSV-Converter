using System;
using System.Collections.Generic;

namespace RTX2CSV_Converter
{
    internal class RTXData
    {
        public string InputFilePath { get; set; }
        public string Filename { get; set; }
        public string Owner { get; set; }
        public string VersionNumber { get; set; }
        public string FileType { get; set; }
        public double Velocity { get; set; }
        public double SampleRate { get; set; }
        public double SampleNumber { get; set; }
        public double TriggerPoint { get; set; }
        public double TriggerInterval { get; set; }
        public double ActualSampleRate { get; set; }
        public int[] Flags { get; set; }
        public string Machine { get; set; }
        public string SerialNumber { get; set; }
        public DateTime Date { get; set; }
        public string By { get; set; }
        public string Axis { get; set; }
        public string Location { get; set; }
        public List<double?> Data { get; set; }

        public RTXData()
        {
            Flags = new int[5];
            Data = new List<double?>();
        }

        public double GetDataInterval()
        {
            return 1 / ActualSampleRate;
        }


        public List<double?> GetAveragedData(int dataPoints)
        {
            List<double?> averagedData = new List<double?>();
            double? dataSum = 0;

            //pad start
            for (int i = 0; i < dataPoints / 2; i++)
            {
                averagedData.Add(null);
            }

            //get averages
            for (int i = 0; i < dataPoints; i++)
            {
                dataSum += Data[i];
            }
            averagedData.Add(dataSum / dataPoints);

            for (int i = dataPoints; i < Data.Count - 1; i++)
            {
                dataSum -= Data[i - dataPoints];
                dataSum += Data[i];
                averagedData.Add(dataSum / dataPoints);
            }

            //pad end
            for (int i = 0; i < dataPoints / 2; i++)
            {
                averagedData.Add(null);
            }

            return averagedData;
        }
    }
}
