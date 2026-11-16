// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;

namespace BrunoGUI_Dames
{
    // Logique pure : quelle langue de l'aide ouvrir. L'aide existe en français (« fr », la langue du programme et le repli) et en anglais (« en »).
    public static class LangueAide
    {
        // --- La langue de l'aide : celle de l'interface de Windows (deux lettres, « en », « fr »…) si l'aide existe dans cette langue, sinon le français.
        // existe(langue) dit si la page de cette langue est présente à côté du programme ---
        public static string Choisit(string langueInterface, Func<string, bool> existe)
        {
            string langue = (langueInterface ?? "").Trim().ToLowerInvariant();
            if (langue == "en" && existe("en"))
                return "en";
            return "fr";
        }
    }
}
