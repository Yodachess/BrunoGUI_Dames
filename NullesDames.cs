// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Les parties nulles que le programme détecte tout seul, d'après l'historique des positions de la partie (logique pure, sans Windows Forms) :
//  - la répétition : la même position se présente pour la troisième fois, avec le même joueur au trait (pas forcément de suite) ;
//  - les coups de dames : pendant 25 coups de chaque joueur (50 demi-coups de suite) seules des dames ont bougé, sans déplacement de pion ni prise ;
//  - les finales à peu de pièces (règles FMJD 6.3 et 6.4) : contre une seule dame, trois dames, deux dames et un pion ou une dame et deux pions
//    laissent 16 coups à chaque joueur ; deux dames, une dame et un pion ou une dame seule en laissent 5 ; le compte part de la première position de la finale.
// Pas détectée : la nulle par accord, qui reste au choix du joueur.

using System.Collections.Generic;

namespace BrunoGUI_Dames
{
    public enum RaisonNulle { Aucune, Repetition, CoupsDeDames, FinaleSeizeCoups, FinaleCinqCoups }

    public static class NullesDames
    {
        public const int RepetitionsPourNulle = 3;
        public const int CoupsDeDamesParJoueur = 25;
        public const int CoupsFinale3Pieces = 16;
        public const int CoupsFinaleSimple = 5;

        // --- La raison pour laquelle la partie est nulle dans la dernière position de l'historique, ou Aucune ---
        // positions : la position de départ, puis la position après chaque demi-coup (la dernière est la position actuelle)
        public static RaisonNulle Detecte(IReadOnlyList<Position> positions)
        {
            if (positions.Count < 2)
                return RaisonNulle.Aucune;
            Position actuelle = positions[positions.Count - 1];
            int occurrences = 0;
            foreach (Position position in positions)
                if (position.Equals(actuelle))     // Position.Equals compare aussi le trait
                    occurrences++;
            if (occurrences >= RepetitionsPourNulle)
                return RaisonNulle.Repetition;

            int demiCoupsDeDames = 0;
            for (int i = positions.Count - 1; i >= 1 && EstCoupDeDameSimple(positions[i - 1], positions[i]); i--)
                demiCoupsDeDames++;
            if (demiCoupsDeDames >= 2 * CoupsDeDamesParJoueur)
                return RaisonNulle.CoupsDeDames;

            if (DemiCoupsDepuisLeDebutDeFinale(positions, CoupsFinale3Pieces) >= 2 * CoupsFinale3Pieces)
                return RaisonNulle.FinaleSeizeCoups;
            if (DemiCoupsDepuisLeDebutDeFinale(positions, CoupsFinaleSimple) >= 2 * CoupsFinaleSimple)
                return RaisonNulle.FinaleCinqCoups;
            return RaisonNulle.Aucune;
        }

        // --- Le nombre de coups (de chaque joueur) que permet la finale de cette position, ou 0 si ce n'est pas une de ces finales (règles 6.3 et 6.4) ---
        // Le camp le plus fort contre une seule dame : 16 coups pour trois dames, deux dames et un pion, ou une dame et deux pions ;
        // 5 coups pour deux dames, une dame et un pion, ou une dame (contre une dame).
        public static int CoupsPermisFinale(Position position)
        {
            int pionsBlancs = position.Compte(Piece.PionBlanc), damesBlanches = position.Compte(Piece.DameBlanche);
            int pionsNoirs = position.Compte(Piece.PionNoir), damesNoires = position.Compte(Piece.DameNoire);
            if (pionsNoirs == 0 && damesNoires == 1)
                return CoupsPermisContreUneDame(pionsBlancs, damesBlanches);
            if (pionsBlancs == 0 && damesBlanches == 1)
                return CoupsPermisContreUneDame(pionsNoirs, damesNoires);
            return 0;
        }

        private static int CoupsPermisContreUneDame(int pions, int dames)
        {
            if ((pions == 0 && dames == 3) || (pions == 1 && dames == 2) || (pions == 2 && dames == 1))
                return CoupsFinale3Pieces;
            if ((pions == 0 && dames == 2) || (pions == 1 && dames == 1) || (pions == 0 && dames == 1))
                return CoupsFinaleSimple;
            return 0;
        }

        // --- Les demi-coups joués depuis la première position de la série qui finit l'historique et qui est une finale de limite donnée (0 s'il n'y en a pas) ---
        private static int DemiCoupsDepuisLeDebutDeFinale(IReadOnlyList<Position> positions, int limite)
        {
            int serie = 0;
            for (int i = positions.Count - 1; i >= 0 && CoupsPermisFinale(positions[i]) == limite; i--)
                serie++;
            return serie > 0 ? serie - 1 : 0;
        }

        // --- Vrai si le demi-coup qui mène de avant à apres est un déplacement de dame sans prise : même nombre de pièces et mêmes cases de pions ---
        // (une prise retire une pièce ; un déplacement de pion, ou une promotion, change les cases de pions)
        public static bool EstCoupDeDameSimple(Position avant, Position apres)
        {
            for (int c = 1; c <= 50; c++)
            {
                Piece a = avant[c], b = apres[c];
                if (EstPion(a) != EstPion(b) || (EstPion(a) && a != b))
                    return false;
            }
            return Nombre(avant) == Nombre(apres);
        }

        private static bool EstPion(Piece piece) => piece == Piece.PionBlanc || piece == Piece.PionNoir;

        private static int Nombre(Position position)
        {
            return position.Compte(Piece.PionBlanc) + position.Compte(Piece.PionNoir) + position.Compte(Piece.DameBlanche) + position.Compte(Piece.DameNoire);
        }
    }
}
