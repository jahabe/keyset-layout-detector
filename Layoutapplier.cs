// Switches the Windows keyboard layout using Win32 APIs via P/Invoke.

using System;
using System.Runtime.InteropServices;

namespace SurfaceKeysetAutoLayout {
    internal sealed class LayoutApplier {
        private const uint KlfActivate = 0x00000001;
        private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);

        private IntPtr _originalHkl = IntPtr.Zero;
        private bool _hasOriginal;
        private bool _applied;

        public void Apply(string klid) {
            if (string.IsNullOrWhiteSpace(klid) || klid.Length != 8)
                throw new ArgumentException("KLID must be exactly 8 hex characters (e.g., 00000407).", nameof(klid));

            if (!_hasOriginal) {
                _originalHkl = GetKeyboardLayout(0);
                _hasOriginal = true;
                Console.WriteLine($"[LayoutApplier] Saved original HKL: 0x{_originalHkl.ToInt64():X}");
            }

            IntPtr hkl = LoadKeyboardLayout(klid, KlfActivate);
            if (hkl == IntPtr.Zero)
                throw new InvalidOperationException($"LoadKeyboardLayout failed for KLID={klid}. " +
                    "Ensure the layout is installed on this machine.");

            IntPtr prev = ActivateKeyboardLayout(hkl, 0);
            if (prev == IntPtr.Zero)
                throw new InvalidOperationException("ActivateKeyboardLayout failed.");

            PostMessage(HWND_BROADCAST, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, hkl);

            _applied = true;
            Console.WriteLine($"[LayoutApplier] Applied KLID={klid}, HKL=0x{hkl.ToInt64():X}. Broadcast sent.");
        }

        public void Revert() {
            if (!_applied || !_hasOriginal || _originalHkl == IntPtr.Zero) {
                Console.WriteLine("[LayoutApplier] Nothing to revert.");
                return;
            }

            IntPtr prev = ActivateKeyboardLayout(_originalHkl, 0);
            if (prev == IntPtr.Zero)
                throw new InvalidOperationException("Revert: ActivateKeyboardLayout failed.");

            PostMessage(HWND_BROADCAST, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, _originalHkl);

            _applied = false;
            Console.WriteLine($"[LayoutApplier] Reverted to original HKL: 0x{_originalHkl.ToInt64():X}. Broadcast sent.");
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        private static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, uint Flags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }
}
