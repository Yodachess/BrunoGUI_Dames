// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Les points par lesquels les tests (projet Tests, ScenarioInterface.cs) pilotent la vraie fenêtre : ce qu'ils remplacent, le programme normal ne le touche
// jamais. Les valeurs par défaut sont celles de l'usage normal.

using System;
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    internal static class ReglagesDeTest
    {
        // --- Toutes les boîtes de message du formulaire passent par là (un test les enregistre au lieu d'attendre qu'on clique) ---
        internal static Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> Afficheur =
            (texte, titre, boutons, icone) => MessageBox.Show(texte, titre, boutons, icone);
        // --- Le texte est déjà traduit par l'appelant (il contient des valeurs) ; le titre est toujours un texte fixe, traduit ici ---
        internal static DialogResult Message(string texte, string titre, MessageBoxButtons boutons, MessageBoxIcon icone) => Afficheur(texte, Texte.T(titre), boutons, icone);

        // --- La fenêtre « À propos » (fonction qui écrit le texte dans la langue en cours, langue actuelle) : rend la langue choisie dans la liste (« fr » ou « en ») ; un test la remplace pour ne pas attendre qu'on la ferme ---
        internal static Func<Func<string>, string, string> APropos = (texte, langue) =>
        {
            using (AProposForm fenetre = new AProposForm(texte, langue))
            {
                fenetre.ShowDialog();
                return fenetre.LangueChoisie;
            }
        };

        internal static Func<TimeSpan>? HorlogePendule;     // L'heure de la pendule (null : le chronomètre du système)
        internal static string? FichierPreferences;         // Le fichier de préférences (null : à côté de l'exe)
        internal static string? CheminMoteur;               // Le chemin de scan.exe (null : à côté de l'exe)
    }
}
