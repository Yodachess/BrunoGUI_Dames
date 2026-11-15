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
            OrdinateurActif = false;
            OrdinateurBosse.Checked = false;
            OrdinateurBosse.Enabled = false;
            OrdinateurJoue.Enabled = false;
            AnalysePosition.Enabled = false;
            LabelAfficheScan.Text = message;
        }
        private static int VersRgb(Color couleur) => couleur.ToArgb() & 0xFFFFFF;
        private static Color DepuisRgb(int rgb) => Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        private void ChargePreferences()
        {   // --- Lit BrunoGUI_Dames.preferences.ini (les avertissements vont au journal) et applique les couleurs du damier et la durée d'animation ---
            preferencesChargees = Preferences.Lire(FichierPreferences);
            foreach (string avertissement in preferencesChargees.Avertissements)
                Journal.Ecrit("Préférences : " + avertissement);
            DureeAnimation = preferencesChargees.DureeAnimation;
            Color Choisie(string nom, Color parDefaut) => preferencesChargees.Couleurs.TryGetValue(nom, out int rgb) ? DepuisRgb(rgb) : parDefaut;
            CouleurCaseSombre = Choisie("CaseSombre", CouleurCaseSombre);
            CouleurCaseClaire = Choisie("CaseClaire", CouleurCaseClaire);
            CouleurCaseDepart = Choisie("CaseDepart", CouleurCaseDepart);
            CouleurTrajetSuivi = Choisie("TrajetSuivi", CouleurTrajetSuivi);
            CouleurCasePivot = Choisie("CasePivot", CouleurCasePivot);
            CouleurCasePrise = Choisie("CasePrise", CouleurCasePrise);
            CouleurCaseArrivee = Choisie("CaseArrivee", CouleurCaseArrivee);
            CouleurCaseEtape = Choisie("CaseEtape", CouleurCaseEtape);
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
        }
        private void EnregistrePreferences()
        {   // --- Écrit les préférences personnelles dans BrunoGUI_Dames.preferences.ini, à côté de l'exe ---
            Preferences aEnregistrer = new Preferences
            {
                TempsReflexion = trackBarTempsReflexion.Value,
                DureeAnimation = DureeAnimation,
                VueCoteNoir = GestionDamier.Orientation.CoteNoir,
                NomJoueurBlanc = nomBlancPrefere,
                NomJoueurNoir = nomNoirPrefere,
                SonEmis = SonEmis.Checked,
                OrdinateurJoue = moteurDisponible ? OrdinateurBosse.Checked : preferencesChargees.OrdinateurJoue     // Sans moteur la case est grisée : on garde le choix précédent
            };
            aEnregistrer.Couleurs["CaseSombre"] = VersRgb(CouleurCaseSombre);
            aEnregistrer.Couleurs["CaseClaire"] = VersRgb(CouleurCaseClaire);
            aEnregistrer.Couleurs["CaseDepart"] = VersRgb(CouleurCaseDepart);
            aEnregistrer.Couleurs["TrajetSuivi"] = VersRgb(CouleurTrajetSuivi);
            aEnregistrer.Couleurs["CasePivot"] = VersRgb(CouleurCasePivot);
            aEnregistrer.Couleurs["CasePrise"] = VersRgb(CouleurCasePrise);
            aEnregistrer.Couleurs["CaseArrivee"] = VersRgb(CouleurCaseArrivee);
            aEnregistrer.Couleurs["CaseEtape"] = VersRgb(CouleurCaseEtape);
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
                Journal.Ecrit("Son impossible : " + ex.Message);
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {   // --- À la fermeture de la fenêtre, par la croix comme par « Quitter », on enregistre les préférences et on arrête le moteur (sinon scan.exe resterait actif) ---
            base.OnFormClosing(e);
            if (!e.Cancel)
            {
                EnregistrePreferences();
                MoteurDamesScan.Quitte();
            }
        }
    }
}
