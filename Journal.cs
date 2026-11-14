// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
// Journal des anomalies, à côté de l'exécutable (Journal.log) : tout catch qui avale une erreur y écrit. Logique pure, sans Windows Forms.

using System;
using System.IO;
using System.Text;

namespace BrunoGUI_Dames
{
    public static class Journal
    {
        private static readonly object verrou = new object();

        // --- Le texte tel qu'il est écrit dans le journal : les caractères de contrôle (octets d'un fichier binaire, par exemple) et le caractère de remplacement
        // deviennent « ? », les lignes qui suivent la première (la trace d'une exception) sont décalées, et un texte trop long est coupé ---
        public static string LigneSure(string texte)
        {
            const int longueurMax = 3000;
            StringBuilder sortie = new StringBuilder();
            foreach (char c in texte)
            {
                if (sortie.Length >= longueurMax) { sortie.Append(" [...]"); break; }
                if (c == (char)10) sortie.Append(Environment.NewLine).Append("    ");
                else if (c == (char)13) continue;
                else if (c == (char)9) sortie.Append(c);
                else if (char.IsControl(c) || c == (char)0xFFFD) sortie.Append('?');
                else sortie.Append(c);
            }
            return sortie.ToString();
        }

        // --- Ajoute une ligne datée au journal ; si le journal ne peut pas être écrit, on continue (il est une aide, pas une condition) ---
        public static void Ecrit(string ligne)
        {
            try
            {
                lock (verrou)
                    File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "Journal.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {LigneSure(ligne)}{Environment.NewLine}");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine("Journal illisible : " + ex.Message);
            }
        }
    }
}