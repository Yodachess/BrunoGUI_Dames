// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Étape 1 du branchement de l'interface sur ReglesDames : comparaison, en lecture seule, des rafles que trouve l'ancien générateur
// (LogiqueMouvementsDames) avec celles de ReglesDames pour la même position. Logique pure, sans Windows Forms.
// Provisoire : ce fichier disparaît quand l'interface n'utilise plus que ReglesDames.

using System;
using System.Collections.Generic;
using System.Linq;

namespace BrunoGUI_Dames
{
    public static class ComparaisonRegles
    {
        // --- Compare les rafles de l'ancien générateur (départ, arrivée finale, cases des pièces prises) avec les prises légales de ReglesDames ---
        // Deux rafles sont les mêmes si elles ont le même départ, la même arrivée et les mêmes pièces prises, quel que soit l'ordre des prises.
        // Les déplacements simples ne sont pas comparés. Detail est vide quand les deux générateurs sont d'accord.
        // Doublons : nombre de rafles que l'ancien générateur donne en plus d'exemplaires identiques (ReglesDames n'en garde qu'un).
        public static (bool Identiques, int Doublons, string Detail) Comparer(Position position, IEnumerable<(int Depart, int Arrivee, IReadOnlyList<int> Prises)> anciennes)
        {
            List<string> listeAnciennes = anciennes.Select(r => Signature(r.Depart, r.Arrivee, r.Prises)).ToList();
            HashSet<string> ensembleAnciennes = listeAnciennes.ToHashSet();
            HashSet<string> ensembleNouvelles = ReglesDames.CoupsLegaux(position).Where(c => c.EstPrise).Select(c => Signature(c.Depart, c.Arrivee, c.Prises)).ToHashSet();
            int doublons = listeAnciennes.Count - ensembleAnciennes.Count;
            if (ensembleAnciennes.SetEquals(ensembleNouvelles)) return (true, doublons, "");

            List<string> seulementAncien = ensembleAnciennes.Except(ensembleNouvelles).OrderBy(s => s).ToList();
            List<string> seulementNouveau = ensembleNouvelles.Except(ensembleAnciennes).OrderBy(s => s).ToList();
            string detail = "ancien seul : [" + string.Join(" ", seulementAncien) + "] ; ReglesDames seul : [" + string.Join(" ", seulementNouveau) + "]";
            return (false, doublons, detail);
        }

        // --- La rafle sous forme de texte « départ x arrivée x prises triées » : « 29x45x40 » ---
        private static string Signature(int depart, int arrivee, IEnumerable<int> prises)
        {
            return depart + "x" + arrivee + "x" + string.Join("x", prises.OrderBy(n => n));
        }
    }
}
