using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace vEBuLa.Extensions
{
  public class ScrollIntoView : Behavior<ListView> {
    protected override void OnAttached() {
      base.OnAttached();
      AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }

    protected override void OnDetaching() {
      AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
      base.OnDetaching();
    }

    private static void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var listView = sender as ListView;

      if (listView?.SelectedItem == null)
        return;

      Action action = () =>
      {
        listView.UpdateLayout();

        if (listView.SelectedItem != null)
          listView.ScrollIntoView(listView.SelectedItem);
      };

      listView.Dispatcher.BeginInvoke(action, DispatcherPriority.ContextIdle);
    }
  }
}
