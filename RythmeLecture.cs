// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// La vitesse de la lecture d'une partie (bouton « Lecture » du groupe « Parcours partie ») : cinq niveaux, du plus lent au plus rapide, qui donnent la pause entre deux coups.
// Logique pure, sans Windows Forms. La durée des animations d'un coup (glissement, étapes d'une rafle) est un autre réglage : DureeEtape des préférences.

using System;

namespace BrunoGUI_Dames
{
    public static class RythmeLecture
    {
        public const int Minimum = 1;           // Le plus lent
        public const int Maximum = 5;           // Le plus rapide
        public const int ParDefaut = 3;
        private static readonly int[] pausesEnMillisecondes = { 1500, 900, 500, 250, 80 };

        // --- Un niveau ramené entre Minimum et Maximum ---
        public static int Limite(int niveau) => Math.Clamp(niveau, Minimum, Maximum);

        // --- La pause entre deux coups, en millisecondes, au niveau donné ---
        public static int PauseMs(int niveau) => pausesEnMillisecondes[Limite(niveau) - 1];

        // --- Le texte de l'infobulle : « Vitesse de la lecture : 3 sur 5 (0,5 s entre deux coups) » ---
        public static string Libelle(int niveau)
        {
            int n = Limite(niveau);
            return Texte.T("Vitesse de la lecture : {0} sur {1} ({2} s entre deux coups)", n, Maximum, (PauseMs(n) / 1000.0).ToString("0.0#", Texte.Culture));
        }
    }
}
