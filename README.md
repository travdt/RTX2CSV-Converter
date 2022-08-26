# RTX2CSV-Converter
Renishaw RTX to CSV file converter

A small utility to convert .rtx files produced from Renishaw XL-80 dynamic captures to CSV format for further processing.
There is the ability to batch convert .rtx files by selecting multiple files or a folder.
There is an option to include a time reference column. This is calculated from the sample rate in the rtx header, incrementing from 0.
By default the rtx data is in mm or mrad units. There is an option to output as um or urad units.
Additionally there is an option to output a moving average of the imported data. The desired number of datapoints to average over can be entered.

When multiple files are passed for conversion there is the option of concatenating to a single CSV.
In this case each set of rtx data is output as a single column in the CSV.
