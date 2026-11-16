// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;


namespace BrunoGUI_Dames
{       // Règles du jeu de Dames : http://www.ffjd.fr/Web/index.php?page=reglesdujeu
        // Information jeu de Dames : https://fr.wikipedia.org/wiki/Dames

    // Le damier virtuel est un damier de 100 cases
    //
    //        PictureBox indéxées Pictxx                      Notation Manoury
    //   0   1   2   3   4   5   6   7   8   9         ..  01  ..  02  ..  03  ..  04  ..  05
    //  10  11  12  13  14  15  16  17  18  19         06  ..  07  ..  08  ..  09  ..  10  ..
    //  20  21  22  23  24  25  26  27  28  29         ..  11  ..  12  ..  13  ..  14  ..  15
    //  30  31  32  33  34  35  36  37  38  39         16  ..  17  ..  18  ..  19  ..  20  ..
    //  40  41  42  43  44  45  46  47  48  49         ..  21  ..  22  ..  23  ..  24  ..  25
    //  50  51  52  53  54  55  56  57  58  59         26  ..  27  ..  28  ..  29  ..  30  ..
    //  60  61  62  63  64  65  66  67  68  69         ..  31  ..  32  ..  33  ..  34  ..  35
    //  70  71  72  73  74  75  76  77  78  79         36  ..  37  ..  38  ..  39  ..  40  ..
    //  80  81  82  83  84  85  86  87  88  89         ..  41  ..  42  ..  43  ..  44  ..  45
    //  90  91  92  93  94  95  96  97  98  99         46  ..  47  ..  48  ..  49  ..  50  ..

    // Le damier de gauche représente les indices des Picturebox affichant les cases du jeu à l'écran ( voir CaseDamier comme List(of Picturebox))
    // Le damier de droite représente le damier de 100 cases avec les 50 cases réelles du jeu.
    // Les cases marquées d'un chiffre (01 à 50) sont les 50 cases actives du damier de 100 cases.
    // Les cases marquées  ".."  sur le damier de 100 cases sont les cases claires inutilisées.
    public class GestionDamier
    {
        // Le sens dans lequel on voit le damier : la correspondance entre les cases Manoury (1-50) et les places à l'écran (0-99), voir OrientationDamier
        public OrientationDamier Orientation = new OrientationDamier();

        // Les pièces du jeu par numéro de case Manoury (indépendant du sens d'affichage du damier) : c'est la seule description du contenu du damier.
        // Elle change à la mise en place d'une position (MiseenplaceFen, InitialiserDamier, ViderDamier) et une fois par coup, à la fin du coup
        // (AppliqueCoupALaPosition, par ReglesDames.Jouer) ; DessinePiece et EffacePiece ne touchent qu'aux images. Le trait n'y est pas suivi (c'est CouleurAuTrait, lu par RecupereFEN).
        public Position PositionCourante = new Position();

        // --- Nombre de pièces (pions et dames) d'une couleur ---
        public int NombreDePieces(Couleur couleur)
        {
            return couleur == Couleur.Blanc ? PositionCourante.Compte(Piece.PionBlanc) + PositionCourante.Compte(Piece.DameBlanche)
                                            : PositionCourante.Compte(Piece.PionNoir) + PositionCourante.Compte(Piece.DameNoire);
        }

        public void InitialiserDamier()
        {   // --- Met la position initiale dans PositionCourante (sans rien afficher) ---
            PositionCourante = Position.Initiale();
        }
        public void ViderDamier()
        {   // --- Vide la position (sans rien afficher) ---
            PositionCourante = new Position();
        }

        public bool EstCaseManouryValide(int numeroCaseManoury)
        {   // --- Vérifie si une case Manoury est valide ---
            return numeroCaseManoury >= 1 && numeroCaseManoury <= 50;
        }
        public int ObtenirIndicePictureBox(int caseManoury)
        {   // --- Obtenir la place à l'écran (index de PictureBox) à partir de la caseManoury (ArgumentOutOfRangeException hors de 1 à 50) ---
            return Orientation.IndexEcran(caseManoury);
        }
        public int ObtenirCaseManoury(int indicePictureBox)
        {   // --- Obtenir case Manoury à partir d'un indicePictureBox ---
            int numeroCase = Orientation.CaseManoury(indicePictureBox);
            if (numeroCase == 0)
                throw new ArgumentOutOfRangeException(nameof(indicePictureBox), "L'indice PictureBox ne correspond pas à une case Manoury valide.");
            return numeroCase;
        }
    }
}