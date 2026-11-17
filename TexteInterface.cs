// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// La traduction des textes que le Designer pose sur les contrôles (boutons, étiquettes, cadres, titres de fenêtre) : le Designer écrit les textes en français,
// la fenêtre les passe par Texte.T à la fin de son constructeur. Sans effet en français.

using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    public static class TexteInterface
    {
        // --- Traduit le texte du contrôle et de tous ses contrôles fils ---
        public static void Traduit(Control racine)
        {
            if (Texte.Langue == Texte.Francais)
                return;
            TraduitUn(racine);
        }
        private static void TraduitUn(Control controle)
        {
            if (controle.Text.Length > 0)
                controle.Text = Texte.T(controle.Text);
            foreach (Control fils in controle.Controls)
                TraduitUn(fils);
        }

        // --- Le filtre et le titre d'une boîte de choix de fichier (écrits en français par le Designer) ---
        public static void Traduit(FileDialog dialogue)
        {
            if (Texte.Langue == Texte.Francais)
                return;
            dialogue.Filter = Texte.T(dialogue.Filter);
            dialogue.Title = Texte.T(dialogue.Title);
        }
    }
}
