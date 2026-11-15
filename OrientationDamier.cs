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
{
    // --- Le sens dans lequel on voit le damier : la correspondance entre les 50 cases Manoury (1 à 50) et les 100 places de l'écran
    // (0 à 99, ligne par ligne depuis le coin en haut à gauche, 10 colonnes), sans rien d'affiché.
    // Vue côté blancs : les cases sombres (ligne + colonne impaire) sont numérotées de 1 à 50 depuis le haut à gauche, les blancs montent vers la case 1.
    // Vue côté noirs : le damier est tourné de 180°, la place i devient la place 99 - i. Remplace deux tables statiques que « Tourner damier » recalculait. ---
    public class OrientationDamier
    {
        private static readonly int[] indexEcranVueBlancs = new int[50];          // [case Manoury - 1] -> place à l'écran, vue côté blancs
        private static readonly int[] caseManouryVueBlancs = new int[100];        // [place à l'écran] -> case Manoury, 0 si la case est claire, vue côté blancs

        static OrientationDamier()
        {   // La vue côté blancs est la géométrie du jeu (Geometrie.Coordonnees) : ligne * 10 + colonne
            for (int numero = 1; numero <= 50; numero++)
            {
                (int ligne, int colonne) = Geometrie.Coordonnees(numero);
                indexEcranVueBlancs[numero - 1] = ligne * 10 + colonne;
                caseManouryVueBlancs[ligne * 10 + colonne] = numero;
            }
        }
        // --- Vrai quand le damier est vu côté noirs (tourné de 180°) ---
        public bool CoteNoir { get; private set; }

        public OrientationDamier(bool coteNoir = false)
        {
            CoteNoir = coteNoir;
        }

        // --- Tourne le damier de 180° ---
        public void Tourner()
        {
            CoteNoir = !CoteNoir;
        }
        // --- La place à l'écran (0 à 99) d'une case Manoury (1 à 50) ; lève ArgumentOutOfRangeException hors du damier ---
        public int IndexEcran(int caseManoury)
        {
            if (caseManoury < 1 || caseManoury > 50)
                throw new ArgumentOutOfRangeException(nameof(caseManoury), "Case Manoury doit être entre 1 et 50.");
            int index = indexEcranVueBlancs[caseManoury - 1];
            return CoteNoir ? 99 - index : index;
        }
        // --- La case Manoury (1 à 50) à une place de l'écran (0 à 99), ou 0 si c'est une case claire (non jouable) ou hors du damier ---
        public int CaseManoury(int indexEcran)
        {
            if (indexEcran < 0 || indexEcran > 99)
                return 0;
            return caseManouryVueBlancs[CoteNoir ? 99 - indexEcran : indexEcran];
        }
    }
}
