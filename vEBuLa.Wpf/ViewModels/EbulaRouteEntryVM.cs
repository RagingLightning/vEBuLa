using System;

namespace vEBuLa.ViewModels;

internal class EbulaRouteEntryVM : BaseVM {
  private StorageConfigScreenVM Screen { get; }
  public EbulaRouteEntryVM(StorageConfigScreenVM screen, EbulaSegmentVM segment, EbulaRouteEntryVM? prev) {
    Screen = screen;

    Screen.PropertyChanged += Screen_PropertyChanged;

    if (prev is null) {
      Name = segment.Origin.Station?.Name ?? segment.Origin.Key.ToString();
    } else {
      Name = segment.Destination.Station?.Name ?? segment.Destination.Key.ToString();
    }
  }

  private void Screen_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(StorageConfigScreenVM.Departure)) OnPropertyChanged(DepartureText);
  }

  public string Name { get; }

  public TimeSpan Departure { get; }
  public string DepartureText => Departure.Add(Screen.Departure).ToString("hh':'mm':'ss");
}