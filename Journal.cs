// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
// Le journal des problèmes (BrunoGUI_Dames.log), à côté de l'exécutable : les erreurs, les plantages et ce qui ne s'est pas passé comme prévu, rien d'autre (ni coups, ni échanges avec le moteur).
// Une ligne datée par fait, avec son niveau :
//     [ERREUR]      une erreur ou un plantage, avec son contexte et, pour une exception, sa pile d'appels
//     [ATTENTION]   un problème qui n'empêche pas de continuer (fichier illisible, moteur sans réponse, avertissement de lecture…)
//     [INFO]        un fait utile pour lire le reste : le démarrage de chaque session (version, système)
// Écrire dans le journal ne lève jamais d'exception ; au-delà de TailleMaximale, le fichier devient BrunoGUI_Dames.log.old (un seul ancien journal est gardé).
// Logique pure, sans Windows Forms.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BrunoGUI_Dames
{
    public static class Journal
    {
        public const long TailleMaximale = 1024 * 1024;         // 1 Mo
        private static readonly object verrou = new object();   // le moteur écrit depuis son propre thread

        // Le fichier du journal : à côté de l'exécutable (déploiement portable, comme le fichier de préférences) ; remplaçable pour les tests
        public static string Chemin { get; set; } = Path.Combine(AppContext.BaseDirectory, "BrunoGUI_Dames.log");

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

        // --- Une erreur avec sa cause : le contexte (ce que faisait le programme), le type et le message de l'exception, puis sa pile d'appels ---
        public static void Erreur(string contexte, Exception exception) =>
            Ecrire("ERREUR", $"{contexte} : {exception.GetType().Name} : {exception.Message}{Environment.NewLine}{exception.StackTrace}");

        // --- Une erreur qui n'est pas une exception (donnée impossible, état incohérent) ---
        public static void Erreur(string message) => Ecrire("ERREUR", message);

        // --- Un problème qui n'empêche pas de continuer ---
        public static void Attention(string message) => Ecrire("ATTENTION", message);

        // --- Un fait utile pour lire le journal (le démarrage) ---
        public static void Info(string message) => Ecrire("INFO", message);

        // --- Ajoute une ligne datée ; si le journal ne peut pas être écrit (dossier protégé, disque plein), on continue : il est une aide, pas une condition ---
        private static void Ecrire(string niveau, string texte)
        {
            string ligne = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{niveau}] {LigneSure(texte)}";
            Debug.WriteLine(ligne);
            try
            {
                lock (verrou)
                {
                    FileInfo fichier = new FileInfo(Chemin);
                    if (fichier.Exists && fichier.Length > TailleMaximale)
                        File.Move(Chemin, Chemin + ".old", overwrite: true);
                    File.AppendAllText(Chemin, ligne + Environment.NewLine);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
            {
                Debug.WriteLine("[Journal] Non écrit : " + ex.Message);
            }
        }
    }
}
