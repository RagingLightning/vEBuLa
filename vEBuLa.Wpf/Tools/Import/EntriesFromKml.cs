using Serilog;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using vEBuLa.Models;

namespace vEBuLa.Tools.Import;

internal partial class EntriesFromKml {

  public static void Run(FileInfo input, DirectoryInfo output, bool dryRun) {
    Log.Information("+---------------------");
    Log.Information("| vEBuLa v0.1 - KML importer");
    Log.Information("| KML data file   : {file}", input);
    Log.Information("| Output directory: {file}", output);
    Log.Information("+---------------------");
    Log.Information("");

    if (!input.Exists) {
      Log.Error("Input file {file} does not exist, exiting", input);
      return;
    }

    var data = XDocument.Load(input.FullName);

    var kml = data.Elements().FirstOrDefault(e => e.Name.LocalName == "kml");
    var document = kml?.Elements().FirstOrDefault(e => e.Name.LocalName == "Document");

    if (document is null) {
      Log.Error("Invalid KML format, exiting");
      return;
    }

    if (dryRun)
      Log.Information("!! Dry run, no files will be written !!");
    else
      Log.Information("!! Existing files will be overwritten !!");
    Log.Information("Starting conversion in 5 seconds...");
    Log.Information("");

    Thread.Sleep(5000);

    foreach (var presetFolder in document.Elements().Where(e => e.Name.LocalName == "Folder")) {
      var presetName = presetFolder.Elements().FirstOrDefault(e => e.Name.LocalName == "name")?.Value;
      if (string.IsNullOrWhiteSpace(presetName)) {
        Log.Information("Found preset folder without name, skipping");
        continue;
      }

      Log.Information("Converting {presetName}", presetName);

      var filePath = Path.Combine(output.FullName, $"{presetName}.ebula");
      EbulaConfig preset;
      if (File.Exists(filePath)) {
        Log.Information("Found matching preset file {presetFile}, loading existing data", $"{presetName}.ebula");
        preset = new EbulaConfig(filePath);

        if (!dryRun)
          File.Copy(filePath, $"{filePath}.bak", true);
      }
      else {
        Log.Warning($"No matching preset file was found, creating new preset");
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

      // ValidateServices(preset);

      if (!dryRun)         preset.Save(Path.Combine(output.FullName, $"{presetName}.ebula"));
    }
  }

  private static void ProcessRoute(EbulaConfig preset, Dictionary<Guid, EbulaSegment> segments, Dictionary<Guid, EbulaStation> stations, XElement routeFolder) {
    var routeName = routeFolder.Elements().FirstOrDefault(e => e.Name.LocalName == "name")?.Value;
    if (string.IsNullOrWhiteSpace(routeName)) {
      Log.Error(" - Found route folder without name, skipping");
      return;
    }
    Log.Information(" - Converting Route: {routeName}", routeName);

    foreach (var segmentFolder in routeFolder.Elements().Where(e => e.Name.LocalName == "Folder")) {
      ProcessSegment(preset, segments, stations, routeName, segmentFolder);
    }
  }

  private static void ProcessSegment(EbulaConfig config, Dictionary<Guid, EbulaSegment> segments, Dictionary<Guid, EbulaStation> stations, string routeName, XElement segmentFolder) {
    var rawSegmentName = segmentFolder.Elements().FirstOrDefault(e => e.Name.LocalName == "name")?.Value;
    if (string.IsNullOrWhiteSpace(rawSegmentName)) {
      Log.Error(" - - Found route segment folder without name, skipping");
      return;
    }

    var segmentStartEnd = rawSegmentName.Split("_");
    if (segmentStartEnd.Length == 1) {
      Log.Error(" - - Invalid segment name: {segmentName}, skipping", rawSegmentName);
      return;
    }

    var segmentStart = segmentStartEnd[0];
    var segmentEnd = segmentStartEnd[1];
    Log.Information(" - - Converting Route Segment: {segmentStart} - {segmentEnd}", segmentStart, segmentEnd);

    // Get start and end stations from preset
    var segmentStartLocation = segmentStart.Split("|")[0];
    var segmentEndLocation = segmentEnd.Split("|")[0];
    var startStation = stations.Values.FirstOrDefault(s => s.Name == segmentStartLocation);
    var endStation = stations.Values.FirstOrDefault(s => s.Name == segmentEndLocation);

    // Create stations if they do not exist
    if (startStation is null) {
      Log.Warning(" - - - Adding new station: {segmentStart}", segmentStartLocation);
      var id = GenerateId(stations.Keys);
      startStation = new EbulaStation(config, id, segmentStartLocation);
      stations.Add(id, startStation);
    }
    else {
      Log.Information(" - - - Using existing station: {segmentStart}", segmentStartLocation);
    }
    if (endStation is null) {
      Log.Warning(" - - - Adding new station: {segmentEnd}", segmentEndLocation);
      var id = GenerateId(stations.Keys);
      endStation = new EbulaStation(config, id, segmentEndLocation);
      stations.Add(id, endStation);
    }
    else {
      Log.Information(" - - - Using existing station: {segmentEnd}", segmentEndLocation);
    }

    // Add stations back to preset
    config.Stations[startStation.Id] = startStation;
    config.Stations[endStation.Id] = endStation;

    // Get existing segment if it exists
    var segmentName = $"{routeName} - {rawSegmentName.Replace('_', '>')}";
    var segment = segments.Values.FirstOrDefault(s =>
      s.Origin == startStation && s.Destination == endStation && s.Name == segmentName
    );

    // Create new segment if it does not exist
    if (segment is null) {
      Log.Warning(" - - - Adding new segment: {segmentName}", segmentName);
      var id = GenerateId(segments.Keys);
      segment = new EbulaSegment(config, id, segmentName) {
        Origin = startStation,
        Destination = endStation
      };
      segments.Add(id, segment);
    }
    else {
      Log.Information(" - - - Using existing segment: {segmentName}", segmentName);
    }

    // Add segment back to preset
    config.Segments.Add(segment.Id, segment);

    // Clear existing entries
    segment.Entries.Clear();

    foreach (var place in segmentFolder.Elements().Where(e => e.Name.LocalName == "Placemark")) {
      ProcessPlaceMark(segment, place);
    }
  }

  private static void ProcessPlaceMark(EbulaSegment segment, XElement place) {
    var placeName = place.Elements().FirstOrDefault(e => e.Name.LocalName == "name")?.Value;
    if (string.IsNullOrWhiteSpace(placeName)) {
      Log.Error(" - - - - Found place mark without name, skipping");
      return;
    }

    var placeNameSegments = placeName.Split(";");
    if (placeNameSegments.Length != 6) {
      Log.Error(" - - - - Invalid place mark name: {placeName}, skipping", placeName);
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
      Log.Error(" - - - - Invalid place mark name: {placeName}, skipping", placeName);
      return;
    }
    if (!Enum.TryParse<Gradient>(placeNameSegments[1], out var gradient)) {
      Log.Error(" - - - - Invalid place mark name: {placeName}, skipping", placeName);
      return;
    }
    if (!int.TryParse(placeNameSegments[2], out var speedLimit)) {
      Log.Error(" - - - - Invalid place mark name: {placeName}, skipping", placeName);
      return;
    }
    if (!Enum.TryParse<EbulaSymbol>(placeNameSegments[3], out var icon)) {
      Log.Error(" - - - - Invalid place mark name: {placeName}, skipping", placeName);
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
      Log.Error(" - - - - Corrupted place mark: {placeName}, skipping", placeName);
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

    Log.Information(" - - - - Adding Entry: {entry}", entry);
  }

  private static void ValidateServices(EbulaConfig preset) {
    Console.WriteLine();
    Console.WriteLine("Checking configuration consistency");

    foreach (var routeKv in preset.Routes) {
      var route = routeKv.Value;

      foreach (var segment in route.Segments) {
        if (!preset.Segments.ContainsKey(segment.Id))           Console.WriteLine($"Warning: Route {routeKv.Value.Name} references missing segment {segment.Name}");
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