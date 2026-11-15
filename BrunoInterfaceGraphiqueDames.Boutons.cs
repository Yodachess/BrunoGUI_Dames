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
        // Boutons, parcours de la partie, options et retour arrière.
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des boutons
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void NouvellePartie_Click(object sender, EventArgs e)
        {   // --- Initialisation du damier et mise à zéro des diverses listes --- 
            GestionDamier.ViderDamier();
            GestionDamier.InitialiserDamier();  // met la position initiale dans PositionCourante SANS afficher les pièces
            ExisteFENDebut = false;
            ActiveDamier(true);
            NettoyageRapide();
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
            SessionPartie session = Partie.Courante;
            if (session.Precedent())    // Faux si l'on est déjà au début
            {
                MiseenplaceFen(session.Fen(session.IndexAffiche), false);
                LabelCoupJoue.Text = session.CoupAffiche != null ? $"Coup {session.IndexAffiche} : {session.CoupAffiche}" : "Position initiale";
            }
            else
            {
                MessageBox.Show("Vous êtes au début de la partie.", "Début de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonSuivant_Click(object sender, EventArgs e)
        {   // --- Bouton pour avancer d'un coup (parcours de partie) ---
            // SessionPartie.IndexAffiche est toujours l'indice de la position affichée (0 : position de départ) : Suivant passe à la suivante, Précédent à la précédente
            SessionPartie session = Partie.Courante;
            if (session.Suivant())      // Faux si l'on est déjà à la fin
            {
                MiseenplaceFen(session.Fen(session.IndexAffiche), false);
                LabelCoupJoue.Text = $"Coup {session.IndexAffiche} : {session.CoupAffiche}";
            }
            else
            {
                MessageBox.Show("Vous êtes à la fin de la partie.", "Fin de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonDebut_Click(object sender, EventArgs e)
        {   // --- Bouton pour aller au début de la partie ---
            CouleurToutesCasesManoury(CouleurCaseSombre);       // On efface les couleurs des cases
            SessionPartie session = Partie.Courante;
            if (session.Positions.Count > 1)    // Vérifie qu'il y a au moins un coup joué
            {
                session.Debut();                // La position de départ est à l'indice 0
                MiseenplaceFen(session.Fen(session.IndexAffiche), false);
                LabelCoupJoue.Text = "Début de partie";
            }
        }
        private void BoutonFin_Click(object sender, EventArgs e)
        {   // --- Bouton pour aller à la fin de la partie ---
            SessionPartie session = Partie.Courante;
            if (session.Positions.Count > 1)    // Vérifie qu'il y a au moins un coup joué
            {
                session.Fin();                  // Dernière position
                MiseenplaceFen(session.Fen(session.IndexAffiche), false);
                LabelCoupJoue.Text = $"Coup {session.IndexAffiche} : {session.CoupAffiche}";
            }
        }
        private async void OrdinateurJoue_Click(object sender, EventArgs e)
        {   // --- L'ordinateur joue son meilleur coup ---
            await LancementReflexionMoteur(Partie.Courante.Hub2Dernier(), TempsReflexion);
            OrdinateurActif = true;
            ChangerCouleurTrait();      // Inverse le trait
            AnalyseCoupObligatoire(CouleurAuTrait);     // Vérifie si un coup obligatoire existe
        }

        private async void AnalysePosition_Click(object sender, EventArgs e)
        {
            LabelInformationJoueur.Text = "Analyse de la position en cours ... (" + TempsReflexion + ")";
            BloqueBoutons(true);
            analyseEnCours = true;
            await LancementReflexionMoteur(Partie.Courante.Hub2(Partie.Courante.IndexAffiche), TempsReflexion);
            LabelInformationJoueur.Text = "Analyse terminée !";
            BloqueBoutons(false);
            analyseEnCours = false;
        }

        public void BoutonMasqueAffiche_Click(object? sender, EventArgs e)
        {   // --- Masque ou affiche la fenêtre de parties PDN ---
            if (fichierPartiePdn == null || fichierPartiePdn.IsDisposed)
            {   // Vérifier si la fenêtre a été fermée
                BoutonMasqueAffiche.Enabled = false;
                BoutonMasqueAffiche.Text = "Affiche parties";
                return;     // La fenêtre a été fermée, on s'assure que le bouton affiche "Affiche parties" et on sort
            }
            if (fichierPartiePdn.Visible)
            {   // Si la fenêtre est actuellement visible, on la masque
                fichierPartiePdn.Hide();    // Masquer la fenêtre
                BoutonMasqueAffiche.Text = "Affiche parties";  // Changer le texte du bouton
            }
            else
            {
                fichierPartiePdn.Show();    // Afficher la fenêtre
                BoutonMasqueAffiche.Text = "Masque parties";  // Changer le texte du bouton
            }
        }
        // --- Le numéro de version de l'application : la balise <Version> du .csproj (1.10), avec le mineur sur deux chiffres ---
        private static string VersionAffichee
        {
            get
            {
                Version? version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return version == null ? "?" : $"{version.Major}.{version.Minor:D2}";
            }
        }
        private void Apropos_Click(object sender, EventArgs e)
        {   // --- Affiche une boîte de dialogue avec les informations sur l'application ---
            string message = $"Version Interface graphique = {VersionAffichee} / Auteur : Bruno Courtois\n" +
                                $"Moteur : {nomMoteur} / Version = {versionMoteur} / Auteur : {auteurMoteur}\n\n" +
                                "Historique :\n" +
                                "1.10 : règles refaites (prise majoritaire, rafles au choix), PDN complet, nulles, préférences, journal, .NET 10.\n" +
                                "1.04 : première version publiée.";
            MessageBox.Show(message, "À propos", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void BoutonAide_Click(object sender, EventArgs e)
        {   // --- Ouvre l'aide (page HTML livrée à côté de l'exe) dans le navigateur ---
            string page = Path.Combine(Application.StartupPath, "Aide", "fr", "index.html");
            if (!File.Exists(page))
            {
                Journal.Ecrit("Aide introuvable : " + page);
                MessageBox.Show("Le fichier d'aide est introuvable :\n" + page, "Aide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(page) { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception || ex is InvalidOperationException)
            {
                Journal.Ecrit("Aide : ouverture impossible (" + ex.Message + ")");
                MessageBox.Show("L'aide n'a pas pu être ouverte :\n" + ex.Message, "Aide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {   // --- F1 ouvre l'aide ---
            if (keyData == Keys.F1)
            {
                BoutonAide_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
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
                Journal.Ecrit($"ERREUR position FEN non chargée ({ChargerFichierFen.FileName}) : {resume}");
                string pourLeJoueur = ex is FormatException ? "Ce fichier ne contient pas de position FEN valide." : ex.Message;
                MessageBox.Show($"La position n'a pas pu être chargée :\n{Path.GetFileName(ChargerFichierFen.FileName)}\n\n{pourLeJoueur}", "Position FEN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!contenuFichier.Contains('"'))
                contenuFichier = "[FEN \"" + contenuFichier + "\"]";        // Un FEN sans l'enrobage [FEN "..."] est accepté
            NettoyageRapide();
            PartieCourante.White =PartieCourante.Black = PartieCourante.Result = PartieCourante.Round = PartieCourante.Site = PartieCourante.Event = "?";
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
                    MessageBox.Show($"Position FEN sauvegardée avec succès\ndans le fichier : {Path.GetFileName(SauverFichierFen.FileName)} !",
                                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Journal.Ecrit($"ERREUR sauvegarde de la position FEN ({SauverFichierFen.FileName}) : {ex.Message}");
                    MessageBox.Show("Erreur lors de la sauvegarde : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void BoxNomJoueurBlanc_TextChanged(object sender, EventArgs e)
        {   // --- Gestion du changement de nom du joueur blanc ---
            NomJoueurBlanc = PartieCourante.White = BoxNomJoueurBlanc.Text;
            if (!nomsDepuisPartie)
                nomBlancPrefere = BoxNomJoueurBlanc.Text;
        }
        private void BoxNomJoueurNoir_TextChanged(object sender, EventArgs e)
        {   //  --- Gestion du changement de nom du joueur noir ---
            NomJoueurNoir = PartieCourante.Black = BoxNomJoueurNoir.Text;
            if (!nomsDepuisPartie)
                nomNoirPrefere = BoxNomJoueurNoir.Text;
        }
        private void trackBarTempsReflexion_ValueChanged(object sender, EventArgs e)
        {   // --- Gestion du trackBar pour le temps de réflexion de l'ordinateur ---
            LabelTempsReflexion.Text = trackBarTempsReflexion.Value.ToString() + " sec.";
            TempsReflexion = trackBarTempsReflexion.Value;
        }
        private void OrdinateurBosse_CheckedChanged(object sender, EventArgs e)
        {   // L'ordinateur joue si la case est cochée (avant, la valeur était basculée, et pouvait se désynchroniser de la case)
            OrdinateurActif = OrdinateurBosse.Checked;
        }
        private void SonEmis_CheckedChanged(object sender, EventArgs e)
        {   // Le son est émis si la case est cochée
            EmetUnSon = SonEmis.Checked;
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
            Partie.Courante.FixeResultat(resultat);     // Le résultat entre dans la partie (il est écrit à la fin des coups PDN)
            PartieCourante.Event = "Entrainement";
            PartieCourante.Site = "Interface graphique";
            PartieCourante.Date = DateTime.Now.ToString("yyyy-MM-dd");      // Stocke la date au format AAAA-MM-JJ
            PartieCourante.White = NomJoueurBlanc;      // Mise à jour des balises de la partie
            PartieCourante.Black = NomJoueurNoir;
            PartieCourante.Result = resultat.Trim(); 
            GroupResultat.Enabled = false;   // Désactive le groupe de boutons
            PartieTerminee = true;
            ArretePendule();                // Le temps s'arrête
        }
        private void BoutonRetourArriere_Click(object sender, EventArgs e)
        {   // --- Retour en arrière d'un coup : la session retire le dernier coup (et le résultat s'il y en a un), la position précédente est remise sur le damier ---
            SessionPartie session = Partie.Courante;
            string? coupEfface = session.RetireDernierCoup();
            if (coupEfface == null)
            {
                MessageBox.Show("Impossible de revenir en arrière :\nAucun coup n'a été joué.",
                                "Retour arrière", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            GestionDamier.ViderDamier();
            MiseenplaceFen(session.Fen(session.IndexAffiche), false);       // Met en place la nouvelle dernière position
            AfficheListesPieces();
            NettoyageRapide();
            ArmePendule();      // La pendule garde les temps et attend le prochain coup
            LabelCoupJoue.Text = $"Mouvement effacé = {coupEfface}"; // Met à jour le label avec le mouvement.
            AnalyseCoupObligatoire(CouleurAuTrait);
            OrdinateurActif = false;
        }        private void BoutonTournerDamier_Click(object sender, EventArgs e)
        {   // --- Rotation du damier de 180° : l'orientation change, CaseDamier suit les places à l'écran (la case i va à la place 99 - i) ---
            GestionDamier.Orientation.Tourner();
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
