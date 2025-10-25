using Microsoft.AspNetCore.Routing;
using System.CommandLine;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using vEBuLa.Models;

Option<FileInfo?> InputPath = new("--input") {
  Aliases = { "-i" },
  Description = "Path to read kml data from",
  DefaultValueFactory = (_) => new FileInfo("./ebula.kml")
};

Option<DirectoryInfo?> OutputPath = new("--output") {
  Aliases = { "-o" },
  Description = "Path to write ebula data to",
  DefaultValueFactory = (r) => r.GetValue(InputPath)?.Directory
};

RootCommand rootCommand = new("Ebula KML conversion tool") {
  Description = "A tool to convert kml folders into ebula presets"
};
rootCommand.Options.Add(InputPath);
rootCommand.Options.Add(OutputPath);

rootCommand.SetAction(res => {
  var input = res.GetValue(InputPath)!;
  var output = res.GetValue(OutputPath)!;
  CreateEbula(input, output);
});

rootCommand.Parse(args).Invoke();

internal partial class Program {

  public static void CreateEbula(FileInfo input, DirectoryInfo output) {
    if (!input.Exists) {
      Console.WriteLine($"Input file '{input.FullName}' does not exist, exiting");
      return;
    }

    var data = XDocument.Load(input.FullName);

    var kml = data.Elements().FirstOrDefault(e => e.Name.LocalName == "kml");
    var document = kml?.Elements().FirstOrDefault(e => e.Name.LocalName == "Document");

    Console.WriteLine($"+---------------------");
    Console.WriteLine($"| EbulaKmlConvert v0.1");
    Console.WriteLine($"| KML > Ebula");
    Console.WriteLine($"| Input: {input.Name}");
    Console.WriteLine($"| Output: {output.Name}");
    Console.WriteLine($"+---------------------");

    if(document is null) {
      Console.WriteLine("Invalid KML format, exiting");
      return;
    }

    Console.WriteLine("!! Existing files will be overwritten !!");
    Console.WriteLine("Starting conversion in 10 seconds...");

    foreach (var presetFolder in document.Elements().Where(e => e.Name.LocalName == "Folder")) {
      var presetName = presetFolder.Elements().FirstOrDefault(e => e.Name.LocalName == "name")?.Value;
      if (string.IsNullOrWhiteSpace(presetName)) {
        Console.WriteLine("Found preset folder without name, skipping");
        continue;
      }

      Console.WriteLine();
      Console.WriteLine($"Converting: {presetName}");

      var filePath = Path.Combine(output.FullName, $"{presetName}.ebula");
      EbulaConfig preset;
      if (File.Exists(filePath)) {
        Console.WriteLine($"Found matching preset file {presetName}.ebula, loading existing data");
        preset = new EbulaConfig(filePath);
      } else {
        Console.WriteLine($"No matching preset file was found, creating new preset");
        preset = new EbulaConfig();
      }

      // Move all preset data into local storage
      var segments = new Dictionary<Guid, EbulaSegment>(preset.Segments);
      var stations = new Dictionary<Guid, EbulaStation>(preset.Stations);
      preset.Segments.Clear();
      preset.Stations.Clear();

      foreach (var routeFolder in presetFolder.Elements().Where(e => e.Name.LocalName == "Folder")) {
        ProcessRoute(preset, segments, stations, routeFolder);
      }
    
      preset.Save(Path.Combine(output.FullName, $"{presetName}.ebula"));

      ValidateServices(preset);
    }
  }

  private static void ProcessRoute(EbulaConfig preset, Dictionary<Guid, EbulaSegment> segments, Dictionary<Guid, EbulaStation> stations, XElement routeFolder) {
    var routeName = routeFolder.Elements().FirstOrDefault(e => e.Name.LocalName == "name")?.Value;
    if (string.IsNullOrWhiteSpace(routeName)) {
      Console.WriteLine("#-Found route folder without name, skipping");
      return;
    }
    Console.WriteLine($" - Converting Route: {routeName}");

    foreach (var segmentFolder in routeFolder.Elements().Where(e => e.Name.LocalName == "Folder")) {
      ProcessSegment(preset, segments, stations, routeName, segmentFolder);
    }
  }

  private static void ProcessSegment(EbulaConfig preset, Dictionary<Guid, EbulaSegment> segments, Dictionary<Guid, EbulaStation> stations, string routeName, XElement segmentFolder) {
    var rawSegmentName = segmentFolder.Elements().FirstOrDefault(e => e.Name.LocalName == "name")?.Value;
    if (string.IsNullOrWhiteSpace(rawSegmentName)) {
      Console.WriteLine("#-#- Found route segment folder without name, skipping");
      return;
    }

    var segmentStartEnd = rawSegmentName.Split("_");
    if (segmentStartEnd.Length == 1) {
      Console.WriteLine($"#-#- Invalid segment name: {rawSegmentName}, skipping");
      return;
    }

    var segmentStart = segmentStartEnd[0];
    var segmentEnd = segmentStartEnd[1];
    Console.WriteLine($" - - Converting Route Segment: {segmentStart} - {segmentEnd}");

    // Get start and end stations from preset
    var segmentStartLocation = segmentStart.Split("|")[0];
    var segmentEndLocation = segmentEnd.Split("|")[0];
    var startStation = stations.Values.FirstOrDefault(s => s.Name == segmentStartLocation);
    var endStation = stations.Values.FirstOrDefault(s => s.Name == segmentEndLocation);

    // Create stations if they do not exist
    if (startStation is null) {
      Console.WriteLine($" - - - Adding new station: {segmentStartLocation}");
      var id = GenerateId(stations.Keys);
      startStation = new EbulaStation(id, segmentStartLocation);
      stations.Add(id, startStation);
    }
    if (endStation is null) {
      Console.WriteLine($" - - - Adding new station: {segmentEndLocation}");
      var id = GenerateId(stations.Keys);
      endStation = new EbulaStation(id, segmentEndLocation);
      stations.Add(id, endStation);
    }

    // Add stations back to preset
    preset.Stations[startStation.Id] = startStation;
    preset.Stations[endStation.Id] = endStation;

    // Get existing segment if it exists
    var segmentName = $"{routeName} - {rawSegmentName.Replace('_', '>')}";
    var segment = preset.Segments.Values.FirstOrDefault(s =>
      s.Origin.Station == startStation && s.Destination.Station == endStation && s.Name == rawSegmentName
    );

    // Create new segment if it does not exist
    if (segment is null) {
      Console.WriteLine($" - - - Adding new segment: {segmentName}");
      var id = GenerateId(segments.Keys);
      segment = new EbulaSegment(preset, id, segmentName) {
        Origin = (startStation.Id, startStation),
        Destination = (endStation.Id, endStation)
      };
      segments.Add(id, segment);
    }

    // Add segment back to preset
    preset.Segments.Add(segment.Id, segment);

    // Clear existing entries
    segment.Entries.Clear();

    foreach (var place in segmentFolder.Elements().Where(e => e.Name.LocalName == "Placemark")) {
      ProcessPlaceMark(segment, place);
    }
  }

  private static void ProcessPlaceMark(EbulaSegment segment, XElement place) {
    var placeName = place.Elements().FirstOrDefault(e => e.Name.LocalName == "name")?.Value;
    if (string.IsNullOrWhiteSpace(placeName)) {
      Console.WriteLine("#-#-#-#- Found place mark without name, skipping");
      return;
    }

    var placeNameSegments = placeName.Split(";");
    if (placeNameSegments.Length != 6) {
      Console.WriteLine($"#-#-#- Invalid place mark name: {placeName}, skipping");
      return;
    }

    bool posBreak = false, tunnelStart = false, tunnelEnd = false, textBox = false;

    var position = placeNameSegments[0];
    if (position.Contains('X')) {
      position = position.Replace("X", "");
      posBreak = true;
    }
    if (position.Contains("TS")) {
      position = position.Replace("TS", "");
      tunnelStart = true;
    }
    if (position.Contains("TE")) {
      position = position.Replace("TE", "");
      tunnelEnd = true;
    }
    if (!int.TryParse(position, out var location)) {
      Console.WriteLine($"#-#-#- Invalid place mark name: {placeName}, skipping");
      return;
    }
    if (!Enum.TryParse<Gradient>(placeNameSegments[1], out var gradient)) {
      Console.WriteLine($"#-#-#- Invalid place mark name: {placeName}, skipping");
      return;
    }
    if (!int.TryParse(placeNameSegments[2], out var speedLimit)) {
      Console.WriteLine($"#-#-#- Invalid place mark name: {placeName}, skipping");
      return;
    }
    if (!Enum.TryParse<EbulaSymbol>(placeNameSegments[3], out var icon)) {
      Console.WriteLine($"#-#-#- Invalid place mark name: {placeName}, skipping");
      return;
    }
    var mainText = placeNameSegments[4].Replace('|', '\n');
    if (mainText.Contains('[') || mainText.Contains(']')) {
      mainText = mainText.Replace("[", "").Replace("]", "");
      textBox = true;
    }
    var subText = placeNameSegments[5].Replace('|', '\n');

    var gpsCoordinates = place.Elements().FirstOrDefault(e => e.Name.LocalName == "Point")?
      .Elements().FirstOrDefault(e => e.Name.LocalName == "coordinates")?.Value;
    if (string.IsNullOrEmpty(gpsCoordinates)) {
      Console.WriteLine($"#-#-#- Corrupted place mark: {placeName}, skipping");
      return;
    }
    var gpsParts = gpsCoordinates.Split(",");
    var gpsLat = float.Parse(gpsParts[1], CultureInfo.InvariantCulture);
    var gpsLon = float.Parse(gpsParts[0], CultureInfo.InvariantCulture);

    // Create new segment entry
    var entry = new EbulaEntry() {
      Location = location,
      Gradient = gradient,
      KilometerBreak = posBreak,
      LabelBox = textBox,
      LocationName = mainText,
      LocationNotes = subText,
      SpeedLimit = speedLimit,
      SpeedSigned = true,
      Symbol = icon,
      TunnelStart = tunnelStart,
      TunnelEnd = tunnelEnd,
      GpsLocation = new(gpsLat, gpsLon)
    };

    segment.Entries.Add(entry);

    Console.WriteLine($" - - - Adding Entry: {entry}");
  }

  private static void ValidateServices(EbulaConfig preset) {
    Console.WriteLine();
    Console.WriteLine("Checking configuration consistency");

    foreach (var routeKv in preset.Routes) {
      var route = routeKv.Value;

      foreach (var segment in route.Segments) {
        if (!preset.Segments.ContainsKey(segment.Id)) {
          Console.WriteLine($"Warning: Route {routeKv.Value.Name} references missing segment {segment.Name}");
        }
      }
    }

    Console.WriteLine("Consistency check completed");
  }

  private static Guid GenerateId(IEnumerable<Guid> exclude) {
    var newId = Guid.NewGuid();
    while (exclude.Contains(newId)) {
      newId = Guid.NewGuid();
    }
    return newId;
  }
}