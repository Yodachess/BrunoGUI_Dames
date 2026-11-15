// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Media;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BrunoGUI_Dames.FichierPartiePdn;
using MethodInvoker = System.Windows.Forms.MethodInvoker;

namespace BrunoGUI_Dames
{
    public partial class BrunoInterfaceGraphiqueDames
    {
        // Dessin du damier, trait, fin de partie et nulles.
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Routines de Dessin
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐

        public static void DessinePiece(int indexPictureBox, TypePiece pieceType)
        {   // --- Dessin de la pièce passée en paramètre sur la case PictureBox en paramètre (l'image seulement) ---
            // La position (PositionCourante) n'est pas touchée ici : elle change à la mise en place d'un FEN et une fois par coup (AppliqueCoupALaPosition)
            if (pieceType != TypePiece.Vide && ListeBitmapsPiece.TryGetValue(pieceType, out Bitmap? image))
                CaseDamier[indexPictureBox].Image = image;      // L'image correspond au type de pièce
            else
                CaseDamier[indexPictureBox].Image = null;       // Case vide, ou aucune image pour ce type
        }
        public static void EffacePiece(int indexPictureBox)
        {   // --- Efface l'image de la pièce située à indexPictureBox ; PositionCourante n'est pas touchée ---
            CaseDamier[indexPictureBox].Image = null;
        }        public static void DessineMouvements(int caseSourceManoury, List<Coup> coups)
        {   // --- Dessine tous les mouvements légaux de la pièce en case source (coups : ses coups selon ReglesDames, déjà limités aux rafles les plus longues) ---
            CaseDamier[GestionDamier.ObtenirIndicePictureBox(caseSourceManoury)].BackColor = CouleurCaseDepart; // Colorier la case source
            if (coups.Any(coup => coup.EstPrise))
            {   // Rafles : s'il en existe plusieurs de même longueur, elles sont toutes signalées, c'est au joueur de choisir
                // Étape 1 : traiter les rafles (une étape par pièce prise : case de la pièce prise et case d'arrivée)
                foreach (Coup rafle in coups)
                {
                    foreach (var (prise, arrivee) in rafle.Prises.Zip(rafle.Chemin, (prise, arrivee) => (prise, arrivee)))
                    {
                        int indexPrise = GestionDamier.ObtenirIndicePictureBox(prise);
                        int indexArrivee = GestionDamier.ObtenirIndicePictureBox(arrivee);
                        if (CaseDamier[indexArrivee].Image == null)       // Ne pas effacer les pièces si la case est occupée
                        {
                            CaseDamier[indexPrise].BackColor = CouleurCasePrise;
                            CaseDamier[indexArrivee].Image = CadreGrisSansPrise;
                            CaseDamier[indexArrivee].BackColor = CouleurCaseEtape;    // Les cases d'arrivée de chaque étape sont colorées en plus des pièces prises
                            // Sauvegarder les cases pour les étapes ultérieures
                            ListeCasesMouvementPrecedent.Add(prise);
                            ListeCasesMouvementPrecedent.Add(arrivee);
                            ListeCasesDestination.Add(arrivee); // Garder les arrivées pour vérification finale
                        }
                    }
                }
                // Étape 2 : colorier uniquement les cases d'arrivée finale en CouleurCaseArrivee
                foreach (Coup rafle in coups)
                    CaseDamier[GestionDamier.ObtenirIndicePictureBox(rafle.Arrivee)].BackColor = CouleurCaseArrivee;
            }
            else
            {   // Déplacements simples (uniquement si aucune rafle n'existe), affichés en CouleurTrajetSuivi avec un cadre gris
                foreach (Coup coup in coups)
                {
                    int index = GestionDamier.ObtenirIndicePictureBox(coup.Arrivee);
                    CaseDamier[index].Image = CadreGrisSansPrise;         // On affiche un cadre gris sur les cases de déplacement simple
                    CaseDamier[index].BackColor = CouleurTrajetSuivi;     // On colorie les cases de déplacement simple avec CouleurTrajetSuivi
                    ListeCasesMouvementPrecedent.Add(coup.Arrivee);
                    ListeCasesDestination.Add(coup.Arrivee);
                }
            }
        }
        public static void EffaceMouvements()
        {   // --- Efface toutes les indications des mouvements pour déplacer la pièce ---
            EffaceCasesDestination();   // Efface les cadres gris des cases de destination générées lors de l'AnalyseCoupoObligatoire
            for (int i = 0; i < ListeCasesDestination.Count; i++)           // On enlève tous les cadres gris
            {   // On enlève les cadres gris
                CaseDamier[GestionDamier.ObtenirIndicePictureBox(ListeCasesDestination[i])].Image = null;
            }
            ListeCasesDestination.Clear();
            for (int i = 0; i < ListeCasesMouvementPrecedent.Count; i++)    // On enlève tous les mouvements affichés précédemment
            {   // On remet la couleur de base sur les cases
                CaseDamier[GestionDamier.ObtenirIndicePictureBox(ListeCasesMouvementPrecedent[i])].BackColor = BrunoInterfaceGraphiqueDames.CouleurCaseSombre;
            }
            ListeCasesMouvementPrecedent.Clear();
        }
        public static void EffaceCasesDestination()
        {   // --- Efface les cases de destination des mouvements possibles ---
            for (int i = 0; i < ListeCasesDestination.Count; i++)           // On enlève tous les cadres gris
            {   // On enlève les cadres gris
                CaseDamier[GestionDamier.ObtenirIndicePictureBox(ListeCasesDestination[i])].Image = null;
            }
            ListeCasesDestination.Clear();
        }
        private void CreeDamier()
        {    // --- Crée les 100 PictureBox représentant les cases du damier (Note : n'affiche pas les pièces sur le damier) ---
            Color couleur;
            int index = 0;          // Les PictureBox sont numérotées de 1 à 100
            int caseLogique = 1;    // Les cases sombres (utilisées pour jouer) sont numérotées de 1 à 50 en partant du haut à gauche
            for (int ligne = 9; ligne >= 0; ligne--)
            {
                couleur = ligne % 2 == 0 ? CouleurCaseSombre : CouleurCaseClaire;       // Couleur des cases du damier
                for (int colonne = 0; colonne <= 9; colonne++)
                {
                    int indexCourant = index;               // Capturer la valeur actuelle de l'index 
                    int caseLogiqueCourant = caseLogique;   // Capturer la valeur actuelle de caseLogique
                    PictureBox Pict = new PictureBox
                    {   //  Les cases sont des PictureBox indéxées, par exemple : case a1 = CaseDamier21, case h8 = CaseDamier98
                        Name = "CaseDamier" + index.ToString(),
                        BackColor = couleur,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Size = new Size(60, 60),    // Taille des case = 60 * 60 pixels
                        Location = new Point(20 + (colonne * 60), 560 - (ligne * 60)),  // 20 et 560 pour ajuster les cases dans le chassis
                        Visible = ligne >= 0 & ligne < 11 & colonne >= 0 & colonne < 11, // On ne rend visible que les 100 cases utiles
                        Enabled = false
                    };
                    // Vérifier si la case est sombre
                    if ((ligne % 2 == 0 && colonne % 2 == 0) || (ligne % 2 != 0 && colonne % 2 != 0))
                    {   // Case sombre : elle porte un numéro Manoury
                        AfficheIndexBox(Pict, indexCourant, caseLogiqueCourant);
                        caseLogique++; // Passer à la case logique suivante
                    }
                    Pict.BringToFront();
                    CaseDamier.Add(Pict);
                    Pict.MouseDown += CaseMouseDown;
                    Damier10x10.Controls.Add(Pict);
                    index++;
                    couleur = couleur == CouleurCaseClaire ? CouleurCaseSombre : CouleurCaseClaire;
                }
            }
        }
        public void ChangerCouleurTrait()
        {   // Inverse la valeur de CouleurAuTrait
            CouleurAuTrait = Position.Adversaire(CouleurAuTrait);
            // Active les cases pour la couleur actuelle
            ActiveCouleurDamier(CouleurAuTrait, true);
            // Désactive les cases de l'autre couleur
            ActiveCouleurDamier(Position.Adversaire(CouleurAuTrait), false);
            LabelInformationJoueur.Text = "Aux " + CouleurAuTrait + "s de jouer";
        }
        public static Couleur CouleurPasAuTrait()
        {   // Retourne l'opposé de CouleurAuTrait
            return Position.Adversaire(CouleurAuTrait);
        }
        public static void ActiveDamier(bool statut)
        {   // --- Active (statut = true) ou désactive (statut = false) les cases du damier ---
            if (!(statut && PartieTerminee))
            {
                for (int i = 0; i <= 99; i++)
                    if (CaseDamier[i].Visible)
                        CaseDamier[i].Enabled = statut;
            }
        }
        public static void ActiveCouleurDamier(Couleur couleur, bool statut)
        {   // Active (statut = true) ou désactive les cases qui portent une pièce de cette couleur dans la position
            ActiveCouleurDamier(GestionDamier.CouleurAffichee(couleur), statut);
        }
        public static void ActiveCouleurDamier(CouleurPiece couleur, bool statut)
        {   // Idem avec la couleur de l'affichage (celle de la pièce jouée)
            for (int i = 0; i <= 99; i++)
            {
                if (GestionDamier.ContenuCaseIndice(i).CouleurPiece == couleur)
                {
                    CaseDamier[i].Enabled = statut;
                }
            }
        }        public static void ActiverCaseManoury(int caseManoury, bool statut)
        {   // --- Active ou désactive une case Manoury (1 à 50) ---
            CaseDamier[GestionDamier.ObtenirIndicePictureBox(caseManoury)].Enabled = statut;
        }
        public void CouleurToutesCasesManoury(Color couleur)
        {   // --- Colorie toutes les cases actives (Manoury) avec la couleur demandée ---
            for (int ligne = 0; ligne < 10; ligne++) // 10 lignes
            {
                for (int colonne = 0; colonne < 10; colonne++) // 10 colonnes
                {   // Les cases Manoury sont les cases sombres : (ligne + colonne) % 2 != 0
                    if ((ligne + colonne) % 2 != 0)
                    {
                        int index = ligne * 10 + colonne; // Calculer l'index de la PictureBox
                        CaseDamier[index].BackColor = couleur;
                    }
                }
            }
        }
        public void VerifierGain()
        {   // --- Verifie le gain uniquement sur a présence de pièces ---
            if (GestionDamier.NombreDePieces(Couleur.Blanc) == 0)
            {   // Vérifier si les listes blanches sont vides
                GroupResultat.Enabled = false;   // Désactive le groupe de boutons
                RadioGainNoir.Checked = PartieTerminee = true;  // Note : RadioGainNoir.Checked mis à true lance la méthode RadioGainNoir_CheckedChanged
                LabelInformationJoueur.Text = "Les Noirs gagnent, les blancs n'ont plus de pièces";
                ReglagesDeTest.Message("Les Noirs gagnent !\n Les blancs n'ont plus de pièces", "Partie terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (GestionDamier.NombreDePieces(Couleur.Noir) == 0)
            {   // Vérifier si les listes noires sont vides
                GroupResultat.Enabled = false;   // Désactive le groupe de boutons
                RadioGainBlanc.Checked = PartieTerminee = true; // Note: RadioGainBlanc.Checked mis à true lance la méthode RadioGainBlanc_CheckedChanged
                LabelInformationJoueur.Text = "Les Blancs gagnent, les noirs n'ont plus de pièces";
                ReglagesDeTest.Message("Les Blancs gagnent !\n Les noirs n'ont plus de pièces", "Partie terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            // Le joueur qui ne peut plus bouger perd : c'est FinPartieParBlocage, appelée par AnalyseCoupObligatoire et avant d'interroger le moteur
            // Les parties nulles (répétition, coups de dames) : NulleActuelle et FinPartieParNulle ; pas détectées : finales à peu de pièces
        }
        private void FinPartieParBlocage(Couleur couleurBloquee)
        {   // --- Le camp de couleurBloquee, au trait, ne peut plus jouer : il perd la partie ---
            if (PartieTerminee)
                return;
            GroupResultat.Enabled = false;   // Désactive le groupe de boutons
            if (couleurBloquee == Couleur.Blanc)
            {
                RadioGainNoir.Checked = PartieTerminee = true;  // Note : RadioGainNoir.Checked mis à true lance la méthode RadioGainNoir_CheckedChanged
                LabelInformationJoueur.Text = "Les Noirs gagnent, les blancs n'ont plus de coup possible";
                ReglagesDeTest.Message("Les Noirs gagnent !\n Les blancs n'ont plus de coup possible", "Partie terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                RadioGainBlanc.Checked = PartieTerminee = true; // Note: RadioGainBlanc.Checked mis à true lance la méthode RadioGainBlanc_CheckedChanged
                LabelInformationJoueur.Text = "Les Blancs gagnent, les noirs n'ont plus de coup possible";
                ReglagesDeTest.Message("Les Blancs gagnent !\n Les noirs n'ont plus de coup possible", "Partie terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private static RaisonNulle NulleActuelle()
        {   // --- Les règles de nulle (NullesDames) appliquées à l'historique de la partie : la position de départ puis celle après chaque demi-coup ---
            return NullesDames.Detecte(Partie.Courante.Positions);
        }
        private void FinPartieParNulle(RaisonNulle raison)
        {   // --- La partie est nulle (répétition de position ou coups de dames) : on l'annonce et on la termine (résultat 1/2-1/2) ---
            if (PartieTerminee)
                return;
            string motif = raison switch
            {
                RaisonNulle.Repetition => "la même position s'est présentée trois fois",
                RaisonNulle.FinaleSeizeCoups => $"finale à peu de pièces : {NullesDames.CoupsFinale3Pieces} coups de chaque joueur sans gagner",
                RaisonNulle.FinaleCinqCoups => $"finale à peu de pièces : {NullesDames.CoupsFinaleSimple} coups de chaque joueur sans gagner",
                _ => $"{NullesDames.CoupsDeDamesParJoueur} coups de dames de chaque joueur, sans déplacement de pion ni prise"
            };
            GroupResultat.Enabled = false;   // Désactive le groupe de boutons
            RadioNulle.Checked = PartieTerminee = true;      // Note : RadioNulle.Checked mis à true lance la méthode RadioNulle_CheckedChanged
            LabelInformationJoueur.Text = "Partie nulle : " + motif;
            ReglagesDeTest.Message("Nulle possible, la partie est terminée :\n" + motif, "Partie nulle", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void MetEnEvidenceRafle(Coup rafle)
        {   // --- Met en évidence sur le damier une rafle proposée au choix : le chemin, la case d'arrivée et, en rouge, les pièces prises ---
            CouleurToutesCasesManoury(CouleurCaseSombre);
            CaseDamier[GestionDamier.ObtenirIndicePictureBox(CaseSourceManoury)].BackColor = CouleurCaseDepart;
            foreach (int caseDuChemin in rafle.Chemin)
                CaseDamier[GestionDamier.ObtenirIndicePictureBox(caseDuChemin)].BackColor = CouleurCaseEtape;
            CaseDamier[GestionDamier.ObtenirIndicePictureBox(rafle.Arrivee)].BackColor = CouleurCaseArrivee;
            foreach (int prise in rafle.Prises)
                CaseDamier[GestionDamier.ObtenirIndicePictureBox(prise)].BackColor = Color.Red;
        }
        private Coup? ChoisitRafle(List<Coup> options)
        {   // --- Fenêtre de choix entre plusieurs rafles de même départ et même arrivée (les pièces prises de chaque option sont mises en évidence) ; null si le joueur annule ---
            using ChoixRafleForm dialogue = new ChoixRafleForm(options, MetEnEvidenceRafle);
            Rectangle zone = Screen.FromControl(this).WorkingArea;
            int x = Right + 8 + dialogue.Width <= zone.Right ? Right + 8 : Math.Max(zone.Left, Left - dialogue.Width - 8);      // À côté de la fenêtre, pour ne pas cacher le damier
            dialogue.StartPosition = FormStartPosition.Manual;
            dialogue.Location = new Point(x, Top + 120);
            dialogue.ShowDialog(this);
            CouleurToutesCasesManoury(CouleurCaseSombre);       // Efface la mise en évidence
            return dialogue.Choisie;
        }
        private bool EstCaseValide(int caseDestinationManoury)
        {   // C'est une case valide si un coup légal de la pièce sélectionnée (selon ReglesDames) y arrive
            return coupsLegauxPieceSelectionnee != null && coupsLegauxPieceSelectionnee.Any(coup => coup.Arrivee == caseDestinationManoury);
        }
        private void InitialiserEtatMouvement()
        {   // --- Remet à zéro les paramètes utilisés pour dessiner les mouvements possibles ---
            pieceCouleurSource = new ContenuCase();
            CaseSourceManoury = 0;
            CaseDestinationManoury = 0;
            IndexPictureBoxSource100 = -1;
            clickCaseSource = true; // Retourne à l'état de sélection de pièce
        }
        public static void AfficheListesPieces()
        {   // --- Affiche les nombres de pions blancs et noirs, de dames blanches et noires de la position ---
            if (fenetrePrincipale == null)
                return;
            Position position = GestionDamier.PositionCourante;
            fenetrePrincipale.LabelPionsBlancs.Text = position.Compte(Piece.PionBlanc) + " pions blancs";
            fenetrePrincipale.LabelPionsNoirs.Text = position.Compte(Piece.PionNoir) + " pions noirs";
            fenetrePrincipale.LabelDamesBlanches.Text = position.Compte(Piece.DameBlanche) + " Dames blanches";
            fenetrePrincipale.LabelDamesNoires.Text = position.Compte(Piece.DameNoire) + " Dames noires";
        }
        public static void AfficheIndexBox(PictureBox pict, int index, int caseLogique)
        {   // --- Affiche les index Box et Manoury an haut à gauche et droite ---
            pict.Tag = $"{index},{caseLogique}";    // Stocker l'index et la case logique sous forme de chaîne dans Tag
            pict.Paint += PictureBox_Paint;         // Ajouter le gestionnaire Paint à la PictureBox
            pict.Invalidate();                      // Forcer la mise à jour
        }
        private static void PictureBox_Paint(object? sender, PaintEventArgs e)       // Déclaration du gestionnaire d'événement Paint
        {   // --- Evénement Paint lié à la PictureBox ---
            if (sender is not PictureBox pict || pict.Tag?.ToString() is not string tag)
                return;     // Le Tag est posé par AfficheIndexBox pour chaque case
            // Récupérer les données depuis Tag et les séparer
            string[] data = tag.Split(',');
            int index = int.Parse(data[0]);
            int caseLogique = int.Parse(data[1]);
            // Effacer le fond
            e.Graphics.FillRectangle(new SolidBrush(pict.BackColor), 0, 0, pict.Width, pict.Height);
            // Redessiner l'image si elle existe
            if (pict.Image != null)
            {
                e.Graphics.DrawImage(pict.Image, 0, 0, pict.Width, pict.Height);
            }
            if (AfficheCaseLogique)            // Afficher l'index logique et l'index Manoury
            {
                string text2 = caseLogique.ToString();
                using (Font font2 = new Font("Arial", 10, FontStyle.Bold))
                {
                    Brush brush2 = Brushes.Cyan;
                    PointF position2 = new PointF(pict.Width - 20, 3); // Position ajustée
                    e.Graphics.DrawString(text2, font2, brush2, position2);
                }
            }
            if (AfficheNumeroBox)            // Afficher l'index de la case
            {
                string text = index.ToString();
                using (Font font = new Font("Arial", 8))
                {
                    Brush brush = Brushes.Black;
                    PointF position = new PointF(3, 3); // Position ajustée
                    e.Graphics.DrawString(text, font, brush, position);
                }
            }
        }
    }
}
