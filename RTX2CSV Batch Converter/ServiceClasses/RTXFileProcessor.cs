using System;
using System.IO;
using System.Text;

namespace RTX2CSV_Converter
{
    internal static class RTXFileProcessor
    {
        public static RTXData ProcessFile(string file)
        {
            byte[] buffer = new byte[8192];
            int bytesRead;
            byte[] dataMarker = { 0x44, 0x61, 0x74, 0x61, 0x3A, 0x0D, 0x0A };
            int dataMarkerPosition = 0;
            int dataStart;
            int valuePosition = 0;
            byte[] valueBuffer = new byte[8];
            RTXData rtxData;

            try
            {
                using (FileStream rtxStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    //get first 8K of file 
                    bytesRead = rtxStream.Read(buffer, 0, buffer.Length);

                    //check buffer has obtained feasible amount of data and starts HEADER:: to validate contents
                    if (bytesRead > 272 && HeaderCheck(ref buffer) == -1)
                    {
                        throw new Exception(file + " is not a valid file!");
                    };

                    //read all header data
                    rtxData = ReadHeaderData(ref buffer);

                    //search for 'Data:/0D/0A' for start of actual data set
                    for (dataStart = 272; dataStart < bytesRead; dataStart++)
                    {
                        if (buffer[dataStart] == dataMarker[dataMarkerPosition])
                        {
                            dataMarkerPosition++;
                        }
                        else
                        {
                            dataMarkerPosition = 0;
                        }

                        //found
                        if (dataMarkerPosition == dataMarker.Length)
                        {
                            dataStart++;
                            break;
                        }
                    }

                    //data marker not found
                    if (dataMarkerPosition == 0)
                    {
                        throw new Exception(file + " is not a valid file!");
                    }

                    //convert data, valueBuffer persists between buffer fills
                    do
                    {
                        while (bytesRead > dataStart && rtxData.Data.Count < rtxData.SampleNumber)
                        {
                            valueBuffer[valuePosition++] = buffer[dataStart++];

                            //value buffer filled
                            if (valuePosition > valueBuffer.Length - 1)
                            {
                                //check for EOF sequence
                                if (valueBuffer[0] == 0x0D)
                                {
                                    if (EOFCheck(ref valueBuffer))
                                    {
                                        break;
                                    }
                                }

                                rtxData.Data.Add(BitConverter.ToDouble(valueBuffer, 0));
                                valuePosition = 0;
                            }
                        }

                        dataStart = 0;

                    } while ((bytesRead = rtxStream.Read(buffer, 0, buffer.Length)) > 0);
                }

                //pad to declared data length in header if file is incomplete
                if (rtxData.Data.Count < rtxData.SampleNumber)
                {
                    while (rtxData.Data.Count < rtxData.SampleNumber)
                    {
                        rtxData.Data.Add(0);
                    }

                }

                return rtxData;

            }
            catch
            {
                //throw;
                throw new Exception("Cannot access " + file);
            }
        }

        //Helper Methods
        private static int HeaderCheck(ref byte[] buffer)
        {
            byte[] headerBytes = { 0x48, 0x45, 0x41, 0x44, 0x45, 0x52, 0x3A, 0x3A, 0x0D, 0x0A };

            for (int j = 0; j < headerBytes.Length; j++)
            {
                if (headerBytes[j] != buffer[j])
                {
                    return -1;
                }
            }

            return 1;
        }

        private static RTXData ReadHeaderData(ref byte[] buffer)
        {
            RTXData headerData = new RTXData();
            int dataStartIndex = 10; //start processing after HEADER::
            int dataEndIndex;
            int dataLength;
            string str;

            for (int i = 0; i < 17; i++)
            {
                if((dataStartIndex = FindNextDataStartPosition(ref buffer, dataStartIndex)) == -1)
                {
                    break;
                }

                if ((dataEndIndex = FindNextDataEndPosition(ref buffer, dataStartIndex)) == -1)
                {
                    break;
                }

                if ((dataLength = dataEndIndex - dataStartIndex) < 1)
                {
                    //header field is null
                    continue;
                }

                str = Encoding.UTF8.GetString(buffer, dataStartIndex, dataEndIndex - dataStartIndex);

                switch (i)
                {
                    //Filename string
                    case 0:
                        headerData.Filename = str;
                        break;

                    //Owner string
                    case 1:
                        headerData.Owner = str;
                        break;

                    //VersionNumber string
                    case 2:
                        headerData.VersionNumber = str;
                        break;

                    //FileType string
                    case 3:
                        headerData.FileType = str;
                        break;

                    //Velocity double
                    case 4:
                        try
                        {
                            headerData.Velocity = Double.Parse(str);
                        }
                        catch
                        {

                        }
                        break;

                    //SampleRate double
                    case 5:
                        try
                        {
                            headerData.SampleRate = Double.Parse(str);
                        }
                        catch
                        {

                        }
                        break;

                    //SampleNumber double
                    case 6:
                        try
                        {
                            headerData.SampleNumber = Double.Parse(str);
                        }
                        catch
                        {

                        }
                        break;

                    //TriggerPoint double
                    case 7:
                        try
                        {
                            headerData.TriggerPoint = Double.Parse(str);
                        }
                        catch
                        {

                        }
                        break;

                    //TriggerInterval double
                    case 8:
                        try
                        {
                            headerData.TriggerInterval = Double.Parse(str);
                        }
                        catch
                        {

                        }
                        break;

                    //ActualSampleRate int
                    case 9:
                        try
                        {
                            headerData.ActualSampleRate = Double.Parse(str);
                        }
                        catch
                        {

                        }
                        break;

                    //Flags int[]
                    case 10:
                        for(int j = 0; j < 5; j++)
                        {
                            headerData.Flags[j] = buffer[dataStartIndex] - 0x30;
                            dataStartIndex += 2;
                        }
                        break;

                    //Machine string
                    case 11:
                        headerData.Machine = str;
                        break;

                    //SerialNumber string
                    case 12:
                        headerData.SerialNumber = str;
                        break;

                    //Date DateTime
                    case 13:
                        try
                        {
                            headerData.Date = DateTime.Parse(str);
                        }
                        catch
                        {

                        }
                        break;

                    //By string
                    case 14:
                        headerData.By = Encoding.UTF8.GetString(buffer, dataStartIndex, dataEndIndex - dataStartIndex);
                        break;

                    //Axis string
                    case 15:
                        headerData.Axis = Encoding.UTF8.GetString(buffer, dataStartIndex, dataEndIndex - dataStartIndex);
                        break;

                    //Location string
                    case 16:
                        headerData.Location = Encoding.UTF8.GetString(buffer, dataStartIndex, dataEndIndex - dataStartIndex);
                        break;
                }

                dataStartIndex = dataEndIndex;

            }

            return headerData;
        }


        private static int FindNextDataStartPosition(ref byte[] buffer, int startPosition)
        {
            while (startPosition < buffer.Length)
            {
                if (buffer[startPosition++] == 0x3A) //:
                {
                    //skip double :: on titles
                    if (buffer[startPosition] == 0x3A)
                    {
                        startPosition++;
                        continue;
                    }
                    
                    //trim 0x20 at start of some strings
                    else if(buffer[startPosition] == 0x20)
                    {
                        startPosition++;
                        return startPosition;
                    }

                    else
                    {
                        return startPosition;
                    }
                }
            }

            return -1;
        }

        private static int FindNextDataEndPosition(ref byte[] buffer, int startPosition)
        {
            while (startPosition < buffer.Length)
            {
                if (buffer[startPosition++] == 0x0D) //CR
                {
                    return --startPosition;
                }
            }

            return -1;
        }

        private static bool EOFCheck(ref byte[] buffer)
        {
            byte[] eofMarker = { 0x0D, 0x0A, 0x45, 0x4F, 0x46 };

            for (int i = 0; i < eofMarker.Length; i++)
            {
                if (buffer[i] != eofMarker[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
