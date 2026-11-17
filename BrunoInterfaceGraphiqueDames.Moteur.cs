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
        // Affichage des informations du moteur, préférences, son et fermeture de la fenêtre.
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
                Journal.Attention($"Affichage d'une ligne du moteur impossible ({ex.Message}) : {ligne}");
            }
        }
        private void AnalyseLigneMoteur(string ligne)
        {   // --- Ce que la ligne du moteur apporte à l'affichage : identité, score, variation principale (la lecture est dans LigneMoteur.cs) ---
            LigneMoteurLue lue = LigneMoteur.Lire(ligne);
            if (lue.EstId)
            {
                LabelAfficheScan.Text = "Moteur : " + lue.Nom + " / Version : " + lue.Version + " / Auteur : " + lue.Auteur + " / Pays : " + lue.Pays;
                nomMoteur = lue.Nom ?? "";
                versionMoteur = lue.Version ?? "";
                auteurMoteur = lue.Auteur ?? "";
            }
            if (lue.Score is decimal score)
                LabelScore.Text = "Score = " + score.ToString("F2", CultureInfo.InvariantCulture);     // Affichage avec le format "XX.YY"
            if (lue.Variation is string variationMoteur)
            {
                if (analyseEnCours)
                {
                    LabelPrisesPossibles.Text = "Ligne SCAN 3.1 complète : " + variationMoteur;
                    LabelCoupJoue.Text = "Coup suggéré par l'analyse : " + variationMoteur.Split(' ')[0] + (lue.Score != null && lue.Profondeur == null ? " (livre)" : "");
                    MarqueFleche(variationMoteur.Split(' ')[0]);       // Une flèche sur le damier ; elle suit la variation à chaque ligne du moteur
                }
                LabelAfficheScan.Text = "Ligne Scan3.1 = " + (variationMoteur.Length > 100 ? variationMoteur.Substring(0, 100) : variationMoteur);     // Limitée pour rester dans le label
            }
        }        private void AfficheDonneesBrutes(string ligne, bool venantDuMoteur)
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
                vue.AppendText(Environment.NewLine + "[SCAN 3.1]    " + ligne);       // Même séparation que pour les lignes de l'interface
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
            OrdinateurBosse.Checked = false;
            OrdinateurBosse.Enabled = false;
            OrdinateurJoue.Enabled = false;
            AnalysePosition.Enabled = false;
            comboNiveau.Enabled = false;
            LabelAfficheScan.Text = message;
        }
        // --- La force du moteur : une liste de six niveaux (NiveauMoteur) dans le cadre « Pendule et temps de réflexion », entre la cadence et le curseur du temps (qui sont resserrés) ---
        private readonly ComboBox comboNiveau = new ComboBox();
        private int NiveauChoisi => NiveauMoteur.Limite(comboNiveau.SelectedIndex + 1);
        private void CreeNiveauMoteur()
        {
            comboNiveau.Name = "ComboNiveau";
            comboNiveau.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCadence.Width = 132;
            comboNiveau.Location = new Point(142, 17);
            comboNiveau.Width = 78;
            for (int niveau = NiveauMoteur.Minimum; niveau <= NiveauMoteur.Maximum; niveau++)
                comboNiveau.Items.Add(NiveauMoteur.Nom(niveau));
            comboNiveau.SelectedIndex = NiveauMoteur.ParDefaut - 1;
            comboNiveau.SelectedIndexChanged += (s, e) => infobulleNiveau.SetToolTip(comboNiveau, NiveauMoteur.Libelle(NiveauChoisi));
            infobulleNiveau.SetToolTip(comboNiveau, NiveauMoteur.Libelle(NiveauChoisi));
            groupBox1.Controls.Add(comboNiveau);
            trackBarTempsReflexion.Location = new Point(223, 16);
            trackBarTempsReflexion.Size = new Size(59, 21);
            LabelTempsReflexion.Location = new Point(284, 18);
            LabelTempsReflexion.Size = new Size(34, 21);
            LabelTempsReflexion.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            LabelTempsReflexion.TextAlign = ContentAlignment.MiddleLeft;
            LabelTempsReflexion.AutoSize = false;
        }
        private readonly ToolTip infobulleNiveau = new ToolTip();
        private static int VersRgb(Color couleur) => couleur.ToArgb() & 0xFFFFFF;
        private static Color CouleurParDefaut(string nom) => DepuisRgb(Preferences.CouleursParDefaut[nom]);
        private static Color DepuisRgb(int rgb) => Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        private void ChargePreferences()
        {   // --- Lit BrunoGUI_Dames.preferences.ini (les avertissements vont au journal) et applique les couleurs du damier et la durée d'une étape d'animation ---
            preferencesChargees = Preferences.Lire(FichierPreferences);
            foreach (string avertissement in preferencesChargees.Avertissements)
                Journal.Attention("Préférences : " + avertissement);
            DureeEtape = preferencesChargees.DureeEtape;
            ArriveesDebutTour = preferencesChargees.ArriveesDebutTour;
            RepereDernierCoup = preferencesChargees.RepereDernierCoup;
            Color Choisie(string nom) => preferencesChargees.Couleurs.TryGetValue(nom, out int rgb) ? DepuisRgb(rgb) : CouleurParDefaut(nom);
            CouleurCaseSombre = Choisie("CaseSombre");
            CouleurCaseClaire = Choisie("CaseClaire");
            CouleurCaseDepart = Choisie("CaseDepart");
            CouleurTrajetSuivi = Choisie("TrajetSuivi");
            CouleurCasePivot = Choisie("CasePivot");
            CouleurCasePrise = Choisie("CasePrise");
            CouleurCaseArrivee = Choisie("CaseArrivee");
            CouleurCaseEtape = Choisie("CaseEtape");
        }
        private void AppliquePreferences()
        {   // --- Applique les préférences enregistrées (temps de réflexion, noms, son, ordinateur qui joue) ; les valeurs absentes ou refusées gardent leur valeur par défaut ---
            Preferences preferences = preferencesChargees;      // Lues par ChargePreferences
            trackBarTempsReflexion.Value = Math.Clamp(preferences.TempsReflexion, trackBarTempsReflexion.Minimum, trackBarTempsReflexion.Maximum);   // ValueChanged met à jour le libellé et TempsReflexion
            if (preferences.NomJoueurBlanc != null)
                BoxNomJoueurBlanc.Text = preferences.NomJoueurBlanc;
            if (preferences.NomJoueurNoir != null)
                BoxNomJoueurNoir.Text = preferences.NomJoueurNoir;
            nomBlancPrefere = BoxNomJoueurBlanc.Text;
            nomNoirPrefere = BoxNomJoueurNoir.Text;
            SonEmis.Checked = preferences.SonEmis;
            OrdinateurBosse.Checked = preferences.OrdinateurJoue;
            SelectionneCadence(preferences.CadenceId);      // Sans pendule par défaut
            trackBarVitesseLecture.Value = RythmeLecture.Limite(preferences.VitesseLecture);
            comboNiveau.SelectedIndex = NiveauMoteur.Limite(preferences.Niveau) - 1;
        }
        private void EnregistrePreferences()
        {   // --- Écrit les préférences personnelles dans BrunoGUI_Dames.preferences.ini, à côté de l'exe ---
            Preferences aEnregistrer = new Preferences
            {
                TempsReflexion = trackBarTempsReflexion.Value,
                Niveau = NiveauChoisi,
                DureeEtape = DureeEtape,
                ArriveesDebutTour = ArriveesDebutTour,
                RepereDernierCoup = RepereDernierCoup,
                VueCoteNoir = Damier.Orientation.CoteNoir,
                NomJoueurBlanc = nomBlancPrefere,
                NomJoueurNoir = nomNoirPrefere,
                SonEmis = SonEmis.Checked,
                CadenceId = CadenceChoisieId,
                VitesseLecture = trackBarVitesseLecture.Value,
                OrdinateurJoue = moteurDisponible ? OrdinateurBosse.Checked : preferencesChargees.OrdinateurJoue     // Sans moteur la case est grisée : on garde le choix précédent
            };
            // Seules les couleurs que le joueur a changées sont écrites : les autres suivent les valeurs par défaut du programme, qui peuvent évoluer d'une version à l'autre
            void EcritSiChangee(string nom, Color couleur) { if (VersRgb(couleur) != Preferences.CouleursParDefaut[nom]) aEnregistrer.Couleurs[nom] = VersRgb(couleur); }
            EcritSiChangee("CaseSombre", CouleurCaseSombre);
            EcritSiChangee("CaseClaire", CouleurCaseClaire);
            EcritSiChangee("CaseDepart", CouleurCaseDepart);
            EcritSiChangee("TrajetSuivi", CouleurTrajetSuivi);
            EcritSiChangee("CasePivot", CouleurCasePivot);
            EcritSiChangee("CasePrise", CouleurCasePrise);
            EcritSiChangee("CaseArrivee", CouleurCaseArrivee);
            EcritSiChangee("CaseEtape", CouleurCaseEtape);
            aEnregistrer.Ecrire(FichierPreferences);
        }
        private static void JoueSonCoupJoue()
        {   // --- Joue le son de Windows « Windows Notify » (dossier Windows lu dans le système) ; à défaut, un son système ---
            try
            {
                string fichier = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media", "Windows Notify.wav");
                if (File.Exists(fichier))
                    new SoundPlayer(fichier).Play();
                else
                    System.Media.SystemSounds.Asterisk.Play();
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is UnauthorizedAccessException)
            {
                Journal.Attention("Son impossible : " + ex.Message);
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {   // --- À la fermeture de la fenêtre, par la croix comme par « Quitter », on enregistre les préférences et on arrête le moteur (sinon scan.exe resterait actif) ---
            base.OnFormClosing(e);
            if (!e.Cancel)
            {
                minuteriePendule.Stop();
                lecture?.Cancel();
                EnregistrePreferences();
                moteur.Quitte();
            }
        }
    }
}
