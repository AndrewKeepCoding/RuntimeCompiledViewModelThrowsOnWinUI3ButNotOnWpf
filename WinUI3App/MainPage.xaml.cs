using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using ViewModelCreatorLib;

namespace WinUI3App;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        // On WinUI 3, TEST 3 works BUT throws "ArgumentNullException" !
        int testNumber = 3;
        this.DataContext = testNumber switch
        {
            // TEST 1 works!
            1 => ViewModelCreator.CreateWithNewKeyword(),

            // TEST 2 works!
            2 => ViewModelCreator.CreateWithReflection(),

            // TEST 3 also works,
            // BUT
            // according to the Debug logs, "ArgumentNullException" is thrown INTERNALLY.
            3 => ViewModelCreator.CreateAtRuntime(),

            _ => throw new NotImplementedException()
        };
    }
}
