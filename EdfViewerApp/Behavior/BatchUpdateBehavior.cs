using EdfViewerApp.Chart;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace EdfViewerApp.Behavior;
public class BatchUpdateBehavior : Behavior<UserControl>
{
    public static readonly DependencyProperty IsBatchUpdatingProperty =
          DependencyProperty.Register(
              nameof(IsBatchUpdating),
              typeof(bool),
              typeof(BatchUpdateBehavior),
              new PropertyMetadata(false, OnIsBatchUpdatingChanged));

    public bool IsBatchUpdating
    {
        get => (bool)GetValue(IsBatchUpdatingProperty);
        set => SetValue(IsBatchUpdatingProperty, value);
    }

    private static void OnIsBatchUpdatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (BatchUpdateBehavior)d;
        var view = behavior.AssociatedObject as CartesianChartView; // 换成你的绘制控件类型
        if (view == null) return;

        if ((bool)e.NewValue)
            view.BeginUpdate();
        else
            view.EndUpdate();
    }
}
