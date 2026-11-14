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
//  - les coups de dames : pendant 25 coups de chaque joueur (50 demi-coups de suite) seules des dames ont bougé, sans déplacement de pion ni prise.
// Pas détectées pour l'instant : les finales à peu de pièces (16 ou 5 coups), la nulle par accord, qui reste au choix du joueur.

using System.Collections.Generic;

namespace BrunoGUI_Dames
{
    public enum RaisonNulle { Aucune, Repetition, CoupsDeDames }

    public static class NullesDames
    {
        public const int RepetitionsPourNulle = 3;
        public const int CoupsDeDamesParJoueur = 25;

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
            return demiCoupsDeDames >= 2 * CoupsDeDamesParJoueur ? RaisonNulle.CoupsDeDames : RaisonNulle.Aucune;
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
