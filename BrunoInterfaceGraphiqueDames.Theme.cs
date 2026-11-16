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
    public partial class BrunoInterfaceGraphiqueDames
    {
        // Les couleurs de l'interface (hors damier), accordées au bois du damier : « noyer et crème », choisi le 2026-11-16 parmi trois essais. Les autres : « vert tapis » (fond DCE5D8, bandeaux 3E6B4F, panneaux F3F6EE, ligne C9D8C4, boutons 2F5D44, survol 3E6B4F, enfoncé 1F3D2D, petits boutons E4EEDF, textes FFFFFF et 1F3326) et « ardoise sobre » (fond E6E3DE, bandeaux 3B4252, panneaux F5F3EF, ligne D8D4CC, boutons 4C566A, survol 5E6A82, enfoncé 2E3440, petits boutons ECEFF4, textes ECEFF4 et 2B303B). Le Designer garde les anciennes valeurs ; AppliqueTheme les remplace
        // une fois les contrôles créés (le constructeur l'appelle après CreeLecture, qui crée les derniers).
        private static readonly Color ThemeFond = Color.FromArgb(0xEA, 0xDB, 0xC4);           // Fond de la fenêtre : sable
        private static readonly Color ThemeBandeau = Color.FromArgb(0x7A, 0x4A, 0x21);        // Bandeaux et cadres : noyer
        private static readonly Color ThemePanneau = Color.FromArgb(0xF8, 0xEF, 0xDD);        // Panneaux de compteurs et de résultat : crème
        private static readonly Color ThemeLigne = Color.FromArgb(0xE0, 0xCB, 0xA8);          // Ligne du moteur et score : crème plus soutenu
        private static readonly Color ThemeBouton = Color.FromArgb(0x5A, 0x3A, 0x24);         // Boutons : espresso
        private static readonly Color ThemeBoutonSurvol = Color.FromArgb(0x7A, 0x4A, 0x21);
        private static readonly Color ThemeBoutonEnfonce = Color.FromArgb(0x3F, 0x28, 0x17);
        private static readonly Color ThemeBoutonClair = Color.FromArgb(0xF0, 0xDD, 0xBF);    // Petits boutons posés sur un bandeau
        private static readonly Color ThemeTexteSurFonce = Color.FromArgb(0xFF, 0xF6, 0xE3);  // Texte crème sur noyer ou espresso
        private static readonly Color ThemeTexteSurClair = Color.FromArgb(0x3A, 0x24, 0x10);  // Texte brun sur crème

        // --- Applique les couleurs de l'interface à tous les contrôles ---
        private void AppliqueTheme()
        {
            BackColor = ThemeFond;
            foreach (Label bandeau in new[] { LabelCoupJoue, LabelInformationJoueur })
            {
                bandeau.BackColor = ThemeBandeau;
                bandeau.ForeColor = ThemeTexteSurFonce;
            }
            foreach (GroupBox cadre in new[] { groupBox1, groupParcoursPartie })
            {
                cadre.BackColor = ThemeBandeau;
                cadre.ForeColor = ThemeTexteSurFonce;
            }
            foreach (Label ligne in new[] { LabelAfficheScan, LabelScore })
            {
                ligne.BackColor = ThemeLigne;
                ligne.ForeColor = ThemeTexteSurClair;
            }
            foreach (Control panneau in new Control[] { LabelPrisesPossibles, LabelPionsBlancs, LabelPionsNoirs, LabelDamesBlanches, LabelDamesNoires, OrdinateurBosse, SonEmis, GroupResultat })
            {
                panneau.BackColor = ThemePanneau;
                panneau.ForeColor = ThemeTexteSurClair;
            }
            foreach (Button petit in new[] { BoutonPrecedent, BoutonSuivant, BoutonDebut, BoutonFin, boutonLecture })
            {
                petit.BackColor = ThemeBoutonClair;
                petit.ForeColor = ThemeTexteSurClair;
                petit.UseVisualStyleBackColor = false;
            }
            foreach (KryptonButton bouton in new[] { NouvellePartie, OrdinateurJoue, BoutonRetourArriere, BoutonTournerDamier, AnalysePosition, VisualisationPdn,
                                                      ChargePartiesPdn, BoutonMasqueAffiche, SauvePartiesPdn, ChargePositionFen, SauvePositionFen, MontreDonneesScan,
                                                      Apropos, BoutonAide, BoutonQuitterApplication })
            {
                ColoreBouton(bouton);
            }
        }

        // --- Un bouton Krypton espresso à texte crème, plus clair au survol, plus sombre enfoncé ; la même couleur dans l'état « par défaut » et « avec le focus »
        // (sinon le bouton par défaut de la fenêtre garde la couleur du thème Krypton) ---
        private static void ColoreBouton(KryptonButton bouton)
        {
            void Pose(dynamic etat, Color fond)     // dynamic : les états (PaletteTriple) et les surcharges (PaletteTripleRedirect) ont les mêmes membres sans type commun
            {
                etat.Back.ColorStyle = PaletteColorStyle.Solid;
                etat.Back.Color1 = fond;
                etat.Back.Color2 = fond;
                etat.Border.Color1 = ThemeBoutonEnfonce;
                etat.Border.Color2 = ThemeBoutonEnfonce;
                etat.Content.ShortText.Color1 = ThemeTexteSurFonce;
                etat.Content.ShortText.Color2 = ThemeTexteSurFonce;
            }
            Pose(bouton.StateCommon, ThemeBouton);
            Pose(bouton.StateNormal, ThemeBouton);
            Pose(bouton.OverrideDefault, ThemeBouton);
            Pose(bouton.OverrideFocus, ThemeBouton);
            Pose(bouton.StateTracking, ThemeBoutonSurvol);
            Pose(bouton.StatePressed, ThemeBoutonEnfonce);
        }
    }
}
