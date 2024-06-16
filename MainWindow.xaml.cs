using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace rat_programmer;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private AppWindow _apw;
    private OverlappedPresenter _presenter;

    public ViewModel ViewModel { get; private set; }

    public void GetAppWindowAndPresenter()
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        _apw = AppWindow.GetFromWindowId(myWndId);
        _presenter = _apw.Presenter as OverlappedPresenter;
        ViewModel = new ViewModel();
    }
    public MainWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(650, 700));
        GetAppWindowAndPresenter();
        _presenter.IsResizable = false;        
        Trace.Listeners.Add(new TextBoxTraceListener(LogTextBox));
        Trace.WriteLine("Отладочная информация");
    }

    private void Connect_Click(object sender, RoutedEventArgs e)
    {
        ToggleButton tb = sender as ToggleButton;
        ViewModel.ConnectCommand.Execute(null);
        tb.IsChecked = ViewModel.IsConnect;
        tb.Content = ViewModel.IsConnect switch
        {
            true => "Отключить",
            false => "Подлключить"
        };

    }

    private void LogTextBox_TextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs e)
    {
        var tb = sender as TextBox;
        var grid = (Grid)VisualTreeHelper.GetChild(tb, 0);
        for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
        {
            object obj = VisualTreeHelper.GetChild(grid, i);
            if (obj is not ScrollViewer)
                continue;
            ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
            break;
        }
    }

    private void NumberBoxEx_ProgTime_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            if (ViewModel.SetTimeAsyncCommand.CanExecute(null))
                ViewModel.SetTimeAsyncCommand.Execute(null);
    }

    private void NumberBoxEx_StepTime_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            if (ViewModel.SetStepAsyncCommand.CanExecute(null))
                ViewModel.SetStepAsyncCommand.Execute(null);
    }

    private void NumberBoxEx_WidthPulse_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            if (ViewModel.SetWidthPulseAsyncCommand.CanExecute(null))
                ViewModel.SetWidthPulseAsyncCommand.Execute(null);
    }
}
