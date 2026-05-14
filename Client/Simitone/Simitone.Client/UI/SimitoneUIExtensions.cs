using FSO.Client.UI.Controls;
using FSO.SimAntics;
using FSO.SimAntics.Model.TS1Platform;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI
{
    internal static class SimitoneSimAnticsVMExtensions
    {
        /// <summary>
        /// Removes all the Avatars on the lot
        /// </summary>
        /// <param name="vm"></param>
        public static void CleanUpAllPeople(this VMTS1LotState State, VM vm)
        {
            List<VMEntity> avatars = vm.Context.ObjectQueries.Avatars;

            while (avatars.Any())
                vm.RemoveEntity(avatars[0]);
        }
    }
    internal static class SimitoneUIExtensions
    {
        /// <summary>
        /// Adjusts the caption font size to fit within the specified available horizontal space, accounting for
        /// optional margin.
        /// </summary>
        /// <remarks>This method reduces the caption font size if the measured width of the caption
        /// exceeds the available horizontal space minus the specified margin. The adjustment ensures that the caption
        /// fits within the designated area.</remarks>
        /// <param name="AvailableSpace">The total available space, in pixels, for rendering the caption. The X component specifies the horizontal
        /// space; the Y component is ignored.</param>
        /// <param name="Margin">An optional margin, in pixels, to subtract from the available space. The X component specifies the
        /// horizontal margin; the Y component is ignored. Defaults to <see cref="Vector2.Zero"/> if not specified.</param>
        public static void FitAvailableSpace(this UILabel Label, Point AvailableSpace, Point Margin = default)
        {
            Vector2 desiredSize = Label.CaptionStyle.MeasureString(Label.Caption);
            if (desiredSize.X > (AvailableSpace.X - Margin.X))
            {
                Label.CaptionStyle.Size = Math.Max(1, Label.CaptionStyle.Size - 1); // find delta size for this font
                Vector2 scaledSize = Label.CaptionStyle.MeasureString(Label.Caption);

                float delta = desiredSize.X - scaledSize.X;
                float overage = desiredSize.X - (AvailableSpace.X - Margin.X);

                int fontSize = (int)(37 - (overage / delta));
                Label.CaptionStyle.Size = fontSize; // new scaled size
            }
        }
    }
}
