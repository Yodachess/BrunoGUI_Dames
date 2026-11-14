// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Règles des dames internationales : génération des coups légaux et exécution d'un coup. Logique pure, sans Windows Forms
// ni Krypton, sans état statique : tout passe par la Position reçue, qui n'est jamais modifiée.
// Règles : http://www.ffjd.fr/Web/index.php?page=reglesdujeu

using System;
using System.Collections.Generic;
using System.Linq;

namespace BrunoGUI_Dames
{
    // Un coup légal : un déplacement simple ou une rafle (suite de prises). Pour une rafle, Chemin donne les cases où la
    // pièce se pose successivement (la dernière est Arrivee) et Prises les pièces prises, dans l'ordre de la rafle. La pièce
    // qui prend peut revenir sur sa case de départ : Depart et Arrivee sont alors égaux.
    public sealed class Coup
    {
        public int Depart { get; }
        public int Arrivee { get; }
        public IReadOnlyList<int> Chemin { get; }
        public IReadOnlyList<int> Prises { get; }
        public bool EstPrise => Prises.Count > 0;

        public Coup(int depart, int arrivee, IReadOnlyList<int> chemin, IReadOnlyList<int> prises)
        {
            Depart = depart;
            Arrivee = arrivee;
            Chemin = chemin;
            Prises = prises;
        }

        // --- Notation du protocole Hub2 : « 32-28 », ou « 28x19x23 » (de 28 vers 19, en prenant 23) ; les pièces prises, sans les pivots. ---
        public string Hub2() => EstPrise ? $"{Depart}x{Arrivee}x{string.Join("x", Prises)}" : $"{Depart}-{Arrivee}";

        // --- Notation PDN abrégée : « 32-28 », ou « 28x19 » (case de départ et case d'arrivée seulement). ---
        public string Pdn() => EstPrise ? $"{Depart}x{Arrivee}" : $"{Depart}-{Arrivee}";

        // --- Notation PDN complète d'une rafle : « 44x37x24 » (départ puis chaque case où la pièce se pose) ; pour un déplacement simple, comme Pdn(). ---
        // Elle départage deux rafles de même départ et même arrivée qui ne prennent pas les mêmes pièces.
        public string PdnComplet() => EstPrise ? $"{Depart}x{string.Join("x", Chemin)}" : Pdn();

        public override string ToString() => Hub2();
    }

    public static class ReglesDames
    {
        // Tous les coups légaux du camp au trait. Si une prise est possible elle est obligatoire (aucun déplacement simple) et
        // seules les rafles qui prennent le plus de pièces sont légales (prise majoritaire : une dame ne compte que pour une
        // pièce). Deux rafles qui partent de la même case, arrivent sur la même case et prennent les mêmes pièces ne font
        // qu'un seul coup, quel que soit l'ordre des prises.
        public static List<Coup> CoupsLegaux(Position position)
        {
            List<Coup> prises = Rafles(position);
            return prises.Count > 0 ? prises : Deplacements(position);
        }

        // --- La notation PDN à écrire pour ce coup parmi les coups légaux de sa position : l'abrégée « 28x19 », ou la complète « 28x37x19 » quand une autre rafle
        // légale part de la même case, arrive sur la même case et prend d'autres pièces (l'abrégée serait alors ambiguë à la relecture). ---
        public static string NotationPdn(IEnumerable<Coup> coupsLegaux, Coup coup)
        {
            bool ambigu = coup.EstPrise && coupsLegaux.Any(autre => autre.EstPrise && autre.Depart == coup.Depart && autre.Arrivee == coup.Arrivee
                                                                  && !autre.Prises.OrderBy(n => n).SequenceEqual(coup.Prises.OrderBy(n => n)));      // Une autre rafle, qui prend d'autres pièces
            return ambigu ? coup.PdnComplet() : coup.Pdn();
        }

        // --- Le camp au trait n'a plus aucun coup (plus de pièce, ou toutes bloquées) : il a perdu. ---
        public static bool EstTerminee(Position position) => CoupsLegaux(position).Count == 0;

        // La position obtenue en jouant ce coup, qui doit être l'un des coups légaux de la position (non vérifié). Une pièce
        // promue en dame l'est seulement si elle termine son coup sur la dernière rangée, jamais en la traversant.
        public static Position Jouer(Position position, Coup coup)
        {
            Position resultat = position.Copie();
            Piece piece = position[coup.Depart];
            foreach (int prise in coup.Prises)
                resultat[prise] = Piece.Vide;
            resultat[coup.Depart] = Piece.Vide;
            if (piece == Piece.PionBlanc && coup.Arrivee <= 5) piece = Piece.DameBlanche;
            else if (piece == Piece.PionNoir && coup.Arrivee >= 46) piece = Piece.DameNoire;
            resultat[coup.Arrivee] = piece;
            resultat.Trait = Position.Adversaire(position.Trait);
            return resultat;
        }

        // Le coup légal que désigne cette notation (« 32-28 », « 28x19 » ou « 28x19x23 »), ou null s'il n'y en a aucun, ou plusieurs
        // (une notation abrégée « 28x19 » peut être ambiguë : deux rafles de même départ et de même arrivée qui ne prennent pas les mêmes pièces).
        public static Coup? TrouverCoup(Position position, string notation)
        {
            string[] jetons = notation.Trim().Split('-', 'x');
            if (jetons.Length < 2) return null;
            int[] cases = new int[jetons.Length];
            for (int i = 0; i < jetons.Length; i++)
                if (!int.TryParse(jetons[i], out cases[i])) return null;
            bool prise = notation.Contains('x');
            IEnumerable<Coup> candidats = CoupsLegaux(position).Where(c => c.Depart == cases[0] && c.Arrivee == cases[1] && c.EstPrise == prise);
            if (prise && cases.Length > 2)
            {
                int[] attendues = cases.Skip(2).OrderBy(n => n).ToArray();
                candidats = candidats.Where(c => c.Prises.OrderBy(n => n).SequenceEqual(attendues));
            }
            List<Coup> trouves = candidats.ToList();
            return trouves.Count == 1 ? trouves[0] : null;
        }

        // Le coup légal que désigne cette notation PDN : « 32-28 » pour un déplacement, « 28x19 » (départ et arrivée) ou « 28x19x8 »
        // (les cases où la pièce se pose successivement) pour une rafle ; les zéros devant les numéros sont permis (« 05 »). Null s'il
        // n'y en a aucun, ou plusieurs (« 28x19 » peut désigner deux rafles qui ne prennent pas les mêmes pièces).
        public static Coup? TrouverCoupPdn(Position position, string notation)
        {
            string[] jetons = notation.Trim().Split('-', 'x');
            if (jetons.Length < 2) return null;
            int[] cases = new int[jetons.Length];
            for (int i = 0; i < jetons.Length; i++)
                if (!int.TryParse(jetons[i], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out cases[i])) return null;
            bool prise = notation.Contains('x');
            if (!prise && cases.Length != 2) return null;

            List<Coup> candidats = CoupsLegaux(position).Where(c => c.Depart == cases[0] && c.Arrivee == cases[^1] && c.EstPrise == prise).ToList();
            if (prise && cases.Length > 2)
            {   // Cases intermédiaires données (toutes, ou seulement quelques-unes comme « 15x33x31 » pour 15 > 33 > 22 > 31) : on garde
                // les coups dont le chemin les contient dans cet ordre ; si aucun ne convient, on retombe sur départ et arrivée seuls
                int[] citees = cases.Skip(1).ToArray();
                List<Coup> compatibles = candidats.Where(c => EstSousSuite(citees, c.Chemin)).ToList();
                if (compatibles.Count > 0) candidats = compatibles;
            }
            return candidats.Count == 1 ? candidats[0] : null;
        }

        // Vrai si tous les éléments de petite se retrouvent dans grande, dans le même ordre (pas forcément à la suite les uns des autres)
        private static bool EstSousSuite(IReadOnlyList<int> petite, IReadOnlyList<int> grande)
        {
            int suivant = 0;
            foreach (int c in grande)
                if (suivant < petite.Count && c == petite[suivant]) suivant++;
            return suivant == petite.Count;
        }

        // ─── Déplacements simples ───────────────────────────────────────────────────────────────────────────────────────

        private static List<Coup> Deplacements(Position position)
        {
            List<Coup> coups = new List<Coup>();
            for (int depart = 1; depart <= 50; depart++)
            {
                Piece piece = position[depart];
                if (!Position.Appartient(piece, position.Trait)) continue;
                if (Position.EstDame(piece))
                {   // La dame se déplace d'autant de cases vides qu'elle veut, dans les quatre directions
                    for (int direction = 0; direction < 4; direction++)
                    {
                        for (int arrivee = Geometrie.Voisin(depart, direction); arrivee != 0 && position[arrivee] == Piece.Vide; arrivee = Geometrie.Voisin(arrivee, direction))
                            coups.Add(new Coup(depart, arrivee, new[] { arrivee }, Array.Empty<int>()));
                    }
                }
                else
                {   // Le pion avance d'une case, vers l'avant seulement (vers la case 1 pour les blancs, vers la case 50 pour les noirs)
                    int premiere = position.Trait == Couleur.Blanc ? Geometrie.NO : Geometrie.SO;
                    for (int direction = premiere; direction < premiere + 2; direction++)
                    {
                        int arrivee = Geometrie.Voisin(depart, direction);
                        if (arrivee != 0 && position[arrivee] == Piece.Vide)
                            coups.Add(new Coup(depart, arrivee, new[] { arrivee }, Array.Empty<int>()));
                    }
                }
            }
            return coups;
        }

        // ─── Rafles ─────────────────────────────────────────────────────────────────────────────────────────────────────

        private sealed class Exploration
        {   // État d'une recherche de rafle pour UNE pièce
            public Piece[] Cases = Array.Empty<Piece>();           // Copie du damier, où la pièce qui prend a quitté sa case de départ
            public readonly bool[] Prise = new bool[51];     // Pièces déjà prises dans cette rafle : elles restent sur le damier jusqu'à la fin
            public Couleur Trait;
            public bool Dame;
            public int Depart;
            public readonly List<int> Chemin = new List<int>();
            public readonly List<int> Prises = new List<int>();
            public readonly List<Coup> Resultats = new List<Coup>();
        }

        private static List<Coup> Rafles(Position position)
        {
            List<Coup> toutes = new List<Coup>();
            Piece[] cases = position.CopieCases();
            for (int depart = 1; depart <= 50; depart++)
            {
                Piece piece = cases[depart];
                if (!Position.Appartient(piece, position.Trait)) continue;
                cases[depart] = Piece.Vide;     // La pièce qui prend quitte sa case : elle peut la traverser ou s'y poser en fin de rafle
                Exploration exploration = new Exploration { Cases = cases, Trait = position.Trait, Dame = Position.EstDame(piece), Depart = depart };
                Explorer(exploration, depart);
                cases[depart] = piece;
                toutes.AddRange(exploration.Resultats);
            }
            if (toutes.Count == 0) return toutes;

            int maximum = toutes.Max(coup => coup.Prises.Count);
            HashSet<(int, int, ulong)> vus = new HashSet<(int, int, ulong)>();
            List<Coup> retenues = new List<Coup>();
            foreach (Coup coup in toutes)
            {
                if (coup.Prises.Count != maximum) continue;
                ulong masque = 0;
                foreach (int prise in coup.Prises) masque |= 1UL << prise;
                if (vus.Add((coup.Depart, coup.Arrivee, masque))) retenues.Add(coup);
            }
            return retenues;
        }

        // Explore les prises possibles depuis la case courante et enregistre dans Resultats les rafles terminées (celles qu'on ne
        // peut plus prolonger). Renvoie vrai si au moins une prise est possible depuis cette case.
        private static bool Explorer(Exploration e, int courante)
        {
            bool prisePossible = false;
            for (int direction = 0; direction < 4; direction++)
            {
                int cible = Geometrie.Voisin(courante, direction);
                if (e.Dame)
                {   // La dame glisse sur les cases vides jusqu'à la première pièce rencontrée
                    while (cible != 0 && e.Cases[cible] == Piece.Vide)
                        cible = Geometrie.Voisin(cible, direction);
                }
                // On ne prend qu'une pièce adverse qui n'a pas déjà été prise dans cette rafle : une pièce prise reste sur le damier
                // jusqu'à la fin de la rafle et bloque le passage, on ne saute jamais deux fois par-dessus
                if (cible == 0 || e.Prise[cible] || !Position.Appartient(e.Cases[cible], Position.Adversaire(e.Trait))) continue;

                // La case juste derrière doit être vide ; la dame peut se poser sur n'importe quelle case vide qui suit
                for (int arrivee = Geometrie.Voisin(cible, direction); arrivee != 0 && e.Cases[arrivee] == Piece.Vide; arrivee = Geometrie.Voisin(arrivee, direction))
                {
                    prisePossible = true;
                    e.Prise[cible] = true;
                    e.Prises.Add(cible);
                    e.Chemin.Add(arrivee);
                    if (!Explorer(e, arrivee))
                        e.Resultats.Add(new Coup(e.Depart, arrivee, e.Chemin.ToArray(), e.Prises.ToArray()));
                    e.Chemin.RemoveAt(e.Chemin.Count - 1);
                    e.Prises.RemoveAt(e.Prises.Count - 1);
                    e.Prise[cible] = false;
                    if (!e.Dame) break;
                }
            }
            return prisePossible;
        }
    }
}
