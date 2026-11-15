// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Collections.Generic;

namespace BrunoGUI_Dames
{
    // --- La partie en cours, sans rien d'affiché : l'historique des positions (l'indice 0 est la position de départ), les coups joués
    // en notation PDN, le résultat éventuel et la position montrée par le parcours de partie. Remplace les trois listes parallèles
    // (FEN, PDN, Hub2) que le formulaire tenait à la main : une seule liste de positions, d'où l'on tire le FEN et le Hub2 à la demande. ---
    public class SessionPartie
    {
        private readonly List<Position> positions = new List<Position>();
        private readonly List<string> coups = new List<string>();

        // --- Les positions successives : positions[0] est le départ, positions[i] celle après le coup i (les coups sont numérotés à partir de 1) ---
        public IReadOnlyList<Position> Positions => positions;
        // --- Les coups joués en notation PDN, sans le résultat : coups[i - 1] mène à positions[i] ---
        public IReadOnlyList<string> CoupsPdn => coups;
        // --- Le résultat (« 1-0 », « 0-1 », « 1/2-1/2 ») ou null tant que la partie n'est pas finie ---
        public string? Resultat { get; private set; }
        // --- L'indice de la position montrée (0 : départ), celui que déplacent Début, Précédent, Suivant et Fin ---
        public int IndexAffiche { get; private set; }

        public bool EstVide => positions.Count == 0;
        // --- Vrai quand la position montrée est la dernière (ou qu'il n'y en a aucune) : seulement là on peut jouer ; ailleurs on parcourt la partie ---
        public bool EstSurLaDernierePosition => positions.Count == 0 || IndexAffiche == positions.Count - 1;
        // --- La dernière position jouée (lève InvalidOperationException si la partie n'a pas commencé) ---
        public Position DernierePosition => positions.Count > 0 ? positions[positions.Count - 1] : throw new InvalidOperationException("La partie n'a pas commencé.");
        public Position PositionAffichee => positions.Count > 0 ? positions[IndexAffiche] : throw new InvalidOperationException("La partie n'a pas commencé.");
        // --- Le coup qui a mené à la position montrée, ou null au départ ---
        public string? CoupAffiche => IndexAffiche > 0 ? coups[IndexAffiche - 1] : null;

        // --- Commence une partie à cette position : tout l'historique, le résultat et le parcours repartent de zéro ---
        public void Commencer(Position depart)
        {
            positions.Clear();
            coups.Clear();
            Resultat = null;
            positions.Add(depart.Copie());
            IndexAffiche = 0;
        }
        // --- Ajoute un coup (sa notation PDN) et la position qu'il donne (avec le trait de celui qui joue ensuite) ; le parcours se place dessus ---
        public void AjouteCoup(string coupPdn, Position apres)
        {
            if (positions.Count == 0)
                throw new InvalidOperationException("La partie n'a pas commencé.");
            coups.Add(coupPdn);
            positions.Add(apres.Copie());
            IndexAffiche = positions.Count - 1;
        }
        // --- Note le résultat de la partie (le précédent, s'il y en avait un, est remplacé) ---
        public void FixeResultat(string resultat)
        {
            Resultat = resultat.Trim();
        }
        // --- Retour arrière : retire le dernier coup et la position qu'il avait donnée, et efface le résultat. Le parcours se place sur la
        // nouvelle dernière position. Renvoie le coup retiré, ou null s'il n'y en a aucun (rien n'est alors modifié) ---
        public string? RetireDernierCoup()
        {
            if (coups.Count == 0)
                return null;
            string retire = coups[coups.Count - 1];
            coups.RemoveAt(coups.Count - 1);
            positions.RemoveAt(positions.Count - 1);
            Resultat = null;
            IndexAffiche = positions.Count - 1;
            return retire;
        }

        // --- Le parcours de partie : chaque méthode renvoie true si la position montrée a changé ---
        public bool Debut()
        {
            if (positions.Count <= 1 || IndexAffiche == 0)
                return false;
            IndexAffiche = 0;
            return true;
        }
        public bool Precedent()
        {
            if (IndexAffiche <= 0)
                return false;
            IndexAffiche--;
            return true;
        }
        public bool Suivant()
        {
            if (IndexAffiche >= positions.Count - 1)
                return false;
            IndexAffiche++;
            return true;
        }
        public bool Fin()
        {
            if (positions.Count <= 1 || IndexAffiche == positions.Count - 1)
                return false;
            IndexAffiche = positions.Count - 1;
            return true;
        }

        // --- Les formats qu'attendent les autres parties du programme, tirés des positions ---
        // Le FEN enrobé de la position i : [FEN "W:W31-50:B1-20"]
        public string Fen(int indice)
        {
            return "[FEN \"" + positions[indice].VersFen() + "\"]";
        }
        // La position Hub2 (celle qu'attend Scan) de la position i
        public string Hub2(int indice)
        {
            return positions[indice].VersHub2();
        }
        // La position Hub2 de la dernière position jouée (celle qu'on envoie au moteur pour qu'il joue)
        public string Hub2Dernier()
        {
            return DernierePosition.VersHub2();
        }
        // Les coups PDN suivis, s'il y en a un, du résultat avec un espace devant (« 1-0 » devient « " 1-0" ») : le format de PartiePdn.Creer
        public List<string> CoupsPdnAvecResultat()
        {
            List<string> liste = new List<string>(coups);
            if (Resultat != null)
                liste.Add(" " + Resultat);
            return liste;
        }
    }
}
