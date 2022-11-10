using System;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using ViewModel;
using System.Diagnostics;
using System.Threading.Tasks;


namespace RTX2CSV_Converter {

    internal class MainViewModel : Presenter
    {
        private string _guiInputPath;
        private bool _writeToInputPath;
        private bool _canEditOutput;
        private int _fileConversionProgress;
        private int _fileWriteProgress;
        private string _statusText;
        private IErrorNotification err = new ErrorNotification();
        private ConverterSettings convertConfig = new ConverterSettings();

        public MainViewModel()
        {
            FileConversionProgress = 0;
            FileWriteProgress = 0;
            WriteToInputPath = true;
        }


        #region Bound Properties

        public string DisplayInputPath
        {
            get { return _guiInputPath; }
            set
            {
               _guiInputPath = value;
                OnPropertyChanged("DisplayInputPath");
            }
        }

        public string OutputPath
        {
            get { return convertConfig.OutputPath; }
            set
            {
                convertConfig.OutputPath = value;
                OnPropertyChanged("OutputPath");
            }
        }

        public string InputLabel
        {
            get { return (convertConfig.IsFolderConvert) ? "Input Folder" : "Input File"; }
        }

        public string SelectInputButtonLabel
        {
            get { return (convertConfig.IsFolderConvert) ? "Select Folder..." : "Select File..."; }
        }

        public string SelectOutputButtonLabel
        {
            get { return (convertConfig.IsFolderConvert || convertConfig.IsMultiSelect) ? "Select Folder..." : "Select File..."; }
        }

        public string OutputLabel
        {
            get { return (convertConfig.IsFolderConvert || convertConfig.IsMultiSelect) ? "Output Folder" : "Output File"; }
        }

        public bool CanSelectConcat
        {
            get { 
                
                if(convertConfig.IsFolderConvert || convertConfig.IsMultiSelect)
                {
                    return true;
                }
                else
                {
                    ConcatData = false;
                    return false;
                }
            }
        }

        public bool WriteToInputPath
        {
            get { return _writeToInputPath; }
            set
            {
                _writeToInputPath = value;
                CanEditOutput = !value;
                FillOutputPath();
                OnPropertyChanged("WriteToInputPath");
            }
        }

        public bool IncludeTimeReference
        {
            get { return convertConfig.IncludeTimeReference; }
            set
            {
                convertConfig.IncludeTimeReference = value;
                OnPropertyChanged("IncludeTimeReference");
            }
        }


        public bool CanEditOutput
        {
            get { return _canEditOutput; }
            set
            {
                _canEditOutput = value;
                OnPropertyChanged("CanEditOutput");
            }
        }

        public int FileConversionProgress
        {
            get { return _fileConversionProgress; }
            set
            {
                _fileConversionProgress = value;
                OnPropertyChanged("FileConversionProgress");
            }
        }

        public int FileWriteProgress
        {
            get { return _fileWriteProgress; }
            set
            {
                _fileWriteProgress = value;
                OnPropertyChanged("FileWriteProgress");
            }
        }

        public bool AveragingData
        {
            get { return convertConfig.IsAveraged; }
            set
            {
                convertConfig.IsAveraged = value;
                OnPropertyChanged("AveragingData");
                OnPropertyChanged("AveragingVisibility");
            }
        }

        public bool ConcatData
        {
            get { return convertConfig.IsConcat; }
            set
            {
                convertConfig.IsConcat = value;
                FillOutputPath();
                OnPropertyChanged("ConcatData");
            }
        }

        public int AveragingDatapoints
        {
            get { return convertConfig.AveragedDataPoints; }
            set
            {
                convertConfig.AveragedDataPoints = value;
                OnPropertyChanged("AveragingDatapoints");
            }
        }

        public Visibility AveragingVisibility
        {
            get { return convertConfig.IsAveraged ? Visibility.Visible : Visibility.Hidden; }
        }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged("StatusText");
            }
        }

        #endregion Bound Properties

        #region Bound Commands
        public ICommand SelectInput_Command => new Command(_ =>
        {
            //File(s)
            if(!convertConfig.IsFolderConvert)
            {
                string[] paths = null;

                if ((paths = LaunchImportFilePicker()) == null)
                {
                    return;
                }

                if (paths.Length > 1)
                {
                    convertConfig.IsMultiSelect = true;
                    OnPropertyChanged("OutputLabel");
                    OnPropertyChanged("SelectOutputButtonLabel");
                    OnPropertyChanged("CanSelectConcat");

                    convertConfig.RTXFiles = paths;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("\"");
                    sb.Append(paths[0]);

                    for (int i = 1; i < paths.Length; i++)
                    {
                        sb.Append("\" \"");
                        sb.Append(paths[i]);
                    }
                    sb.Append("\"");

                    DisplayInputPath = sb.ToString();
                }
                else
                {
                    convertConfig.IsMultiSelect = false;
                    OnPropertyChanged("OutputLabel");
                    OnPropertyChanged("SelectOutputButtonLabel");
                    OnPropertyChanged("CanSelectConcat");

                    convertConfig.RTXFiles = new string[] { paths[0] };
                    DisplayInputPath = paths[0];
                }
            }
            //folder
            else
            {
                string path = null;
                if ((path = LaunchFolderPicker()) == null)
                {
                    return;
                }

                convertConfig.RTXFiles = Directory.GetFiles(path, "*.rtx");
                DisplayInputPath = path;
            }

            FillOutputPath();
        });

        public ICommand SelectOutput_Command => new Command(_ =>
        {
            string path;

            if (convertConfig.IsConcat || (!convertConfig.IsFolderConvert && !convertConfig.IsMultiSelect))
            {
                path = LaunchExportFilePicker();
            }
            else
            {
                path = LaunchFolderPicker();
            }

            if (path != null)
            {
                OutputPath = path;
            }
        });

        public ICommand StartConversion_Command => new Command(_ =>
        {
            StatusText = null;
            FileConversionProgress = 0;
            StartConvert(convertConfig);
        });

        public ICommand SelectFiles_Command => new Command(_ =>
        {
            convertConfig.IsFolderConvert = false;
            convertConfig.IsMultiSelect = false;
            DisplayInputPath = null;
            convertConfig.RTXFiles = null;
            OutputPath = null;
            OnPropertyChanged("InputLabel");
            OnPropertyChanged("OutputLabel");
            OnPropertyChanged("SelectInputButtonLabel");
            OnPropertyChanged("SelectOutputButtonLabel");
            OnPropertyChanged("CanSelectConcat");
        });

        public ICommand SelectFolder_Command => new Command(_ =>
        {
            convertConfig.IsFolderConvert = true;
            convertConfig.IsMultiSelect = false;
            DisplayInputPath = null;
            convertConfig.RTXFiles = null;
            OutputPath = null;
            OnPropertyChanged("InputLabel");
            OnPropertyChanged("OutputLabel");
            OnPropertyChanged("SelectInputButtonLabel");
            OnPropertyChanged("SelectOutputButtonLabel");
            OnPropertyChanged("CanSelectConcat");
        });

        public ICommand SelectMradUnits_Command => new Command(_ =>
        {
            convertConfig.ExportUnit = ConverterSettings.exportUnits.mrad;
        });

        public ICommand SelectUradUnits_Command => new Command(_ =>
        {
            convertConfig.ExportUnit = ConverterSettings.exportUnits.urad;
        });

        #endregion Bound Commands

        #region Command and Helper Methods

        private string LaunchFolderPicker()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Multiselect = false;
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }

            return null;
        }

        private string[] LaunchImportFilePicker()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filters.Add(new CommonFileDialogFilter("RTX File","*.rtx"));
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                return Enumerable.ToArray(dialog.FileNames);
            }

            return null;
        }

        private string LaunchExportFilePicker()
        {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("CSV File", "*.csv"));
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }

            return null;
        }

        private void FillOutputPath()
        {
            //from write to output
            if (_writeToInputPath && convertConfig.RTXFiles != null)
            {
                if (convertConfig.IsFolderConvert || convertConfig.IsMultiSelect)
                {
                    OutputPath = Path.GetDirectoryName(convertConfig.RTXFiles[0]);
                }
                else
                {
                    OutputPath = Path.ChangeExtension(convertConfig.RTXFiles[0], "csv");
                }
            }

            if (_writeToInputPath && convertConfig.RTXFiles != null && ConcatData)
            {
                OutputPath = Path.GetDirectoryName(convertConfig.RTXFiles[0]) + "\\Concatenated_Output.csv";
            }
        }

        async private void StartConvert(ConverterSettings config)
        {

            //check if null
            if (DisplayInputPath == null || convertConfig.RTXFiles == null)
            {
                err.ShowError("Input path not specified!");
                return;
            }

            if (OutputPath == null)
            {
                err.ShowError("Output path not specified!");
                return;
            }

            //confirm files exist
            if (config.IsFolderConvert && convertConfig.RTXFiles.Length == 0)
            {
                err.ShowError("No .rtx files found in folder!");
                return;
            }

            //Process
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                FileConversionProgress = 0;
                FileWriteProgress = 0;
                int fileCount = 1;

                StatusText = string.Format("Processing file {0} of {1}", fileCount, convertConfig.RTXFiles.Length.ToString());

                //top bar
                Progress<int> processProgress = new Progress<int>(value =>
                {
                    FileConversionProgress = value;
                });

                //bottom bar
                Progress<int> writeProgress = new Progress<int>(value =>
                {
                    FileWriteProgress = value;

                    //increment file processing count
                    if(value == 100)
                    {
                        StatusText = string.Format("Processing file {0} of {1}", ++fileCount, convertConfig.RTXFiles.Length.ToString());
                    }
                });

                List<RTXData> rtxDatas = await Task.Run(() => ProcessFiles(convertConfig, processProgress));

                await Task.Run(() => WriteFiles(rtxDatas, convertConfig, writeProgress));

                sw.Stop();

                if (convertConfig.RTXFiles.Length == 1)
                {
                    StatusText = string.Format("Done! {0} file converted in {1:0.000}s.", rtxDatas.Count, (float)(sw.ElapsedMilliseconds) / 1000);
                }
                else
                {
                    StatusText = string.Format("Done! {0} files converted in {1:0.000}s.", rtxDatas.Count, (float)(sw.ElapsedMilliseconds) / 1000);
                }
            }
            catch(Exception ex)
            {
                err.ShowError(ex.Message);
                StatusText = "Conversion Failed!";
            }
        }

        private List<RTXData> ProcessFiles(ConverterSettings convertConfig, IProgress<int> progress)
        {
            List<RTXData> rtxDatas = new List<RTXData>();

            for (int i = 0; i < convertConfig.RTXFiles.Length; i++)
            {
                rtxDatas.Add(RTXFileProcessor.ProcessFile(convertConfig.RTXFiles[i]));
                rtxDatas[i].InputFilePath = convertConfig.RTXFiles[i];

                progress.Report((int)(((float)(i + 1) / (float)convertConfig.RTXFiles.Length) * 100));
            }

            progress.Report(100);

            return rtxDatas;
        }

        private void WriteFiles(List<RTXData> rtxDatas, ConverterSettings convertConfig, IProgress<int> progress)
        {
            string outputFilePath;

            if (!convertConfig.IsConcat)
            {
                foreach (RTXData rtxData in rtxDatas)
                {
                    //Output paths
                    if (convertConfig.IsFolderConvert || convertConfig.IsMultiSelect)
                    {
                        outputFilePath = convertConfig.OutputPath + "\\" + Path.GetFileNameWithoutExtension(rtxData.InputFilePath) + ".csv";
                    }
                    else
                    {
                        outputFilePath = convertConfig.OutputPath;
                    }

                    //Check/get averages
                    List<double?> values = rtxData.Data;

                    if (convertConfig.IsAveraged && rtxData.Data.Count < convertConfig.AveragedDataPoints)
                    {
                        throw new Exception("Total dataset is smaller than the number of datapoints to average entered - " + rtxData.InputFilePath);
                    }
                    else if(convertConfig.IsAveraged)
                    {
                        values = rtxData.GetAveragedData(convertConfig.AveragedDataPoints);
                    }

                    //Write
                    if (convertConfig.IncludeTimeReference)
                    {
                        if (CSVOutput.WriteCSV(values, outputFilePath, convertConfig.ExportUnit, progress, rtxData.GetDataInterval()) == -1)
                        {
                            throw new Exception("Error writing to " + outputFilePath);
                        }
                    }
                    else
                    {
                        if (CSVOutput.WriteCSV(values, outputFilePath, convertConfig.ExportUnit, progress) == -1)
                        {
                            throw new Exception("Error writing to " + outputFilePath);
                        }
                    }
                }
            }
            //Concat
            else
            {
                double concatDataInterval = 0;
                List<List<double?>> concatDatasets = new List<List<double?>>();

                //get/check data intervals across dataset
                foreach (var rtxData in rtxDatas)
                {
                    if (concatDataInterval != 0 && concatDataInterval != rtxData.GetDataInterval())
                    {
                        throw new Exception("Selected files have differing sample rates!");
                    }
                    else
                    {
                        concatDataInterval = rtxData.GetDataInterval();
                    }
                }

                //build concat data
                foreach (var rtxData in rtxDatas)
                {
                    if (convertConfig.IsAveraged && rtxData.Data.Count < convertConfig.AveragedDataPoints)
                    {
                        throw new Exception("Total dataset is smaller than the number of datapoints to average entered - " + rtxData.InputFilePath);
                    }
                    else if (convertConfig.IsAveraged)
                    {
                        concatDatasets.Add(rtxData.GetAveragedData(convertConfig.AveragedDataPoints));
                    }
                    else
                    {
                        concatDatasets.Add(rtxData.Data);
                    }
                }

                outputFilePath = convertConfig.OutputPath;

                if (convertConfig.IncludeTimeReference)
                {
                    if (CSVOutput.WriteConcatCSV(concatDatasets, outputFilePath, convertConfig.ExportUnit, concatDataInterval, progress) == -1)
                    {
                        throw new Exception("Error writing to " + outputFilePath);
                    }
                }
                else
                {
                    if (CSVOutput.WriteConcatCSV(concatDatasets, outputFilePath, convertConfig.ExportUnit, progress) == -1)
                    {
                        throw new Exception("Error writing to " + outputFilePath);
                    }
                }
            }

            progress.Report(100);
        }

        #endregion Command and Helper Methods
    }
}