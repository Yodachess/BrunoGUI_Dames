namespace BrunoGUI_Dames
{
    partial class FichierPartiePdn
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
            this.TableauPartiesPdn = new System.Windows.Forms.DataGridView();
            this.joueurBlanc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.joueurNoir = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResultatPartie = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.numeroRonde = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.datePartie = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.evenementPartie = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sitePartie = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nombreDemiCoups = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NombrePartiesFichier = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TableauPartiesPdn)).BeginInit();
            this.SuspendLayout();
            // 
            // TableauPartiesPdn
            // 
            this.TableauPartiesPdn.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TableauPartiesPdn.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.joueurBlanc,
            this.joueurNoir,
            this.ResultatPartie,
            this.numeroRonde,
            this.datePartie,
            this.evenementPartie,
            this.sitePartie,
            this.nombreDemiCoups});
            this.TableauPartiesPdn.Location = new System.Drawing.Point(0, 27);
            this.TableauPartiesPdn.Name = "TableauPartiesPdn";
            this.TableauPartiesPdn.Size = new System.Drawing.Size(801, 427);
            this.TableauPartiesPdn.TabIndex = 0;
            this.TableauPartiesPdn.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.TableauPartiesPdn_CellDoubleClick);
            // 
            // joueurBlanc
            // 
            this.joueurBlanc.HeaderText = "Blancs";
            this.joueurBlanc.Name = "joueurBlanc";
            // 
            // joueurNoir
            // 
            this.joueurNoir.HeaderText = "Noirs";
            this.joueurNoir.Name = "joueurNoir";
            // 
            // ResultatPartie
            // 
            this.ResultatPartie.HeaderText = "Résultat";
            this.ResultatPartie.Name = "ResultatPartie";
            this.ResultatPartie.Width = 80;
            // 
            // numeroRonde
            // 
            this.numeroRonde.HeaderText = "Ronde";
            this.numeroRonde.Name = "numeroRonde";
            this.numeroRonde.Width = 50;
            // 
            // datePartie
            // 
            this.datePartie.HeaderText = "Date";
            this.datePartie.Name = "datePartie";
            // 
            // evenementPartie
            // 
            this.evenementPartie.HeaderText = "Evènement";
            this.evenementPartie.Name = "evenementPartie";
            // 
            // sitePartie
            // 
            this.sitePartie.HeaderText = "Site";
            this.sitePartie.Name = "sitePartie";
            // 
            // nombreDemiCoups
            // 
            this.nombreDemiCoups.HeaderText = "Nombre 1/2 coups";
            this.nombreDemiCoups.Name = "nombreDemiCoups";
            this.nombreDemiCoups.Width = 120;
            // 
            // NombrePartiesFichier
            // 
            this.NombrePartiesFichier.BackColor = System.Drawing.Color.PaleGreen;
            this.NombrePartiesFichier.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NombrePartiesFichier.Location = new System.Drawing.Point(0, 1);
            this.NombrePartiesFichier.Name = "NombrePartiesFichier";
            this.NombrePartiesFichier.Size = new System.Drawing.Size(801, 23);
            this.NombrePartiesFichier.TabIndex = 1;
            this.NombrePartiesFichier.Text = "Nombre de parties dans le fichier";
            this.NombrePartiesFichier.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FichierPartiePdn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.NombrePartiesFichier);
            this.Controls.Add(this.TableauPartiesPdn);
            this.Name = "FichierPartiePdn";
            this.Text = "Fichier de Partie(s) Pdn";
            ((System.ComponentModel.ISupportInitialize)(this.TableauPartiesPdn)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.DataGridView TableauPartiesPdn;
        private System.Windows.Forms.DataGridViewTextBoxColumn joueurBlanc;
        private System.Windows.Forms.DataGridViewTextBoxColumn joueurNoir;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResultatPartie;
        private System.Windows.Forms.DataGridViewTextBoxColumn numeroRonde;
        private System.Windows.Forms.DataGridViewTextBoxColumn datePartie;
        private System.Windows.Forms.DataGridViewTextBoxColumn evenementPartie;
        private System.Windows.Forms.DataGridViewTextBoxColumn sitePartie;
        private System.Windows.Forms.DataGridViewTextBoxColumn nombreDemiCoups;
        public System.Windows.Forms.Label NombrePartiesFichier;
    }
}