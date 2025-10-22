using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Models;

namespace vEBuLa.Events;

public delegate void PositionPassedEventHandler(object? sender, PositionPassedEventArgs args);

public record PositionPassedEventArgs(EbulaEntry Entry, Vector2 Position);
