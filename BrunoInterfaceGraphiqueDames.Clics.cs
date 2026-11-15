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
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion du click de la souris
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private async void CaseMouseDown(object? sender, MouseEventArgs e)
        {   // --- Gestion de la sélection/déplacement des pièces avec retour à l'origine si mouvement incorrect ---
            try
            {   // Le joueur sélectionne la case source ou destination avec la souris
                if (sender is PictureBox CaseClick)     // Une PictureBox est cliquée
                {
                    int indexCase100 = CaseDamier.IndexOf(CaseClick);       // La place de la case à l'écran (CaseDamier suit les places, quel que soit le sens du damier)
                    if (indexCase100 < 0)
                    {
                        Journal.Ecrit("ERREUR : CaseMouseDown appelé par une PictureBox qui n'est pas dans CaseDamier : " + CaseClick.Name);
                        return;
                    }
                    if (clickCaseSource)     // Permet de savoir si c'est la sélection de la pièce ou le déplacement
                    {   // --- Sélection d'une pièce ---
                        if (RefuseSiPartieParcourue())
                            return;
                        CouleurToutesCasesManoury(CouleurCaseSombre);       // On réinitialise les couleurs des cases
                        IndexPictureBoxSource100 = indexCase100;
                        CaseSourceManoury = GestionDamier.ObtenirCaseManoury(IndexPictureBoxSource100);     // Quelle case Manoury est cliquée ?
                        pieceCouleurSource = GestionDamier.ContenuCaseIndice(IndexPictureBoxSource100);     // Quelle pièce est cliquée  ? (d'après la position)
                        CalculeCoupsLegauxPieceSelectionnee();      // Les coups de cette pièce selon ReglesDames, avant qu'elle soit enlevée de sa case
                        Image? imageCase = CaseDamier[indexCase100].Image;
                        if (imageCase != null)
                        {   // Si la case cliquée contient bien une pièce ou un pion, on va utiliser le thumbnail de la pièce comme curseur :-)
                            using (Bitmap Piece = new Bitmap(imageCase))
                            {   // Quand on bouge la souris, on bouge le thumbnail de la pièce comme un curseur :-)
                                Bitmap thumbnail = (Bitmap)Piece.GetThumbnailImage(150, 150, null, IntPtr.Zero);
                                Cursor = new Cursor(thumbnail.GetHicon());
                            }
                            for (int i = 0; i < ListeCasesCoupSelectionne.Count; i++)    // On enlève tous les mouvements affichés précédemment
                            {   // On remet la couleur de base sur les cases
                                CaseDamier[GestionDamier.ObtenirIndicePictureBox(ListeCasesCoupSelectionne[i])].BackColor = BrunoInterfaceGraphiqueDames.CouleurCaseSombre;
                            }
                            ListeCasesCoupSelectionne.Clear();
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
                        Cursor = Cursors.Default;       // On revient au curseur "normal"
                        pieceEnMain = false;
                        try
                        {   // Le try-catch récupère l'exception qui se produit lorsque que l'on clique sur une case inactive
                            CaseDestinationManoury = GestionDamier.Orientation.CaseManoury(indexCase100);     // 0 : case claire, non jouable (la pièce revient)
                            // Il faut que la case destination soit une case Manoury et qu'un coup légal de la pièce y arrive. Elle peut être la case de départ elle-même :
                            // une rafle peut revenir sur sa case (un tour complet) ; sans telle rafle, cliquer sa propre case n'est pas un coup et la pièce revient
                            if (GestionDamier.EstCaseManouryValide(CaseDestinationManoury) && EstCaseValide(CaseDestinationManoury))
                            {
                                // --- ReglesDames décide si le coup est légal et quelle rafle est jouée ---
                                List<Coup> candidats = coupsLegauxPieceSelectionnee?.Where(c => c.Arrivee == CaseDestinationManoury).ToList() ?? new List<Coup>();    // Vide si le calcul a échoué : le coup est refusé
                                Coup? coupJoue = candidats.FirstOrDefault();
                                if (candidats.Count > 1)
                                {   // Plusieurs rafles de même départ et même arrivée qui ne prennent pas les mêmes pièces : le joueur choisit, en voyant les pièces prises de chaque option
                                    coupJoue = ChoisitRafle(candidats);
                                    if (coupJoue == null)
                                    {   // Choix annulé : la pièce revient sur sa case et le damier retrouve ses indications
                                        DessinePiece(IndexPictureBoxSource100, pieceCouleurSource.TypePiece);
                                        AnalyseCoupObligatoire(CouleurAuTrait);
                                        return;     // Le finally remet l'état de sélection à zéro
                                    }
                                }
                                if (coupJoue == null)
                                {   // Le coup n'est pas légal, on remet la pièce sur sa case d'origine
                                    DessinePiece(IndexPictureBoxSource100, pieceCouleurSource.TypePiece);
                                    CaseDamier[IndexPictureBoxSource100].BackColor = CouleurTrajetSuivi;
                                }
                                else if (!PenduleCoupJoue(CouleurAuTrait))
                                {   // Le coup est décidé : la pendule le décompte (sans pendule, toujours vrai). Faux : le drapeau était tombé, la partie est perdue au temps et le coup n'est pas joué
                                    DessinePiece(IndexPictureBoxSource100, pieceCouleurSource.TypePiece);
                                    return;     // Le finally remet l'état de sélection à zéro
                                }
                                else if (coupJoue.EstPrise)
                                {   // Une rafle : une étape (pièce prise, case d'arrivée) par pièce prise
                                    var indexSourceBox = GestionDamier.ObtenirIndicePictureBox(CaseSourceManoury);   // Index PictureBox initial
                                    string coupPdnRafle = await AnimerEtExecuterRafle(indexSourceBox, coupJoue, pieceCouleurSource, ReglesDames.NotationPdn(coupsLegauxPieceSelectionnee ?? new List<Coup>(), coupJoue));
                                    EnregistrePositionApresCoupHumain(coupPdnRafle);
                                }
                                else
                                {   // Un déplacement simple
                                    string coupPdnSimple = ExecuteDeplacement(IndexPictureBoxSource100, coupJoue, pieceCouleurSource);
                                    EnregistrePositionApresCoupHumain(coupPdnSimple);
                                }
                                LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s";
                                // Le joueur vient de déplacer sa pièce, on va donc passer la main au moteur 
                                if (OrdinateurActif)
                                {   // --- Lancement du moteur SCAN pour réfléchir à un coup sur la position "hub2" pendant "TempsReflexion" ---
                                    ChangerCouleurTrait();
                                    await LancementReflexionMoteur(Partie.Courante.Hub2Dernier(), TempsReflexion);
                                    AnalyseCoupObligatoire(CouleurAuTrait);
                                }   // Le moteur a joué, il faut analyser le damier pour voir s'il existe une prise obligatoire
                                else
                                {   // Jeu entre humains, on vérifie s'il y a une prise obligatoire
                                    AnalyseCoupObligatoire(CouleurAuTrait);
                                }
                            }
                            else
                            {   // Si le coup n'est pas valide, on remet la pièce sur sa case d'origine !
                                DessinePiece(IndexPictureBoxSource100, pieceCouleurSource.TypePiece);
                                CaseDamier[IndexPictureBoxSource100].BackColor = CouleurCaseSombre;
                            }
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {   // Une case hors du damier a été trouvée dans un calcul (anomalie : un clic sur une case claire est déjà traité plus haut), on redessine la pièce sur sa case d'origine
                            Journal.Ecrit($"ERREUR au clic de destination : {ex}");
                            DessinePiece(IndexPictureBoxSource100, pieceCouleurSource.TypePiece);
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
                    Journal.Ecrit("ERREUR : CaseMouseDown appelé par un objet qui n'est pas une PictureBox");
                }
            }
            catch (Exception ex)
            {
                Journal.Ecrit($"ERREUR au clic sur une case : {ex}");
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
            ReponseMoteur? reponse = await MoteurDamesScan.ReflechirAsync(positionHub2, tempsReflexion);   // Au moteur de bosser .... :-)
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
                LabelCoupJoue.Text = "Coup joué : " + reponse.Coup + " --  Suggéré : " + reponse.Suggestion;
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
                    int indexSourceBox = GestionDamier.ObtenirIndicePictureBox(coupMoteur.Depart);
                    CaseDamier[indexSourceBox].BackColor = CouleurTrajetSuivi;
                    ContenuCase pieceDuMoteur = GestionDamier.ContenuCaseCaseManoury(coupMoteur.Depart);
                    string coupPdnMoteur;
                    if (coupMoteur.EstPrise)
                    {   // Une rafle : une étape (pièce prise, case d'arrivée) par pièce prise
                        string notationDuMoteur = ReglesDames.NotationPdn(ReglesDames.CoupsLegaux(Position.DepuisHub2(positionHub2)), coupMoteur);
                        coupPdnMoteur = await AnimerEtExecuterRafle(indexSourceBox, coupMoteur, pieceDuMoteur, notationDuMoteur);
                    }
                    else
                    {   // Un déplacement simple
                        coupPdnMoteur = ExecuteDeplacement(indexSourceBox, coupMoteur, pieceDuMoteur);
                    }
                    Partie.Courante.AjouteCoup(coupPdnMoteur, PositionDuDamier());      // Le coup et la position qu'il donne entrent dans l'historique
                    LabelPrisesPossibles.Text = "";
                    LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s";
                }
            }
            OrdinateurActif = true;
            jeuMoteurEnCours = false;
            if (!PartieTerminee)
                ActiveCouleurDamier(CouleurAuTrait, true);
            BloqueBoutons(false);
        }
        private void AnalyseCoupObligatoire(Couleur couleur)
        {   // --- Analyse pour toutes les pièces des coups obligatoires (prise la + longue) pour la couleur donnée, selon ReglesDames ---
            ActiveDamier(false);            // Désactiver les interactions pendant l'analyse
            int nombrePieces = GestionDamier.NombreDePieces(couleur);
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
                Journal.Ecrit($"ERREUR analyse des coups obligatoires : {ex.Message}");
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
            var casesActivables = new HashSet<int>();           // Ensemble des cases à activer (départ et arrivée finale)
            foreach (Coup rafle in prises)
            {
                casesActivables.Add(rafle.Depart);
                casesActivables.Add(rafle.Arrivee);
                CaseDamier[GestionDamier.ObtenirIndicePictureBox(rafle.Depart)].BackColor = CouleurCaseDepart;
                foreach (int arrivee in rafle.Chemin)
                {   // Colorier chaque case d'arrivée avec le cadre gris
                    PictureBox caseArrivee = CaseDamier[GestionDamier.ObtenirIndicePictureBox(arrivee)];
                    caseArrivee.BackColor = CouleurCaseArrivee;
                    if (caseArrivee.Image == null)
                    {   // Pas de cadre sur une case qui porte une pièce : une rafle qui revient sur sa case de départ passe par la case de la pièce qui prend, qui doit rester visible
                        caseArrivee.Image = CadreGrisSansPrise;
                        ListeCasesDestination.Add(arrivee);     // Ajouter la case d'arrivée à la liste des cases de destination (dont l'image sera effacée)
                    }
                }
            }
            foreach (int caseActivable in casesActivables)      // Parcourir toutes les cases activables
            {
                ActiverCaseManoury(caseActivable, true);        // Activer directement chaque case activable
                LabelInformationJoueur.Text = "Trait aux " + couleur + "s (prise obligatoire !)";
            }
            // Une chaîne par rafle (toutes ont le même nombre de pièces prises), un saut de ligne entre chaque
            List<string> raflesText = prises.Select(rafle => string.Join(", ", rafle.Prises.Zip(rafle.Chemin, (prise, arrivee) => $"{prise} -> {arrivee}"))).ToList();
            LabelPrisesPossibles.Text = raflesText.Count + " Rafle(s) possible(s) de " + prises[0].Prises.Count + " pièce(s) \n" + string.Join("\n", raflesText);
        }
        public static void MiseenplaceFen(string fenAutiliser, bool commenceLaPartie)     // Préparation du logiciel avec la position FEN
        {   // --- La méthode met la position FEN sur le damier (PositionCourante et images) ; si commenceLaPartie, c'est la position de départ d'une nouvelle partie (Partie.Courante) ---
            GestionDamier.ViderDamier();
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
                    Partie.Courante.Commencer(position);                // La partie repart de zéro (historique, résultat, parcours) à cette position
                GestionDamier.PositionCourante = position.Copie();      // La position du jeu : celle du FEN
                for (int numeroCase = 1; numeroCase <= 50; numeroCase++)
                {   // Dessiner chaque pièce : DessinePiece met l'image
                    Piece piece = position[numeroCase];
                    if (piece != Piece.Vide)
                        DessinePiece(GestionDamier.ObtenirIndicePictureBox(numeroCase), GestionDamier.TypeDe(piece));
                }
                AfficheListesPieces();
            }
            else
            {
                Journal.Ecrit($"ERREUR MiseenplaceFen : aucun texte entre guillemets dans « {fenAutiliser} », le damier reste vide");
            }
        }
        public static string RecupereFEN()
        {   // --- Génère et renvoie le FEN de la position actuelle (GestionDamier.PositionCourante, avec le trait CouleurAuTrait) ---
            return "[FEN \"" + PositionDuDamier().VersFen() + "\"]";
        }
        // --- La position du damier avec le trait CouleurAuTrait : celle qu'on met dans l'historique après un coup ---
        private static Position PositionDuDamier()
        {
            Position position = GestionDamier.PositionCourante.Copie();
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
                Journal.Ecrit($"ERREUR calcul des coups légaux : {ex.Message}");
            }
        }
        private static Coup? TrouveCoupDuMoteur(string positionHub2, string coupScan)
        {   // --- Le coup légal que désigne la notation Hub2 de Scan (« 28x19x23 », « 32-28 ») dans la position qu'on lui a envoyée, ou null (journal) ---
            // La position vient de la chaîne Hub2 envoyée au moteur, pas de CouleurAuTrait : pendant que le moteur réfléchit, cette couleur est celle de l'humain
            try
            {
                Coup? coup = ReglesDames.TrouverCoup(Position.DepuisHub2(positionHub2), coupScan);
                if (coup == null) Journal.Ecrit($"MOTEUR coup {coupScan} non retrouvé (illégal ou ambigu) dans {positionHub2} : rien n'est joué");
                return coup;
            }
            catch (Exception ex)
            {
                Journal.Ecrit($"ERREUR MOTEUR {coupScan} dans {positionHub2} : {ex.Message}");
                return null;
            }
        }
        // --- On ne joue que sur la dernière position de la partie : sur une position parcourue (Préc., Début…), un coup serait ajouté à la fin de l'historique sans en être la suite.
        // Vrai (avec un message) si la position affichée n'est pas la dernière : le coup ou la demande est refusé ---
        private bool RefuseSiPartieParcourue()
        {
            SessionPartie session = Partie.Courante;
            if (session.EstSurLaDernierePosition)
                return false;
            ReglagesDeTest.Message($"Vous parcourez la partie (position {session.IndexAffiche} sur {session.Positions.Count - 1}).\n\nPour jouer, revenez à la dernière position avec le bouton « Fin ».",
                            "Partie parcourue", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        private void EnregistrePositionApresCoupHumain(string coupPdn)
        {   // --- Après un coup humain : change le trait et ajoute le coup (PDN) et la nouvelle position à l'historique ---
            ChangerCouleurTrait();
            Partie.Courante.AjouteCoup(coupPdn, PositionDuDamier());
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Mouvements  divers
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // --- Execute un déplacement simple (coup sans prise) en vérifiant la promotion ---
        // Les images du damier sont mises à jour pendant le coup ; la position (PositionCourante) l'est une seule fois, à la fin, par ReglesDames.Jouer
        // Renvoie le coup en notation PDN, que l'appelant met dans l'historique (Partie.Courante) avec la position obtenue
        public string ExecuteDeplacement(int indexSourceBox, Coup coup, ContenuCase pieceSource)
        {
            int destinationManoury = coup.Arrivee;
            ActiveDamier(false); // Désactiver les interactions pendant le déplacement
            EffaceCasesDestination();   // Efface les cadres gris des cases de destination générées lors de l'AnalyseCoupoObligatoire
            var couleurPiece = pieceSource.CouleurPiece;
            ListeCasesCoupSelectionne.Add(GestionDamier.ObtenirCaseManoury(indexSourceBox));     // Ajoute la case source au suivi des coups
            // Promotion en dame si le pion arrive sur la dernière rangée (pour une dame, c'est sans effet)
            if (derniereRangeeBlanche.Contains(destinationManoury) && pieceSource.CouleurPiece == CouleurPiece.Noir)
            {
                pieceSource.TypePiece = TypePiece.DameNoire; // Promotion en Dame noire
            }
            else if (derniereRangeeNoire.Contains(destinationManoury) && pieceSource.CouleurPiece == CouleurPiece.Blanc)
            {
                pieceSource.TypePiece = TypePiece.DameBlanche; // Promotion en Dame blanche
            }
            DessinePiece(GestionDamier.ObtenirIndicePictureBox(destinationManoury), pieceSource.TypePiece);
            CaseDamier[GestionDamier.ObtenirIndicePictureBox(destinationManoury)].BackColor = CouleurCaseArrivee;
            EffacePiece(indexSourceBox);                  // Efface la pièce source
            CaseDamier[indexSourceBox].BackColor = CouleurTrajetSuivi;      // mais on la colorie pour suivre le trajet
            ListeCasesCoupSelectionne.Add(destinationManoury);  // Ajoute la destinationManoury au suivi des coups
            string coupEffectue = $"{GestionDamier.ObtenirCaseManoury(indexSourceBox)}-{destinationManoury}";
            if (!jeuMoteurEnCours)                      // On l'affiche si ce n'est pas le moteur qui joue
                LabelCoupJoue.Text = "Coup joué : " + coupEffectue;
            AppliqueCoupALaPosition(coup);
            ActiveDamier(true);         // Réactiver les interactions après le déplacement
            ActiveCouleurDamier(couleurPiece, false);
            VerifierGain();
            return coupEffectue;
        }
        // --- Met la position à jour avec le coup qui vient d'être joué sur le damier (une seule fois par coup, par ReglesDames.Jouer), puis les décomptes affichés ---
        private static void AppliqueCoupALaPosition(Coup coup)
        {
            GestionDamier.PositionCourante = ReglesDames.Jouer(GestionDamier.PositionCourante, coup);
            AfficheListesPieces();
        }
        // --- Execute une rafle complète avec une animation à la vitesse DureeAnimation, ne prend les pièces qu'à la fin de la rafle ---
        // (le coup donne les étapes : une pièce prise et une case d'arrivée par prise)
        // Renvoie le coup en notation PDN, que l'appelant met dans l'historique (Partie.Courante) avec la position obtenue
        public async Task<string> AnimerEtExecuterRafle(int indexSourceBox, Coup coup, ContenuCase pieceSource, string? notationPdn = null)
        {
            List<(int casePrise, int caseArrivee)> rafle = coup.Prises.Zip(coup.Chemin, (prise, arrivee) => (prise, arrivee)).ToList();
            ActiveDamier(false); // Désactiver les interactions pendant l'animation
            EffaceCasesDestination();   // Efface les cadres gris des cases de destination générées lors de l'AnalyseCoupoObligatoire
            List<int> boxesAEffacerFinRafle = new List<int>();
            // Créer une copie locale des propriétés de pieceSource
            ContenuCase pieceSourceLocal = pieceSource;
            var typePiece = pieceSource.TypePiece;
            var couleurPiece = pieceSource.CouleurPiece;
            int derniereCaseRafle = 0;
            int indexSourceBoxActuel = indexSourceBox;
            var imagePiece = ListeBitmapsPiece[typePiece];  // Récupérer l'image de la pièce source
            // On Assure que la pièce source reste affichée sur la case de départ
            CaseDamier[indexSourceBoxActuel].Image = imagePiece;  // Afficher la pièce source
            CaseDamier[indexSourceBoxActuel].BackColor = CouleurCaseDepart; // Marquer la case de départ

            foreach (var (casePrise, caseArrivee) in rafle)
            {
                // Calculer les indices graphiques des cases de prise et d'arrivée
                int indexPriseBox = GestionDamier.ObtenirIndicePictureBox(casePrise);
                int indexArriveeBox = GestionDamier.ObtenirIndicePictureBox(caseArrivee);
                CaseDamier[indexPriseBox].BackColor = CouleurCasePrise;       // Marquer la case de prise en CouleurCasePrise
                CaseDamier[indexArriveeBox].BackColor = CouleurCasePivot;     // et la case d'arricée en CouleurCasePivot
                CaseDamier[indexArriveeBox].Image = CadreGrisSansPrise;       // Afficher le cadre gris sans pièce
                CaseDamier[indexSourceBoxActuel].Image = imagePiece;  // Afficher la pièce source

                // Attendre un peu pour l'effet visuel de la prise
                await Task.Delay(DelaiAnimation);
 
                boxesAEffacerFinRafle.Add(indexPriseBox);                   // On enregistre la pièce qu'il faudra prendre, mais on ne l'efface pas encore
                EffacePiece(indexSourceBoxActuel);                    // On efface la pièce qui prend sur la case de départ
                DessinePiece(indexArriveeBox, typePiece);     // ON place la pièce qui prend sur sa case d'arrivée
                // Attendre un peu avant de continuer avec la prochaine prise
                await Task.Delay(DelaiAnimation);

                indexSourceBoxActuel = indexArriveeBox;                     // Mettre à jour l'indice de la source pour le prochain déplacement
                derniereCaseRafle = caseArrivee;                            // Enregistrer la dernière case de la rafle pour vérifier la promotion
            }
            // Vérification si promotion en Dame à la fin de la rafle
            if (derniereRangeeBlanche.Contains(derniereCaseRafle) && couleurPiece == CouleurPiece.Noir)
            {   // promotion du pion noir
                typePiece = TypePiece.DameNoire; // Promotion en Dame noire
                DessinePiece(GestionDamier.ObtenirIndicePictureBox(derniereCaseRafle), typePiece);
                CaseDamier[GestionDamier.ObtenirIndicePictureBox(derniereCaseRafle)].BackColor = CouleurCaseArrivee;
            }
            else if (derniereRangeeNoire.Contains(derniereCaseRafle) && couleurPiece == CouleurPiece.Blanc)
            {   // promotion du pion blanc
                typePiece = TypePiece.DameBlanche; // Promotion en Dame blanche
                DessinePiece(GestionDamier.ObtenirIndicePictureBox(derniereCaseRafle), typePiece);
                CaseDamier[GestionDamier.ObtenirIndicePictureBox(derniereCaseRafle)].BackColor = CouleurCaseArrivee;
            }
            // Parcourir la liste des pièces prises et effacer les pièces
            foreach (var index in boxesAEffacerFinRafle)
            {
                EffacePiece(index);
                await Task.Delay(DelaiAnimation);
            }
            boxesAEffacerFinRafle.Clear();
            // Notation PDN du coup : abrégée « 28x19 », ou complète si une autre rafle de même départ et même arrivée existe (notationPdn)
            string coupEffectue = notationPdn ?? GestionDamier.ObtenirCaseManoury(indexSourceBox) + "x"  + derniereCaseRafle;
            CaseDamier[GestionDamier.ObtenirIndicePictureBox(derniereCaseRafle)].BackColor = CouleurCaseArrivee;
            if (!jeuMoteurEnCours)                      // On l'affiche si ce n'est pas le moteur qui joue
                LabelCoupJoue.Text = "Coup joué : " + coupEffectue;
            AppliqueCoupALaPosition(coup);
            ActiveDamier(true); // Réactiver les interactions après l'animation
            ActiveCouleurDamier(couleurPiece, false);
            VerifierGain();
            return coupEffectue;
        }
    }
}
