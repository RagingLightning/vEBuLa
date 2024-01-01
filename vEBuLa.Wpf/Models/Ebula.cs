using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace vEBuLa.Models;
public class Ebula {
  public List<EbulaEntry> Entries { get; } = new();
  public int currentEntry = 0;

  public Ebula() {
    Entries.Add(new EbulaEntry { TunnelEnd = true, SpeedLimit = 100, Location = 39300 });
    Entries.Add(new EbulaEntry { Location = 39600, LocationName = "Bksig", Symbol = EbulaSymbol.WEICHENBEREICH });
    Entries.Add(new EbulaEntry { Location = 40000, SpeedLimit = 120 });
    Entries.Add(new EbulaEntry { Location = 41600, LocationName = "Esig", LocationNotes = "E60" });
    Entries.Add(new EbulaEntry { Location = 43000, LocationName = "Eitorf", LocationNameBold = true, Arrival = new TimeSpan(6, 58, 18), Departure = new TimeSpan(6, 59, 0) });
    Entries.Add(new EbulaEntry { Location = 43200, LocationName = "Asig" });
    Entries.Add(new EbulaEntry { Location = 43400, Symbol = EbulaSymbol.WEICHENBEREICH });
    Entries.Add(new EbulaEntry { Location = 48000, SpeedLimit = 100 });
    Entries.Add(new EbulaEntry { Location = 49000, LocationName = "Esig" });
    Entries.Add(new EbulaEntry { Location = 49800, LocationName = "Herchen", LocationNameBold = true, Arrival = new TimeSpan(7, 3, 18), Departure = new TimeSpan(7, 4, 0) });
    Entries.Add(new EbulaEntry { Location = 50000, LocationName = "Asig", LocationNotes = "A60" });
    Entries.Add(new EbulaEntry { Location = 50200, Symbol = EbulaSymbol.WEICHENBEREICH });
    Entries.Add(new EbulaEntry { Location = 50500, TunnelStart = true, LocationName = "Herchener Tunnel", Symbol = EbulaSymbol.STUMPFGLEIS });
    Entries.Add(new EbulaEntry { Location = 50500, SpeedLimit = 120, Symbol = EbulaSymbol.BERMSWEG_KURZ });
    Entries.Add(new EbulaEntry { Location = 50900, TunnelEnd = true, Symbol = EbulaSymbol.ZUGFUNK });
  }

  public bool Load(string fileName) {
    try {
      List<EbulaEntry>? entries = JsonConvert.DeserializeObject<List<EbulaEntry>>(File.ReadAllText(fileName), new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });
      if (entries is null) return false;
      Entries.Clear();
      Entries.AddRange(entries);
      return true;
    }
    catch (Exception) {
      return false;
    }
  }

  public bool Save(string fileName) {
    string json = JsonConvert.SerializeObject(Entries, Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });
    File.WriteAllText(fileName, json);
    return true;
  }
}
