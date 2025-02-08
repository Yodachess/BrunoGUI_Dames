// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames est développé par Bruno COURTOIS.  Copyright © 2024/2025  █  
// █ BrunoGUI_Dames est gratuit, sauf s'il est utilisé commercialement        █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

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
using static BrunoGUI_Dames.LogiqueMouvementsDames;

namespace BrunoGUI_Dames
{
    public partial class BrunoInterfaceGraphiqueDames : Form
    {
        public static Dictionary<int, PictureBox> ManouryVersPictureBoxIndicee = new Dictionary<int, PictureBox>();
        // Les listes
        public static readonly List<PictureBox> PictJeux = new List<PictureBox>();      // les 100 cases du jeu
        private readonly List<int> IndiceVisuCoteNoir = new List<int>();                // liste des indices si les noirs sont en bas de l'écran
        private readonly List<Color> CouleurCaseOrigines = new List<Color>();           // Couleurs d'origine des cases
        public static readonly List<int> DeplacementsSimplesPossibles = new List<int>();
        // Une rafle est une liste de prises, donc on a une liste de listes
        public static readonly List<List<(int casePrise, int caseArrivee)>> RaflesPossibles = new List<List<(int casePrise, int caseArrivee)>>();
        public static readonly Dictionary<int, List<List<(int casePrise, int caseArrivee, int caseDepart)>>> RaflesMaximales =
                                                            new Dictionary<int, List<List<(int casePrise, int caseArrivee, int caseDepart)>>>();
        public static readonly List<int> ListeCasesPivot = new List<int>();
        public static readonly List<int> ListeCasesCoupSelectionne = new List<int>();
        public static readonly List<int> ListeCasesMouvementPrecedent = new List<int>();
        public static readonly List<int> ListeCasesDestination = new List<int>();
        // liste des Bitmaps pour les pièces
        public static readonly Dictionary<LogiqueMouvementsDames.TypePiece, Bitmap> ListeBitmapsPiece = new Dictionary<LogiqueMouvementsDames.TypePiece, Bitmap>();
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
        public static Color CouleurCaseDepart = Color.LightGreen;
        public static Color CouleurTrajetSuivi = Color.DarkKhaki;
        public static Color CouleurCasePivot = Color.Orange;
        public static Color CouleurCasePrise = Color.OrangeRed;                          // Color.DarkSeaGreen;
        public static Color CouleurCaseArrivee = Color.LightGreen;
        public static string CouleurPieceCliquee, CouleurAuTrait;
        public static string NomJoueurBlanc = "Bruno" , NomJoueurNoir = "SCAN 3.1";
        public static int IndexPictureBoxSource100, CaseSourceManoury, CaseDestinationManoury, DureeAnimation, TempsReflexion;
        public static bool VisuCoteNoir;          // True quand les Noirs sont en bas de l'écran
        public static bool AfficheNumeroBox, AfficheCaseLogique, PartieEnCours, PartieTerminee, FinPartie, EmetUnSon;
        private static bool ClickCaseSource, MontreDonneesDames, JeuMoteurEnCours;
        private CaseDamier PieceCouleurSource;
        private string[] DonneesMoteurScan;        // Données en provenance du Moteur SCAN
        private string CheminMoteur;
        private int indexFenCoupActuel = 0; // Indice du coup affiché
        // Définir les indices des dernières rangées en notation Manoury
        private static readonly int[] derniereRangeeBlanche = { 46, 47, 48, 49, 50 };
        private static readonly int[] derniereRangeeNoire = { 1, 2, 3, 4, 5 };
        // -- les classes --
        private readonly DonneesBrutesDames donneesBrutesDames = new DonneesBrutesDames();
        private AffichePdn affichePdn = new AffichePdn();   // affichePdn est à la fois déclarée et instanciée. Prêt à être utilisé dès le début
        // GestionDamier et CaseDamier dans GestionDamier.cs
        // LogiqueMouvementsDames dans LogiqueMouvementsDames.cs

        public BrunoInterfaceGraphiqueDames()
        {
            InitializeComponent();
        }
        private void BrunoInterfaceGraphiqueDames_Load(object sender, EventArgs e)
        {
            // les évènements dans les classes
            MoteurDamesScan.AfficheScan += AfficheScan;
            MoteurDamesScan.AfficheCoupMoteur += AfficheCoupMoteur;
            MoteurDamesScan.AfficheDonneesBrutes += AfficheDonneesBrutes;
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
            AfficheNumeroBox = false;
            AfficheCaseLogique = ClickCaseSource = true;
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
            // MiseenplaceFen("[FEN \"W:W34,40,45:B13,23,25\"]", true);   // Les blancs jouent et gagnent
            // MiseenplaceFen("[FEN \"W:W26,31,36:B8,16,18\"]", true);    // Les blancs jouent et gagnent
            // MiseenplaceFen("[FEN \"W:WK4,K36,K37:BK29\"]", true);      // Les blancs jouent et gagnent -- 37-42 (29 x 47) 4 - 15 gagne --
            // MiseenplaceFen("[FEN \"W:WK30,K40,K44:BK16\"]", true);     // Les blancs jouent et gagnent -- 30 - 43 (16 x 49) 40 -35 (49 x 40) 35 x 44 gagne --
            // MiseenplaceFen("[FEN \"B:W18,14,21,23,24,26,31,32,34:B1,2,3,4,6,7,9,10,11,12,30,40\"]", true);
            // MiseenplaceFen("[FEN \"W:W27,28,32,33,37,38,39,40,45:B3,6,12,13,14,18,19,23,24\"]", true);      // Coup Royal 1. 27-22 !
            // MiseenplaceFen("[FEN \"B:W35,42,45,46,47,48,49,50,25,29,38,39,27:B3,4,5,6,9,10,14,12,7,18,13,17\"]", true);   // 18-22 !!
            // MiseenplaceFen("[FEN \"B:W18,14,21,23,34:B1,2,3,4,6,7,9\"]", true);
            // MiseenplaceFen("[FEN \"B:W18,24,27,28,36,37,38,39,40,K10,K15:B12,16,20,K22,K25,K29\"]", true);
            // MiseenplaceFen("[FEN \"W:W17,23,33,38,40:B6,8,9,10,20,K30\"]", true);     // Les blancs jouent et gagnent -- 40-34 (30x43x21x12x29) 33x2 B+
            // MiseenplaceFen("[FEN \"B:W13,14,18,23,29,30,38,39:BK35\"]", true);  // (Coup Turc) -- Les Noirs doivent prendre 4 pions blancs par la rafle(35 x 19 x 32 x 43 x 34).
            // MiseenplaceFen("[FEN \"W:W31-50:B1-20\"]", true);
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
            CheminMoteur = Path.Combine(Directory.GetCurrentDirectory(), "Dames_Scan31_FabienLetouzey", "scan.exe");
            // Instanciation et démarrage du moteur
            var moteurDamesScan = new MoteurDamesScan();
            moteurDamesScan.Start(CheminMoteur);
            string hub2 =  LogiqueMouvementsDames.ConvertitFenVersHub2(LogiqueMouvementsDames.ListeCoupsFen[0]);
            AnalyseCoupObligatoire(CouleurAuTrait);
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion du click de la souris
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private async void CaseMouseDown(object sender, MouseEventArgs e)
        {   // --- Gestion de la sélection/déplacement des pièces avec retour à l'origine si mouvement incorrect ---
            try
            {   // Le joueur sélectionne la case source ou destination avec la souris
                if (sender is PictureBox CaseClick)     // Une PictureBox est cliquée
                {
                    int indexCase100 = Convert.ToInt32(CaseClick.Name.Substring(8)); // Utilise le numéro de la PictureBox comme index
                    if (VisuCoteNoir)
                    {   // Si on regarde côté noir, il faut inverser l'index par rapport a la vue côté blanc
                        indexCase100 = IndiceVisuCoteNoir[indexCase100];    
                    }
                    if (ClickCaseSource)     // Permet de savoir si c'est la sélection de la pièce ou le déplacement
                    {   // --- Sélection d'une pièce ---
                        CouleurToutesCasesManoury(CouleurCasesombre);       // On réinitialise les couleurs des cases
                        IndexPictureBoxSource100 = indexCase100;
                        CaseSourceManoury = GestionDamier.ObtenirCaseManoury(IndexPictureBoxSource100);     // Quelle case Manoury est cliquée ?
                        (int ligne, int colonne) = GestionDamier.IndiceVersCoordonnees(IndexPictureBoxSource100);
                        PieceCouleurSource = GestionDamier.DamierContenu[ligne, colonne];                   // Quelle pièce est cliquée  ?
                        CouleurPieceCliquee = PieceCouleurSource.CouleurPiece.ToString();                   // Quelle couleur de pièce ?
                        if (PictJeux[indexCase100].Image != null)
                        {   // Si la case cliquée contient bien une pièce ou un pion, on va utiliser le thumbnail de la pièce comme curseur :-)
                            using (Bitmap Piece = new Bitmap(PictJeux[indexCase100].Image))
                            {   // Quand on bouge la souris, on bouge le thumbnail de la pièce comme un curseur :-)
                                Bitmap thumbnail = (Bitmap)Piece.GetThumbnailImage(150, 150, null, IntPtr.Zero);
                                Cursor = new Cursor(thumbnail.GetHicon());
                            }
                            for (int i = 0; i < ListeCasesCoupSelectionne.Count; i++)    // On enlève tous les mouvements affichés précédemment
                            {   // On remet la couleur de base sur les cases
                                PictJeux[GestionDamier.ManouryVersIndexPictureBox[ListeCasesCoupSelectionne[i] - 1]].BackColor = BrunoInterfaceGraphiqueDames.CouleurCasesombre;
                            }
                            ListeCasesCoupSelectionne.Clear();
                            EffacePiece(indexCase100, false);       // On efface la case d'origine sans le faire dans DamierContenu ...
                            PictJeux[indexCase100].BackColor = CouleurTrajetSuivi;
                            DessineMouvements(CaseSourceManoury);   // On montre les mouvements possibles
                            // Crée une liste de chaînes de caractères représentant chaque rafle
                            var raflesText = RaflesPossibles.Select(rafle => string.Join(", ", rafle.Select(r => $"{r.casePrise} -> {r.caseArrivee}"))).ToList();
                            // Détermine le nombre d'éléments raflés par rafle (toutes les rafles ont la même taille)
                            int elementsParRafle = RaflesPossibles.FirstOrDefault()?.Count ?? 0; // Prend la taille de la première rafle ou 0 si la liste est vide
                            // Combine toutes les chaînes en une seule avec un saut de ligne entre chaque liste
                            LabelPrisesPossibles.Text = RaflesPossibles.Count + " Rafles possibles de " + elementsParRafle + " pièces\n" + string.Join("\n", raflesText);
                            ClickCaseSource = false;
                        }
                        else
                        {
                            Console.WriteLine($"Passage sur une PictureBox = null : {PictJeux[indexCase100].Image}");
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
                                if (RaflesPossibles != null && RaflesPossibles.Any())   // Il y a au moins une rafle possible
                                {   // Trouver une rafle qui se termine par la CaseDestinationManoury
                                    var rafleCorrespondante = RaflesPossibles
                                        .FirstOrDefault(rafle => rafle.LastOrDefault().caseArrivee == CaseDestinationManoury);
                                    if (rafleCorrespondante != null)
                                    {   // Il y a une rafle qui se termine par la CaseDestinationManoury
                                        var indexSourceBox = GestionDamier.ManouryVersIndexPictureBox[CaseSourceManoury - 1];   // Index PictureBox initial
                                        (int ligne, int colonne) = GestionDamier.IndiceVersCoordonnees(indexSourceBox);
                                        CaseDamier pieceSource = GestionDamier.DamierContenu[ligne, colonne];
                                        await AnimerEtExecuterRafle(indexSourceBox, rafleCorrespondante, pieceSource);      // On exécute le déplacement
                                        ChangerCouleurTrait();
                                        string fenPosition = LogiqueMouvementsDames.RecupereFEN();                      // On met à jour les listes de coups
                                        LogiqueMouvementsDames.ListeCoupsFen.Add(fenPosition);                          // pour les FEN et Hub2, pour les Pdn
                                        LogiqueMouvementsDames.ListeCoupsHub2.Add(ConvertitFenVersHub2(fenPosition));   // c'est fait dans AnimerEtExecuterRafle
                                        indexFenCoupActuel = ListeCoupsFen.Count - 1; // Toujours pointer sur le dernier coup
                                    }
                                }
                                else if (DeplacementsSimplesPossibles != null && DeplacementsSimplesPossibles.Any()) // Il y a des déplacements simples possibles
                                {   // Pas de rafles possibles mais des déplacements sont possibles
                                    if (DeplacementsSimplesPossibles.Contains(CaseDestinationManoury))
                                    {   // Pas de rafle, donc on execute un déplacement simple
                                        ExecuteDeplacement(
                                            indexSourceBox: IndexPictureBoxSource100,
                                            destinationManoury: CaseDestinationManoury,
                                            pieceSource: PieceCouleurSource,
                                            piecePrise: false, 0
                                        );
                                        ChangerCouleurTrait();
                                        string fenPosition = LogiqueMouvementsDames.RecupereFEN();                      // On met à jour les listes de coups
                                        LogiqueMouvementsDames.ListeCoupsFen.Add(fenPosition);                          // pour les FEN et Hub2, pour les Pdn
                                        LogiqueMouvementsDames.ListeCoupsHub2.Add(ConvertitFenVersHub2(fenPosition));   // c'est fait dans AnimerEtExecuterRafle
                                        indexFenCoupActuel = ListeCoupsFen.Count - 1; // Toujours pointer sur le dernier coup
                                    }
                                    else
                                    {   // Si le coup n'est pas valide, remet la pièce sur sa case d'origine
                                        DessinePiece(IndexPictureBoxSource100, PieceCouleurSource.TypePiece, PieceCouleurSource.CouleurPiece);
                                        PictJeux[IndexPictureBoxSource100].BackColor = CouleurTrajetSuivi;
                                        Console.WriteLine($"La case {CaseDestinationManoury} n'est pas dans les déplacements simples possibles.");
                                    }
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
                                DessinePiece(IndexPictureBoxSource100, PieceCouleurSource.TypePiece, PieceCouleurSource.CouleurPiece);
                                PictJeux[IndexPictureBoxSource100].BackColor = CouleurCasesombre;
                            }
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {   // Une case non-Manoury a été cliquée comme destination, on redessine la pièce sur sa case d'origine
                            Console.WriteLine($"Erreur : {ex.Message}");
                            DessinePiece(IndexPictureBoxSource100, PieceCouleurSource.TypePiece, PieceCouleurSource.CouleurPiece);
                            PictJeux[IndexPictureBoxSource100].BackColor = CouleurCasesombre;
                        }
                        finally
                        {
                            // Cursor = Cursors.Default;       // On revient au curseur "normal"
                            ClickCaseSource = true;
                            InitialiserEtatMouvement();
                            Console.WriteLine($"Couleur au trait = {CouleurAuTrait}");
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

        private async Task LancementReflexionMoteur(string hub2, int tempsReflexion)
        {   // --- Laancement du moteur SCAN pour réfléchir à un coup sur la position "hub2" pendant "tempsReflexion" ---
            JeuMoteurEnCours = true;
            ActiveCouleurDamier(CouleurAuTrait, false);
            LabelInformationJoueur.Text = "Trait aux " + CouleurPasAuTrait() + "s, le moteur réfléchit ...";
            LabelCoupJoue.Text = "";
            MoteurDamesScan.CoupScanJoue = false;
            MoteurDamesScan.StandardInputDataToScan("pos pos=" + hub2);     // On envoie la position au format hub2
            MoteurDamesScan.StandardInputDataToScan("level move-time=" + tempsReflexion);   // x second pour le coup
            MoteurDamesScan.StandardInputDataToScan("go think");            // Au moteur de bosser .... :-)
            while (!MoteurDamesScan.CoupScanJoue)   // On boucle jusqu'à que le coup soit joué
            {
                await Task.Delay(100);  // Attendre un peu avant de vérifier à nouveau
            }
            CouleurToutesCasesManoury(CouleurCasesombre);
            LabelCoupJoue.Text = "Joué : " + MoteurDamesScan.CoupScan + " --  Suggéré : " + MoteurDamesScan.SuggestionScan;
            if (EmetUnSon)
            {
                SoundPlayer player = new SoundPlayer(@"C:\Windows\Media\Windows Notify.wav");       // Son pour dire que le coup est joué
                player.Play();
            }
            
            if (MoteurDamesScan.CoupScan.Contains("x"))                 ///////// Rafle détectée /////////
            {   // Découper le coup en une séquence de prises
                string[] positions = MoteurDamesScan.CoupScan.Split('x');
                int caseSource = int.Parse(positions[0]);   // Case source de la rafle (Manoury)
                int caseArriveeFinale = int.Parse(positions[1]);  // Case arrivée finale (!) de la rafle (Manoury)

                AnalyseCoupObligatoireUnique(caseSource);
                // Sélectionne toutes les rafles qui se terminent par caseArriveeFinale
                var raflesFiltrees = RaflesMaximales
                    .SelectMany(kvp => kvp.Value) // Prend toutes les rafles dans le dictionnaire
                    .Where(rafleMax => rafleMax.Any() && rafleMax.Last().caseArrivee == caseArriveeFinale) // Vérifie la dernière case d'arrivée
                    .ToList();
                // DEUBG
                // Affichage des rafles trouvées
                Console.WriteLine($"Rafles qui se terminent par {caseArriveeFinale} :");
                foreach (var rafleMax in raflesFiltrees)
                {
                    Console.WriteLine("   " + string.Join(" -> ", rafleMax.Select(m => $"{m.caseDepart}->{m.casePrise}x{m.caseArrivee}")));
                }
                // DEBUG
                // Récupére la première rafle disponible et enleve caseDepart
                var rafleAnimation = RaflesMaximales.Values
                    .SelectMany(liste => liste)  // Récupère toutes les rafles
                    .Select(rafle => rafle.Select(m => (m.casePrise, m.caseArrivee)).ToList()) // Supprime caseDepart
                    .FirstOrDefault();  // Prend la première rafle ou null si aucune
                if (rafleAnimation == null || rafleAnimation.Count == 0)
                {
                    Console.WriteLine("Aucune rafle trouvée !");
                    return;
                }
                // Appel à la méthode pour exécuter la rafle
                int indexSourceBox = GestionDamier.ObtenirIndicePictureBox(caseSource);
                PictJeux[indexSourceBox].BackColor = CouleurTrajetSuivi;
                await AnimerEtExecuterRafle(indexSourceBox, rafleAnimation, GestionDamier.CaseDamierCaseManoury(caseSource));
            }

            else if (MoteurDamesScan.CoupScan.Contains("-"))            ///////// Coup simple détecté /////////
            {   // Découper le coup en source et destination
                string[] positions = MoteurDamesScan.CoupScan.Split('-');
                int caseSource = int.Parse(positions[0]);       // Case de départ (Manoury)
                int caseDestination = int.Parse(positions[1]);  // Case d'arrivée (Manoury)
                int indexSourceBox = GestionDamier.ObtenirIndicePictureBox(caseSource);
                // Appel à la méthode pour exécuter le coup simple
                PictJeux[indexSourceBox].BackColor = CouleurTrajetSuivi;
                ExecuteDeplacement(indexSourceBox, caseDestination, GestionDamier.CaseDamierCaseManoury(caseSource), false, 0);
            }
            else
            {
                Console.WriteLine($"Format de coup non valide : {MoteurDamesScan.CoupScan}");
            }
            string fenPosition = LogiqueMouvementsDames.RecupereFEN();                      // On met à jour les listes de coups
            LogiqueMouvementsDames.ListeCoupsFen.Add(fenPosition);                          // pour les FEN et Hub2, pour les Pdn
            LogiqueMouvementsDames.ListeCoupsHub2.Add(ConvertitFenVersHub2(fenPosition));   // c'est fait dans AnimerEtExecuterRafle
            indexFenCoupActuel = ListeCoupsFen.Count - 1;   // Toujours pointer sur le dernier coup
            LabelPrisesPossibles.Text = "";
            PartieEnCours = true;
            JeuMoteurEnCours = false;
            LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s";
            ActiveCouleurDamier(CouleurAuTrait, true);
        }
        private void AnalyseCoupObligatoireUnique(int caseManoury)
        {   // --- Analyse pour la pièce en caseManoury des coups obligatoires (prise la + longue) pour la couleur au trait ---
            ActiveDamier(false);  // Désactiver les interactions
            RaflesPossibles.Clear();
            RaflesMaximales.Clear();
            CouleurAuTrait = CouleurPasAuTrait();           // Obligé sinon on ne peut pas trouver les rafles de la bonne couleur ??
            TrouverMouvementsPossibles(caseManoury);        // Trouver les mouvements possibles pour la pièce sélectionnée
            CouleurAuTrait = CouleurPasAuTrait();           // On revient à l'état initial
            if (!RaflesPossibles.Any())  // Si aucune prise n'est trouvée
            {
                ActiveDamier(true);
                ActiveCouleurDamier(CouleurAuTrait, true);
                ActiveCouleurDamier(CouleurPasAuTrait(), false);
                return;
            }
            // Ajouter les rafles en incluant la case de départ
            RaflesMaximales[caseManoury] = RaflesPossibles.Select(rafle => rafle.Select(t => (t.casePrise, t.caseArrivee, caseManoury)).ToList()).ToList();
            // Trouver le nombre maximal de prises
            int maxPrises = RaflesMaximales[caseManoury].Max(r => r.Count);
            // Filtrer pour ne garder que les rafles maximales
            RaflesMaximales[caseManoury] = RaflesMaximales[caseManoury].Where(r => r.Count == maxPrises).ToList();
            // Affichage des rafles maximales
            var casesActivables = new HashSet<int> { caseManoury }; // Toujours activer la case de départ
            foreach (var rafleMaximale in RaflesMaximales[caseManoury])
            {
                int caseArriveeFinale = rafleMaximale.Last().caseArrivee;
                casesActivables.Add(caseArriveeFinale);
                foreach (var mouvement in rafleMaximale)
                {
                    PictJeux[GestionDamier.ManouryVersIndexPictureBox[mouvement.caseArrivee - 1]].BackColor = Color.LightGreen;
                }
            }
            LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s (prise obligatoire !)";
            LabelPrisesPossibles.Text = RaflesMaximales[caseManoury].Count + " Rafle(s) possible(s) de " + maxPrises + " pièce(s)\n" +
                                        string.Join("\n", RaflesMaximales[caseManoury].Select(rafle =>
                                            string.Join(", ", rafle.Select(r => $"{r.casePrise} -> {r.caseArrivee}"))));
        }
        private void AnalyseCoupObligatoire(string couleur)
        {   // --- Analyse pour toutes les pièces des coups obligatoires (prise la + longue) pour la couleur donnée ---
            ActiveDamier(false);            // Désactiver les interactions pendant l'analyse
            // Obtenir la liste des pièces en fonction de la couleur
            List<int> listePieces = (couleur == "Blanc" ? ListePionsBlancs.Concat(ListeDamesBlanches)
                                                        : ListePionsNoirs.Concat(ListeDamesNoires)).ToList();
            RaflesPossibles.Clear(); // Réinitialiser les rafles possibles
            RaflesMaximales.Clear(); // Réinitialiser les rafles maximales

            var raflesAvecDepart = new List<List<(int casePrise, int caseArrivee, int caseDepart)>>();
            foreach (int casePiece in listePieces)          // Générer les rafles possibles pour chaque pièce
            {   
                TrouverMouvementsPossibles(casePiece);      // Obtenir les mouvements possibles pour cette pièce
                foreach (var rafle in RaflesPossibles)      // Ajouter les rafles possibles en y incluant la case de départ
                {   // Ajouter `caseDepart` à chaque tuple de la rafle
                    var rafleAvecDepart = rafle.Select(t => (t.casePrise, t.caseArrivee, casePiece)).ToList();
                    raflesAvecDepart.Add(rafleAvecDepart);
                }
            }
            if (!raflesAvecDepart.Any())        // S'il n'y a pas de rafles possibles, on réactive toutes les cases
            {
                // Console.WriteLine("Aucune rafle possible. Réactivation de CouleurAuTrait.");
                ActiveDamier(true);     // Réactiver toutes les cases, y compris les cases vides
                ActiveCouleurDamier(CouleurAuTrait, true);          // Réactiver les cases de la couleur au trait
                ActiveCouleurDamier(CouleurPasAuTrait(), false);    // Désactiver les cases de la couleur pas au trait
                return;             // Sortir de la méthode, rien à faire de plus
            } 
            // Regrouper les rafles par case de départ
            foreach (var rafle in raflesAvecDepart)
            {
                if (rafle.Count == 0) continue;     // Pas de rafle, on sort ...

                int caseDepart = rafle[0].caseDepart;
                if (!RaflesMaximales.ContainsKey(caseDepart))       // Ajouter la rafle à la liste pour cette case de départ
                {
                    RaflesMaximales[caseDepart] = new List<List<(int casePrise, int caseArrivee, int caseDepart)>>();
                }
                RaflesMaximales[caseDepart].Add(rafle);
            }
            // Trouver le nombre maximum de prises sur toutes les rafles
            int maxPrises = RaflesMaximales.Values.SelectMany(r => r).Max(r => r.Count);
            // Filtrer pour ne conserver que les rafles maximales et collecter les cases activables
            var casesActivables = new HashSet<int>(); // Ensemble des cases à activer (départ et arrivée)
            foreach (var caseDepart in RaflesMaximales.Keys.ToList())
            {
                var rafles = RaflesMaximales[caseDepart];
                RaflesMaximales[caseDepart] = rafles.Where(r => r.Count == maxPrises).ToList();
                foreach (var rafleMaximale in RaflesMaximales[caseDepart])
                {   // Ajouter la case de départ et la case d'arrivée des rafles maximales
                    int caseArriveeFinale = rafleMaximale.Last().caseArrivee; // Dernière case d'arrivée de la rafle
                    casesActivables.Add(caseDepart);
                    casesActivables.Add(caseArriveeFinale);     // Ajouter uniquement la case d'arrivée finale
                    // Colorier les cases  de départ et d'arrivée des mouvements de la rafle maximale
                    CouleurToutesCasesManoury(CouleurCasesombre);       // On réinitialise les couleurs des cases
                    foreach (var mouvement in rafleMaximale)
                    {
                        PictJeux[GestionDamier.ManouryVersIndexPictureBox[mouvement.caseDepart - 1]].BackColor = CouleurCaseDepart;
                        PictJeux[GestionDamier.ManouryVersIndexPictureBox[mouvement.caseArrivee - 1]].BackColor = CouleurCaseArrivee;   // Colorier chaque case d'arrivée 
                        PictJeux[GestionDamier.ManouryVersIndexPictureBox[mouvement.caseArrivee - 1]].Image = CadreGrisSansPrise;       // avec le cadre gris    
                        ListeCasesDestination.Add(mouvement.caseArrivee);    // Ajouter la case d'arrivée à la liste des cases de destination
                        Console.WriteLine($"Liste Cases Destination : {string.Join(", ", ListeCasesDestination)}");
                    }
                }
            }
            foreach (int caseActivable in casesActivables)      // Parcourir toutes les cases activables
            {
                ActiverCaseManoury(caseActivable, true);        // Activer directement chaque case activable
                LabelInformationJoueur.Text = "Trait aux " + couleur + "s (prise obligatoire !)";
            }
            // Crée une liste de chaînes de caractères représentant chaque rafle
            var raflesText = RaflesMaximales
                .SelectMany(kv => kv.Value) // Parcourt toutes les listes de rafles pour chaque clé
                .Select(rafle => string.Join(", ", rafle.Select(r => $"{r.casePrise} -> {r.caseArrivee}"))) // Formate chaque rafle
                .ToList();
            // Détermine le nombre d'éléments raflés par rafle (toutes les rafles ont la même taille)
            int elementsParRafle = RaflesMaximales.Values
                .SelectMany(listeDeRafles => listeDeRafles)
                .FirstOrDefault()?.Count ?? 0; // Prend la taille de la première rafle ou 0 si la liste est vide
                                               // Combine toutes les chaînes en une seule avec un saut de ligne entre chaque liste
            LabelPrisesPossibles.Text = raflesText.Count + " Rafle(s) possible(s) de " + elementsParRafle + " pièce(s) \n" + string.Join("\n", raflesText);
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Mouvements  divers
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        public void ExecuteDeplacement(int indexSourceBox, int destinationManoury, CaseDamier pieceSource, bool piecePrise, int casePrise = 0)
        {   // --- Execute un déplacement simple en vérifiant la promotion ---
            ActiveDamier(false); // Désactiver les interactions pendant le déplacement
            EffaceCasesDestiantion();   // Efface les cadres gris des cases de destination générées lors de l'AnalyseCoupoObligatoire
            var couleurPiece = pieceSource.CouleurPiece;
            ListeCasesCoupSelectionne.Add(GestionDamier.PictureBoxVersManoury[indexSourceBox]);     // Ajoute la case source au suivi des coups
            if (piecePrise && casePrise > 0)
            {   // Efface la pièce prise
                EffacePiece(GestionDamier.ManouryVersIndexPictureBox[casePrise - 1], true);
                PictJeux[GestionDamier.ManouryVersIndexPictureBox[casePrise - 1]].BackColor = CouleurTrajetSuivi;
                ListeCasesCoupSelectionne.Add(casePrise); // Ajoute la case prise
            }
            // Vérification si promotion en Dame : IL FAUDRAIT S'ASSURER QUE LA PIECE N'EST PAS DEJA UNE DAME !!!!!!!!!!!!!!!!!!!
            if (derniereRangeeBlanche.Contains(destinationManoury) && pieceSource.CouleurPiece == LogiqueMouvementsDames.CouleurPiece.Noir)
            {
                MiseaJourPromotion(indexSourceBox, pieceSource);    // On met à jour la liste de pions avant de faire la promotion ...
                pieceSource.TypePiece = LogiqueMouvementsDames.TypePiece.DameNoire; // Promotion en Dame noire
            }
            else if (derniereRangeeNoire.Contains(destinationManoury) && pieceSource.CouleurPiece == LogiqueMouvementsDames.CouleurPiece.Blanc)
            {
                MiseaJourPromotion(indexSourceBox, pieceSource);    // On met à jour la liste de pions avant de faire la promotion ...
                pieceSource.TypePiece = LogiqueMouvementsDames.TypePiece.DameBlanche; // Promotion en Dame blanche
            }
            DessinePiece(GestionDamier.ManouryVersIndexPictureBox[destinationManoury - 1], pieceSource.TypePiece, pieceSource.CouleurPiece);
            PictJeux[GestionDamier.ManouryVersIndexPictureBox[destinationManoury - 1]].BackColor = CouleurCaseArrivee;
            EffacePiece(indexSourceBox, true);                  // Efface la pièce source
            ListeCasesCoupSelectionne.Add(destinationManoury);  // Ajoute la destinationManoury au suivi des coups
            string coupEffectue = piecePrise
                ? $"{GestionDamier.PictureBoxVersManoury[indexSourceBox]}x{destinationManoury}"
                : $"{GestionDamier.PictureBoxVersManoury[indexSourceBox]}-{destinationManoury}";
            ListeCoupsPdn.Add(coupEffectue);            // Ajoute le mouvement dans la liste des coups au format pdn.
            if (!JeuMoteurEnCours)                      // Et on l'affiche si ce n'est pas le moteur qui joue
                LabelCoupJoue.Text = "Joué : " + coupEffectue;
            ActiveDamier(true);         // Réactiver les interactions après le déplacement
            ActiveCouleurDamier(couleurPiece.ToString(), false);
            VerifierGain();
        }
        public async Task AnimerEtExecuterRafle(int indexSourceBox, List<(int casePrise, int caseArrivee)> rafle, CaseDamier pieceSource)
        {   // --- Execute une rafle complète avec une animation à la vitesse DureeAnimation, ne prend les pièces qu'à la fin de la rafle ---
            ActiveDamier(false); // Désactiver les interactions pendant l'animation
            EffaceCasesDestiantion();   // Efface les cadres gris des cases de destination générées lors de l'AnalyseCoupoObligatoire
            List<int> boxesAEffacerFinRafle = new List<int>();
            // Créer une copie locale des propriétés de pieceSource
            CaseDamier pieceSourceLocal = pieceSource;
            var typePiece = pieceSource.TypePiece;
            var couleurPiece = pieceSource.CouleurPiece;
            int derniereCaseRafle = 0;
            int indexSourceBoxActuel = indexSourceBox;
            var imagePiece = ListeBitmapsPiece[typePiece];  // Récupérer l'image de la pièce source
            // On Assure que la pièce source reste affichée sur la case de départ
            PictJeux[indexSourceBoxActuel].Image = imagePiece;  // Afficher la pièce source
            PictJeux[indexSourceBoxActuel].BackColor = CouleurCaseDepart; // Marquer la case de départ

            foreach (var (casePrise, caseArrivee) in rafle)
            {
                // Calculer les indices graphiques des cases de prise et d'arrivée
                int indexPriseBox = GestionDamier.ManouryVersIndexPictureBox[casePrise - 1];
                int indexArriveeBox = GestionDamier.ManouryVersIndexPictureBox[caseArrivee - 1];
                PictJeux[indexPriseBox].BackColor = CouleurCasePrise;       // Marquer la case de prise en CouleurCasePrise
                PictJeux[indexArriveeBox].BackColor = CouleurCasePivot;     // et la case d'arricée en CouleurCasePivot
                PictJeux[indexArriveeBox].Image = CadreGrisSansPrise;       // Afficher le cadre gris sans pièce
                PictJeux[indexSourceBoxActuel].Image = imagePiece;  // Afficher la pièce source

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
            if (derniereRangeeBlanche.Contains(derniereCaseRafle) && couleurPiece == LogiqueMouvementsDames.CouleurPiece.Noir)
            {   // promotion du pion noir
                MiseaJourPromotion(indexSourceBoxActuel, pieceSourceLocal);    // On met à jour la liste de pions avant de faire la promotion ...
                LogiqueMouvementsDames.MiseaJourListes(indexSourceBoxActuel, typePiece, false);  // false pour retirer pièce de la liste
                typePiece = LogiqueMouvementsDames.TypePiece.DameNoire; // Promotion en Dame noire
                DessinePiece(GestionDamier.ManouryVersIndexPictureBox[derniereCaseRafle - 1], typePiece, couleurPiece);
                PictJeux[GestionDamier.ManouryVersIndexPictureBox[derniereCaseRafle - 1]].BackColor = CouleurCaseArrivee;
            }
            else if (derniereRangeeNoire.Contains(derniereCaseRafle) && couleurPiece == LogiqueMouvementsDames.CouleurPiece.Blanc)
            {   // promotion du pion blanc
                MiseaJourPromotion(indexSourceBoxActuel, pieceSourceLocal);    // On met à jour la liste de pions avant de faire la promotion ...
                LogiqueMouvementsDames.MiseaJourListes(indexSourceBoxActuel, typePiece, false);  // false pour retirer pièce de la liste
                typePiece = LogiqueMouvementsDames.TypePiece.DameBlanche; // Promotion en Dame blanche
                DessinePiece(GestionDamier.ManouryVersIndexPictureBox[derniereCaseRafle - 1], typePiece, couleurPiece);
                PictJeux[GestionDamier.ManouryVersIndexPictureBox[derniereCaseRafle - 1]].BackColor = CouleurCaseArrivee;
            }
            // Parcourir la liste des pièces prises et effacer les pièces
            foreach (var index in boxesAEffacerFinRafle)
            {
                EffacePiece(index, true);
                await Task.Delay(DureeAnimation);
            }
            boxesAEffacerFinRafle.Clear();
            string coupEffectue = GestionDamier.PictureBoxVersManoury[indexSourceBox] + "x"  + derniereCaseRafle;
            PictJeux[GestionDamier.ObtenirIndicePictureBox(derniereCaseRafle)].BackColor = CouleurCaseArrivee;
            ListeCoupsPdn.Add(coupEffectue);            // Ajoute le mouvement dans la liste des coups au format pdn.
            if (!JeuMoteurEnCours)                      // Et on l'affiche si ce n'est pas le moteur qui joue
                LabelCoupJoue.Text = "Joué : " + coupEffectue;
            ActiveDamier(true); // Réactiver les interactions après l'animation
            ActiveCouleurDamier(couleurPiece.ToString(), false);
            VerifierGain();
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Procédures d'affichage diverses
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void AfficheScan()  // Analyse et affiche les informations du moteur SCAN
        {   // Le protocole Hub2 est assez simpliste et peu documenté ...   
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(AfficheScan));
                return; // Empêche l'exécution du reste de la méthode sur le thread d'origine
            }
            else
                DonneesMoteurScan = MoteurDamesScan.DonneesScan.Split(' ');                      // Découpage des informations du moteur SCAN
            if (string.IsNullOrWhiteSpace(DonneesMoteurScan[0]) == false & DonneesMoteurScan.Length > 2)
            {                                                   // true si la chaine est " ", "\n", null, ""
                switch (DonneesMoteurScan[0])          // identifier le premier mot
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
                            string name = null, version = null, author = null, country = null;
                            foreach (Match match in Regex.Matches(MoteurDamesScan.DonneesScan, pattern))
                            {
                                if (match.Groups[1].Success) name = match.Groups[1].Value;
                                if (match.Groups[2].Success) version = match.Groups[2].Value;
                                if (match.Groups[3].Success) author = match.Groups[3].Value;
                                if (match.Groups[4].Success) country = match.Groups[4].Value;
                            }
                            // Affichage des résultats
                            LabelAfficheScan.Text = "Nom programme : " + name + " / Version : " + version + " / Auteur : " + author + " / Pays : " + country;
                        }
                        break;
                    case "info":
                        for (int scanIndex = 1; (scanIndex < DonneesMoteurScan.Length); scanIndex++) // Recherche des informations sur la chaine DonneesMoteurScan
                            switch (DonneesMoteurScan[scanIndex].Split('=')[0])
                            {   // On regarde ce qu'il y a avant le signe =
                                case "score":
                                    // Conversion en décimal avec gestion de la culture
                                    decimal valeur = Decimal.Parse(DonneesMoteurScan[scanIndex].Split('=')[1], CultureInfo.InvariantCulture);
                                    LabelScore.Text = "Score = " + valeur.ToString("F2", CultureInfo.InvariantCulture);     // Affichage avec le format "XX.YY"
                                    break;
                                case "pv":          // Affichage de la variation principlale
                                    int position = MoteurDamesScan.DonneesScan.IndexOf(" pv");
                                    string variationMoteur = MoteurDamesScan.DonneesScan.Substring(position + 4);   // exemple : pv="14x23x19 50-44 
                                    if (variationMoteur.Length > 100)
                                        variationMoteur = variationMoteur.Substring(0, 100);     // On limite la longueur de la variation, pour rester dans le label
                                    LabelAfficheScan.Text = "Ligne Scan3.1 = " + variationMoteur;
                                    break;
                            }
                        break;
                    case "error":
                        Console.WriteLine($"ERROR MESSAGE RETOURNE PAR SCAN !");
                        break;
                }
            }
        }
        private void AfficheCoupMoteur()        // Le moteur SCAN joue son meilleur coup
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(AfficheCoupMoteur));
                return; // Empêche l'exécution du reste de la méthode sur le thread d'origine
            }
            else
            {
                LabelInformationJoueur.Text = "A vous de jouer";
                Console.WriteLine($"Coup retouré par Scan 3.1 : {MoteurDamesScan.CoupScan}");
            }
        }
        private void AfficheDonneesBrutes()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(AfficheDonneesBrutes));
                return;     // Empêche le code suivant de s'exécuter sur le thread secondaire
            }
            else
            {
                if (MoteurDamesScan.ScanVersGui)
                {
                    donneesBrutesDames.DonneesBrutesVue.AppendText(Environment.NewLine + "[" + "SCAN 3.1" + "]" + MoteurDamesScan.DonneesScan);
                }
                else
                {
                    donneesBrutesDames.DonneesBrutesVue.AppendText(Environment.NewLine + " [BrunoGUI_Stockfish]    " + MoteurDamesScan.DonneesVersScan);
                    donneesBrutesDames.DonneesBrutesVue.ScrollToCaret();   // Pour garder l'affichage dans toute la fenêtre
                }
            }
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Routines de Dessin
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐

        public void DessinePiece(int indexPictureBox, TypePiece pieceType, CouleurPiece pieceCouleur)
        {   // --- Dessin de la pièce passée en paramètre sur la case PictureBox en paramètre + mise à jour de DamierContenu + mise à jour des listes ---
            try
            {   // Traduire l'index linéaire 0-99 en coordonnées (ligne, colonne) pour DamierContenu
                (int ligne, int colonne) = GestionDamier.IndiceVersCoordonnees(indexPictureBox);
                // Mettre à jour DamierContenu
                GestionDamier.DamierContenu[ligne, colonne].CouleurPiece = pieceCouleur;
                GestionDamier.DamierContenu[ligne, colonne].TypePiece = pieceType;
                // Mettre à jour les listes
                LogiqueMouvementsDames.MiseaJourListes(indexPictureBox, pieceType, true);   // true pour ajouter la pièce à la liste
                AfficheListesPieces();
                if (pieceType == TypePiece.Vide)
                {   // Si la case est vide, on efface l'image
                    PictJeux[indexPictureBox].Image = null;
                    return;
                }
                if (ListeBitmapsPiece.ContainsKey(pieceType))
                {   // Définit l'image en fonction du type de pièce
                    PictJeux[indexPictureBox].Image = ListeBitmapsPiece[pieceType];
                }
                else
                {
                    PictJeux[indexPictureBox].Image = null;     // Par défaut, aucune image
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur dans DessinePiece : " + ex.Message);
                Console.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }
        public void EffacePiece(int indexPictureBox, bool damier)
        {   // --- Efface la pièce située à indexPictureBox, et actualise DamierContenu si damier est vrai ---
            if (damier)
            {
                (int ligne, int colonne) = GestionDamier.IndiceVersCoordonnees(indexPictureBox);
                TypePiece pieceAEffacer = GestionDamier.DamierContenu[ligne, colonne].TypePiece;
                GestionDamier.DamierContenu[ligne, colonne].TypePiece = TypePiece.Vide;
                GestionDamier.DamierContenu[ligne, colonne].CouleurPiece = CouleurPiece.Vide;
                LogiqueMouvementsDames.MiseaJourListes(indexPictureBox, pieceAEffacer, false);  // false pour retirer pièce de la liste
                AfficheListesPieces();
            }
            PictJeux[indexPictureBox].Image = null;
        }
        public static void DessineMouvements(int caseSourceManoury)
        {   // --- Obtiens et dessine tous les mouvements possibles pour la case source ---
            PictJeux[GestionDamier.ManouryVersIndexPictureBox[caseSourceManoury - 1]].BackColor = CouleurCaseDepart; // Colorier la case source
            var mouvements = LogiqueMouvementsDames.TrouverMouvementsPossibles(caseSourceManoury);
            // Vérifier s'il existe des rafles
            bool existeRafles = mouvements.Rafles != null && mouvements.Rafles.Any();
            if (existeRafles)
            {   // Trouver la longueur maximale des rafles
                int maxLongueurRafle = mouvements.Rafles.Max(rafle => rafle.Count);
                /* 
                S'il existe plusieurs rafles possibles, la plus longue est le seul coup valide et doit donc être signalée (Orange ?)
                S'il existe 2 rafles les + longues de même longueur, elles doivent être signalées (Orange ?), c'est au joueur de choisir
                Les rafles de longueur inférieure à la + longue ne sont pas affichées
                */
                // Filtrer les rafles pour ne garder que celles de longueur maximale
                mouvements.Rafles = mouvements.Rafles
                    .Where(rafle => rafle.Count == maxLongueurRafle)
                    .ToList();
                // Mettre à jour la liste globale des rafles possibles
                BrunoInterfaceGraphiqueDames.RaflesPossibles.Clear();
                BrunoInterfaceGraphiqueDames.RaflesPossibles.AddRange(mouvements.Rafles);
                // DEBUG
                Console.WriteLine("Rafles après filtrage (longueur maximale) :");
                foreach (var rafle in mouvements.Rafles)
                {
                    Console.WriteLine(string.Join(", ", rafle.Select(x => x.Prise.ToString() + " -> " + x.Arrivee.ToString())));
                }
                // DEBUG
                // Traiter les rafles (étape 1 : traitement général)
                foreach (var rafle in mouvements.Rafles)
                {
                    foreach (var (prise, arrivee) in rafle)
                    {
                        // Vérifier si les indices sont valides pour "Prise" et "Arrivee"
                        if (prise - 1 >= 0 && prise - 1 < GestionDamier.ManouryVersIndexPictureBox.Length &&
                            arrivee - 1 >= 0 && arrivee - 1 < GestionDamier.ManouryVersIndexPictureBox.Length)
                        {
                            int indexPrise = GestionDamier.ManouryVersIndexPictureBox[prise - 1];
                            int indexArrivee = GestionDamier.ManouryVersIndexPictureBox[arrivee - 1];
                            if (PictJeux[indexArrivee].Image == null)       // Ne pas effacer les pièces si la case est occupée
                            {   // Vérification pour les cases de prise
                                PictJeux[indexPrise].BackColor = CouleurCasePrise;
                                PictJeux[indexArrivee].Image = CadreGrisSansPrise;
                                PictJeux[indexArrivee].BackColor = Color.Orange;    // A enlever si on ne veut que les cases pièces prises !!!!
                                // Sauvegarder les cases pour les étapes ultérieures
                                ListeCasesMouvementPrecedent.Add(prise);
                                ListeCasesMouvementPrecedent.Add(arrivee);
                                ListeCasesDestination.Add(arrivee); // Garder les arrivées pour vérification finale
                            }
                        }
                    }
                }
                // Étape 2 : Colorier uniquement les cases d'arrivée finale en CouleurCaseArrivee
                foreach (var rafle in mouvements.Rafles)
                {
                    if (rafle.Count > 0) // Vérifier qu'il y a bien des mouvements dans la rafle
                    {
                        var dernierMouvement = rafle[rafle.Count - 1]; // Récupérer le dernier mouvement de la rafle (prise, arrivée)
                        int arriveeFinale = dernierMouvement.Arrivee;
                        // Vérifier si l'indice de l'arrivée finale est valide
                        if (arriveeFinale - 1 >= 0 && arriveeFinale - 1 < GestionDamier.ManouryVersIndexPictureBox.Length)
                        {
                            int indexArriveeFinale = GestionDamier.ManouryVersIndexPictureBox[arriveeFinale - 1];
                            PictJeux[indexArriveeFinale].BackColor = CouleurCaseArrivee; // Colorier la case finale
                        }
                    }
                }
            }
            else if (mouvements.DeplacementsSimples != null)
            {   // Traiter les déplacements simples uniquement si aucune rafle n'existe, ils sont affichés en CouleurTrajetSuivi avec un cadre gris
                foreach (var caseManoury in mouvements.DeplacementsSimples)
                {
                    if (caseManoury - 1 >= 0 && caseManoury - 1 < GestionDamier.ManouryVersIndexPictureBox.Length)
                    {
                        int index = GestionDamier.ManouryVersIndexPictureBox[caseManoury - 1];
                        PictJeux[index].Image = CadreGrisSansPrise;         // On affiche un cadre gris sur les cases de déplacement simple
                        PictJeux[index].BackColor = CouleurTrajetSuivi;     // On colorie les cases de déplacement simple avec CouleurTrajetSuivi
                        DeplacementsSimplesPossibles.Add(caseManoury);
                        ListeCasesMouvementPrecedent.Add(caseManoury);
                        ListeCasesDestination.Add(caseManoury);
                    }
                }
            }
        }
        public static void EffaceMouvements()
        {   // --- Efface toutes les indications des mouvements pour déplacer la pièce ---
            EffaceCasesDestiantion();   // Efface les cadres gris des cases de destination générées lors de l'AnalyseCoupoObligatoire
            for (int i = 0; i < ListeCasesDestination.Count; i++)           // On enlève tous les cadres gris
            {   // On enlève les cadres gris
                PictJeux[GestionDamier.ManouryVersIndexPictureBox[ListeCasesDestination[i] - 1]].Image = null;
            }
            ListeCasesDestination.Clear();
            for (int i = 0; i < ListeCasesMouvementPrecedent.Count; i++)    // On enlève tous les mouvements affichés précédemment
            {   // On remet la couleur de base sur les cases
                PictJeux[GestionDamier.ManouryVersIndexPictureBox[ListeCasesMouvementPrecedent[i] - 1]].BackColor = BrunoInterfaceGraphiqueDames.CouleurCasesombre;
            }
            ListeCasesMouvementPrecedent.Clear();
        }
        public static void EffaceCasesDestiantion()
        {   // --- Efface les cases de destination des mouvements possibles ---
            for (int i = 0; i < ListeCasesDestination.Count; i++)           // On enlève tous les cadres gris
            {   // On enlève les cadres gris
                PictJeux[GestionDamier.ManouryVersIndexPictureBox[ListeCasesDestination[i] - 1]].Image = null;
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
                    {   //  Les cases sont des PictureBox indéxées, par exemple : case a1 = PictJeux21, case h8 = PictJeux98
                        Name = "Pictjeux" + index.ToString(),
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
                    PictJeux.Add(Pict);
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
                    if (PictJeux[i].Visible)
                        PictJeux[i].Enabled = statut;
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
                    PictJeux[i].Enabled = statut;
                }
            }
        }
        public static void ActiverCaseManoury(int caseManoury, bool statut)
        {   // --- Active ou désactive une case Manoury (1 à 50) ---
            PictJeux[GestionDamier.ObtenirIndicePictureBox(caseManoury)].Enabled = statut;
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
                        PictJeux[index].BackColor = couleur;
                    }
                }
            }
        }
        public void VerifierGain()
        {   // --- Verifie le gain uniquement sur a présence de pièces ---
            if (ListePionsBlancs.Count == 0 && ListeDamesBlanches.Count == 0)
            {   // Vérifier si les listes blanches sont vides
                GroupResultat.Enabled = false;   // Désactive le groupe de boutons
                RadioGainNoir.Checked = PartieTerminee = FinPartie = true;  // Note : RadioGainNoir.Checked mis à true lance la méthode RadioGainNoir_CheckedChanged
                LabelInformationJoueur.Text = "Les Noirs gagnent, les blancs n'ont plus de pièces";
                MessageBox.Show("Les Noirs gagnent !\n Les blancs n'ont plus de pièces", "Partie terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (ListePionsNoirs.Count == 0 && ListeDamesNoires.Count == 0)
            {   // Vérifier si les listes noires sont vides
                GroupResultat.Enabled = false;   // Désactive le groupe de boutons
                RadioGainBlanc.Checked = PartieTerminee = FinPartie = true; // Note: RadioGainBlanc.Checked mis à true lance la méthode RadioGainBlanc_CheckedChanged
                LabelInformationJoueur.Text = "Les Blancs gagnent, les noirs n'ont plus de pièces";
                MessageBox.Show("Les Blancs gagnent !\n Les noirs n'ont plus de pièces", "Partie terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            // TODO : Ajouter la vérification du cas ou un joueur ne peut plus bouger
            // TODO : Chercher quels sont les cas de gain de la partie ?
            // TODO : Chercher quels sont les cas de partie nulle ?
        }
        public static bool EstCaseValide(int CaseDestinationManoury)
        {   // C'est une case valide si :
            // 1. Elle fait partie des déplacements simples possibles
            // 2. Elle est la dernière case d'une rafle possible
            return DeplacementsSimplesPossibles.Contains(CaseDestinationManoury) ||
                   RaflesPossibles.Any(rafle => rafle.Last().caseArrivee == CaseDestinationManoury);
        }
        private void InitialiserEtatMouvement()
        {   // --- Remet à zéro les paramètes utilisés pour dessiner les mouvements possibles ---
            PieceCouleurSource = null;
            CaseSourceManoury = 0;
            CaseDestinationManoury = 0;
            IndexPictureBoxSource100 = -1;
            DeplacementsSimplesPossibles.Clear();
            ClickCaseSource = true; // Retourne à l'état de sélection de pièce
        }
        public void AfficheListesPieces()
        {   // --- Affiche les 4 listes de pièces, pions blancs et noirs, dames blanches et noires ---
            LabelPionsBlancs.Text = ListePionsBlancs.Count +  " pions blancs";
            LabelPionsNoirs.Text = ListePionsNoirs.Count +  " pions noirs";
            LabelDamesBlanches.Text = ListeDamesBlanches.Count +  " Dames blanches";
            LabelDamesNoires.Text = ListeDamesNoires.Count +  " Dames noires";
        }
        public static void AfficheIndexBox(PictureBox pict, int index, int caseLogique)
        {   // --- Affiche les index Box et Manoury an haut à gauche et droite ---
            pict.Tag = $"{index},{caseLogique}";    // Stocker l'index et la case logique sous forme de chaîne dans Tag
            pict.Paint += PictureBox_Paint;         // Ajouter le gestionnaire Paint à la PictureBox
            pict.Invalidate();                      // Forcer la mise à jour
        }
        private static void PictureBox_Paint(object sender, PaintEventArgs e)        // Déclaration du gestionnaire d'événement Paint
        {   // --- Evénement Paint lié à la PictureBox ---
            PictureBox pict = sender as PictureBox;
            // Récupérer les données depuis Tag et les séparer
            string[] data = pict.Tag.ToString().Split(',');
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
        private void MontreDonneesScan_Click(object sender, EventArgs e)
        {   // --- Affiche ou masque les données de SCAN 3.1 à chaque clic ---
            MontreDonneesDames = !MontreDonneesDames;       
            MontreDonneesScan.Text = MontreDonneesDames ? "Masque protoc." : "Affiche protoc.";
            if (MontreDonneesDames) donneesBrutesDames.Show();    // On affiche les données brutes SCAN
            else donneesBrutesDames.Hide();                       // On masque les données brutes SCAN
            donneesBrutesDames.DonneesBrutesVue.ScrollToCaret();      // Pour garder l'affichage dans toute la fenêtre
        }
        private void VisualisationPdn_Click(object sender, EventArgs e)
        {   // --- Bouton pour voir la partie en Pdn ---
            if (affichePdn == null || affichePdn.IsDisposed)
            {   // Traitement pour prendre en compte la fermeture par croix rouge en haut à droite ...
                affichePdn = new AffichePdn();
                affichePdn.FormClosed += (s, args) =>
                {   // On évite que la référence de affichePdn pointe vers un objet supprimé.
                    affichePdn = null;  // S'assure que la référence est libérée à la fermeture
                };
            }
            affichePdn.Show();
            affichePdn.AffichePdnDansZone();
        }
        private void BoutonPrecedent_Click(object sender, EventArgs e)
        {   // --- Bouton pour reculer d'un coup (parcours de partie) ---
            if (indexFenCoupActuel > 0) // Assure que l'on peut reculer sans sortir de la liste
            {
                indexFenCoupActuel--;
                LogiqueMouvementsDames.MiseenplaceFen(ListeCoupsFen[indexFenCoupActuel], false);
                // Vérifie si l'on est au premier coup pour éviter un accès incorrect à ListeCoupsPdn
                if (indexFenCoupActuel > 0)
                {
                    LabelCoupJoue.Text = $"Coup {indexFenCoupActuel} : {ListeCoupsPdn[indexFenCoupActuel - 1]}";
                    Console.WriteLine($"Coup {indexFenCoupActuel} : {ListeCoupsPdn[indexFenCoupActuel - 1]}");
                }
                else
                {   // Si on est au début, afficher un message spécial ou la position initiale.
                    LabelCoupJoue.Text = $"Position initiale : {ListeCoupsPdn[0]}";
                    Console.WriteLine($"Position initiale : {ListeCoupsPdn[0]}");
                }
            }
            else
            {
                MessageBox.Show("Vous êtes au début de la partie.", "Début de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonSuivant_Click(object sender, EventArgs e)
        {   // --- Bouton pour avancer d'un coup (parcours de partie) ---
            if (indexFenCoupActuel < ListeCoupsFen.Count - 1 && indexFenCoupActuel - 1 < ListeCoupsPdn.Count) // Vérifie la synchronisation
            {
                indexFenCoupActuel++;
                LogiqueMouvementsDames.MiseenplaceFen(ListeCoupsFen[indexFenCoupActuel], false);
                // Accède à ListeCoupsPdn en tenant compte du décalage
                LabelCoupJoue.Text = $"Coup {indexFenCoupActuel} : {ListeCoupsPdn[indexFenCoupActuel - 1]}";
                Console.WriteLine($"Coup {indexFenCoupActuel} : {ListeCoupsPdn[indexFenCoupActuel - 1]}");
            }
            else
            {
                MessageBox.Show("Vous êtes à la fin de la partie ou une des listes est vide.", "Fin de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonDebut_Click(object sender, EventArgs e)
        {
            if (ListeCoupsFen.Count > 1) // Vérifie qu'il y a au moins un coup joué
            {
                indexFenCoupActuel = 1; // Premier coup joué (position initiale est à l'index 0)
                LogiqueMouvementsDames.MiseenplaceFen(ListeCoupsFen[indexFenCoupActuel], false);
                LabelCoupJoue.Text = $"Coup {indexFenCoupActuel} : {ListeCoupsPdn[indexFenCoupActuel - 1]}";
            }
        }
        private void BoutoonFin_Click(object sender, EventArgs e)
        {
            if (ListeCoupsFen.Count > 1) // Vérifie qu'il y a au moins un coup joué
            {
                indexFenCoupActuel = ListeCoupsFen.Count - 1; // Dernière position
                LogiqueMouvementsDames.MiseenplaceFen(ListeCoupsFen[indexFenCoupActuel], false);
                LabelCoupJoue.Text = $"Coup {indexFenCoupActuel} : {ListeCoupsPdn[indexFenCoupActuel - 1]}";
            }
        }
        private async void OrdinateurJoue_Click(object sender, EventArgs e)
        {   // --- L'ordinateur joue son meilleur coup ---
            await LancementReflexionMoteur(LogiqueMouvementsDames.ListeCoupsHub2[LogiqueMouvementsDames.ListeCoupsHub2.Count - 1], TempsReflexion);
            PartieEnCours = true;
            ChangerCouleurTrait();      // Inverse le trait
            AnalyseCoupObligatoire(CouleurAuTrait);     // Vérifie si un coup obligatoire existe
        }
        private void PositionFen_Click(object sender, EventArgs e)
        {   // --- Affiche la boîte de dialogue et traite le fichier FEN sélectionné  ---
            if (ChargerFichierFen.ShowDialog() == DialogResult.OK)
            {
                string cheminFichier = ChargerFichierFen.FileName;
                try
                {
                    // Lire le contenu du fichier et l'afficher dans la console
                    string contenuFichier = File.ReadAllText(cheminFichier);
                    NettoyageRapide();
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
        {
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
        private void PointArret_Click(object sender, EventArgs e)
        {   // --- Permet de générer un poiont d'arrêt par un bouton ---
            Console.WriteLine($"Point d'arrêt");
            // Pour debug
        }
        private void BoxNomJoueurBlanc_TextChanged(object sender, EventArgs e)
        {
            NomJoueurBlanc = BoxNomJoueurBlanc.Text;
            Console.WriteLine($"Texte saisi : {NomJoueurBlanc}");
        }
        private void BoxNomJoueurNoir_TextChanged(object sender, EventArgs e)
        {
            NomJoueurNoir = BoxNomJoueurNoir.Text;
            Console.WriteLine($"Texte saisi : {NomJoueurNoir}");
        }
        private void trackBarTempsReflexion_ValueChanged(object sender, EventArgs e)
        {
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
        {
            if (RadioGainBlanc.Checked)
            {   // 1-0 les blancs gagnent
                LabelInformationJoueur.Text = "Gain Blanc sélectionné (Partie terminée)";
                ListeCoupsPdn.Add(" 1-0");
                GroupResultat.Enabled = false;   // Désactive le groupe de boutons
                PartieTerminee = FinPartie = true;
            }
        }
        private void RadioGainNoir_CheckedChanged(object sender, EventArgs e)
        {
            if (RadioGainNoir.Checked)
            {   // 0-1 les noirs gagnent
                LabelInformationJoueur.Text = "Gain Noir sélectionné (Partie terminée)";
                ListeCoupsPdn.Add(" 0-1");
                GroupResultat.Enabled = false;   // Désactive le groupe de boutons
                PartieTerminee = FinPartie = true;
            }
        }
        private void RadioNulle_CheckedChanged(object sender, EventArgs e)
        {
            if (RadioNulle.Checked)
            {   // 1/2-1/2 partie nulle
                LabelInformationJoueur.Text = "Nulle sélectionnée (Partie terminée)";
                ListeCoupsPdn.Add(" 1/2-1/2");
                GroupResultat.Enabled = false;   // Désactive le groupe de boutons
                PartieTerminee = FinPartie = true;
            }
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
            LogiqueMouvementsDames.MiseenplaceFen(ListeCoupsFen[ListeCoupsFen.Count - 2], false);       // Met en place la position précédente
            AfficheListesPieces();
            ListeCoupsPdn.RemoveAt(dernierIndex);   // Supprime les derniers éléments des listes
            ListeCoupsFen.RemoveAt(ListeCoupsFen.Count - 1);
            ListeCoupsHub2.RemoveAt(ListeCoupsHub2.Count - 1);
            NettoyageRapide();
            LabelCoupJoue.Text = $"Mouvement effacé = {coupEfface}"; // Met à jour le label avec le mouvement.
            AnalyseCoupObligatoire(CouleurAuTrait);
            PartieEnCours = false;
        }
        private void BoutonTournerDamier_Click(object sender, EventArgs e)
        {   // Nouveau tableau temporaire pour les PictureBox
            PictureBox[] nouveauPictJeux = new PictureBox[100];
            // Mettre à jour les positions et indices
            for (int i = 0; i < PictJeux.Count; i++)
            {
                // Obtenir la PictureBox actuelle
                PictureBox pict = PictJeux[i];

                int ligneActuelle = i / 10;                                 // Calculer les coordonnées ligne/colonne actuelles
                int colonneActuelle = i % 10;
                int nouvelleLigne = 9 - ligneActuelle;                      // Calculer les nouvelles coordonnées après rotation
                int nouvelleColonne = 9 - colonneActuelle;
                int nouvelIndex = nouvelleLigne * 10 + nouvelleColonne;     // Calculer le nouvel index après rotation

                // Positionner graphiquement la PictureBox
                Point nouvellePosition = new Point(20 + (nouvelleColonne * 60), 20 + (nouvelleLigne * 60));
                pict.Location = nouvellePosition;
                // Mettre à jour la liste temporaire
                nouveauPictJeux[nouvelIndex] = pict;
            }
            // Copier les nouvelles valeurs dans la liste existante
            for (int i = 0; i < PictJeux.Count; i++)
            {
                PictJeux[i] = nouveauPictJeux[i];
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
            CaseDamier[,] nouveauDamierContenu = new CaseDamier[10, 10];
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
            MoteurDamesScan.Quitte();
            Close();
        }
    }
}
