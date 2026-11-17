// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    public partial class BrunoInterfaceGraphiqueDames
    {
        // L'image du damier et le menu qu'on ouvre d'un clic droit sur le damier (rien dans la colonne de boutons, qui est pleine) : enregistrer l'image (Ctrl+E), la copier, tourner le damier (Ctrl+F)
        private ContextMenuStrip? menuDamier;
        private ToolStripMenuItem? menuEnregistreImage, menuCopieImage, menuTourne;

        // --- Crée le menu du damier et le pose sur le panneau et sur chacune des 100 cases (appelée après CreeDamier) ---
        private void CreeMenuDamier()
        {
            menuDamier = new ContextMenuStrip { Name = "menuDamier" };
            menuEnregistreImage = new ToolStripMenuItem("Enregistrer l'image du damier…") { Name = "menuEnregistreImage", ShortcutKeyDisplayString = "Ctrl+E" };
            menuCopieImage = new ToolStripMenuItem("Copier l'image du damier") { Name = "menuCopieImage" };
            menuTourne = new ToolStripMenuItem("Tourner le damier") { Name = "menuTourne", ShortcutKeyDisplayString = "F" };
            menuEnregistreImage.Click += (s, e) => EnregistreImageDamier();
            menuCopieImage.Click += (s, e) => CopieImageDamier();
            menuTourne.Click += (s, e) => ToucheRaccourci(Keys.Control | Keys.F);
            menuDamier.Items.AddRange(new ToolStripItem[] { menuEnregistreImage, menuCopieImage, new ToolStripSeparator(), menuTourne });
            menuDamier.BackColor = ThemeInterface.Panneau;
            menuDamier.ForeColor = ThemeInterface.TexteSurClair;
            Damier10x10.ContextMenuStrip = menuDamier;
            foreach (PictureBox c in CaseDamier)
                c.ContextMenuStrip = menuDamier;
        }

        // --- L'image du damier tel qu'il est à l'écran : pièces, couleurs, repères et numéros (640 × 640) ---
        private Bitmap ImageDuDamier()
        {
            Bitmap image = new Bitmap(Damier10x10.Width, Damier10x10.Height);
            Damier10x10.DrawToBitmap(image, new Rectangle(0, 0, image.Width, image.Height));
            return image;
        }

        // --- Écrit l'image du damier dans un fichier PNG ; rend faux, avec un message, si le fichier ne peut pas être écrit ---
        public bool EnregistreImageDamier(string chemin)
        {
            try
            {
                using (Bitmap image = ImageDuDamier())
                    image.Save(chemin, ImageFormat.Png);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ExternalException || ex is ArgumentException)
            {
                Journal.Erreur($"Image du damier non enregistrée ({chemin})", ex);
                ReglagesDeTest.Message("L'image n'a pas pu être enregistrée :\n" + ex.Message, "Image du damier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
        private void EnregistreImageDamier()
        {   // --- Demande où écrire l'image, puis l'écrit ---
            using (SaveFileDialog dialogue = new SaveFileDialog { Title = "Enregistrer l'image du damier", Filter = "Image PNG (*.png)|*.png", FileName = "damier.png", DefaultExt = "png", OverwritePrompt = true })
                if (dialogue.ShowDialog(this) == DialogResult.OK)
                    EnregistreImageDamier(dialogue.FileName);
        }

        // --- Met l'image du damier dans le presse-papiers ; rend faux, avec un message, si le presse-papiers est occupé ---
        public bool CopieImageDamier()
        {
            try
            {
                using (Bitmap image = ImageDuDamier())
                    Clipboard.SetImage(image);
                return true;
            }
            catch (ExternalException ex)
            {   // Le presse-papiers est tenu par un autre programme
                Journal.Attention("Image du damier non copiée : " + ex.Message);
                ReglagesDeTest.Message("L'image n'a pas pu être copiée :\n" + ex.Message, "Image du damier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        // --- Les raccourcis qui ne sont pas le parcours : Ctrl+N (nouvelle partie), Ctrl+Z (retour arrière), Ctrl+E (image du damier), Ctrl+F (tourner le damier). Rend vrai si la touche a servi.
        // Sans effet dans une zone de saisie (Ctrl+Z y défait la frappe), ni quand le bouton correspondant est inactif (lecture, réflexion du moteur) ---
        private bool ToucheRaccourci(Keys touche)
        {
            if (touche != (Keys.Control | Keys.N) && touche != (Keys.Control | Keys.Z) && touche != (Keys.Control | Keys.E) && touche != (Keys.Control | Keys.F))
                return false;
            if (EstEnSaisie() || lecture != null)
                return false;
            switch (touche)
            {
                case Keys.Control | Keys.N:
                    if (NouvellePartie.Enabled) NouvellePartie_Click(this, EventArgs.Empty);
                    break;
                case Keys.Control | Keys.Z:
                    if (BoutonRetourArriere.Enabled && !pieceEnMain) BoutonRetourArriere_Click(this, EventArgs.Empty);
                    break;
                case Keys.Control | Keys.E:
                    EnregistreImageDamier();
                    break;
                default:
                    if (!pieceEnMain) BoutonTournerDamier_Click(this, EventArgs.Empty);
                    break;
            }
            return true;
        }
    }
}