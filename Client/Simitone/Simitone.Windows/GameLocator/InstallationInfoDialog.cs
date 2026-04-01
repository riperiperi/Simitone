using System;
using System.Drawing;
using System.Windows.Forms;

namespace Simitone.Windows.GameLocator
{
    public class InstallationInfoDialog : Form
    {
        public InstallationInfoDialog(string installationType, string gamePath, string savesPath, string simitoneSavesPath)
        {
            InitializeComponents(installationType, gamePath, savesPath, simitoneSavesPath);
        }

        private void InitializeComponents(string installationType, string gamePath, string savesPath, string simitoneSavesPath)
        {
            // Form settings
            this.Text = "The Sims Installation Configured";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Main panel with padding
            var mainPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(560, 430),
                AutoScroll = true
            };

            int yPos = 10;

            // Success icon/text
            var successLabel = new Label
            {
                Text = "✓ Installation Configured Successfully",
                Location = new Point(10, yPos),
                Size = new Size(540, 30),
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };
            mainPanel.Controls.Add(successLabel);
            yPos += 40;

            // Installation type
            var installLabel = new Label
            {
                Text = $"Installation Type:\n{installationType}",
                Location = new Point(10, yPos),
                Size = new Size(540, 40),
                Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold)
            };
            mainPanel.Controls.Add(installLabel);
            yPos += 50;

            // Game files path
            var gamePathLabel = new Label
            {
                Text = $"Game Files Location:\n{gamePath}",
                Location = new Point(10, yPos),
                Size = new Size(540, 40),
                AutoSize = false
            };
            mainPanel.Controls.Add(gamePathLabel);
            yPos += 50;

            // Saves path
            var savesPathLabel = new Label
            {
                Text = $"Original Saves Location:\n{savesPath}",
                Location = new Point(10, yPos),
                Size = new Size(540, 40),
                AutoSize = false
            };
            mainPanel.Controls.Add(savesPathLabel);
            yPos += 50;

            // Simitone saves path
            var simitoneSavesLabel = new Label
            {
                Text = $"Simitone Saves Location (Safe Copy):\n{simitoneSavesPath}",
                Location = new Point(10, yPos),
                Size = new Size(540, 40),
                AutoSize = false,
                Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold)
            };
            mainPanel.Controls.Add(simitoneSavesLabel);
            yPos += 60;

            // Warning section
            var warningPanel = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(540, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow
            };

            var warningIcon = new Label
            {
                Text = "⚠",
                Location = new Point(10, 10),
                Size = new Size(30, 30),
                Font = new Font(this.Font.FontFamily, 16, FontStyle.Bold),
                ForeColor = Color.DarkOrange
            };
            warningPanel.Controls.Add(warningIcon);

            var warningText = new Label
            {
                Text = "IMPORTANT: Steam and non-Steam saves are NOT compatible!\n\n" +
                       "If you switch installations later, you will need to start fresh. " +
                       "Your Simitone saves are safely stored separately and won't affect " +
                       "your original game saves.",
                Location = new Point(45, 10),
                Size = new Size(480, 80),
                AutoSize = false
            };
            warningPanel.Controls.Add(warningText);

            mainPanel.Controls.Add(warningPanel);
            yPos += 110;

            // How to change section
            var changeLabel = new Label
            {
                Text = "To change installation later:",
                Location = new Point(10, yPos),
                Size = new Size(540, 20),
                Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold)
            };
            mainPanel.Controls.Add(changeLabel);
            yPos += 25;

            var changeOption1 = new Label
            {
                Text = "• Delete config.ini from your Simitone documents folder and restart",
                Location = new Point(10, yPos),
                Size = new Size(540, 20),
                AutoSize = false
            };
            mainPanel.Controls.Add(changeOption1);
            yPos += 25;

            var changeOption2 = new Label
            {
                Text = "• Use command line: Simitone.exe -path\"C:\\Your\\Path\\Here\"",
                Location = new Point(10, yPos),
                Size = new Size(540, 20),
                AutoSize = false
            };
            mainPanel.Controls.Add(changeOption2);

            this.Controls.Add(mainPanel);

            // OK Button
            var okButton = new Button
            {
                Text = "OK",
                Location = new Point(250, 450),
                Size = new Size(100, 35),
                DialogResult = DialogResult.OK
            };
            okButton.Click += (s, e) => this.Close();
            this.Controls.Add(okButton);

            this.AcceptButton = okButton;
        }
    }
}
