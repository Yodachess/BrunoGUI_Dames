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
using System.Drawing;
using System.Linq;

namespace BrunoGUI_Dames
{       // Règles du jeu de Dames : http://www.ffjd.fr/Web/index.php?page=reglesdujeu
        // Information jeu de Dames : https://fr.wikipedia.org/wiki/Dames
    public class LogiqueMouvementsDames
    {
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

        public static string FenDepart { get; set; } = "[FEN \"W:W31-50:B1-20\"]";
        // Contient la liste de coups dans différents formats  
        public static List<string> ListeCoupsFen = new List<string>();      // https://pdn.fmjd.org/fen.html
        public static List<string> ListeCoupsPdn = new List<string>();      // https://pdn.fmjd.org/index.html
        public static List<string> ListeCoupsHub2 = new List<string>();     // format protocole Hub2 
        /*    format protocole Hub2 
        Le format de position est un caractère pour le côté à déplacer ('W' ou 'B') + un caractère par case dans l'ordre standard, donc 51 au total. 
        Pour chaque case :       'w' : homme blanc  --  'b' : homme noir  --  'W' : roi (dame) blanc  --  'B' : roi (dame) noir  --  'e' : vide  --
        pos pos=Wbbbbbbbbbbbbbbbbbbbbbeeeeeeeeeewwwwwwwwwwwwwwwwwwww
        pos pos=BeeeWWeeeeeeeeeeeeeeeeeeeweeeeeeeeeewweBeeeeeeeeeBee 
         */
        // Contient les listes des différentes pièces 
        public static List<int> ListePionsBlancs = new List<int>();
        public static List<int> ListeDamesBlanches = new List<int>();
        public static List<int> ListePionsNoirs = new List<int>();
        public static List<int> ListeDamesNoires = new List<int>();
        public static CouleurPiece ConvertirCouleurTrait(string couleurTrait)
        {   // --- Renvoie la CouleurPiece de la couleurTrait --- 
            switch (couleurTrait)
            {
                case "Blanc":
                    return CouleurPiece.Blanc;
                case "Noir":
                    return CouleurPiece.Noir;
                default:
                    return CouleurPiece.Vide; // Par défaut si aucune correspondance
            }
        }

        public static void MiseenplaceFen(string fenAutiliser, bool ajouteListeCoups)     // Préparation du logiciel avec la position FEN
        {   // --- La méthode remplit les listes avec les pièces et les affiche à la fin ---
            GestionDamier.ViderDamier();            
            ListePionsBlancs.Clear();           // Clearer la liste DamierContenu et les ListePions... ListesDames...
            ListePionsNoirs.Clear();
            ListeDamesBlanches.Clear();
            ListeDamesNoires.Clear();     
            Console.WriteLine("Mise en place de la position FEN : " + fenAutiliser);
            foreach (var pict in BrunoInterfaceGraphiqueDames.CaseDamier)
            {
                pict.Image = null;          // Efface l'image de chaque PictureBox
            }
            if (ajouteListeCoups)
            {
                ListeCoupsFen.Add(fenAutiliser);    // On enregistre la position dans les listes de coups
                ListeCoupsHub2.Add(ConvertitFenVersHub2(fenAutiliser));
                // Note : On ne touche pas à ListeCoupsPdn car aucun n'a été joué pour l'instnat
            }
            // Trouver la première occurrence de guillemet
            int debutIndex = fenAutiliser.IndexOf("\"") + 1;        // On commence après le premier guillemet
            int finIndex = fenAutiliser.IndexOf("\"", debutIndex);  // On trouve le prochain guillemet après le debutIndex
            if (debutIndex >= 0 && finIndex > debutIndex)
            {   // Extraire le texte entre les guillemets
                string contenu = fenAutiliser.Substring(debutIndex, finIndex - debutIndex);
                string[] champsFen = contenu.Split(':');           // On récupère les 3 champs du FEN dans un tableau
                // Le champ 0 est la couleur qui doit jouer le prochain coup 
                BrunoInterfaceGraphiqueDames.CouleurAuTrait = (champsFen[0] == "W") ? "Blanc" : (champsFen[0] == "B") ? "Noir" : "Inconnu";
                // Active les cases pour la couleur actuelle
                BrunoInterfaceGraphiqueDames.ActiveCouleurDamier(BrunoInterfaceGraphiqueDames.CouleurAuTrait, true);
                // Désactive les cases de l'autre couleur
                BrunoInterfaceGraphiqueDames.ActiveCouleurDamier(BrunoInterfaceGraphiqueDames.CouleurAuTrait == "Blanc" ? "Noir" : "Blanc", false);

                // Parcourir les deux champs des pièces (W... et B...)
                for (int i = 1; i < champsFen.Length; i++)
                {   // Identifier la couleur (W pour blancs, B pour noirs)
                    char couleur = champsFen[i][0]; // Premier caractère : 'W' ou 'B'
                    string[] pieces = champsFen[i].Substring(1).Split(','); // Enlever le 'W' ou 'B' et découper les pièces
                    foreach (string piece in pieces)
                    {
                        if (piece.StartsWith("K"))
                        {   // C'est une dame
                            int numeroCase = int.Parse(piece.Substring(1)); // Numéro après le 'K'
                            if (couleur == 'W')
                                ListeDamesBlanches.Add(numeroCase);
                            else
                                ListeDamesNoires.Add(numeroCase);
                        }
                        else if (piece.Contains("-"))
                        {   // C'est une suite de pions
                            string[] range = piece.Split('-');
                            int start = int.Parse(range[0]);
                            int end = int.Parse(range[1]);

                            // Ajouter toutes les cases dans l'intervalle
                            for (int numeroCase = start; numeroCase <= end; numeroCase++)
                            {
                                if (couleur == 'W')
                                    ListePionsBlancs.Add(numeroCase);               // On ajoute le pion à la liste
                                else
                                    ListePionsNoirs.Add(numeroCase);                // On ajoute le pion à la liste
                            }
                        }
                        else
                        {   // C'est un pion
                            int numeroCase = int.Parse(piece);              // Directement le numéro de case
                            if (couleur == 'W')
                                ListePionsBlancs.Add(numeroCase);               // On ajoute le pion à la liste
                            else
                                ListePionsNoirs.Add(numeroCase);                // On ajoute le pion à la liste
                        }
                    }
                }
                DessinePiecesAvecListes();
            }
            else
            {
                Console.WriteLine("Aucun texte trouvé entre les guillemets.");
            }
        }
        public static string RecupereFEN()
        {   // --- Génère et renvoie le FEN de la position actuelle ---
            string baseFEN = "[FEN \"";            // Début de la chaîne FEN.
            baseFEN += BrunoInterfaceGraphiqueDames.CouleurAuTrait == "Blanc" ? "W" : "B";  // Couleur au trait.
            // Ajout des pions et dames blancs.
            baseFEN += ":W" + string.Join(",", ListePionsBlancs);
            foreach (int dame in ListeDamesBlanches)
            {   // Ajoute les dames blanches avec le bon préfixe.
                baseFEN += ListePionsBlancs.Count == 0 && baseFEN.EndsWith(":W") ? "K" + dame : ",K" + dame;
            }
            // Ajout des pions et dames noirs.
            baseFEN += ":B" + string.Join(",", ListePionsNoirs);
            foreach (int dame in ListeDamesNoires)
            {   // Ajoute les dames noires avec le bon préfixe.
                baseFEN += ListePionsNoirs.Count == 0 && baseFEN.EndsWith(":B") ? "K" + dame : ",K" + dame;
            }

            baseFEN += "\"]";            // Fin de la chaîne FEN.
            return baseFEN;
        }
        public static string ConvertitFenVersHub2(string fen)
        {   // --- Convertit une position FEN en format Hub2 compréhensible par SCAN 3.1 ---
            string fenNettoye = fen.Trim(); // Supprime les espaces inutiles
            if (fenNettoye.StartsWith("[FEN \"") && fenNettoye.EndsWith("\"]"))
            {   // Nettoyer la FEN pour extraire le contenu entre les guillemets
                fenNettoye = fenNettoye.Substring(6, fenNettoye.Length - 8);    // Enlever [FEN " et "]
            }
            else
            {
                throw new ArgumentException("FEN invalide : format attendu [FEN \"...\"]");
            }
            char[] hub2 = new char[51];         // Tableau des 51 caractères : 1 pour le trait, 50 pour les cases
            string[] partieFEN = fenNettoye.Split(':');     // Décomposer la FEN nettoyée
            char coteQuivaJouer = partieFEN[0][0];          // Joueur au trait ('W' ou 'B')
            hub2[0] = coteQuivaJouer;   // Premier caractère

            for (int i = 1; i <= 50; i++)   // Initialiser le damier avec des cases vides
            {
                hub2[i] = 'e'; // 'e' pour case vide
            }
            for (int i = 1; i < partieFEN.Length; i++)      // Parcourir les positions de pièces
            {
                char pieceColor = partieFEN[i][0];      // 'W' ou 'B'
                string positions = partieFEN[i].Substring(1);   // Positions (ex : "18,24,K10", ou "31-50")
                foreach (string position in positions.Split(','))
                {
                    bool isKing = position.StartsWith("K");
                    string range = isKing ? position.Substring(1) : position;
                    if (range.Contains('-'))
                    {   // Gérer les plages comme "31-50"
                        string[] bounds = range.Split('-');
                        int start = int.Parse(bounds[0]);
                        int end = int.Parse(bounds[1]);
                        for (int manouryIndex = start; manouryIndex <= end; manouryIndex++)
                        {
                            PlacePiece(hub2, manouryIndex, pieceColor, isKing);
                        }
                    }
                    else
                    {
                        // Gérer une position unique
                        int manouryIndex = int.Parse(range);
                        PlacePiece(hub2, manouryIndex, pieceColor, isKing);
                    }
                }
            }
            return new string(hub2);
        }
        private static void PlacePiece(char[] hub2, int indexManoury, char couleur, bool isKing)
        {   // --- Place une pièce, pion ou dame (king) dans le tableau Hub2 ---
            if (indexManoury < 1 || indexManoury > 50)
            {   // Valider l'index Manoury directement
                throw new ArgumentException($"Index Manoury {indexManoury} est hors limites.");
            }
            // Placer la pièce (pion ou dame) sur le plateau
            if (couleur == 'W') hub2[indexManoury] = isKing ? 'W' : 'w';
            else if (couleur == 'B') hub2[indexManoury] = isKing ? 'B' : 'b';
        }
        public static void DessinePiecesAvecListes()
        {   //  --- Dessine l'ensemble des pièces sur le damier à partir des listes (et pas de DamierContenu) ---
            BrunoInterfaceGraphiqueDames brunoInterface = new BrunoInterfaceGraphiqueDames();
            DessinerPieces(ListePionsBlancs, LogiqueMouvementsDames.TypePiece.PionBlanc, LogiqueMouvementsDames.CouleurPiece.Blanc, brunoInterface);
            DessinerPieces(ListePionsNoirs, LogiqueMouvementsDames.TypePiece.PionNoir, LogiqueMouvementsDames.CouleurPiece.Noir, brunoInterface);
            DessinerPieces(ListeDamesBlanches, LogiqueMouvementsDames.TypePiece.DameBlanche, LogiqueMouvementsDames.CouleurPiece.Blanc, brunoInterface);
            DessinerPieces(ListeDamesNoires, LogiqueMouvementsDames.TypePiece.DameNoire, LogiqueMouvementsDames.CouleurPiece.Noir, brunoInterface);
        }
        public static void DessinerPieces(List<int> listeCases, TypePiece pieceType, CouleurPiece pieceCouleur, BrunoInterfaceGraphiqueDames brunoInterface)
        {   // --- Dessine toutes les pièces de la liste listeCases ---
            if (listeCases.Count > 0)
            {
                foreach (int numeroCase in listeCases)
                {   // Obtenir l'indice de la PictureBox associée
                    int indexPictureBox = GestionDamier.ObtenirIndicePictureBox(numeroCase);
                    // Dessiner la pièce et met à jour DamierContenu
                    brunoInterface.DessinePiece(indexPictureBox, pieceType, pieceCouleur);
                }
            }
        }
        public static void MiseaJourListes(int indexPictureBox, LogiqueMouvementsDames.TypePiece pieceType, bool ajouterDansListe)
        {   // --- Met à jour la liste avec la pièce située en indexPictureBox, l'ajoute si ajouterDansListe, l'enlève sinon ---
            int caseMiseaJour = GestionDamier.ObtenirCaseManoury(indexPictureBox);
            string nomListe;    // Obtenir la liste correspondante et son nom
            List<int>? listeCorrespondante = ListeCorrespondantTypePiece(pieceType, out nomListe);
            if (listeCorrespondante != null)
            {   // Ajouter ou retirer la case dans la liste
                if (ajouterDansListe)
                {
                    if (!listeCorrespondante.Contains(caseMiseaJour))
                    {
                        listeCorrespondante.Add(caseMiseaJour);
                    }
                }
                else
                {
                    if (listeCorrespondante.Contains(caseMiseaJour))
                    {
                        listeCorrespondante.Remove(caseMiseaJour);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Aucune liste trouvée pour le type de pièce : {pieceType}");
            }
        }
        public static void MiseaJourPromotion (int indexPictureBox, ContenuCase pieceSource)
        {   // --- Met à jour la liste avec la pièce promue --- 
            int casePionSource = GestionDamier.ObtenirCaseManoury(indexPictureBox);
            string nomListe;    // Obtenir la liste correspondante et son nom
            List<int>? listeCorrespondante = ListeCorrespondantTypePiece(pieceSource.TypePiece, out nomListe);
            if (listeCorrespondante != null)
            {   // Ajouter ou retirer la case dans la liste
                if (listeCorrespondante.Contains(casePionSource))
                {
                    listeCorrespondante.Remove(casePionSource);
                }
                else
                {
                    Console.WriteLine($"Aucun pion trouvé : {casePionSource} dans la liste {nomListe}");
                }
            }
            else
            {
                Console.WriteLine($"Aucune liste trouvée pour le type de pièce : {pieceSource.TypePiece}");
            }
        }
        public static List<int>? ListeCorrespondantTypePiece(LogiqueMouvementsDames.TypePiece typePiece, out string nomListe)
        {   // --- Le paramètre out permet de retourner à la fois la liste et son nom (string) dans une seule méthode. ---
            switch (typePiece)
            {
                case LogiqueMouvementsDames.TypePiece.PionBlanc:
                    nomListe = "ListePionsBlancs";
                    return ListePionsBlancs;
                case LogiqueMouvementsDames.TypePiece.PionNoir:
                    nomListe = "ListePionsNoirs";
                    return ListePionsNoirs;
                case LogiqueMouvementsDames.TypePiece.DameBlanche:
                    nomListe = "ListeDamesBlanches";
                    return ListeDamesBlanches;
                case LogiqueMouvementsDames.TypePiece.DameNoire:
                    nomListe = "ListeDamesNoires";
                    return ListeDamesNoires;
                default:
                    nomListe = "Aucune liste";
                    return null;
            }
        }
    }
}