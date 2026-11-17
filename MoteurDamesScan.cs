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
    public sealed record ReponseMoteur(string Coup, string Suggestion, bool DuLivre = false);

    public class MoteurDamesScan
    {
        public event AfficheMoteurDamesScan? AfficheScan;                  // Une ligne reçue du moteur (thread du pool)
        public event AfficheDonneesBrutesDamesScan? AfficheDonneesBrutes;  // Une ligne échangée, avec son sens (thread du pool ou de l'interface)
        public event Action<string>? MoteurArrete;                         // Le moteur s'est arrêté sans qu'on le lui demande (thread du pool)

        private Process? Proc;
        private readonly object verrou = new object();
        private TaskCompletionSource<ReponseMoteur?>? demande;             // La demande de coup en cours, ou null
        private bool arretVoulu;
        private bool rechercheVue, livreVu;                                // Pour la demande en cours : une ligne « info depth=… » (une recherche) ou une ligne de score sans profondeur (le livre d'ouvertures) est arrivée

        public bool EstDemarre
        {
            get { lock (verrou) { return Proc != null && !Proc.HasExited; } }
        }

        // --- Lance « scan.exe hub » dans son dossier. Lève FileNotFoundException si le moteur n'existe pas, et l'exception du système s'il ne peut pas démarrer ---
        public void Demarre(string fichierMoteurDames)
        {
            if (!File.Exists(fichierMoteurDames))
                throw new FileNotFoundException(Texte.T("Le moteur Scan est introuvable : {0}", fichierMoteurDames), fichierMoteurDames);
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

        // --- Demande un coup pour la position Hub2 en réfléchissant secondes secondes au plus, et pas plus de profondeur demi-coups (NiveauMoteur.ProfondeurMaximale : pas de limite) ;
        // null si le moteur ne répond pas (arrêté, délai dépassé, remplacé par une autre demande) ---
        public async Task<ReponseMoteur?> ReflechirAsync(string positionHub2, int secondes, int profondeur = NiveauMoteur.ProfondeurMaximale)
        {
            TaskCompletionSource<ReponseMoteur?> cetteDemande = new TaskCompletionSource<ReponseMoteur?>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (verrou)
            {
                demande?.TrySetResult(null);        // Une nouvelle demande abandonne la précédente
                demande = cetteDemande;
                rechercheVue = livreVu = false;
            }
            try
            {
                Envoie("pos pos=" + positionHub2);          // On envoie la position au format Hub2
                Envoie("level depth=" + profondeur + " move-time=" + secondes);      // La profondeur est toujours envoyée : Scan garde la limite d'une demande à l'autre
                Envoie("go think");
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidOperationException)
            {
                Journal.Attention("Moteur : commande non envoyée : " + ex.Message);
                lock (verrou) { if (demande == cetteDemande) demande = null; }
                return null;
            }
            Task premiere = await Task.WhenAny(cetteDemande.Task, Task.Delay(TimeSpan.FromSeconds(secondes + 15)));
            if (premiere != cetteDemande.Task)
            {   // Le moteur ne répond plus : on arrête la recherche et on rend la main
                Journal.Attention($"Moteur : pas de réponse après {secondes + 15} s, recherche interrompue");
                lock (verrou) { if (demande == cetteDemande) demande = null; }
                try { Envoie("stop"); } catch (Exception ex) when (ex is IOException || ex is InvalidOperationException) { }
                return null;
            }
            return await cetteDemande.Task;
        }

        // --- Envoi d'une commande de l'interface vers le moteur ---
        public void Envoie(string data)
        {
            Process processus;
            lock (verrou)
                processus = Proc ?? throw new InvalidOperationException("Le moteur Scan n'est pas démarré.");
            Declenche(() => AfficheDonneesBrutes?.Invoke(data, false));
            processus.StandardInput.Write(data + Environment.NewLine);
        }

        // --- Arrête le moteur : « quit », puis arrêt forcé s'il ne s'est pas terminé tout seul ; sans effet s'il n'est pas démarré ---
        public void Quitte()
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
                Journal.Attention("Moteur : erreur à l'arrêt : " + ex.Message);
                try { processus.Kill(true); }
                catch (Exception exArret) { Journal.Attention("Moteur : arrêt forcé impossible : " + exArret.Message); }
            }
            finally
            {
                processus.Dispose();
            }
        }

        // Une ligne de sortie du moteur SCAN (thread du pool)
        private void ProcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string? ligne = e.Data;
            if (string.IsNullOrWhiteSpace(ligne))
                return;
            Declenche(() => AfficheScan?.Invoke(ligne));
            Declenche(() => AfficheDonneesBrutes?.Invoke(ligne, true));
            string[] mots = ligne.Split(' ');
            switch (mots[0])
            {
                case "info":        // « info depth=… » : une recherche ; « info score=… pv=… » sans profondeur : un coup du livre d'ouvertures ; « info pv=… » seul : un coup forcé
                    LigneMoteurLue lue = LigneMoteur.Lire(ligne);
                    lock (verrou)
                    {
                        if (lue.Profondeur != null) rechercheVue = true;
                        else if (lue.Score != null) livreVu = true;
                    }
                    break;
                case "done":        // le moteur SCAN propose le meilleur coup : « done move=28-23 ponder=17-22 », ou « done » seul s'il n'a aucun coup
                    string coup = "", suggestion = "";
                    foreach (string mot in mots)
                    {
                        if (mot.StartsWith("move=")) coup = mot.Substring("move=".Length);
                        else if (mot.StartsWith("ponder=")) suggestion = mot.Substring("ponder=".Length);
                    }
                    TaskCompletionSource<ReponseMoteur?>? attendante;
                    bool duLivre;
                    lock (verrou)
                    {
                        attendante = demande;
                        demande = null;
                        duLivre = livreVu && !rechercheVue;
                    }
                    attendante?.TrySetResult(new ReponseMoteur(coup, suggestion, duLivre));
                    break;
                case "error":
                    Journal.Attention("Moteur : " + ligne);
                    break;
            }
        }

        // Le processus s'est terminé (thread du pool) : si on ne l'a pas demandé, la demande en cours reçoit null et l'interface est prévenue
        private void ProcExited(object? sender, EventArgs e)
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
                Journal.Erreur("Moteur : le processus scan.exe s'est arrêté de lui-même");
                Declenche(() => MoteurArrete?.Invoke(Texte.T("Le moteur Scan s'est arrêté.")));
            }
        }

        // --- Exécute une notification vers l'interface sans que l'exception d'un abonné (fenêtre déjà fermée…) ne fasse tomber le thread du pool ---
        private void Declenche(Action notification)
        {
            try { notification(); }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
            {
                Journal.Attention("Moteur : notification de l'interface impossible : " + ex.Message);
            }
        }
    }
}