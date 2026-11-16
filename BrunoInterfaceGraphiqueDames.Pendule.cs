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
using System.IO;
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    public partial class BrunoInterfaceGraphiqueDames
    {
        // La pendule : affichage des deux temps, minuterie, choix de la cadence, chute du drapeau. La logique est dans Pendule.cs ; sans cadence choisie, il n'y a pas de pendule et les deux cadres affichent « --:-- ».
        private Pendule? pendule;                                                 // Null : pas de pendule
        private readonly System.Windows.Forms.Timer minuteriePendule = new System.Windows.Forms.Timer { Interval = 200 };
        private readonly Label labelPenduleBlanc = new Label();
        private readonly Label labelPenduleNoir = new Label();
        private readonly ComboBox comboCadence = new ComboBox();
        private bool pieceEnMain;                                                 // Vrai entre la sélection d'une pièce et le clic sur sa destination (son image est alors sur le curseur)

        // --- Crée, en code comme le damier, les deux temps (en haut à droite) et le choix de la cadence (dans le cadre du temps de réflexion) ---
        private void CreePendule()
        {
            Font police = new Font("Consolas", 11F, FontStyle.Bold);
            labelPenduleBlanc.Name = "LabelPenduleBlanc";      // Les noms permettent aux tests de retrouver les contrôles créés ici
            labelPenduleNoir.Name = "LabelPenduleNoir";
            comboCadence.Name = "ComboCadence";
            ConfigureLabelPendule(labelPenduleBlanc, 5, "Temps restant des blancs");      // Alignés sur les deux lignes du haut (noms des joueurs, score) et sur la colonne de boutons
            ConfigureLabelPendule(labelPenduleNoir, 31, "Temps restant des noirs");
            labelPenduleBlanc.Font = labelPenduleNoir.Font = police;
            // Le cadre « Pendule et temps de réflexion » (groupBox1, dans le Designer) : la liste des cadences à gauche, puis le curseur du temps de réflexion et sa valeur
            comboCadence.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCadence.Location = new Point(6, 17);
            comboCadence.Width = 158;
            comboCadence.DropDownWidth = 260;
            comboCadence.Items.Add("Sans pendule");
            foreach (Cadence cadence in Cadence.Officielles)
                comboCadence.Items.Add(cadence.Court);
            comboCadence.SelectedIndex = 0;
            comboCadence.SelectedIndexChanged += ComboCadence_SelectedIndexChanged;
            groupBox1.Controls.Add(comboCadence);
            new ToolTip().SetToolTip(comboCadence, "Cadence de la pendule (cadences de la FMJD). Le premier coup n'est pas décompté.");
            minuteriePendule.Tick += MinuteriePendule_Tick;
            minuteriePendule.Start();
            MetAJourAffichagePendule();
        }
        private void ConfigureLabelPendule(Label label, int haut, string aide)
        {
            label.Location = new Point(653, haut);
            label.Size = new Size(110, 24);
            label.BorderStyle = BorderStyle.FixedSingle;
            label.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(label);
            label.BringToFront();
            new ToolTip().SetToolTip(label, aide);
        }

        // --- L'Id de la cadence choisie (écrit dans les préférences), ou null ---
        private string? CadenceChoisieId => comboCadence.SelectedIndex > 0 ? Cadence.Officielles[comboCadence.SelectedIndex - 1].Id : null;
        private void SelectionneCadence(string? id)
        {
            int index = 0;
            for (int i = 0; i < Cadence.Officielles.Count; i++)
                if (Cadence.Officielles[i].Id == id)
                    index = i + 1;
            comboCadence.SelectedIndex = index;
        }
        private void ComboCadence_SelectedIndexChanged(object? sender, EventArgs e)
        {   // --- Une autre cadence : la pendule repart à zéro (elle attend le prochain coup) ---
            pendule = comboCadence.SelectedIndex > 0 ? new Pendule(Cadence.Officielles[comboCadence.SelectedIndex - 1], ReglagesDeTest.HorlogePendule) : null;
            ReinitialisePendule();
        }

        // --- Nouvelle partie, position chargée, partie PDN chargée : chaque camp retrouve son temps de base ; la pendule attend le prochain coup ---
        private void ReinitialisePendule()
        {
            pendule?.Reinitialiser(CouleurAuTrait);
            if (PartieTerminee)
                pendule?.Arreter();
            MetAJourAffichagePendule();
        }
        // --- Retour arrière : les temps sont gardés, la pendule attend le prochain coup ---
        private void ArmePendule()
        {
            pendule?.Armer(CouleurAuTrait);
            MetAJourAffichagePendule();
        }
        // --- La partie est finie (résultat) : le temps s'arrête ---
        private void ArretePendule()
        {
            pendule?.Arreter();
            MetAJourAffichagePendule();
        }
        // --- Le camp « joueur » vient de jouer : à appeler quand le coup est décidé, avant son animation (joueur : CouleurAuTrait pour un coup humain, le trait de la position envoyée pour le moteur).
        // Faux si son drapeau était tombé : la partie est perdue au temps et le coup n'est pas joué ---
        private bool PenduleCoupJoue(Couleur joueur)
        {
            if (pendule == null || pendule.CoupJoue(joueur))
            {
                MetAJourAffichagePendule();
                return true;
            }
            MetAJourAffichagePendule();
            if (pendule.Perdant is Couleur perdant)
                FinPartieAuTemps(perdant);
            return false;
        }
        private void MinuteriePendule_Tick(object? sender, EventArgs e)
        {   // --- Tous les cinquièmes de seconde : les temps sont affichés et le drapeau qui tombe termine la partie ---
            if (pendule == null)
                return;
            Couleur? drapeauTombe = pendule.Verifie();
            MetAJourAffichagePendule();
            if (drapeauTombe is Couleur perdant)
                FinPartieAuTemps(perdant);
        }
        private void MetAJourAffichagePendule()
        {   // --- Les deux temps ; le camp dont le temps s'écoule est en vert, moins de 20 secondes en rouge. Sans pendule, les cadres restent affichés avec « --:-- » ---
            if (pendule == null)
            {
                labelPenduleBlanc.Text = "B --:--";
                labelPenduleBlanc.BackColor = Color.White;
                labelPenduleBlanc.ForeColor = Color.Black;
                labelPenduleNoir.Text = "N --:--";
                labelPenduleNoir.BackColor = Color.Black;
                labelPenduleNoir.ForeColor = Color.White;
                return;
            }
            AfficheTemps(labelPenduleBlanc, "B ", Couleur.Blanc, Color.White, Color.Black, Color.PaleGreen);
            AfficheTemps(labelPenduleNoir, "N ", Couleur.Noir, Color.Black, Color.White, Color.DarkGreen);
        }
        private void AfficheTemps(Label label, string prefixe, Couleur couleur, Color fond, Color texte, Color fondEnMarche)
        {
            if (pendule == null)
                return;
            TimeSpan restant = pendule.TempsRestant(couleur);
            bool enMarche = pendule.Etat == EtatPendule.EnMarche && pendule.Trait == couleur;
            label.Text = prefixe + Pendule.Formate(restant);
            label.BackColor = enMarche ? fondEnMarche : fond;
            label.ForeColor = restant < TimeSpan.FromSeconds(20) && (enMarche || restant == TimeSpan.Zero) ? Color.Red : texte;
        }

        // --- Le drapeau de « perdante » est tombé : elle perd la partie (résultat écrit comme pour un gain par prises) ; si le moteur réfléchit, on lui dit d'arrêter ---
        private void FinPartieAuTemps(Couleur perdante)
        {
            if (PartieTerminee)
                return;
            if (jeuMoteurEnCours)
            {   // Son coup, quand il arrivera, ne sera pas joué (PartieTerminee) ; on lui demande de s'arrêter
                try { moteur.Envoie("stop"); }
                catch (Exception ex) when (ex is IOException || ex is InvalidOperationException)
                {
                    Journal.Ecrit("Moteur : « stop » non envoyé à la chute du drapeau : " + ex.Message);
                }
            }
            if (pieceEnMain)
            {   // Une pièce était sélectionnée (son image est sur le curseur) : elle revient sur sa case
                pieceEnMain = false;
                RendLeCurseur();
                EffaceMouvements();
                DessinePiece(IndexPictureBoxSource100, pieceSelectionnee);
                CouleurToutesCasesManoury(CouleurCaseSombre);
                clickCaseSource = true;
            }
            ActiveDamier(false);
            FinAuTemps fin = Pendule.FinDePartie(perdante);     // Le gagnant et les textes viennent de la logique testée
            TermineLaPartie(fin.Gagnant, fin.Statut, fin.Message);
        }
    }
}
