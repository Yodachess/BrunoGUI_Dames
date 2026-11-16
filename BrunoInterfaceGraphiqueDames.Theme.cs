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
        // --- Applique les couleurs de l'interface (ThemeInterface) aux contrôles du formulaire principal : le Designer garde les anciennes valeurs, remplacées une fois
        // tous les contrôles créés (le constructeur appelle AppliqueTheme après CreeLecture, qui crée les derniers) ---
        private void AppliqueTheme()
        {
            BackColor = ThemeInterface.Fond;
            foreach (Label bandeau in new[] { LabelCoupJoue, LabelInformationJoueur })
            {
                bandeau.BackColor = ThemeInterface.Bandeau;
                bandeau.ForeColor = ThemeInterface.TexteSurFonce;
            }
            foreach (GroupBox cadre in new[] { groupBox1, groupParcoursPartie })
            {
                cadre.BackColor = ThemeInterface.Bandeau;
                cadre.ForeColor = ThemeInterface.TexteSurFonce;
            }
            foreach (Label ligne in new[] { LabelAfficheScan, LabelScore })
            {
                ligne.BackColor = ThemeInterface.Ligne;
                ligne.ForeColor = ThemeInterface.TexteSurClair;
            }
            foreach (Control panneau in new Control[] { LabelPrisesPossibles, LabelPionsBlancs, LabelPionsNoirs, LabelDamesBlanches, LabelDamesNoires, OrdinateurBosse, SonEmis, GroupResultat })
            {
                panneau.BackColor = ThemeInterface.Panneau;
                panneau.ForeColor = ThemeInterface.TexteSurClair;
            }
            foreach (Button petit in new[] { BoutonPrecedent, BoutonSuivant, BoutonDebut, BoutonFin, boutonLecture })
                ThemeInterface.ColoreBoutonClair(petit);
            foreach (KryptonButton bouton in new[] { NouvellePartie, OrdinateurJoue, BoutonRetourArriere, BoutonTournerDamier, AnalysePosition, VisualisationPdn,
                                                      ChargePartiesPdn, BoutonMasqueAffiche, SauvePartiesPdn, ChargePositionFen, SauvePositionFen, MontreDonneesScan,
                                                      Apropos, BoutonAide, BoutonQuitterApplication })
                ThemeInterface.ColoreBouton(bouton);
        }
    }
}
