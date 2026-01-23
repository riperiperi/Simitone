using FSO.Common;
using FSO.Common.Rendering.Framework;
using FSO.Common.Utils;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace Simitone.Client.Utils
{
    // Manages Simitone-specific custom cursors that aren't part of FreeSO.
    public static class SimitoneCursors
    {
        private static MouseCursor EyedropperCursor;
        private static bool Initialized;

        // Initialize Simitone-specific cursors.
        public static void Init(GraphicsDevice gd)
        {
            if (Initialized) return;

            var pngPath = Path.Combine(FSOEnvironment.ContentDir, "Cursors", "eyedropper.png");

            if (File.Exists(pngPath))
            {
                try
                {
                    using (var stream = File.Open(pngPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var texture = Texture2D.FromStream(gd, stream);
                        // Hotspot at tip of eyedropper (adjust X,Y as needed)
                        EyedropperCursor = MouseCursor.FromTexture2D(texture, 1, 1);
                    }
                }
                catch (Exception)
                {
                    EyedropperCursor = null;
                }
            }

            Initialized = true;
        }

        // Sets the eyedropper cursor if available, otherwise uses a fallback.
        public static void SetEyedropperCursor()
        {
            if (EyedropperCursor != null)
            {
                Mouse.SetCursor(EyedropperCursor);
            }
            else
            {
                // Fallback to crosshair if custom cursor not available
                Mouse.SetCursor(MouseCursor.Crosshair);
            }
        }

        // Returns true if the eyedropper cursor was loaded successfully.
        public static bool HasEyedropperCursor => EyedropperCursor != null;
    }
}
