// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// La pendule du jeu et les cadences de la FMJD (annexes 2024 : annexe 3 article 6.1, annexe 6). Logique pure, sans Windows Forms :
// l'heure vient d'une fonction que l'on peut remplacer, ce qui permet de tester la pendule sans attendre.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace BrunoGUI_Dames
{
    // --- Une cadence : le temps de base de chaque joueur, ce qu'il gagne à chaque coup (système Fischer) et le temps ajouté aux contrôles de temps ---
    public sealed class Cadence
    {
        public string Id { get; }                       // Le nom écrit dans les préférences (« classique », « blitz »…)
        public string Nom { get; }                      // Le nom complet, pour le choix de la cadence
        public string Court { get; }                    // Le nom court, pour la liste de la fenêtre
        public TimeSpan Base { get; }                   // Le temps de chaque joueur au départ
        public TimeSpan Increment { get; }              // Le temps gagné à chaque coup (système Fischer)
        public int IncrementDepuisLeCoup { get; }       // L'incrément est gagné à partir de ce coup de chaque joueur (1 : dès le premier)
        public int PremierControle { get; }             // Après ce nombre de coups de chaque joueur, TempsAuControle est ajouté (0 : aucun contrôle)
        public int ControleSuivant { get; }             // Puis tous les ControleSuivant coups (0 : une seule fois)
        public TimeSpan TempsAuControle { get; }        // Le temps ajouté à chaque contrôle

        private Cadence(string id, string nom, string court, TimeSpan baseTemps, TimeSpan increment, int incrementDepuisLeCoup = 1,
                        int premierControle = 0, int controleSuivant = 0, TimeSpan tempsAuControle = default)
        {
            Id = id; Nom = nom; Court = court; Base = baseTemps; Increment = increment; IncrementDepuisLeCoup = incrementDepuisLeCoup;
            PremierControle = premierControle; ControleSuivant = controleSuivant; TempsAuControle = tempsAuControle;
        }

        // --- Les cadences proposées. Classique : annexe 3, article 6.1 de la FMJD (a : le défaut, b : la préférence du comité des joueurs, c : l'ancien standard).
        // Rapide (de 15 à 30 minutes) et blitz (de 5 à 15 minutes) : l'annexe 6 ne fixe que ces fourchettes ; l'incrément est celui des championnats du monde (15 min + 5 s, 5 min + 3 s) ---
        public static readonly IReadOnlyList<Cadence> Officielles = new List<Cadence>
        {
            new Cadence("classique", "Classique : 1 h 20 + 30 s par coup (défaut FMJD)", "1 h 20 + 30 s", TimeSpan.FromMinutes(80), TimeSpan.FromSeconds(30)),
            new Cadence("comite", "Classique : 90 min pour 45 coups, puis 30 min en plus et 30 s par coup dès le 46e", "90 min/45 coups", TimeSpan.FromMinutes(90), TimeSpan.FromSeconds(30),
                        incrementDepuisLeCoup: 46, premierControle: 45, tempsAuControle: TimeSpan.FromMinutes(30)),
            new Cadence("ancien", "Ancien standard : 2 h pour 50 coups, puis 1 h pour chaque série de 25 coups", "2 h/50 coups", TimeSpan.FromHours(2), TimeSpan.Zero,
                        premierControle: 50, controleSuivant: 25, tempsAuControle: TimeSpan.FromHours(1)),
            new Cadence("rapide", "Rapide : 15 min + 5 s par coup", "15 min + 5 s", TimeSpan.FromMinutes(15), TimeSpan.FromSeconds(5)),
            new Cadence("blitz", "Blitz : 5 min + 3 s par coup", "5 min + 3 s", TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(3))
        };

        // --- La cadence de ce nom (sans tenir compte des majuscules), ou null si elle n'existe pas ---
        public static Cadence? Trouve(string? id)
        {
            foreach (Cadence cadence in Officielles)
                if (string.Equals(cadence.Id, id?.Trim(), StringComparison.OrdinalIgnoreCase))
                    return cadence;
            return null;
        }

        // --- Le temps ajouté quand un joueur vient de jouer son coup numéro « coups » (1 pour le premier) : l'incrément puis, aux contrôles, le temps du contrôle ---
        internal TimeSpan GainApresCoup(int coups, bool avecIncrement)
        {
            TimeSpan gain = TimeSpan.Zero;
            if (avecIncrement && Increment > TimeSpan.Zero && coups >= IncrementDepuisLeCoup)
                gain += Increment;
            if (PremierControle > 0 && (coups == PremierControle || (ControleSuivant > 0 && coups > PremierControle && (coups - PremierControle) % ControleSuivant == 0)))
                gain += TempsAuControle;
            return gain;
        }
    }

    // --- Où en est la pendule ---
    public enum EtatPendule
    {
        Armee,      // Elle attend le premier coup : il n'est pas décompté (le temps part ensuite pour l'adversaire)
        EnMarche,   // Le temps du camp au trait s'écoule
        Arretee     // La partie est finie (ou le drapeau est tombé)
    }

    // --- Ce qu'on annonce quand un drapeau tombe : le gagnant, la ligne de statut et le message de la boîte de dialogue ---
    public sealed record FinAuTemps(Couleur Gagnant, string Statut, string Message);

    // --- La pendule d'une partie : le temps restant de chaque camp, qui est au trait, les drapeaux tombés ---
    public sealed class Pendule
    {
        private static readonly TimeSpan TempsRendu = TimeSpan.FromSeconds(30);     // Ce que reprend un camp dont le drapeau était tombé quand on revient en arrière

        private readonly Func<TimeSpan> horloge;
        private readonly TimeSpan[] restant = new TimeSpan[2];       // 0 : blancs, 1 : noirs
        private readonly int[] coups = new int[2];
        private TimeSpan debut;                                      // La lecture de l'horloge au début du temps du camp au trait

        public Cadence Cadence { get; }
        public EtatPendule Etat { get; private set; }
        public Couleur Trait { get; private set; }                   // Le camp dont c'est le tour de jouer (celui dont le temps s'écoule quand elle est en marche)
        public Couleur? Perdant { get; private set; }                // Le camp dont le drapeau est tombé, ou null

        // --- Une pendule armée à la cadence donnée, trait aux blancs ; « horloge » rend le temps écoulé depuis un instant fixe (le chronomètre du système par défaut) ---
        public Pendule(Cadence cadence, Func<TimeSpan>? horloge = null)
        {
            Cadence = cadence;
            if (horloge == null)
            {
                Stopwatch chronometre = Stopwatch.StartNew();
                horloge = () => chronometre.Elapsed;
            }
            this.horloge = horloge;
            Reinitialiser(Couleur.Blanc);
        }

        private static int Indice(Couleur couleur) => couleur == Couleur.Blanc ? 0 : 1;

        // --- Nouvelle partie : chaque camp retrouve son temps de base, la pendule attend le premier coup du camp « trait » ---
        public void Reinitialiser(Couleur trait)
        {
            restant[0] = restant[1] = Cadence.Base;
            coups[0] = coups[1] = 0;
            Perdant = null;
            Trait = trait;
            Etat = EtatPendule.Armee;
        }

        // --- Retour arrière : la pendule s'arrête, garde les temps actuels et attend le prochain coup du camp « trait » (qui n'est pas décompté) ;
        // un camp dont le drapeau était tombé reprend avec 30 secondes ---
        public void Armer(Couleur trait)
        {
            Charge();
            foreach (int i in new[] { 0, 1 })
                if (restant[i] <= TimeSpan.Zero)
                    restant[i] = TempsRendu;
            Perdant = null;
            Trait = trait;
            Etat = EtatPendule.Armee;
        }

        // --- Le temps de l'horloge écoulé depuis le début du temps du camp au trait est retiré de son temps restant ; vrai si la pendule était en marche ---
        private bool Charge()
        {
            if (Etat != EtatPendule.EnMarche)
                return false;
            restant[Indice(Trait)] -= horloge() - debut;
            debut = horloge();
            return true;
        }

        // --- Le camp au trait vient de jouer (voir CoupJoue(Couleur)) ---
        public bool CoupJoue() => CoupJoue(Trait);

        // --- Le camp « joueur » vient de jouer : son temps est décompté, il gagne l'incrément et le temps des contrôles, l'autre camp commence.
        // Si ce n'est pas le camp que la pendule croyait au trait (on a joué depuis une position parcourue, par exemple), c'est lui qui est décompté : le temps
        // écoulé depuis le dernier coup lui revient, l'autre camp n'a rien perdu. Rend faux si son drapeau était déjà tombé : le coup ne compte pas et il perd (Perdant).
        // Le premier coup d'une pendule armée n'est pas décompté ---
        public bool CoupJoue(Couleur joueur)
        {
            if (Etat != EtatPendule.Arretee)
                Trait = joueur;
            int i = Indice(Trait);
            if (Etat == EtatPendule.Arretee)
                return Perdant == null;
            bool premierCoup = Etat == EtatPendule.Armee;
            if (!premierCoup)
            {
                Charge();
                if (restant[i] <= TimeSpan.Zero)
                {
                    restant[i] = TimeSpan.Zero;
                    Perdant = Trait;
                    Etat = EtatPendule.Arretee;
                    return false;
                }
            }
            coups[i]++;
            restant[i] += Cadence.GainApresCoup(coups[i], avecIncrement: !premierCoup);
            Trait = Position.Adversaire(Trait);
            debut = horloge();
            Etat = EtatPendule.EnMarche;
            return true;
        }

        // --- À appeler régulièrement : rend le camp dont le drapeau vient de tomber (la pendule s'arrête et il a perdu), ou null ---
        public Couleur? Verifie()
        {
            if (Etat != EtatPendule.EnMarche)
                return null;
            Charge();
            int i = Indice(Trait);
            if (restant[i] > TimeSpan.Zero)
                return null;
            restant[i] = TimeSpan.Zero;
            Perdant = Trait;
            Etat = EtatPendule.Arretee;
            return Trait;
        }

        // --- La fin de partie au temps : le camp qui gagne, la ligne de statut et le message annoncés quand le drapeau de « perdante » est tombé ---
        public static FinAuTemps FinDePartie(Couleur perdante)
        {
            Couleur gagnante = Position.Adversaire(perdante);
            string gagnants = gagnante == Couleur.Blanc ? "Blancs" : "Noirs";
            string drapeau = perdante == Couleur.Blanc ? "des blancs" : "des noirs";
            return new FinAuTemps(gagnante, $"Les {gagnants} gagnent au temps", $"Les {gagnants} gagnent !\nLe drapeau {drapeau} est tombé.");
        }

        // --- La partie est finie : le temps s'arrête ---
        public void Arreter()
        {
            Charge();
            if (Etat != EtatPendule.Arretee)
                Etat = EtatPendule.Arretee;
        }

        // --- Le temps restant d'un camp, en comptant celui qui s'écoule s'il est au trait (jamais négatif) ---
        public TimeSpan TempsRestant(Couleur couleur)
        {
            TimeSpan temps = restant[Indice(couleur)];
            if (Etat == EtatPendule.EnMarche && couleur == Trait)
                temps -= horloge() - debut;
            return temps < TimeSpan.Zero ? TimeSpan.Zero : temps;
        }

        // --- Le nombre de coups joués (et décomptés ou non) par un camp depuis le début ---
        public int CoupsJoues(Couleur couleur) => coups[Indice(couleur)];

        // --- Le temps de réflexion à donner au moteur pour un coup, en secondes entières (au moins 1) : jamais plus que « plafond » (le réglage du joueur),
        // un vingtième du temps restant plus presque tout l'incrément, et jamais plus de la moitié du temps restant ---
        public int TempsConseille(Couleur couleur, int plafond)
        {
            double reste = TempsRestant(couleur).TotalSeconds;
            bool incrementAGagner = Cadence.Increment > TimeSpan.Zero && CoupsJoues(couleur) + 1 >= Cadence.IncrementDepuisLeCoup && Etat != EtatPendule.Armee;
            double increment = incrementAGagner ? Cadence.Increment.TotalSeconds : 0;
            int budget = (int)Math.Floor(reste / 20 + increment * 0.8);
            int moitie = (int)Math.Floor(reste / 2);
            return Math.Max(1, Math.Min(plafond, Math.Min(budget, moitie)));
        }

        // --- Le temps tel qu'une pendule l'affiche : « 1:19:42 » à partir d'une heure, « 04:07 » sinon ; arrondi à la seconde supérieure (0:00 seulement quand le drapeau est tombé) ---
        public static string Formate(TimeSpan temps)
        {
            if (temps < TimeSpan.Zero)
                temps = TimeSpan.Zero;
            int secondes = (int)Math.Ceiling(temps.TotalSeconds);
            int heures = secondes / 3600;
            int minutes = secondes % 3600 / 60;
            int reste = secondes % 60;
            return heures > 0
                ? string.Create(CultureInfo.InvariantCulture, $"{heures}:{minutes:00}:{reste:00}")
                : string.Create(CultureInfo.InvariantCulture, $"{minutes:00}:{reste:00}");
        }
    }
}
