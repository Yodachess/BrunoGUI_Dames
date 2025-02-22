// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames est développé par Bruno COURTOIS.  Copyright © 2024/2025  █  
// █ BrunoGUI_Dames est gratuit, sauf s'il est utilisé commercialement        █
// █ Utilisation du moteur SCAN 3.1 de Fabien Letouzey                        █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

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
using System.Text.RegularExpressions;

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
            public string CoupsPartiePDN { get; set; }
        }
        public FichierPartiePdn()
        {
            InitializeComponent();
        }
        public List<string> DecodeFichierPDN(string fichierpdn)
        {   // --- On découpe le fichier PDN pour obtenir la liste des parties contenues dans le fichier. ---
            List<string> listeParties = new List<string>();
            using (StreamReader lecteur = new StreamReader(fichierpdn))
            {   // Note : StreamReader attend le chemin d'accès au fichier, pas le contenu du fichier.
                string ligne;
                string partieCourante = "";
                while ((ligne = lecteur.ReadLine()) != null)
                {
                    if (ligne.StartsWith("[Event "))        // Avec un espace à la fin de Event, pour ne pas confondre avec le Tag EventDate ...
                    {   // Commencer une nouvelle partie
                        if (!string.IsNullOrEmpty(partieCourante))
                        {
                            listeParties.Add(partieCourante);
                        }
                        partieCourante = ligne + " ";
                    }
                    else
                    {   // Ajouter les coups à la partie en cours
                        partieCourante += ligne + " ";
                    }
                }
                if (!string.IsNullOrEmpty(partieCourante))
                {       // Ajouter la dernière partie à la liste des parties
                    listeParties.Add(partieCourante);
                }
                foreach (var partie in listeParties)
                {
                    Console.WriteLine(partie);
                }
            }
            return listeParties;
        }
        public PartieDamesPdn DecodePartiePDN(string pdn)
        {   // --- On recoit UNE partie au format pdn avec balises et on remplit la structure PartieDamesPdn ---

            // Normalisation des sauts de ligne
            pdn = pdn.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n\n", "\n");

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
                    default:
                        break;
                }   // On se limite aux balises obligatoires + nombre de coups, il en existe beaucoup d'autres
            }

            // Recherche de l'index de la première ligne vide, GRAND FOUTOIR DANS LES FICHIERS AVEC LES ESPACES ET LES LIGNES VIDES
            int indexLigneVide = pdn.IndexOf("]  1.");
            if (indexLigneVide == -1)
            {
                // Si la première recherche ne réussit pas, essayez avec une autre séquence de retour à la ligne
                indexLigneVide = pdn.IndexOf("]   1.");
            }
            if (indexLigneVide != -1)
            {   // Recherche de l'index où commence "1." après la première ligne vide
                string SectionCoupsPDN = pdn.Substring(indexLigneVide);
                indexLigneVide = SectionCoupsPDN.IndexOf("1.");
                SectionCoupsPDN = SectionCoupsPDN.Substring(indexLigneVide);
                PartiePDN.CoupsPartiePDN = SectionCoupsPDN;         // PartiePDN.CoupsPartiePDN contient les coups de la partie
                string[] coupsPartie = PartiePDN.CoupsPartiePDN.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);       // On decoupe la liste de coups recue
                // Il faut éliminer les coups contenant $ ou ... ou ) ou (   !!!!!!!!
                coupsPartie = coupsPartie.Where(c => !c.Contains("$") && !c.Contains("...") && !c.Contains("(") && !c.Contains(")")).ToArray();
                PartiePDN.CoupsPartiePDN = string.Join(" ", coupsPartie);
            }
            else
            {
                Console.WriteLine("La première ligne vide n'a pas été trouvée dans le fichier PDN.");
            }
            Console.WriteLine($"Partie {PartiePDN.White} vs {PartiePDN.Black} - PDN nettoyé = {PartiePDN.CoupsPartiePDN}");
            return PartiePDN;
        }
        private void TableauPartiesPdn_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {   // Utilise 'Tag' pour récupérer l'objet complet
                var partie = (PartieDamesPdn)TableauPartiesPdn.Rows[e.RowIndex].Tag;
                if (partie != null)
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
