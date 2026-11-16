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
using System.Windows.Forms;
using Krypton.Toolkit;

namespace BrunoGUI_Dames
{
    // Les couleurs de l'interface (hors damier), accordées au bois du damier : « noyer et crème », choisi le 2026-11-16 parmi trois essais. Les autres : « vert tapis » (fond DCE5D8,
    // bandeaux 3E6B4F, panneaux F3F6EE, ligne C9D8C4, boutons 2F5D44, survol 3E6B4F, enfoncé 1F3D2D, petits boutons E4EEDF, textes FFFFFF et 1F3326) et « ardoise sobre » (fond E6E3DE,
    // bandeaux 3B4252, panneaux F5F3EF, ligne D8D4CC, boutons 4C566A, survol 5E6A82, enfoncé 2E3440, petits boutons ECEFF4, textes ECEFF4 et 2B303B).
    // Le formulaire principal (BrunoInterfaceGraphiqueDames.Theme.cs) les pose contrôle par contrôle ; les fenêtres secondaires appellent Applique(fenetre).
    internal static class ThemeInterface
    {
        public static readonly Color Fond = Color.FromArgb(0xEA, 0xDB, 0xC4);           // Fond des fenêtres : sable
        public static readonly Color Bandeau = Color.FromArgb(0x7A, 0x4A, 0x21);        // Bandeaux et cadres : noyer
        public static readonly Color Panneau = Color.FromArgb(0xF8, 0xEF, 0xDD);        // Panneaux, listes et zones de texte : crème
        public static readonly Color Ligne = Color.FromArgb(0xE0, 0xCB, 0xA8);          // Lignes d'information, grilles : crème plus soutenu
        public static readonly Color Bouton = Color.FromArgb(0x5A, 0x3A, 0x24);         // Boutons : espresso
        public static readonly Color BoutonSurvol = Color.FromArgb(0x7A, 0x4A, 0x21);
        public static readonly Color BoutonEnfonce = Color.FromArgb(0x3F, 0x28, 0x17);
        public static readonly Color BoutonClair = Color.FromArgb(0xF0, 0xDD, 0xBF);    // Petits boutons posés sur un bandeau
        public static readonly Color TexteSurFonce = Color.FromArgb(0xFF, 0xF6, 0xE3);  // Texte crème sur noyer ou espresso
        public static readonly Color TexteSurClair = Color.FromArgb(0x3A, 0x24, 0x10);  // Texte brun sur crème

        // --- Un bouton Krypton espresso à texte crème, plus clair au survol, plus sombre enfoncé ; la même couleur dans l'état « par défaut » et « avec le focus »
        // (sinon le bouton par défaut de la fenêtre garde la couleur du thème Krypton) ---
        public static void ColoreBouton(KryptonButton bouton)
        {
            void Pose(dynamic etat, Color fond)     // dynamic : les états (PaletteTriple) et les surcharges (PaletteTripleRedirect) ont les mêmes membres sans type commun
            {
                etat.Back.ColorStyle = PaletteColorStyle.Solid;
                etat.Back.Color1 = fond;
                etat.Back.Color2 = fond;
                etat.Border.Color1 = BoutonEnfonce;
                etat.Border.Color2 = BoutonEnfonce;
                etat.Content.ShortText.Color1 = TexteSurFonce;
                etat.Content.ShortText.Color2 = TexteSurFonce;
            }
            Pose(bouton.StateCommon, Bouton);
            Pose(bouton.StateNormal, Bouton);
            Pose(bouton.OverrideDefault, Bouton);
            Pose(bouton.OverrideFocus, Bouton);
            Pose(bouton.StateTracking, BoutonSurvol);
            Pose(bouton.StatePressed, BoutonEnfonce);
        }

        // --- Un bouton Windows ordinaire, crème clair à texte brun ---
        public static void ColoreBoutonClair(Button bouton)
        {
            bouton.BackColor = BoutonClair;
            bouton.ForeColor = TexteSurClair;
            bouton.UseVisualStyleBackColor = false;
        }

        // --- Habille une fenêtre secondaire : fond sable, boutons, zones de texte, listes et grilles accordés au thème (tous les contrôles, à tous les niveaux) ---
        public static void Applique(Form fenetre)
        {
            fenetre.BackColor = Fond;
            fenetre.ForeColor = TexteSurClair;
            AppliqueAuxContenus(fenetre.Controls);
        }
        private static void AppliqueAuxContenus(Control.ControlCollection controles)
        {
            foreach (Control c in controles)
            {
                switch (c)
                {
                    case KryptonButton krypton:
                        ColoreBouton(krypton);
                        break;
                    case Button bouton:
                        ColoreBoutonClair(bouton);
                        break;
                    case RichTextBox texte:
                        texte.BackColor = Panneau;
                        texte.ForeColor = TexteSurClair;
                        break;
                    case DataGridView grille:
                        grille.BackgroundColor = Panneau;
                        grille.GridColor = Ligne;
                        grille.EnableHeadersVisualStyles = false;
                        grille.ColumnHeadersDefaultCellStyle.BackColor = Bandeau;
                        grille.ColumnHeadersDefaultCellStyle.ForeColor = TexteSurFonce;
                        grille.DefaultCellStyle.BackColor = Panneau;
                        grille.DefaultCellStyle.ForeColor = TexteSurClair;
                        grille.DefaultCellStyle.SelectionBackColor = Bandeau;
                        grille.DefaultCellStyle.SelectionForeColor = TexteSurFonce;
                        break;
                    case Label etiquette:
                        etiquette.ForeColor = TexteSurClair;
                        if (etiquette.BackColor != SystemColors.Control && etiquette.BackColor != Color.Transparent)
                            etiquette.BackColor = Ligne;        // Une étiquette dont le fond avait été choisi (une ligne d'information) prend celui du thème
                        break;
                }
                if (c.HasChildren)
                    AppliqueAuxContenus(c.Controls);
            }
        }
    }
}
