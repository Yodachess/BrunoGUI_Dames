// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// La langue de l'affichage. Les sources sont en français : le texte français est la clé, et l'anglais ne se trouve que dans le fichier Langues\en.tsv (une ligne
// « français TAB anglais » ; \n pour un retour à la ligne, \\ pour une barre oblique inverse ; les lignes qui commencent par # sont des commentaires). T(« texte français »)
// rend ce texte en français, sa traduction en anglais ; une clé absente du fichier reste en français et est notée dans ClesManquantes (un test vérifie que
// toutes les clés des sources sont traduites). Un texte avec des valeurs s'écrit avec {0}, {1}… : l'ordre des mots dépend de la langue, donc la phrase entière est la clé.
// Logique pure, sans Windows Forms.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace BrunoGUI_Dames
{
    public static class Texte
    {
        public const string Francais = "fr";
        public const string Anglais = "en";
        private static Dictionary<string, string> traductions = new Dictionary<string, string>();
        private static readonly HashSet<string> clesManquantes = new HashSet<string>();
        private static readonly object verrou = new object();

        // --- Le fichier de traductions donné à Definit (la fenêtre « À propos » s'en sert pour changer de langue sous les yeux du joueur) ---
        public static string? FichierTraductions { get; private set; }

        // --- La langue en cours (« fr » au départ) ---
        public static string Langue { get; private set; } = Francais;

        // --- La culture des nombres et dates écrits à l'écran : celle de la langue ---
        public static CultureInfo Culture => Langue == Anglais ? CultureInfo.GetCultureInfo("en-US") : CultureInfo.GetCultureInfo("fr-FR");

        // --- Les clés demandées en anglais qui ne sont pas dans le fichier (pour les tests) ---
        public static IReadOnlyCollection<string> ClesManquantes
        {
            get { lock (verrou) return new List<string>(clesManquantes); }
        }

        // --- Choisit la langue (« fr » ou « en » ; autre chose : français) ; pour l'anglais, charge le fichier de traductions s'il existe (sinon tout reste en français, c'est noté dans le journal) ---
        public static void Definit(string? langue, string? fichierTraductions)
        {
            lock (verrou)
            {
                clesManquantes.Clear();
                FichierTraductions = fichierTraductions;
                if (string.Equals(langue, Anglais, StringComparison.OrdinalIgnoreCase) && fichierTraductions != null && File.Exists(fichierTraductions))
                {
                    traductions = Lire(fichierTraductions);
                    Langue = Anglais;
                }
                else
                {
                    if (string.Equals(langue, Anglais, StringComparison.OrdinalIgnoreCase))
                        Journal.Attention("Traductions anglaises introuvables : l'affichage reste en français (" + fichierTraductions + ")");
                    traductions = new Dictionary<string, string>();
                    Langue = Francais;
                }
            }
        }

        // --- La langue qui convient à Windows : l'anglais si l'interface de Windows est en anglais, sinon le français ---
        public static string LangueDuSysteme() => string.Equals(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "en", StringComparison.OrdinalIgnoreCase) ? Anglais : Francais;

        // --- Le fichier de traductions : clé française, tabulation, anglais ---
        public static Dictionary<string, string> Lire(string fichier)
        {
            Dictionary<string, string> lues = new Dictionary<string, string>();
            foreach (string ligneBrute in File.ReadAllLines(fichier, new UTF8Encoding(false)))
            {
                string ligne = ligneBrute.TrimEnd('\r').TrimStart((char)0xFEFF);
                if (ligne.Length == 0 || ligne[0] == '#')
                    continue;
                int tabulation = ligne.IndexOf('\t');
                if (tabulation <= 0)
                {
                    Journal.Attention("Traductions : ligne ignorée (pas de tabulation) : " + ligne);
                    continue;
                }
                lues[Decode(ligne.Substring(0, tabulation))] = Decode(ligne.Substring(tabulation + 1));
            }
            return lues;
        }

        // --- \n devient un retour à la ligne, \\ une barre oblique inverse ---
        private static string Decode(string texte)
        {
            if (texte.IndexOf('\\') < 0)
                return texte;
            StringBuilder resultat = new StringBuilder();
            for (int i = 0; i < texte.Length; i++)
            {
                if (texte[i] == '\\' && i + 1 < texte.Length && (texte[i + 1] == 'n' || texte[i + 1] == '\\'))
                {
                    resultat.Append(texte[i + 1] == 'n' ? '\n' : '\\');
                    i++;
                }
                else
                    resultat.Append(texte[i]);
            }
            return resultat.ToString();
        }

        // --- Le texte dans la langue en cours ; le texte vide ou sans lettre (« 1-0 », « 12 ») n'est pas traduit ---
        public static string T(string francais)
        {
            if (Langue == Francais || francais.Length == 0)
                return francais;
            lock (verrou)
            {
                if (traductions.TryGetValue(francais, out string? anglais))
                    return anglais;
                if (AUneLettre(francais))
                    clesManquantes.Add(francais);
            }
            return francais;
        }

        // --- Le texte dans la langue en cours, avec ses valeurs (« Coup {0} : {1} ») ---
        public static string T(string francais, params object?[] valeurs) => string.Format(Culture, T(francais), valeurs);

        private static bool AUneLettre(string texte)
        {
            foreach (char c in texte)
                if (char.IsLetter(c))
                    return true;
            return false;
        }
    }
}
