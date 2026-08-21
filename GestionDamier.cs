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
using static BrunoGUI_Dames.LogiqueMouvementsDames;

namespace BrunoGUI_Dames
{       // Règles du jeu de Dames : http://www.ffjd.fr/Web/index.php?page=reglesdujeu
        // Information jeu de Dames : https://fr.wikipedia.org/wiki/Dames
    public class ContenuCase
    {   // --- Définition de la classe ContenuCase pour contenir le type de pièce et la couleur ---
        public LogiqueMouvementsDames.TypePiece TypePiece { get; set; } = LogiqueMouvementsDames.TypePiece.Vide;
        public LogiqueMouvementsDames.CouleurPiece CouleurPiece { get; set; } = LogiqueMouvementsDames.CouleurPiece.Vide;
        public ContenuCase(LogiqueMouvementsDames.TypePiece typePiece = LogiqueMouvementsDames.TypePiece.Vide,
                          LogiqueMouvementsDames.CouleurPiece couleurPiece = LogiqueMouvementsDames.CouleurPiece.Vide)
        {
            TypePiece = typePiece;
            CouleurPiece = couleurPiece;
        }
        public bool EstVide()
        {
            return TypePiece == LogiqueMouvementsDames.TypePiece.Vide;
        }
    }

    public static class GestionDamier
    {
        // Tableau de correspondance entre case Manoury (1-50) et indices PictureBox (0-99)
        public static readonly int[] ManouryVersIndexPictureBox = new int[50];      // ATTENTION : Manoury commence à 1 mais le tableau ManouryVersIndexPictureBox à 0 !!

        // Dictionnaire inverse : Indice PictureBox (0-99) -> Case Manoury (1-50)
        public static Dictionary<int, int> PictureBoxVersManoury = new Dictionary<int, int>();

        public static ContenuCase[,] DamierContenu = new ContenuCase[10, 10]; // Tableau de [10*10] décrivant les contenu de chaque avec [Type de pièce, Couleur de pièce]

        public static void InitialiserDamier()
        {   // --- Remplit le damier avec des pions, et met à jour le tableau DamierContenu, ainsi que les listes de pions ---
            for (int ligne = 9; ligne >= 0; ligne--)
            {
                for (int colonne = 0; colonne < 10; colonne++)
                {
                    if ((ligne % 2 == 0 && colonne % 2 != 0) || (ligne % 2 != 0 && colonne % 2 == 0))
                    {   // Ajouter des pions blancs ou noirs selon la logique de placement
                        if (ligne < 4)
                        {   // Pions noirs
                            DamierContenu[ligne, colonne] = new ContenuCase(LogiqueMouvementsDames.TypePiece.PionNoir, LogiqueMouvementsDames.CouleurPiece.Noir);
                            LogiqueMouvementsDames.ListePionsNoirs.Add(PictureBoxVersManoury[((ligne * 10) + colonne)]);
                        }
                        else if (ligne > 5)
                        {   // Pions blancs
                            DamierContenu[ligne, colonne] = new ContenuCase(LogiqueMouvementsDames.TypePiece.PionBlanc, LogiqueMouvementsDames.CouleurPiece.Blanc);
                            LogiqueMouvementsDames.ListePionsBlancs.Add(PictureBoxVersManoury[((ligne * 10) + colonne)]);
                        }
                        else
                        {   // Milieu du damier = cases vides
                            DamierContenu[ligne, colonne] = new ContenuCase(LogiqueMouvementsDames.TypePiece.Vide, LogiqueMouvementsDames.CouleurPiece.Vide);
                        }
                    }
                    else
                    {   // Les cases inactives, généralement les cases sombres non jouables
                        DamierContenu[ligne, colonne] = new ContenuCase(LogiqueMouvementsDames.TypePiece.Inactive, LogiqueMouvementsDames.CouleurPiece.Inactive);
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
                        DamierContenu[ligne, colonne] = new ContenuCase(LogiqueMouvementsDames.TypePiece.Vide, LogiqueMouvementsDames.CouleurPiece.Vide);
                    }
                    else
                    {   // Les cases inactives, généralement les cases sombres non jouables
                        DamierContenu[ligne, colonne] = new ContenuCase(LogiqueMouvementsDames.TypePiece.Inactive, LogiqueMouvementsDames.CouleurPiece.Inactive);
                    }
                }
            }
            ListeDamesBlanches.Clear();
            ListeDamesNoires.Clear();
            ListePionsBlancs.Clear();
            ListePionsNoirs.Clear();
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
                        PictureBox pictureBox = controls.Find(nomPictureBox, true).FirstOrDefault() as PictureBox;
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