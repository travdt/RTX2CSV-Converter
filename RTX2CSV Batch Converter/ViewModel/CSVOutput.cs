using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RTX2CSV_Converter
{
    internal static class CSVOutput
    {
        public static int WriteCSV(List<double?> data, string outputFilePath, ConverterSettings.exportUnits unit, IProgress<int> progress)
        {
            return WriteSingleDataSet(ref data, ref outputFilePath, ref unit, progress, 0);
        }

        public static int WriteCSV(List<double?> data, string outputFilePath, ConverterSettings.exportUnits unit, IProgress<int> progress, double timeInterval)
        {
            return WriteSingleDataSet(ref data, ref outputFilePath, ref unit, progress, timeInterval);
        }

        public static int WriteConcatCSV(List<List<double?>> fileData, string outputFilePath, ConverterSettings.exportUnits unit, IProgress<int> progress)
        {
            return WriteMultipleDataSet(ref fileData, ref outputFilePath, ref unit, progress, 0);
        }

        public static int WriteConcatCSV(List<List<double?>> fileData, string outputFilePath, ConverterSettings.exportUnits unit, double timeInterval, IProgress<int> progress)
        {
            return WriteMultipleDataSet(ref fileData, ref outputFilePath, ref unit, progress, timeInterval);
        }

        private static int WriteSingleDataSet(ref List<double?> values, ref string outputFilePath, ref ConverterSettings.exportUnits unit, IProgress<int> progress, double timeInterval)
        {
            double? lineValue;
            int lineCount = 0;
            double time;

            int reportCompare = values.Count / 100;
            int reportCounter = 0;
            int progressPercent = 0;

            try
            {
                using (StreamWriter outputFile = new StreamWriter(outputFilePath))
                {
                    foreach (double? value in values)
                    {
                        lineValue = value;

                        if (unit == ConverterSettings.exportUnits.urad)
                        {
                            lineValue *= 1000;
                        }

                        //time reference
                        if (timeInterval != 0)
                        {
                            time = (lineCount++ * timeInterval);

                            string line = String.Format("{0:0.000000}, {1:0.00000000E+0}", time, lineValue);
                            outputFile.WriteLine(line);
                        }
                        else
                        {
                            lineCount++;
                            string line = String.Format("{0:0.00000000E+0}", lineValue);
                            outputFile.WriteLine(line);

                        }

                        reportCounter++;

                        if (reportCounter == reportCompare)
                        {
                            progress.Report(++progressPercent);
                            reportCounter = 0;
                        }
                    }

                    outputFile.Close();
                    return 1;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static int WriteMultipleDataSet(ref List<List<double?>> filesData, ref string outputFilePath, ref ConverterSettings.exportUnits unit, IProgress<int> progress, double timeInterval)
        {

            int dataLength = 0;
            double time;
            int lineCount = 0;


            //find largest data set
            foreach (var dataSet in filesData)
            {
                if (dataSet.Count > dataLength)
                {
                    dataLength = dataSet.Count;
                }
            }

            int reportCompare = dataLength / 100;
            int reportCounter = 0;
            int progressPercent = 0;

            double? value = null;

            try
            {
                using (StreamWriter outputFile = new StreamWriter(outputFilePath))
                {
                    StringBuilder line = new StringBuilder();

                    for (int i = 0; i < dataLength; i++)
                    {
                        //time reference
                        if(timeInterval != 0)
                        {
                            time = (lineCount++ * timeInterval);
                            line.Append(String.Format("{0:0.000000}", time));
                        }

                        foreach (var dataSet in filesData)
                        {
                            if (dataSet.Count >= i)
                            {
                                value = dataSet[i];
                            }
                            else
                            {
                                value = 0; //pad
                            }

                            //mrad/urad
                            if (unit == ConverterSettings.exportUnits.urad)
                            {
                                value *= 1000;
                            }

                            //add delimter
                            if (line.Length != 0)
                            {
                                line.Append(", ");
                            }

                            line.Append(String.Format("{0:0.00000000E+0}", value));
                        }

                        outputFile.WriteLine(line);
                        line.Clear();

                        reportCounter++;

                        if (reportCounter == reportCompare)
                        {
                            progress.Report(++progressPercent);
                            reportCounter = 0;
                        }
                    }

                    outputFile.Close();
                    return 1;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}