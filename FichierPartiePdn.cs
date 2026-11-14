// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    public partial class FichierPartiePdn : Form
    {
        public class PartieDamesPdn
        {   // --- Format de chaque partie qui se trouve dans ListeParties ---
            public string Event { get; set; } = "";
            public string Site { get; set; } = "";
            public string Date { get; set; } = "";
            public string Round { get; set; } = "";
            public string White { get; set; } = "";
            public string Black { get; set; } = "";
            public string Result { get; set; } = "";
            public string PlyCount { get; set; } = "";
            public string FENDebut { get; set; } = "";
            public string CoupsPartiePDN { get; set; } = "";
        }
        public FichierPartiePdn()
        {
            InitializeComponent();
        }
        // Lit un fichier PDN avec LecturePdn (qui lit tout le texte d'un seul passage : balises précédées de tabulations ou groupées sur
        // une ligne, commentaires, variantes, « ? » conservés dans les balises…) et renvoie ses parties dans le format de cette fenêtre :
        // les coups en un texte « 32-28 17-22 28x17 … » suivi du marqueur de résultat quand il existe.
        // --- Ce qui empêche d'utiliser un fichier comme liste de parties, en phrase pour le joueur, ou null s'il contient au moins une partie avec des coups ---
        // (un fichier de position FEN se lit comme une « partie » sans coup, à la balise FEN : ce n'est pas une liste de parties)
        public static string? RaisonFichierNonUtilisable(IReadOnlyList<PartieDamesPdn> parties)
        {
            if (parties.Count == 0)
                return "Ce fichier ne contient aucune partie PDN.\nIl n'est probablement pas au format PDN.";
            if (parties.Any(partie => !string.IsNullOrEmpty(partie.CoupsPartiePDN)))
                return null;
            return parties.Any(partie => !string.IsNullOrEmpty(partie.FENDebut))
                ? "Ce fichier contient une position (FEN) mais aucune partie avec des coups.\nPour charger une position, utilise « Charger position FEN »."
                : "Ce fichier ne contient aucune partie avec des coups.";
        }

        private static readonly System.Text.RegularExpressions.Regex NomDeBalise = new System.Text.RegularExpressions.Regex("^[A-Za-z][A-Za-z0-9_]*$");     // Nom de balise PDN usuel (Event, White, Result…)
        public static List<PartieDamesPdn> LireParties(string fichierpdn)
        {
            List<PartieDamesPdn> parties = new List<PartieDamesPdn>();
            foreach (PartiePdn lue in LecturePdn.LireFichier(fichierpdn))
            {
                if (lue.Coups.Count == 0 && !lue.Balises.Any(balise => NomDeBalise.IsMatch(balise.Nom)))
                    continue;       // Aucun coup et aucune balise au nom plausible : ce n'est pas une partie (un fichier qui n'est pas du PDN, comme une image, donne une « partie » à balise illisible)
                string coups = string.Join(" ", lue.Coups);
                if (coups.Length > 0 && !string.IsNullOrEmpty(lue.Resultat)) coups += " " + lue.Resultat;     // Une partie sans coup reste vide (message « aucun coup »)
                parties.Add(new PartieDamesPdn
                {
                    Event = lue.Event,
                    Site = lue.Site,
                    Date = lue.Date,
                    Round = lue.Round,
                    White = lue.White,
                    Black = lue.Black,
                    Result = lue.Result.Length > 0 ? lue.Result : lue.Resultat ?? "",
                    PlyCount = (lue.Balise("PlyCount") ?? "").Trim(),
                    FENDebut = (lue.Fen ?? "").TrimEnd('.'),
                    CoupsPartiePDN = coups
                });
            }
            return parties;
        }

        private void TableauPartiesPdn_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {   // Utilise 'Tag' pour récupérer l'objet complet
                var partie = TableauPartiesPdn.Rows[e.RowIndex].Tag as PartieDamesPdn;
                if (partie != null && partie.CoupsPartiePDN != null)
                {   // Récupére la fenêtre principale
                    var mainForm = Application.OpenForms["BrunoInterfaceGraphiqueDames"] as BrunoInterfaceGraphiqueDames;
                    if (mainForm != null)
                    {   // Charge la partie sélectionnée dans la fenêtre principale
                        mainForm.boutonMasqueAffiche_Click(null, EventArgs.Empty);
                        mainForm.ChargerPartieDepuisPdn(partie);
                        this.Hide();        // Masque la fenêtre FichierPartiePdn
                    }
                }
                else
                {
                    MessageBox.Show("La partie sélectionée ne contient pas de coups", "Pas de coups dans la partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Journal.Ecrit("Partie PDN sans coups choisie dans la liste des parties");
                }
            }
        }
        public void AfficherListeParties(List<PartieDamesPdn> listeParties)
        {
            if (listeParties == null || listeParties.Count == 0)            // Vérifier si la liste est vide
            {
                TableauPartiesPdn.DataSource = null; // Rien à afficher
                return;
            }
            TableauPartiesPdn.Rows.Clear();         // On vide explicitement toutes les lignes
            TableauPartiesPdn.DataSource = null;    // Réinitialisation complète du DataSource
            foreach (var partie in listeParties)    // Ajout manuel des données ligne par ligne (par mesure de sécurité)
            {   // Crée une nouvelle ligne avec les données de la partie
                int rowIndex = TableauPartiesPdn.Rows.Add(partie.White, partie.Black, partie.Result, partie.Round,
                    partie.Date, partie.Event, partie.Site, partie.PlyCount);
                TableauPartiesPdn.Rows[rowIndex].Tag = partie;      // Ajoute l'objet PartieDamesPdn comme 'Tag' de la ligne
            }
            TableauPartiesPdn.Columns[0].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);   // Mise en valeur des joueurs et du résultat dans le tableau
            TableauPartiesPdn.Columns[1].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            TableauPartiesPdn.Columns[2].DefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            TableauPartiesPdn.Refresh();        // Rafraîchir la DataGridView pour s'assurer que les données sont bien affichées
        }
    }
}
