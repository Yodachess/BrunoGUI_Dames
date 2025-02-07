// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames est développé par Bruno COURTOIS.  Copyright © 2024/2025  █  
// █ BrunoGUI_Dames est gratuit, sauf s'il est utilisé commercialement        █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BrunoGUI_Dames
{
    public delegate void AfficheMoteurDamesScan();
    public delegate void AfficheCoupMoteurDamesScan();
    public delegate void AfficheDonneesBrutesDamesScan();
    public class MoteurDamesScan
    {
        public static event AfficheMoteurDamesScan AfficheScan;
        public static event AfficheDonneesBrutesDamesScan AfficheDonneesBrutes;
        public static event AfficheCoupMoteurDamesScan AfficheCoupMoteur;
        public static string DonneesScan { get; set; }
        public static string DonneesVersScan { get; set; }
        public static string CoupScan { get; set; }
        public static string SuggestionScan { get; set; }
        public static string FichierMoteurScan { get; set; }
        public static string AuteurMoteur { get; set; }
        public static bool CoupScanJoue; // Flag indiquant si le coup a été mis à jour
        public static bool ScanVersGui { get; set; }
        private static Process Proc;

        public void Start(string fichierMoteurDames)
        {
            // Récupère le répertoire contenant le fichier moteur
            string repertoireMoteur = Path.GetDirectoryName(fichierMoteurDames);
            Proc = new Process();

            // Paramétrage de Proc.StartInfo
            Proc.StartInfo.FileName = fichierMoteurDames;   // Chemin du moteur
            Proc.StartInfo.Arguments = "hub";               // Ajout du paramètre "hub"
            Proc.StartInfo.WorkingDirectory = repertoireMoteur; // Répertoire de travail
            Proc.StartInfo.UseShellExecute = false;
            Proc.StartInfo.RedirectStandardOutput = true;
            Proc.StartInfo.RedirectStandardInput = true;
            Proc.StartInfo.CreateNoWindow = true;
            Proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // Démarrer le processus
            Proc.Start();

            // Gestionnaire d'événement de sortie de données
            Proc.OutputDataReceived += ProcOutputDataReceived;

            // Commencer à lire les sorties de données
            Proc.BeginOutputReadLine();

            // Première interrogation du processus : le moteur SCAN est-il prêt ?
            StandardInputDataToScan("hub"); // On demande les infos au moteur
            StandardInputDataToScan("init"); // On demande les infos au moteur
            StandardInputDataToScan("ping"); // Pour le fun ...
        }

        // Evènement de sortie de données du processus SCAN vers l'interface pour jouer le coup du moteur SCAN
        private void ProcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            ScanVersGui = true;
            if (string.IsNullOrWhiteSpace(e.Data) == false)   // true si la chaine est " ", "\n", null, ""
            {
                // extraire les mots de l'instruction
                DonneesScan = e.Data;
                AfficheScan();

                string[] DataTableau = DonneesScan.Split(' ');
                AfficheDonneesBrutes();

                switch (DataTableau[0])         // Analyse réponse moteur SCAN 
                {   // identifier le premier mot 
                    case "\n":
                    case " ":
                        break;
                    case "done": // le moteur SCAN propose le meilleur coup
                        if (DataTableau[1].Contains("move="))
                        {   // Récupérer ce qu'il y a après "move="
                            CoupScan = DataTableau[1].Split(new[] { "move=" }, StringSplitOptions.None)[1];
                            // Vérifier si DataTableau contient plus de 2 éléments et si "ponder=" est présent
                            if (DataTableau.Count() > 2 && DataTableau[2].Contains("ponder="))
                            {   // Récupérer ce qu'il y a après "ponder="
                                SuggestionScan = DataTableau[2].Split(new[] { "ponder=" }, StringSplitOptions.None)[1];
                            }
                            else
                            {   // Aucun "ponder=" trouvé
                                SuggestionScan = ""; // ou une autre valeur par défaut
                            }
                            CoupScanJoue = true;  // Indique que le coup a été mis à jour
                            // Afficher le résultat
                            Console.WriteLine($"CoupScan : {CoupScan} / SuggestionScan : {SuggestionScan}");
                        }
                        else
                        {
                            BrunoInterfaceGraphiqueDames.FinPartie = true;
                        }
                        break;
                    case "error":
                        Console.WriteLine($"ERROR MESSAGE RETOURNE PAR SCAN !");
                        break;
                }
            }
            ScanVersGui = false;
        }

        public static void StandardInputDataToScan(string Data)
        {   // Envoi de données de l'interface vers moteur SCAN
            // Debug.WriteLine($"[App] {Data}");
            ScanVersGui = false;
            DonneesVersScan = Data;
            AfficheDonneesBrutes();
            Proc.StandardInput.Write(Data + Environment.NewLine);
        }
        public static void Quitte()
        {   // On ferme le moteur SCAN
            StandardInputDataToScan("quit");
            Proc.Dispose();
        }
    }
}


