// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames est développé par Bruno COURTOIS.  Copyright © 2024/2025  █  
// █ BrunoGUI_Dames est gratuit, sauf s'il est utilisé commercialement        █
// █ Utilisation du moteur SCAN 3.1 de Fabien Letouzey                        █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    public partial class AffichePdn : Form
    {
        private readonly AffichePdn afficheZone;
        private RichTextBox ZoneAffichage;
        private Krypton.Toolkit.KryptonButton MasqueAffichePdn;
        private Krypton.Toolkit.KryptonButton AffichePdnFr;
        private Krypton.Toolkit.KryptonButton AfficheFen;
        private Krypton.Toolkit.KryptonButton EnregistrerPdn;
        private Krypton.Toolkit.KryptonButton AfficheListeHub2;
        private SaveFileDialog EnregistrerFichierPdn;

        public AffichePdn()
        {
            InitializeComponent();
            afficheZone = this;
        }
        public string GenerationPdn()
        {   // --- Génération de la partie au format Pdn (Portable Draughts Notation) ---        https://pdn.fmjd.org/index.html
            if (LogiqueMouvementsDames.ListeCoupsPdn.Count == 0)    // Vérifier si aucun coup n'a été joué pour éviter une erreur
            {
                return "[Event \"Entrainement\"]\n[Site \"Maison\"]\n[Date \"" + DateTime.Now.ToString("yyyy.MM.dd") +
                       "\"]\n[Round \"1\"]\n[White \"" + BrunoInterfaceGraphiqueDames.NomJoueurBlanc +
                       "\"]\n[Black \"" + BrunoInterfaceGraphiqueDames.NomJoueurNoir + "\"]\n[Result \"*\"]\n\n";
            }
            string contenuPdn = "[Event \"Entrainement\"]\n" + "[Site \"Maison\"]\n" + "[Date \"" + DateTime.Now.ToString("yyyy.MM.dd") + "\"]\n";
            contenuPdn = contenuPdn + "[Round \"1\"]\n" + "[White \"" + BrunoInterfaceGraphiqueDames.NomJoueurBlanc + 
                                        "\"]\n" + "[Black \"" + BrunoInterfaceGraphiqueDames.NomJoueurNoir + "\"]\n";
            string dernierCoup = LogiqueMouvementsDames.ListeCoupsPdn[LogiqueMouvementsDames.ListeCoupsPdn.Count - 1];
            if (dernierCoup == " 1-0" || dernierCoup == " 0-1" || dernierCoup == " 1/2-1/2" || dernierCoup == " 2-0" || dernierCoup == " 0-2" || dernierCoup == " 1-1" || dernierCoup == " *")
            {
                contenuPdn = contenuPdn + "[Result \"" + dernierCoup + "\"]\n\n";
            }
            else
            {
                contenuPdn = contenuPdn + "[Result \"*\"]\n";
            }
            // Ajout de la balise PlyCount (nombre de demi-coups)
            contenuPdn += "[PlyCount \"" + LogiqueMouvementsDames.ListeCoupsPdn.Count + "\"]\n\n";
            // Fin des balises, début de la liste des coups
            int coupsParLigne = 5;      // Nombre de paires (blanc/noir) par ligne
            string ligneEnCours = "";   // Ligne temporaire pour construire la sortie
            int numeroCoup = 1;
            for (int i = 0; i < LogiqueMouvementsDames.ListeCoupsPdn.Count; i++)
            {   // Ajouter le numéro du coup uniquement pour les coups blancs (index pair)
                string coupActuel = LogiqueMouvementsDames.ListeCoupsPdn[i];        
                if (i % 2 == 0 && !(coupActuel == " 1-0" || coupActuel == " 0-1" || coupActuel == " 1/2-1/2"))
                {   // Ajoute le numéro du coup si c'est un coup blanc et non un résultat
                    ligneEnCours += $" {numeroCoup}. ";
                }
                ligneEnCours += LogiqueMouvementsDames.ListeCoupsPdn[i];                // Ajouter le coup (blanc ou noir)
                // Ajouter un espace après chaque coup sauf le dernier de la ligne
                if (i % 2 == 0 || i == LogiqueMouvementsDames.ListeCoupsPdn.Count - 1)
                {
                    ligneEnCours += " ";
                }
                if (i % 2 == 1)                 // Incrémenter le numéro de coup après chaque coup noir
                {
                    numeroCoup++;
                }
                // Ajouter la ligne au contenu quand on atteint le nombre maximal de paires par ligne
                if (numeroCoup > 1 && (numeroCoup - 1) % coupsParLigne == 0 && i % 2 == 1)
                {
                    contenuPdn += ligneEnCours.TrimEnd() + Environment.NewLine;
                    ligneEnCours = ""; // Réinitialiser la ligne
                }
            }
            if (ligneEnCours.Length > 0)        // Ajouter la dernière ligne s'il reste des coups
            {
                contenuPdn += ligneEnCours.TrimEnd();
            }
            return contenuPdn;
        }
        public void AffichePdnDansZone()
        {
            afficheZone.Show();
            string contenuPdn = GenerationPdn();
            // Afficher le résultat final (et je veux voir le contenu dans la console)
            Console.WriteLine(contenuPdn);
            afficheZone.ZoneAffichage.Text = contenuPdn;
        }
        private void AffichePdnFr_Click(object sender, EventArgs e)
        {
            afficheZone.Show();
            AffichePdnDansZone();
        }
        private void ListeFenAffichePdn_Click(object sender, EventArgs e)
        {
            afficheZone.Show();
            string contenuFen = "Nombre de 1/2 coups = " + LogiqueMouvementsDames.ListeCoupsFen.Count + "\n";
            for (int i = 0; i < LogiqueMouvementsDames.ListeCoupsFen.Count; i++)     // Parcours de la liste des FEN
            {
                contenuFen = contenuFen + "  [" + i + "]: \"" + LogiqueMouvementsDames.ListeCoupsFen[i] + "\"" + " \n";
            }
            afficheZone.ZoneAffichage.Text = contenuFen;
        }
        private void ListeHub2AffichePdn_Click(object sender, EventArgs e)
        {
            afficheZone.Show();
            string contenuHub2 = "Nombre de 1/2 coups = " + LogiqueMouvementsDames.ListeCoupsHub2.Count + "\n";
            for (int i = 0; i < LogiqueMouvementsDames.ListeCoupsHub2.Count; i++)     // Parcours de la liste des FEN
            {
                contenuHub2 = contenuHub2 + "  [" + i + "]: \"" + LogiqueMouvementsDames.ListeCoupsHub2[i] + "\"" + " \n";
            }
            afficheZone.ZoneAffichage.Text = contenuHub2;
        }
        private void MasqueAffichePdn_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        public void LancerEnregistrementPdn()
        {
            EnregistrerPdn_Click(this, EventArgs.Empty);
        }
        private void EnregistrerPdn_Click(object sender, EventArgs e)
        {
            try
            {
                string contenuPdn = GenerationPdn();
                // Ecriture du fichier PSN (Partie complète + en-tête)
                {
                    EnregistrerFichierPdn.OverwritePrompt = false;      // Permet d'éviter l'affichage de 2 boites de dialogue si le fichier choisi existe...
                    DialogResult Reponse = EnregistrerFichierPdn.ShowDialog();      // l'utilisateur doit rentrer le nom du fichier PGN
                    if (Reponse == DialogResult.OK)                             // On ne sauvegarde que si l'utilisateur est d'accord
                    {
                        string cheminPdn = EnregistrerFichierPdn.FileName;
                        if (File.Exists(cheminPdn))                         // Si le fichier existe déjà
                        {   // On demande à l'utilisateur s'il veut écraser le fichier ou ajouter la partie
                            DialogResult resultat = MessageBox.Show("ATTENTION, le fichier " + Path.GetFileName(cheminPdn) + " existe déjà. \nCliquer Oui pour ajouter la partie à la fin." +
                               "\nCliquer Non pour écraser le fichier existant.\nCancel pour afficher le fichier PGN.", "Fichier existant", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                            if (resultat == DialogResult.No)
                            {   // Écrase le fichier existant avec la nouvelle partie
                                File.WriteAllText(cheminPdn, contenuPdn);
                                MessageBox.Show($"La partie est écrite dans \nle ficher {Path.GetFileName(cheminPdn)}", "Ecriture fichier PDN", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // LabelInformationJoueur.Text = "La partie est écrite dans le fichier " + Path.GetFileName(cheminPdn);
                            }
                            else if (resultat == DialogResult.Yes)
                            {   // Ajoute la nouvelle partie à la fin du fichier existant
                                string contenuExistant = File.ReadAllText(cheminPdn);
                                contenuExistant += "\n\n" + contenuPdn;
                                File.WriteAllText(cheminPdn, contenuExistant);
                                MessageBox.Show($"La partie est ajoutée dans \nle ficher {Path.GetFileName(cheminPdn)}", "Ajout fichier PDN", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // LabelInformationJoueur.Text = "La partie est ajoutée dans le fichier " + Path.GetFileName(cheminPdn);
                            }
                            else if (resultat == DialogResult.Cancel)
                            {   // Affiche la nouvelle partie
                                MessageBox.Show($"Fichier PDN :\n {contenuPdn}", "Affichage fichier PDN", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {   // Si le fichier n'existe pas, écrire simplement la nouvelle partie
                            File.WriteAllText(cheminPdn, contenuPdn);               // Ecriture du fichier au format PGN
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur s'est produite : {ex.Message}", "Erreur méthode Enregistrer PDN", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Console.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }
        private void AffichePdn_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;  // Annule la fermeture de la fenêtre
            this.Hide();      // Masque la fenêtre au lieu de la fermer;
        }

        private void InitializeComponent()
        {
            this.ZoneAffichage = new System.Windows.Forms.RichTextBox();
            this.MasqueAffichePdn = new Krypton.Toolkit.KryptonButton();
            this.AffichePdnFr = new Krypton.Toolkit.KryptonButton();
            this.AfficheFen = new Krypton.Toolkit.KryptonButton();
            this.AfficheListeHub2 = new Krypton.Toolkit.KryptonButton();
            this.EnregistrerPdn = new Krypton.Toolkit.KryptonButton();
            this.EnregistrerFichierPdn = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // ZoneAffichage
            // 
            this.ZoneAffichage.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ZoneAffichage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZoneAffichage.Location = new System.Drawing.Point(13, 13);
            this.ZoneAffichage.Name = "ZoneAffichage";
            this.ZoneAffichage.Size = new System.Drawing.Size(756, 512);
            this.ZoneAffichage.TabIndex = 0;
            this.ZoneAffichage.Text = "Zone Affichage de la partie";
            // 
            // MasqueAffichePdn
            // 
            this.MasqueAffichePdn.Location = new System.Drawing.Point(639, 531);
            this.MasqueAffichePdn.Name = "MasqueAffichePdn";
            this.MasqueAffichePdn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.MasqueAffichePdn.Size = new System.Drawing.Size(120, 40);
            this.MasqueAffichePdn.StateCommon.Border.Rounding = 20F;
            this.MasqueAffichePdn.StateCommon.Border.Width = 1;
            this.MasqueAffichePdn.TabIndex = 1;
            this.MasqueAffichePdn.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.MasqueAffichePdn.Values.Text = "Fermer";
            this.MasqueAffichePdn.Click += new System.EventHandler(this.MasqueAffichePdn_Click);
            // 
            // AffichePdnFr
            // 
            this.AffichePdnFr.Location = new System.Drawing.Point(13, 531);
            this.AffichePdnFr.Name = "AffichePdnFr";
            this.AffichePdnFr.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.AffichePdnFr.Size = new System.Drawing.Size(120, 40);
            this.AffichePdnFr.StateCommon.Border.Rounding = 20F;
            this.AffichePdnFr.StateCommon.Border.Width = 1;
            this.AffichePdnFr.TabIndex = 2;
            this.AffichePdnFr.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.AffichePdnFr.Values.Text = "Coups Pdn";
            this.AffichePdnFr.Click += new System.EventHandler(this.AffichePdnFr_Click);
            // 
            // AfficheFen
            // 
            this.AfficheFen.Location = new System.Drawing.Point(163, 531);
            this.AfficheFen.Name = "AfficheFen";
            this.AfficheFen.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.AfficheFen.Size = new System.Drawing.Size(120, 40);
            this.AfficheFen.StateCommon.Border.Rounding = 20F;
            this.AfficheFen.StateCommon.Border.Width = 1;
            this.AfficheFen.TabIndex = 3;
            this.AfficheFen.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.AfficheFen.Values.Text = "Coups FEN";
            this.AfficheFen.Click += new System.EventHandler(this.ListeFenAffichePdn_Click);
            // 
            // AfficheListeHub2
            // 
            this.AfficheListeHub2.Location = new System.Drawing.Point(322, 531);
            this.AfficheListeHub2.Name = "AfficheListeHub2";
            this.AfficheListeHub2.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.AfficheListeHub2.Size = new System.Drawing.Size(120, 40);
            this.AfficheListeHub2.StateCommon.Border.Rounding = 20F;
            this.AfficheListeHub2.StateCommon.Border.Width = 1;
            this.AfficheListeHub2.TabIndex = 4;
            this.AfficheListeHub2.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.AfficheListeHub2.Values.Text = "Coups Hub2";
            this.AfficheListeHub2.Click += new System.EventHandler(this.ListeHub2AffichePdn_Click);
            // 
            // EnregistrerPdn
            // 
            this.EnregistrerPdn.Location = new System.Drawing.Point(469, 531);
            this.EnregistrerPdn.Name = "EnregistrerPdn";
            this.EnregistrerPdn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            this.EnregistrerPdn.Size = new System.Drawing.Size(120, 40);
            this.EnregistrerPdn.StateCommon.Border.Rounding = 20F;
            this.EnregistrerPdn.StateCommon.Border.Width = 1;
            this.EnregistrerPdn.TabIndex = 5;
            this.EnregistrerPdn.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.EnregistrerPdn.Values.Text = "Enregistre Partie";
            this.EnregistrerPdn.Click += new System.EventHandler(this.EnregistrerPdn_Click);
            // 
            // EnregistrerFichierPdn
            // 
            this.EnregistrerFichierPdn.DefaultExt = "pdn";
            this.EnregistrerFichierPdn.Filter = "Fichiers PDN (*.pdn)|*.pdn|Tous les fichiers (*.*)|*.*";
            // 
            // AffichePdn
            // 
            this.ClientSize = new System.Drawing.Size(771, 577);
            this.Controls.Add(this.EnregistrerPdn);
            this.Controls.Add(this.AfficheListeHub2);
            this.Controls.Add(this.AfficheFen);
            this.Controls.Add(this.AffichePdnFr);
            this.Controls.Add(this.MasqueAffichePdn);
            this.Controls.Add(this.ZoneAffichage);
            this.Name = "AffichePdn";
            this.Text = "  - Affichage liste coups de la partie - ";
            this.ResumeLayout(false);
        }
    }
}