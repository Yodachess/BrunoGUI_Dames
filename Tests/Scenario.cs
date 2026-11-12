// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Les vérifications de la logique du jeu de dames (Position, ReglesDames), sans l'interface graphique. Chacune est enregistrée
// dans la liste renvoyée par Scenario.Executer :
//  - Program.cs les affiche en console (dotnet run --project Tests [-- --complet]) ;
//  - VerificationsTests.cs en fait un test xUnit par vérification (Explorateur de tests de Visual Studio, dotnet test Tests).
// Pour ajouter une vérification de coups : VerifieCoups(nom, fen, "coups attendus en notation Hub2 séparés par des espaces").
// La numérotation des cases est celle de Geometrie : de 1 (en haut à gauche) à 50, les blancs montent vers la case 1.

using System;
using System.Collections.Generic;
using System.Linq;
using BrunoGUI_Dames;

public record Verification(string Nom, bool Ok, string Detail);

public static class Scenario
{
    public static List<Verification> Executer(bool complet)
    {
        List<Verification> verifications = [];

        void Verifie(string nom, bool ok, string detail)
        {
            Console.WriteLine($"{(ok ? "OK   " : "ECHEC")} {nom} -> {detail}");
            verifications.Add(new Verification(nom, ok, detail));
        }

        // Compare les coups légaux d'une position (en notation Hub2, sans tenir compte de l'ordre des coups ni de l'ordre des prises)
        // avec ceux qu'on attend
        void VerifieCoups(string nom, string fen, string attendus)
        {
            string obtenus = Ensemble(ReglesDames.CoupsLegaux(Position.DepuisFen(fen)).Select(c => c.Hub2()));
            string voulus = Ensemble(attendus.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            Verifie(nom, obtenus == voulus, obtenus == voulus ? $"{fen} : {obtenus}" : $"{fen} : obtenus [{obtenus}], attendus [{voulus}]");
        }

        // ═══════════════ Position, FEN, Hub2 ═══════════════
        Console.WriteLine("── Position ──");

        Position depart = Position.Initiale();
        Verifie("Position initiale : 20 pions de chaque couleur, aucune dame, trait aux blancs",
                depart.Compte(Piece.PionBlanc) == 20 && depart.Compte(Piece.PionNoir) == 20 && depart.Compte(Piece.DameBlanche) == 0
                && depart.Compte(Piece.DameNoire) == 0 && depart.Trait == Couleur.Blanc,
                $"blancs {depart.Compte(Piece.PionBlanc)}, noirs {depart.Compte(Piece.PionNoir)}, trait {depart.Trait}");
        Verifie("Position initiale : noirs sur 1 à 20, blancs sur 31 à 50",
                Enumerable.Range(1, 20).All(c => depart[c] == Piece.PionNoir) && Enumerable.Range(21, 10).All(c => depart[c] == Piece.Vide)
                && Enumerable.Range(31, 20).All(c => depart[c] == Piece.PionBlanc), "cases 1 à 50");
        Verifie("Position initiale : chaîne Hub2 du protocole", depart.VersHub2() == "Wbbbbbbbbbbbbbbbbbbbbeeeeeeeeeewwwwwwwwwwwwwwwwwwww", depart.VersHub2());
        string fenDepart = "W:W31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50:B1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20";
        Verifie("Position initiale : FEN", depart.VersFen() == fenDepart, depart.VersFen());
        Verifie("FEN : lecture avec l'enrobage [FEN \"...\"] et les suites de cases",
                Position.DepuisFen("[FEN \"W:W31-50:B1-20\"]").Equals(depart), "[FEN \"W:W31-50:B1-20\"]");

        Position avecDames = Position.DepuisFen("B:WK10,31-33:B1,K20");
        Verifie("FEN : dames (K), suites de cases et trait aux noirs",
                avecDames[10] == Piece.DameBlanche && avecDames[31] == Piece.PionBlanc && avecDames[33] == Piece.PionBlanc
                && avecDames[1] == Piece.PionNoir && avecDames[20] == Piece.DameNoire && avecDames.Trait == Couleur.Noir
                && avecDames.Compte(Piece.PionBlanc) == 3,
                avecDames.VersHub2());
        Verifie("FEN : écriture puis relecture à l'identique", Position.DepuisFen(avecDames.VersFen()).Equals(avecDames) && avecDames.VersFen() == "B:WK10,31,32,33:B1,K20",
                avecDames.VersFen());

        foreach (string invalide in new[] { "X:W31:B1", "W:W51:B1", "W:W0:B1", "W:W31,31:B1", "W:W31-30:B1", "W31:B1", "W:W3a:B1" })
        {
            string detail;
            try { Position.DepuisFen(invalide); detail = "aucune exception"; }
            catch (FormatException) { detail = ""; }
            Verifie($"FEN invalide refusée : {invalide}", detail == "", detail == "" ? "FormatException" : detail);
        }

        string hub2Protocole = "BeeeWWeeeeeeeeeeeeeeeeeeeweeeeeeeeewweBeeeeeeeeeBee";      // Exemple de protocol.txt (Scan)
        Position depuisHub2 = Position.DepuisHub2(hub2Protocole);
        Verifie("Hub2 : l'exemple du protocole se relit à l'identique",
                hub2Protocole.Length == 51 && depuisHub2.VersHub2() == hub2Protocole && depuisHub2.Trait == Couleur.Noir
                && depuisHub2[4] == Piece.DameBlanche && depuisHub2[5] == Piece.DameBlanche,
                $"{hub2Protocole.Length} caractères, {depuisHub2.VersFen()}");

        Position copie = depart.Copie();
        copie[28] = Piece.PionBlanc;
        copie.Trait = Couleur.Noir;
        Verifie("Copie indépendante de la position d'origine", depart[28] == Piece.Vide && depart.Trait == Couleur.Blanc && !copie.Equals(depart), "la copie modifiée n'a pas changé l'original");

        // ═══════════════ Déplacements simples ═══════════════
        Console.WriteLine("── Déplacements ──");

        VerifieCoups("Position initiale : 9 coups pour les blancs", "W:W31-50:B1-20", "31-26 31-27 32-27 32-28 33-28 33-29 34-29 34-30 35-30");
        VerifieCoups("Position initiale : 9 coups pour les noirs", "B:W31-50:B1-20", "16-21 17-21 17-22 18-22 18-23 19-23 19-24 20-24 20-25");
        VerifieCoups("Pion blanc au bord du damier : une seule case", "W:W26:B1", "26-21");
        VerifieCoups("Pion noir : avance seulement vers le bas", "B:W46:B15", "15-20");
        VerifieCoups("Pion bloqué par une pièce amie", "W:W28,22,23:B1", "22-17 22-18 23-18 23-19");
        VerifieCoups("Dame : toute la grande diagonale", "W:WK46:B1", "46-41 46-37 46-32 46-28 46-23 46-19 46-14 46-10 46-5");
        VerifieCoups("Dame : arrêtée par sa propre pièce", "W:WK46,37:B1", "46-41 37-31 37-32");
        VerifieCoups("Dame en milieu de damier : les quatre diagonales", "W:WK28:B1",
                     "28-22 28-17 28-11 28-6 28-23 28-19 28-14 28-10 28-5 28-32 28-37 28-41 28-46 28-33 28-39 28-44 28-50");

        // ═══════════════ Prises ═══════════════
        Console.WriteLine("── Prises ──");

        VerifieCoups("Prise simple d'un pion, obligatoire (pas de déplacement simple)", "W:W28:B22", "28x17x22");
        VerifieCoups("Prise en arrière par un pion", "W:W28:B33", "28x39x33");
        VerifieCoups("Prise par un pion noir", "B:W23:B18", "18x29x23");
        VerifieCoups("Pas de prise si la case derrière la pièce est occupée", "W:W28:B17,22", "28-23");
        VerifieCoups("Pas de prise de sa propre pièce", "W:W28,22:B1", "28-23 22-17 22-18");
        VerifieCoups("Prise obligatoire avec une pièce, même si une autre peut se déplacer", "W:W28,34:B22", "28x17x22");
        VerifieCoups("Prise majoritaire : la rafle de 2 pièces est seule légale, pas celle d'une pièce", "W:W28:B11,22,23", "28x6x22x11");
        VerifieCoups("Prise majoritaire entre deux pions : seul celui qui prend le plus joue", "W:W28,45:B22,29,40", "45x23x29x40");
        VerifieCoups("Dame volante : toutes les cases d'arrivée derrière la pièce prise", "W:WK46:B37",
                     "46x32x37 46x28x37 46x23x37 46x19x37 46x14x37 46x10x37 46x5x37");
        VerifieCoups("Dame : doit poursuivre la rafle et choisit sa case d'arrivée", "W:WK46:B17,37", "46x11x37x17 46x6x37x17");
        VerifieCoups("Rafle en boucle : retour sur la case de départ, un seul coup", "W:W28:B12,13,22,23", "28x28x12x13x22x23");
        VerifieCoups("Pion : la rafle passe par la dernière rangée et continue", "W:W13:B9,10", "13x15x9x10");

        // Sans la majorité, le pion 28 pourrait prendre 22 (une pièce) : avec elle, seule la rafle de 2 pièces reste légale
        List<Coup> deuxPieces = ReglesDames.CoupsLegaux(Position.DepuisFen("W:W28,45:B22,29,40"));
        Verifie("Prise majoritaire : la prise d'une seule pièce par l'autre pion est écartée",
                deuxPieces.Count == 1 && deuxPieces[0].Depart == 45 && deuxPieces[0].Prises.Count == 2, string.Join(" ", deuxPieces.Select(c => c.Hub2())));

        List<Coup> boucle = ReglesDames.CoupsLegaux(Position.DepuisFen("W:W28:B12,13,22,23"));
        Verifie("Rafle en boucle : 4 pièces prises et départ = arrivée",
                boucle.Count == 1 && boucle[0].Depart == 28 && boucle[0].Arrivee == 28 && boucle[0].Prises.Count == 4,
                string.Join(" ", boucle.Select(c => c.Hub2())));

        // ═══════════════ Exécution d'un coup ═══════════════
        Console.WriteLine("── Exécution des coups ──");

        Position avantPrise = Position.DepuisFen("W:W28:B22,1");
        Coup? prise = ReglesDames.TrouverCoup(avantPrise, "28x17x22");
        Position apresPrise = prise == null ? avantPrise : ReglesDames.Jouer(avantPrise, prise);
        Verifie("Prise : la pièce prise disparaît, la pièce arrive, le trait change",
                prise != null && apresPrise[22] == Piece.Vide && apresPrise[28] == Piece.Vide && apresPrise[17] == Piece.PionBlanc && apresPrise.Trait == Couleur.Noir,
                apresPrise.VersFen());
        Verifie("Jouer ne modifie pas la position d'origine", avantPrise[28] == Piece.PionBlanc && avantPrise[22] == Piece.PionNoir && avantPrise.Trait == Couleur.Blanc,
                avantPrise.VersFen());

        Position avantPromotion = Position.DepuisFen("W:W10:B1");
        Position apresPromotion = ReglesDames.Jouer(avantPromotion, ReglesDames.TrouverCoup(avantPromotion, "10-5")!);
        Verifie("Promotion d'un pion blanc arrivé sur la case 5", apresPromotion[5] == Piece.DameBlanche && apresPromotion[10] == Piece.Vide, apresPromotion.VersFen());

        Position avantPromotionNoire = Position.DepuisFen("B:W1:B45");
        Position apresPromotionNoire = ReglesDames.Jouer(avantPromotionNoire, ReglesDames.TrouverCoup(avantPromotionNoire, "45-50")!);
        Verifie("Promotion d'un pion noir arrivé sur la case 50", apresPromotionNoire[50] == Piece.DameNoire, apresPromotionNoire.VersFen());

        Position avantRafleDamee = Position.DepuisFen("W:W13:B9");
        Position apresRafleDamee = ReglesDames.Jouer(avantRafleDamee, ReglesDames.TrouverCoup(avantRafleDamee, "13x4x9")!);
        Verifie("Promotion d'un pion qui termine sa prise sur la dernière rangée", apresRafleDamee[4] == Piece.DameBlanche && apresRafleDamee[9] == Piece.Vide, apresRafleDamee.VersFen());

        Position avantRafle = Position.DepuisFen("W:W13:B9,10");
        Position apresRafle = ReglesDames.Jouer(avantRafle, ReglesDames.TrouverCoup(avantRafle, "13x15x9x10")!);
        Verifie("Pas de promotion en traversant la dernière rangée au milieu d'une rafle", apresRafle[15] == Piece.PionBlanc && apresRafle.Compte(Piece.DameBlanche) == 0, apresRafle.VersFen());

        Verifie("TrouverCoup : notation abrégée « 28x17 » et complète « 28x17x22 » désignent le même coup",
                ReglesDames.TrouverCoup(avantPrise, "28x17")?.Hub2() == "28x17x22" && ReglesDames.TrouverCoup(avantPrise, "28x17x22")?.Hub2() == "28x17x22",
                ReglesDames.TrouverCoup(avantPrise, "28x17")?.Hub2() ?? "(aucun)");
        Verifie("TrouverCoup : coup illégal ou déplacement simple pendant une prise obligatoire refusés",
                ReglesDames.TrouverCoup(avantPrise, "28-23") == null && ReglesDames.TrouverCoup(avantPrise, "28x17x23") == null && ReglesDames.TrouverCoup(avantPrise, "n'importe quoi") == null,
                "28-23, 28x17x23, texte invalide");

        // ═══════════════ Fin de partie ═══════════════
        Console.WriteLine("── Fin de partie ──");

        Verifie("Position initiale : partie non terminée", !ReglesDames.EstTerminee(Position.Initiale()), "9 coups possibles");
        Verifie("Camp au trait sans aucune pièce : partie terminée", ReglesDames.EstTerminee(Position.DepuisFen("W:W:B1")), "W:W:B1");
        Verifie("Camp au trait dont toutes les pièces sont bloquées : partie terminée", ReglesDames.EstTerminee(Position.DepuisFen("W:W6:B1")), "W:W6:B1 (le pion 6 est bloqué par le pion 1)");

        // ═══════════════ Partie réelle de Scan ═══════════════
        Console.WriteLine("── Partie de Scan ──");

        // Ligne principale que Scan donne dans protocol.txt, depuis la position initiale : 28 demi-coups avec de nombreuses prises.
        // Chacun doit être l'un des coups légaux de la position où il est joué (notation Hub2 des prises : pièces prises sans les pivots).
        string ligneScan = "32-28 17-22 28x17x22 12x21x17 33-28 7-12 39-33 19-23 28x19x23 14x23x19 44-39 9-14 50-44 4-9 34-29 23x34x29 39x30x34 21-26 44-39 11-17 31-27 6-11 37-31 26x37x31 41x32x37 20-24 30x19x24 13x24x19";
        Position courante = Position.Initiale();
        int coupsJoues = 0;
        string premierRefus = "";
        foreach (string notation in ligneScan.Split(' '))
        {
            Coup? coup = ReglesDames.TrouverCoup(courante, notation);
            if (coup == null) { premierRefus = $"demi-coup {coupsJoues + 1} « {notation} » refusé dans {courante.VersFen()} (légaux : {string.Join(" ", ReglesDames.CoupsLegaux(courante))})"; break; }
            courante = ReglesDames.Jouer(courante, coup);
            coupsJoues++;
        }
        Verifie("Ligne de Scan (protocol.txt) : les 28 demi-coups sont légaux", coupsJoues == 28, coupsJoues == 28 ? courante.VersFen() : premierRefus);

        // Séquence de l'exemple du protocole, jouée depuis la position Hub2 de l'exemple (dames seulement)
        string exempleMoves = "38-15 5-41 48-26";
        Position exemple = depuisHub2.Copie();
        int exempleJoues = 0;
        foreach (string notation in exempleMoves.Split(' '))
        {
            Coup? coup = ReglesDames.TrouverCoup(exemple, notation);
            if (coup == null) break;
            exemple = ReglesDames.Jouer(exemple, coup);
            exempleJoues++;
        }
        Verifie("Exemple de protocol.txt (pos + moves) : les 3 coups de dames sont légaux", exempleJoues == 3, $"{exempleJoues} coups joués sur 3");

        // ═══════════════ Perft ═══════════════
        // Nombre de positions atteintes après n demi-coups depuis la position initiale : valeurs de référence de la littérature
        // des dames internationales (mêmes valeurs pour tous les générateurs de coups conformes aux règles).
        Console.WriteLine("── Perft ──");

        long[] perftReference = [9, 81, 658, 4265, 27117, 167140, 1049442];
        int profondeurMax = complet ? 7 : 6;
        for (int profondeur = 1; profondeur <= profondeurMax; profondeur++)
        {
            long obtenu = Perft(Position.Initiale(), profondeur);
            Verifie($"Perft {profondeur} depuis la position initiale", obtenu == perftReference[profondeur - 1], $"{obtenu} (attendu {perftReference[profondeur - 1]})");
        }

        return verifications;
    }

    // Les coups d'une liste sous une forme où l'ordre des coups et l'ordre des pièces prises ne comptent pas
    private static string Ensemble(IEnumerable<string> coups) => string.Join(" ", coups.Select(Cle).OrderBy(c => c, StringComparer.Ordinal));

    private static string Cle(string notation)
    {
        int[] cases = notation.Split('-', 'x').Select(int.Parse).ToArray();
        return notation.Contains('x') ? $"{cases[0]}x{cases[1]}x" + string.Join("x", cases.Skip(2).OrderBy(n => n)) : $"{cases[0]}-{cases[1]}";
    }

    private static long Perft(Position position, int profondeur)
    {
        List<Coup> coups = ReglesDames.CoupsLegaux(position);
        if (profondeur == 1) return coups.Count;
        long total = 0;
        foreach (Coup coup in coups)
            total += Perft(ReglesDames.Jouer(position, coup), profondeur - 1);
        return total;
    }
}
