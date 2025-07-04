using CommunityToolkit.Mvvm.ComponentModel;
using Core.Primitive;
using EdfViewerApp.Chart;
using System.Collections.ObjectModel;

namespace EdfViewerApp;

[ObservableObject]
public partial class HeatSeriesProxy : HeatSeries
{
    [ObservableProperty]
    private ObservableCollection<Coordinate> _data = [];

    partial void OnDataChanged(ObservableCollection<Coordinate> value)
    {
        Values = value;
    }
}
