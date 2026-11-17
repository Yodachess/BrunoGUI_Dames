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
                        Journal.Attention($"Fichier inutilisable comme liste de parties PDN ({cheminFichier}) : {raison.Replace("\n", " ")}");
                        ReglagesDeTest.Message(Path.GetFileName(cheminFichier) + "\n\n" + raison, "Parties PDN", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    fichierPartiePdn.NombrePartiesFichier.Text = T("{0} partie(s) dans le fichier  {1}", ListePartiesPdn.Count, cheminFichier.Substring(cheminFichier.LastIndexOf('\\') + 1));
                    fichierPartiePdn.AfficherListeParties(ListePartiesPdn); 
                    fichierPartiePdn.Show();
                    BoutonMasqueAffiche.Enabled = true; // Active le bouton pour masquer/afficher la liste
                }
                catch (Exception ex)
                {
                    Journal.Erreur($"Chargement des parties PDN ({cheminFichier})", ex);
                    ReglagesDeTest.Message(T("Le fichier PDN n'a pas pu être lu :\n{0}", ex.Message), "Parties PDN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        public void ChargerPartieDepuisPdn(PartieDamesPdn partie)
        {   // --- Charge une partie depuis un fichier PDN lorsqu'on double-clique : elle entre tout de suite dans l'historique et le damier montre sa position de départ.
            // « Lecture » la déroule avec ses animations, « Suiv. » avance coup à coup (le groupe « Parcours partie ») ---
            if (lecture != null || jeuMoteurEnCours || analyseEnCours)
            {
                ReglagesDeTest.Message(T("Une lecture est en cours, ou le moteur réfléchit.\nAttends la fin avant de charger une partie."), "Chargement impossible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // La partie est d'abord rejouée par les règles, sans toucher à la fenêtre : un coup illégal n'empêche plus de charger ce qui est jouable et ne fait plus tomber l'application
            PartiePdn partiePdn = LecturePdn.LireTexte(partie.CoupsPartiePDN).FirstOrDefault() ?? new PartiePdn();
            if (!string.IsNullOrWhiteSpace(partie.FENDebut))
                partiePdn.Fen = partie.FENDebut;
            RejeuPdn rejeu = partiePdn.Rejouer(premierSiAmbigu: true);
            if (rejeu.Positions.Count == 0)
            {   // La position de départ (FEN) est invalide : rien n'est chargé et le damier n'est pas touché
                Journal.Attention($"Partie PDN non chargée ({partie.White} - {partie.Black}) : {rejeu.Erreur}");
                ReglagesDeTest.Message(T("La partie ne peut pas être chargée :\n{0}", rejeu.Erreur), "Partie PDN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (string ambiguite in rejeu.Ambiguites ?? Array.Empty<string>())
                Journal.Attention($"AMBIGU PDN ({partie.White} - {partie.Black}) : {ambiguite}");
            foreach (string avertissement in partiePdn.Avertissements)
                Journal.Attention($"Partie PDN ({partie.White} - {partie.Black}) : ignoré à la lecture : {avertissement}");
            NettoyageRapide();
            nomsDepuisPartie = true;       // Ces noms sont ceux de la partie chargée : ils ne deviennent pas des préférences
            BoxNomJoueurNoir.Text = NomJoueurNoir = partie.Black;
            BalisesPartie.Event = partie.Event;
            BalisesPartie.Site = partie.Site;
            BalisesPartie.Date = partie.Date;
            BalisesPartie.Round = partie.Round;
            BalisesPartie.White = BoxNomJoueurBlanc.Text = NomJoueurBlanc = partie.White;
            BalisesPartie.Black = BoxNomJoueurNoir.Text = NomJoueurNoir = partie.Black;
            nomsDepuisPartie = false;
            BalisesPartie.Result = partie.Result;
            BalisesPartie.CoupsPartiePDN = partie.CoupsPartiePDN;
            BalisesPartie.FENDebut = partie.FENDebut;
            LabelAfficheScan.Text = T("Partie de {0} contre {1} ({2})", partie.White, partie.Black, partie.Result);
            LabelScore.Text = T("Résultat : {0}", partie.Result);
            // La partie entre d'un coup dans l'historique (chaque coup avec la position qu'il donne), puis le damier montre la position de départ
            SessionPartie session = Session;
            session.Commencer(rejeu.Positions[0]);
            for (int i = 0; i < rejeu.Coups.Count; i++)
                session.AjouteCoup(ReglesDames.NotationPdn(ReglesDames.CoupsLegaux(rejeu.Positions[i]), rejeu.Coups[i]), rejeu.Positions[i + 1]);
            session.Debut();
            MiseenplaceFen(session.Fen(0), false);
            AfficheListesPieces();
            LabelCoupJoue.Text = T("Position initiale");
            LabelPrisesPossibles.Text = T("Partie chargée : {0} coup(s).\nLecture la déroule, Suiv. avance coup à coup.", rejeu.Coups.Count);
            LabelInformationJoueur.Text = TraitAux(CouleurAuTrait);
            ReinitialisePendule();      // Si l'on continue la partie chargée, la pendule part de son temps de base
            AnalyseCoupObligatoire(CouleurAuTrait);     // Les cases jouables et la prise obligatoire éventuelle de la position de départ
            if (rejeu.Erreur != null)
            {   // La partie est chargée jusqu'au dernier coup légal
                Journal.Attention($"Partie PDN chargée en partie ({partie.White} - {partie.Black}) : {rejeu.Erreur}");
                ReglagesDeTest.Message(T("La partie n'a pas pu être rejouée entièrement :\n{0}\n\nElle est chargée jusqu'au coup {1}.", rejeu.Erreur, rejeu.Coups.Count), "Partie PDN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }    }
}
