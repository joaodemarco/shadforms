using System.Runtime.InteropServices;

namespace ShadForms.Interop;

internal static class NativeMethods
{
    [DllImport("gdi32.dll")]
    internal static extern IntPtr AddFontMemResourceEx(
        IntPtr pbFont,
        uint cbFont,
        IntPtr pdv,
        [In] ref uint pcFonts);
}
