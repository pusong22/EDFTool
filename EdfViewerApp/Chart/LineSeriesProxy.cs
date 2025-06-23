using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace EdfViewerApp.Chart;

[ObservableObject]
public partial class LineSeriesProxy : LineSeries<double>
{
    [ObservableProperty]
    private ObservableCollection<double> _data = [];

    partial void OnDataChanged(ObservableCollection<double> value)
    {
        Values = value;
    }
}
