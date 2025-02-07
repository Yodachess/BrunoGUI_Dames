namespace BrunoGUI_Dames
{
    partial class DonneesBrutesDames
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DonneesBrutesVue = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // DonneesBrutesVue
            // 
            this.DonneesBrutesVue.BackColor = System.Drawing.Color.LightSteelBlue;
            this.DonneesBrutesVue.ForeColor = System.Drawing.Color.DarkBlue;
            this.DonneesBrutesVue.Location = new System.Drawing.Point(12, 12);
            this.DonneesBrutesVue.Name = "DonneesBrutesVue";
            this.DonneesBrutesVue.Size = new System.Drawing.Size(1387, 905);
            this.DonneesBrutesVue.TabIndex = 0;
            this.DonneesBrutesVue.Text = "";
            // 
            // DonneesBrutesDames
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1411, 926);
            this.Controls.Add(this.DonneesBrutesVue);
            this.Name = "DonneesBrutesDames";
            this.Text = "DonneesBrutesDames";
            this.Load += new System.EventHandler(this.DonneesBrutesDames_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox DonneesBrutesVue;
    }
}