using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Simitone.Windows.GameLocator
{
    public class InstallationSelector : Form
    {
        private ListBox installListBox;
        private Button okButton;
        private Button cancelButton;
        private Label instructionLabel;
        private List<(string description, string path, TS1InstallationType type)> installations;

        public string SelectedPath { get; private set; }
        public TS1InstallationType SelectedType { get; private set; }

        public InstallationSelector(List<(string description, string path, TS1InstallationType type)> installations)
        {
            this.installations = installations;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Text = "Select The Sims Installation";
            this.Size = new Size(600, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Instruction label
            instructionLabel = new Label
            {
                Text = "Multiple installations of The Sims were detected. Please select which one to use:",
                Location = new Point(10, 10),
                Size = new Size(560, 40),
                AutoSize = false
            };

            // ListBox for installations
            installListBox = new ListBox
            {
                Location = new Point(10, 60),
                Size = new Size(560, 150)
            };

            foreach (var (description, path, type) in installations)
            {
                installListBox.Items.Add($"{description}\n{path}");
            }

            if (installListBox.Items.Count > 0)
            {
                installListBox.SelectedIndex = 0;
            }

            installListBox.DoubleClick += (s, e) => OkButton_Click(s, e);

            // OK Button
            okButton = new Button
            {
                Text = "OK",
                Location = new Point(410, 220),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            // Cancel Button
            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(495, 220),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

            // Add controls to form
            this.Controls.Add(instructionLabel);
            this.Controls.Add(installListBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (installListBox.SelectedIndex >= 0)
            {
                var selected = installations[installListBox.SelectedIndex];
                SelectedPath = selected.path;
                SelectedType = selected.type;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select an installation.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
