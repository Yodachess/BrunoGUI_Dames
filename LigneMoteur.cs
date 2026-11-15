// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Lecture d'une ligne que le moteur Scan envoie (protocole Hub2, Docs\Protocole_Hub2.txt) : ce que le formulaire affiche (identité du moteur, score,
// variation principale). Logique pure, sans Windows Forms : une ligne qu'on ne sait pas lire ne lève jamais d'exception, elle ne donne simplement rien.
// Le coup choisi (« done move=… ») n'est pas lu ici : c'est MoteurDamesScan qui le traite.

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BrunoGUI_Dames
{
    // --- Ce qu'une ligne du moteur apporte à l'affichage (les champs absents de la ligne sont null) ---
    public sealed record LigneMoteurLue(bool EstId, string? Nom, string? Version, string? Auteur, string? Pays, decimal? Score, string? Variation);

    public static class LigneMoteur
    {
        private static readonly Regex ChampsDeId = new Regex(@"name=(\S+)|version=(\S+)|author=""([^""]+)""|country=(\S+)");

        // --- Les informations d'affichage d'une ligne : « id name=Scan version=3.1 author="…" country=… » donne l'identité ;
        // « info depth=21 score=-0.01 … pv="14x23x19 50-44 …" » donne le score et la variation principale (sans les guillemets) ---
        public static LigneMoteurLue Lire(string ligne)
        {
            LigneMoteurLue rien = new LigneMoteurLue(false, null, null, null, null, null, null);
            string[] mots = ligne.Split(' ');
            if (string.IsNullOrWhiteSpace(mots[0]) || mots.Length <= 2)
                return rien;
            if (mots[0] == "id")
            {
                string? nom = null, version = null, auteur = null, pays = null;
                foreach (Match champ in ChampsDeId.Matches(ligne))
                {
                    if (champ.Groups[1].Success) nom = champ.Groups[1].Value;
                    if (champ.Groups[2].Success) version = champ.Groups[2].Value;
                    if (champ.Groups[3].Success) auteur = champ.Groups[3].Value;
                    if (champ.Groups[4].Success) pays = champ.Groups[4].Value;
                }
                return new LigneMoteurLue(true, nom, version, auteur, pays, null, null);
            }
            if (mots[0] != "info")
                return rien;
            decimal? score = null;
            string? variation = null;
            for (int i = 1; i < mots.Length; i++)
            {
                string[] nomEtValeur = mots[i].Split('=');
                if (nomEtValeur[0] == "score" && nomEtValeur.Length > 1 && decimal.TryParse(nomEtValeur[1], NumberStyles.Float, CultureInfo.InvariantCulture, out decimal valeur))
                    score = valeur;
                else if (nomEtValeur[0] == "pv" && variation == null)
                {
                    int debut = ligne.IndexOf(" pv", StringComparison.Ordinal) + 4;       // « pv=" » : le texte commence après le guillemet d'ouverture
                    if (debut >= 4 && debut <= ligne.Length)
                        variation = ligne.Substring(debut).Trim('"');
                }
            }
            return new LigneMoteurLue(false, null, null, null, null, score, variation);
        }
    }
}
