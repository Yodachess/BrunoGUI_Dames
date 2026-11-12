// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Position du jeu de dames internationales (damier 10x10, notation Manoury) : logique pure, sans Windows Forms ni Krypton,
// sans état statique. Une position se copie (Copie) et les calculs « pour voir » se font sur la copie, jamais sur la partie.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BrunoGUI_Dames
{
    public enum Couleur { Blanc, Noir }

    public enum Piece : byte { Vide, PionBlanc, PionNoir, DameBlanche, DameNoire }

    /// <summary>
    /// Géométrie du damier : les 50 cases jouables, numérotées de 1 à 50 (notation Manoury) en partant du haut à gauche,
    /// vues du côté des blancs. Les noirs occupent les cases 1 à 20 au départ, les blancs les cases 31 à 50.
    /// </summary>
    public static class Geometrie
    {
        //   Ligne 0 :   .  01  .  02  .  03  .  04  .  05      (les cases jouables sont en colonnes 1, 3, 5, 7, 9)
        //   Ligne 1 :  06  .  07  .  08  .  09  .  10  .       (colonnes 0, 2, 4, 6, 8)
        //   ... et ainsi de suite jusqu'à la ligne 9 (cases 46 à 50).
        public const int NO = 0, NE = 1, SO = 2, SE = 3;        // Les quatre directions diagonales (vers le haut = vers la case 1)

        private static readonly int[] voisins = new int[51 * 4];

        static Geometrie()
        {
            for (int c = 1; c <= 50; c++)
            {
                (int ligne, int colonne) = Coordonnees(c);
                for (int direction = 0; direction < 4; direction++)
                {
                    int nouvelleLigne = ligne + (direction < 2 ? -1 : 1);
                    int nouvelleColonne = colonne + (direction % 2 == 0 ? -1 : 1);
                    bool dansLeDamier = nouvelleLigne >= 0 && nouvelleLigne < 10 && nouvelleColonne >= 0 && nouvelleColonne < 10;
                    voisins[c * 4 + direction] = dansLeDamier ? Numero(nouvelleLigne, nouvelleColonne) : 0;
                }
            }
        }

        /// <summary>Ligne (0 en haut) et colonne (0 à gauche) de la case Manoury c, sur le damier de 10 x 10.</summary>
        public static (int ligne, int colonne) Coordonnees(int c)
        {
            int ligne = (c - 1) / 5;
            return (ligne, 2 * ((c - 1) % 5) + (ligne % 2 == 0 ? 1 : 0));
        }

        /// <summary>Numéro Manoury de la case jouable située à (ligne, colonne).</summary>
        public static int Numero(int ligne, int colonne) => ligne * 5 + (colonne - (ligne % 2 == 0 ? 1 : 0)) / 2 + 1;

        /// <summary>Case voisine de c dans la direction donnée (NO, NE, SO, SE), ou 0 si on sort du damier.</summary>
        public static int Voisin(int c, int direction) => voisins[c * 4 + direction];
    }

    public sealed class Position : IEquatable<Position>
    {
        private readonly Piece[] cases = new Piece[51];     // Cases 1 à 50 (l'indice 0 n'est pas utilisé)

        public Couleur Trait { get; set; } = Couleur.Blanc;

        public Piece this[int c]
        {
            get { VerifieCase(c); return cases[c]; }
            set { VerifieCase(c); cases[c] = value; }
        }

        public static bool EstBlanche(Piece piece) => piece == Piece.PionBlanc || piece == Piece.DameBlanche;
        public static bool EstNoire(Piece piece) => piece == Piece.PionNoir || piece == Piece.DameNoire;
        public static bool EstDame(Piece piece) => piece == Piece.DameBlanche || piece == Piece.DameNoire;
        public static bool Appartient(Piece piece, Couleur couleur) => couleur == Couleur.Blanc ? EstBlanche(piece) : EstNoire(piece);
        public static Couleur Adversaire(Couleur couleur) => couleur == Couleur.Blanc ? Couleur.Noir : Couleur.Blanc;

        private static void VerifieCase(int c)
        {
            if (c < 1 || c > 50) throw new ArgumentOutOfRangeException(nameof(c), c, "Une case Manoury est comprise entre 1 et 50.");
        }

        public Position Copie()
        {
            Position copie = new Position { Trait = Trait };
            Array.Copy(cases, copie.cases, cases.Length);
            return copie;
        }

        /// <summary>Copie du tableau des 51 cases (l'indice 0 est inutilisé), pour les calculs internes de la génération des coups.</summary>
        internal Piece[] CopieCases() => (Piece[])cases.Clone();

        public int Compte(Piece piece)
        {
            int nombre = 0;
            for (int c = 1; c <= 50; c++)
                if (cases[c] == piece) nombre++;
            return nombre;
        }

        /// <summary>Position de départ : 20 pions noirs sur les cases 1 à 20, 20 pions blancs sur les cases 31 à 50, trait aux blancs.</summary>
        public static Position Initiale() => DepuisFen("W:W31-50:B1-20");

        // ─── FEN (https://pdn.fmjd.org/fen.html) ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Lit une position FEN : « W:W31-50:B1-20 », avec ou sans l'enrobage [FEN "..."]. Les dames sont préfixées par K
        /// (« K10 »), les suites de cases s'écrivent « 31-50 ». Lève FormatException si le texte n'est pas une position valide.
        /// </summary>
        public static Position DepuisFen(string fen)
        {
            string texte = fen.Trim();
            int debut = texte.IndexOf('"');
            if (debut >= 0)
            {   // Enrobage [FEN "..."] : on ne garde que ce qui est entre les guillemets
                int fin = texte.IndexOf('"', debut + 1);
                if (fin < 0) throw new FormatException($"FEN invalide (guillemet non fermé) : {fen}");
                texte = texte.Substring(debut + 1, fin - debut - 1);
            }
            texte = texte.Trim().TrimEnd('.');
            string[] champs = texte.Split(':');
            if (champs.Length < 2) throw new FormatException($"FEN invalide : {fen}");

            Position position = new Position();
            string trait = champs[0].Trim();
            if (trait == "W") position.Trait = Couleur.Blanc;
            else if (trait == "B") position.Trait = Couleur.Noir;
            else throw new FormatException($"FEN invalide (trait « {trait} », attendu W ou B) : {fen}");

            for (int i = 1; i < champs.Length; i++)
            {
                string champ = champs[i].Trim();
                if (champ.Length == 0) continue;
                char lettre = champ[0];
                if (lettre != 'W' && lettre != 'B') throw new FormatException($"FEN invalide (couleur « {lettre} », attendu W ou B) : {fen}");
                foreach (string element in champ.Substring(1).Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    string texteElement = element.Trim();
                    bool dame = texteElement.StartsWith('K');
                    if (dame) texteElement = texteElement.Substring(1);
                    string[] bornes = texteElement.Split('-');
                    if (bornes.Length > 2) throw new FormatException($"FEN invalide (« {element} ») : {fen}");
                    int premiere = LitCase(bornes[0], fen);
                    int derniere = bornes.Length == 2 ? LitCase(bornes[1], fen) : premiere;
                    if (derniere < premiere) throw new FormatException($"FEN invalide (suite « {element} » à l'envers) : {fen}");
                    for (int c = premiere; c <= derniere; c++)
                    {
                        if (position.cases[c] != Piece.Vide) throw new FormatException($"FEN invalide (case {c} occupée deux fois) : {fen}");
                        position.cases[c] = lettre == 'W' ? (dame ? Piece.DameBlanche : Piece.PionBlanc) : (dame ? Piece.DameNoire : Piece.PionNoir);
                    }
                }
            }
            return position;
        }

        private static int LitCase(string texte, string fen)
        {
            if (!int.TryParse(texte.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int c) || c < 1 || c > 50)
                throw new FormatException($"FEN invalide (case « {texte} » : un nombre de 1 à 50 est attendu) : {fen}");
            return c;
        }

        /// <summary>La position en FEN, sans l'enrobage [FEN "..."] : « W:W31,32,...:B1,2,... », dames préfixées par K.</summary>
        public string VersFen()
        {
            StringBuilder texte = new StringBuilder();
            texte.Append(Trait == Couleur.Blanc ? 'W' : 'B');
            foreach (Couleur couleur in new[] { Couleur.Blanc, Couleur.Noir })
            {
                texte.Append(':').Append(couleur == Couleur.Blanc ? 'W' : 'B');
                bool premiere = true;
                for (int c = 1; c <= 50; c++)
                {
                    if (!Appartient(cases[c], couleur)) continue;
                    if (!premiere) texte.Append(',');
                    premiere = false;
                    if (EstDame(cases[c])) texte.Append('K');
                    texte.Append(c);
                }
            }
            return texte.ToString();
        }

        // ─── Protocole Hub2 de Scan : un caractère pour le trait (W ou B) puis un par case (w b W B e) ──────────────────────

        public static Position DepuisHub2(string hub2)
        {
            if (hub2.Length != 51) throw new FormatException($"Position Hub2 invalide : 51 caractères attendus, {hub2.Length} reçus.");
            Position position = new Position();
            if (hub2[0] == 'W') position.Trait = Couleur.Blanc;
            else if (hub2[0] == 'B') position.Trait = Couleur.Noir;
            else throw new FormatException($"Position Hub2 invalide (trait « {hub2[0]} », attendu W ou B).");
            for (int c = 1; c <= 50; c++)
            {
                position.cases[c] = hub2[c] switch
                {
                    'e' => Piece.Vide,
                    'w' => Piece.PionBlanc,
                    'b' => Piece.PionNoir,
                    'W' => Piece.DameBlanche,
                    'B' => Piece.DameNoire,
                    _ => throw new FormatException($"Position Hub2 invalide (case {c} : « {hub2[c]} »).")
                };
            }
            return position;
        }

        public string VersHub2()
        {
            char[] texte = new char[51];
            texte[0] = Trait == Couleur.Blanc ? 'W' : 'B';
            for (int c = 1; c <= 50; c++)
            {
                texte[c] = cases[c] switch
                {
                    Piece.Vide => 'e',
                    Piece.PionBlanc => 'w',
                    Piece.PionNoir => 'b',
                    Piece.DameBlanche => 'W',
                    _ => 'B'
                };
            }
            return new string(texte);
        }

        // ─── Égalité ────────────────────────────────────────────────────────────────────────────────────────────────────

        public bool Equals(Position? autre)
        {
            if (autre is null || Trait != autre.Trait) return false;
            for (int c = 1; c <= 50; c++)
                if (cases[c] != autre.cases[c]) return false;
            return true;
        }

        public override bool Equals(object? obj) => Equals(obj as Position);

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Trait);
            for (int c = 1; c <= 50; c++)
                hash.Add(cases[c]);
            return hash.ToHashCode();
        }

        public override string ToString() => VersFen();
    }
}
