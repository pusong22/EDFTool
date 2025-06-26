using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace EdfViewerApp.Behavior;
public class SliderThumbDragBehavior : Behavior<Slider>
{
    private Thumb? _thumb;

    public ICommand DragCompletedCommand
    {
        get { return (ICommand)GetValue(DragCompletedCommandProperty); }
        set { SetValue(DragCompletedCommandProperty, value); }
    }


    public static readonly DependencyProperty DragCompletedCommandProperty =
       DependencyProperty.Register(
           "DragCompletedCommand", 
           typeof(ICommand), 
           typeof(SliderThumbDragBehavior), 
           new PropertyMetadata(null));

   
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.ApplyTemplate(); // 确保模板已应用，否则 Thumb 可能不存在
        _thumb = FindVisualChild<Thumb>(AssociatedObject);

        if (_thumb != null)
        {
            _thumb.DragCompleted += OnThumbDragCompleted;
        }
        else
        {
            // 如果 Slider 的模板尚未完全加载或 Thumb 不存在，
            // 可以等待 Loaded 事件或进行其他处理
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (_thumb != null)
        {
            _thumb.DragCompleted -= OnThumbDragCompleted;
        }
        AssociatedObject.Loaded -= AssociatedObject_Loaded;
        _thumb = null;
    }

    private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        // 如果 OnAttached 时 Thumb 没找到，尝试在 Loaded 事件后再次寻找
        AssociatedObject.Loaded -= AssociatedObject_Loaded; // 移除事件，避免重复
        _thumb = FindVisualChild<Thumb>(AssociatedObject);
        if (_thumb != null)
        {
            _thumb.DragCompleted += OnThumbDragCompleted;
        }
    }

    private void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
    {
        // 执行绑定的命令，传递 Slider 的当前值
        if (DragCompletedCommand != null && DragCompletedCommand.CanExecute(AssociatedObject.Value))
        {
            DragCompletedCommand.Execute(AssociatedObject.Value);
        }
    }

    private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
            {
                return typedChild;
            }
            else
            {
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
        }
        return null;
    }
}
