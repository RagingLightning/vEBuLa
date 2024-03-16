using System;
using System.ComponentModel;

namespace vEBuLa.ViewModels;

internal class EbulaRouteEntryVM : BaseVM {
  private EbulaVM Ebula { get; }
  private EbulaSegmentVM Segment { get; }
  public EbulaRouteEntryVM(EbulaVM ebula, EbulaSegmentVM segment, EbulaRouteEntryVM? prev) {
    Ebula = ebula;
    Segment = segment;

    Ebula.PropertyChanged += Ebula_PropertyChanged;

    if (prev is null) {
      Name = segment.Origin.Station?.Name ?? segment.Origin.Key.ToString();
    } else {
      Name = segment.Destination.Station?.Name ?? segment.Destination.Key.ToString();
      Departure = prev.Departure + prev.Segment.Duration;
    }
  }

  public override void Destroy() {
    base.Destroy();
    Ebula.PropertyChanged -= Ebula_PropertyChanged;
  }

  private void Ebula_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(Ebula.ServiceStartTime)) OnPropertyChanged(nameof(DepartureText));
  }

  public string Name { get; }

  public TimeSpan Departure { get; }
  public string DepartureText => Departure.Add(Ebula.ServiceStartTime).ToString(@"hh\:mm\:ss");
}