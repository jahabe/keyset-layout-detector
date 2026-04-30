using System;
using System.Management;
using System.Threading;

namespace SurfaceKeysetAutoLayout {
    internal sealed class DeviceWatcher : IDisposable {
        // WMI EventType 2 = device arrival, 3 = device removal.
        private const string AttachQuery = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2";
        private const string DetachQuery = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3";

        // How long to wait after the last event before evaluating device state.
        private const int DebounceMs = 400;

        private ManagementEventWatcher? _attachWatcher;
        private ManagementEventWatcher? _detachWatcher;
        private Timer? _debounceTimer;

        private readonly object _lock = new object();
        private readonly Action _onDeviceChanged;

        public DeviceWatcher(Action onDeviceChanged) {
            _onDeviceChanged = onDeviceChanged ?? throw new ArgumentNullException(nameof(onDeviceChanged));
        }
        public void Start() {
            _debounceTimer = new Timer(_ => _onDeviceChanged(), null, Timeout.Infinite, Timeout.Infinite);

            _attachWatcher = new ManagementEventWatcher(new WqlEventQuery(AttachQuery));
            _detachWatcher = new ManagementEventWatcher(new WqlEventQuery(DetachQuery));

            _attachWatcher.EventArrived += (_, __) => ScheduleEvaluation();
            _detachWatcher.EventArrived += (_, __) => ScheduleEvaluation();

            _attachWatcher.Start();
            _detachWatcher.Start();

            Console.WriteLine("[DeviceWatcher] Started. Watching for USB events.");

            // Evaluate immediately so an already-connected board is detected at launch.
            _onDeviceChanged();
        }

        public void Stop() => Dispose();

        public void Dispose() {
            _attachWatcher?.Stop();
            _attachWatcher?.Dispose();
            _attachWatcher = null;

            _detachWatcher?.Stop();
            _detachWatcher?.Dispose();
            _detachWatcher = null;

            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }

        // Resets the debounce window on every incoming event.
        // The callback fires only after DebounceMs of silence.
        private void ScheduleEvaluation() {
            lock (_lock) {
                _debounceTimer?.Change(DebounceMs, Timeout.Infinite);
                Console.WriteLine($"[DeviceWatcher] Event received. Evaluating in {DebounceMs}ms...");
            }
        }
    }
}
