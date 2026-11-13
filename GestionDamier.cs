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
using System.Reflection;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using System.Linq;


namespace BrunoGUI_Dames
{       // Règles du jeu de Dames : http://www.ffjd.fr/Web/index.php?page=reglesdujeu
        // Information jeu de Dames : https://fr.wikipedia.org/wiki/Dames
    public enum TypePiece
    {
        Vide,        // Case vide
        PionNoir,
        PionBlanc,
        DameNoire,
        DameBlanche,
        CadreGris, CadreVert, CadreRouge, CadreMarron,
        Inactive     // Case paire qui ne sert à rien
    }

    public enum CouleurPiece
    {
        Vide,        // Pas de pièce sur la case
        Blanc,
        Noir,
        Inactive     // Case inactive
    }

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
    public class ContenuCase
    {   // --- Définition de la classe ContenuCase pour contenir le type de pièce et la couleur ---
        public TypePiece TypePiece { get; set; } = TypePiece.Vide;
        public CouleurPiece CouleurPiece { get; set; } = CouleurPiece.Vide;
        public ContenuCase(TypePiece typePiece = TypePiece.Vide,
                          CouleurPiece couleurPiece = CouleurPiece.Vide)
        {
            TypePiece = typePiece;
            CouleurPiece = couleurPiece;
        }
        public bool EstVide()
        {
            return TypePiece == TypePiece.Vide;
        }
    }

    public static class GestionDamier
    {
        // Tableau de correspondance entre case Manoury (1-50) et indices PictureBox (0-99)
        public static readonly int[] ManouryVersIndexPictureBox = new int[50];      // ATTENTION : Manoury commence à 1 mais le tableau ManouryVersIndexPictureBox à 0 !!

        // Dictionnaire inverse : Indice PictureBox (0-99) -> Case Manoury (1-50)
        public static Dictionary<int, int> PictureBoxVersManoury = new Dictionary<int, int>();

        public static ContenuCase[,] DamierContenu = new ContenuCase[10, 10]; // Tableau de [10*10] décrivant les contenu de chaque avec [Type de pièce, Couleur de pièce]

        // Les pièces du jeu par numéro de case Manoury (indépendant du sens d'affichage du damier). Elle est tenue à jour avec DamierContenu
        // par DessinePiece et EffacePiece ; le trait n'y est pas suivi (c'est CouleurAuTrait, lu par RecupereFEN).
        public static Position PositionCourante = new Position();

        // --- Conversions entre les types de pièces de l'affichage et ceux de Position ---
        public static Piece PieceDe(TypePiece typePiece)
        {
            switch (typePiece)
            {
                case TypePiece.PionBlanc: return Piece.PionBlanc;
                case TypePiece.PionNoir: return Piece.PionNoir;
                case TypePiece.DameBlanche: return Piece.DameBlanche;
                case TypePiece.DameNoire: return Piece.DameNoire;
                default: return Piece.Vide;
            }
        }
        public static TypePiece TypeDe(Piece piece)
        {
            switch (piece)
            {
                case Piece.PionBlanc: return TypePiece.PionBlanc;
                case Piece.PionNoir: return TypePiece.PionNoir;
                case Piece.DameBlanche: return TypePiece.DameBlanche;
                case Piece.DameNoire: return TypePiece.DameNoire;
                default: return TypePiece.Vide;
            }
        }
        public static CouleurPiece CouleurDe(Piece piece)
        {
            if (Position.EstBlanche(piece)) return CouleurPiece.Blanc;
            if (Position.EstNoire(piece)) return CouleurPiece.Noir;
            return CouleurPiece.Vide;
        }
        // --- Nombre de pièces (pions et dames) d'une couleur ---
        public static int NombreDePieces(Couleur couleur)
        {
            return couleur == Couleur.Blanc ? PositionCourante.Compte(Piece.PionBlanc) + PositionCourante.Compte(Piece.DameBlanche)
                                            : PositionCourante.Compte(Piece.PionNoir) + PositionCourante.Compte(Piece.DameNoire);
        }

        public static void InitialiserDamier()
        {   // --- Remplit le damier avec des pions, et met à jour le tableau DamierContenu, ainsi que PositionCourante ---
            PositionCourante = new Position();
            for (int ligne = 9; ligne >= 0; ligne--)
            {
                for (int colonne = 0; colonne < 10; colonne++)
                {
                    if ((ligne % 2 == 0 && colonne % 2 != 0) || (ligne % 2 != 0 && colonne % 2 == 0))
                    {   // Ajouter des pions blancs ou noirs selon la logique de placement
                        if (ligne < 4)
                        {   // Pions noirs
                            DamierContenu[ligne, colonne] = new ContenuCase(TypePiece.PionNoir, CouleurPiece.Noir);
                            PositionCourante[PictureBoxVersManoury[((ligne * 10) + colonne)]] = Piece.PionNoir;
                        }
                        else if (ligne > 5)
                        {   // Pions blancs
                            DamierContenu[ligne, colonne] = new ContenuCase(TypePiece.PionBlanc, CouleurPiece.Blanc);
                            PositionCourante[PictureBoxVersManoury[((ligne * 10) + colonne)]] = Piece.PionBlanc;
                        }
                        else
                        {   // Milieu du damier = cases vides
                            DamierContenu[ligne, colonne] = new ContenuCase(TypePiece.Vide, CouleurPiece.Vide);
                        }
                    }
                    else
                    {   // Les cases inactives, généralement les cases sombres non jouables
                        DamierContenu[ligne, colonne] = new ContenuCase(TypePiece.Inactive, CouleurPiece.Inactive);
                    }
                }
            }
        }
        public static void ViderDamier()
        {   // Remplit le damier avec des cases vides, et met à jour le tableau DamierContenu
            for (int ligne = 9; ligne >= 0; ligne--)
            {
                for (int colonne = 0; colonne < 10; colonne++)
                {
                    if ((ligne % 2 == 0 && colonne % 2 != 0) || (ligne % 2 != 0 && colonne % 2 == 0))
                    {   // Vider les cases actives
                        DamierContenu[ligne, colonne] = new ContenuCase(TypePiece.Vide, CouleurPiece.Vide);
                    }
                    else
                    {   // Les cases inactives, généralement les cases sombres non jouables
                        DamierContenu[ligne, colonne] = new ContenuCase(TypePiece.Inactive, CouleurPiece.Inactive);
                    }
                }
            }
            PositionCourante = new Position();
        }

        public static void InitialiserCorrespondance(Control.ControlCollection controls)
        {   // --- Crée les tables de correspondances PictureBox (0-99) <-> Case Manoury (1-50) ---
            int indexManoury = 1; // Manoury commence à 1

            for (int ligne = 0; ligne < 10; ligne++) // Parcours des lignes
            {
                for (int colonne = 0; colonne < 10; colonne++) // Parcours des colonnes
                {
                    int pictureBoxIndex = ligne * 10 + colonne;
                    if ((ligne + colonne) % 2 != 0)           // Case foncée si (ligne + colonne) % 2 != 0
                    {
                        int indexPictureBox = ((ligne * 10) + colonne);
                        string nomPictureBox = $"ContenuCase{indexPictureBox:D2}";
                        PictureBox? pictureBox = controls.Find(nomPictureBox, true).FirstOrDefault() as PictureBox;
                        if (pictureBox != null)
                        {
                            BrunoInterfaceGraphiqueDames.ManouryVersPictureBoxIndicee[indexManoury] = pictureBox;
                        }
                        GestionDamier.ManouryVersIndexPictureBox[indexManoury - 1] = pictureBoxIndex;
                        GestionDamier.PictureBoxVersManoury[pictureBoxIndex] = indexManoury;
                        indexManoury++;
                    }
                }
            }
        }
        public static ContenuCase ContenuCaseCaseManoury(int numeroCaseManoury)
        {   // --- Retourne le ContenuCase présente sur la case Manoury ---
            (int ligne, int colonne) = IndiceVersCoordonnees(ObtenirIndicePictureBox(numeroCaseManoury));
            return (GestionDamier.DamierContenu[ligne, colonne]);
        }
        public static (int ligne, int colonne) IndiceVersCoordonnees(int indiceBox)
        {   // --- Retourne la ligne et la colonne de la case indiceBox ---
            return (indiceBox / 10, indiceBox % 10);
        }
        public static int CoordonneesVersIndice(int ligne, int colonne)
        {   // --- Retourne l'indiceBox située à la ligne, colonne ---
            return ligne * 10 + colonne;
        }
        public static bool EstNumeroPictureBoxValide(int numeroPictureBox)
        {   // Verifie si le numeroPictureBox est dans les limites du damier ---
            var (ligne, colonne) = GestionDamier.IndiceVersCoordonnees(numeroPictureBox);
            return ligne >= 0 && ligne < 10 && colonne >= 0 && colonne < 10;
        }
        public static bool EstCaseManouryValide(int numeroCaseManoury)              
        {   // --- Vérifie si une case Manoury est valide ---
            return numeroCaseManoury >= 1 && numeroCaseManoury <= 50;
        }
        public static int ObtenirIndicePictureBox(int caseManoury)
        {   // --- Obtenir PictureBox à partir de la caseManoury ---
            if (caseManoury < 1 || caseManoury > 50)
                throw new ArgumentOutOfRangeException(nameof(caseManoury), "Case Manoury doit être entre 1 et 50.");
            return ManouryVersIndexPictureBox[caseManoury - 1];
        }
        public static int ObtenirCaseManoury(int indicePictureBox)
        {   // --- Obtenir case Manoury à partir d'un indicePictureBox ---
            if (!PictureBoxVersManoury.ContainsKey(indicePictureBox))
                throw new ArgumentOutOfRangeException(nameof(indicePictureBox), "L'indice PictureBox ne correspond pas à une case Manoury valide.");
            return PictureBoxVersManoury[indicePictureBox];
        }
    }
}