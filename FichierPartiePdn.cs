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
            public string Event { get; set; }
            public string Site { get; set; }
            public string Date { get; set; }
            public string Round { get; set; }
            public string White { get; set; }
            public string Black { get; set; }
            public string Result { get; set; }
            public string PlyCount { get; set; }
            public string FENDebut { get; set; }
            public string CoupsPartiePDN { get; set; }
        }
        public FichierPartiePdn()
        {
            InitializeComponent();
        }
        public List<string> DecodeFichierPDN(string fichierpdn)
        {   // --- On découpe le fichier PDN pour obtenir la liste des parties contenues dans le fichier. ---
            List<string> listeParties = new List<string>();

            using (StreamReader lecteur = new StreamReader(fichierpdn, Encoding.UTF8))
            {   // Note : StreamReader attend le chemin d'accès au fichier, pas le contenu du fichier.
                string ligne;
                StringBuilder partieCourante = new StringBuilder();

                while ((ligne = lecteur.ReadLine()) != null)
                {
                    ligne = ligne.Replace("\r", "").Replace("?", "").Replace("!", "").Replace("..", "");    // Tentaive de nettoyage
                    if (ligne.StartsWith("[Event ")) // Avec un espace à la fin de Event, pour ne pas confondre avec le Tag EventDate ...
                    {
                        // Commencer une nouvelle partie
                        if (partieCourante.Length > 0)
                        {
                            // Ajouter la partie précédente à la liste si elle existe
                            listeParties.Add(partieCourante.ToString().Trim()); // Enlever l'espace final éventuel
                        }
                        // Réinitialiser partieCourante pour une nouvelle partie
                        partieCourante.Clear();
                    }
                    // Ajouter la ligne avec un saut de ligne explicite
                    partieCourante.AppendLine(ligne);
                }

                if (partieCourante.Length > 0)
                {
                    // Ajouter la dernière partie à la liste des parties
                    listeParties.Add(partieCourante.ToString().Trim());
                }
                // Affichage des parties pour vérification
                foreach (var partie in listeParties)
                {
                    Console.WriteLine($"Partie décodée \n" + partie);
                }
            }
            return listeParties;
        }
        public PartieDamesPdn DecodePartiePDN(string pdn)
        {   // --- On recoit UNE partie au format pdn avec balises et on remplit la structure PartieDamesPdn ---
            pdn = pdn.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n\n", "\n");  // Normalisation des sauts de ligne en cas de besoin
            BrunoInterfaceGraphiqueDames.ExisteFENDebut = false;    // Msie à zéro du FEN de départ 

            List<string> balises = new List<string>();
            PartieDamesPdn PartiePDN = new PartieDamesPdn();

            Regex regex = new Regex(@"\[(.*?)\]");                      // Utilisation d'une expression régulière pour extraire les balises [ et ]
            MatchCollection correspondances = regex.Matches(pdn);       // Recherche de toutes les correspondances
            foreach (Match correspondance in correspondances)           // Ajout des balises trouvées à la liste "balises"
            {
                balises.Add(correspondance.Groups[1].Value);
            }
            foreach (var balise in balises)
            {   // Ce code suppose le format usuel pour les balises dans la partie PDN, c'est-à-dire que le nom de la balise est avant
                // le premier guillemet et que la valeur de la balise est entre les guillemets. 
                string[] parts = balise.Split('"');                 // Divise chaque balise en deux parties en utilisant le caractère ". 
                string NomBalise = parts[0].Trim();                 // Extrait le nom de la balise et le nettoie de tout espace indésirable.
                string ValeurBalise = parts.Length > 1 ? parts[1].Trim() : "";      // Extrait la valeur de la balise, si elle existe,
                                                                                    // et la nettoie également de tout espace indésirable.
                switch (NomBalise)
                {   // Met à jour les propriétés de la partie en fonction de la balise (je gère les 7 obligatoires + PlyCount)
                    case "Event":   //  Balise obligatoire
                        PartiePDN.Event = ValeurBalise;
                        break;
                    case "Site":   //  Balise obligatoire
                        PartiePDN.Site = ValeurBalise;
                        break;
                    case "Date":   //  Balise obligatoire
                        PartiePDN.Date = ValeurBalise;
                        break;
                    case "Round":   //  Balise obligatoire
                        PartiePDN.Round = ValeurBalise;
                        break;
                    case "White":   //  Balise obligatoire
                        PartiePDN.White = ValeurBalise;
                        break;
                    case "Black":   //  Balise obligatoire
                        PartiePDN.Black = ValeurBalise;
                        break;
                    case "Result":   //  Balise obligatoire
                        PartiePDN.Result = ValeurBalise;
                        break;
                    case "PlyCount":   //  Balise qui m'intéresse
                        PartiePDN.PlyCount = ValeurBalise;
                        break;
                    case "FEN":   //  Balise qui m'intéresse
                        PartiePDN.FENDebut = ValeurBalise;
                        PartiePDN.FENDebut = PartiePDN.FENDebut.TrimEnd('.');
                        BrunoInterfaceGraphiqueDames.ExisteFENDebut = true;  // Indique que la FEN de départ existe
                        break;
                    default:
                        break;
                }   // On se limite aux balises obligatoires + nombre de coups, il en existe beaucoup d'autres
            }

            // Purge des commentaires entre accolades :
            pdn = SupprimeCommentaires(pdn, '{', '}');
            pdn = pdn.Replace("\r", "\r\n");
            Console.WriteLine($"PDN sans accolades :\n{pdn}");             // DEBUG 05/02

            // Expression régulière pour supprimer toutes les balises avant les coups
            string pattern = @"\[.*?\]"; // Recherche tout ce qui est entre crochets []
            string contenuNettoye = Regex.Replace(pdn, pattern, string.Empty).Trim();

            // Maintenant on cherche à partir de "1." dans le texte nettoyé
            int indexDebut = contenuNettoye.IndexOf("1.");

            if (indexDebut != -1)
            {   // Extraire la partie du texte à partir de "1."
                string sectionCoupsPDN = contenuNettoye.Substring(indexDebut).Trim();
                sectionCoupsPDN = SupprimeCommentaires(sectionCoupsPDN, '(', ')');      // Purge des commentaires entre parenthèses
                // sectionCoupsPDN = Regex.Replace(sectionCoupsPDN, @"(\r|\n)+", "");      // Purge des \r et \n
                sectionCoupsPDN = Regex.Replace(sectionCoupsPDN, @"[?!]+|\.{3}|!!|\.{2}", string.Empty);     // Purge des signes de notation
                sectionCoupsPDN = sectionCoupsPDN.Replace("\\r", "");       // En fait, \r est présent comme 2 caractères le \ et le r !!???
                PartiePDN.CoupsPartiePDN = sectionCoupsPDN;
            }
            else
            {
                PartiePDN.CoupsPartiePDN = string.Empty; // Si "1." n'est pas trouvé
            }
            Console.WriteLine($"Partie {PartiePDN.White} vs {PartiePDN.Black} - PDN nettoyé = {PartiePDN.CoupsPartiePDN}");
            return PartiePDN;
        }
        public static string SupprimeCommentaires(string chaine, char accoladeOuvrante, char accoladeFermante)
        {   /* Lorsqu'une accolade ouvrante est rencontrée, le niveau d'imbrication est augmenté de 1.
            Lorsqu'une accolade fermante est rencontrée et que le niveau d'imbrication est supérieur à 0, 
            cela signifie qu'elle correspond à une paire d'accolades imbriquées, donc le niveau d'imbrication est décrémenté de 1.
            Si une accolade fermante est rencontrée et que le niveau d'imbrication est déjà à 0, 
            cela signifie qu'elle est en dehors de toute paire d'accolades imbriquées, donc elle est conservée dans le résultat final.
            Les caractères qui ne sont pas situés entre des accolades imbriquées sont ajoutés au résultat final. */

            StringBuilder resultat = new StringBuilder();
            int niveauAccolade = 0;
            foreach (char caractere in chaine)
            {
                if (caractere == accoladeOuvrante)
                {
                    niveauAccolade++;
                }
                else if (caractere == accoladeFermante && niveauAccolade > 0)
                {
                    niveauAccolade--;
                }
                else if (niveauAccolade == 0)
                {
                    resultat.Append(caractere);
                }
            }
            return resultat.ToString();
        }

        private void TableauPartiesPdn_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {   // Utilise 'Tag' pour récupérer l'objet complet
                var partie = (PartieDamesPdn)TableauPartiesPdn.Rows[e.RowIndex].Tag;
                if (partie != null && partie.CoupsPartiePDN != null)
                {   // Récupére la fenêtre principale
                    var mainForm = (BrunoInterfaceGraphiqueDames)Application.OpenForms["BrunoInterfaceGraphiqueDames"];
                    if (mainForm != null)
                    {   // Charge la partie sélectionnée dans la fenêtre principale
                        Console.WriteLine($"Chargement de la partie {partie.Event} - {partie.White} vs {partie.Black}");
                        Console.WriteLine($"Partie = {partie.CoupsPartiePDN}");
                        mainForm.boutonMasqueAffiche_Click(null, EventArgs.Empty);
                        mainForm.ChargerPartieDepuisPdn(partie);
                        this.Hide();        // Masque la fenêtre FichierPartiePdn
                    }
                }
                else
                {
                    MessageBox.Show("La partie sélectionée ne contient pas de coups", "Pas de coups dans la partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine("Erreur : La partie = null !? (sans doute vide ...)");
                }
            }
        }
        public void AfficherListeParties(List<PartieDamesPdn> listeParties)
        {
            if (listeParties == null || listeParties.Count == 0)            // Vérifier si la liste est vide
            {
                Console.WriteLine("Aucune partie à afficher.");
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
            Console.WriteLine($"Chargement de {listeParties.Count} parties");       // Vérification du nombre de parties ajoutées
            TableauPartiesPdn.Refresh();        // Rafraîchir la DataGridView pour s'assurer que les données sont bien affichées
        }
    }
}
