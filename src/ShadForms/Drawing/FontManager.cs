using ShadForms.Interop;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ShadForms.Drawing;

internal static class FontManager
{
    private static readonly PrivateFontCollection _fontCollection = new();
    private static readonly List<IntPtr> _gdiHandles = [];
    private static readonly List<IntPtr> _allocatedMemory = [];

    private static readonly ConcurrentDictionary<string, Font> _fontCache = new();

    static FontManager()
    {
        LoadEmbeddedFont("ShadForms.Assets.Fonts.Geist-Regular.ttf");
        LoadEmbeddedFont("ShadForms.Assets.Fonts.Geist-Medium.ttf");
        LoadEmbeddedFont("ShadForms.Assets.Fonts.Geist-SemiBold.ttf");
        LoadEmbeddedFont("ShadForms.Assets.Fonts.Geist-Bold.ttf");
    }

    public static Font Geist(float size, FontStyle style = FontStyle.Regular)
    {
        string cacheKey = $"{size}pt_{style}";

        return _fontCache.GetOrAdd(cacheKey, _ => CreateFontInstance(size, style));
    }

    private static Font CreateFontInstance(float size, FontStyle style)
    {
        var fontFamily = _fontCollection.Families
            .FirstOrDefault(f => f.Name.Equals("Geist", StringComparison.OrdinalIgnoreCase));

        return fontFamily is not null
            ? new Font(fontFamily, size, style)
            : new Font("Segoe UI", size, style); // Fallback
    }

    private static void LoadEmbeddedFont(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            Debug.WriteLine($"[ShadForms] Font resource not found: {resourceName}");
            return;
        }

        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        var fontData = buffer.ToArray();

        IntPtr unmanagedMemory = Marshal.AllocCoTaskMem(fontData.Length);

        Marshal.Copy(fontData, 0, unmanagedMemory, fontData.Length);
        _allocatedMemory.Add(unmanagedMemory);

        // Add to GDI+
        _fontCollection.AddMemoryFont(unmanagedMemory, fontData.Length);

        // Add to GDI
        uint count = 0;
        IntPtr gdiHandle = NativeMethods.AddFontMemResourceEx(
            unmanagedMemory,
            (uint)fontData.Length,
            IntPtr.Zero,
            ref count);

        if (gdiHandle != IntPtr.Zero)
        {
            _gdiHandles.Add(gdiHandle);
        }
    }
}
