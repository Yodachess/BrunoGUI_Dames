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
        // Clics sur le damier, coups humains, prises obligatoires, déplacements et dialogue avec le moteur pour jouer un coup.
        // --- Le curseur « pièce en main » : l'icône (HICON) que crée GetHicon appartient à l'appelant, qui doit la détruire (sinon un handle GDI fuit à chaque sélection) ---
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hicon);
        private Cursor? curseurPiece;
        private IntPtr iconeCurseurPiece = IntPtr.Zero;
        private void PoseCurseurPiece(Image imagePiece)
        {
            RendLeCurseur();
            using (Bitmap source = new Bitmap(imagePiece))
            using (Bitmap miniature = (Bitmap)source.GetThumbnailImage(150, 150, null, IntPtr.Zero))
            {
                iconeCurseurPiece = miniature.GetHicon();
                curseurPiece = new Cursor(iconeCurseurPiece);
            }
            Cursor = curseurPiece;
        }
        private void RendLeCurseur()
        {   // Curseur normal, et libération du curseur de la pièce s'il y en avait un
            Cursor = Cursors.Default;
            curseurPiece?.Dispose();
            curseurPiece = null;
            if (iconeCurseurPiece != IntPtr.Zero)
            {
                DestroyIcon(iconeCurseurPiece);
                iconeCurseurPiece = IntPtr.Zero;
            }
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion du click de la souris
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private async void CaseMouseDown(object? sender, MouseEventArgs e)
        {   // --- Gestion de la sélection/déplacement des pièces avec retour à l'origine si mouvement incorrect ---
            if (e.Button != MouseButtons.Left)
                return;     // Le clic droit ouvre le menu du damier ; il ne sélectionne ni ne joue
            try
            {   // Le joueur sélectionne la case source ou destination avec la souris
                if (sender is PictureBox CaseClick)     // Une PictureBox est cliquée
                {
                    int indexCase100 = CaseDamier.IndexOf(CaseClick);       // La place de la case à l'écran (CaseDamier suit les places, quel que soit le sens du damier)
                    if (indexCase100 < 0)
                    {
                        Journal.Erreur("Incohérence : CaseMouseDown appelé par une PictureBox qui n'est pas dans CaseDamier : " + CaseClick.Name);
                        return;
                    }
                    if (clickCaseSource)     // Permet de savoir si c'est la sélection de la pièce ou le déplacement
                    {   // --- Sélection d'une pièce ---
                        if (RefuseSiPartieParcourue())
                            return;
                        if (Damier.Orientation.CaseManoury(indexCase100) == 0)
                            return;         // Une case claire (jouable seulement en apparence : ActiveDamier les active toutes) : rien à sélectionner, et surtout pas une erreur dans le journal
                        CouleurToutesCasesManoury(CouleurCaseSombre);       // On réinitialise les couleurs des cases
                        IndexPictureBoxSource100 = indexCase100;
                        CaseSourceManoury = Damier.ObtenirCaseManoury(IndexPictureBoxSource100);     // Quelle case Manoury est cliquée ?
                        pieceSelectionnee = Damier.PositionCourante[CaseSourceManoury];     // Quelle pièce est cliquée ? (d'après la position)
                        CalculeCoupsLegauxPieceSelectionnee();      // Les coups de cette pièce selon ReglesDames, avant qu'elle soit enlevée de sa case
                        Image? imageCase = CaseDamier[indexCase100].Image;
                        if (imageCase != null)
                        {   // Si la case cliquée contient bien une pièce ou un pion, on va utiliser le thumbnail de la pièce comme curseur :-)
                            PoseCurseurPiece(imageCase);    // Quand on bouge la souris, on bouge le thumbnail de la pièce comme un curseur :-)
                            EffacePiece(indexCase100);       // On efface l'image de la case d'origine ; la pièce reste dans la position jusqu'à la fin du coup
                            CaseDamier[indexCase100].BackColor = CouleurTrajetSuivi;
                            List<Coup> coupsDeLaPiece = coupsLegauxPieceSelectionnee ?? new List<Coup>();
                            DessineMouvements(CaseSourceManoury, coupsDeLaPiece);   // On montre les mouvements possibles
                            // Crée une liste de chaînes de caractères représentant chaque rafle
                            List<Coup> raflesDeLaPiece = coupsDeLaPiece.Where(coup => coup.EstPrise).ToList();
                            var raflesText = raflesDeLaPiece.Select(rafle => string.Join(", ", rafle.Prises.Zip(rafle.Chemin, (prise, arrivee) => $"{prise} -> {arrivee}"))).ToList();
                            // Détermine le nombre d'éléments raflés par rafle (toutes les rafles ont la même taille)
                            int elementsParRafle = raflesDeLaPiece.FirstOrDefault()?.Prises.Count ?? 0;
                            // Combine toutes les chaînes en une seule avec un saut de ligne entre chaque liste
                            LabelPrisesPossibles.Text = raflesDeLaPiece.Count + " Rafles possibles de " + elementsParRafle + " pièces\n" + string.Join("\n", raflesText);
                            clickCaseSource = false;
                            pieceEnMain = true;
                        }
                        else
                        {
                            // La case cliquée ne porte aucune image : il n'y a rien à sélectionner
                        }
                    }
                    else
                    {   // --- Déplacement d'une pièce ---
                        EffaceMouvements();
                        RendLeCurseur();                // On revient au curseur "normal"
                        pieceEnMain = false;
                        try
                        {   // Le try-catch récupère l'exception qui se produit lorsque que l'on clique sur une case inactive
                            CaseDestinationManoury = Damier.Orientation.CaseManoury(indexCase100);     // 0 : case claire, non jouable (la pièce revient)
                            // Il faut que la case destination soit une case Manoury et qu'un coup légal de la pièce y arrive. Elle peut être la case de départ elle-même :
                            // une rafle peut revenir sur sa case (un tour complet) ; sans telle rafle, cliquer sa propre case n'est pas un coup et la pièce revient
                            if (Damier.EstCaseManouryValide(CaseDestinationManoury) && EstCaseValide(CaseDestinationManoury))
                            {
                                // --- ReglesDames décide si le coup est légal et quelle rafle est jouée ---
                                List<Coup> candidats = coupsLegauxPieceSelectionnee?.Where(c => c.Arrivee == CaseDestinationManoury).ToList() ?? new List<Coup>();    // Vide si le calcul a échoué : le coup est refusé
                                Coup? coupJoue = candidats.FirstOrDefault();
                                if (candidats.Count > 1)
                                {   // Plusieurs rafles de même départ et même arrivée qui ne prennent pas les mêmes pièces : le joueur choisit, en voyant les pièces prises de chaque option
                                    coupJoue = ChoisitRafle(candidats);
                                    if (coupJoue == null)
                                    {   // Choix annulé : la pièce revient sur sa case et le damier retrouve ses indications
                                        DessinePiece(IndexPictureBoxSource100, pieceSelectionnee);
                                        AnalyseCoupObligatoire(CouleurAuTrait);
                                        return;     // Le finally remet l'état de sélection à zéro
                                    }
                                }
                                if (coupJoue == null)
                                {   // Le coup n'est pas légal, on remet la pièce sur sa case d'origine
                                    DessinePiece(IndexPictureBoxSource100, pieceSelectionnee);
                                    CaseDamier[IndexPictureBoxSource100].BackColor = CouleurTrajetSuivi;
                                }
                                else if (!PenduleCoupJoue(CouleurAuTrait))
                                {   // Le coup est décidé : la pendule le décompte (sans pendule, toujours vrai). Faux : le drapeau était tombé, la partie est perdue au temps et le coup n'est pas joué
                                    DessinePiece(IndexPictureBoxSource100, pieceSelectionnee);
                                    return;     // Le finally remet l'état de sélection à zéro
                                }
                                else
                                {   // Le coup est joué sur le damier (une rafle est animée, un déplacement simple est immédiat), puis il entre dans l'historique
                                    string coupPdn = await JoueCoupSurLeDamier(coupJoue);
                                    EnregistrePositionApresCoupHumain(coupPdn);
                                }
                                LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s";
                                // Le joueur vient de déplacer sa pièce, on va donc passer la main au moteur 
                                if (OrdinateurActif)
                                {   // --- Lancement du moteur SCAN pour réfléchir à un coup sur la position "hub2" pendant "TempsReflexion" ---
                                    ChangerCouleurTrait();
                                    await LancementReflexionMoteur(Session.Hub2Dernier(), TempsReflexion);
                                    AnalyseCoupObligatoire(CouleurAuTrait);
                                }   // Le moteur a joué, il faut analyser le damier pour voir s'il existe une prise obligatoire
                                else
                                {   // Jeu entre humains, on vérifie s'il y a une prise obligatoire
                                    AnalyseCoupObligatoire(CouleurAuTrait);
                                }
                            }
                            else
                            {   // Si le coup n'est pas valide, on remet la pièce sur sa case d'origine !
                                DessinePiece(IndexPictureBoxSource100, pieceSelectionnee);
                                CaseDamier[IndexPictureBoxSource100].BackColor = CouleurCaseSombre;
                            }
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {   // Une case hors du damier a été trouvée dans un calcul (anomalie : un clic sur une case claire est déjà traité plus haut), on redessine la pièce sur sa case d'origine
                            Journal.Erreur("Clic sur la case de destination", ex);
                            DessinePiece(IndexPictureBoxSource100, pieceSelectionnee);
                            CaseDamier[IndexPictureBoxSource100].BackColor = CouleurCaseSombre;
                        }
                        finally
                        {
                            // Cursor = Cursors.Default;       // On revient au curseur "normal"
                            coupsLegauxPieceSelectionnee = null;
                            clickCaseSource = true;
                            InitialiserEtatMouvement();
                        }
                    }
                }
                else
                {
                    Journal.Erreur("Incohérence : CaseMouseDown appelé par un objet qui n'est pas une PictureBox");
                }
            }
            catch (Exception ex)
            {
                Journal.Erreur("Clic sur une case", ex);
            }
        }

        private async Task LancementReflexionMoteur(string positionHub2, int tempsReflexion)
        {   // --- Lancement du moteur SCAN pour réfléchir à un coup sur la position "positionHub2" pendant "tempsReflexion" ---
            if (!analyseEnCours && !PartieTerminee && NulleActuelle() != RaisonNulle.Aucune)
            {   // La partie est nulle : inutile d'interroger le moteur
                FinPartieParNulle(NulleActuelle());
                return;
            }
            if (!analyseEnCours && (PartieTerminee || ReglesDames.EstTerminee(Position.DepuisHub2(positionHub2))))
            {   // La partie est déjà finie, ou le camp du moteur n'a plus aucun coup (il a perdu) : inutile d'interroger le moteur
                FinPartieParBlocage(positionHub2[0] == 'W' ? Couleur.Blanc : Couleur.Noir);      // Sans effet si la partie est déjà finie (plus de pièces)
                return;
            }
            Couleur campDuMoteur = positionHub2[0] == 'W' ? Couleur.Blanc : Couleur.Noir;     // Le trait de la position envoyée (pendant la réflexion, CouleurAuTrait est celui de l'humain)
            if (!analyseEnCours && pendule != null)
                tempsReflexion = pendule.TempsConseille(campDuMoteur, tempsReflexion);     // Avec une pendule, jamais plus que le réglage, moins quand il reste peu de temps
            BloqueBoutons(true);
            jeuMoteurEnCours = true;
            ActiveCouleurDamier(CouleurAuTrait, false);
            string couleurEnJeu = positionHub2[0] == 'W' ? "Blancs" : "Noirs";      // On récupère la couleur en jeu
            LabelInformationJoueur.Text = "Trait aux " + couleurEnJeu + ", le moteur réfléchit ...";
            ReponseMoteur? reponse = await moteur.ReflechirAsync(positionHub2, tempsReflexion, analyseEnCours ? NiveauMoteur.ProfondeurMaximale : NiveauMoteur.Profondeur(NiveauChoisi));   // Au moteur de bosser .... :-)
            CouleurToutesCasesManoury(CouleurCaseSombre);
            if (reponse == null)
            {   // Le moteur n'a pas répondu (arrêté, trop long ou remplacé par une autre demande) : rien n'est joué, c'est noté dans le journal
                LabelInformationJoueur.Text = "Le moteur n'a pas répondu";
            }
            else if (reponse.Coup.Length == 0)
            {   // « done » sans coup : le moteur n'a aucun coup à jouer
                if (!analyseEnCours)
                {
                    LabelInformationJoueur.Text = "Le moteur n'a aucun coup à jouer";
                }
            }
            else if (!analyseEnCours && !PartieTerminee)    // On execute les coups si ce n'est pas une analyse (ni une partie finie pendant sa réflexion : drapeau tombé)
            {
                LabelCoupJoue.Text = "Coup joué : " + reponse.Coup + " --  Suggéré : " + reponse.Suggestion + (reponse.DuLivre ? " (livre)" : "");     // La mention en dernier : le cadre est étroit, c'est elle qui serait coupée
                if (EmetUnSon)
                {
                    JoueSonCoupJoue();       // Son pour dire que le coup est joué
                }
                Coup? coupMoteur = TrouveCoupDuMoteur(positionHub2, reponse.Coup);
                if (coupMoteur == null)
                {   // Le coup de Scan n'a pas été retrouvé dans les coups légaux (c'est noté dans le journal) : rien n'est joué
                    LabelInformationJoueur.Text = "Coup du moteur non reconnu : " + reponse.Coup;
                }
                else if (PenduleCoupJoue(campDuMoteur))
                {   // Le coup exact de Scan (départ, arrivée et pièces prises), retrouvé par ReglesDames sur la position envoyée au moteur ; la pendule l'a décompté (sinon son drapeau est tombé : il perd, rien n'est joué)
                    CaseDamier[Damier.ObtenirIndicePictureBox(coupMoteur.Depart)].BackColor = CouleurTrajetSuivi;
                    string coupPdnMoteur = await JoueCoupSurLeDamier(coupMoteur, glisse: true);      // Les pièces du moteur glissent : sans cela, son coup passe inaperçu
                    Session.AjouteCoup(coupPdnMoteur, PositionDuDamier());      // Le coup et la position qu'il donne entrent dans l'historique
                    LabelPrisesPossibles.Text = "";
                    LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s";
                }
            }
            jeuMoteurEnCours = false;
            if (!PartieTerminee)
                ActiveCouleurDamier(CouleurAuTrait, true);
            BloqueBoutons(false);
        }
        private void AnalyseCoupObligatoire(Couleur couleur)
        {   // --- Analyse pour toutes les pièces des coups obligatoires (prise la + longue) pour la couleur donnée, selon ReglesDames ---
            ActiveDamier(false);            // Désactiver les interactions pendant l'analyse
            int nombrePieces = Damier.NombreDePieces(couleur);
            if (nombrePieces > 0)
                VerifierGain();             // Vérifier si la partie n'est pas finie ?
            List<Coup> prises = new List<Coup>();
            bool aucunCoup = false;
            try
            {   // Le trait de la position est CouleurAuTrait, qui est « couleur » à tous les appels ; CoupsLegaux ne renvoie que les rafles les plus longues quand une prise existe
                List<Coup> coupsLegaux = ReglesDames.CoupsLegaux(Position.DepuisFen(RecupereFEN()));
                prises = coupsLegaux.Where(c => c.EstPrise).ToList();
                aucunCoup = coupsLegaux.Count == 0;
            }
            catch (Exception ex)
            {
                Journal.Erreur("Analyse des coups obligatoires", ex);
            }
            if (aucunCoup && nombrePieces > 0 && !PartieTerminee)
            {   // Le camp au trait a encore des pièces mais plus aucun coup : il a perdu (les cases restent désactivées)
                FinPartieParBlocage(couleur);
                return;
            }
            RaisonNulle nulle = NulleActuelle();
            if (nulle != RaisonNulle.Aucune && !PartieTerminee)
            {   // La partie est nulle (répétition, 25 coups de dames) : elle est terminée (les cases restent désactivées)
                FinPartieParNulle(nulle);
                return;
            }
            if (prises.Count == 0)              // S'il n'y a pas de rafles possibles, on réactive toutes les cases
            {
                ActiveDamier(true);     // Réactiver toutes les cases, y compris les cases vides (sans effet si la partie est finie)
                if (!PartieTerminee)    // Une partie finie (au temps, par exemple, pendant que le moteur réfléchissait) garde son damier inactif
                {
                    ActiveCouleurDamier(CouleurAuTrait, true);          // Réactiver les cases de la couleur au trait
                    ActiveCouleurDamier(CouleurPasAuTrait(), false);    // Désactiver les cases de la couleur pas au trait
                }
                return;                 // Sortir de la méthode, rien à faire de plus
            }
            CouleurToutesCasesManoury(CouleurCaseSombre);       // On réinitialise les couleurs des cases
            EffaceNumerosEtape();
            bool coupForce = prises.Count == 1;                 // Une seule prise possible : le coup est forcé, on montre aussi sa destination (et l'ordre de ses étapes)
            var casesActivables = new HashSet<int>();           // Ensemble des cases à activer (départ et arrivée finale)
            foreach (Coup rafle in prises)
            {
                casesActivables.Add(rafle.Depart);
                casesActivables.Add(rafle.Arrivee);
                CaseDamier[Damier.ObtenirIndicePictureBox(rafle.Depart)].BackColor = CouleurCaseDepart;
                if (!ArriveesDebutTour && !coupForce)
                    continue;           // Seules les pièces qui doivent prendre sont montrées ; les arrivées de la pièce choisie apparaissent à la sélection (DessineMouvements). Sauf si le coup est forcé
                foreach (int arrivee in rafle.Chemin)
                {   // Colorier chaque case d'arrivée avec le cadre gris
                    PictureBox caseArrivee = CaseDamier[Damier.ObtenirIndicePictureBox(arrivee)];
                    caseArrivee.BackColor = CouleurCaseArrivee;
                    if (caseArrivee.Image == null)
                    {   // Pas de cadre sur une case qui porte une pièce : une rafle qui revient sur sa case de départ passe par la case de la pièce qui prend, qui doit rester visible
                        caseArrivee.Image = CadreGrisSansPrise;
                    }
                }
            }
            if (coupForce)
                MarqueEtapes(prises[0]);                        // L'ordre des étapes de la rafle forcée
            foreach (int caseActivable in casesActivables)      // Parcourir toutes les cases activables
            {
                ActiverCaseManoury(caseActivable, true);        // Activer directement chaque case activable
                LabelInformationJoueur.Text = "Trait aux " + couleur + "s (prise obligatoire !)";
            }
            // Une chaîne par rafle (toutes ont le même nombre de pièces prises), un saut de ligne entre chaque
            List<string> raflesText = prises.Select(rafle => string.Join(", ", rafle.Prises.Zip(rafle.Chemin, (prise, arrivee) => $"{prise} -> {arrivee}"))).ToList();
            LabelPrisesPossibles.Text = raflesText.Count + " Rafle(s) possible(s) de " + prises[0].Prises.Count + " pièce(s) \n" + string.Join("\n", raflesText);
        }
        public void MiseenplaceFen(string fenAutiliser, bool commenceLaPartie)     // Préparation du logiciel avec la position FEN
        {   // --- La méthode met la position FEN sur le damier (PositionCourante et images) ; si commenceLaPartie, c'est la position de départ d'une nouvelle partie (Session) ---
            Damier.ViderDamier();
            CouleurToutesCasesManoury(CouleurCaseSombre);       // Les cases colorées par le coup précédent (départ, arrivée) ne sont plus celles de cette position : le parcours de la partie les laissait
            foreach (var pict in CaseDamier)
            {
                pict.Image = null;          // Efface l'image de chaque PictureBox
            }
            // Trouver la première occurrence de guillemet
            int debutIndex = fenAutiliser.IndexOf("\"") + 1;        // On commence après le premier guillemet
            int finIndex = fenAutiliser.IndexOf("\"", debutIndex);  // On trouve le prochain guillemet après le debutIndex
            if (debutIndex >= 0 && finIndex > debutIndex)
            {   // Extraire le texte entre les guillemets
                string contenu = fenAutiliser.Substring(debutIndex, finIndex - debutIndex);
                Position position = Position.DepuisFen(contenu);        // Lève FormatException si le FEN n'est pas valide
                CouleurAuTrait = position.Trait;                        // Le premier champ du FEN : la couleur qui joue le prochain coup
                if (commenceLaPartie)
                    Session.Commencer(position);                // La partie repart de zéro (historique, résultat, parcours) à cette position
                Damier.PositionCourante = position.Copie();      // La position du jeu : celle du FEN
                for (int numeroCase = 1; numeroCase <= 50; numeroCase++)
                {   // Dessiner chaque pièce : DessinePiece met l'image
                    Piece piece = position[numeroCase];
                    if (piece != Piece.Vide)
                        DessinePiece(Damier.ObtenirIndicePictureBox(numeroCase), piece);
                }
                AfficheListesPieces();
                EffaceNumerosEtape();
                if (commenceLaPartie)
                    MarqueDernierCoup(0, 0);                            // Une nouvelle partie : pas de dernier coup
                else
                    MarqueDernierCoupAffiche();                         // Le parcours : le dernier coup de la position affichée
            }
            else
            {
                Journal.Erreur($"Position non mise en place : aucun texte entre guillemets dans « {fenAutiliser} », le damier reste vide");
            }
        }
        public string RecupereFEN()
        {   // --- Génère et renvoie le FEN de la position actuelle (Damier.PositionCourante, avec le trait CouleurAuTrait) ---
            return "[FEN \"" + PositionDuDamier().VersFen() + "\"]";
        }
        // --- La position du damier avec le trait CouleurAuTrait : celle qu'on met dans l'historique après un coup ---
        private Position PositionDuDamier()
        {
            Position position = Damier.PositionCourante.Copie();
            position.Trait = CouleurAuTrait;
            return position;
        }
        private List<Coup>? coupsLegauxPieceSelectionnee;   // Coups légaux de la pièce sélectionnée selon ReglesDames ; null si le calcul a échoué
        private void CalculeCoupsLegauxPieceSelectionnee()
        {   // --- À la sélection d'une pièce : ses coups légaux selon ReglesDames, sur la position avant qu'elle quitte sa case ---
            try
            {
                Position position = Position.DepuisFen(RecupereFEN());
                coupsLegauxPieceSelectionnee = ReglesDames.CoupsLegaux(position).Where(c => c.Depart == CaseSourceManoury).ToList();
            }
            catch (Exception ex)
            {
                coupsLegauxPieceSelectionnee = null;
                Journal.Erreur("Calcul des coups légaux", ex);
            }
        }
        private Coup? TrouveCoupDuMoteur(string positionHub2, string coupScan)
        {   // --- Le coup légal que désigne la notation Hub2 de Scan (« 28x19x23 », « 32-28 ») dans la position qu'on lui a envoyée, ou null (journal) ---
            // La position vient de la chaîne Hub2 envoyée au moteur, pas de CouleurAuTrait : pendant que le moteur réfléchit, cette couleur est celle de l'humain
            try
            {
                Coup? coup = ReglesDames.TrouverCoup(Position.DepuisHub2(positionHub2), coupScan);
                if (coup == null) Journal.Attention($"Moteur : coup {coupScan} non retrouvé (illégal ou ambigu) dans {positionHub2} : rien n'est joué");
                return coup;
            }
            catch (Exception ex)
            {
                Journal.Erreur($"Coup du moteur {coupScan} dans {positionHub2}", ex);
                return null;
            }
        }
        // --- On ne joue que sur la dernière position de la partie : sur une position parcourue (Préc., Début…), un coup serait ajouté à la fin de l'historique sans en être la suite.
        // Vrai (avec un message) si la position affichée n'est pas la dernière : le coup ou la demande est refusé ---
        private bool RefuseSiPartieParcourue()
        {
            if (lecture != null)
                return true;        // Pendant la lecture d'une partie, les clics sur le damier sont ignorés (sans message)
            SessionPartie session = Session;
            if (session.EstSurLaDernierePosition)
                return false;
            ReglagesDeTest.Message($"Vous parcourez la partie (position {session.IndexAffiche} sur {session.Positions.Count - 1}).\n\nPour jouer, revenez à la dernière position avec le bouton « Fin ».",
                            "Partie parcourue", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        private void EnregistrePositionApresCoupHumain(string coupPdn)
        {   // --- Après un coup humain : change le trait et ajoute le coup (PDN) et la nouvelle position à l'historique ---
            ChangerCouleurTrait();
            Session.AjouteCoup(coupPdn, PositionDuDamier());
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Mouvements  divers
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // --- Joue un coup légal sur le damier : un déplacement simple ou une rafle, avec les animations de DureeEtape millisecondes par étape. Renvoie le coup en notation PDN
        // (l'abrégée « 28x19 », ou la complète si une autre rafle légale de mêmes départ et arrivée prend d'autres pièces), que l'appelant met dans l'historique (Session)
        // avec la position obtenue. « glisse » : les pièces glissent d'une case à l'autre (le coup du moteur, le rejeu d'une partie) ; sinon (le coup que le joueur vient de choisir)
        // le déplacement simple est immédiat et les étapes d'une rafle s'enchaînent sans glissement. Les images du damier sont mises à jour pendant le coup ;
        // la position (PositionCourante) l'est une seule fois, à la fin, par ReglesDames.Jouer ---
        public async Task<string> JoueCoupSurLeDamier(Coup coup, bool glisse = false)
        {
            Position avant = Damier.PositionCourante.Copie();
            avant.Trait = Position.CouleurDe(avant[coup.Depart]);     // Le camp de la pièce qui joue (CouleurAuTrait est celui de l'humain pendant que le moteur réfléchit)
            string notationPdn = ReglesDames.NotationPdn(ReglesDames.CoupsLegaux(avant), coup);
            string notation = coup.EstPrise ? await AnimerEtExecuterRafle(coup, notationPdn, glisse) : await ExecuteDeplacement(coup, notationPdn, glisse);
            MarqueDernierCoup(coup.Depart, coup.Arrivee);       // Le repère brun sur les deux cases du coup, jusqu'au coup suivant
            return notation;
        }
        // --- Un déplacement simple (coup sans prise) ; la promotion éventuelle vient de la position obtenue par ReglesDames.Jouer ---
        private async Task<string> ExecuteDeplacement(Coup coup, string notationPdn, bool glisse)
        {
            Piece pieceQuiJoue = Damier.PositionCourante[coup.Depart];
            Position apres = ReglesDames.Jouer(Damier.PositionCourante, coup);
            Couleur couleurPiece = Position.CouleurDe(pieceQuiJoue);
            int indexSourceBox = Damier.ObtenirIndicePictureBox(coup.Depart);
            int indexArriveeBox = Damier.ObtenirIndicePictureBox(coup.Arrivee);
            ActiveDamier(false); // Désactiver les interactions pendant le déplacement
            EffaceCasesDestination();   // Efface les cadres gris des cases de destination générées lors de l'AnalyseCoupoObligatoire
            CaseDamier[indexArriveeBox].BackColor = CouleurCaseArrivee;
            CaseDamier[indexSourceBox].BackColor = CouleurTrajetSuivi;      // La case de départ reste colorée pour suivre le trajet
            if (glisse)
                await GlisseUnePiece(indexSourceBox, indexArriveeBox, pieceQuiJoue, DureeEtape);
            else
                EffacePiece(indexSourceBox);
            DessinePiece(indexArriveeBox, apres[coup.Arrivee]);      // Le pion devenu dame est dessiné en dame
            if (!jeuMoteurEnCours)                      // On l'affiche si ce n'est pas le moteur qui joue
                LabelCoupJoue.Text = "Coup joué : " + notationPdn;
            AppliqueCoupALaPosition(apres);
            ActiveDamier(true);         // Réactiver les interactions après le déplacement
            ActiveCouleurDamier(couleurPiece, false);
            VerifierGain();
            return notationPdn;
        }
        // --- Met la position à jour avec le coup qui vient d'être joué sur le damier (une seule fois par coup), puis les décomptes affichés ---
        private void AppliqueCoupALaPosition(Position apres)
        {
            Damier.PositionCourante = apres;
            AfficheListesPieces();
        }
        // --- Une rafle complète : une étape de DureeEtape millisecondes par pièce prise (la pièce qui prend va de case d'arrivée en case d'arrivée, en glissant si « glisse »),
        // puis une dernière pause, et les pièces prises sont enlevées toutes ensemble à la fin de la rafle (le coup donne les étapes : une pièce prise et une case d'arrivée
        // par prise). La promotion éventuelle, en fin de rafle, vient de la position obtenue par ReglesDames.Jouer ---
        private async Task<string> AnimerEtExecuterRafle(Coup coup, string notationPdn, bool glisse)
        {
            Position apres = ReglesDames.Jouer(Damier.PositionCourante, coup);
            Piece pieceQuiPrend = Damier.PositionCourante[coup.Depart];
            Couleur couleurPiece = Position.CouleurDe(pieceQuiPrend);
            List<(int casePrise, int caseArrivee)> rafle = coup.Prises.Zip(coup.Chemin, (prise, arrivee) => (prise, arrivee)).ToList();
            ActiveDamier(false); // Désactiver les interactions pendant l'animation
            EffaceCasesDestination();   // Efface les cadres gris des cases de destination générées lors de l'AnalyseCoupoObligatoire
            List<int> boxesAEffacerFinRafle = new List<int>();
            int indexSourceBoxActuel = Damier.ObtenirIndicePictureBox(coup.Depart);
            CaseDamier[indexSourceBoxActuel].Image = ListeBitmapsPiece[pieceQuiPrend];     // La pièce qui prend reste affichée sur la case de départ (le joueur l'avait « en main »)
            CaseDamier[indexSourceBoxActuel].BackColor = CouleurCaseDepart; // Marquer la case de départ

            foreach (var (casePrise, caseArrivee) in rafle)
            {
                // Calculer les indices graphiques des cases de prise et d'arrivée
                int indexPriseBox = Damier.ObtenirIndicePictureBox(casePrise);
                int indexArriveeBox = Damier.ObtenirIndicePictureBox(caseArrivee);
                CaseDamier[indexPriseBox].BackColor = CouleurCasePrise;       // Marquer la case de prise en CouleurCasePrise
                CaseDamier[indexArriveeBox].BackColor = CouleurCasePivot;     // et la case d'arrivée en CouleurCasePivot
                CaseDamier[indexArriveeBox].Image = CadreGrisSansPrise;       // Afficher le cadre gris sans pièce
                if (glisse)
                {
                    await GlisseUnePiece(indexSourceBoxActuel, indexArriveeBox, pieceQuiPrend, DureeEtape);
                }
                else
                {
                    await Task.Delay(DureeEtape);                             // Le temps de voir l'étape
                    EffacePiece(indexSourceBoxActuel);                        // La pièce qui prend quitte sa case...
                    DessinePiece(indexArriveeBox, pieceQuiPrend);             // ... et se place sur sa case d'arrivée
                }
                boxesAEffacerFinRafle.Add(indexPriseBox);                     // On enregistre la pièce qu'il faudra prendre, mais on ne l'efface pas encore
                indexSourceBoxActuel = indexArriveeBox;                       // Mettre à jour l'indice de la source pour le prochain déplacement
            }
            int indexArriveeFinale = Damier.ObtenirIndicePictureBox(coup.Arrivee);
            Piece pieceFinale = apres[coup.Arrivee];
            if (pieceFinale != pieceQuiPrend)
                DessinePiece(indexArriveeFinale, pieceFinale);                // Promotion en dame à la fin de la rafle
            await Task.Delay(DureeEtape);                                     // Une dernière pause avant que les pièces prises disparaissent
            foreach (int index in boxesAEffacerFinRafle)
                EffacePiece(index);                                           // Toutes ensemble
            EffaceNumerosEtape();
            CaseDamier[indexArriveeFinale].BackColor = CouleurCaseArrivee;
            if (!jeuMoteurEnCours)                      // On l'affiche si ce n'est pas le moteur qui joue
                LabelCoupJoue.Text = "Coup joué : " + notationPdn;
            AppliqueCoupALaPosition(apres);
            ActiveDamier(true); // Réactiver les interactions après l'animation
            ActiveCouleurDamier(couleurPiece, false);
            VerifierGain();
            return notationPdn;
        }    }
}
