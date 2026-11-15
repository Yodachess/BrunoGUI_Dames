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
    public partial class BrunoInterfaceGraphiqueDames : Form
    {
        // Les listes
        public static readonly List<PictureBox> CaseDamier = new List<PictureBox>();    // les 100 cases du jeu
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
        public static Color CouleurCaseSombre = Color.Peru;         // Cases sombres damier
        public static Color CouleurCaseClaire = Color.BurlyWood;    // Cases claires damier
        public static Color CouleurCaseDepart = Color.SpringGreen;
        public static Color CouleurTrajetSuivi = Color.YellowGreen;
        public static Color CouleurCasePivot = Color.DarkKhaki;
        public static Color CouleurCasePrise = Color.DarkOliveGreen;                        
        public static Color CouleurCaseArrivee = Color.OliveDrab;
        public static Color CouleurCaseEtape = Color.Orange;        // Cases d'arrivée intermédiaires d'une rafle
        public static Couleur CouleurAuTrait = Couleur.Blanc;     // Le camp dont c'est le tour à l'écran ; pendant que le moteur réfléchit, c'est celui de l'humain (voir ChangerCouleurTrait)
        public static string NomJoueurBlanc = "Bruno" , NomJoueurNoir = "SCAN 3.1";
        public static FichierPartiePdn.PartieDamesPdn PartieCourante = new FichierPartiePdn.PartieDamesPdn();
        public static int IndexPictureBoxSource100, CaseSourceManoury, CaseDestinationManoury, DureeAnimation, TempsReflexion;
        private const int DiviseurAnimationRejeu = 6;
        // --- La durée de chaque attente de l'animation : DureeAnimation, ou son sixième pendant le déroulé d'une partie PDN chargée ---
        private int DelaiAnimation => rejeuPdnEnCours ? DureeAnimation / DiviseurAnimationRejeu : DureeAnimation;
        public static bool ExisteFENDebut;
        public static bool AfficheNumeroBox, AfficheCaseLogique, OrdinateurActif, PartieTerminee, EmetUnSon;
        private static bool clickCaseSource, montreDonneesDames, jeuMoteurEnCours, analyseEnCours;
        private ContenuCase pieceCouleurSource = new ContenuCase();     // Pièce sélectionnée au premier clic (case vide = aucune)
        private string[] donneesMoteurScan = Array.Empty<string>();        // Données en provenance du Moteur SCAN
        private string cheminMoteur = "";
        private Preferences preferencesChargees = new Preferences();     // Celles lues au démarrage (gardées si le moteur est absent)
        private string nomBlancPrefere = "", nomNoirPrefere = "";        // Les noms tapés par le joueur (pas ceux d'une partie PDN chargée)
        private bool nomsDepuisPartie;                                   // Vrai pendant que les noms d'une partie PDN chargée sont mis dans les zones de texte
        private static string FichierPreferences => Path.Combine(Application.StartupPath, "BrunoGUI_Dames.preferences.ini");
        private static bool moteurDisponible = true;     // Faux si le moteur n'a pas pu démarrer ou s'est arrêté
        private string nomMoteur = "", auteurMoteur = "", versionMoteur = "";
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
            CouleurAuTrait = Couleur.Blanc;
            GestionDamier.Orientation = new OrientationDamier();       // On commence avec la vue côté Blanc
            OrdinateurActif = true;
            AfficheNumeroBox = ExisteFENDebut = analyseEnCours = false;
            BoutonMasqueAffiche.Enabled = false;
            AfficheCaseLogique = clickCaseSource = true;
            DureeAnimation = 900;
            ChargePreferences();                // Les couleurs et la durée d'animation avant la création du damier ; le reste des préférences plus bas (AppliquePreferences)
            LabelTempsReflexion.Text = trackBarTempsReflexion.Value.ToString() + " sec.";            // Affichage 
            TempsReflexion = trackBarTempsReflexion.Value;              // et initialisation du temps de réflexion

            CreeDamier();                       // Crée les 100 PictureBox correspondant aux cases SANS afficher de pièce
            GestionDamier.InitialiserDamier();  // met la position initiale dans PositionCourante SANS afficher les pièces
            ActiveDamier(true);
            BoxNomJoueurBlanc.Text = NomJoueurBlanc;
            BoxNomJoueurNoir.Text = NomJoueurNoir;
            CreePendule();                      // Les temps et le choix de la cadence (BrunoInterfaceGraphiqueDames.Pendule.cs), avant les préférences qui choisissent la cadence
            AppliquePreferences();
            MiseenplaceFen("[FEN \"W:W31-50:B1-20\"]", true);          // Position initiale
            AfficheListesPieces();
            ReinitialisePendule();              // Sans pendule choisie, sans effet
            if (preferencesChargees.VueCoteNoir)
                BoutonTournerDamier_Click(this, EventArgs.Empty);       // Le damier est vu du côté des noirs, comme à la dernière fermeture
            if (CouleurAuTrait == Couleur.Blanc)
            {
                ActiveCouleurDamier(Couleur.Noir, false);
                LabelInformationJoueur.Text = "Trait aux Blancs";
            }
            else
            {
                ActiveCouleurDamier(Couleur.Blanc, false);
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
    }
}
