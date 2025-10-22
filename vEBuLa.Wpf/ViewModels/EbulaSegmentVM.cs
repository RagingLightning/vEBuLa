using Microsoft.Extensions.Logging;
using System;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;

internal static class EbulaSegmentExtension { public static EbulaSegmentVM ToVM(this EbulaSegment segment) => new(segment); }

public class EbulaSegmentVM : BaseVM {
  public EbulaSegment Model { get; }
  public override string ToString() => $"[{Model}]";
  public EbulaSegmentVM(EbulaSegment segment) { Model = segment; }

  public override bool Equals(object? obj) => obj is EbulaSegmentVM vm && vm.Model.Equals(Model);
  public override int GetHashCode() => Model.GetHashCode();
  public static bool operator ==(EbulaSegmentVM a, EbulaSegmentVM b) => a.Equals(b);
  public static bool operator !=(EbulaSegmentVM a, EbulaSegmentVM b) => !a.Equals(b);

  public Guid Id => Model.Id;
  public string Name {
    get => Model.Name;
    set { Model.Name = value; OnPropertyChanged(nameof(Name)); }
  }
  public (Guid Key, EbulaStationVM? Station) Origin {
    get => (Model.Origin.Key, Model.Origin.Station?.ToVM());
    set { Model.Origin = (value.Key, value.Station?.Model); OnPropertyChanged(nameof(Origin)); }
  }
  public (Guid Key, EbulaStationVM? Station) Destination {
    get => (Model.Destination.Key, Model.Destination.Station?.ToVM());
    set { Model.Destination = (value.Key, value.Station?.Model); OnPropertyChanged(nameof(Destination)); }
  }
  public TimeSpan Duration {
    get => Model.Duration;
    set { Model.Duration = value; OnPropertyChanged(nameof(Duration)); }
  }
}
