using System;

namespace RTX2CSV_Converter
{
    internal class ConverterSettings
    {
        public enum exportUnits { mrad, urad };
        public exportUnits ExportUnit { get; set; }
        public string OutputPath { get; set; }
        public bool IsFolderConvert { get; set; }
        public bool IsMultiSelect { get; set; }
        public bool IncludeTimeReference { get; set; }
        public string[] RTXFiles { get; set; }
        public int AveragedDataPoints { get; set; }
        public bool IsAveraged { get; set; }
        public bool IsConcat { get; set; }

        public ConverterSettings()
        {
            AveragedDataPoints = 1;
        }

    }
}
