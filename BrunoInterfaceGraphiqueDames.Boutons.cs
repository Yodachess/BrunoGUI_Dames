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
        // Boutons, parcours de la partie, options et retour arrière.
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des boutons
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void NouvellePartie_Click(object sender, EventArgs e)
        {   // --- Initialisation du damier et mise à zéro des diverses listes --- 
            Damier.ViderDamier();
            Damier.InitialiserDamier();  // met la position initiale dans PositionCourante SANS afficher les pièces
            NettoyageRapide();                  // Remet PartieTerminee à faux : à faire AVANT d'activer les cases (ActiveDamier(true) ne fait rien tant que la partie est finie)
            ActiveDamier(true);
            MiseenplaceFen("[FEN \"W:W31-50:B1-20\"]", true);   // Position initiale : la nouvelle partie repart de zéro (historique, résultat, parcours)
            AfficheListesPieces();
            InitialiserEtatMouvement();
            LabelCoupJoue.Text = "";
            ReinitialisePendule();
        }
        private void NettoyageRapide()
        {   // --- Nettoyage rapide du damier, labels et boutons  ---
            CouleurToutesCasesManoury(CouleurCaseSombre);       // On efface les couleurs des cases
            GroupResultat.Enabled = true;                       // On remet à zéro le résultat
            RadioGainNoir.Checked = false;
            RadioGainBlanc.Checked = false;
            RadioNulle.Checked = false;
            PartieTerminee = false;
            LabelCoupJoue.Text = LabelInformationJoueur.Text = LabelPrisesPossibles.Text = "";
        }
        private void BloqueBoutons(bool bloque) 
        {   // --- Blocage (true) /déblocage (false) des boutons ---
            NouvellePartie.Enabled = OrdinateurJoue.Enabled = BoutonRetourArriere.Enabled = SauvePositionFen.Enabled = !bloque;
            VisualisationPdn.Enabled = BoutonMasqueAffiche.Enabled = groupParcoursPartie.Enabled = AnalysePosition.Enabled = !bloque;
            ChargePartiesPdn.Enabled = BoutonMasqueAffiche.Enabled = SauvePartiesPdn.Enabled = ChargePositionFen.Enabled = !bloque;
            if (!moteurDisponible)
                OrdinateurJoue.Enabled = AnalysePosition.Enabled = false;      // Sans moteur, ces deux boutons restent désactivés
        }
        private void MontreDonneesScan_Click(object sender, EventArgs e)
        {   // --- Affiche ou masque les données de SCAN 3.1 à chaque clic ---
            montreDonneesDames = !montreDonneesDames;       
            MontreDonneesScan.Text = montreDonneesDames ? T("Masque protoc.") : T("Affiche protoc.");
            if (montreDonneesDames) donneesBrutesDames.Show();    // On affiche les données brutes SCAN
            else donneesBrutesDames.Hide();                       // On masque les données brutes SCAN
            donneesBrutesDames.DonneesBrutesVue.ScrollToCaret();      // Pour garder l'affichage dans toute la fenêtre
        }
        private void VisualisationPdn_Click(object sender, EventArgs e)
        {   // --- Bouton pour voir la partie en Pdn ---
            if (affichePdn == null || affichePdn.IsDisposed)
            {   // Traitement pour prendre en compte la fermeture par croix rouge en haut à droite ...
                affichePdn = new AffichePdn(this);      // Crée une nouvelle instance si nécessaire
                affichePdn.FormClosed += (s, args) =>
                {   // On évite que la référence de affichePdn pointe vers un objet supprimé.
                    affichePdn = null;  // S'assure que la référence est libérée à la fermeture
                };
            }
            affichePdn.Show();          // Affiche la fenêtre et met à jour son contenu
            affichePdn.AffichePdnDansZone();
        }
        // --- Le trait de la position affichée par le parcours (en haut) ; sur la dernière position d'une partie finie, le résultat annoncé reste ---
        private void AfficheTraitParcours()
        {
            if (PartieTerminee && Session.EstSurLaDernierePosition)
                return;
            LabelInformationJoueur.Text = TraitAux(CouleurAuTrait);
        }
        private void BoutonPrecedent_Click(object sender, EventArgs e)
        {   // --- Bouton pour reculer d'un coup (parcours de partie) ---
            SessionPartie session = Session;
            if (session.Precedent())    // Faux si l'on est déjà au début
            {
                MiseenplaceFen(session.Fen(session.IndexAffiche), false);
                AfficheTraitParcours();
                LabelCoupJoue.Text = session.CoupAffiche != null ? T("Coup {0} : {1}", session.IndexAffiche, session.CoupAffiche) : T("Position initiale");
            }
            else
            {
                ReglagesDeTest.Message(T("Vous êtes au début de la partie."), "Début de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonSuivant_Click(object sender, EventArgs e)
        {   // --- Bouton pour avancer d'un coup (parcours de partie) ---
            // SessionPartie.IndexAffiche est toujours l'indice de la position affichée (0 : position de départ) : Suivant passe à la suivante, Précédent à la précédente
            SessionPartie session = Session;
            if (session.Suivant())      // Faux si l'on est déjà à la fin
            {
                MiseenplaceFen(session.Fen(session.IndexAffiche), false);
                AfficheTraitParcours();
                LabelCoupJoue.Text = T("Coup {0} : {1}", session.IndexAffiche, session.CoupAffiche);
            }
            else
            {
                ReglagesDeTest.Message(T("Vous êtes à la fin de la partie."), "Fin de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonDebut_Click(object sender, EventArgs e)
        {   // --- Bouton pour aller au début de la partie ---
            CouleurToutesCasesManoury(CouleurCaseSombre);       // On efface les couleurs des cases
            SessionPartie session = Session;
            if (session.Positions.Count > 1)    // Vérifie qu'il y a au moins un coup joué
            {
                session.Debut();                // La position de départ est à l'indice 0
                MiseenplaceFen(session.Fen(session.IndexAffiche), false);
                AfficheTraitParcours();
                LabelCoupJoue.Text = T("Début de partie");
            }
        }
        private void BoutonFin_Click(object sender, EventArgs e)
        {   // --- Bouton pour aller à la fin de la partie ---
            SessionPartie session = Session;
            if (session.Positions.Count > 1)    // Vérifie qu'il y a au moins un coup joué
            {
                session.Fin();                  // Dernière position
                MiseenplaceFen(session.Fen(session.IndexAffiche), false);
                AfficheTraitParcours();
                LabelCoupJoue.Text = T("Coup {0} : {1}", session.IndexAffiche, session.CoupAffiche);
            }
        }
        private async void OrdinateurJoue_Click(object sender, EventArgs e)
        {   // --- L'ordinateur joue son meilleur coup (sur la dernière position de la partie) ---
            if (RefuseSiPartieParcourue())
                return;
            await LancementReflexionMoteur(Session.Hub2Dernier(), TempsReflexion);
            ChangerCouleurTrait();      // Inverse le trait
            AnalyseCoupObligatoire(CouleurAuTrait);     // Vérifie si un coup obligatoire existe
        }

        private async void AnalysePosition_Click(object sender, EventArgs e)
        {
            LabelInformationJoueur.Text = T("Analyse de la position en cours ... ({0})", TempsReflexion);
            MarqueFleche(null);         // Pas de flèche de l'analyse précédente
            BloqueBoutons(true);
            analyseEnCours = true;
            await LancementReflexionMoteur(Session.Hub2(Session.IndexAffiche), TempsReflexion);
            LabelInformationJoueur.Text = T("Analyse terminée !");
            BloqueBoutons(false);
            analyseEnCours = false;
        }

        public void BoutonMasqueAffiche_Click(object? sender, EventArgs e)
        {   // --- Masque ou affiche la fenêtre de parties PDN ---
            if (fichierPartiePdn == null || fichierPartiePdn.IsDisposed)
            {   // Vérifier si la fenêtre a été fermée
                BoutonMasqueAffiche.Enabled = false;
                BoutonMasqueAffiche.Text = T("Affiche parties");
                return;     // La fenêtre a été fermée, on s'assure que le bouton affiche "Affiche parties" et on sort
            }
            if (fichierPartiePdn.Visible)
            {   // Si la fenêtre est actuellement visible, on la masque
                fichierPartiePdn.Hide();    // Masquer la fenêtre
                BoutonMasqueAffiche.Text = T("Affiche parties");  // Changer le texte du bouton
            }
            else
            {
                fichierPartiePdn.Show();    // Afficher la fenêtre
                BoutonMasqueAffiche.Text = T("Masque parties");  // Changer le texte du bouton
            }
        }
        // --- Le numéro de version de l'application : la balise <Version> du .csproj (1.11), avec le mineur sur deux chiffres ---
        private string VersionAffichee
        {
            get
            {
                Version? version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return version == null ? "?" : $"{version.Major}.{version.Minor:D2}";
            }
        }
        private void Apropos_Click(object sender, EventArgs e)
        {   // --- Affiche une boîte de dialogue avec les informations sur l'application ---
            // Le texte est écrit par une fonction : la fenêtre le réécrit quand on change la langue dans sa liste
            string Message() => T("Version Interface graphique = {0} / Auteur : Bruno Courtois\n", VersionAffichee) +
                             T("Moteur : {0} / Version = {1} / Auteur : {2}\n\n", nomMoteur, versionMoteur, auteurMoteur) +
                             T("Historique :\n") +
                             T("1.12 : thème bois, repères du dernier coup, lecture animée d'une partie, parcours au clavier, aide en anglais, animations plus vives, état du formulaire par instance.\n") +
                             T("1.11 : pendule (cadences FMJD), mise en page, parcours de partie sécurisé, tests de l'interface.\n") +
                             T("1.10 : règles refaites (prise majoritaire, rafles au choix), PDN complet, nulles, préférences, journal, .NET 10.\n") +
                             T("1.04 : première version publiée.");
            string choix = ReglagesDeTest.APropos(Message, langueChoisie ?? Texte.Langue);      // La fenêtre « À propos » propose aussi la langue
            if (choix != (langueChoisie ?? Texte.Langue))
            {   // Une autre langue : elle est retenue dans les préférences et s'applique au prochain démarrage
                langueChoisie = choix;
                Journal.Info("Langue choisie : " + choix);
            }
        }
        private void BoutonAide_Click(object sender, EventArgs e)
        {   // --- Ouvre l'aide (page HTML livrée à côté de l'exe) dans le navigateur : en anglais si Windows est en anglais, sinon en français (chaque page a un lien vers l'autre) ---
            string PageAide(string langue) => Path.Combine(Application.StartupPath, "Aide", langue, "index.html");
            string page = PageAide(LangueAide.Choisit(System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, langue => File.Exists(PageAide(langue))));
            if (!File.Exists(page))
            {
                Journal.Attention("Aide introuvable : " + page);
                ReglagesDeTest.Message(T("Le fichier d'aide est introuvable :\n{0}", page), "Aide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(page) { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception || ex is InvalidOperationException)
            {
                Journal.Attention("Aide : ouverture impossible (" + ex.Message + ")");
                ReglagesDeTest.Message(T("L'aide n'a pas pu être ouverte :\n{0}", ex.Message), "Aide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {   // --- F1 ouvre l'aide ; les flèches gauche et droite, Début et Fin parcourent la partie ---
            if (keyData == Keys.F1)
            {
                BoutonAide_Click(this, EventArgs.Empty);
                return true;
            }
            if (ToucheParcours(keyData) || ToucheRaccourci(keyData))
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
        // --- Vrai quand le focus est dans une zone de saisie, une liste ou un curseur, où les touches ont leur propre rôle ---
        private bool EstEnSaisie()
        {
            Control? actif = ActiveControl;
            while (actif is ContainerControl conteneur && conteneur.ActiveControl != null)
                actif = conteneur.ActiveControl;
            string typeActif = actif?.GetType().Name ?? "";
            return typeActif.Contains("TextBox") || typeActif.Contains("ComboBox") || typeActif.Contains("TrackBar") || typeActif.Contains("UpDown");
        }
        // --- Le parcours au clavier, comme les boutons : ← Préc., → Suiv., Début, Fin. Rend vrai si la touche a servi. Sans effet quand les flèches ont un autre rôle : saisie d'un nom, liste ou curseur
        // qui ont le focus, pièce en main, lecture en cours ou boutons bloqués ---
        private bool ToucheParcours(Keys touche)
        {
            if (touche != Keys.Left && touche != Keys.Right && touche != Keys.Home && touche != Keys.End)
                return false;
            if (EstEnSaisie())
                return false;
            if (!groupParcoursPartie.Enabled || lecture != null || pieceEnMain)
                return false;
            // Aux extrémités, les flèches ne font rien (les boutons, eux, l'expliquent par un message : trop pesant pour une touche qu'on répète)
            if (touche == Keys.Left) { if (Session.IndexAffiche > 0) BoutonPrecedent_Click(this, EventArgs.Empty); }
            else if (touche == Keys.Right) { if (!Session.EstSurLaDernierePosition) BoutonSuivant_Click(this, EventArgs.Empty); }
            else if (touche == Keys.Home) BoutonDebut_Click(this, EventArgs.Empty);
            else BoutonFin_Click(this, EventArgs.Empty);
            return true;
        }        private void SauvePartiesPdn_Click(object sender, EventArgs e)
        {   // --- Ouvre la feneêtre de sauvegarde de fichier ---
            affichePdn?.LancerEnregistrementPdn();
        }
        private void PositionFen_Click(object sender, EventArgs e)
        {   // --- Charge une position au format FEN à partir d'un fichier  ---
            if (ChargerFichierFen.ShowDialog() != DialogResult.OK)
                return;         // Rien n'a été touché : la partie en cours reste telle quelle
            string contenuFichier;
            try
            {   // Le fichier est lu et la position vérifiée avant de toucher à la partie
                contenuFichier = File.ReadAllText(ChargerFichierFen.FileName).Trim();
                Position.DepuisFen(contenuFichier);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is FormatException)
            {
                // Le message d'une FEN invalide recopie le contenu du fichier : le journal n'en garde que le début
                string resume = ex is FormatException ? "contenu qui n'est pas une position FEN (" + new FileInfo(ChargerFichierFen.FileName).Length + " octets)" : ex.Message;
                Journal.Erreur($"Position FEN non chargée ({ChargerFichierFen.FileName}) : {resume}");
                string pourLeJoueur = ex is FormatException ? T("Ce fichier ne contient pas de position FEN valide.") : ex.Message;
                ReglagesDeTest.Message(T("La position n'a pas pu être chargée :\n{0}\n\n{1}", Path.GetFileName(ChargerFichierFen.FileName), pourLeJoueur), "Position FEN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!contenuFichier.Contains('"'))
                contenuFichier = "[FEN \"" + contenuFichier + "\"]";        // Un FEN sans l'enrobage [FEN "..."] est accepté
            NettoyageRapide();
            BalisesPartie.White =BalisesPartie.Black = BalisesPartie.Result = BalisesPartie.Round = BalisesPartie.Site = BalisesPartie.Event = "?";
            MiseenplaceFen(contenuFichier, true);   // Met en place la position
            AfficheListesPieces();
            ReinitialisePendule();
            AnalyseCoupObligatoire(CouleurAuTrait);     // Vérifie si un coup obligatoire existe
        }        private void SauvePositionFen_Click(object sender, EventArgs e)
        {   // --- Sauvegarde la position au format FEN dans un fichier texte ---
            if (SauverFichierFen.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string fen = RecupereFEN();
                    File.WriteAllText(SauverFichierFen.FileName, fen);
                    ReglagesDeTest.Message(T("Position FEN sauvegardée avec succès\ndans le fichier : {0} !", Path.GetFileName(SauverFichierFen.FileName)),
                                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Journal.Erreur($"Sauvegarde de la position FEN ({SauverFichierFen.FileName})", ex);
                    ReglagesDeTest.Message(T("Erreur lors de la sauvegarde : {0}", ex.Message), "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void BoxNomJoueurBlanc_TextChanged(object sender, EventArgs e)
        {   // --- Gestion du changement de nom du joueur blanc ---
            NomJoueurBlanc = BalisesPartie.White = BoxNomJoueurBlanc.Text;
            if (!nomsDepuisPartie)
                nomBlancPrefere = BoxNomJoueurBlanc.Text;
        }
        private void BoxNomJoueurNoir_TextChanged(object sender, EventArgs e)
        {   //  --- Gestion du changement de nom du joueur noir ---
            NomJoueurNoir = BalisesPartie.Black = BoxNomJoueurNoir.Text;
            if (!nomsDepuisPartie)
                nomNoirPrefere = BoxNomJoueurNoir.Text;
        }
        private void trackBarTempsReflexion_ValueChanged(object sender, EventArgs e)
        {   // --- Gestion du trackBar pour le temps de réflexion de l'ordinateur ---
            LabelTempsReflexion.Text = trackBarTempsReflexion.Value.ToString() + " s";
            TempsReflexion = trackBarTempsReflexion.Value;
        }
        private void SonEmis_CheckedChanged(object sender, EventArgs e)
        {   // Le son est émis si la case est cochée
            EmetUnSon = SonEmis.Checked;
        }
        private void RadioGainBlanc_CheckedChanged(object sender, EventArgs e)
        {   // --- Gestion du bouton radio gain blanc ---
            if (RadioGainBlanc.Checked)
            {   // 1-0 les blancs gagnent
                LabelInformationJoueur.Text = T("Gain Blanc sélectionné (Partie terminée)");
                MiseaJourFinPartie(" 1-0");
            }
        }
        private void RadioGainNoir_CheckedChanged(object sender, EventArgs e)
        {   // --- Gestion du bouton radio gain noir ---
            if (RadioGainNoir.Checked)
            {   // 0-1 les noirs gagnent
                LabelInformationJoueur.Text = T("Gain Noir sélectionné (Partie terminée)");
                MiseaJourFinPartie(" 0-1");
            }
        }
        private void RadioNulle_CheckedChanged(object sender, EventArgs e)
        {   // --- Gestion du bouton radio partie nulle ---
            if (RadioNulle.Checked)
            {   // 1/2-1/2 partie nulle
                LabelInformationJoueur.Text = T("Nulle sélectionnée (Partie terminée)");
                MiseaJourFinPartie(" 1/2-1/2");
            }
        }
        private void MiseaJourFinPartie(string resultat)
        {   // --- Met à jour certains éléments à la fin de la partie ---
            Session.FixeResultat(resultat);     // Le résultat entre dans la partie (il est écrit à la fin des coups PDN)
            BalisesPartie.Event = "Entrainement";
            BalisesPartie.Site = "Interface graphique";
            BalisesPartie.Date = DateTime.Now.ToString("yyyy-MM-dd");      // Stocke la date au format AAAA-MM-JJ
            BalisesPartie.White = NomJoueurBlanc;      // Mise à jour des balises de la partie
            BalisesPartie.Black = NomJoueurNoir;
            BalisesPartie.Result = resultat.Trim(); 
            GroupResultat.Enabled = false;   // Désactive le groupe de boutons
            PartieTerminee = true;
            ArretePendule();                // Le temps s'arrête
        }
        private void BoutonRetourArriere_Click(object sender, EventArgs e)
        {   // --- Retour en arrière d'un coup : la session retire le dernier coup (et le résultat s'il y en a un), la position précédente est remise sur le damier ---
            SessionPartie session = Session;
            string? coupEfface = session.RetireDernierCoup();
            if (coupEfface == null)
            {
                ReglagesDeTest.Message(T("Impossible de revenir en arrière :\nAucun coup n'a été joué."),
                                "Retour arrière", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            Damier.ViderDamier();
            MiseenplaceFen(session.Fen(session.IndexAffiche), false);       // Met en place la nouvelle dernière position
            AfficheListesPieces();
            NettoyageRapide();
            AfficheTraitParcours();     // NettoyageRapide a vidé l'affichage du trait (une prise obligatoire le réécrit plus bas)
            ArmePendule();      // La pendule garde les temps et attend le prochain coup
            LabelCoupJoue.Text = T("Mouvement effacé = {0}", coupEfface); // Met à jour le label avec le mouvement.
            AnalyseCoupObligatoire(CouleurAuTrait);
        }        private void BoutonTournerDamier_Click(object sender, EventArgs e)
        {   // --- Rotation du damier de 180° : l'orientation change, CaseDamier suit les places à l'écran (la case i va à la place 99 - i) ---
            Damier.Orientation.Tourner();
            CaseDamier.Reverse();
            for (int place = 0; place < CaseDamier.Count; place++)
                CaseDamier[place].Location = new Point(20 + (place % 10) * 60, 20 + (place / 10) * 60);
            Damier10x10.Refresh();      // Actualiser l'affichage
        }        private void BoutonQuitterApplication_Click(object sender, EventArgs e)
        {   // --- Ferme et quitte l'appliccation proprement ---
            Close();        // OnFormClosing arrête le moteur
        }
    }
}
