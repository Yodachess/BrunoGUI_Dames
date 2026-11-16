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
        public readonly List<PictureBox> CaseDamier = new List<PictureBox>();    // les 100 cases du jeu
        public List<PartieDamesPdn> ListePartiesPdn = new List<PartieDamesPdn>();
        // liste des Bitmaps pour les pièces
        public readonly Dictionary<Piece, Bitmap> ListeBitmapsPiece = new Dictionary<Piece, Bitmap>();
        // les Bitmaps
        private readonly Bitmap PionBlanc = Properties.Resources.DamesPionBlanc_transparent;
        private readonly Bitmap PionNoir = Properties.Resources.DamesPionNoir_transparent;
        private readonly Bitmap DameBlanche = Properties.Resources.DameBlanche;
        private readonly Bitmap DameNoire = Properties.Resources.DameNoire;
        private readonly Bitmap CadreGrisSansPrise = Properties.Resources.SansPrise;
        // les variables
        // Les couleurs du damier : celles de Preferences.CouleursParDefaut, changées par le fichier de préférences (ChargePreferences)
        public Color CouleurCaseSombre = CouleurParDefaut("CaseSombre");         // Cases sombres damier (les cases jouables)
        public Color CouleurCaseClaire = CouleurParDefaut("CaseClaire");         // Cases claires damier
        public Color CouleurCaseDepart = CouleurParDefaut("CaseDepart");         // La pièce sélectionnée
        public Color CouleurTrajetSuivi = CouleurParDefaut("TrajetSuivi");       // Les destinations possibles et le trajet du dernier coup
        public Color CouleurCasePivot = CouleurParDefaut("CasePivot");           // L'arrivée de chaque étape d'une rafle pendant son animation
        public Color CouleurCasePrise = CouleurParDefaut("CasePrise");           // Les pièces prises
        public Color CouleurCaseArrivee = CouleurParDefaut("CaseArrivee");       // L'arrivée du coup
        public Color CouleurCaseEtape = CouleurParDefaut("CaseEtape");           // Cases d'arrivée intermédiaires d'une rafle
        public Couleur CouleurAuTrait = Couleur.Blanc;     // Le camp dont c'est le tour à l'écran ; pendant que le moteur réfléchit, c'est celui de l'humain (voir ChangerCouleurTrait)
        public string NomJoueurBlanc = "Bruno" , NomJoueurNoir = "SCAN 3.1";
        public FichierPartiePdn.PartieDamesPdn BalisesPartie = new FichierPartiePdn.PartieDamesPdn();
        public bool ArriveesDebutTour;                           // Prise obligatoire : montrer dès le début du tour les cases d'arrivée de toutes les prises (préférence ArriveesDebutTour)
        public string RepereDernierCoup = "coin";                // Le repère du dernier coup : coin, cadre ou aucun (préférence RepereDernierCoup)
        public int IndexPictureBoxSource100, CaseSourceManoury, CaseDestinationManoury, DureeEtape, TempsReflexion;     // DureeEtape : millisecondes d'une étape d'animation (un glissement, une étape de rafle)
        public bool OrdinateurActif, PartieTerminee, EmetUnSon;
        private bool clickCaseSource, montreDonneesDames, jeuMoteurEnCours, analyseEnCours;
        private Piece pieceSelectionnee = Piece.Vide;     // Pièce sélectionnée au premier clic (Piece.Vide = aucune)
        private string cheminMoteur = "";
        private Preferences preferencesChargees = new Preferences();     // Celles lues au démarrage (gardées si le moteur est absent)
        private string nomBlancPrefere = "", nomNoirPrefere = "";        // Les noms tapés par le joueur (pas ceux d'une partie PDN chargée)
        private bool nomsDepuisPartie;                                   // Vrai pendant que les noms d'une partie PDN chargée sont mis dans les zones de texte
        private static string FichierPreferences => ReglagesDeTest.FichierPreferences ?? Path.Combine(Application.StartupPath, "BrunoGUI_Dames.preferences.ini");
        private bool moteurDisponible = true;     // Faux si le moteur n'a pas pu démarrer ou s'est arrêté
        private string nomMoteur = "", auteurMoteur = "", versionMoteur = "";
        // -- les classes --
        private readonly DonneesBrutesDames donneesBrutesDames = new DonneesBrutesDames();
        private AffichePdn? affichePdn;   // affichePdn est à la fois déclarée et instanciée. Prêt à être utilisé dès le début
        private FichierPartiePdn? fichierPartiePdn = new FichierPartiePdn();     // idem pour fichierPartiePdn
        // La position du damier et le sens d'affichage (GestionDamier.cs) et l'historique de la partie (SessionPartie.cs) appartiennent à la fenêtre
        public readonly GestionDamier Damier = new GestionDamier();
        public readonly SessionPartie Session = new SessionPartie();

        public BrunoInterfaceGraphiqueDames()
        {
            InitializeComponent();
            affichePdn = new AffichePdn(this);
        }
        private void BrunoInterfaceGraphiqueDames_Load(object sender, EventArgs e)
        {
            // les évènements dans les classes
            MoteurDamesScan.AfficheScan += AfficheScan;
            MoteurDamesScan.AfficheDonneesBrutes += AfficheDonneesBrutes;
            MoteurDamesScan.MoteurArrete += MoteurArrete;
            // Liste des pièces du jeu
            ListeBitmapsPiece.Add(Piece.PionBlanc, PionBlanc);       // pion blanc
            ListeBitmapsPiece.Add(Piece.PionNoir, PionNoir);         // pion noir
            ListeBitmapsPiece.Add(Piece.DameBlanche, DameBlanche);   // Dame blanche
            ListeBitmapsPiece.Add(Piece.DameNoire, DameNoire);       // Dame noire
            // les variables
            CouleurAuTrait = Couleur.Blanc;
            Damier.Orientation = new OrientationDamier();       // On commence avec la vue côté Blanc
            OrdinateurActif = true;
            analyseEnCours = false;
            BoutonMasqueAffiche.Enabled = false;
            clickCaseSource = true;
            DureeEtape = 250;
            ChargePreferences();                // Les couleurs et la durée d'animation avant la création du damier ; le reste des préférences plus bas (AppliquePreferences)
            LabelTempsReflexion.Text = trackBarTempsReflexion.Value.ToString() + " sec.";            // Affichage 
            TempsReflexion = trackBarTempsReflexion.Value;              // et initialisation du temps de réflexion

            CreeDamier();                       // Crée les 100 PictureBox correspondant aux cases SANS afficher de pièce
            Damier.InitialiserDamier();  // met la position initiale dans PositionCourante SANS afficher les pièces
            ActiveDamier(true);
            BoxNomJoueurBlanc.Text = NomJoueurBlanc;
            BoxNomJoueurNoir.Text = NomJoueurNoir;
            CreePendule();                      // Les temps et le choix de la cadence (BrunoInterfaceGraphiqueDames.Pendule.cs), avant les préférences qui choisissent la cadence
            CreeLecture();                      // Le bouton de lecture et sa vitesse, dans le groupe « Parcours partie » (BrunoInterfaceGraphiqueDames.Lecture.cs)
            AppliqueTheme();                    // Les couleurs de l'interface, une fois tous les contrôles créés (BrunoInterfaceGraphiqueDames.Theme.cs)
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
            cheminMoteur = ReglagesDeTest.CheminMoteur ?? Path.Combine(Application.StartupPath, "Dames_Scan31_FabienLetouzey", "scan.exe");     // À côté de l'exe, jamais relatif au dossier courant
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
