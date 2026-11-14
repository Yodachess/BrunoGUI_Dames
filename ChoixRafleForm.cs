// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Petite fenêtre (modale) où le joueur choisit entre plusieurs rafles de même départ et de même arrivée qui ne prennent pas les mêmes pièces.
// Pour chaque option, la fenêtre principale met en évidence sur le damier les pièces prises et le chemin (fonction montre).

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    public sealed class ChoixRafleForm : Form
    {
        private readonly IReadOnlyList<Coup> options;
        private readonly Action<Coup> montre;
        private int indice;
        private readonly Label titre = new Label();
        private readonly Label detail = new Label();
        private readonly Button precedente = new Button();
        private readonly Button suivante = new Button();
        private readonly Button jouer = new Button();
        private readonly Button annuler = new Button();

        // La rafle choisie, ou null si le joueur a annulé
        public Coup? Choisie { get; private set; }

        public ChoixRafleForm(IReadOnlyList<Coup> optionsPossibles, Action<Coup> montreSurLeDamier)
        {
            options = optionsPossibles;
            montre = montreSurLeDamier;
            Text = "Choix de la rafle";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(390, 150);

            titre.Location = new Point(12, 10);
            titre.Size = new Size(366, 20);
            titre.Font = new Font(Font, FontStyle.Bold);
            detail.Location = new Point(12, 36);
            detail.Size = new Size(366, 54);

            precedente.Text = "< Précédente";
            precedente.Location = new Point(12, 100);
            precedente.Size = new Size(90, 30);
            precedente.Click += (s, e) => Montre(indice - 1);
            suivante.Text = "Suivante >";
            suivante.Location = new Point(108, 100);
            suivante.Size = new Size(90, 30);
            suivante.Click += (s, e) => Montre(indice + 1);
            jouer.Text = "Jouer cette rafle";
            jouer.Location = new Point(204, 100);
            jouer.Size = new Size(112, 30);
            jouer.Click += (s, e) => { Choisie = options[indice]; DialogResult = DialogResult.OK; };
            annuler.Text = "Annuler";
            annuler.Location = new Point(322, 100);
            annuler.Size = new Size(58, 30);
            annuler.Click += (s, e) => { Choisie = null; DialogResult = DialogResult.Cancel; };

            Controls.AddRange(new Control[] { titre, detail, precedente, suivante, jouer, annuler });
            AcceptButton = jouer;
            CancelButton = annuler;
            Shown += (s, e) => Montre(0);
        }

        // --- Passe à l'option numéro i (en tournant) et la met en évidence sur le damier ---
        private void Montre(int i)
        {
            indice = (i + options.Count) % options.Count;
            Coup rafle = options[indice];
            titre.Text = $"Rafle {indice + 1} sur {options.Count} (de {rafle.Depart} à {rafle.Arrivee})";
            detail.Text = $"Pièces prises (en rouge sur le damier) : {string.Join(", ", rafle.Prises)}\nChemin : {rafle.Depart} > {string.Join(" > ", rafle.Chemin)}";
            montre(rafle);
        }
    }
}
