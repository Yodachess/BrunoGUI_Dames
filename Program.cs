// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    internal static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Police par défaut de .NET Framework (celle de la version 1.04), sur laquelle le Designer est calé (AutoScaleDimensions 6 x 13) :
            // avec la police par défaut de .NET (Segoe UI 9) la fenêtre est plus grande d'environ 16 %
            Application.SetDefaultFont(new System.Drawing.Font("Microsoft Sans Serif", 8.25F));
            // Une exception qu'aucun code n'a attrapée ne ferme plus l'application sans trace : elle est écrite dans Journal.log et signalée
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) => ErreurImprevue(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Journal.Ecrit("ERREUR fatale non attrapée : " + e.ExceptionObject);
            Application.Run(new BrunoInterfaceGraphiqueDames());
        }

        // --- Une erreur imprévue dans l'interface : on la note dans le journal et on le dit, l'application continue ---
        private static void ErreurImprevue(Exception exception)
        {
            Journal.Ecrit("ERREUR imprévue : " + exception);
            MessageBox.Show("Une erreur imprévue s'est produite :\n" + exception.Message + "\n\nElle est notée dans Journal.log, à côté du programme.\nSi l'application se comporte bizarrement, ferme-la et relance-la.",
                            "Erreur imprévue", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
