// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// La force du moteur : six niveaux, du plus faible au plus fort, qui limitent la profondeur de recherche de Scan (« level depth=N » du protocole Hub2,
// en demi-coups). Le temps de réflexion reste un autre réglage : le moteur s'arrête à la première des deux limites. Logique pure, sans Windows Forms.

using System;

namespace BrunoGUI_Dames
{
    public static class NiveauMoteur
    {
        public const int Minimum = 1;           // Le plus faible
        public const int Maximum = 6;           // Pleine force : seule la limite de temps compte
        public const int ParDefaut = Maximum;
        public const int ProfondeurMaximale = 99;       // La profondeur maximale de Scan (Depth_Max) : « pas de limite »
        private static readonly int[] profondeurs = { 1, 2, 4, 6, 9, ProfondeurMaximale };

        // --- Un niveau ramené entre Minimum et Maximum ---
        public static int Limite(int niveau) => Math.Clamp(niveau, Minimum, Maximum);

        // --- La profondeur de recherche donnée à Scan au niveau donné ---
        public static int Profondeur(int niveau) => profondeurs[Limite(niveau) - 1];

        // --- Le nom court (pour la liste) : « Niveau 3 », « Maximum » ---
        public static string Nom(int niveau)
        {
            int n = Limite(niveau);
            return n == Maximum ? "Maximum" : "Niveau " + n;
        }

        // --- Le texte de l'infobulle : « Force du moteur : niveau 3 sur 6 (recherche limitée à 4 demi-coups) » ---
        public static string Libelle(int niveau)
        {
            int n = Limite(niveau);
            if (n == Maximum)
                return Texte.T("Force du moteur : maximum (niveau {0} sur {1}), sans limite de profondeur ; seul le temps de réflexion borne la recherche", n, Maximum);
            return Profondeur(n) > 1
                ? Texte.T("Force du moteur : niveau {0} sur {1} (recherche limitée à {2} demi-coups). Au maximum, le moteur joue à pleine force", n, Maximum, Profondeur(n))
                : Texte.T("Force du moteur : niveau {0} sur {1} (recherche limitée à {2} demi-coup). Au maximum, le moteur joue à pleine force", n, Maximum, Profondeur(n));
        }
    }
}
