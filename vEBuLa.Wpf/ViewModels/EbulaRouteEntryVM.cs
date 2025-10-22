using System;
using System.ComponentModel;

namespace vEBuLa.ViewModels;

public class EbulaRouteEntryVM : BaseVM {
  private SetupScreenVM Screen { get; }
  private EbulaSegmentVM Segment { get; }
  public EbulaRouteEntryVM(SetupScreenVM screen, EbulaSegmentVM segment, EbulaRouteEntryVM? prev) {
    Screen = screen;
    Segment = segment;

    Screen.PropertyChanged += Screen_PropertyChanged;

    if (prev is null) {
      Name = segment.Origin.Station?.Name ?? segment.Origin.Key.ToString();
    } else {
      Name = segment.Destination.Station?.Name ?? segment.Destination.Key.ToString();
      Departure = prev.Departure + prev.Segment.Duration;
    }
  }

  public override void Destroy() {
    base.Destroy();
    Screen.PropertyChanged -= Screen_PropertyChanged;
  }

  private void Screen_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(SetupScreenVM.ServiceStart)) OnPropertyChanged(nameof(DepartureText));
  }

  public string Name { get; }

  public TimeSpan Departure { get; }
  public string DepartureText => Departure.Add(Screen.ServiceStart).ToString(@"hh\:mm\:ss");
}