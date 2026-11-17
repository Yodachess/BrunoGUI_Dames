// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using static BrunoGUI_Dames.Texte;
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

        public void DessinePiece(int indexPictureBox, Piece piece)
        {   // --- Dessin de la pièce passée en paramètre sur la case PictureBox en paramètre (l'image seulement) ---
            // La position (PositionCourante) n'est pas touchée ici : elle change à la mise en place d'un FEN et une fois par coup (AppliqueCoupALaPosition)
            if (ListeBitmapsPiece.TryGetValue(piece, out Bitmap? image))
                CaseDamier[indexPictureBox].Image = image;      // L'image de la pièce
            else
                CaseDamier[indexPictureBox].Image = null;       // Case vide
        }
        public void EffacePiece(int indexPictureBox)
        {   // --- Efface l'image de la pièce située à indexPictureBox ; PositionCourante n'est pas touchée ---
            CaseDamier[indexPictureBox].Image = null;
        }        public void DessineMouvements(int caseSourceManoury, List<Coup> coups)
        {   // --- Dessine tous les mouvements légaux de la pièce en case source (coups : ses coups selon ReglesDames, déjà limités aux rafles les plus longues) ---
            EffaceNumerosEtape();
            CaseDamier[Damier.ObtenirIndicePictureBox(caseSourceManoury)].BackColor = CouleurCaseDepart; // Colorier la case source
            if (coups.Any(coup => coup.EstPrise))
            {   // Rafles : s'il en existe plusieurs de même longueur, elles sont toutes signalées, c'est au joueur de choisir
                // Étape 1 : traiter les rafles (une étape par pièce prise : case de la pièce prise et case d'arrivée)
                foreach (Coup rafle in coups)
                {
                    foreach (var (prise, arrivee) in rafle.Prises.Zip(rafle.Chemin, (prise, arrivee) => (prise, arrivee)))
                    {
                        int indexPrise = Damier.ObtenirIndicePictureBox(prise);
                        int indexArrivee = Damier.ObtenirIndicePictureBox(arrivee);
                        if (CaseDamier[indexArrivee].Image == null || CaseDamier[indexArrivee].Image == CadreGrisSansPrise)       // Ne pas effacer les pièces si la case est occupée (un cadre gris, déjà posé au début du tour, compte comme une case vide)
                        {
                            CaseDamier[indexPrise].BackColor = CouleurCasePrise;
                            CaseDamier[indexArrivee].Image = CadreGrisSansPrise;
                            CaseDamier[indexArrivee].BackColor = CouleurCaseEtape;    // Les cases d'arrivée de chaque étape sont colorées en plus des pièces prises
                        }
                    }
                }
                // Étape 2 : colorier uniquement les cases d'arrivée finale en CouleurCaseArrivee
                foreach (Coup rafle in coups)
                    CaseDamier[Damier.ObtenirIndicePictureBox(rafle.Arrivee)].BackColor = CouleurCaseArrivee;
                // Étape 3 : l'ordre des étapes quand il n'y a qu'une rafle (avec plusieurs, les numéros se mélangeraient : la fenêtre de choix les montre une à une)
                List<Coup> rafles = coups.Where(coup => coup.EstPrise).ToList();
                if (rafles.Count == 1)
                    MarqueEtapes(rafles[0]);
            }
            else
            {   // Déplacements simples (uniquement si aucune rafle n'existe), affichés en CouleurTrajetSuivi avec un cadre gris
                foreach (Coup coup in coups)
                {
                    int index = Damier.ObtenirIndicePictureBox(coup.Arrivee);
                    CaseDamier[index].Image = CadreGrisSansPrise;         // On affiche un cadre gris sur les cases de déplacement simple
                    CaseDamier[index].BackColor = CouleurTrajetSuivi;     // On colorie les cases de déplacement simple avec CouleurTrajetSuivi
                }
            }
        }
        public void EffaceMouvements()
        {   // --- Efface toutes les indications des mouvements possibles d'une pièce sélectionnée : les cadres gris et les couleurs des cases ---
            EffaceCasesDestination();
            EffaceNumerosEtape();
            CouleurToutesCasesManoury(CouleurCaseSombre);       // Au clic de destination, seules les indications de la sélection sont affichées (tout a été remis à zéro à la sélection)
        }
        public void EffaceCasesDestination()
        {   // --- Efface les cadres gris que le damier montre sur des cases vides (destinations possibles, cases d'arrivée d'une prise obligatoire) : on les reconnaît à leur image ---
            foreach (PictureBox caseDuDamier in CaseDamier)
                if (caseDuDamier.Image == CadreGrisSansPrise)
                    caseDuDamier.Image = null;
        }        private void CreeDamier()
        {    // --- Crée les 100 PictureBox représentant les cases du damier (Note : n'affiche pas les pièces sur le damier) ---
            Color couleur;
            int index = 0;          // Les PictureBox sont numérotées de 1 à 100
            int caseLogique = 1;    // Les cases sombres (utilisées pour jouer) sont numérotées de 1 à 50 en partant du haut à gauche
            for (int ligne = 9; ligne >= 0; ligne--)
            {
                couleur = ligne % 2 == 0 ? CouleurCaseSombre : CouleurCaseClaire;       // Couleur des cases du damier
                for (int colonne = 0; colonne <= 9; colonne++)
                {
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
                        PoseNumeroCase(Pict, caseLogiqueCourant);
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
        // --- Le nom d'un camp et « Trait aux … » dans la langue de l'affichage ---
        private static string NomCamp(Couleur couleur) => T(couleur == Couleur.Blanc ? "Blancs" : "Noirs");
        private static string TraitAux(Couleur couleur) => T("Trait aux {0}", NomCamp(couleur));
        public void ChangerCouleurTrait()
        {   // Inverse la valeur de CouleurAuTrait
            CouleurAuTrait = Position.Adversaire(CouleurAuTrait);
            // Active les cases pour la couleur actuelle
            ActiveCouleurDamier(CouleurAuTrait, true);
            // Désactive les cases de l'autre couleur
            ActiveCouleurDamier(Position.Adversaire(CouleurAuTrait), false);
            LabelInformationJoueur.Text = T("Aux {0} de jouer", NomCamp(CouleurAuTrait));
        }
        public Couleur CouleurPasAuTrait()
        {   // Retourne l'opposé de CouleurAuTrait
            return Position.Adversaire(CouleurAuTrait);
        }
        public void ActiveDamier(bool statut)
        {   // --- Active (statut = true) ou désactive (statut = false) les cases du damier ---
            if (!(statut && PartieTerminee))
            {
                for (int i = 0; i <= 99; i++)
                    if (CaseDamier[i].Visible)
                        CaseDamier[i].Enabled = statut;
            }
        }
        public void ActiveCouleurDamier(Couleur couleur, bool statut)
        {   // Active (statut = true) ou désactive les cases qui portent une pièce de cette couleur dans la position
            for (int i = 0; i <= 99; i++)
            {
                int numeroCase = Damier.Orientation.CaseManoury(i);      // 0 : case claire, qui ne porte jamais de pièce
                if (numeroCase != 0 && Position.Appartient(Damier.PositionCourante[numeroCase], couleur))
                    CaseDamier[i].Enabled = statut;
            }
        }
        public void ActiverCaseManoury(int caseManoury, bool statut)
        {   // --- Active ou désactive une case Manoury (1 à 50) ---
            CaseDamier[Damier.ObtenirIndicePictureBox(caseManoury)].Enabled = statut;
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
        // --- Termine la partie : le résultat est coché dans le groupe Résultat (gagnant : Blanc ou Noir ; null pour une nulle), ce qui l'écrit dans la partie
        // (RadioXxx_CheckedChanged), puis le statut est affiché et le message annoncé. C'est le seul endroit où une partie se termine pour une raison de jeu ---
        private void TermineLaPartie(Couleur? gagnant, string statut, string message, string titre = "Partie terminée")
        {
            GroupResultat.Enabled = false;   // Désactive le groupe de boutons
            if (gagnant == Couleur.Blanc)
                RadioGainBlanc.Checked = PartieTerminee = true;     // Note : cocher la case lance RadioGainBlanc_CheckedChanged
            else if (gagnant == Couleur.Noir)
                RadioGainNoir.Checked = PartieTerminee = true;      // Note : cocher la case lance RadioGainNoir_CheckedChanged
            else
                RadioNulle.Checked = PartieTerminee = true;         // Note : cocher la case lance RadioNulle_CheckedChanged
            LabelInformationJoueur.Text = statut;
            ReglagesDeTest.Message(message, titre, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public void VerifierGain()
        {   // --- Vérifie le gain par l'absence de pièces (pas pendant la lecture d'une partie : elle ne fait que revoir des coups déjà joués) ---
            if (lecture != null)
                return;
            if (Damier.NombreDePieces(Couleur.Blanc) == 0)
                TermineLaPartie(Couleur.Noir, T("Les Noirs gagnent, les blancs n'ont plus de pièces"), T("Les Noirs gagnent !\n Les blancs n'ont plus de pièces"));
            if (Damier.NombreDePieces(Couleur.Noir) == 0)
                TermineLaPartie(Couleur.Blanc, T("Les Blancs gagnent, les noirs n'ont plus de pièces"), T("Les Blancs gagnent !\n Les noirs n'ont plus de pièces"));
            // Le joueur qui ne peut plus bouger perd : c'est FinPartieParBlocage, appelée par AnalyseCoupObligatoire et avant d'interroger le moteur
            // Les parties nulles (répétition, coups de dames, finales) : NulleActuelle et FinPartieParNulle
        }
        private void FinPartieParBlocage(Couleur couleurBloquee)
        {   // --- Le camp de couleurBloquee, au trait, ne peut plus jouer : il perd la partie ---
            if (PartieTerminee)
                return;
            if (couleurBloquee == Couleur.Blanc)
                TermineLaPartie(Couleur.Noir, T("Les Noirs gagnent, les blancs n'ont plus de coup possible"), T("Les Noirs gagnent !\n Les blancs n'ont plus de coup possible"));
            else
                TermineLaPartie(Couleur.Blanc, T("Les Blancs gagnent, les noirs n'ont plus de coup possible"), T("Les Blancs gagnent !\n Les noirs n'ont plus de coup possible"));
        }        private RaisonNulle NulleActuelle()
        {   // --- Les règles de nulle (NullesDames) appliquées à l'historique de la partie : la position de départ puis celle après chaque demi-coup ---
            return NullesDames.Detecte(Session.Positions);
        }
        private void FinPartieParNulle(RaisonNulle raison)
        {   // --- La partie est nulle (répétition de position ou coups de dames) : on l'annonce et on la termine (résultat 1/2-1/2) ---
            if (PartieTerminee)
                return;
            string motif = raison switch
            {
                RaisonNulle.Repetition => T("la même position s'est présentée trois fois"),
                RaisonNulle.FinaleSeizeCoups => T("finale à peu de pièces : {0} coups de chaque joueur sans gagner", NullesDames.CoupsFinale3Pieces),
                RaisonNulle.FinaleCinqCoups => T("finale à peu de pièces : {0} coups de chaque joueur sans gagner", NullesDames.CoupsFinaleSimple),
                _ => T("{0} coups de dames de chaque joueur, sans déplacement de pion ni prise", NullesDames.CoupsDeDamesParJoueur)
            };
            TermineLaPartie(null, T("Partie nulle : {0}", motif), T("Nulle possible, la partie est terminée :\n{0}", motif), "Partie nulle");
        }
        private void MetEnEvidenceRafle(Coup rafle)
        {   // --- Met en évidence sur le damier une rafle proposée au choix : le chemin, la case d'arrivée et, en rouge, les pièces prises ---
            CouleurToutesCasesManoury(CouleurCaseSombre);
            CaseDamier[Damier.ObtenirIndicePictureBox(CaseSourceManoury)].BackColor = CouleurCaseDepart;
            foreach (int caseDuChemin in rafle.Chemin)
                CaseDamier[Damier.ObtenirIndicePictureBox(caseDuChemin)].BackColor = CouleurCaseEtape;
            CaseDamier[Damier.ObtenirIndicePictureBox(rafle.Arrivee)].BackColor = CouleurCaseArrivee;
            foreach (int prise in rafle.Prises)
                CaseDamier[Damier.ObtenirIndicePictureBox(prise)].BackColor = Color.Red;
            EffaceNumerosEtape();
            MarqueEtapes(rafle);
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
            EffaceNumerosEtape();
            return dialogue.Choisie;
        }
        private bool EstCaseValide(int caseDestinationManoury)
        {   // C'est une case valide si un coup légal de la pièce sélectionnée (selon ReglesDames) y arrive
            return coupsLegauxPieceSelectionnee != null && coupsLegauxPieceSelectionnee.Any(coup => coup.Arrivee == caseDestinationManoury);
        }
        private void InitialiserEtatMouvement()
        {   // --- Remet à zéro les paramètes utilisés pour dessiner les mouvements possibles ---
            pieceSelectionnee = Piece.Vide;
            CaseSourceManoury = 0;
            CaseDestinationManoury = 0;
            IndexPictureBoxSource100 = -1;
            clickCaseSource = true; // Retourne à l'état de sélection de pièce
        }
        public void AfficheListesPieces()
        {   // --- Affiche les nombres de pions blancs et noirs, de dames blanches et noires de la position ---
            Position position = Damier.PositionCourante;
            LabelPionsBlancs.Text = T("{0} pions blancs", position.Compte(Piece.PionBlanc));
            LabelPionsNoirs.Text = T("{0} pions noirs", position.Compte(Piece.PionNoir));
            LabelDamesBlanches.Text = T("{0} Dames blanches", position.Compte(Piece.DameBlanche));
            LabelDamesNoires.Text = T("{0} Dames noires", position.Compte(Piece.DameNoire));
        }
        private void PoseNumeroCase(PictureBox pict, int numeroManoury)
        {   // --- Une case sombre porte son numéro Manoury (dessiné en haut à droite par PictureBox_Paint) ---
            pict.Tag = numeroManoury;               // Le numéro est stocké dans le Tag de la case
            pict.Paint += PictureBox_Paint;         // Ajouter le gestionnaire Paint à la PictureBox
            pict.Invalidate();                      // Forcer la mise à jour
        }
        private void PictureBox_Paint(object? sender, PaintEventArgs e)
        {   // --- Événement Paint des cases sombres : fond, image de la pièce et numéro Manoury ---
            if (sender is not PictureBox pict || pict.Tag is not int numeroManoury)
                return;     // Le Tag est posé par PoseNumeroCase pour chaque case sombre
            using (SolidBrush fond = new SolidBrush(pict.BackColor))
                e.Graphics.FillRectangle(fond, 0, 0, pict.Width, pict.Height);      // Effacer le fond
            if (pict.Image != null)
                e.Graphics.DrawImage(pict.Image, 0, 0, pict.Width, pict.Height);    // Redessiner l'image si elle existe
            DessineMarques(e.Graphics, numeroManoury, pict.Width, pict.Height);    // Dernier coup, pièces à prendre, numéros d'étapes (Marques.cs)
            DessineFleche(e.Graphics, pict);            // La flèche du meilleur coup de l'analyse
            // Le numéro est blanc cerné de noir : il se voit sur toutes les cases et sur les pièces blanches comme sur les noires
            using (System.Drawing.Drawing2D.GraphicsPath contour = new System.Drawing.Drawing2D.GraphicsPath())
            {
                contour.AddString(numeroManoury.ToString(), new FontFamily("Arial"), (int)FontStyle.Bold, e.Graphics.DpiY * 10 / 72, new PointF(pict.Width - 20, 3), StringFormat.GenericDefault);
                System.Drawing.Drawing2D.SmoothingMode ancien = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (Pen cerne = new Pen(Color.Black, 3f) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round })
                    e.Graphics.DrawPath(cerne, contour);
                e.Graphics.FillPath(Brushes.White, contour);
                e.Graphics.SmoothingMode = ancien;
            }
        }
    }
}
