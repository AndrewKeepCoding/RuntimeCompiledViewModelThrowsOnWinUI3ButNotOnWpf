using System.Windows;
using ViewModelCreatorLib;

namespace WpfApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // On WPF, the 3 of them works without throwing EXCEPTIONS!
        int testNumber = 3;
        this.DataContext = testNumber switch
        {
            1 => ViewModelCreator.CreateWithNewKeyword(),

            2 => ViewModelCreator.CreateWithReflection(),

            3 => ViewModelCreator.CreateAtRuntime(),

            _ => throw new System.NotImplementedException()
        };
    }
}