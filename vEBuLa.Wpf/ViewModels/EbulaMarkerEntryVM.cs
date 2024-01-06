using Microsoft.Extensions.Logging;
using vEBuLa.Commands;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal class EbulaMarkerEntryVM : BaseVM {
  private ILogger<EbulaEntryVM>? Logger => App.GetService<ILogger<EbulaEntryVM>>();
  public EbulaSegmentVM Segment { get; }
  public EbulaScreenVM Screen { get; }
  public EbulaMarkerType MarkerType { get; }
  public override bool Equals(object? obj) => obj is EbulaMarkerEntryVM marker && marker.Segment == Segment && marker.MarkerType == MarkerType;
  public override int GetHashCode() => Segment.GetHashCode();
  public static bool operator ==(EbulaMarkerEntryVM a, EbulaMarkerEntryVM b) => a.Equals(b);
  public static bool operator !=(EbulaMarkerEntryVM a, EbulaMarkerEntryVM b) => !a.Equals(b);

  public EbulaMarkerEntryVM(EbulaScreenVM screen, EbulaSegment segment, EbulaMarkerType type) {
    Screen = screen;
    Segment = segment.ToVM();
    MarkerType = type;

    EditSegmentDurationCommand = EditSegmentDurationC.INSTANCE;

    switch (MarkerType) {
      case EbulaMarkerType.PRE:
        IsMain = false;
        IsPrePost = true;
        MarkerTypeName = "PRE";
        EditSegmentLocationCommand = EditSegmentStationC.ORIGIN;
        LocationName = Segment.Origin.Station?.Name ?? "null";
        break;
      case EbulaMarkerType.MAIN:
        IsMain = true;
        IsPrePost = false;
        MarkerTypeName = "MAIN";
        EditSegmentOriginCommand = EditSegmentStationC.ORIGIN;
        EditSegmentDestinationCommand = EditSegmentStationC.DESTINATION;
        OriginName = Segment.Origin.Station?.Name ?? "null";
        DestinationName = Segment.Destination.Station?.Name ?? "null";
        break;
      case EbulaMarkerType.POST:
        IsMain = false;
        IsPrePost = true;
        MarkerTypeName = "POST";
        EditSegmentLocationCommand = EditSegmentStationC.DESTINATION;
        LocationName = Segment.Destination.Station?.Name ?? "null";
        break;
    }
  }

  #region Properties

  public BaseC EditSegmentDurationCommand { get; }
  public BaseC? EditSegmentOriginCommand { get; }
  public BaseC? EditSegmentDestinationCommand { get; }
  public BaseC? EditSegmentLocationCommand { get; }

  public string MarkerTypeName { get; }
  public bool IsMain { get; }
  public bool IsPrePost { get; }

  private int _entryCount;
  public int EntryCount {
    get {
      return _entryCount;
    }
    set {
      _entryCount = value;
      OnPropertyChanged(nameof(EntryCount));
    }
  }

  private string _originName = string.Empty;
  public string OriginName {
    get {
      return _originName;
    }
    set {
      _originName = value;
      OnPropertyChanged(nameof(OriginName));
    }
  }

  private string _destinationName = string.Empty;
  public string DestinationName {
    get {
      return _destinationName;
    }
    set {
      _destinationName = value;
      OnPropertyChanged(nameof(DestinationName));
    }
  }

  private string _locationName = "->";
  public string LocationName {
    get {
      return _locationName;
    }
    set {
      _locationName = value;
      OnPropertyChanged(nameof(LocationName));
    }
  }

  private string _duration;
  public string Duration {
    get {
      return _duration;
    }
    set {
      _duration = value;
      OnPropertyChanged(nameof(Duration));
    }
  }

  #endregion

}

internal enum EbulaMarkerType {
  PRE,
  MAIN,
  POST
}