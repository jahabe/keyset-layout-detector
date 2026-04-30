using System;
using System.Threading;

namespace SurfaceKeysetAutoLayout
{
    internal static class Program
    {
        // ── Configuration ────────────────────────────────────────────────────────

        private const string TargetDeviceId = "CHANGE_TO_YOUR_BOARD_ID";

        private const string TargetLangCode = "DE";

        private const string TargetKlid = "00000407";

        // ── State ────────────────────────────────────────────────────────────────

        private static readonly LayoutApplier _applier = new LayoutApplier();
        private static readonly object _lock = new object();

        private static bool _isApplied;

        // ── Entry point ──────────────────────────────────────────────────────────

        private static void Main() {
            Console.WriteLine("=== SurfaceKeysetAutoLayout — Stage 2 ===");
            Console.WriteLine($"[Config] Target device : {TargetDeviceId}");
            Console.WriteLine($"[Config] Target layout : {TargetLangCode} (KLID={TargetKlid})");
            Console.WriteLine("[Info]  Press Enter to exit.\n");

            using var watcher = new DeviceWatcher(EvaluateState);
            watcher.Start();

            Console.ReadLine();

            Console.WriteLine("[Info] Shutting down...");
        }

        // ── State machine ────────────────────────────────────────────────────────

        private static void EvaluateState()
        {
            lock (_lock) {
                bool connected = DeviceScanner.IsConnected(TargetDeviceId, out string? matchedId);

                Console.WriteLine($"[State] connected={connected}, layoutApplied={_isApplied}");

                if (connected && !_isApplied) {
                    Console.WriteLine($"[Attach] Detected: {matchedId}");
                    TryApply();
                    _isApplied = true;
                } else if (!connected && _isApplied) {
                    Console.WriteLine("[Detach] Device removed.");
                    TryRevert();
                    _isApplied = false;
                }
                // else: no state change — board was already applied or already absent.
            }
        }

        private static void TryApply()
        {
            try {
                Console.WriteLine($"[Layout] Applying {TargetLangCode} ({TargetKlid})...");
                _applier.Apply(TargetKlid);
            } catch (Exception ex) {
                Console.WriteLine($"[ERROR] Apply failed: {ex.Message}");
            }
        }

        private static void TryRevert() {
            try {
                Console.WriteLine("[Layout] Reverting to original layout...");
                _applier.Revert();
            } catch (Exception ex) {
                Console.WriteLine($"[ERROR] Revert failed: {ex.Message}");
            }
        }
    }
}
