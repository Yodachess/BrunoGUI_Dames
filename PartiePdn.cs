// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Une partie au format PDN (Portable Draughts Notation, https://pdn.fmjd.org/index.html) : les balises, les coups et le
// résultat. Logique pure, sans Windows Forms : la lecture est dans LecturePdn.cs, l'écriture et le rejeu des coups ici.

using System;
using System.Collections.Generic;
using System.Text;

namespace BrunoGUI_Dames
{
    // Résultat du rejeu d'une partie : les positions successives, les coups joués et, si un coup n'a pas pu l'être, l'erreur.
    // Positions : La position de départ puis la position après chaque coup joué (une de plus que Coups).
    // Coups : Les coups joués, jusqu'à l'éventuelle erreur.
    // Erreur : Null si tous les coups de la partie sont légaux ; sinon la raison de l'arrêt.
    public sealed record RejeuPdn(IReadOnlyList<Position> Positions, IReadOnlyList<Coup> Coups, string? Erreur);

    public sealed class PartiePdn
    {
        private readonly List<(string Nom, string Valeur)> balises = new List<(string, string)>();

        // --- Les balises dans l'ordre où elles ont été lues ou ajoutées. ---
        public IReadOnlyList<(string Nom, string Valeur)> Balises => balises;

        // --- Les coups, un par demi-coup, en notation PDN abrégée normalisée : « 32-28 », « 28x19 » (cases sans zéro devant). ---
        public List<string> Coups { get; } = new List<string>();

        // --- Le marqueur de fin lu après les coups (« 1-0 », « 0-1 », « 1/2-1/2 », « 2-0 », « 0-2 », « 1-1 », « * » ou « - »), ou null. ---
        public string? Resultat { get; set; }

        // --- Ce que la lecture n'a pas compris et a ignoré (jeton inconnu, case invalide…) : jamais une erreur bloquante. ---
        public List<string> Avertissements { get; } = new List<string>();

        // ─── Balises ────────────────────────────────────────────────────────────────────────────────────────────────────

        public void AjouterBalise(string nom, string valeur) => balises.Add((nom, valeur));

        // --- Donne sa valeur à la balise (la première de ce nom si plusieurs), en l'ajoutant à la fin si elle n'existe pas. ---
        public void DefinirBalise(string nom, string valeur)
        {
            int indice = balises.FindIndex(b => string.Equals(b.Nom, nom, StringComparison.OrdinalIgnoreCase));
            if (indice >= 0) balises[indice] = (balises[indice].Nom, valeur);
            else balises.Add((nom, valeur));
        }

        // --- La valeur de la balise (la première de ce nom, sans tenir compte des majuscules), ou null si elle n'existe pas. ---
        public string? Balise(string nom)
        {
            foreach ((string n, string valeur) in balises)
                if (string.Equals(n, nom, StringComparison.OrdinalIgnoreCase)) return valeur;
            return null;
        }

        public string Event { get => (Balise("Event") ?? "").Trim(); set => DefinirBalise("Event", value); }
        public string Site { get => (Balise("Site") ?? "").Trim(); set => DefinirBalise("Site", value); }
        public string Date { get => (Balise("Date") ?? "").Trim(); set => DefinirBalise("Date", value); }
        public string Round { get => (Balise("Round") ?? "").Trim(); set => DefinirBalise("Round", value); }
        public string White { get => (Balise("White") ?? "").Trim(); set => DefinirBalise("White", value); }
        public string Black { get => (Balise("Black") ?? "").Trim(); set => DefinirBalise("Black", value); }
        public string Result { get => (Balise("Result") ?? "").Trim(); set => DefinirBalise("Result", value); }

        // --- Position de départ en FEN si la partie ne part pas de la position initiale (balise FEN), sinon null. ---
        public string? Fen { get => Balise("FEN")?.Trim(); set { if (value != null) DefinirBalise("FEN", value); } }

        // --- La position de départ : celle de la balise FEN, sinon la position initiale. Lève FormatException si la FEN est invalide. ---
        public Position PositionDepart() => string.IsNullOrWhiteSpace(Fen) ? Position.Initiale() : Position.DepuisFen(Fen);

        // ─── Création depuis l'historique de l'application ─────────────────────────────────────────────────────────────────

        private static readonly HashSet<string> marqueursDeFin = new HashSet<string> { "1-0", "0-1", "1/2-1/2", "2-0", "0-2", "1-1", "*", "-" };

        // La partie à écrire, à partir de l'historique de l'application : les sept balises d'en-tête, puis, s'il y a des coups, Result
        // (la valeur donnée, sinon le marqueur de fin trouvé dans la liste, sinon « * »), la balise FEN si la partie ne part pas de la
        // position initiale, et PlyCount. Dans coups, un élément qui est un marqueur de fin (« 1-0 » avec ou sans
        // espaces autour) devient Resultat et n'est pas compté comme un coup ; les autres sont des coups « 32-28 » ou « 28x19 ».
        public static PartiePdn Creer(string evenement, string site, string date, string tour, string blancs, string noirs,
                                      string resultat, string? fenDepart, IEnumerable<string> coups)
        {
            PartiePdn partie = new PartiePdn();
            partie.AjouterBalise("Event", evenement);
            partie.AjouterBalise("Site", site);
            partie.AjouterBalise("Date", date);
            partie.AjouterBalise("Round", tour);
            partie.AjouterBalise("White", blancs);
            partie.AjouterBalise("Black", noirs);
            foreach (string element in coups)
            {
                string coup = element.Trim();
                if (coup.Length == 0) continue;
                if (marqueursDeFin.Contains(coup)) partie.Resultat = coup;
                else partie.Coups.Add(coup);
            }
            if (partie.Coups.Count > 0)     // Sans aucun coup, pas de résultat
            {
                string valeur = resultat.Trim();
                if (valeur.Length == 0 || valeur == "*") valeur = partie.Resultat ?? "*";
                partie.AjouterBalise("Result", valeur);
            }
            if (!string.IsNullOrWhiteSpace(fenDepart)) partie.AjouterBalise("FEN", fenDepart.Trim());
            partie.AjouterBalise("PlyCount", partie.Coups.Count.ToString());
            return partie;
        }

        // ─── Rejeu ──────────────────────────────────────────────────────────────────────────────────────────────────────

        // Rejoue les coups avec les règles du jeu depuis la position de départ. S'arrête au premier coup illégal, ou ambigu
        // (« 28x19 » qui désigne deux rafles différentes), en le disant dans Erreur ; rien n'est jamais modifié dans la partie.
        public RejeuPdn Rejouer()
        {
            Position position;
            try { position = PositionDepart(); }
            catch (FormatException e) { return new RejeuPdn(Array.Empty<Position>(), Array.Empty<Coup>(), $"Position de départ invalide : {e.Message}"); }

            List<Position> positions = new List<Position> { position };
            List<Coup> coups = new List<Coup>();
            for (int i = 0; i < Coups.Count; i++)
            {
                Coup? coup = ReglesDames.TrouverCoupPdn(position, Coups[i]);
                if (coup == null)
                    return new RejeuPdn(positions, coups, $"Coup {i + 1} « {Coups[i]} » illégal ou ambigu dans la position {position.VersFen()}");
                position = ReglesDames.Jouer(position, coup);
                positions.Add(position);
                coups.Add(coup);
            }
            return new RejeuPdn(positions, coups, null);
        }

        // ─── Écriture ───────────────────────────────────────────────────────────────────────────────────────────────────

        // La partie au format PDN : les balises (valeurs entre guillemets, « \ » et « " » précédés d'un « \ »), une ligne vide, puis les
        // coups numérotés (« 1. 32-28 17-22 2. 28x17 12x21 »), à la ligne avant la largeur donnée, terminés par le résultat
        // (Resultat, sinon la balise Result, sinon « * »). Si les noirs jouent en premier (FEN), le premier coup est précédé de « 1... ».
        public string VersTexte(int largeur = 78, string? finDeLigne = null)
        {
            finDeLigne ??= Environment.NewLine;
            StringBuilder texte = new StringBuilder();
            foreach ((string nom, string valeur) in balises)
                texte.Append('[').Append(nom).Append(" \"").Append(valeur.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\"]").Append(finDeLigne);
            texte.Append(finDeLigne);

            bool noirsEnPremier = false;
            try { noirsEnPremier = PositionDepart().Trait == Couleur.Noir; }
            catch (FormatException) { }     // FEN invalide : on numérote comme si les blancs commençaient

            List<string> jetons = new List<string>();
            int decalage = noirsEnPremier ? 1 : 0;
            for (int i = 0; i < Coups.Count; i++)
            {
                bool coupBlanc = (i + decalage) % 2 == 0;
                if (coupBlanc || i == 0) jetons.Add($"{(i + decalage) / 2 + 1}.{(coupBlanc ? "" : "..")}");
                jetons.Add(Coups[i]);
            }
            string fin = !string.IsNullOrWhiteSpace(Resultat) ? Resultat!.Trim() : (Result.Length > 0 ? Result : "*");
            jetons.Add(fin);

            StringBuilder ligne = new StringBuilder();
            foreach (string jeton in jetons)
            {
                if (ligne.Length > 0 && ligne.Length + 1 + jeton.Length > largeur)
                {
                    texte.Append(ligne).Append(finDeLigne);
                    ligne.Clear();
                }
                if (ligne.Length > 0) ligne.Append(' ');
                ligne.Append(jeton);
            }
            texte.Append(ligne).Append(finDeLigne);
            return texte.ToString();
        }
    }
}
