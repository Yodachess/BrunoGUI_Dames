// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Lecture de parties PDN (Portable Draughts Notation, https://pdn.fmjd.org/index.html) : logique pure, sans Windows Forms.
// Le texte est lu d'un seul passage, caractère par caractère, et non ligne par ligne : une balise peut être précédée de
// tabulations, plusieurs balises peuvent tenir sur une même ligne, les coups peuvent suivre les balises sur la même ligne, le
// numéro d'un coup peut être collé à celui-ci (« 12.33-29 »). Sont ignorés sans erreur : les commentaires entre accolades ou
// après « ; », les variantes entre parenthèses, les annotations (« $1 », « ! », « ?! ») et les « \r » écrits en toutes lettres
// (une barre oblique inverse suivie de la lettre r) que certains exports laissent entre les lignes.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BrunoGUI_Dames
{
    public static class LecturePdn
    {
        // Marqueurs de fin de partie : 2-0, 0-2 et 1-1 sont ceux des dames (gain blanc, gain noir, nulle), « - » celui d'un forfait
        private static readonly HashSet<string> Resultats = new HashSet<string>(StringComparer.Ordinal) { "1-0", "0-1", "1/2-1/2", "2-0", "0-2", "1-1", "*", "-" };
        private static readonly Regex NumeroDeCoup = new Regex(@"^\d+\.+", RegexOptions.Compiled);
        private static readonly Regex CoupPdn = new Regex(@"^\d{1,2}([-x]\d{1,2})+$", RegexOptions.Compiled);
        private static readonly Regex Nombre = new Regex(@"\d+", RegexOptions.Compiled);

        // --- Les parties d'un fichier PDN, en UTF-8 (ou en ISO-8859-1 si le fichier n'est pas de l'UTF-8 valide). ---
        public static List<PartiePdn> LireFichier(string chemin)
        {
            byte[] octets = File.ReadAllBytes(chemin);
            string texte;
            try { texte = new UTF8Encoding(false, true).GetString(octets); }
            catch (DecoderFallbackException) { texte = Encoding.Latin1.GetString(octets); }
            return LireTexte(texte);
        }

        // Les parties du texte, dans l'ordre. Une partie commence à sa première balise (ou, sans balise, à son premier coup) et finit à
        // son marqueur de résultat ou à la balise de la partie suivante. Un texte sans partie donne une liste vide.
        public static List<PartiePdn> LireTexte(string texte)
        {
            Lecture lecture = new Lecture();
            int i = 0;
            while (i < texte.Length)
            {
                int blanc = LongueurBlanc(texte, i);
                if (blanc > 0) { i += blanc; continue; }
                switch (texte[i])
                {
                    case '[':
                        if (lecture.Partie == null || lecture.CoupsLus || lecture.Terminee) lecture.NouvellePartie();
                        i = LireBalise(texte, i, lecture.Partie!);
                        continue;
                    case '{': i = SauteAccolades(texte, i); continue;
                    case ';': i = SauteLigne(texte, i); continue;
                    case '(': i = SauteVariante(texte, i); continue;
                    case ')':
                    case '}':
                    case ']': i++; continue;     // Fermants sans ouvrant : ignorés (sinon FinDuMot ne ferait pas avancer la lecture)
                    case '%' when i == 0 || texte[i - 1] == '\n': i = SauteLigne(texte, i); continue;
                }
                int debut = i;
                i = FinDuMot(texte, i);
                TraiteMot(texte.Substring(debut, i - debut), lecture);
            }
            return lecture.Parties;
        }

        private sealed class Lecture
        {
            public readonly List<PartiePdn> Parties = new List<PartiePdn>();
            public PartiePdn? Partie;
            public bool CoupsLus;       // La partie en cours a déjà au moins un coup : une balise qui suit est celle d'une nouvelle partie
            public bool Terminee;       // Le marqueur de résultat de la partie en cours a été lu

            public void NouvellePartie()
            {
                Partie = new PartiePdn();
                Parties.Add(Partie);
                CoupsLus = Terminee = false;
            }

            public PartiePdn PartiePourLesCoups()
            {
                if (Partie == null || Terminee) NouvellePartie();
                return Partie!;
            }
        }

        private static void TraiteMot(string mot, Lecture lecture)
        {
            Match numero = NumeroDeCoup.Match(mot);
            if (numero.Success)
            {   // « 12. », « 12... » ou « 12.33-29 » : on ne garde que ce qui suit le numéro
                mot = mot.Substring(numero.Length);
                if (mot.Length == 0) { lecture.PartiePourLesCoups(); return; }
            }
            mot = mot.TrimEnd('!', '?');            // Annotations « ! », « ?! »…
            if (mot.Length == 0 || mot[0] == '$') return;       // Annotation seule, ou NAG « $1 »

            if (Resultats.Contains(mot))
            {
                PartiePdn partie = lecture.Partie != null && !lecture.Terminee ? lecture.Partie : AvertitEtRetourne(lecture, mot);
                if (partie.Resultat == null) partie.Resultat = mot;
                lecture.Terminee = true;
                return;
            }
            if (CoupPdn.IsMatch(mot))
            {
                PartiePdn partie = lecture.PartiePourLesCoups();
                string normalise = Nombre.Replace(mot, m => int.Parse(m.Value).ToString());
                foreach (Match m in Nombre.Matches(mot))
                    if (int.Parse(m.Value) < 1 || int.Parse(m.Value) > 50)
                        partie.Avertissements.Add($"Case {m.Value} hors du damier dans le coup « {mot} »");
                partie.Coups.Add(normalise);
                lecture.CoupsLus = true;
                return;
            }
            lecture.Partie?.Avertissements.Add($"Jeton ignoré : « {mot} »");
        }

        private static PartiePdn AvertitEtRetourne(Lecture lecture, string resultat)
        {   // Un second marqueur de résultat après la fin d'une partie, ou un marqueur sans partie : on le range sur une partie vide
            lecture.NouvellePartie();
            lecture.Partie!.Avertissements.Add($"Résultat « {resultat} » sans coups ni balises");
            return lecture.Partie;
        }

        // ─── Balises ────────────────────────────────────────────────────────────────────────────────────────────────────

        private static int LireBalise(string texte, int i, PartiePdn partie)
        {   // i est sur le « [ » : [Nom "valeur"]
            int n = texte.Length;
            i++;
            i = SauteBlancs(texte, i);
            int debut = i;
            while (i < n && LongueurBlanc(texte, i) == 0 && texte[i] != '"' && texte[i] != ']') i++;
            string nom = texte.Substring(debut, i - debut);
            i = SauteBlancs(texte, i);

            string valeur = "";
            if (i < n && texte[i] == '"')
            {
                i++;
                StringBuilder valeurLue = new StringBuilder();
                while (i < n)
                {
                    char c = texte[i];
                    if (c == '"') { i++; break; }
                    if (c == '\n' || c == '\r') break;          // Guillemet fermant oublié : la valeur s'arrête à la fin de la ligne
                    if (c == '\\' && i + 1 < n && (texte[i + 1] == '"' || texte[i + 1] == '\\'))
                    {
                        valeurLue.Append(texte[i + 1]);
                        i += 2;
                        continue;
                    }
                    valeurLue.Append(c);
                    i++;
                }
                valeur = valeurLue.ToString();
            }
            while (i < n && texte[i] != ']' && texte[i] != '\n') i++;     // Jusqu'au « ] » fermant, sans déborder sur la ligne suivante
            if (i < n && texte[i] == ']') i++;
            if (nom.Length > 0) partie.AjouterBalise(nom, valeur);
            return i;
        }

        // ─── Caractères et parties du texte à ignorer ───────────────────────────────────────────────────────────────────

        // --- Longueur du blanc en position i : 1 pour un espace, une fin de ligne ou le BOM, 2 pour un « \r » écrit en toutes lettres, sinon 0. ---
        private static int LongueurBlanc(string texte, int i)
        {
            char c = texte[i];
            if (char.IsWhiteSpace(c) || c == '﻿') return 1;
            if (c == '\\' && i + 1 < texte.Length && texte[i + 1] == 'r') return 2;
            return 0;
        }

        private static int SauteBlancs(string texte, int i)
        {
            int longueur;
            while (i < texte.Length && (longueur = LongueurBlanc(texte, i)) > 0) i += longueur;
            return i;
        }

        private static int FinDuMot(string texte, int i)
        {
            while (i < texte.Length && LongueurBlanc(texte, i) == 0 && "{}()[];".IndexOf(texte[i]) < 0) i++;
            return i;
        }

        private static int SauteLigne(string texte, int i)
        {
            while (i < texte.Length && texte[i] != '\n' && !(texte[i] == '\\' && i + 1 < texte.Length && texte[i + 1] == 'r')) i++;
            return i;
        }

        private static int SauteAccolades(string texte, int i)
        {   // Les commentaires {…} ne s'imbriquent pas en PDN, mais on tolère l'imbrication
            int profondeur = 0;
            while (i < texte.Length)
            {
                if (texte[i] == '{') profondeur++;
                else if (texte[i] == '}' && --profondeur == 0) return i + 1;
                i++;
            }
            return i;
        }

        private static int SauteVariante(string texte, int i)
        {   // Variante (…) : peut contenir d'autres variantes et des commentaires {…} qui contiennent des parenthèses
            int profondeur = 0;
            while (i < texte.Length)
            {
                if (texte[i] == '{') { i = SauteAccolades(texte, i); continue; }
                if (texte[i] == '(') profondeur++;
                else if (texte[i] == ')' && --profondeur == 0) return i + 1;
                i++;
            }
            return i;
        }
    }
}
