using System;
using System.Windows;


namespace RTX2CSV_Converter
{
    interface IErrorNotification
    {
        void ShowError(string errorText, string errorTitle);
        void ShowError(string errorText);
    }

    public class ErrorNotification : IErrorNotification
    {
        public void ShowError(string errorText, string errorTitle)
        {
            MessageBox.Show(errorText, errorTitle, MessageBoxButton.OK);
        }

        public void ShowError(string errorText)
        {
            MessageBox.Show(errorText, "Error", MessageBoxButton.OK);
        }
    }
}
