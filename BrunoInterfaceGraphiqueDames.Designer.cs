namespace BrunoGUI_Dames
{
    partial class BrunoInterfaceGraphiqueDames
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrunoInterfaceGraphiqueDames));
            this.Damier10x10 = new System.Windows.Forms.PictureBox();
            this.BoutonQuitterApplication = new Krypton.Toolkit.KryptonButton();
            this.BoutonTournerDamier = new Krypton.Toolkit.KryptonButton();
            this.LabelCoupJoue = new System.Windows.Forms.Label();
            this.LabelPrisesPossibles = new System.Windows.Forms.Label();
            this.LabelPionsNoirs = new System.Windows.Forms.Label();
            this.LabelDamesBlanches = new System.Windows.Forms.Label();
            this.LabelDamesNoires = new System.Windows.Forms.Label();
            this.LabelPionsBlancs = new System.Windows.Forms.Label();
            this.BoutonRetourArriere = new Krypton.Toolkit.KryptonButton();
            this.MontreDonneesScan = new Krypton.Toolkit.KryptonButton();
            this.VisualisationPdn = new Krypton.Toolkit.KryptonButton();
            this.OrdinateurJoue = new Krypton.Toolkit.KryptonButton();
            this.LabelInformationJoueur = new System.Windows.Forms.Label();
            this.NouvellePartie = new Krypton.Toolkit.KryptonButton();
            this.LabelAfficheScan = new System.Windows.Forms.Label();
            this.LabelScore = new System.Windows.Forms.Label();
            this.BoxNomJoueurNoir = new System.Windows.Forms.RichTextBox();
            this.BoxNomJoueurBlanc = new System.Windows.Forms.RichTextBox();
            this.OrdinateurBosse = new System.Windows.Forms.CheckBox();
            this.SonEmis = new System.Windows.Forms.CheckBox();
            this.GroupResultat = new System.Windows.Forms.GroupBox();
            this.RadioNulle = new System.Windows.Forms.RadioButton();
            this.RadioGainNoir = new System.Windows.Forms.RadioButton();
            this.RadioGainBlanc = new System.Windows.Forms.RadioButton();
            this.ChargePositionFen = new Krypton.Toolkit.KryptonButton();
            this.Apropos = new Krypton.Toolkit.KryptonButton();
            this.ChargerFichierFen = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.trackBarTempsReflexion = new System.Windows.Forms.TrackBar();
            this.LabelTempsReflexion = new System.Windows.Forms.Label();
            this.SauverFichierFen = new System.Windows.Forms.SaveFileDialog();
            this.SauvePositionFen = new Krypton.Toolkit.KryptonButton();
            this.groupParcoursPartie = new System.Windows.Forms.GroupBox();
            this.BoutoonFin = new System.Windows.Forms.Button();
            this.BoutonDebut = new System.Windows.Forms.Button();
            this.BoutonSuivant = new System.Windows.Forms.Button();
            this.BoutonPrecedent = new System.Windows.Forms.Button();
            this.ChargePartiesPdn = new Krypton.Toolkit.KryptonButton();
            this.ChargerPartiesPdn = new System.Windows.Forms.OpenFileDialog();
            this.SauvePartiesPdn = new Krypton.Toolkit.KryptonButton();
            this.boutonMasqueAffiche = new Krypton.Toolkit.KryptonButton();
            this.AnalysePosition = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.Damier10x10)).BeginInit();
            this.GroupResultat.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTempsReflexion)).BeginInit();
            this.groupParcoursPartie.SuspendLayout();
            this.SuspendLayout();
            // 
            // Damier10x10
            // 
            this.Damier10x10.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Damier10x10.Image = ((System.Drawing.Image)(resources.GetObject("Damier10x10.Image")));
            this.Damier10x10.Location = new System.Drawing.Point(7, 60);
            this.Damier10x10.Name = "Damier10x10";
            this.Damier10x10.Size = new System.Drawing.Size(640, 640);
            this.Damier10x10.TabIndex = 0;
            this.Damier10x10.TabStop = false;
            // 
            // BoutonQuitterApplication
            // 
            this.BoutonQuitterApplication.Location = new System.Drawing.Point(653, 670);
            this.BoutonQuitterApplication.Name = "BoutonQuitterApplication";
            this.BoutonQuitterApplication.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.BoutonQuitterApplication.Size = new System.Drawing.Size(110, 30);
            this.BoutonQuitterApplication.StateCommon.Border.Rounding = 20F;
            this.BoutonQuitterApplication.StateCommon.Border.Width = 1;
            this.BoutonQuitterApplication.TabIndex = 1;
            this.BoutonQuitterApplication.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.BoutonQuitterApplication.Values.Text = "Quitter";
            this.BoutonQuitterApplication.Click += new System.EventHandler(this.BoutonQuitterApplication_Click);
            // 
            // BoutonTournerDamier
            // 
            this.BoutonTournerDamier.Location = new System.Drawing.Point(653, 168);
            this.BoutonTournerDamier.Name = "BoutonTournerDamier";
            this.BoutonTournerDamier.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.BoutonTournerDamier.Size = new System.Drawing.Size(110, 30);
            this.BoutonTournerDamier.StateCommon.Border.Rounding = 20F;
            this.BoutonTournerDamier.StateCommon.Border.Width = 1;
            this.BoutonTournerDamier.TabIndex = 2;
            this.BoutonTournerDamier.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.BoutonTournerDamier.Values.Text = "Tourner damier";
            this.BoutonTournerDamier.Click += new System.EventHandler(this.BoutonTournerDamier_Click);
            // 
            // LabelCoupJoue
            // 
            this.LabelCoupJoue.BackColor = System.Drawing.Color.Goldenrod;
            this.LabelCoupJoue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelCoupJoue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelCoupJoue.Location = new System.Drawing.Point(7, 703);
            this.LabelCoupJoue.Name = "LabelCoupJoue";
            this.LabelCoupJoue.Size = new System.Drawing.Size(354, 23);
            this.LabelCoupJoue.TabIndex = 4;
            this.LabelCoupJoue.Text = "Mouvement : ";
            this.LabelCoupJoue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelPrisesPossibles
            // 
            this.LabelPrisesPossibles.BackColor = System.Drawing.Color.NavajoWhite;
            this.LabelPrisesPossibles.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelPrisesPossibles.Location = new System.Drawing.Point(7, 726);
            this.LabelPrisesPossibles.Name = "LabelPrisesPossibles";
            this.LabelPrisesPossibles.Size = new System.Drawing.Size(354, 55);
            this.LabelPrisesPossibles.TabIndex = 6;
            this.LabelPrisesPossibles.Text = "Prises possibles";
            this.LabelPrisesPossibles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelPionsNoirs
            // 
            this.LabelPionsNoirs.BackColor = System.Drawing.Color.NavajoWhite;
            this.LabelPionsNoirs.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelPionsNoirs.Location = new System.Drawing.Point(367, 721);
            this.LabelPionsNoirs.Name = "LabelPionsNoirs";
            this.LabelPionsNoirs.Size = new System.Drawing.Size(83, 18);
            this.LabelPionsNoirs.TabIndex = 10;
            this.LabelPionsNoirs.Text = "Label Pions Noirs";
            this.LabelPionsNoirs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelDamesBlanches
            // 
            this.LabelDamesBlanches.BackColor = System.Drawing.Color.NavajoWhite;
            this.LabelDamesBlanches.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelDamesBlanches.Location = new System.Drawing.Point(447, 703);
            this.LabelDamesBlanches.Name = "LabelDamesBlanches";
            this.LabelDamesBlanches.Size = new System.Drawing.Size(108, 18);
            this.LabelDamesBlanches.TabIndex = 11;
            this.LabelDamesBlanches.Text = "Label Dames Blanches";
            this.LabelDamesBlanches.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelDamesNoires
            // 
            this.LabelDamesNoires.BackColor = System.Drawing.Color.NavajoWhite;
            this.LabelDamesNoires.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelDamesNoires.Location = new System.Drawing.Point(447, 721);
            this.LabelDamesNoires.Name = "LabelDamesNoires";
            this.LabelDamesNoires.Size = new System.Drawing.Size(108, 18);
            this.LabelDamesNoires.TabIndex = 12;
            this.LabelDamesNoires.Text = "Label Dames Noires";
            this.LabelDamesNoires.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelPionsBlancs
            // 
            this.LabelPionsBlancs.BackColor = System.Drawing.Color.NavajoWhite;
            this.LabelPionsBlancs.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelPionsBlancs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelPionsBlancs.Location = new System.Drawing.Point(367, 703);
            this.LabelPionsBlancs.Name = "LabelPionsBlancs";
            this.LabelPionsBlancs.Size = new System.Drawing.Size(83, 18);
            this.LabelPionsBlancs.TabIndex = 13;
            this.LabelPionsBlancs.Text = "Label Pions Blancs";
            this.LabelPionsBlancs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BoutonRetourArriere
            // 
            this.BoutonRetourArriere.Location = new System.Drawing.Point(653, 132);
            this.BoutonRetourArriere.Name = "BoutonRetourArriere";
            this.BoutonRetourArriere.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.BoutonRetourArriere.Size = new System.Drawing.Size(110, 30);
            this.BoutonRetourArriere.StateCommon.Border.Rounding = 20F;
            this.BoutonRetourArriere.StateCommon.Border.Width = 1;
            this.BoutonRetourArriere.TabIndex = 15;
            this.BoutonRetourArriere.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.BoutonRetourArriere.Values.Text = "Retour Arrière";
            this.BoutonRetourArriere.Click += new System.EventHandler(this.BoutonRetourArriere_Click);
            // 
            // MontreDonneesScan
            // 
            this.MontreDonneesScan.Location = new System.Drawing.Point(653, 598);
            this.MontreDonneesScan.Name = "MontreDonneesScan";
            this.MontreDonneesScan.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.MontreDonneesScan.Size = new System.Drawing.Size(110, 30);
            this.MontreDonneesScan.StateCommon.Border.Rounding = 20F;
            this.MontreDonneesScan.StateCommon.Border.Width = 1;
            this.MontreDonneesScan.TabIndex = 16;
            this.MontreDonneesScan.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.MontreDonneesScan.Values.Text = "Protocole";
            this.MontreDonneesScan.Click += new System.EventHandler(this.MontreDonneesScan_Click);
            // 
            // VisualisationPdn
            // 
            this.VisualisationPdn.Location = new System.Drawing.Point(653, 240);
            this.VisualisationPdn.Name = "VisualisationPdn";
            this.VisualisationPdn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.VisualisationPdn.Size = new System.Drawing.Size(110, 30);
            this.VisualisationPdn.StateCommon.Border.Rounding = 20F;
            this.VisualisationPdn.StateCommon.Border.Width = 1;
            this.VisualisationPdn.TabIndex = 17;
            this.VisualisationPdn.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.VisualisationPdn.Values.Text = "Liste des coups";
            this.VisualisationPdn.Click += new System.EventHandler(this.VisualisationPdn_Click);
            // 
            // OrdinateurJoue
            // 
            this.OrdinateurJoue.Location = new System.Drawing.Point(653, 96);
            this.OrdinateurJoue.Name = "OrdinateurJoue";
            this.OrdinateurJoue.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.OrdinateurJoue.Size = new System.Drawing.Size(110, 30);
            this.OrdinateurJoue.StateCommon.Border.Rounding = 20F;
            this.OrdinateurJoue.StateCommon.Border.Width = 1;
            this.OrdinateurJoue.TabIndex = 18;
            this.OrdinateurJoue.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.OrdinateurJoue.Values.Text = "Ordinateur joue";
            this.OrdinateurJoue.Click += new System.EventHandler(this.OrdinateurJoue_Click);
            // 
            // LabelInformationJoueur
            // 
            this.LabelInformationJoueur.BackColor = System.Drawing.Color.Goldenrod;
            this.LabelInformationJoueur.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelInformationJoueur.Location = new System.Drawing.Point(143, 5);
            this.LabelInformationJoueur.Name = "LabelInformationJoueur";
            this.LabelInformationJoueur.Size = new System.Drawing.Size(346, 25);
            this.LabelInformationJoueur.TabIndex = 19;
            this.LabelInformationJoueur.Text = "Label Information Joueur";
            this.LabelInformationJoueur.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // NouvellePartie
            // 
            this.NouvellePartie.Location = new System.Drawing.Point(653, 60);
            this.NouvellePartie.Name = "NouvellePartie";
            this.NouvellePartie.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.NouvellePartie.Size = new System.Drawing.Size(110, 30);
            this.NouvellePartie.StateCommon.Border.Rounding = 20F;
            this.NouvellePartie.StateCommon.Border.Width = 1;
            this.NouvellePartie.TabIndex = 20;
            this.NouvellePartie.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.NouvellePartie.Values.Text = "Nouvelle Partie";
            this.NouvellePartie.Click += new System.EventHandler(this.NouvellePartie_Click);
            // 
            // LabelAfficheScan
            // 
            this.LabelAfficheScan.BackColor = System.Drawing.Color.LightSteelBlue;
            this.LabelAfficheScan.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelAfficheScan.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelAfficheScan.ForeColor = System.Drawing.Color.DarkBlue;
            this.LabelAfficheScan.Location = new System.Drawing.Point(133, 35);
            this.LabelAfficheScan.Name = "LabelAfficheScan";
            this.LabelAfficheScan.Size = new System.Drawing.Size(514, 20);
            this.LabelAfficheScan.TabIndex = 21;
            this.LabelAfficheScan.Text = "Label Affiche Scan";
            // 
            // LabelScore
            // 
            this.LabelScore.BackColor = System.Drawing.Color.LightSteelBlue;
            this.LabelScore.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelScore.ForeColor = System.Drawing.Color.DarkBlue;
            this.LabelScore.Location = new System.Drawing.Point(10, 35);
            this.LabelScore.Name = "LabelScore";
            this.LabelScore.Size = new System.Drawing.Size(120, 20);
            this.LabelScore.TabIndex = 26;
            this.LabelScore.Text = "Score";
            // 
            // BoxNomJoueurNoir
            // 
            this.BoxNomJoueurNoir.BackColor = System.Drawing.Color.Black;
            this.BoxNomJoueurNoir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BoxNomJoueurNoir.ForeColor = System.Drawing.Color.White;
            this.BoxNomJoueurNoir.Location = new System.Drawing.Point(495, 5);
            this.BoxNomJoueurNoir.Multiline = false;
            this.BoxNomJoueurNoir.Name = "BoxNomJoueurNoir";
            this.BoxNomJoueurNoir.Size = new System.Drawing.Size(152, 25);
            this.BoxNomJoueurNoir.TabIndex = 28;
            this.BoxNomJoueurNoir.Text = "Joueur noir";
            this.BoxNomJoueurNoir.TextChanged += new System.EventHandler(this.BoxNomJoueurNoir_TextChanged);
            // 
            // BoxNomJoueurBlanc
            // 
            this.BoxNomJoueurBlanc.BackColor = System.Drawing.Color.White;
            this.BoxNomJoueurBlanc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BoxNomJoueurBlanc.ForeColor = System.Drawing.Color.Black;
            this.BoxNomJoueurBlanc.Location = new System.Drawing.Point(7, 5);
            this.BoxNomJoueurBlanc.Multiline = false;
            this.BoxNomJoueurBlanc.Name = "BoxNomJoueurBlanc";
            this.BoxNomJoueurBlanc.Size = new System.Drawing.Size(130, 25);
            this.BoxNomJoueurBlanc.TabIndex = 29;
            this.BoxNomJoueurBlanc.Text = "Joueur blanc";
            this.BoxNomJoueurBlanc.TextChanged += new System.EventHandler(this.BoxNomJoueurBlanc_TextChanged);
            // 
            // OrdinateurBosse
            // 
            this.OrdinateurBosse.BackColor = System.Drawing.Color.NavajoWhite;
            this.OrdinateurBosse.Checked = true;
            this.OrdinateurBosse.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OrdinateurBosse.Location = new System.Drawing.Point(561, 703);
            this.OrdinateurBosse.Name = "OrdinateurBosse";
            this.OrdinateurBosse.Size = new System.Drawing.Size(85, 18);
            this.OrdinateurBosse.TabIndex = 30;
            this.OrdinateurBosse.Text = "Ordinateur";
            this.OrdinateurBosse.UseVisualStyleBackColor = false;
            this.OrdinateurBosse.CheckedChanged += new System.EventHandler(this.OrdinateurBosse_CheckedChanged);
            // 
            // SonEmis
            // 
            this.SonEmis.BackColor = System.Drawing.Color.NavajoWhite;
            this.SonEmis.Location = new System.Drawing.Point(561, 722);
            this.SonEmis.Name = "SonEmis";
            this.SonEmis.Size = new System.Drawing.Size(86, 18);
            this.SonEmis.TabIndex = 31;
            this.SonEmis.Text = "Son";
            this.SonEmis.UseVisualStyleBackColor = false;
            this.SonEmis.CheckedChanged += new System.EventHandler(this.SonEmis_CheckedChanged);
            // 
            // GroupResultat
            // 
            this.GroupResultat.BackColor = System.Drawing.Color.NavajoWhite;
            this.GroupResultat.Controls.Add(this.RadioNulle);
            this.GroupResultat.Controls.Add(this.RadioGainNoir);
            this.GroupResultat.Controls.Add(this.RadioGainBlanc);
            this.GroupResultat.Location = new System.Drawing.Point(658, 703);
            this.GroupResultat.Name = "GroupResultat";
            this.GroupResultat.Size = new System.Drawing.Size(87, 80);
            this.GroupResultat.TabIndex = 32;
            this.GroupResultat.TabStop = false;
            this.GroupResultat.Text = "Résultat";
            // 
            // RadioNulle
            // 
            this.RadioNulle.AutoSize = true;
            this.RadioNulle.Location = new System.Drawing.Point(6, 57);
            this.RadioNulle.Name = "RadioNulle";
            this.RadioNulle.Size = new System.Drawing.Size(62, 17);
            this.RadioNulle.TabIndex = 2;
            this.RadioNulle.TabStop = true;
            this.RadioNulle.Text = "1/2-1/2";
            this.RadioNulle.UseVisualStyleBackColor = true;
            this.RadioNulle.CheckedChanged += new System.EventHandler(this.RadioNulle_CheckedChanged);
            // 
            // RadioGainNoir
            // 
            this.RadioGainNoir.AutoSize = true;
            this.RadioGainNoir.Location = new System.Drawing.Point(6, 36);
            this.RadioGainNoir.Name = "RadioGainNoir";
            this.RadioGainNoir.Size = new System.Drawing.Size(40, 17);
            this.RadioGainNoir.TabIndex = 1;
            this.RadioGainNoir.TabStop = true;
            this.RadioGainNoir.Text = "0-1";
            this.RadioGainNoir.UseVisualStyleBackColor = true;
            this.RadioGainNoir.CheckedChanged += new System.EventHandler(this.RadioGainNoir_CheckedChanged);
            // 
            // RadioGainBlanc
            // 
            this.RadioGainBlanc.AutoSize = true;
            this.RadioGainBlanc.Location = new System.Drawing.Point(7, 17);
            this.RadioGainBlanc.Name = "RadioGainBlanc";
            this.RadioGainBlanc.Size = new System.Drawing.Size(40, 17);
            this.RadioGainBlanc.TabIndex = 0;
            this.RadioGainBlanc.TabStop = true;
            this.RadioGainBlanc.Text = "1-0";
            this.RadioGainBlanc.UseVisualStyleBackColor = true;
            this.RadioGainBlanc.CheckedChanged += new System.EventHandler(this.RadioGainBlanc_CheckedChanged);
            // 
            // ChargePositionFen
            // 
            this.ChargePositionFen.Location = new System.Drawing.Point(653, 483);
            this.ChargePositionFen.Name = "ChargePositionFen";
            this.ChargePositionFen.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.ChargePositionFen.Size = new System.Drawing.Size(110, 30);
            this.ChargePositionFen.StateCommon.Border.Rounding = 20F;
            this.ChargePositionFen.StateCommon.Border.Width = 1;
            this.ChargePositionFen.TabIndex = 33;
            this.ChargePositionFen.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.ChargePositionFen.Values.Text = "Charge position";
            this.ChargePositionFen.Click += new System.EventHandler(this.PositionFen_Click);
            // 
            // Apropos
            // 
            this.Apropos.Location = new System.Drawing.Point(653, 634);
            this.Apropos.Name = "Apropos";
            this.Apropos.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.Apropos.Size = new System.Drawing.Size(110, 30);
            this.Apropos.StateCommon.Border.Rounding = 20F;
            this.Apropos.StateCommon.Border.Width = 1;
            this.Apropos.TabIndex = 34;
            this.Apropos.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.Apropos.Values.Text = "A propos de BIG";
            this.Apropos.Click += new System.EventHandler(this.Apropos_Click);
            // 
            // ChargerFichierFen
            // 
            this.ChargerFichierFen.FileName = "openFileDialog1";
            this.ChargerFichierFen.Filter = "FEN Files (*.fen)|*.fen|Tous les fichiers (*.*)|*.*";
            this.ChargerFichierFen.Title = "Sélectionner un fichier FEN";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Goldenrod;
            this.groupBox1.Controls.Add(this.trackBarTempsReflexion);
            this.groupBox1.Controls.Add(this.LabelTempsReflexion);
            this.groupBox1.Location = new System.Drawing.Point(367, 743);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(280, 40);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Temps de réflexion SCAN 3.1 (en secondes)";
            // 
            // trackBarTempsReflexion
            // 
            this.trackBarTempsReflexion.AutoSize = false;
            this.trackBarTempsReflexion.Location = new System.Drawing.Point(6, 13);
            this.trackBarTempsReflexion.Maximum = 25;
            this.trackBarTempsReflexion.Minimum = 1;
            this.trackBarTempsReflexion.Name = "trackBarTempsReflexion";
            this.trackBarTempsReflexion.Size = new System.Drawing.Size(198, 21);
            this.trackBarTempsReflexion.TabIndex = 1;
            this.trackBarTempsReflexion.Value = 5;
            this.trackBarTempsReflexion.ValueChanged += new System.EventHandler(this.trackBarTempsReflexion_ValueChanged);
            // 
            // LabelTempsReflexion
            // 
            this.LabelTempsReflexion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTempsReflexion.Location = new System.Drawing.Point(204, 13);
            this.LabelTempsReflexion.Name = "LabelTempsReflexion";
            this.LabelTempsReflexion.Size = new System.Drawing.Size(56, 21);
            this.LabelTempsReflexion.TabIndex = 0;
            this.LabelTempsReflexion.Text = "Sec.";
            // 
            // SauverFichierFen
            // 
            this.SauverFichierFen.Filter = "FEN Files (*.fen)|*.fen|Tous les fichiers (*.*)|*.*";
            // 
            // SauvePositionFen
            // 
            this.SauvePositionFen.Location = new System.Drawing.Point(653, 519);
            this.SauvePositionFen.Name = "SauvePositionFen";
            this.SauvePositionFen.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.SauvePositionFen.Size = new System.Drawing.Size(110, 30);
            this.SauvePositionFen.StateCommon.Border.Rounding = 20F;
            this.SauvePositionFen.StateCommon.Border.Width = 1;
            this.SauvePositionFen.TabIndex = 36;
            this.SauvePositionFen.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.SauvePositionFen.Values.Text = "Sauve position";
            this.SauvePositionFen.Click += new System.EventHandler(this.SauvePositionFen_Click);
            // 
            // groupParcoursPartie
            // 
            this.groupParcoursPartie.BackColor = System.Drawing.Color.Goldenrod;
            this.groupParcoursPartie.Controls.Add(this.BoutoonFin);
            this.groupParcoursPartie.Controls.Add(this.BoutonDebut);
            this.groupParcoursPartie.Controls.Add(this.BoutonSuivant);
            this.groupParcoursPartie.Controls.Add(this.BoutonPrecedent);
            this.groupParcoursPartie.Location = new System.Drawing.Point(653, 277);
            this.groupParcoursPartie.Name = "groupParcoursPartie";
            this.groupParcoursPartie.Size = new System.Drawing.Size(110, 68);
            this.groupParcoursPartie.TabIndex = 37;
            this.groupParcoursPartie.TabStop = false;
            this.groupParcoursPartie.Text = "   Parcours partie   ";
            // 
            // BoutoonFin
            // 
            this.BoutoonFin.BackColor = System.Drawing.Color.Tan;
            this.BoutoonFin.Location = new System.Drawing.Point(53, 38);
            this.BoutoonFin.Name = "BoutoonFin";
            this.BoutoonFin.Size = new System.Drawing.Size(51, 24);
            this.BoutoonFin.TabIndex = 3;
            this.BoutoonFin.Text = "Fin";
            this.BoutoonFin.UseVisualStyleBackColor = false;
            this.BoutoonFin.Click += new System.EventHandler(this.BoutoonFin_Click);
            // 
            // BoutonDebut
            // 
            this.BoutonDebut.BackColor = System.Drawing.Color.Tan;
            this.BoutonDebut.Location = new System.Drawing.Point(5, 38);
            this.BoutonDebut.Name = "BoutonDebut";
            this.BoutonDebut.Size = new System.Drawing.Size(47, 24);
            this.BoutonDebut.TabIndex = 2;
            this.BoutonDebut.Text = "Début";
            this.BoutonDebut.UseVisualStyleBackColor = false;
            this.BoutonDebut.Click += new System.EventHandler(this.BoutonDebut_Click);
            // 
            // BoutonSuivant
            // 
            this.BoutonSuivant.BackColor = System.Drawing.Color.Tan;
            this.BoutonSuivant.Location = new System.Drawing.Point(53, 14);
            this.BoutonSuivant.Name = "BoutonSuivant";
            this.BoutonSuivant.Size = new System.Drawing.Size(51, 24);
            this.BoutonSuivant.TabIndex = 1;
            this.BoutonSuivant.Text = "Suiv.";
            this.BoutonSuivant.UseVisualStyleBackColor = false;
            this.BoutonSuivant.Click += new System.EventHandler(this.BoutonSuivant_Click);
            // 
            // BoutonPrecedent
            // 
            this.BoutonPrecedent.BackColor = System.Drawing.Color.Tan;
            this.BoutonPrecedent.Location = new System.Drawing.Point(5, 14);
            this.BoutonPrecedent.Name = "BoutonPrecedent";
            this.BoutonPrecedent.Size = new System.Drawing.Size(47, 24);
            this.BoutonPrecedent.TabIndex = 0;
            this.BoutonPrecedent.Text = "Préc.";
            this.BoutonPrecedent.UseVisualStyleBackColor = false;
            this.BoutonPrecedent.Click += new System.EventHandler(this.BoutonPrecedent_Click);
            // 
            // ChargePartiesPdn
            // 
            this.ChargePartiesPdn.Location = new System.Drawing.Point(653, 375);
            this.ChargePartiesPdn.Name = "ChargePartiesPdn";
            this.ChargePartiesPdn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.ChargePartiesPdn.Size = new System.Drawing.Size(110, 30);
            this.ChargePartiesPdn.StateCommon.Border.Rounding = 20F;
            this.ChargePartiesPdn.StateCommon.Border.Width = 1;
            this.ChargePartiesPdn.TabIndex = 38;
            this.ChargePartiesPdn.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.ChargePartiesPdn.Values.Text = "Charge Parties";
            this.ChargePartiesPdn.Click += new System.EventHandler(this.ChargePartiesPdn_Click);
            // 
            // ChargerPartiesPdn
            // 
            this.ChargerPartiesPdn.FileName = "openFileDialog1";
            this.ChargerPartiesPdn.Filter = "PDN Files (*.pdn)|*.pdn|Tous les fichiers (*.*)|*.*";
            // 
            // SauvePartiesPdn
            // 
            this.SauvePartiesPdn.Location = new System.Drawing.Point(653, 447);
            this.SauvePartiesPdn.Name = "SauvePartiesPdn";
            this.SauvePartiesPdn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.SauvePartiesPdn.Size = new System.Drawing.Size(110, 30);
            this.SauvePartiesPdn.StateCommon.Border.Rounding = 20F;
            this.SauvePartiesPdn.StateCommon.Border.Width = 1;
            this.SauvePartiesPdn.TabIndex = 39;
            this.SauvePartiesPdn.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.SauvePartiesPdn.Values.Text = "Sauve Partie";
            this.SauvePartiesPdn.Click += new System.EventHandler(this.SauvePartiesPdn_Click);
            // 
            // boutonMasqueAffiche
            // 
            this.boutonMasqueAffiche.Location = new System.Drawing.Point(653, 411);
            this.boutonMasqueAffiche.Name = "boutonMasqueAffiche";
            this.boutonMasqueAffiche.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.boutonMasqueAffiche.Size = new System.Drawing.Size(110, 30);
            this.boutonMasqueAffiche.StateCommon.Border.Rounding = 20F;
            this.boutonMasqueAffiche.StateCommon.Border.Width = 1;
            this.boutonMasqueAffiche.TabIndex = 40;
            this.boutonMasqueAffiche.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.boutonMasqueAffiche.Values.Text = "Masque parties";
            this.boutonMasqueAffiche.Click += new System.EventHandler(this.boutonMasqueAffiche_Click);
            // 
            // AnalysePosition
            // 
            this.AnalysePosition.Location = new System.Drawing.Point(653, 204);
            this.AnalysePosition.Name = "AnalysePosition";
            this.AnalysePosition.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.AnalysePosition.Size = new System.Drawing.Size(110, 30);
            this.AnalysePosition.StateCommon.Border.Rounding = 20F;
            this.AnalysePosition.StateCommon.Border.Width = 1;
            this.AnalysePosition.TabIndex = 41;
            this.AnalysePosition.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.AnalysePosition.Values.Text = "Analyse position";
            this.AnalysePosition.Click += new System.EventHandler(this.AnalysePosition_Click);
            // 
            // BrunoInterfaceGraphiqueDames
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(770, 786);
            this.Controls.Add(this.AnalysePosition);
            this.Controls.Add(this.boutonMasqueAffiche);
            this.Controls.Add(this.SauvePartiesPdn);
            this.Controls.Add(this.ChargePartiesPdn);
            this.Controls.Add(this.groupParcoursPartie);
            this.Controls.Add(this.SauvePositionFen);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Apropos);
            this.Controls.Add(this.ChargePositionFen);
            this.Controls.Add(this.GroupResultat);
            this.Controls.Add(this.SonEmis);
            this.Controls.Add(this.OrdinateurBosse);
            this.Controls.Add(this.BoxNomJoueurBlanc);
            this.Controls.Add(this.BoxNomJoueurNoir);
            this.Controls.Add(this.LabelScore);
            this.Controls.Add(this.LabelAfficheScan);
            this.Controls.Add(this.NouvellePartie);
            this.Controls.Add(this.LabelInformationJoueur);
            this.Controls.Add(this.OrdinateurJoue);
            this.Controls.Add(this.VisualisationPdn);
            this.Controls.Add(this.MontreDonneesScan);
            this.Controls.Add(this.BoutonRetourArriere);
            this.Controls.Add(this.LabelPionsBlancs);
            this.Controls.Add(this.LabelDamesNoires);
            this.Controls.Add(this.LabelDamesBlanches);
            this.Controls.Add(this.LabelPionsNoirs);
            this.Controls.Add(this.LabelPrisesPossibles);
            this.Controls.Add(this.LabelCoupJoue);
            this.Controls.Add(this.BoutonTournerDamier);
            this.Controls.Add(this.BoutonQuitterApplication);
            this.Controls.Add(this.Damier10x10);
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "BrunoInterfaceGraphiqueDames";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " Bruno Interface Graphique";
            this.Load += new System.EventHandler(this.BrunoInterfaceGraphiqueDames_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CaseMouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.Damier10x10)).EndInit();
            this.GroupResultat.ResumeLayout(false);
            this.GroupResultat.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTempsReflexion)).EndInit();
            this.groupParcoursPartie.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox Damier10x10;
        private Krypton.Toolkit.KryptonButton BoutonQuitterApplication;
        private Krypton.Toolkit.KryptonButton BoutonTournerDamier;
        private System.Windows.Forms.Label LabelCoupJoue;
        private System.Windows.Forms.Label LabelPrisesPossibles;
        private System.Windows.Forms.Label LabelPionsNoirs;
        private System.Windows.Forms.Label LabelDamesBlanches;
        private System.Windows.Forms.Label LabelDamesNoires;
        public System.Windows.Forms.Label LabelPionsBlancs;
        private Krypton.Toolkit.KryptonButton BoutonRetourArriere;
        private Krypton.Toolkit.KryptonButton MontreDonneesScan;
        private Krypton.Toolkit.KryptonButton VisualisationPdn;
        private Krypton.Toolkit.KryptonButton OrdinateurJoue;
        private Krypton.Toolkit.KryptonButton NouvellePartie;
        private System.Windows.Forms.Label LabelAfficheScan;
        private System.Windows.Forms.Label LabelScore;
        public System.Windows.Forms.Label LabelInformationJoueur;
        private System.Windows.Forms.RichTextBox BoxNomJoueurNoir;
        private System.Windows.Forms.RichTextBox BoxNomJoueurBlanc;
        private System.Windows.Forms.CheckBox OrdinateurBosse;
        private System.Windows.Forms.CheckBox SonEmis;
        private System.Windows.Forms.GroupBox GroupResultat;
        private System.Windows.Forms.RadioButton RadioNulle;
        private System.Windows.Forms.RadioButton RadioGainNoir;
        private System.Windows.Forms.RadioButton RadioGainBlanc;
        private Krypton.Toolkit.KryptonButton ChargePositionFen;
        private Krypton.Toolkit.KryptonButton Apropos;
        private System.Windows.Forms.OpenFileDialog ChargerFichierFen;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TrackBar trackBarTempsReflexion;
        private System.Windows.Forms.Label LabelTempsReflexion;
        private System.Windows.Forms.SaveFileDialog SauverFichierFen;
        private Krypton.Toolkit.KryptonButton SauvePositionFen;
        private System.Windows.Forms.GroupBox groupParcoursPartie;
        private System.Windows.Forms.Button BoutonPrecedent;
        private System.Windows.Forms.Button BoutonSuivant;
        private System.Windows.Forms.Button BoutoonFin;
        private System.Windows.Forms.Button BoutonDebut;
        private Krypton.Toolkit.KryptonButton ChargePartiesPdn;
        private System.Windows.Forms.OpenFileDialog ChargerPartiesPdn;
        private Krypton.Toolkit.KryptonButton SauvePartiesPdn;
        private Krypton.Toolkit.KryptonButton boutonMasqueAffiche;
        private Krypton.Toolkit.KryptonButton AnalysePosition;
    }
}

