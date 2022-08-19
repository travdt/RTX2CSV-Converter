using System.Windows;


namespace RTX2CSV_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            MainViewModel mvm = new MainViewModel();
            DataContext = mvm;
            InitializeComponent();
        }
    }
}
