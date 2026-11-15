// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Les préférences personnelles (temps de réflexion, noms des joueurs, son, ordinateur qui joue, durée d'animation, sens du damier,
// couleurs du damier), enregistrées dans un fichier texte
// « Cle=Valeur » à côté de l'exécutable et relues au démarrage. Logique pure, sans Windows Forms : une valeur absente, illisible ou
// hors limites ne fait jamais d'erreur, elle est signalée dans Avertissements et la valeur par défaut est gardée.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BrunoGUI_Dames
{
    public sealed class Preferences
    {
        public int TempsReflexion { get; set; } = 5;                    // Secondes de réflexion du moteur
        public string? NomJoueurBlanc { get; set; }                     // Null : on garde le nom par défaut de l'application
        public string? NomJoueurNoir { get; set; }
        public bool SonEmis { get; set; }                               // Un son quand le moteur a joué
        public bool OrdinateurJoue { get; set; } = true;                // Le moteur répond aux coups du joueur
        public int DureeEtape { get; set; } = 250;                     // Millisecondes d'une étape d'animation : un glissement de pièce, une étape de rafle (0 à 10000 ; 0 : sans animation). Les versions 1.10 et 1.11 avaient 900
        public int VitesseLecture { get; set; } = RythmeLecture.ParDefaut;      // La vitesse de la lecture d'une partie, de 1 (lente) à 5 (rapide)
        public bool VueCoteNoir { get; set; }                           // Le damier est vu du côté des noirs
        public string? CadenceId { get; set; }                          // La cadence de la pendule (Id d'une Cadence) ; null : pas de pendule
        public static readonly string[] NomsCouleurs = { "CaseSombre", "CaseClaire", "CaseDepart", "TrajetSuivi", "CasePivot", "CasePrise", "CaseArrivee", "CaseEtape" };
        // --- Les couleurs par défaut du damier (0xRRGGBB) : cases sombres et claires (bois), puis les surbrillances (celles des versions 1.10 et 1.11, gardées : une palette
        // pour daltoniens essayée le 2026-11-15 n'a pas plu) ---
        public static readonly IReadOnlyDictionary<string, int> CouleursParDefaut = new Dictionary<string, int>
        {
            ["CaseSombre"] = 0xCD853F, ["CaseClaire"] = 0xDEB887, ["CaseDepart"] = 0x00FF7F, ["TrajetSuivi"] = 0x9ACD32,
            ["CasePivot"] = 0xBDB76B, ["CasePrise"] = 0x556B2F, ["CaseArrivee"] = 0x6B8E23, ["CaseEtape"] = 0xFFA500
        };
        public Dictionary<string, int> Couleurs { get; } = new Dictionary<string, int>();   // Couleurs du damier changées ou à enregistrer : nom (de NomsCouleurs) -> 0xRRGGBB ; absent = couleur par défaut
        public List<string> Avertissements { get; } = new List<string>();   // Ce que la lecture n'a pas compris (lignes ignorées, valeurs refusées)

        // --- Lit le fichier ; s'il n'existe pas ou ne peut pas être lu, toutes les valeurs sont celles par défaut (l'illisibilité est notée dans Avertissements) ---
        public static Preferences Lire(string fichier)
        {
            if (!File.Exists(fichier))
                return new Preferences();
            try
            {
                return DepuisTexte(File.ReadAllText(fichier, Encoding.UTF8));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Preferences parDefaut = new Preferences();
                parDefaut.Avertissements.Add("Fichier de préférences illisible : " + ex.Message);
                return parDefaut;
            }
        }

        // --- Les préférences d'un texte « Cle=Valeur » (une par ligne ; les lignes vides, celles qui commencent par « ; » ou « # » et les sections « [ ... ] » sont ignorées) ---
        public static Preferences DepuisTexte(string texte)
        {
            Preferences preferences = new Preferences();
            foreach (string ligneBrute in texte.Split('\n'))
            {
                string ligne = ligneBrute.Trim('\r', ' ', '\t', (char)0xFEFF);
                if (ligne.Length == 0 || ligne[0] == ';' || ligne[0] == '#' || ligne[0] == '[')
                    continue;
                int egal = ligne.IndexOf('=');
                if (egal <= 0)
                {
                    preferences.Avertissements.Add($"Ligne ignorée (pas de « Cle=Valeur ») : {ligne}");
                    continue;
                }
                string cle = ligne.Substring(0, egal).Trim();
                string valeur = ligne.Substring(egal + 1).Trim();
                switch (cle.ToLowerInvariant())
                {
                    case "tempsreflexion":
                        if (int.TryParse(valeur, NumberStyles.None, CultureInfo.InvariantCulture, out int secondes) && secondes >= 1 && secondes <= 3600)
                            preferences.TempsReflexion = secondes;
                        else
                            preferences.Avertissements.Add($"TempsReflexion refusé (un nombre de secondes de 1 à 3600 est attendu) : {valeur}");
                        break;
                    case "nomjoueurblanc":
                        preferences.NomJoueurBlanc = valeur.Length > 0 ? valeur : null;
                        break;
                    case "nomjoueurnoir":
                        preferences.NomJoueurNoir = valeur.Length > 0 ? valeur : null;
                        break;
                    case "sonemis":
                        if (LitBooleen(valeur, out bool son)) preferences.SonEmis = son;
                        else preferences.Avertissements.Add($"SonEmis refusé (0 ou 1 attendu) : {valeur}");
                        break;
                    case "ordinateurjoue":
                        if (LitBooleen(valeur, out bool ordinateur)) preferences.OrdinateurJoue = ordinateur;
                        else preferences.Avertissements.Add($"OrdinateurJoue refusé (0 ou 1 attendu) : {valeur}");
                        break;
                    case "dureeanimation":
                        break;      // Clé des versions 1.10 et 1.11 (900 ms par attente, trop lent) : ignorée sans avertissement, la clé DureeEtape la remplace
                    case "vitesselecture":
                        if (int.TryParse(valeur, NumberStyles.None, CultureInfo.InvariantCulture, out int vitesse) && vitesse >= RythmeLecture.Minimum && vitesse <= RythmeLecture.Maximum)
                            preferences.VitesseLecture = vitesse;
                        else
                            preferences.Avertissements.Add($"VitesseLecture refusée (un nombre de {RythmeLecture.Minimum} à {RythmeLecture.Maximum} est attendu) : {valeur}");
                        break;
                    case "dureeetape":
                        if (int.TryParse(valeur, NumberStyles.None, CultureInfo.InvariantCulture, out int millisecondes) && millisecondes <= 10000)
                            preferences.DureeEtape = millisecondes;
                        else
                            preferences.Avertissements.Add($"DureeEtape refusée (un nombre de millisecondes de 0 à 10000 est attendu) : {valeur}");
                        break;
                    case "vuecotenoir":
                        if (LitBooleen(valeur, out bool vueNoire)) preferences.VueCoteNoir = vueNoire;
                        else preferences.Avertissements.Add($"VueCoteNoir refusée (0 ou 1 attendu) : {valeur}");
                        break;
                    case "cadence":
                        if (valeur.Length == 0 || string.Equals(valeur, "sans", StringComparison.OrdinalIgnoreCase))
                            preferences.CadenceId = null;
                        else if (Cadence.Trouve(valeur) is Cadence cadence)
                            preferences.CadenceId = cadence.Id;
                        else
                            preferences.Avertissements.Add($"Cadence refusée (« sans » ou un des noms : {string.Join(", ", Cadence.Officielles.Select(c => c.Id))}) : {valeur}");
                        break;
                    default:
                        string? nomCouleur = cle.StartsWith("Couleur", StringComparison.OrdinalIgnoreCase)
                            ? Array.Find(NomsCouleurs, nom => string.Equals("Couleur" + nom, cle, StringComparison.OrdinalIgnoreCase)) : null;
                        if (nomCouleur == null)
                            preferences.Avertissements.Add($"Clé inconnue ignorée : {cle}");
                        else if (LitCouleur(valeur, out int rgb))
                        {
                            preferences.Couleurs[nomCouleur] = rgb;
                        }
                        else
                            preferences.Avertissements.Add($"{cle} refusée (une couleur « #RRGGBB » est attendue) : {valeur}");
                        break;
                }
            }
            return preferences;
        }

        // --- Une couleur « #RRGGBB » (le « # » est facultatif) ---
        private static bool LitCouleur(string valeur, out int rgb)
        {
            string chiffres = valeur.StartsWith('#') ? valeur.Substring(1) : valeur;
            if (chiffres.Length == 6 && int.TryParse(chiffres, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out rgb))
                return true;
            rgb = 0;
            return false;
        }

        private static bool LitBooleen(string valeur, out bool resultat)
        {
            switch (valeur.ToLowerInvariant())
            {
                case "1": case "true": case "oui": resultat = true; return true;
                case "0": case "false": case "non": resultat = false; return true;
                default: resultat = false; return false;
            }
        }

        // --- Le texte du fichier : une ligne « Cle=Valeur » par préférence (les noms vides ou nuls ne sont pas écrits) ---
        public string VersTexte()
        {
            StringBuilder texte = new StringBuilder();
            texte.Append("; Préférences personnelles de BrunoGUI_Dames (écrit à la fermeture, relu au démarrage ; les couleurs s'écrivent #RRGGBB)").Append(Environment.NewLine);
            texte.Append("TempsReflexion=").Append(TempsReflexion.ToString(CultureInfo.InvariantCulture)).Append(Environment.NewLine);
            if (!string.IsNullOrWhiteSpace(NomJoueurBlanc)) texte.Append("NomJoueurBlanc=").Append(UneLigne(NomJoueurBlanc)).Append(Environment.NewLine);
            if (!string.IsNullOrWhiteSpace(NomJoueurNoir)) texte.Append("NomJoueurNoir=").Append(UneLigne(NomJoueurNoir)).Append(Environment.NewLine);
            texte.Append("SonEmis=").Append(SonEmis ? "1" : "0").Append(Environment.NewLine);
            texte.Append("OrdinateurJoue=").Append(OrdinateurJoue ? "1" : "0").Append(Environment.NewLine);
            texte.Append("DureeEtape=").Append(DureeEtape.ToString(CultureInfo.InvariantCulture)).Append(Environment.NewLine);
            texte.Append("VitesseLecture=").Append(VitesseLecture.ToString(CultureInfo.InvariantCulture)).Append(Environment.NewLine);
            texte.Append("VueCoteNoir=").Append(VueCoteNoir ? "1" : "0").Append(Environment.NewLine);
            texte.Append("Cadence=").Append(CadenceId ?? "sans").Append(Environment.NewLine);
            foreach (string nom in NomsCouleurs)
                if (Couleurs.TryGetValue(nom, out int rgb))
                    texte.Append("Couleur").Append(nom).Append("=#").Append((rgb & 0xFFFFFF).ToString("X6", CultureInfo.InvariantCulture)).Append(Environment.NewLine);
            return texte.ToString();
        }

        private static string UneLigne(string valeur) => valeur.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ').Trim();

        // --- Écrit le fichier (UTF-8) ; renvoie faux, en le notant dans le journal, si le dossier n'est pas inscriptible ---
        public bool Ecrire(string fichier)
        {
            try
            {
                File.WriteAllText(fichier, VersTexte(), new UTF8Encoding(false));
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Journal.Ecrit("Préférences non enregistrées : " + ex.Message);
                return false;
            }
        }
    }
}
