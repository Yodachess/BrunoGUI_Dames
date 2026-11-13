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

namespace BrunoGUI_Dames
{
    public static class Journal
    {
        private static readonly object verrou = new object();

        // --- Ajoute une ligne datée au journal ; si le journal ne peut pas être écrit, on continue (il est une aide, pas une condition) ---
        public static void Ecrit(string ligne)
        {
            try
            {
                lock (verrou)
                    File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "Journal.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {ligne}{Environment.NewLine}");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine("Journal illisible : " + ex.Message);
            }
        }
    }
}