// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Threading.Tasks;
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
            // Une exception qu'aucun code n'a attrapée ne ferme plus l'application sans trace : elle est écrite dans BrunoGUI_Dames.log et signalée
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) => ErreurImprevue(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Journal.Erreur("Erreur fatale non attrapée (l'application s'arrête)", e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()));
            TaskScheduler.UnobservedTaskException += (sender, e) => { Journal.Erreur("Erreur dans une tâche", e.Exception); e.SetObserved(); };
            // La langue de l'affichage : celle des préférences, sinon celle de Windows (avant la création de la première fenêtre)
            Texte.Definit(Preferences.Lire(System.IO.Path.Combine(Application.StartupPath, "BrunoGUI_Dames.preferences.ini")).Langue ?? Texte.LangueDuSysteme(), System.IO.Path.Combine(Application.StartupPath, "Langues", "en.tsv"));
            Journal.Info($"Démarrage de BrunoGUI Dames {Application.ProductVersion} (.NET {Environment.Version}, {Environment.OSVersion})");
            Application.Run(new BrunoInterfaceGraphiqueDames());
        }

        // --- Une erreur imprévue dans l'interface : on la note dans le journal et on le dit, l'application continue ---
        private static void ErreurImprevue(Exception exception)
        {
            Journal.Erreur("Erreur imprévue dans l'interface", exception);
            MessageBox.Show(Texte.T("Une erreur imprévue s'est produite :\n{0}\n\nElle est notée dans BrunoGUI_Dames.log, à côté du programme.\nSi l'application se comporte bizarrement, ferme-la et relance-la.", exception.Message),
                            Texte.T("Erreur imprévue"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
