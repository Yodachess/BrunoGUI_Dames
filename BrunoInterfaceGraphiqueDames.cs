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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BrunoGUI_Dames.Partie;
using static BrunoGUI_Dames.FichierPartiePdn;
using MethodInvoker = System.Windows.Forms.MethodInvoker;

namespace BrunoGUI_Dames
{
    public partial class BrunoInterfaceGraphiqueDames : Form
    {
        public static Dictionary<int, PictureBox> ManouryVersPictureBoxIndicee = new Dictionary<int, PictureBox>();
        // Les listes
        public static readonly List<PictureBox> CaseDamier = new List<PictureBox>();    // les 100 cases du jeu
        private readonly List<int> IndiceVisuCoteNoir = new List<int>();                // liste des indices si les noirs sont en bas de l'écran
        private readonly List<Color> CouleurCaseOrigines = new List<Color>();           // Couleurs d'origine des cases
        public static readonly List<int> ListeCasesCoupSelectionne = new List<int>();
        public static readonly List<int> ListeCasesMouvementPrecedent = new List<int>();
        public static readonly List<int> ListeCasesDestination = new List<int>();
        public static List<PartieDamesPdn> ListePartiesPdn = new List<PartieDamesPdn>();
        // liste des Bitmaps pour les pièces
        public static readonly Dictionary<TypePiece, Bitmap> ListeBitmapsPiece = new Dictionary<TypePiece, Bitmap>();
        // les Bitmaps
        private readonly Bitmap PionBlanc = Properties.Resources.DamesPionBlanc_transparent;
        private readonly Bitmap PionNoir = Properties.Resources.DamesPionNoir_transparent;
        private readonly Bitmap DameBlanche = Properties.Resources.DameBlanche;
        private readonly Bitmap DameNoire = Properties.Resources.DameNoire;
        private static readonly Bitmap CadreGrisSansPrise = Properties.Resources.SansPrise;
        private static readonly Bitmap CadreVert = Properties.Resources.CadreVertSansPriseTransparent;
        private static readonly Bitmap CadreRouge = Properties.Resources.CadreRougeSansPriseTransparent;
        private static readonly Bitmap CadreMarron = Properties.Resources.CadreMarronSansPriseTransparent;
        // les variables
        public static Color CouleurCasesombre = Color.Peru;         // Cases sombres damier
        public static Color CouleurCaseclaire = Color.BurlyWood;    // Cases claires damier
        public static Color CouleurCaseDepart = Color.SpringGreen;
        public static Color CouleurTrajetSuivi = Color.YellowGreen;
        public static Color CouleurCasePivot = Color.DarkKhaki;
        public static Color CouleurCasePrise = Color.DarkOliveGreen;                        
        public static Color CouleurCaseArrivee = Color.OliveDrab;
        public static string CouleurAuTrait = "";
        public static string NomJoueurBlanc = "Bruno" , NomJoueurNoir = "SCAN 3.1";
        public static FichierPartiePdn.PartieDamesPdn PartieCourante = new FichierPartiePdn.PartieDamesPdn();
        public static int IndexPictureBoxSource100, CaseSourceManoury, CaseDestinationManoury, DureeAnimation, TempsReflexion;
        public static bool ExisteFENDebut, VisuCoteNoir;          // True quand les Noirs sont en bas de l'écran
        public static bool AfficheNumeroBox, AfficheCaseLogique, PartieEnCours, PartieTerminee, FinPartie, EmetUnSon;
        private static bool clickCaseSource, montreDonneesDames, jeuMoteurEnCours, analyseEnCours;
        private ContenuCase pieceCouleurSource = new ContenuCase();     // Pièce sélectionnée au premier clic (case vide = aucune)
        private string[] donneesMoteurScan = Array.Empty<string>();        // Données en provenance du Moteur SCAN
        private string cheminMoteur = "";
        private static bool moteurDisponible = true;     // Faux si le moteur n'a pas pu démarrer ou s'est arrêté
        private string nomMoteur = "", auteurMoteur = "", versionMoteur = "";
        private int indexFenCoupActuel = 0; // Indice du coup affiché
        // Définir les indices des dernières rangées en notation Manoury
        private static readonly int[] derniereRangeeBlanche = { 46, 47, 48, 49, 50 };
        private static readonly int[] derniereRangeeNoire = { 1, 2, 3, 4, 5 };
        // -- les classes --
        private readonly DonneesBrutesDames donneesBrutesDames = new DonneesBrutesDames();
        private AffichePdn? affichePdn = new AffichePdn();   // affichePdn est à la fois déclarée et instanciée. Prêt à être utilisé dès le début
        private FichierPartiePdn? fichierPartiePdn = new FichierPartiePdn();     // idem pour fichierPartiePdn
        // GestionDamier, ContenuCase, TypePiece et CouleurPiece dans GestionDamier.cs ; Partie (listes de coups) dans Partie.cs

        public BrunoInterfaceGraphiqueDames()
        {
            InitializeComponent();
        }
        private static BrunoInterfaceGraphiqueDames? fenetrePrincipale;     // La fenêtre affichée, pour mettre à jour les décomptes de pièces depuis les méthodes statiques
        private void BrunoInterfaceGraphiqueDames_Load(object sender, EventArgs e)
        {
            fenetrePrincipale = this;
            // les évènements dans les classes
            MoteurDamesScan.AfficheScan += AfficheScan;
            MoteurDamesScan.AfficheDonneesBrutes += AfficheDonneesBrutes;
            MoteurDamesScan.MoteurArrete += MoteurArrete;
            // Liste des pièces du jeu
            ListeBitmapsPiece.Add(TypePiece.PionBlanc, PionBlanc);       // pion blanc
            ListeBitmapsPiece.Add(TypePiece.PionNoir, PionNoir);         // pion noir
            ListeBitmapsPiece.Add(TypePiece.DameBlanche, DameBlanche);   // Dame blanche
            ListeBitmapsPiece.Add(TypePiece.DameNoire, DameNoire);       // Dame noire
            ListeBitmapsPiece.Add(TypePiece.CadreGris, CadreGrisSansPrise);
            ListeBitmapsPiece.Add(TypePiece.CadreVert, CadreVert);
            ListeBitmapsPiece.Add(TypePiece.CadreRouge, CadreRouge);
            ListeBitmapsPiece.Add(TypePiece.CadreMarron, CadreMarron);
            // les variables
            CouleurAuTrait = "Blanc";
            VisuCoteNoir = false;                       // On commence avec la vue côté Blanc
            PartieEnCours = true;
            AfficheNumeroBox = ExisteFENDebut = analyseEnCours = false;
            boutonMasqueAffiche.Enabled = false;
            AfficheCaseLogique = clickCaseSource = true;
            DureeAnimation = 900;
            LabelTempsReflexion.Text = trackBarTempsReflexion.Value.ToString() + " sec.";            // Affichage 
            TempsReflexion = trackBarTempsReflexion.Value;              // et initialisation du temps de réflexion

            GestionDamier.InitialiserCorrespondance(this.Controls);     // Création de la correspondance PictureBox <-> Manoury
            CreeDamier();                       // Crée les 100 PictureBox correspondant aux cases SANS afficher de pièce
            GestionDamier.InitialiserDamier();  // intialise DamierContenu (10*10) avec les pièces SANS les afficher
            ActiveDamier(true);
            ListeCoupsFen.Clear();
            ListeCoupsPdn.Clear();
            ListeCoupsHub2.Clear();
            BoxNomJoueurBlanc.Text = NomJoueurBlanc;
            BoxNomJoueurNoir.Text = NomJoueurNoir;
            MiseenplaceFen("[FEN \"W:W31-50:B1-20\"]", true);          // Position initiale
            AfficheListesPieces();
            if (CouleurAuTrait == "Blanc")
            {
                ActiveCouleurDamier("Noir", false);
                LabelInformationJoueur.Text = "Trait aux Blancs";
            }
            else
            {
                ActiveCouleurDamier("Blanc", false);
                LabelInformationJoueur.Text = "Trait aux Noirs";
            }
            cheminMoteur = Path.Combine(Application.StartupPath, "Dames_Scan31_FabienLetouzey", "scan.exe");     // À côté de l'exe, jamais relatif au dossier courant
            try
            {   // Démarrage du moteur
                MoteurDamesScan.Demarre(cheminMoteur);
            }
            catch (Exception ex) when (ex is IOException || ex is System.ComponentModel.Win32Exception || ex is InvalidOperationException)
            {   // Sans moteur on peut jouer entre humains : on désactive ce qui en dépend
                Journal.Ecrit("Moteur : démarrage impossible : " + ex.Message);
                DesactiveMoteur("Le moteur Scan n'a pas pu démarrer : " + ex.Message);
            }
            AnalyseCoupObligatoire(CouleurAuTrait);
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion du click de la souris
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private async void CaseMouseDown(object? sender, MouseEventArgs e)
        {   // --- Gestion de la sélection/déplacement des pièces avec retour à l'origine si mouvement incorrect ---
            try
            {   // Le joueur sélectionne la case source ou destination avec la souris
                if (sender is PictureBox CaseClick)     // Une PictureBox est cliquée
                {
                    int indexCase100 = Convert.ToInt32(CaseClick.Name.Substring(10)); // Utilise le numéro de la PictureBox comme index
                    if (VisuCoteNoir)
                    {   // Si on regarde côté noir, il faut inverser l'index par rapport a la vue côté blanc
                        indexCase100 = IndiceVisuCoteNoir[indexCase100];    
                    }
                    if (clickCaseSource)     // Permet de savoir si c'est la sélection de la pièce ou le déplacement
                    {   // --- Sélection d'une pièce ---
                        CouleurToutesCasesManoury(CouleurCasesombre);       // On réinitialise les couleurs des cases
                        IndexPictureBoxSource100 = indexCase100;
                        CaseSourceManoury = GestionDamier.ObtenirCaseManoury(IndexPictureBoxSource100);     // Quelle case Manoury est cliquée ?
                        (int ligne, int colonne) = GestionDamier.IndiceVersCoordonnees(IndexPictureBoxSource100);
                        pieceCouleurSource = GestionDamier.DamierContenu[ligne, colonne];                   // Quelle pièce est cliquée  ?
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
                                CaseDamier[GestionDamier.ManouryVersIndexPictureBox[ListeCasesCoupSelectionne[i] - 1]].BackColor = BrunoInterfaceGraphiqueDames.CouleurCasesombre;
                            }
                            ListeCasesCoupSelectionne.Clear();
                            EffacePiece(indexCase100, false);       // On efface la case d'origine sans le faire dans DamierContenu ...
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
                        }
                        else
                        {
                            Console.WriteLine($"Passage sur une PictureBox = null : {CaseDamier[indexCase100].Image}");
                        }
                    }
                    else
                    {   // --- Déplacement d'une pièce ---
                        EffaceMouvements();
                        Cursor = Cursors.Default;       // On revient au curseur "normal"
                        try
                        {   // Le try-catch récupère l'exception qui se produit lorsque que l'on clique sur une case inactive
                            CaseDestinationManoury = GestionDamier.ObtenirCaseManoury(indexCase100);
                            // Il faut que la case destination soit une case Manoury, soit valide et != de la source
                            if (GestionDamier.EstCaseManouryValide(CaseDestinationManoury) && EstCaseValide(CaseDestinationManoury) && CaseDestinationManoury != CaseSourceManoury)
                            {
                                // --- ReglesDames décide si le coup est légal et quelle rafle est jouée ---
                                List<Coup> candidats = coupsLegauxPieceSelectionnee?.Where(c => c.Arrivee == CaseDestinationManoury).ToList() ?? new List<Coup>();    // Vide si le calcul a échoué : le coup est refusé
                                if (candidats.Count > 1)    // Plusieurs rafles de même départ et même arrivée qui ne prennent pas les mêmes pièces : on joue la première
                                    Journal.Ecrit($"AMBIGU {RecupereFEN()} : {string.Join(" ou ", candidats.Select(c => c.Hub2()))}, joué : {candidats[0].Hub2()}");
                                Coup? coupJoue = candidats.FirstOrDefault();
                                if (coupJoue == null)
                                {   // Le coup n'est pas légal, on remet la pièce sur sa case d'origine
                                    DessinePiece(IndexPictureBoxSource100, pieceCouleurSource.TypePiece, pieceCouleurSource.CouleurPiece);
                                    CaseDamier[IndexPictureBoxSource100].BackColor = CouleurTrajetSuivi;
                                }
                                else if (coupJoue.EstPrise)
                                {   // Une rafle : une étape (pièce prise, case d'arrivée) par pièce prise
                                    List<(int casePrise, int caseArrivee)> rafleAJouer = coupJoue.Prises.Zip(coupJoue.Chemin, (prise, arrivee) => (prise, arrivee)).ToList();
                                    var indexSourceBox = GestionDamier.ManouryVersIndexPictureBox[CaseSourceManoury - 1];   // Index PictureBox initial
                                    (int ligne, int colonne) = GestionDamier.IndiceVersCoordonnees(indexSourceBox);
                                    await AnimerEtExecuterRafle(indexSourceBox, rafleAJouer, GestionDamier.DamierContenu[ligne, colonne]);
                                    EnregistrePositionApresCoupHumain();
                                }
                                else
                                {   // Un déplacement simple
                                    ExecuteDeplacement(indexSourceBox: IndexPictureBoxSource100, destinationManoury: CaseDestinationManoury, pieceSource: pieceCouleurSource, piecePrise: false, 0);
                                    EnregistrePositionApresCoupHumain();
                                }
                                LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s";
                                // Le joueur vient de déplacer sa pièce, on va donc passer la main au moteur 
                                if (PartieEnCours)
                                {   // --- Lancement du moteur SCAN pour réfléchir à un coup sur la position "hub2" pendant "TempsReflexion" ---
                                    ChangerCouleurTrait();
                                    await LancementReflexionMoteur(ListeCoupsHub2[ListeCoupsHub2.Count - 1], TempsReflexion);
                                    AnalyseCoupObligatoire(CouleurAuTrait);
                                }   // Le moteur a joué, il faut analyser le damier pour voir s'il existe une prise obligatoire
                                else
                                {   // Jeu entre humains, on vérifie s'il y a une prise obligatoire
                                    AnalyseCoupObligatoire(CouleurAuTrait);
                                }
                            }
                            else
                            {   // Si le coup n'est pas valide, on remet la pièce sur sa case d'origine !
                                DessinePiece(IndexPictureBoxSource100, pieceCouleurSource.TypePiece, pieceCouleurSource.CouleurPiece);
                                CaseDamier[IndexPictureBoxSource100].BackColor = CouleurCasesombre;
                            }
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {   // Une case non-Manoury a été cliquée comme destination, on redessine la pièce sur sa case d'origine
                            Console.WriteLine($"Erreur : {ex.Message}");
                            DessinePiece(IndexPictureBoxSource100, pieceCouleurSource.TypePiece, pieceCouleurSource.CouleurPiece);
                            CaseDamier[IndexPictureBoxSource100].BackColor = CouleurCasesombre;
                        }
                        finally
                        {
                            // Cursor = Cursors.Default;       // On revient au curseur "normal"
                            coupsLegauxPieceSelectionnee = null;
                            clickCaseSource = true;
                            InitialiserEtatMouvement();
                            // Console.WriteLine($"Couleur au trait = {CouleurAuTrait}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Erreur : sender n'est pas une PictureBox.");
                }
                // GestionDamier.AfficherDamierContenu();      // DEBUG
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur dans CaseMoveDown : " + ex.Message);
                Console.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }

        private async Task LancementReflexionMoteur(string positionHub2, int tempsReflexion)
        {   // --- Laancement du moteur SCAN pour réfléchir à un coup sur la position "positionHub2" pendant "tempsReflexion" ---
            BloqueBoutons(true);
            jeuMoteurEnCours = true;
            ActiveCouleurDamier(CouleurAuTrait, false);
            string couleurEnJeu = positionHub2[0] == 'W' ? "Blancs" : "Noirs";      // On récupère la couleur en jeu
            LabelInformationJoueur.Text = "Trait aux " + couleurEnJeu + ", le moteur réfléchit ...";
            ReponseMoteur? reponse = await MoteurDamesScan.ReflechirAsync(positionHub2, tempsReflexion);   // Au moteur de bosser .... :-)
            CouleurToutesCasesManoury(CouleurCasesombre);
            if (reponse == null)
            {   // Le moteur n'a pas répondu (arrêté, trop long ou remplacé par une autre demande) : rien n'est joué, c'est noté dans le journal
                LabelInformationJoueur.Text = "Le moteur n'a pas répondu";
            }
            else if (reponse.Coup.Length == 0)
            {   // « done » sans coup : le moteur n'a aucun coup à jouer
                if (!analyseEnCours)
                {
                    FinPartie = true;
                    LabelInformationJoueur.Text = "Le moteur n'a aucun coup à jouer";
                }
            }
            else if (!analyseEnCours)    // On execute les coups si ce n'est pas une analyse
            {
                LabelCoupJoue.Text = "Coup joué : " + reponse.Coup + " --  Suggéré : " + reponse.Suggestion;
                if (EmetUnSon)
                {
                    SoundPlayer player = new SoundPlayer(@"C:\Windows\Media\Windows Notify.wav");       // Son pour dire que le coup est joué
                    player.Play();
                }
                Coup? coupMoteur = TrouveCoupDuMoteur(positionHub2, reponse.Coup);
                if (coupMoteur == null)
                {   // Le coup de Scan n'a pas été retrouvé dans les coups légaux (c'est noté dans le journal) : rien n'est joué
                    LabelInformationJoueur.Text = "Coup du moteur non reconnu : " + reponse.Coup;
                }
                else
                {   // Le coup exact de Scan (départ, arrivée et pièces prises), retrouvé par ReglesDames sur la position envoyée au moteur
                    int indexSourceBox = GestionDamier.ObtenirIndicePictureBox(coupMoteur.Depart);
                    CaseDamier[indexSourceBox].BackColor = CouleurTrajetSuivi;
                    ContenuCase pieceDuMoteur = GestionDamier.ContenuCaseCaseManoury(coupMoteur.Depart);
                    if (coupMoteur.EstPrise)
                    {   // Une rafle : une étape (pièce prise, case d'arrivée) par pièce prise
                        List<(int casePrise, int caseArrivee)> rafleDuMoteur = coupMoteur.Prises.Zip(coupMoteur.Chemin, (prise, arrivee) => (prise, arrivee)).ToList();
                        await AnimerEtExecuterRafle(indexSourceBox, rafleDuMoteur, pieceDuMoteur);
                    }
                    else
                    {   // Un déplacement simple
                        ExecuteDeplacement(indexSourceBox, coupMoteur.Arrivee, pieceDuMoteur, false, 0);
                    }
                    string fenPosition = RecupereFEN();                      // On met à jour les listes de coups
                    Partie.ListeCoupsFen.Add(fenPosition);                          // pour les FEN et Hub2, pour les Pdn
                    Partie.ListeCoupsHub2.Add(Partie.Hub2DepuisFen(fenPosition));   // c'est fait dans AnimerEtExecuterRafle
                    indexFenCoupActuel = ListeCoupsFen.Count - 1;   // Toujours pointer sur le dernier coup
                    LabelPrisesPossibles.Text = "";
                    LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s";
                }
            }
            PartieEnCours = true;
            jeuMoteurEnCours = false;
            ActiveCouleurDamier(CouleurAuTrait, true);
            BloqueBoutons(false);
        }
        private void AnalyseCoupObligatoire(string couleur)
        {   // --- Analyse pour toutes les pièces des coups obligatoires (prise la + longue) pour la couleur donnée, selon ReglesDames ---
            ActiveDamier(false);            // Désactiver les interactions pendant l'analyse
            int nombrePieces = GestionDamier.NombreDePieces(couleur == "Blanc" ? Couleur.Blanc : Couleur.Noir);
            if (nombrePieces > 0)
                VerifierGain();             // Vérifier si la partie n'est pas finie ?
            List<Coup> prises = new List<Coup>();
            try
            {   // Le trait de la position est CouleurAuTrait, qui est « couleur » à tous les appels ; CoupsLegaux ne renvoie que les rafles les plus longues quand une prise existe
                prises = ReglesDames.CoupsLegaux(Position.DepuisFen(RecupereFEN())).Where(c => c.EstPrise).ToList();
            }
            catch (Exception ex)
            {
                Journal.Ecrit($"ERREUR analyse des coups obligatoires : {ex.Message}");
            }
            if (prises.Count == 0)              // S'il n'y a pas de rafles possibles, on réactive toutes les cases
            {
                ActiveDamier(true);     // Réactiver toutes les cases, y compris les cases vides
                ActiveCouleurDamier(CouleurAuTrait, true);          // Réactiver les cases de la couleur au trait
                ActiveCouleurDamier(CouleurPasAuTrait(), false);    // Désactiver les cases de la couleur pas au trait
                return;                 // Sortir de la méthode, rien à faire de plus
            }
            CouleurToutesCasesManoury(CouleurCasesombre);       // On réinitialise les couleurs des cases
            var casesActivables = new HashSet<int>();           // Ensemble des cases à activer (départ et arrivée finale)
            foreach (Coup rafle in prises)
            {
                casesActivables.Add(rafle.Depart);
                casesActivables.Add(rafle.Arrivee);
                CaseDamier[GestionDamier.ManouryVersIndexPictureBox[rafle.Depart - 1]].BackColor = CouleurCaseDepart;
                foreach (int arrivee in rafle.Chemin)
                {   // Colorier chaque case d'arrivée avec le cadre gris
                    CaseDamier[GestionDamier.ManouryVersIndexPictureBox[arrivee - 1]].BackColor = CouleurCaseArrivee;
                    CaseDamier[GestionDamier.ManouryVersIndexPictureBox[arrivee - 1]].Image = CadreGrisSansPrise;
                    ListeCasesDestination.Add(arrivee);     // Ajouter la case d'arrivée à la liste des cases de destination
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
        public static void MiseenplaceFen(string fenAutiliser, bool ajouteListeCoups)     // Préparation du logiciel avec la position FEN
        {   // --- La méthode met la position FEN sur le damier (DamierContenu, PositionCourante et images) et l'ajoute aux listes de coups si ajouteListeCoups ---
            GestionDamier.ViderDamier();
            Console.WriteLine("Mise en place de la position FEN : " + fenAutiliser);
            foreach (var pict in CaseDamier)
            {
                pict.Image = null;          // Efface l'image de chaque PictureBox
            }
            if (ajouteListeCoups)
            {
                ListeCoupsFen.Add(fenAutiliser);    // On enregistre la position dans les listes de coups
                ListeCoupsHub2.Add(Partie.Hub2DepuisFen(fenAutiliser));
                // Note : On ne touche pas à ListeCoupsPdn car aucun n'a été joué pour l'instant
            }
            // Trouver la première occurrence de guillemet
            int debutIndex = fenAutiliser.IndexOf("\"") + 1;        // On commence après le premier guillemet
            int finIndex = fenAutiliser.IndexOf("\"", debutIndex);  // On trouve le prochain guillemet après le debutIndex
            if (debutIndex >= 0 && finIndex > debutIndex)
            {   // Extraire le texte entre les guillemets
                string contenu = fenAutiliser.Substring(debutIndex, finIndex - debutIndex);
                string[] champsFen = contenu.Split(':');           // On récupère les 3 champs du FEN dans un tableau
                // Le champ 0 est la couleur qui doit jouer le prochain coup
                CouleurAuTrait = (champsFen[0] == "W") ? "Blanc" : (champsFen[0] == "B") ? "Noir" : "Inconnu";
                // Active les cases pour la couleur actuelle
                ActiveCouleurDamier(CouleurAuTrait, true);
                // Désactive les cases de l'autre couleur
                ActiveCouleurDamier(CouleurAuTrait == "Blanc" ? "Noir" : "Blanc", false);

                Position position = Position.DepuisFen(contenu);        // Lève FormatException si le FEN n'est pas valide
                for (int numeroCase = 1; numeroCase <= 50; numeroCase++)
                {   // Dessiner chaque pièce : DessinePiece met aussi à jour DamierContenu et GestionDamier.PositionCourante
                    Piece piece = position[numeroCase];
                    if (piece != Piece.Vide)
                        DessinePiece(GestionDamier.ObtenirIndicePictureBox(numeroCase), GestionDamier.TypeDe(piece), GestionDamier.CouleurDe(piece));
                }
            }
            else
            {
                Console.WriteLine("Aucun texte trouvé entre les guillemets.");
            }
        }
        public static string RecupereFEN()
        {   // --- Génère et renvoie le FEN de la position actuelle (GestionDamier.PositionCourante, avec le trait CouleurAuTrait) ---
            Position position = GestionDamier.PositionCourante.Copie();
            position.Trait = CouleurAuTrait == "Blanc" ? Couleur.Blanc : Couleur.Noir;
            return "[FEN \"" + position.VersFen() + "\"]";
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
        private static Coup? TrouveCoupPdn(string mouvement)
        {   // --- Le coup légal que désigne ce coup PDN (« 32-28 », « 28x19 », « 28x19x8 ») dans la position affichée, ou null s'il n'y en a aucun ---
            // Si « 28x19 » désigne plusieurs rafles qui ne prennent pas les mêmes pièces, on joue la première (comme avant) et on le note dans le journal
            Position position = Position.DepuisFen(RecupereFEN());
            Coup? coup = ReglesDames.TrouverCoupPdn(position, mouvement);
            if (coup != null)
                return coup;
            int[] cases = mouvement.Split('-', 'x').Select(int.Parse).ToArray();
            List<Coup> candidats = ReglesDames.CoupsLegaux(position).Where(c => c.Depart == cases[0] && c.Arrivee == cases[^1] && c.EstPrise == mouvement.Contains('x')).ToList();
            if (candidats.Count > 1)
            {
                Journal.Ecrit($"AMBIGU PDN {RecupereFEN()} : {mouvement} peut être {string.Join(" ou ", candidats.Select(c => c.Hub2()))}, joué : {candidats[0].Hub2()}");
                return candidats[0];
            }
            return null;
        }
        private void EnregistrePositionApresCoupHumain()
        {   // --- Après un coup humain : change le trait et ajoute la nouvelle position aux listes FEN et Hub2 (le coup PDN est ajouté à l'exécution) ---
            ChangerCouleurTrait();
            string fenPosition = RecupereFEN();
            Partie.ListeCoupsFen.Add(fenPosition);
            Partie.ListeCoupsHub2.Add(Partie.Hub2DepuisFen(fenPosition));
            indexFenCoupActuel = ListeCoupsFen.Count - 1;       // Toujours pointer sur le dernier coup
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Mouvements  divers
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        public void ExecuteDeplacement(int indexSourceBox, int destinationManoury, ContenuCase pieceSource, bool piecePrise, int casePrise = 0)
        {   // --- Execute un déplacement simple en vérifiant la promotion ---
            ActiveDamier(false); // Désactiver les interactions pendant le déplacement
            EffaceCasesDestination();   // Efface les cadres gris des cases de destination générées lors de l'AnalyseCoupoObligatoire
            var couleurPiece = pieceSource.CouleurPiece;
            ListeCasesCoupSelectionne.Add(GestionDamier.PictureBoxVersManoury[indexSourceBox]);     // Ajoute la case source au suivi des coups
            if (piecePrise && casePrise > 0)
            {   // Efface la pièce prise
                EffacePiece(GestionDamier.ManouryVersIndexPictureBox[casePrise - 1], true);
                CaseDamier[GestionDamier.ManouryVersIndexPictureBox[casePrise - 1]].BackColor = CouleurTrajetSuivi;
                ListeCasesCoupSelectionne.Add(casePrise); // Ajoute la case prise
            }
            // Vérification si promotion en Dame : IL FAUDRAIT S'ASSURER QUE LA PIECE N'EST PAS DEJA UNE DAME !!!!!!!!!!!!!!!!!!!
            if (derniereRangeeBlanche.Contains(destinationManoury) && pieceSource.CouleurPiece == CouleurPiece.Noir)
            {
                pieceSource.TypePiece = TypePiece.DameNoire; // Promotion en Dame noire
            }
            else if (derniereRangeeNoire.Contains(destinationManoury) && pieceSource.CouleurPiece == CouleurPiece.Blanc)
            {
                pieceSource.TypePiece = TypePiece.DameBlanche; // Promotion en Dame blanche
            }
            DessinePiece(GestionDamier.ManouryVersIndexPictureBox[destinationManoury - 1], pieceSource.TypePiece, pieceSource.CouleurPiece);
            CaseDamier[GestionDamier.ManouryVersIndexPictureBox[destinationManoury - 1]].BackColor = CouleurCaseArrivee;
            EffacePiece(indexSourceBox, true);                  // Efface la pièce source
            CaseDamier[indexSourceBox].BackColor = CouleurTrajetSuivi;      // mais on la colorie pour suivre le trajet
            ListeCasesCoupSelectionne.Add(destinationManoury);  // Ajoute la destinationManoury au suivi des coups
            string coupEffectue = piecePrise
                ? $"{GestionDamier.PictureBoxVersManoury[indexSourceBox]}x{destinationManoury}"
                : $"{GestionDamier.PictureBoxVersManoury[indexSourceBox]}-{destinationManoury}";
            ListeCoupsPdn.Add(coupEffectue);            // Ajoute le mouvement dans la liste des coups au format pdn.
            if (!jeuMoteurEnCours)                      // Et on l'affiche si ce n'est pas le moteur qui joue
                LabelCoupJoue.Text = "Coup joué : " + coupEffectue;
            Console.WriteLine($"Coup joué : {coupEffectue}");
            ActiveDamier(true);         // Réactiver les interactions après le déplacement
            ActiveCouleurDamier(couleurPiece.ToString(), false);
            VerifierGain();
        }
        public async Task AnimerEtExecuterRafle(int indexSourceBox, List<(int casePrise, int caseArrivee)> rafle, ContenuCase pieceSource)
        {   // --- Execute une rafle complète avec une animation à la vitesse DureeAnimation, ne prend les pièces qu'à la fin de la rafle ---
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
                int indexPriseBox = GestionDamier.ManouryVersIndexPictureBox[casePrise - 1];
                int indexArriveeBox = GestionDamier.ManouryVersIndexPictureBox[caseArrivee - 1];
                CaseDamier[indexPriseBox].BackColor = CouleurCasePrise;       // Marquer la case de prise en CouleurCasePrise
                CaseDamier[indexArriveeBox].BackColor = CouleurCasePivot;     // et la case d'arricée en CouleurCasePivot
                CaseDamier[indexArriveeBox].Image = CadreGrisSansPrise;       // Afficher le cadre gris sans pièce
                CaseDamier[indexSourceBoxActuel].Image = imagePiece;  // Afficher la pièce source

                // Attendre un peu pour l'effet visuel de la prise
                await Task.Delay(DureeAnimation);
 
                boxesAEffacerFinRafle.Add(indexPriseBox);                   // On enregistre la pièce qu'il faudra prendre, mais on ne l'efface pas encore
                EffacePiece(indexSourceBoxActuel, true);                    // On efface la pièce qui prend sur la case de départ
                DessinePiece(indexArriveeBox, typePiece, couleurPiece);     // ON place la pièce qui prend sur sa case d'arrivée
                // Attendre un peu avant de continuer avec la prochaine prise
                await Task.Delay(DureeAnimation);

                indexSourceBoxActuel = indexArriveeBox;                     // Mettre à jour l'indice de la source pour le prochain déplacement
                derniereCaseRafle = caseArrivee;                            // Enregistrer la dernière case de la rafle pour vérifier la promotion
            }
            // Vérification si promotion en Dame à la fin de la rafle
            if (derniereRangeeBlanche.Contains(derniereCaseRafle) && couleurPiece == CouleurPiece.Noir)
            {   // promotion du pion noir
                typePiece = TypePiece.DameNoire; // Promotion en Dame noire
                DessinePiece(GestionDamier.ManouryVersIndexPictureBox[derniereCaseRafle - 1], typePiece, couleurPiece);
                CaseDamier[GestionDamier.ManouryVersIndexPictureBox[derniereCaseRafle - 1]].BackColor = CouleurCaseArrivee;
            }
            else if (derniereRangeeNoire.Contains(derniereCaseRafle) && couleurPiece == CouleurPiece.Blanc)
            {   // promotion du pion blanc
                typePiece = TypePiece.DameBlanche; // Promotion en Dame blanche
                DessinePiece(GestionDamier.ManouryVersIndexPictureBox[derniereCaseRafle - 1], typePiece, couleurPiece);
                CaseDamier[GestionDamier.ManouryVersIndexPictureBox[derniereCaseRafle - 1]].BackColor = CouleurCaseArrivee;
            }
            // Parcourir la liste des pièces prises et effacer les pièces
            foreach (var index in boxesAEffacerFinRafle)
            {
                EffacePiece(index, true);
                await Task.Delay(DureeAnimation);
            }
            boxesAEffacerFinRafle.Clear();
            string coupEffectue = GestionDamier.PictureBoxVersManoury[indexSourceBox] + "x"  + derniereCaseRafle;
            CaseDamier[GestionDamier.ObtenirIndicePictureBox(derniereCaseRafle)].BackColor = CouleurCaseArrivee;
            ListeCoupsPdn.Add(coupEffectue);            // Ajoute le mouvement dans la liste des coups au format pdn.
            if (!jeuMoteurEnCours)                      // Et on l'affiche si ce n'est pas le moteur qui joue
                LabelCoupJoue.Text = "Coup joué : " + coupEffectue;
            Console.WriteLine($"Coup joué : {coupEffectue}");
            ActiveDamier(true); // Réactiver les interactions après l'animation
            ActiveCouleurDamier(couleurPiece.ToString(), false);
            VerifierGain();
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Procédures d'affichage diverses
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void AfficheScan(string ligne)  // Analyse et affiche les informations du moteur SCAN (appelée sur le thread du moteur, passe sur celui de l'interface)
        {   // --- Le protocole Hub2 est malheuresement assez peu documenté ...  ---
            if (IsDisposed || !IsHandleCreated)
                return;     // La fenêtre se ferme : plus rien à afficher
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AfficheScan), ligne);
                return; // Empêche l'exécution du reste de la méthode sur le thread d'origine
            }
            try
            {
                AnalyseLigneMoteur(ligne);
            }
            catch (Exception ex)
            {   // Une ligne du moteur que l'on ne sait pas lire ne doit jamais faire tomber l'application
                Journal.Ecrit($"Affichage d'une ligne du moteur impossible ({ex.Message}) : {ligne}");
            }
        }
        private void AnalyseLigneMoteur(string ligne)
        {
            donneesMoteurScan = ligne.Split(' ');     // Découpage des informations du moteur SCAN
            if (string.IsNullOrWhiteSpace(donneesMoteurScan[0]) == false & donneesMoteurScan.Length > 2)
            {   // true si la chaine est " ", "\n", null, ""
                switch (donneesMoteurScan[0])   // identifier le premier mot
                {
                    case "\n":                  // Analyse réponse moteur SCAN
                    case " ":
                        break;
                    case "done":            // le moteur SCAN propose le meilleur coup
                        // Le meiileur coup est traité dans MoteurDamesScan.ProcOutputDataReceived
                        break;
                    case "id":      // sous la forme : id name=... version=... author="..." country=...
                        {   // Expression régulière pour extraire les données
                            string pattern = @"name=(\S+)|version=(\S+)|author=""([^""]+)""|country=(\S+)";     // Extraction des correspondances
                            string? name = null, version = null, author = null, country = null;
                            foreach (Match match in Regex.Matches(ligne, pattern))
                            {
                                if (match.Groups[1].Success) name = match.Groups[1].Value;
                                if (match.Groups[2].Success) version = match.Groups[2].Value;
                                if (match.Groups[3].Success) author = match.Groups[3].Value;
                                if (match.Groups[4].Success) country = match.Groups[4].Value;
                            }           // Affichage des résultats
                            LabelAfficheScan.Text = "Moteur : " + name + " / Version : " + version + " / Auteur : " + author + " / Pays : " + country;
                            nomMoteur = name ?? "";
                            versionMoteur = version ?? "";
                            auteurMoteur = author ?? "";
                        }
                        break;
                    case "info":
                        for (int scanIndex = 1; (scanIndex < donneesMoteurScan.Length); scanIndex++) // Recherche des informations sur la chaine donneesMoteurScan
                            switch (donneesMoteurScan[scanIndex].Split('=')[0])
                            {   // On regarde ce qu'il y a avant le signe =
                                case "score":
                                    // Conversion en décimal avec gestion de la culture
                                    decimal valeur = Decimal.Parse(donneesMoteurScan[scanIndex].Split('=')[1], CultureInfo.InvariantCulture);
                                    LabelScore.Text = "Score = " + valeur.ToString("F2", CultureInfo.InvariantCulture);     // Affichage avec le format "XX.YY"
                                    break;
                                case "pv":          // Affichage de la variation principlale
                                    int position = ligne.IndexOf(" pv");
                                    string variationMoteur = ligne.Substring(position + 4);   // exemple : pv="14x23x19 50-44 
                                    variationMoteur = variationMoteur.Trim('"');       // Il arrive qu'il y ait un " en début de chaine ??!
                                    if (analyseEnCours)
                                        {
                                        LabelPrisesPossibles.Text = "Ligne SCNA 3.1 complète : " + variationMoteur;
                                        LabelCoupJoue.Text = "Coup suggéré par l'analyse =  : " + variationMoteur.Split(' ')[0];
                                        }
                                    if (variationMoteur.Length > 100)
                                        variationMoteur = variationMoteur.Substring(0, 100);     // On limite la longueur de la variation, pour rester dans le label
                                    LabelAfficheScan.Text = "Ligne Scan3.1 = " + variationMoteur;
                                    break;
                            }
                        break;
                    case "error":   // Déjà noté dans le journal par MoteurDamesScan
                        break;
                }
            }
        }
        private void AfficheDonneesBrutes(string ligne, bool venantDuMoteur)
        {   // --- Ajoute une ligne échangée avec le moteur à la fenêtre du protocole (appelée sur le thread du moteur ou celui de l'interface) ---
            if (IsDisposed || !IsHandleCreated)
                return;     // La fenêtre se ferme : plus rien à afficher
            if (InvokeRequired)
            {
                Invoke(new Action<string, bool>(AfficheDonneesBrutes), ligne, venantDuMoteur);
                return;     // Empêche le code suivant de s'exécuter sur le thread secondaire
            }
            RichTextBox vue = donneesBrutesDames.DonneesBrutesVue;
            if (venantDuMoteur)
                vue.AppendText(Environment.NewLine + "[" + "SCAN 3.1" + "]" + ligne);
            else
                vue.AppendText(Environment.NewLine + " [BrunoGUI_Dames]    " + ligne);
            vue.SelectionStart = vue.TextLength;
            vue.ScrollToCaret();        // Pour garder la dernière ligne visible, quel que soit le sens de l'échange (avant, seules les lignes de l'interface faisaient défiler)
        }
        private void MoteurArrete(string message)
        {   // --- Le moteur s'est arrêté sans qu'on le lui demande (appelée sur le thread du moteur) ---
            if (IsDisposed || !IsHandleCreated)
                return;
            if (InvokeRequired)
            {
                Invoke(new Action<string>(MoteurArrete), message);
                return;
            }
            DesactiveMoteur(message);
        }
        private void DesactiveMoteur(string message)
        {   // --- Sans moteur : on désactive ce qui en dépend (l'ordinateur ne joue plus, pas d'analyse), le jeu entre humains reste possible ---
            moteurDisponible = false;
            PartieEnCours = false;
            OrdinateurBosse.Checked = false;
            OrdinateurBosse.Enabled = false;
            OrdinateurJoue.Enabled = false;
            AnalysePosition.Enabled = false;
            LabelAfficheScan.Text = message;
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {   // --- À la fermeture de la fenêtre, par la croix comme par « Quitter », on arrête le moteur (sinon scan.exe resterait actif) ---
            base.OnFormClosing(e);
            if (!e.Cancel)
                MoteurDamesScan.Quitte();
        }        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Routines de Dessin
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐

        public static void DessinePiece(int indexPictureBox, TypePiece pieceType, CouleurPiece pieceCouleur)
        {   // --- Dessin de la pièce passée en paramètre sur la case PictureBox en paramètre + mise à jour de DamierContenu et de PositionCourante ---
            try
            {   // Traduire l'index linéaire 0-99 en coordonnées (ligne, colonne) pour DamierContenu
                (int ligne, int colonne) = GestionDamier.IndiceVersCoordonnees(indexPictureBox);
                // Mettre à jour DamierContenu
                GestionDamier.DamierContenu[ligne, colonne].CouleurPiece = pieceCouleur;
                GestionDamier.DamierContenu[ligne, colonne].TypePiece = pieceType;
                GestionDamier.PositionCourante[GestionDamier.ObtenirCaseManoury(indexPictureBox)] = GestionDamier.PieceDe(pieceType);   // Mettre à jour la position
                AfficheListesPieces();
                if (pieceType == TypePiece.Vide)
                {   // Si la case est vide, on efface l'image
                    CaseDamier[indexPictureBox].Image = null;
                    return;
                }
                if (ListeBitmapsPiece.ContainsKey(pieceType))
                {   // Définit l'image en fonction du type de pièce
                    CaseDamier[indexPictureBox].Image = ListeBitmapsPiece[pieceType];
                }
                else
                {
                    CaseDamier[indexPictureBox].Image = null;     // Par défaut, aucune image
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur dans DessinePiece : " + ex.Message);
                Console.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }
        public static void EffacePiece(int indexPictureBox, bool damier)
        {   // --- Efface la pièce située à indexPictureBox, et actualise DamierContenu et PositionCourante si damier est vrai ---
            if (damier)
            {
                (int ligne, int colonne) = GestionDamier.IndiceVersCoordonnees(indexPictureBox);
                GestionDamier.DamierContenu[ligne, colonne].TypePiece = TypePiece.Vide;
                GestionDamier.DamierContenu[ligne, colonne].CouleurPiece = CouleurPiece.Vide;
                GestionDamier.PositionCourante[GestionDamier.ObtenirCaseManoury(indexPictureBox)] = Piece.Vide;     // Retirer la pièce de la position
                AfficheListesPieces();
            }
            CaseDamier[indexPictureBox].Image = null;
        }
        public static void DessineMouvements(int caseSourceManoury, List<Coup> coups)
        {   // --- Dessine tous les mouvements légaux de la pièce en case source (coups : ses coups selon ReglesDames, déjà limités aux rafles les plus longues) ---
            CaseDamier[GestionDamier.ManouryVersIndexPictureBox[caseSourceManoury - 1]].BackColor = CouleurCaseDepart; // Colorier la case source
            if (coups.Any(coup => coup.EstPrise))
            {   // Rafles : s'il en existe plusieurs de même longueur, elles sont toutes signalées, c'est au joueur de choisir
                // Étape 1 : traiter les rafles (une étape par pièce prise : case de la pièce prise et case d'arrivée)
                foreach (Coup rafle in coups)
                {
                    foreach (var (prise, arrivee) in rafle.Prises.Zip(rafle.Chemin, (prise, arrivee) => (prise, arrivee)))
                    {
                        int indexPrise = GestionDamier.ManouryVersIndexPictureBox[prise - 1];
                        int indexArrivee = GestionDamier.ManouryVersIndexPictureBox[arrivee - 1];
                        if (CaseDamier[indexArrivee].Image == null)       // Ne pas effacer les pièces si la case est occupée
                        {
                            CaseDamier[indexPrise].BackColor = CouleurCasePrise;
                            CaseDamier[indexArrivee].Image = CadreGrisSansPrise;
                            CaseDamier[indexArrivee].BackColor = Color.Orange;    // A enlever si on ne veut que les cases pièces prises !!!!
                            // Sauvegarder les cases pour les étapes ultérieures
                            ListeCasesMouvementPrecedent.Add(prise);
                            ListeCasesMouvementPrecedent.Add(arrivee);
                            ListeCasesDestination.Add(arrivee); // Garder les arrivées pour vérification finale
                        }
                    }
                }
                // Étape 2 : colorier uniquement les cases d'arrivée finale en CouleurCaseArrivee
                foreach (Coup rafle in coups)
                    CaseDamier[GestionDamier.ManouryVersIndexPictureBox[rafle.Arrivee - 1]].BackColor = CouleurCaseArrivee;
            }
            else
            {   // Déplacements simples (uniquement si aucune rafle n'existe), affichés en CouleurTrajetSuivi avec un cadre gris
                foreach (Coup coup in coups)
                {
                    int index = GestionDamier.ManouryVersIndexPictureBox[coup.Arrivee - 1];
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
                CaseDamier[GestionDamier.ManouryVersIndexPictureBox[ListeCasesDestination[i] - 1]].Image = null;
            }
            ListeCasesDestination.Clear();
            for (int i = 0; i < ListeCasesMouvementPrecedent.Count; i++)    // On enlève tous les mouvements affichés précédemment
            {   // On remet la couleur de base sur les cases
                CaseDamier[GestionDamier.ManouryVersIndexPictureBox[ListeCasesMouvementPrecedent[i] - 1]].BackColor = BrunoInterfaceGraphiqueDames.CouleurCasesombre;
            }
            ListeCasesMouvementPrecedent.Clear();
        }
        public static void EffaceCasesDestination()
        {   // --- Efface les cases de destination des mouvements possibles ---
            for (int i = 0; i < ListeCasesDestination.Count; i++)           // On enlève tous les cadres gris
            {   // On enlève les cadres gris
                CaseDamier[GestionDamier.ManouryVersIndexPictureBox[ListeCasesDestination[i] - 1]].Image = null;
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
                couleur = ligne % 2 == 0 ? CouleurCasesombre : CouleurCaseclaire;       // Couleur des cases du damier
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
                    IndiceVisuCoteNoir.Add(index);      // et on crée la liste des indices de 1 à 100
                    // Vérifier si la case est sombre
                    if ((ligne % 2 == 0 && colonne % 2 == 0) || (ligne % 2 != 0 && colonne % 2 != 0))
                    {   // Associer la case logique à sa PictureBox dans le dictionnaire 
                        ManouryVersPictureBoxIndicee[caseLogique] = Pict;
                        AfficheIndexBox(Pict, indexCourant, caseLogiqueCourant);
                        caseLogique++; // Passer à la case logique suivante
                    }
                    CouleurCaseOrigines.Add(couleur);
                    Pict.BringToFront();
                    CaseDamier.Add(Pict);
                    Pict.MouseDown += CaseMouseDown;
                    Damier10x10.Controls.Add(Pict);
                    index++;
                    couleur = couleur == CouleurCaseclaire ? CouleurCasesombre : CouleurCaseclaire;
                }
            }
            IndiceVisuCoteNoir.Reverse();       // Inverser la liste des indices pour les cases sombres
        }
        public void ChangerCouleurTrait()
        {   // Inverse la valeur de CouleurAuTrait
            CouleurAuTrait = CouleurAuTrait == "Blanc" ? "Noir" : "Blanc";
            // Active les cases pour la couleur actuelle
            ActiveCouleurDamier(CouleurAuTrait, true);
            // Désactive les cases de l'autre couleur
            ActiveCouleurDamier(CouleurAuTrait == "Blanc" ? "Noir" : "Blanc", false);
            LabelInformationJoueur.Text = "Aux " + CouleurAuTrait + "s de jouer";
        }
        public static string CouleurPasAuTrait()
        {   // Vérifie la valeur de CouleurAuTrait et retourne l'opposé
            return CouleurAuTrait == "Blanc" ? "Noir" : "Blanc";
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
        public static void ActiveCouleurDamier(string couleur, bool statut)
        {   // Couleur : "Blanc" ou "Noir", active si statut = true
            for (int i = 0; i <= 99; i++)
            {
                (int ligne, int colonne) = GestionDamier.IndiceVersCoordonnees(i);
                var piece = GestionDamier.DamierContenu[ligne, colonne];
                // Active/désactive uniquement si la pièce correspond à la couleur demandée
                if (piece != null
                    && string.Equals(piece.CouleurPiece.ToString(), couleur, StringComparison.OrdinalIgnoreCase))
                {
                    CaseDamier[i].Enabled = statut;
                }
            }
        }
        public static void ActiverCaseManoury(int caseManoury, bool statut)
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
                RadioGainNoir.Checked = PartieTerminee = FinPartie = true;  // Note : RadioGainNoir.Checked mis à true lance la méthode RadioGainNoir_CheckedChanged
                LabelInformationJoueur.Text = "Les Noirs gagnent, les blancs n'ont plus de pièces";
                MessageBox.Show("Les Noirs gagnent !\n Les blancs n'ont plus de pièces", "Partie terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (GestionDamier.NombreDePieces(Couleur.Noir) == 0)
            {   // Vérifier si les listes noires sont vides
                GroupResultat.Enabled = false;   // Désactive le groupe de boutons
                RadioGainBlanc.Checked = PartieTerminee = FinPartie = true; // Note: RadioGainBlanc.Checked mis à true lance la méthode RadioGainBlanc_CheckedChanged
                LabelInformationJoueur.Text = "Les Blancs gagnent, les noirs n'ont plus de pièces";
                MessageBox.Show("Les Blancs gagnent !\n Les noirs n'ont plus de pièces", "Partie terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            // TODO : joueur qui ne peut plus bouger : ReglesDames.EstTerminee(position) le dit déjà
            // TODO : Chercher quels sont les cas de gain de la partie ?
            // TODO : Chercher quels sont les cas de partie nulle ?
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

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des boutons
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void NouvellePartie_Click(object sender, EventArgs e)
        {   // --- Initialisation du damier et mise à zéro des diverses listes --- 
            GestionDamier.ViderDamier();
            GestionDamier.InitialiserDamier();  // intialise DamierContenu (10*10) avec les pièces SANS les afficher
            ExisteFENDebut = false;
            ActiveDamier(true);
            ListeCoupsFen.Clear();
            ListeCoupsPdn.Clear();
            ListeCoupsHub2.Clear();
            indexFenCoupActuel = 0;     // On repart du début
            NettoyageRapide();
            MiseenplaceFen("[FEN \"W:W31-50:B1-20\"]", true);   // Position initiale
            AfficheListesPieces();
            InitialiserEtatMouvement();
            LabelCoupJoue.Text = "";
        }
        private void NettoyageRapide()
        {   // --- Nettoyage rapide du damier, labels et boutons  ---
            CouleurToutesCasesManoury(CouleurCasesombre);       // On efface les couleurs des cases
            GroupResultat.Enabled = true;                       // On remet à zéro le résultat
            RadioGainNoir.Checked = false;
            RadioGainBlanc.Checked = false;
            RadioNulle.Checked = false;
            PartieTerminee = FinPartie = false;
            LabelCoupJoue.Text = LabelInformationJoueur.Text = LabelPrisesPossibles.Text = "";
        }
        private void BloqueBoutons(bool bloque) 
        {   // --- Blocage (true) /déblocage (false) des boutons ---
            NouvellePartie.Enabled = OrdinateurJoue.Enabled = BoutonRetourArriere.Enabled = SauvePositionFen.Enabled = !bloque;
            VisualisationPdn.Enabled = boutonMasqueAffiche.Enabled = groupParcoursPartie.Enabled = AnalysePosition.Enabled = !bloque;
            ChargePartiesPdn.Enabled = boutonMasqueAffiche.Enabled = SauvePartiesPdn.Enabled = ChargePositionFen.Enabled = !bloque;
            if (!moteurDisponible)
                OrdinateurJoue.Enabled = AnalysePosition.Enabled = false;      // Sans moteur, ces deux boutons restent désactivés
        }
        private void MontreDonneesScan_Click(object sender, EventArgs e)
        {   // --- Affiche ou masque les données de SCAN 3.1 à chaque clic ---
            montreDonneesDames = !montreDonneesDames;       
            MontreDonneesScan.Text = montreDonneesDames ? "Masque protoc." : "Affiche protoc.";
            if (montreDonneesDames) donneesBrutesDames.Show();    // On affiche les données brutes SCAN
            else donneesBrutesDames.Hide();                       // On masque les données brutes SCAN
            donneesBrutesDames.DonneesBrutesVue.ScrollToCaret();      // Pour garder l'affichage dans toute la fenêtre
        }
        private void VisualisationPdn_Click(object sender, EventArgs e)
        {   // --- Bouton pour voir la partie en Pdn ---
            if (affichePdn == null || affichePdn.IsDisposed)
            {   // Traitement pour prendre en compte la fermeture par croix rouge en haut à droite ...
                affichePdn = new AffichePdn();      // Crée une nouvelle instance si nécessaire
                affichePdn.FormClosed += (s, args) =>
                {   // On évite que la référence de affichePdn pointe vers un objet supprimé.
                    affichePdn = null;  // S'assure que la référence est libérée à la fermeture
                };
            }
            affichePdn.Show();          // Affiche la fenêtre et met à jour son contenu
            affichePdn.AffichePdnDansZone();
        }
        private void BoutonPrecedent_Click(object sender, EventArgs e)
        {   // --- Bouton pour reculer d'un coup (parcours de partie) ---
            if (indexFenCoupActuel > 0) // Assure que l'on peut reculer sans sortir de la liste
            {
                indexFenCoupActuel--;
                MiseenplaceFen(ListeCoupsFen[indexFenCoupActuel], false);
                if (indexFenCoupActuel > 0)         // Vérifie si l'on est au premier coup pour éviter un accès incorrect à ListeCoupsPdn
                {
                    LabelCoupJoue.Text = $"Coup {indexFenCoupActuel} : {ListeCoupsPdn[indexFenCoupActuel - 1]}";
                }
                else
                {   // Si on est au début, afficher un message spécial ou la position initiale.
                    LabelCoupJoue.Text = $"Position initiale : {ListeCoupsPdn[0]}";
                }
                Console.WriteLine("Précédent : indexFenCoupActuel : " + indexFenCoupActuel);
            }
            else
            {
                MessageBox.Show("Vous êtes au début de la partie.", "Début de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonSuivant_Click(object sender, EventArgs e)
        {   // --- Bouton pour avancer d'un coup (parcours de partie) ---
            Console.WriteLine("Suivant : indexFenCoupActuel : " + indexFenCoupActuel);
            Console.WriteLine("ListeCoupsFen.Count : " + ListeCoupsFen.Count);
            Console.WriteLine("ListeCoupsPdn.Count : " + ListeCoupsPdn.Count);
            if (indexFenCoupActuel <= ListeCoupsFen.Count - 1 && indexFenCoupActuel <= ListeCoupsPdn.Count) // Vérifie la synchronisation
            {
                MiseenplaceFen(ListeCoupsFen[indexFenCoupActuel], false);
                // Accède à ListeCoupsPdn en tenant compte du décalage
                LabelCoupJoue.Text = $"Coup {indexFenCoupActuel} : {ListeCoupsPdn[indexFenCoupActuel - 1]}";
                indexFenCoupActuel++;
            }
            else
            {
                MessageBox.Show("Vous êtes à la fin de la partie ou une des listes est vide.", "Fin de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonDebut_Click(object sender, EventArgs e)
        {   // --- Bouton pour aller au début de la partie ---
            CouleurToutesCasesManoury(CouleurCasesombre);       // On efface les couleurs des cases
            if (ListeCoupsFen.Count > 1) // Vérifie qu'il y a au moins un coup joué
            {
                indexFenCoupActuel = 1; // Premier coup joué (position initiale est à l'index 0)
                MiseenplaceFen(ListeCoupsFen[indexFenCoupActuel - 1], false);
                LabelCoupJoue.Text = "Début de partie";
                Console.WriteLine("Début : indexFenCoupActuel : " + indexFenCoupActuel);
            }
        }
        private void BoutoonFin_Click(object sender, EventArgs e)
        {   // --- Bouton pour aller à la fin de la partie ---
            if (ListeCoupsFen.Count > 1) // Vérifie qu'il y a au moins un coup joué
            {
                indexFenCoupActuel = ListeCoupsFen.Count - 1; // Dernière position
                MiseenplaceFen(ListeCoupsFen[indexFenCoupActuel], false);
                LabelCoupJoue.Text = $"Coup {indexFenCoupActuel} : {ListeCoupsPdn[indexFenCoupActuel - 1]}";
                Console.WriteLine("Fin : indexFenCoupActuel : " + indexFenCoupActuel);
            }
        }
        private async void OrdinateurJoue_Click(object sender, EventArgs e)
        {   // --- L'ordinateur joue son meilleur coup ---
            await LancementReflexionMoteur(Partie.ListeCoupsHub2[Partie.ListeCoupsHub2.Count - 1], TempsReflexion);
            PartieEnCours = true;
            ChangerCouleurTrait();      // Inverse le trait
            AnalyseCoupObligatoire(CouleurAuTrait);     // Vérifie si un coup obligatoire existe
        }

        private async void AnalysePosition_Click(object sender, EventArgs e)
        {
            LabelInformationJoueur.Text = "Analyse de la position en cours ... (" + TempsReflexion + ")";
            BloqueBoutons(true);
            analyseEnCours = true;
            Console.WriteLine("Analyse de la position à l'index : " + indexFenCoupActuel);
            await LancementReflexionMoteur(ListeCoupsHub2[indexFenCoupActuel], TempsReflexion);
            LabelInformationJoueur.Text = "Analyse terminée !";
            BloqueBoutons(false);
            analyseEnCours = false;
        }

        private void ChargePartiesPdn_Click(object sender, EventArgs e)
        {   // --- Affiche la boîte de dialogue et traite le fichier PDN sélectionné  ---
            ListePartiesPdn.Clear(); // On vide la liste des parties PDN
            if (ChargerPartiesPdn.ShowDialog() == DialogResult.OK)
            {
                string cheminFichier = ChargerPartiesPdn.FileName;
                try
                {   // Vérifie et obtient le chemin complet
                    string fullPath = Path.GetFullPath(cheminFichier);
                    Console.WriteLine("Chemin complet du fichier : " + fullPath);

                    // Lire le contenu du fichier et l'afficher dans la console
                    string contenuFichier = File.ReadAllText(fullPath);
                    NettoyageRapide();
                    if (fichierPartiePdn == null || fichierPartiePdn.IsDisposed)
                    {   // Traitement pour prendre en compte la fermeture par croix rouge en haut à droite ...
                        fichierPartiePdn = new FichierPartiePdn();
                        fichierPartiePdn.FormClosed += (s, args) =>
                        {   // On évite que la référence de affichePdn pointe vers un objet supprimé.
                            fichierPartiePdn = null;
                        };
                    }
                    Console.WriteLine("Contenu du fichier PDN : \n" + contenuFichier);
                    ListePartiesPdn.AddRange(fichierPartiePdn.LireParties(fullPath));     // Lit toutes les parties du fichier PDN
                    fichierPartiePdn.NombrePartiesFichier.Text = ListePartiesPdn.Count.ToString()
                        + " partie(s) dans le fichier  " + cheminFichier.Substring(cheminFichier.LastIndexOf('\\') + 1); ;
                    fichierPartiePdn.AfficherListeParties(ListePartiesPdn); 
                    fichierPartiePdn.Show();
                    boutonMasqueAffiche.Enabled = true; // Active le bouton pour masquer/afficher la liste
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Chargement Pdn : Erreur lors de la lecture du fichier : " + ex.Message);
                }
            }
        }
        public void ChargerPartieDepuisPdn(PartieDamesPdn partie)
        {   // --- Charge une partie depuis un fichier PDN lorsque'on double-clique ---
            NettoyageRapide();
            PartieCourante.CoupsPartiePDN = "";
            Partie.ListeCoupsPdn.Clear();
            Console.WriteLine("ChargerPartie :  " + partie);
            BoxNomJoueurNoir.Text = NomJoueurNoir = partie.Black;
            PartieCourante.Event = partie.Event;
            PartieCourante.Site = partie.Site;
            PartieCourante.Date = partie.Date;
            PartieCourante.Round = partie.Round;
            PartieCourante.White = BoxNomJoueurBlanc.Text = NomJoueurBlanc = partie.White;
            PartieCourante.Black = BoxNomJoueurNoir.Text = NomJoueurNoir = partie.Black;
            PartieCourante.Result = partie.Result;
            PartieCourante.CoupsPartiePDN = partie.CoupsPartiePDN;
            PartieCourante.FENDebut = partie.FENDebut;
            Console.WriteLine($"Liste des coups : {PartieCourante.CoupsPartiePDN}");
            LabelAfficheScan.Text = $"Partie de {partie.White} contre {partie.Black} ({partie.Result})";
            LabelScore.Text = $"Résultat : {partie.Result}";
            ExisteFENDebut = !string.IsNullOrWhiteSpace(partie.FENDebut);     // Propre à CETTE partie (avant, le drapeau gardait la valeur de la dernière partie lue du fichier)
            GenerationListesCoups(PartieCourante.CoupsPartiePDN);
        }
        public async void GenerationListesCoups(string coupsPartiePDN)
        {   // --- Génère les listes et affiche les coups à partir de la partie PDN ---
            ListeCoupsFen.Clear();  // On vide la liste avant d’ajouter les nouveaux coups
            indexFenCoupActuel = 0; // On repart du début
            ListeCoupsHub2.Clear(); // On vide la liste avant d’ajouter les nouveaux coups
            if (ExisteFENDebut)
            {
                MiseenplaceFen("[FEN \"" + PartieCourante.FENDebut + "\"]", true);  // Position initiale
                LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s";
            }
            else
            {
                MiseenplaceFen("[FEN \"W:W31-50:B1-20\"]", true);  // Position initiale
            }
            // Blocage des boutons pendant le traitement
            ActiveDamier(false);
            BloqueBoutons(true);
            coupsPartiePDN = coupsPartiePDN.Replace("\n", " "); // Remplace les retours chariots par des espaces
            // Séparation des coups en supprimant les espaces inutiles
            string[] coups = coupsPartiePDN.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (coups.Length == 0)                      // Vérifier si la liste est vide
            {
                MessageBox.Show("Aucun coup n'a été trouvé dans la partie.", "Information : Pas de coup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LabelPrisesPossibles.Text = "Pas de coup trouvé dans la partie chargée";
                BloqueBoutons(false);   // Remettre les boutons en fonction !!
                AnalyseCoupObligatoire(CouleurAuTrait);     // Vérifie si un coup obligatoire existe
                return;
            }            
            string dernierElement = coups.Last();       // Vérifier si le dernier élément est un résultat
            string[] resultatsValides = { "1-0", "0-1", "1/2-1/2", "*", "2-0", "0-2", "1-1", "-"};
            string resultatPartiePdn = "";
            if (resultatsValides.Contains(dernierElement))
                {
                    resultatPartiePdn = dernierElement;
                    coups = coups.Take(coups.Length - 1).ToArray(); // Supprimer le dernier élément
                }
            CouleurToutesCasesManoury(CouleurCasesombre);       // On efface les couleurs des cases
            await Task.Delay(DureeAnimation);
            foreach (string coupOriginal in coups)
            {
                string coup = coupOriginal; // Crée une copie modifiable
                Console.WriteLine("Coup original : " + coup);
                if (coup.Contains("."))
                {
                    coup = coup.Split('.')[1].Trim();
                }
                // Séparer les mouvements individuels ("23-19" ou "28x19x23")
                string[] mouvements = coup.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string mouvement in mouvements)
                {
                    Console.WriteLine("Mouvement = " + mouvement);
                    CouleurToutesCasesManoury(CouleurCasesombre);       // On efface les couleurs des cases
                    if (!Regex.IsMatch(mouvement, @"^\d+[-x]\d+")) // Vérifie le format (ex: "23-19" ou "28x19x23")
                    {
                        throw new FormatException($"Format invalide du coup : {mouvement}");
                    }
                    string[] positions = mouvement.Split(new char[] { '-', 'x' });
                    if (!positions.All(p => int.TryParse(p, out int num) && num >= 1 && num <= 50))
                    {   // Vérification des numéros de case Manoury
                        throw new ArgumentOutOfRangeException($"Numéro de case invalide dans le coup : {mouvement}");
                    }
                    int caseDepart = int.Parse(positions[0]);
                    var contenuCaseDepart = GestionDamier.ContenuCaseCaseManoury(caseDepart);
                    if (contenuCaseDepart.TypePiece == TypePiece.Vide)
                    {
                        throw new InvalidOperationException($"Pièce inconnue ou case vide : {caseDepart}");
                    }
                    Coup? coupPdn = TrouveCoupPdn(mouvement);       // Le coup légal que désigne cette notation dans la position affichée
                    if (coupPdn == null)
                    {
                        throw new InvalidOperationException($"Coup illégal : {mouvement}");
                    }
                    int indexSourceBox = GestionDamier.ObtenirIndicePictureBox(caseDepart);
                    if (coupPdn.EstPrise)       // Prise (rafle)
                    {
                        List<(int casePrise, int caseArrivee)> rafle = coupPdn.Prises.Zip(coupPdn.Chemin, (prise, arrivee) => (prise, arrivee)).ToList();
                        await AnimerEtExecuterRafle(indexSourceBox, rafle, contenuCaseDepart);
                        ChangerCouleurTrait();
                    }
                    else                        // Déplacement simple
                    {
                        ExecuteDeplacement(indexSourceBox, coupPdn.Arrivee, contenuCaseDepart, false);
                        ChangerCouleurTrait();
                        await Task.Delay(DureeAnimation);       // Attendre un peu pour l'effet visuel du déplacement
                    }
                    string positionFEN = RecupereFEN();
                    ListeCoupsFen.Add(positionFEN);   // Ajouter la position FEN après le coup
                    indexFenCoupActuel++;
                    ListeCoupsHub2.Add(Partie.Hub2DepuisFen(positionFEN)); // Ajouter la position Hub2 après le coup
                }
            }
            LabelPrisesPossibles.Text = "Fin des coups de la partie";
            BloqueBoutons(false);
        }
        public void boutonMasqueAffiche_Click(object? sender, EventArgs e)
        {   // --- Masque ou affiche la fenêtre de parties PDN ---
            if (fichierPartiePdn == null || fichierPartiePdn.IsDisposed)
            {   // Vérifier si la fenêtre a été fermée
                boutonMasqueAffiche.Enabled = false;
                boutonMasqueAffiche.Text = "Affiche parties";
                return;     // La fenêtre a été fermée, on s'assure que le bouton affiche "Affiche parties" et on sort
            }
            if (fichierPartiePdn.Visible)
            {   // Si la fenêtre est actuellement visible, on la masque
                fichierPartiePdn.Hide();    // Masquer la fenêtre
                boutonMasqueAffiche.Text = "Affiche parties";  // Changer le texte du bouton
            }
            else
            {
                fichierPartiePdn.Show();    // Afficher la fenêtre
                boutonMasqueAffiche.Text = "Masque parties";  // Changer le texte du bouton
            }
        }
        private void Apropos_Click(object sender, EventArgs e)
        {   // --- Affiche une boîte de dialogue avec les informations sur l'application ---
            string message = $"Version Interface graphique = 1.04 / Auteur : Bruno Courtois\n" +
                                $"Moteur : {nomMoteur} / Version = {versionMoteur} / Auteur : {auteurMoteur}";
            MessageBox.Show(message, "À propos", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void SauvePartiesPdn_Click(object sender, EventArgs e)
        {   // --- Ouvre la feneêtre de sauvegarde de fichier ---
            affichePdn?.LancerEnregistrementPdn();
        }
        private void PositionFen_Click(object sender, EventArgs e)
        {   // --- Charge une position au format FEN à partir d'un fichier  ---
            ListeCoupsFen.Clear();
            ListeCoupsPdn.Clear();
            ListeCoupsHub2.Clear();
            indexFenCoupActuel = 0;     // On repart du début
            NettoyageRapide();
            PartieCourante.White =PartieCourante.Black = PartieCourante.Result = PartieCourante.Round = PartieCourante.Site = PartieCourante.Event = "?";
            if (ChargerFichierFen.ShowDialog() == DialogResult.OK)
            {
                string cheminFichier = ChargerFichierFen.FileName;
                try
                {
                    // Lire le contenu du fichier et l'afficher dans la console
                    string contenuFichier = File.ReadAllText(cheminFichier);
                    MiseenplaceFen(contenuFichier, true);   // Met en place la position
                    AfficheListesPieces();
                    AnalyseCoupObligatoire(CouleurAuTrait);     // Vérifie si un coup obligatoire existe
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors de la lecture du fichier : " + ex.Message);
                }
            }
        }
        private void SauvePositionFen_Click(object sender, EventArgs e)
        {   // --- Sauvegarde la position au format FEN dans un fichier texte ---
            if (SauverFichierFen.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string fen = RecupereFEN();
                    File.WriteAllText(SauverFichierFen.FileName, fen);
                    MessageBox.Show($"Position FEN sauvegardée avec succès\ndans le fichier : {Path.GetFileName(SauverFichierFen.FileName)} !",
                                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors de la sauvegarde : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void BoxNomJoueurBlanc_TextChanged(object sender, EventArgs e)
        {   // --- Gestion du changement de nom du joueur blanc ---
            NomJoueurBlanc = PartieCourante.White = BoxNomJoueurBlanc.Text;
        }
        private void BoxNomJoueurNoir_TextChanged(object sender, EventArgs e)
        {   //  --- Gestion du changement de nom du joueur noir ---
            NomJoueurNoir = PartieCourante.Black = BoxNomJoueurNoir.Text;
        }
        private void trackBarTempsReflexion_ValueChanged(object sender, EventArgs e)
        {   // --- Gestion du trackBar pour le temps de réflexion de l'ordinateur ---
            LabelTempsReflexion.Text = trackBarTempsReflexion.Value.ToString() + " sec.";
            TempsReflexion = trackBarTempsReflexion.Value;
            Console.WriteLine($"Temps de réflexion = {TempsReflexion} secondes");
        }
        private void OrdinateurBosse_CheckedChanged(object sender, EventArgs e)
        {   // Bascule la valeur de PartieEnCours
            PartieEnCours = !PartieEnCours;
        }
        private void SonEmis_CheckedChanged(object sender, EventArgs e)
        {   // Bascule la valeur de EmetUnson
            EmetUnSon = !EmetUnSon;
        }
        private void RadioGainBlanc_CheckedChanged(object sender, EventArgs e)
        {   // --- Gestion du bouton radio gain blanc ---
            if (RadioGainBlanc.Checked)
            {   // 1-0 les blancs gagnent
                LabelInformationJoueur.Text = "Gain Blanc sélectionné (Partie terminée)";
                MiseaJourFinPartie(" 1-0");
            }
        }
        private void RadioGainNoir_CheckedChanged(object sender, EventArgs e)
        {   // --- Gestion du bouton radio gain noir ---
            if (RadioGainNoir.Checked)
            {   // 0-1 les noirs gagnent
                LabelInformationJoueur.Text = "Gain Noir sélectionné (Partie terminée)";
                MiseaJourFinPartie(" 0-1");
            }
        }
        private void RadioNulle_CheckedChanged(object sender, EventArgs e)
        {   // --- Gestion du bouton radio partie nulle ---
            if (RadioNulle.Checked)
            {   // 1/2-1/2 partie nulle
                LabelInformationJoueur.Text = "Nulle sélectionnée (Partie terminée)";
                MiseaJourFinPartie(" 1/2-1/2");
            }
        }
        private void MiseaJourFinPartie(string resultat)
        {   // --- Met à jour certains éléments à la fin de la partie ---
            ListeCoupsPdn.Add(resultat);        // Ajoute le résultat à la liste des coups
            PartieCourante.Event = "Entrainement";
            PartieCourante.Site = "Interface graphique";
            PartieCourante.Date = DateTime.Now.ToString("yyyy-MM-dd");      // Stocke la date au format AAAA-MM-JJ
            PartieCourante.White = NomJoueurBlanc;      // Mise à jour des balises de la partie
            PartieCourante.Black = NomJoueurNoir;
            PartieCourante.Result = resultat.Trim(); 
            GroupResultat.Enabled = false;   // Désactive le groupe de boutons
            PartieTerminee = FinPartie = true;
        }
        private void BoutonRetourArriere_Click(object sender, EventArgs e)
        {   // --- Retour en arrière d'un coup en récupérant l'avant-dernier FEN et en effaçant le dernier ---
            if (ListeCoupsPdn.Count == 0 || ListeCoupsFen.Count < 2 || ListeCoupsHub2.Count < 2)
            {
                MessageBox.Show("Impossible de revenir en arrière :\nIl n'y a pas assez d'éléments dans les listes.",
                                "Retour arrière", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            GestionDamier.ViderDamier();
            int dernierIndex = ListeCoupsPdn.Count - 1;
            string coupEfface = ListeCoupsPdn[dernierIndex]; // Récupère le dernier coup
            if (coupEfface == " 1-0" || coupEfface == " 0-1" || coupEfface == " 1/2-1/2")
            {   // Vérifie si le dernier élément est un résultat et le supprime si nécessaire
                ListeCoupsPdn.RemoveAt(dernierIndex);
                dernierIndex--; // Met à jour l'index du dernier élément
                if (dernierIndex < 0) // Vérifie qu'il reste encore un coup après suppression
                {
                    MessageBox.Show("Impossible de revenir en arrière :\nAucun coup valide après suppression du résultat.",
                                    "Retour arrière", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            MiseenplaceFen(ListeCoupsFen[ListeCoupsFen.Count - 2], false);       // Met en place la position précédente
            AfficheListesPieces();
            ListeCoupsPdn.RemoveAt(dernierIndex);   // Supprime les derniers éléments des listes
            ListeCoupsFen.RemoveAt(ListeCoupsFen.Count - 1);
            indexFenCoupActuel--;   // Décrémente l'index du coup actuel
            ListeCoupsHub2.RemoveAt(ListeCoupsHub2.Count - 1);
            NettoyageRapide();
            LabelCoupJoue.Text = $"Mouvement effacé = {coupEfface}"; // Met à jour le label avec le mouvement.
            AnalyseCoupObligatoire(CouleurAuTrait);
            PartieEnCours = false;
        }
        private void BoutonTournerDamier_Click(object sender, EventArgs e)
        {   // --- Rotation du damier de 180° incluant les indices ---
            PictureBox[] nouveauCaseDamier = new PictureBox[100];           // Nouveau tableau temporaire pour les PictureBox
            // Mettre à jour les positions et indices
            for (int i = 0; i < CaseDamier.Count; i++)
            {
                // Obtenir la PictureBox actuelle
                PictureBox pict = CaseDamier[i];

                int ligneActuelle = i / 10;                                 // Calculer les coordonnées ligne/colonne actuelles
                int colonneActuelle = i % 10;
                int nouvelleLigne = 9 - ligneActuelle;                      // Calculer les nouvelles coordonnées après rotation
                int nouvelleColonne = 9 - colonneActuelle;
                int nouvelIndex = nouvelleLigne * 10 + nouvelleColonne;     // Calculer le nouvel index après rotation

                // Positionner graphiquement la PictureBox
                Point nouvellePosition = new Point(20 + (nouvelleColonne * 60), 20 + (nouvelleLigne * 60));
                pict.Location = nouvellePosition;
                // Mettre à jour la liste temporaire
                nouveauCaseDamier[nouvelIndex] = pict;
            }
            // Copier les nouvelles valeurs dans la liste existante
            for (int i = 0; i < CaseDamier.Count; i++)
            {
                CaseDamier[i] = nouveauCaseDamier[i];
            }
            // Mettre à jour les indices Manoury et PictureBox
            Dictionary<int, int> nouveauPictureBoxVersManoury = new Dictionary<int, int>();
            for (int manoury = 1; manoury <= 50; manoury++)
            {   // Vérifier la valeur actuelle de l'index
                int indexActuel = GestionDamier.ManouryVersIndexPictureBox[manoury - 1];
                // Calculer le nouvel index
                int nouvelIndex = 99 - indexActuel;
                // Mettre à jour les correspondances
                GestionDamier.ManouryVersIndexPictureBox[manoury - 1] = nouvelIndex;
                // Ajout au nouveau dictionnaire
                if (!nouveauPictureBoxVersManoury.ContainsKey(nouvelIndex))
                {
                    nouveauPictureBoxVersManoury[nouvelIndex] = manoury;
                }
                else
                {
                    Debug.WriteLine($"Conflit détecté : nouvelIndex {nouvelIndex} déjà attribué !");
                }
            }
            // Mise à jour finale du dictionnaire
            GestionDamier.PictureBoxVersManoury = nouveauPictureBoxVersManoury;

            // Mettre à jour le tableau DamierContenu
            ContenuCase[,] nouveauDamierContenu = new ContenuCase[10, 10];
            for (int ligne = 0; ligne < 10; ligne++)
            {
                for (int colonne = 0; colonne < 10; colonne++)
                {
                    int nouvelleLigne = 9 - ligne;
                    int nouvelleColonne = 9 - colonne;

                    nouveauDamierContenu[nouvelleLigne, nouvelleColonne] = GestionDamier.DamierContenu[ligne, colonne];
                }
            }
            GestionDamier.DamierContenu = nouveauDamierContenu;
            // Actualiser l'affichage
            Damier10x10.Refresh();
            VisuCoteNoir = !VisuCoteNoir;   // On inverse le flag de côté de visualisation
        }
        private void BoutonQuitterApplication_Click(object sender, EventArgs e)
        {   // --- Ferme et quitte l'appliccation proprement ---
            Close();        // OnFormClosing arrête le moteur
        }
    }
}
