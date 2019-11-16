using FSO.Client;
using FSO.LotView;
using FSO.UI;
using Simitone.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simitone.Windows
{
    public class GameStartProxy
    {
        public void Start(bool useDX)
        {
            GameFacade.DirectX = useDX;
            World.DirectX = useDX;
            SimitoneGame game = new SimitoneGame();
            var form = (Form)Form.FromHandle(game.Window.Handle);
            if (form != null) form.FormClosing += Form_FormClosing;
            game.Run();
            game.Dispose();
        }

        private static void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !(GameFacade.Screens.CurrentUIScreen?.CloseAttempt() ?? true);
        }
    }
}
