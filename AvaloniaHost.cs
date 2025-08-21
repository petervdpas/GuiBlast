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
    /// <summary>
    /// Provides a singleton Avalonia host running on a dedicated background UI thread.
    /// <list type="bullet">
    /// <item>Reuses an existing <see cref="Application"/> if one is already initialized.</item>
    /// <item>Creates a hidden owner <see cref="Window"/> to support proper modal <see cref="Window.ShowDialog{TWindow}(Window)"/> usage.</item>
    /// <item>Ensures that all UI code executes on Avalonia's <see cref="Dispatcher.UIThread"/>.</item>
    /// </list>
    /// </summary>
    internal static class AvaloniaHost
    {
        private static readonly object Gate = new();
        private static bool _started;
        private static readonly ManualResetEventSlim Ready = new(false);
        private static readonly CancellationTokenSource Cts = new();

        private static Dispatcher Ui { get; set; } = null!;

        /// <summary>
        /// Gets the hidden <see cref="Window"/> used as the owner for modal dialogs.
        /// Ensures that calls to <see cref="Window.ShowDialog{TWindow}(Window)"/> always have a valid owner.
        /// </summary>
        public static Window Owner { get; private set; } = null!;

        private static ClassicDesktopStyleApplicationLifetime? _lifetime;

        /// <summary>
        /// Executes the specified asynchronous function on Avalonia's UI thread.
        /// </summary>
        /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
        /// <param name="func">The asynchronous function to execute on the UI thread.</param>
        /// <returns>A <see cref="Task{TResult}"/> that completes with the result of <paramref name="func"/>.</returns>
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

        /// <summary>
        /// Ensures that the Avalonia application and UI thread are initialized.
        /// If not already started, a new background UI thread is created.
        /// </summary>
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

        /// <summary>
        /// Ensures that the hidden <see cref="Owner"/> window exists.
        /// This window is required for modal dialogs and is created only once.
        /// </summary>
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
                    // Park it off-screen
                    Position = new PixelPoint(-10000, -10000)
                };

                _lifetime.MainWindow = Owner;
                Owner.Show();
                Owner.WindowState = WindowState.Minimized; // Keep it minimized just in case
            }
            else
            {
                Owner = _lifetime.MainWindow!;
            }
        }

        /// <summary>
        /// Builds a minimal Avalonia <see cref="AppBuilder"/>.
        /// Used when starting a fresh Avalonia application in the background.
        /// </summary>
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .WithInterFont()
                         .LogToTrace();

        /// <summary>
        /// Shuts down the background Avalonia UI thread and cancels the dispatcher loop.
        /// </summary>
        public static void Shutdown() => Cts.Cancel();
    }
}
