using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.Media;

namespace GuiBlast
{
    /// Single Avalonia host on a background UI thread.
    /// - Reuses existing App if already initialized
    /// - Creates a hidden owner MainWindow to support true modal ShowDialog(owner)
    internal static class AvaloniaHost
    {
        private static readonly object Gate = new();
        private static bool _started;
        private static readonly ManualResetEventSlim Ready = new(false);
        private static readonly CancellationTokenSource Cts = new();

        private static Dispatcher Ui { get; set; } = null!;
        public static Window Owner { get; private set; } = null!;
        private static ClassicDesktopStyleApplicationLifetime? _lifetime;

        /// Marshal a function to the Avalonia UI thread.
        public static Task<T> RunOnUI<T>(Func<Task<T>> func)
        {
            EnsureStarted();
            var tcs = new TaskCompletionSource<T>();
            Ui.Post(async void () =>
            {
                try { tcs.TrySetResult(await func()); }
                catch (Exception ex) { tcs.TrySetException(ex); }
            });
            return tcs.Task;
        }

        private static void EnsureStarted()
        {
            if (_started) { EnsureOwner(); return; }

            lock (Gate)
            {
                if (_started) { EnsureOwner(); return; }

                // Reuse an existing Avalonia Application if present
                if (Application.Current is { } existing)
                {
                    Ui = Dispatcher.UIThread;
                    _lifetime = existing.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime
                                ?? throw new InvalidOperationException("ClassicDesktopStyleApplicationLifetime required.");
                    EnsureOwner();
                    _started = true;
                    Ready.Set();
                    return;
                }

                _started = true;

                var t = new Thread(() =>
                {
                    _lifetime = new ClassicDesktopStyleApplicationLifetime
                    {
                        ShutdownMode = ShutdownMode.OnExplicitShutdown
                    };

                    BuildAvaloniaApp().SetupWithLifetime(_lifetime);

                    Ui = Dispatcher.UIThread;

                    EnsureOwner();
                    Ready.Set();

                    // Keep UI thread alive
                    Dispatcher.UIThread.MainLoop(Cts.Token);
                })
                {
                    Name = "GuiBlast-UI",
                    IsBackground = true
                };

                // STA is only meaningful on Windows (COM apartments).
                if (OperatingSystem.IsWindows())
                {
                    t.SetApartmentState(ApartmentState.STA);
                }

                t.Start();
                Ready.Wait();
            }
        }

        private static void EnsureOwner()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                var ev = new ManualResetEventSlim(false);
                Dispatcher.UIThread.Post(() => { EnsureOwner(); ev.Set(); });
                ev.Wait();
                return;
            }

            _lifetime ??= (Application.Current?.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)
                          ?? throw new InvalidOperationException("ClassicDesktopStyleApplicationLifetime required.");

            if (_lifetime.MainWindow == null)
            {
                Owner = new Window
                {
                    ShowInTaskbar = false,
                    CanResize = false,
                    SystemDecorations = SystemDecorations.None,     // no chrome
                    Background = Brushes.Transparent,
                    Opacity = 0,                                     // fully transparent
                    Width = 1,
                    Height = 1,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    ShowActivated = false,
                    IsHitTestVisible = false,
                    // park it off-screen
                    Position = new PixelPoint(-10000, -10000)
                };

                _lifetime.MainWindow = Owner;
                Owner.Show();
                Owner.WindowState = WindowState.Minimized; // keep it minimized just in case
            }
            else
            {
                Owner = _lifetime.MainWindow!;
            }
        }

        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .WithInterFont()
                         .LogToTrace();

        /// Optional: stop the background UI thread
        public static void Shutdown() => Cts.Cancel();
    }
}
