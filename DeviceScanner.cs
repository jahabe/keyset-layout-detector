/// Queries the Windows PnP device list via WMI to detect whether
/// a specific USB device is currently connected.

using System;
using System.Management;

namespace SurfaceKeysetAutoLayout {
    internal static class DeviceScanner {
        public static bool IsConnected(string targetDeviceId, out string? matchedPnpId) {
            matchedPnpId = null;

            try {
                // WMI query: fetch only the fields we need to keep it lightweight.
                using var searcher = new ManagementObjectSearcher(
                    "SELECT PNPDeviceID, Name FROM Win32_PnPEntity"
                );

                int totalCount = 0;
                int vendorMatchCount = 0;

                // Extract VID prefix for diagnostic logging (e.g. "VID_03EB").
                string vendorPrefix = ExtractVendorPrefix(targetDeviceId);

                Console.WriteLine("[DeviceScanner] Scanning PnP devices...");

                foreach (ManagementObject obj in searcher.Get()) {
                    totalCount++;
                    var pnpId = (obj["PNPDeviceID"] as string) ?? "";

                    // Log all devices from our vendor for easier debugging.
                    if (!string.IsNullOrEmpty(vendorPrefix) &&
                        pnpId.Contains(vendorPrefix, StringComparison.OrdinalIgnoreCase)) {
                        vendorMatchCount++;
                        Console.WriteLine($"[DeviceScanner] Found vendor device: {pnpId}");
                    }

                    // Full target match check.
                    if (pnpId.IndexOf(targetDeviceId, StringComparison.OrdinalIgnoreCase) >= 0) {
                        matchedPnpId = pnpId;
                        Console.WriteLine($"[DeviceScanner] MATCHED target: {pnpId}");
                        return true;
                    }
                }

                Console.WriteLine(
                    $"[DeviceScanner] Scanned {totalCount} devices. " +
                    $"Found {vendorMatchCount} vendor device(s). " +
                    $"Target '{targetDeviceId}' not present."
                );
            }
            catch (Exception ex) {
                Console.WriteLine($"[DeviceScanner] WMI query failed: {ex.Message}");
            }

            return false;
        }

        // Extracts "VID_XXXX" from a full device ID string for vendor-level logging.
        private static string ExtractVendorPrefix(string deviceId) {
            int ampIdx = deviceId.IndexOf('&');
            return ampIdx > 0 ? deviceId[..ampIdx] : deviceId;
        }
    }
}
