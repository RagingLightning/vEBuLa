using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using vEBuLa.Commands;
using vEBuLa.Extensions;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;

public partial class EbulaServiceVM : BaseVM {
  public IEbulaService Model { get; }
  public SetupScreenVM? Screen { get; }

  public EbulaServiceVM(IEbulaService ebulaService, BaseC? editCommand = null, SetupScreenVM? screen = null) {
    Model = ebulaService;
    EditCommand = editCommand;
    Screen = screen;
  }

  private void GenerateStopInfo() {
    _stopInfo += $"Start Time: {FormattedStartTime}\n";
    _stopInfo += $"Service Duration: {FormattedDuration}\n";

    List<EbulaEntry> allEntries = [];
    allEntries.AddRange(Model.Segments[0].PreEntries);
    foreach (var segment in Model.Segments)
      allEntries.AddRange(segment.Entries);
    allEntries.AddRange(Model.Segments[^1].PostEntries);

    foreach (var stop in Model.Stops) {
      if (stop.Arrival is null && stop.Departure is null) return;
      var entry = allEntries.FirstOrDefault(e => e.Location == stop.EntryLocation && e.LocationName == stop.EntryName);
      if (entry is null)
        continue;
      _stopInfo += $"\n- {entry.LocationName.Replace('\n',' ').Crop(16)}";

      _stopInfo += $" {(stop.Arrival is null ? "        " : stop.Arrival?.ToString("HH':'mm':'ss"))}";
      _stopInfo += $"|{(stop.Departure is null ? "        " : stop.Departure?.ToString("HH':'mm':'ss"))}";
    }
  }

  private void CalculateDuration() {
    DateTime start = DateTime.MaxValue;
    DateTime end = DateTime.MinValue;
    Model.Stops.ForEach(stop => {
      start = stop.Departure is DateTime d && d < start ? d : start;
      end = stop.Arrival is DateTime a && a > end ? a : end;
    });
    if (start == DateTime.MaxValue || end == DateTime.MinValue) return;
    Duration = end - start;
  }

  #region Properties
  public BaseC? EditCommand { get; init; }

  public string ConfigName {
    get => Model is EbulaService s ? s.Config.Name : "CUSTOM";
  }

  public string Name {
    get => Model.Name;
    set {
      Model.Name = value;
      OnPropertyChanged(nameof(Name));
      OnPropertyChanged(nameof(Info));
    }
  }

  public string Description {
    get => Model.Description;
    set {
      Model.Description = value;
      OnPropertyChanged(nameof(Description));
      OnPropertyChanged(nameof(Info));
    }
  }

  public TimeSpan StartTime {
    get => Model.StartTime;
    set {
      Model.StartTime = value;
      OnPropertyChanged(nameof(StartTime));
      OnPropertyChanged(nameof(FormattedStartTime));
      OnPropertyChanged(nameof(Info));
    }
  }

  private TimeSpan _duration = TimeSpan.Zero;
  public TimeSpan Duration {
    get {
      if (_duration == TimeSpan.Zero) CalculateDuration();
      return _duration;
    }
    set {
      _duration = value;
      OnPropertyChanged(nameof(Duration));
      OnPropertyChanged(nameof(FormattedDuration));
    }
  }
  public string FormattedStartTime {
    get => StartTime.ToString("hh':'mm':'ss");
    set {
      if (ShortTime().IsMatch(value) && TimeSpan.TryParse($"{value[..2]}:{value[2..4]}:{value[4..]}", out var t)
        || Time().IsMatch(value) && TimeSpan.TryParse(value, out t)) {
        StartTime = t;
      }
    }
  }

  public string FormattedDuration => Duration.ToString("hh':'mm':'ss");

  public string Info => $"{Model.Name} {DateTime.Today:dd/MM/yy} {FormattedStartTime}, {Model.Description}, {Model.Vehicles.Aggregate((a, b) => $"{a};{b}")}";

  private string _stopInfo = string.Empty;
  public string StopInfo {
    get {
      if (_stopInfo == string.Empty) GenerateStopInfo();
      return _stopInfo;
    }
    set {
      _stopInfo = value;
      OnPropertyChanged(nameof(StopInfo));
    }
  }

  private bool _selected = false;
  public bool Selected {
    get {
      return _selected;
    }
    set {
      _selected = value;
      OnPropertyChanged(nameof(Selected));
    }
  }
  public string Vehicles {
    get {
      return Model.Vehicles.Aggregate((a, b) => $"{a};{b}");
    }
    set {
      Model.Vehicles.Clear();
      Model.Vehicles.AddRange(value.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
      OnPropertyChanged(nameof(Selected));
    }
  }

  #endregion

  [GeneratedRegex("\\d{2}:\\d{2}:\\d{2}")]
  private static partial Regex Time();

  [GeneratedRegex("\\d{6}")]
  private static partial Regex ShortTime();
}