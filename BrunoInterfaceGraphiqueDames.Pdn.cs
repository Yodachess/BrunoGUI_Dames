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
        // Chargement des parties PDN et déroulé d'une partie sur le damier.
        private void ChargePartiesPdn_Click(object sender, EventArgs e)
        {   // --- Affiche la boîte de dialogue et traite le fichier PDN sélectionné  ---
            if (ChargerPartiesPdn.ShowDialog() == DialogResult.OK)
            {
                string cheminFichier = ChargerPartiesPdn.FileName;
                try
                {   // Vérifie et obtient le chemin complet
                    string fullPath = Path.GetFullPath(cheminFichier);
                    List<PartieDamesPdn> partiesLues = FichierPartiePdn.LireParties(fullPath);       // Lit toutes les parties du fichier PDN
                    string? raison = FichierPartiePdn.RaisonFichierNonUtilisable(partiesLues);
                    if (raison != null)
                    {   // Pas de partie avec des coups (image, position FEN…) : on le dit et on ne touche à rien
                        Journal.Ecrit($"Fichier inutilisable comme liste de parties PDN ({cheminFichier}) : {raison.Replace("\n", " ")}");
                        MessageBox.Show($"{Path.GetFileName(cheminFichier)}\n\n{raison}", "Parties PDN", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    ListePartiesPdn.Clear(); // On vide la liste des parties PDN
                    NettoyageRapide();
                    if (fichierPartiePdn == null || fichierPartiePdn.IsDisposed)
                    {   // Traitement pour prendre en compte la fermeture par croix rouge en haut à droite ...
                        fichierPartiePdn = new FichierPartiePdn();
                        fichierPartiePdn.FormClosed += (s, args) =>
                        {   // On évite que la référence de affichePdn pointe vers un objet supprimé.
                            fichierPartiePdn = null;
                        };
                    }
                    ListePartiesPdn.AddRange(partiesLues);
                    fichierPartiePdn.NombrePartiesFichier.Text = ListePartiesPdn.Count.ToString()
                        + " partie(s) dans le fichier  " + cheminFichier.Substring(cheminFichier.LastIndexOf('\\') + 1); ;
                    fichierPartiePdn.AfficherListeParties(ListePartiesPdn); 
                    fichierPartiePdn.Show();
                    BoutonMasqueAffiche.Enabled = true; // Active le bouton pour masquer/afficher la liste
                }
                catch (Exception ex)
                {
                    Journal.Ecrit($"ERREUR chargement des parties PDN ({cheminFichier}) : {ex}");
                    MessageBox.Show($"Le fichier PDN n'a pas pu être lu :\n{ex.Message}", "Parties PDN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private bool rejeuPdnEnCours;      // Vrai pendant qu'une partie PDN est rejouée sur le damier
        public async void ChargerPartieDepuisPdn(PartieDamesPdn partie)
        {   // --- Charge une partie depuis un fichier PDN lorsque'on double-clique ---
            if (rejeuPdnEnCours || jeuMoteurEnCours || analyseEnCours)
            {
                MessageBox.Show("Une partie est déjà en cours de chargement, ou le moteur réfléchit.\nAttends la fin avant d'en charger une autre.", "Chargement impossible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // La partie est d'abord rejouée par les règles, sans toucher à la fenêtre : un coup illégal n'empêche plus de charger ce qui est jouable et ne fait plus tomber l'application
            PartiePdn partiePdn = LecturePdn.LireTexte(partie.CoupsPartiePDN).FirstOrDefault() ?? new PartiePdn();
            if (!string.IsNullOrWhiteSpace(partie.FENDebut))
                partiePdn.Fen = partie.FENDebut;
            RejeuPdn rejeu = partiePdn.Rejouer(premierSiAmbigu: true);
            if (rejeu.Positions.Count == 0)
            {   // La position de départ (FEN) est invalide : rien n'est chargé et le damier n'est pas touché
                Journal.Ecrit($"Partie PDN non chargée ({partie.White} - {partie.Black}) : {rejeu.Erreur}");
                MessageBox.Show($"La partie ne peut pas être chargée :\n{rejeu.Erreur}", "Partie PDN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (string ambiguite in rejeu.Ambiguites ?? Array.Empty<string>())
                Journal.Ecrit($"AMBIGU PDN ({partie.White} - {partie.Black}) : {ambiguite}");
            foreach (string avertissement in partiePdn.Avertissements)
                Journal.Ecrit($"Partie PDN ({partie.White} - {partie.Black}) : ignoré à la lecture : {avertissement}");
            NettoyageRapide();
            PartieCourante.CoupsPartiePDN = "";
            nomsDepuisPartie = true;       // Ces noms sont ceux de la partie chargée : ils ne deviennent pas des préférences
            BoxNomJoueurNoir.Text = NomJoueurNoir = partie.Black;
            PartieCourante.Event = partie.Event;
            PartieCourante.Site = partie.Site;
            PartieCourante.Date = partie.Date;
            PartieCourante.Round = partie.Round;
            PartieCourante.White = BoxNomJoueurBlanc.Text = NomJoueurBlanc = partie.White;
            PartieCourante.Black = BoxNomJoueurNoir.Text = NomJoueurNoir = partie.Black;
            nomsDepuisPartie = false;
            PartieCourante.Result = partie.Result;
            PartieCourante.CoupsPartiePDN = partie.CoupsPartiePDN;
            PartieCourante.FENDebut = partie.FENDebut;
            LabelAfficheScan.Text = $"Partie de {partie.White} contre {partie.Black} ({partie.Result})";
            LabelScore.Text = $"Résultat : {partie.Result}";
            ExisteFENDebut = !string.IsNullOrWhiteSpace(partie.FENDebut);     // Propre à CETTE partie (avant, le drapeau gardait la valeur de la dernière partie lue du fichier)
            rejeuPdnEnCours = true;
            try
            {
                await GenerationListesCoups(rejeu.Coups);
                if (rejeu.Erreur != null)
                {   // La partie est chargée jusqu'au dernier coup légal
                    Journal.Ecrit($"Partie PDN chargée en partie ({partie.White} - {partie.Black}) : {rejeu.Erreur}");
                    MessageBox.Show($"La partie n'a pas pu être rejouée entièrement :\n{rejeu.Erreur}\n\nElle est chargée jusqu'au coup {rejeu.Coups.Count}.", "Partie PDN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {   // Quoi qu'il arrive pendant l'animation, l'application ne tombe pas : on le dit et on rend la main
                Journal.Ecrit($"ERREUR au rejeu d'une partie PDN ({partie.White} - {partie.Black}) : {ex}");
                MessageBox.Show($"Le chargement de la partie s'est arrêté :\n{ex.Message}", "Partie PDN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BloqueBoutons(false);
                AnalyseCoupObligatoire(CouleurAuTrait);
            }
            finally
            {
                rejeuPdnEnCours = false;
            }
        }
        public async Task GenerationListesCoups(IReadOnlyList<Coup> coupsDeLaPartie)
        {   // --- Met la position de départ sur le damier, puis rejoue (avec animation) les coups déjà validés par les règles et remplit les listes de coups ---
            if (ExisteFENDebut)         // MiseenplaceFen(..., true) fait repartir l'historique de zéro avant d'y ajouter les coups
            {
                MiseenplaceFen("[FEN \"" + PartieCourante.FENDebut + "\"]", true);  // Position initiale
                LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s";
            }
            else
            {
                MiseenplaceFen("[FEN \"W:W31-50:B1-20\"]", true);  // Position initiale
            }
            if (coupsDeLaPartie.Count == 0)                      // Vérifier si la liste est vide
            {
                MessageBox.Show("Aucun coup n'a été trouvé dans la partie.", "Information : Pas de coup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LabelPrisesPossibles.Text = "Pas de coup trouvé dans la partie chargée";
                AnalyseCoupObligatoire(CouleurAuTrait);     // Vérifie si un coup obligatoire existe
                return;
            }
            // Blocage des boutons pendant le traitement
            ActiveDamier(false);
            BloqueBoutons(true);
            try
            {
                CouleurToutesCasesManoury(CouleurCaseSombre);       // On efface les couleurs des cases
                await Task.Delay(DelaiAnimation);
                foreach (Coup coupPdn in coupsDeLaPartie)
                {
                    CouleurToutesCasesManoury(CouleurCaseSombre);       // On efface les couleurs des cases
                    int indexSourceBox = GestionDamier.ObtenirIndicePictureBox(coupPdn.Depart);
                    ContenuCase pieceSource = GestionDamier.ContenuCaseCaseManoury(coupPdn.Depart);
                    string notationCoupRejoue;
                    if (coupPdn.EstPrise)       // Prise (rafle)
                    {
                        string notationRejeu = ReglesDames.NotationPdn(ReglesDames.CoupsLegaux(Position.DepuisFen(RecupereFEN())), coupPdn);
                        notationCoupRejoue = await AnimerEtExecuterRafle(indexSourceBox, coupPdn, pieceSource, notationRejeu);
                        ChangerCouleurTrait();
                    }
                    else                        // Déplacement simple
                    {
                        notationCoupRejoue = ExecuteDeplacement(indexSourceBox, coupPdn, pieceSource);
                        ChangerCouleurTrait();
                        await Task.Delay(DelaiAnimation);       // Attendre un peu pour l'effet visuel du déplacement
                    }
                    Partie.Courante.AjouteCoup(notationCoupRejoue, PositionDuDamier());     // Le coup et la position qu'il donne entrent dans l'historique
                }
                LabelPrisesPossibles.Text = "Fin des coups de la partie";
            }
            finally
            {
                BloqueBoutons(false);       // Même si un coup n'a pas pu être joué, l'interface est rendue
            }
        }
    }
}
