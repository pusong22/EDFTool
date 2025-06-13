using CommunityToolkit.Mvvm.Messaging;
using EDFToolApp.Message;
using System.Windows;

namespace EDFToolApp;

public partial class StartupWindow : Window
{
    public StartupWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<RequestCloseWindowMessage>(
            this, (r, m) =>
            {
                if (m.Close)
                {
                    DialogResult = true;
                    Close();
                }
            });
    }
}
