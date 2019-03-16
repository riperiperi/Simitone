using FSO.Client;
using FSO.Common.Rendering.Framework;
using FSO.Common.Rendering.Framework.Camera;
using FSO.Content;
using FSO.SimAntics;
using FSO.SimAntics.Model;
using FSO.Vitaboy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simitone.Client.UI.Model
{
    /// <summary>
    /// Caches icons for objects missing them, eg. heads, some catalog objects..
    /// </summary>
    public static class UIIconCache
    {
        //indexed as mesh:texture
        private static Dictionary<string, Texture2D> AvatarHeadCache = new Dictionary<string, Texture2D>();

        public static Texture2D GetObject(VMEntity obj)
        {
            if (obj is VMAvatar)
            {
                var ava = (VMAvatar)obj;
                var headname = ava.HeadOutfit.Name;
                if (headname == "") headname = ava.BodyOutfit.OftData.TS1TextureID;
                var id = headname +":"+ ava.HeadOutfit.OftData.TS1TextureID;

                Texture2D result = null;
                if (!AvatarHeadCache.TryGetValue(id, out result))
                {
                    result = GenHeadTex(ava);
                    AvatarHeadCache[id] = result;
                }
                return result;
            }
            else if (obj is VMGameObject)
            {
                if (obj.Object.OBJ.GUID == 0x000007C4) return Content.Get().CustomUI.Get("int_gohere.png").Get(GameFacade.GraphicsDevice);
                else return obj.GetIcon(GameFacade.GraphicsDevice, 0);
            }
            return null;
        }

        public static Texture2D GenHeadTex(VMAvatar ava)
        {
            var m_Head = new SimAvatar(ava.Avatar); //talk about confusing...
            m_Head.StripAllButHead();


            var HeadCamera = new BasicCamera(GameFacade.GraphicsDevice, new Vector3(0.0f, 7.0f, -17.0f), Vector3.Zero, Vector3.Up);

            var pos2 = m_Head.Skeleton.GetBone("HEAD").AbsolutePosition;
            pos2.Y += 0.1f;
            HeadCamera.Position = new Vector3(0, pos2.Y, 12.5f);
            HeadCamera.FOV = (float)Math.PI / 3f;
            HeadCamera.Target = pos2;
            HeadCamera.ProjectionOrigin = new Vector2(74/2, 74/2);

            var HeadScene = new _3DTargetScene(GameFacade.GraphicsDevice, HeadCamera, new Point(74, 74), (GlobalSettings.Default.AntiAlias > 0) ? 8 : 0);
            HeadScene.ID = "UIPieMenuHead";

            m_Head.Scene = HeadScene;
            m_Head.Scale = new Vector3(1f);

            HeadCamera.Zoom = 13f;

            //rotate camera, similar to pie menu

            double xdir = Math.Atan(50 / 100.0);
            double ydir = Math.Atan(-50 / 100.0);

            Vector3 off = new Vector3(0, 0, 13.5f);
            Matrix mat = Microsoft.Xna.Framework.Matrix.CreateRotationY((float)xdir) * Microsoft.Xna.Framework.Matrix.CreateRotationX((float)ydir);

            HeadCamera.Position = new Vector3(0, pos2.Y, 0) + Vector3.Transform(off, mat);

            if (ava.IsPet)
            {
                HeadCamera.Zoom *= 1.5f;
            }
            //end rotate camera

            HeadScene.Initialize(GameFacade.Scenes);
            HeadScene.Add(m_Head);

            HeadScene.Draw(GameFacade.GraphicsDevice);
            return HeadScene.Target;
        }
    }
}
