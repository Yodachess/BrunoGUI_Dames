// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
// Le moteur Scan 3.1 (processus « scan.exe hub », protocole Hub2) : démarrage, envoi des commandes, lecture de sa sortie et réponse à une demande
// de coup. Logique sans Windows Forms. La sortie du moteur arrive sur un thread du pool : les événements sont déclenchés sur ce thread et ne
// modifient jamais l'état de la partie, c'est à l'interface de repasser sur son thread. Une seule demande de coup est active à la fois : une
// nouvelle demande abandonne la précédente, et la réponse arrive par ReflechirAsync (jamais d'attente sans fin : délai maximal, arrêt du moteur).

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BrunoGUI_Dames
{
    public delegate void AfficheMoteurDamesScan(string ligne);
    public delegate void AfficheDonneesBrutesDamesScan(string ligne, bool venantDuMoteur);

    // --- La réponse du moteur à une demande de coup : Coup est la notation Hub2 (« 32-28 », « 28x19x23 »), vide si le moteur n'a aucun coup à jouer ---
    public sealed record ReponseMoteur(string Coup, string Suggestion);

    public class MoteurDamesScan
    {
        public static event AfficheMoteurDamesScan? AfficheScan;                  // Une ligne reçue du moteur (thread du pool)
        public static event AfficheDonneesBrutesDamesScan? AfficheDonneesBrutes;  // Une ligne échangée, avec son sens (thread du pool ou de l'interface)
        public static event Action<string>? MoteurArrete;                         // Le moteur s'est arrêté sans qu'on le lui demande (thread du pool)

        private static Process? Proc;
        private static readonly object verrou = new object();
        private static TaskCompletionSource<ReponseMoteur?>? demande;             // La demande de coup en cours, ou null
        private static bool arretVoulu;

        public static bool EstDemarre
        {
            get { lock (verrou) { return Proc != null && !Proc.HasExited; } }
        }

        // --- Lance « scan.exe hub » dans son dossier. Lève FileNotFoundException si le moteur n'existe pas, et l'exception du système s'il ne peut pas démarrer ---
        public static void Demarre(string fichierMoteurDames)
        {
            if (!File.Exists(fichierMoteurDames))
                throw new FileNotFoundException("Le moteur Scan est introuvable : " + fichierMoteurDames, fichierMoteurDames);
            Process processus = new Process();
            processus.StartInfo.FileName = fichierMoteurDames;
            processus.StartInfo.Arguments = "hub";
            processus.StartInfo.WorkingDirectory = Path.GetDirectoryName(fichierMoteurDames) ?? string.Empty;
            processus.StartInfo.UseShellExecute = false;
            processus.StartInfo.RedirectStandardOutput = true;
            processus.StartInfo.RedirectStandardInput = true;
            processus.StartInfo.CreateNoWindow = true;
            processus.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processus.EnableRaisingEvents = true;
            processus.OutputDataReceived += ProcOutputDataReceived;     // Avant Start, pour ne perdre aucune ligne
            processus.Exited += ProcExited;
            lock (verrou)
            {
                arretVoulu = false;
                processus.Start();
                Proc = processus;
            }
            processus.BeginOutputReadLine();
            Envoie("hub");      // On demande les infos au moteur
            Envoie("init");
            Envoie("ping");
        }

        // --- Demande un coup pour la position Hub2 en réfléchissant secondes secondes ; null si le moteur ne répond pas (arrêté, délai dépassé, remplacé par une autre demande) ---
        public static async Task<ReponseMoteur?> ReflechirAsync(string positionHub2, int secondes)
        {
            TaskCompletionSource<ReponseMoteur?> cetteDemande = new TaskCompletionSource<ReponseMoteur?>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (verrou)
            {
                demande?.TrySetResult(null);        // Une nouvelle demande abandonne la précédente
                demande = cetteDemande;
            }
            try
            {
                Envoie("pos pos=" + positionHub2);          // On envoie la position au format Hub2
                Envoie("level move-time=" + secondes);      // x secondes pour le coup
                Envoie("go think");
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidOperationException)
            {
                Journal.Ecrit("Moteur : commande non envoyée : " + ex.Message);
                lock (verrou) { if (demande == cetteDemande) demande = null; }
                return null;
            }
            Task premiere = await Task.WhenAny(cetteDemande.Task, Task.Delay(TimeSpan.FromSeconds(secondes + 15)));
            if (premiere != cetteDemande.Task)
            {   // Le moteur ne répond plus : on arrête la recherche et on rend la main
                Journal.Ecrit($"Moteur : pas de réponse après {secondes + 15} s, recherche interrompue");
                lock (verrou) { if (demande == cetteDemande) demande = null; }
                try { Envoie("stop"); } catch (Exception ex) when (ex is IOException || ex is InvalidOperationException) { }
                return null;
            }
            return await cetteDemande.Task;
        }

        // --- Envoi d'une commande de l'interface vers le moteur ---
        public static void Envoie(string data)
        {
            Process processus;
            lock (verrou)
                processus = Proc ?? throw new InvalidOperationException("Le moteur Scan n'est pas démarré.");
            Declenche(() => AfficheDonneesBrutes?.Invoke(data, false));
            processus.StandardInput.Write(data + Environment.NewLine);
        }

        // --- Arrête le moteur : « quit », puis arrêt forcé s'il ne s'est pas terminé tout seul ; sans effet s'il n'est pas démarré ---
        public static void Quitte()
        {
            Process? processus;
            lock (verrou)
            {
                processus = Proc;
                Proc = null;
                arretVoulu = true;
                demande?.TrySetResult(null);
                demande = null;
            }
            if (processus == null)
                return;
            try
            {
                if (!processus.HasExited)
                {
                    try { processus.StandardInput.Write("quit" + Environment.NewLine); }
                    catch (Exception ex) when (ex is IOException || ex is InvalidOperationException) { }       // Le moteur est déjà arrêté : le Kill ci-dessous ne sera pas nécessaire
                    if (!processus.WaitForExit(1500))
                        processus.Kill(true);       // true : toute l'arborescence
                }
            }
            catch (Exception ex)
            {
                Journal.Ecrit("Moteur : erreur à l'arrêt : " + ex.Message);
                try { processus.Kill(true); }
                catch (Exception exArret) { Journal.Ecrit("Moteur : arrêt forcé impossible : " + exArret.Message); }
            }
            finally
            {
                processus.Dispose();
            }
        }

        // Une ligne de sortie du moteur SCAN (thread du pool)
        private static void ProcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string? ligne = e.Data;
            if (string.IsNullOrWhiteSpace(ligne))
                return;
            Declenche(() => AfficheScan?.Invoke(ligne));
            Declenche(() => AfficheDonneesBrutes?.Invoke(ligne, true));
            string[] mots = ligne.Split(' ');
            switch (mots[0])
            {
                case "done":        // le moteur SCAN propose le meilleur coup : « done move=28-23 ponder=17-22 », ou « done » seul s'il n'a aucun coup
                    string coup = "", suggestion = "";
                    foreach (string mot in mots)
                    {
                        if (mot.StartsWith("move=")) coup = mot.Substring("move=".Length);
                        else if (mot.StartsWith("ponder=")) suggestion = mot.Substring("ponder=".Length);
                    }
                    TaskCompletionSource<ReponseMoteur?>? attendante;
                    lock (verrou)
                    {
                        attendante = demande;
                        demande = null;
                    }
                    attendante?.TrySetResult(new ReponseMoteur(coup, suggestion));
                    break;
                case "error":
                    Journal.Ecrit("Moteur : " + ligne);
                    break;
            }
        }

        // Le processus s'est terminé (thread du pool) : si on ne l'a pas demandé, la demande en cours reçoit null et l'interface est prévenue
        private static void ProcExited(object? sender, EventArgs e)
        {
            TaskCompletionSource<ReponseMoteur?>? attendante;
            bool voulu;
            lock (verrou)
            {
                attendante = demande;
                demande = null;
                voulu = arretVoulu;
            }
            attendante?.TrySetResult(null);
            if (!voulu)
            {
                Journal.Ecrit("Moteur : le processus scan.exe s'est arrêté de lui-même");
                Declenche(() => MoteurArrete?.Invoke("Le moteur Scan s'est arrêté."));
            }
        }

        // --- Exécute une notification vers l'interface sans que l'exception d'un abonné (fenêtre déjà fermée…) ne fasse tomber le thread du pool ---
        private static void Declenche(Action notification)
        {
            try { notification(); }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
            {
                Journal.Ecrit("Moteur : notification de l'interface impossible : " + ex.Message);
            }
        }
    }
}