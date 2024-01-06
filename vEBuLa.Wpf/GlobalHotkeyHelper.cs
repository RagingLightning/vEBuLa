using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace vEBuLa;
internal class GlobalHotkeyHelper {
  [DllImport("User32.dll")] private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);
  [DllImport("User32.dll")] private static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);
  [DllImport("kernel32.dll")] private static extern uint GetLastError();

  private ILogger<GlobalHotkeyHelper>? Logger => App.GetService<ILogger<GlobalHotkeyHelper>>();
  private HwndSource _source;
  private readonly Random IdGenerator = new Random();
  private readonly Dictionary<int, Action> HotkeyHandlers = new();

  public GlobalHotkeyHelper() {
    var helper = new WindowInteropHelper(Application.Current.MainWindow);
    _source = HwndSource.FromHwnd(helper.Handle);
    _source.AddHook(HwndHook);
  }

  ~GlobalHotkeyHelper() {
    _source.RemoveHook(HwndHook);
    _source = null;
    UnregisterHotKeys();
  }

  public void RegisterHotKey(Action handler, uint vkCode, uint modifiers = 0) {
    var helper = new WindowInteropHelper(Application.Current.MainWindow);
    var id = IdGenerator.Next();
    if (HotkeyHandlers.ContainsKey(id)) id = IdGenerator.Next();
    if (!RegisterHotKey(helper.Handle, id, modifiers, vkCode)) {
      Logger?.LogError("Failed to create global hotkey {Modifiers}&{VKCode} for {Handler}: {ErrorCode}", modifiers, vkCode, handler, GetLastError());
    }
    Logger?.LogDebug("Global hotkey {Modifiers}&{VKCode} for {Handler} registered", modifiers, vkCode, handler);
    HotkeyHandlers[id] = handler;
  }

  public void UnregisterHotKey(Action handler) {
    var helper = new WindowInteropHelper(Application.Current.MainWindow);
    if (!HotkeyHandlers.Any(e => e.Value == handler)) {
      Logger?.LogWarning("Tried to unregister nonexistent hotkey for {Handler}", handler);
      return;
    }
    var id = HotkeyHandlers.First(e => e.Value == handler).Key;
    UnregisterHotKey(helper.Handle, id);
    HotkeyHandlers.Remove(id);
  }

  public void UnregisterHotKeys() {
    var helper = new WindowInteropHelper(Application.Current.MainWindow);
    foreach (var handler in HotkeyHandlers) {
      UnregisterHotKey(helper.Handle, handler.Key);
    }
    HotkeyHandlers.Clear();
  }

  private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
    const int WM_HOTKEY = 0x0312;
    switch (msg) {
      case WM_HOTKEY:
        if (HotkeyHandlers.ContainsKey(wParam.ToInt32()))
          HotkeyHandlers[wParam.ToInt32()]();
        break;
    }
    return IntPtr.Zero;
  }
}
