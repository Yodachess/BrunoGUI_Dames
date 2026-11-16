// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    public partial class BrunoInterfaceGraphiqueDames
    {
        // La lecture d'une partie : à partir de la position affichée, les coups de l'historique (Session) sont joués un à un sur le damier, avec leurs animations,
        // à la vitesse choisie. « Pause » l'arrête après le coup en cours ; « Lecture » reprend où l'on en est (ou recommence au début si l'on est à la fin).
        // Pendant la lecture les autres boutons sont inactifs et les clics sur le damier sont ignorés.
        private CancellationTokenSource? lecture;                      // Non null pendant la lecture
        private readonly Button boutonLecture = new Button();
        private readonly TrackBar trackBarVitesseLecture = new TrackBar();
        private readonly ToolTip infobulleLecture = new ToolTip();

        // --- Crée, dans le groupe « Parcours partie », le bouton de lecture et le curseur de vitesse ---
        private void CreeLecture()
        {
            boutonLecture.Name = "BoutonLecture";
            boutonLecture.Location = new Point(5, 62);
            boutonLecture.Size = new Size(99, 24);
            boutonLecture.Text = "Lecture";
            boutonLecture.Click += BoutonLecture_Click;
            trackBarVitesseLecture.Name = "TrackBarVitesseLecture";
            trackBarVitesseLecture.AutoSize = false;
            trackBarVitesseLecture.Location = new Point(3, 87);
            trackBarVitesseLecture.Size = new Size(103, 19);
            trackBarVitesseLecture.Minimum = RythmeLecture.Minimum;
            trackBarVitesseLecture.Maximum = RythmeLecture.Maximum;
            trackBarVitesseLecture.Value = RythmeLecture.ParDefaut;
            trackBarVitesseLecture.TickStyle = TickStyle.None;
            trackBarVitesseLecture.ValueChanged += (sender, e) => MetAJourInfobulleLecture();
            groupParcoursPartie.Controls.Add(boutonLecture);
            groupParcoursPartie.Controls.Add(trackBarVitesseLecture);
            infobulleLecture.SetToolTip(boutonLecture, "Joue les coups de la partie un à un à partir de la position affichée ; un second clic fait une pause");
            MetAJourInfobulleLecture();
        }
        private void MetAJourInfobulleLecture()
        {
            infobulleLecture.SetToolTip(trackBarVitesseLecture, RythmeLecture.Libelle(trackBarVitesseLecture.Value));
        }

        private async void BoutonLecture_Click(object? sender, EventArgs e)
        {   // --- Lecture : démarre ; pendant la lecture, c'est le bouton Pause (la lecture s'arrête après le coup en cours) ---
            if (lecture != null)
            {
                lecture.Cancel();
                return;
            }
            await LitLaPartie();
        }

        // --- Joue sur le damier les coups de l'historique, depuis la position affichée jusqu'à la dernière, tant que la pause n'est pas demandée ---
        private async Task LitLaPartie()
        {
            SessionPartie session = Session;
            if (jeuMoteurEnCours || analyseEnCours)
                return;
            if (session.Positions.Count <= 1)
            {
                ReglagesDeTest.Message("La partie n'a aucun coup à lire.", "Lecture", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (session.EstSurLaDernierePosition)
            {   // On est à la fin : la lecture recommence au début
                session.Debut();
                MiseenplaceFen(session.Fen(0), false);
                AfficheListesPieces();
            }
            lecture = new CancellationTokenSource();
            CancellationToken jeton = lecture.Token;
            boutonLecture.Text = "Pause";
            BloqueBoutons(true);                                    // Rien d'autre ne doit toucher à la partie pendant la lecture...
            groupParcoursPartie.Enabled = true;                     // ... sauf Pause et la vitesse, qui sont dans ce groupe
            ActiveBoutonsParcours(false);
            ActiveDamier(false);
            try
            {
                while (!session.EstSurLaDernierePosition)
                {
                    int i = session.IndexAffiche;
                    Coup? coup = ReglesDames.TrouverCoupPdn(session.Positions[i], session.CoupsPdn[i]);
                    if (coup == null)
                    {   // L'historique n'est fait que de coups légaux : ne devrait pas arriver
                        Journal.Ecrit($"ERREUR lecture : le coup {i + 1} « {session.CoupsPdn[i]} » n'est pas légal dans {session.Positions[i].VersFen()}");
                        break;
                    }
                    CouleurToutesCasesManoury(CouleurCaseSombre);
                    await JoueCoupSurLeDamier(coup, glisse: true);
                    session.Suivant();
                    CouleurAuTrait = session.PositionAffichee.Trait;
                    LabelCoupJoue.Text = $"Coup {session.IndexAffiche} : {session.CoupAffiche}";
                    if (jeton.IsCancellationRequested || session.EstSurLaDernierePosition)
                        break;
                    try { await Task.Delay(RythmeLecture.PauseMs(trackBarVitesseLecture.Value), jeton); }
                    catch (OperationCanceledException) { break; }
                }
            }
            finally
            {
                lecture.Dispose();
                lecture = null;
                boutonLecture.Text = "Lecture";
                BloqueBoutons(false);
                ActiveBoutonsParcours(true);
                ActiveDamier(true);
                LabelInformationJoueur.Text = "Trait aux " + CouleurAuTrait + "s";
            }
        }
        private void ActiveBoutonsParcours(bool actifs)
        {
            BoutonPrecedent.Enabled = BoutonSuivant.Enabled = BoutonDebut.Enabled = BoutonFin.Enabled = actifs;
        }
    }
}
