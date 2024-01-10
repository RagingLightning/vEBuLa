using System;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;

public static class EbulaStationExtension { public static EbulaStationVM ToVM(this EbulaStation station) => new EbulaStationVM(station); }

public class EbulaStationVM : BaseVM {
  public override string ToString() => $"[{Model}]";
  public EbulaStation Model { get; }
  public EbulaStationVM(EbulaStation station) { Model = station; }
  public override bool Equals(object? obj) => obj is EbulaStationVM vm && vm.Model.Equals(Model);
  public override int GetHashCode() => Model.GetHashCode();
  public static bool operator ==(EbulaStationVM a, EbulaStationVM b) => a.Equals(b);
  public static bool operator !=(EbulaStationVM a, EbulaStationVM b) => !a.Equals(b);

  public Guid Id => Model.Id;
  public string Name {
    get => Model.Name;
    set { Model.Name = value; OnPropertyChanged(nameof(Name)); }
  }
}