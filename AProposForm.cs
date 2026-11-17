// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// La fenêtre « À propos » : les versions du programme et du moteur, l'historique, et le choix de la langue de l'affichage (retenu dans les préférences, appliqué au
// prochain démarrage). Quand on change la langue dans la liste, cette fenêtre se traduit tout de suite (la langue du texte est changée le temps que la fenêtre est ouverte,
// puis remise à la fermeture : le reste du programme garde sa langue jusqu'au prochain démarrage). LangueChoisie rend « fr » ou « en ».

using System;
using System.Drawing;
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    public class AProposForm : Form
    {
        private readonly ComboBox listeLangue = new ComboBox();
        private readonly Label contenu = new Label { Name = "TexteAPropos" };
        private readonly Label note = new Label { Name = "NoteLangue" };
        private readonly Func<string> construitTexte;
        private readonly string langueDuProgramme;      // La langue en cours quand la fenêtre s'ouvre : elle est remise à la fermeture

        public string LangueChoisie => listeLangue.SelectedIndex == 1 ? Texte.Anglais : Texte.Francais;

        public AProposForm(Func<string> texte, string langueActuelle)
        {
            construitTexte = texte;
            langueDuProgramme = Texte.Langue;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(560, 300);
            contenu.Location = new Point(12, 10);
            contenu.Size = new Size(536, 190);
            contenu.AutoSize = false;
            Label etiquette = new Label { Name = "EtiquetteLangue", Text = "Langue / Language :", Location = new Point(12, 212), Size = new Size(130, 21), TextAlign = ContentAlignment.MiddleLeft };
            listeLangue.Name = "ListeLangue";
            listeLangue.DropDownStyle = ComboBoxStyle.DropDownList;
            listeLangue.Location = new Point(146, 212);
            listeLangue.Width = 140;
            listeLangue.Items.Add("Français");
            listeLangue.Items.Add("English");
            listeLangue.SelectedIndex = string.Equals(langueActuelle, Texte.Anglais, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            note.Location = new Point(12, 246);
            note.Size = new Size(440, 40);          // Plus étroite que la fenêtre : le bouton OK est à droite, sur la même ligne
            note.AutoSize = false;
            Button ok = new Button { Name = "BoutonOk", Text = "OK", DialogResult = DialogResult.OK, Location = new Point(468, 260), Size = new Size(80, 28) };
            Controls.AddRange(new Control[] { contenu, etiquette, listeLangue, note, ok });
            AcceptButton = ok;
            ThemeInterface.Applique(this);
            ChangeLaLangue();       // La liste montre la langue choisie : la fenêtre s'écrit dans cette langue dès l'ouverture
            listeLangue.SelectedIndexChanged += (s, e) => ChangeLaLangue();
            FormClosed += (s, e) => Texte.Definit(langueDuProgramme, Texte.FichierTraductions);
        }

        // --- Écrit la fenêtre dans la langue de la liste ---
        private void ChangeLaLangue()
        {
            if (LangueChoisie != Texte.Langue)
                Texte.Definit(LangueChoisie, Texte.FichierTraductions);
            Text = Texte.T("À propos");
            contenu.Text = construitTexte();
            note.Text = Texte.T("Le changement de langue s'applique au prochain démarrage du programme.");
        }
    }
}
