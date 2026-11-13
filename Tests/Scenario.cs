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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        // Règle essentielle : au cours d'une rafle on peut passer deux fois sur une même case vide, mais pas deux fois sur la même pièce ;
        // les pièces prises ne sont enlevées qu'une fois la pièce prenante posée sur sa case terminale (elles bloquent donc le passage)

        // La dame en 29 prend 40 et se pose en 45 : de là, la diagonale vers 29 passe par 40, pièce déjà prise, qui reste sur le damier
        // et ne peut être ni sautée une seconde fois ni traversée. Si 40 était retirée tout de suite, la dame traverserait sa case et sa
        // propre case de départ pour prendre aussi 7 (« 29x45x40x7 ») : ce coup de deux pièces ne doit pas exister.
        List<Coup> pieceBloquante = ReglesDames.CoupsLegaux(Position.DepuisFen("W:WK29:B3,7,40"));
        VerifieCoups("Rafle : une pièce déjà prise reste sur le damier, ne peut être sautée deux fois ni traversée", "W:WK29:B3,7,40", "29x45x40 29x1x7");
        Verifie("Rafle : aucune rafle de la dame en 29 ne prend plus d'une pièce", pieceBloquante.All(c => c.Prises.Count == 1), string.Join(" ", pieceBloquante.Select(c => c.Hub2())));

        // La dame en 7 prend 23, 24, 14 puis 31 : sa dernière diagonale (9 vers 36) repasse par la case vide 18, déjà traversée au premier
        // trajet (7 vers 29). Passer deux fois sur une case vide est permis, et c'est le seul coup légal.
        List<Coup> deuxFoisVide = ReglesDames.CoupsLegaux(Position.DepuisFen("W:WK7:B14,23,24,31"));
        Verifie("Rafle : la dame repasse deux fois sur la même case vide (18) et prend 4 pièces",
                deuxFoisVide.Count == 1 && deuxFoisVide[0].Hub2() == "7x36x23x24x14x31" && string.Join(">", deuxFoisVide[0].Chemin) == "29>20>9>36" && deuxFoisVide[0].Pdn() == "7x36",
                string.Join(" ", deuxFoisVide.Select(c => $"{c.Hub2()} via {string.Join(">", c.Chemin)}")));
        Position apresDeuxFoisVide = ReglesDames.Jouer(Position.DepuisFen("W:WK7:B14,23,24,31"), deuxFoisVide[0]);
        Verifie("Rafle : les 4 pièces prises disparaissent à la fin, la dame est en 36",
                apresDeuxFoisVide[36] == Piece.DameBlanche && new[] { 7, 14, 23, 24, 31 }.All(n => apresDeuxFoisVide[n] == Piece.Vide) && apresDeuxFoisVide.Trait == Couleur.Noir,
                apresDeuxFoisVide.VersFen());

        // ═══════════════ Coup du moteur ═══════════════
        Console.WriteLine("── Coup du moteur ──");

        // Le coup de Scan est cherché dans la position Hub2 envoyée au moteur (le trait est celui de cette chaîne), avec la notation complète du coup
        string hub2NoirsAuTrait = Position.DepuisFen("B:W31-50:B1-20").VersHub2();
        Coup? coupDeScanSimple = ReglesDames.TrouverCoup(Position.DepuisHub2(hub2NoirsAuTrait), "17-22");
        Verifie("Coup du moteur : « 17-22 » retrouvé dans une position Hub2 où les noirs ont le trait", coupDeScanSimple != null && coupDeScanSimple.Depart == 17 && coupDeScanSimple.Arrivee == 22, coupDeScanSimple?.Hub2() ?? "(aucun)");
        Coup? coupDeScanAuxBlancs = ReglesDames.TrouverCoup(Position.DepuisHub2(hub2NoirsAuTrait), "32-28");
        Verifie("Coup du moteur : un coup de l'autre camp n'est pas retrouvé", coupDeScanAuxBlancs == null, coupDeScanAuxBlancs?.Hub2() ?? "(aucun)");
        Position avantRafleScan = Position.DepuisFen("W:WK29:B3,7,40");
        Coup? rafleDeScan = ReglesDames.TrouverCoup(Position.DepuisHub2(avantRafleScan.VersHub2()), "29x45x40");
        Verifie("Coup du moteur : « 29x45x40 » donne la rafle exacte (départ 29, arrivée 45, pièce prise 40, étapes pièce prise / case d'arrivée)",
                rafleDeScan != null && rafleDeScan.Prises.Zip(rafleDeScan.Chemin, (prise, arrivee) => (prise, arrivee)).SequenceEqual([(40, 45)]), rafleDeScan?.Hub2() ?? "(aucun)");

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

        // ═══════════════ PDN : lecture ═══════════════
        Console.WriteLine("── PDN : lecture ──");

        static string Coups(PartiePdn partie) => string.Join(" ", partie.Coups);

        List<PartiePdn> classique = LecturePdn.LireTexte(
            "[Event \"Entrainement\"]\n[Site \"Maison\"]\n[Date \"2025.02.22\"]\n[Round \"1\"]\n[White \"Brigitte\"]\n[Black \"SCAN 3.1\"]\n[Result \" 0-1\"]\n\n" +
            "[PlyCount \"6\"]\n\n 1. 31-26 18-23 2. 37-31 12-18\n 3. 41-37 7-12 0-1\n");
        Verifie("PDN : partie classique (balises, coups numérotés, résultat, balise après une ligne vide)",
                classique.Count == 1 && classique[0].Event == "Entrainement" && classique[0].White == "Brigitte" && classique[0].Black == "SCAN 3.1"
                && classique[0].Result == "0-1" && classique[0].Balise("PlyCount") == "6" && classique[0].Resultat == "0-1"
                && Coups(classique[0]) == "31-26 18-23 37-31 12-18 41-37 7-12" && classique[0].Avertissements.Count == 0,
                classique.Count == 1 ? $"{classique[0].White} / {Coups(classique[0])} / {classique[0].Resultat}" : $"{classique.Count} parties");

        List<PartiePdn> uneLigne = LecturePdn.LireTexte("[Event \"Open\"] [White \"Tomczak, Patrycja (7)\"] [Black \"Cichocki, Karol\"] [Result \"2-0\"]  1.33-29 19-23 2.35-30 14-19 3.30-25 2-0");
        Verifie("PDN : balises et coups sur une même ligne, numéro collé au coup, parenthèses dans une balise",
                uneLigne.Count == 1 && uneLigne[0].White == "Tomczak, Patrycja (7)" && uneLigne[0].Balises.Count == 4
                && Coups(uneLigne[0]) == "33-29 19-23 35-30 14-19 30-25" && uneLigne[0].Resultat == "2-0",
                uneLigne.Count == 1 ? $"{uneLigne[0].White} / {Coups(uneLigne[0])}" : $"{uneLigne.Count} parties");

        List<PartiePdn> tabulations = LecturePdn.LireTexte("\t\t\t[Event \"Serie A\"]\r\n\t\t\t[White \"Ramassamy, Albert\"]\r\n\t\t\t[Result \"1-1\"]\r\n\t\t\t1. 31-26 17-21\r\n\t\t\t2. 26x17 12x21\r\n\t\t\t3. 34-30 1-1\r\n\r\n\t\t\t[Event \"Serie B\"]\r\n\t\t\t1. 32-28 *\r\n");
        Verifie("PDN : balises précédées de tabulations (fichiers Montpellier), fins de ligne CRLF",
                tabulations.Count == 2 && tabulations[0].Event == "Serie A" && Coups(tabulations[0]) == "31-26 17-21 26x17 12x21 34-30" && tabulations[0].Resultat == "1-1"
                && tabulations[1].Event == "Serie B" && Coups(tabulations[1]) == "32-28",
                $"{tabulations.Count} parties");

        List<PartiePdn> annotee = LecturePdn.LireTexte("[Event \"X\"]\n\n1. 32-28 {bon coup (vraiment)} 18-23?! (1... 17-21 2. 31-26 {variante (imbriquée)}) $1 2. 28x19 {Best move was 28-6.} 14x23!! ; commentaire jusqu'à la fin de la ligne\n3. 37-32\n1-0");
        Verifie("PDN : commentaires, variantes, annotations et NAG ignorés",
                annotee.Count == 1 && Coups(annotee[0]) == "32-28 18-23 28x19 14x23 37-32" && annotee[0].Resultat == "1-0" && annotee[0].Avertissements.Count == 0,
                annotee.Count == 1 ? $"{Coups(annotee[0])} / avertissements : {string.Join("; ", annotee[0].Avertissements)}" : $"{annotee.Count} parties");

        List<PartiePdn> barreR = LecturePdn.LireTexte("[Event \"A\"]\\r\n[Site \"B\"]\\r\n\\r\n1. 31-26 19-23 2. 37-31 14-19 3. 41-37 10-14 6.\\r\n34-29 23x34\\r\n");
        Verifie("PDN : « \\r » écrits en toutes lettres entre les lignes (export défectueux) traités comme des blancs",
                barreR.Count == 1 && barreR[0].Event == "A" && barreR[0].Site == "B" && Coups(barreR[0]) == "31-26 19-23 37-31 14-19 41-37 10-14 34-29 23x34",
                barreR.Count == 1 ? $"{barreR[0].Event}/{barreR[0].Site} {Coups(barreR[0])}" : $"{barreR.Count} parties");

        List<PartiePdn> deux = LecturePdn.LireTexte("[Event \"A\"]\n\n1. 32-28 17-22\n\n[Event \"B\"]\n\n1. 31-26 *\n");
        Verifie("PDN : la balise suivante commence une nouvelle partie, même sans résultat à la fin de la précédente",
                deux.Count == 2 && deux[0].Resultat == null && Coups(deux[0]) == "32-28 17-22" && deux[1].Resultat == "*" && Coups(deux[1]) == "31-26",
                $"{deux.Count} parties");

        List<PartiePdn> sansBalises = LecturePdn.LireTexte("1. 32-28 17-22 1-0 1. 31-26 19-23 0-1");
        Verifie("PDN : parties sans balises séparées par leur résultat",
                sansBalises.Count == 2 && Coups(sansBalises[0]) == "32-28 17-22" && sansBalises[0].Resultat == "1-0" && Coups(sansBalises[1]) == "31-26 19-23" && sansBalises[1].Resultat == "0-1",
                $"{sansBalises.Count} parties");

        Verifie("PDN : zéros devant les numéros de cases normalisés (« 23x05 » devient « 23x5 »)",
                Coups(LecturePdn.LireTexte("[Event \"Z\"]\n1. 43-39 37x46 2. 28-23 46x35 3. 23x05 09-14 *")[0]) == "43-39 37x46 28-23 46x35 23x5 9-14", "23x05 09-14");

        PartiePdn echappees = LecturePdn.LireTexte("[Event \"Open \\\"d'été\\\" \\\\ test\"]\n[Black \"D\\'Almeida, Jean\"]\n[White \"?\"]\n[Date \"????.??.??\"]\n1. 32-28 *")[0];
        Verifie("PDN : valeurs de balises avec \\\" et \\\\ ; « \\' » et les « ? » conservés tels quels",
                echappees.Event == "Open \"d'été\" \\ test" && echappees.Black == "D\\'Almeida, Jean" && echappees.White == "?" && echappees.Date == "????.??.??",
                $"{echappees.Event} | {echappees.Black} | {echappees.White} | {echappees.Date}");

        List<PartiePdn> forfait = LecturePdn.LireTexte("[Event \"X\"]\n1. 32-28 17-22 -");
        Verifie("PDN : le résultat « - » (forfait) est reconnu", forfait.Count == 1 && forfait[0].Resultat == "-" && Coups(forfait[0]) == "32-28 17-22", forfait.Count == 1 ? Coups(forfait[0]) : "");

        List<PartiePdn> noirsDabord = LecturePdn.LireTexte("[Event \"X\"]\n[FEN \"B:W31-50:B1-20\"]\n1... 17-22 2. 31-26 *");
        Verifie("PDN : « 1... » (les noirs jouent en premier) ne change pas la liste des coups", noirsDabord.Count == 1 && Coups(noirsDabord[0]) == "17-22 31-26", Coups(noirsDabord[0]));

        Verifie("PDN : texte vide ou sans partie : aucune partie",
                LecturePdn.LireTexte("").Count == 0 && LecturePdn.LireTexte("  \r\n \t ").Count == 0 && LecturePdn.LireTexte("{ juste un commentaire }").Count == 0, "texte vide, blancs, commentaire seul");
        List<PartiePdn> sansCoups = LecturePdn.LireTexte("[Event \"X\"]\n[White \"A\"]\n");
        Verifie("PDN : balises sans coups : une partie vide", sansCoups.Count == 1 && sansCoups[0].Coups.Count == 0 && sansCoups[0].White == "A", $"{sansCoups.Count} partie(s)");

        List<PartiePdn> inconnu = LecturePdn.LireTexte("[Event \"X\"]\n1. 32-28 17-22 e.p. 2. 28-23 *");
        Verifie("PDN : un jeton inconnu est ignoré avec un avertissement, la partie reste lisible",
                inconnu.Count == 1 && Coups(inconnu[0]) == "32-28 17-22 28-23" && inconnu[0].Avertissements.Count == 1 && inconnu[0].Avertissements[0].Contains("e.p."),
                inconnu.Count == 1 ? $"{Coups(inconnu[0])} / {string.Join("; ", inconnu[0].Avertissements)}" : $"{inconnu.Count}");

        List<PartiePdn> orphelins = LecturePdn.LireTexte("[Event \"X\"]\n1. 32-28 ) } ] 17-22 *");
        Verifie("PDN : fermants sans ouvrant ignorés (sans boucle infinie)", orphelins.Count == 1 && Coups(orphelins[0]) == "32-28 17-22", orphelins.Count == 1 ? Coups(orphelins[0]) : "");
        Verifie("PDN : commentaire ou variante jamais fermés : pas d'exception",
                LecturePdn.LireTexte("[Event \"X\"]\n1. 32-28 { jamais fermé 17-22").Count == 1 && LecturePdn.LireTexte("[Event \"X\"]\n1. 32-28 (1. 31-26 17-22").Count == 1, "accolade et parenthèse non fermées");
        PartiePdn guillemet = LecturePdn.LireTexte("[Event \"A\n[White \"B\"]\n1. 32-28 *")[0];
        Verifie("PDN : guillemet fermant oublié : la valeur s'arrête à la fin de la ligne", guillemet.Event == "A" && guillemet.White == "B" && Coups(guillemet) == "32-28", $"{guillemet.Event} / {guillemet.White}");

        // ═══════════════ PDN : écriture ═══════════════
        Console.WriteLine("── PDN : écriture ──");

        PartiePdn aEcrire = new PartiePdn();
        aEcrire.Event = "Tournoi"; aEcrire.Site = "Paris"; aEcrire.Date = "2026.11.13"; aEcrire.Round = "1"; aEcrire.White = "Blanc"; aEcrire.Black = "Noir"; aEcrire.Result = "1-0";
        aEcrire.Coups.AddRange(["32-28", "17-22", "28x17", "12x21", "33-28"]);
        aEcrire.Resultat = "1-0";
        string texteEcrit = aEcrire.VersTexte(finDeLigne: "\n");
        string attenduEcrit = "[Event \"Tournoi\"]\n[Site \"Paris\"]\n[Date \"2026.11.13\"]\n[Round \"1\"]\n[White \"Blanc\"]\n[Black \"Noir\"]\n[Result \"1-0\"]\n\n1. 32-28 17-22 2. 28x17 12x21 3. 33-28 1-0\n";
        Verifie("PDN : écriture d'une partie (balises, ligne vide, coups numérotés, résultat)", texteEcrit == attenduEcrit, texteEcrit.Replace("\n", "⏎"));

        PartiePdn longue = new PartiePdn();
        longue.Event = "Valeurs \"spéciales\" \\ ici"; longue.White = "Blanc (3)"; longue.Black = "?"; longue.Result = "2-0";
        longue.Fen = "W:W31-50:B1-20";
        string[] ligneScanPdn = "32-28 17-22 28x17 12x21 33-28 7-12 39-33 19-23 28x19 14x23 44-39 9-14 50-44 4-9 34-29 23x34 39x30 21-26 44-39 11-17 31-27 6-11 37-31 26x37 41x32 20-24 30x19 13x24".Split(' ');
        longue.Coups.AddRange(ligneScanPdn);
        longue.Resultat = "2-0";
        PartiePdn relue = LecturePdn.LireTexte(longue.VersTexte(largeur: 40, finDeLigne: "\r\n"))[0];
        Verifie("PDN : écriture puis relecture à l'identique (balises avec guillemets et barre oblique, 28 coups, résultat)",
                relue.Balises.SequenceEqual(longue.Balises) && relue.Coups.SequenceEqual(longue.Coups) && relue.Resultat == "2-0",
                $"{relue.Event} / {relue.Coups.Count} coups / {relue.Resultat}");
        string[] lignesCoups = longue.VersTexte(largeur: 40, finDeLigne: "\n").Split('\n').SkipWhile(l => !l.StartsWith("1.")).Where(l => l.Length > 0).ToArray();
        Verifie("PDN : les lignes de coups ne dépassent pas la largeur demandée", lignesCoups.Length > 3 && lignesCoups.All(l => l.Length <= 40), $"{lignesCoups.Length} lignes, la plus longue : {lignesCoups.Max(l => l.Length)} caractères");

        PartiePdn pourNoirs = new PartiePdn { Result = "*" };
        pourNoirs.Fen = "B:W31-50:B1-20";
        pourNoirs.Coups.AddRange(["17-22", "31-26", "19-23"]);
        Verifie("PDN : écriture quand les noirs jouent en premier : « 1... » puis « 2. »", pourNoirs.VersTexte(finDeLigne: "\n").EndsWith("\n\n1... 17-22 2. 31-26 19-23 *\n"), pourNoirs.VersTexte(finDeLigne: "\n").Replace("\n", "⏎"));
        PartiePdn sansResultat = new PartiePdn();
        sansResultat.Coups.Add("32-28");
        PartiePdn resultatBalise = new PartiePdn { Result = "0-2" };
        Verifie("PDN : le résultat écrit est le marqueur lu, sinon la balise Result, sinon « * »",
                sansResultat.VersTexte(finDeLigne: "\n").EndsWith("1. 32-28 *\n") && resultatBalise.VersTexte(finDeLigne: "\n").EndsWith("\n\n0-2\n"), "* et 0-2");

        PartiePdn historique = PartiePdn.Creer("Entrainement", "Interface graphique", "2026-11-13", "?", "Bruno", "Scan", "1-0", null, ["32-28", "17-22", "28x17", " 1-0"]);
        string texteHistorique = historique.VersTexte(finDeLigne: "\n");
        Verifie("PDN : création depuis l'historique de l'application (marqueur de fin retiré des coups, PlyCount juste)",
                historique.Coups.Count == 3 && historique.Resultat == "1-0" && historique.Balise("PlyCount") == "3" && historique.Balise("Result") == "1-0"
                && texteHistorique.EndsWith("\n\n1. 32-28 17-22 2. 28x17 1-0\n"), texteHistorique.Replace("\n", "⏎"));
        PartiePdn enCours = PartiePdn.Creer("E", "S", "D", "R", "B", "N", "", "B:W31-50:B1-20", ["17-22", "31-26"]);
        Verifie("PDN : création d'une partie en cours : Result « * », balise FEN, « 1... » car les noirs commencent",
                enCours.Balise("Result") == "*" && enCours.Fen == "B:W31-50:B1-20" && enCours.VersTexte(finDeLigne: "\n").EndsWith("\n\n1... 17-22 2. 31-26 *\n"), enCours.VersTexte(finDeLigne: "\n").Replace("\n", "⏎"));
        PartiePdn aucunCoup = PartiePdn.Creer("E", "S", "D", "R", "B", "N", "*", null, []);
        Verifie("PDN : création sans aucun coup : pas de balise Result, PlyCount 0", aucunCoup.Balise("Result") == null && aucunCoup.Balise("PlyCount") == "0", aucunCoup.VersTexte(finDeLigne: "\n").Replace("\n", "⏎"));
        PartiePdn relueCreee = LecturePdn.LireTexte(historique.VersTexte())[0];
        Verifie("PDN : la partie créée se relit à l'identique et se rejoue", relueCreee.Balises.SequenceEqual(historique.Balises) && relueCreee.Coups.SequenceEqual(historique.Coups) && relueCreee.Rejouer().Erreur == null, relueCreee.Rejouer().Erreur ?? "ok");

        // ═══════════════ PDN : rejeu des coups par les règles ═══════════════
        Console.WriteLine("── PDN : rejeu ──");

        PartiePdn ligneRejouee = new PartiePdn();
        ligneRejouee.Coups.AddRange(ligneScanPdn);
        RejeuPdn rejeu = ligneRejouee.Rejouer();
        Verifie("Rejeu : la ligne de Scan en notation PDN abrégée (« 28x17 » sans la pièce prise) se rejoue entièrement",
                rejeu.Erreur == null && rejeu.Coups.Count == 28 && rejeu.Positions.Count == 29, rejeu.Erreur ?? $"{rejeu.Coups.Count} coups");

        PartiePdn illegale = new PartiePdn();
        illegale.Coups.AddRange(["32-28", "17-22", "28-23", "22-27"]);
        RejeuPdn rejeuIllegal = illegale.Rejouer();
        Verifie("Rejeu : un déplacement simple pendant une prise obligatoire arrête le rejeu au bon coup",
                rejeuIllegal.Erreur != null && rejeuIllegal.Erreur.Contains("Coup 3") && rejeuIllegal.Coups.Count == 2 && rejeuIllegal.Positions.Count == 3, rejeuIllegal.Erreur ?? "aucune erreur");

        PartiePdn aPartirDeFen = new PartiePdn { Fen = "W:W28:B22" };
        aPartirDeFen.Coups.Add("28x17");
        RejeuPdn rejeuFen = aPartirDeFen.Rejouer();
        Verifie("Rejeu : la partie part de la FEN de ses balises", rejeuFen.Erreur == null && rejeuFen.Positions[^1].VersFen() == "B:W17:B", rejeuFen.Erreur ?? rejeuFen.Positions[^1].VersFen());
        Verifie("Rejeu : une FEN invalide est signalée, sans exception",
                new PartiePdn { Fen = "W:W99:B1" }.Rejouer().Erreur?.StartsWith("Position de départ invalide") == true, "W:W99:B1");

        Verifie("Rejeu : notation complète d'une rafle « 28x19x8x17x28 » et abrégée « 28x28 » désignent le même coup",
                ReglesDames.TrouverCoupPdn(Position.DepuisFen("W:W28:B12,13,22,23"), "28x28") != null
                && ReglesDames.TrouverCoupPdn(Position.DepuisFen("W:W28:B12,13,22,23"), "28x19x8x17x28")?.Prises.Count == 4,
                "boucle autour de 4 pièces");
        // Dame blanche en 15 : deux rafles de 3 pièces partent de 15 et finissent en 31, par 15 > 24 > 13 > 31 ou par 15 > 33 > 22 > 31
        Position deuxRafles = Position.DepuisFen("W:WK15,34,39,43,45:B2,19,20,25,27,28,35");
        Verifie("Rejeu : cases intermédiaires partielles d'une rafle de dame (« 15x33x31 » pour 15 > 33 > 22 > 31) départagent deux rafles de même arrivée",
                ReglesDames.TrouverCoupPdn(deuxRafles, "15x33x31")?.Prises.OrderBy(n => n).SequenceEqual([20, 27, 28]) == true
                && ReglesDames.TrouverCoupPdn(deuxRafles, "15x24x31")?.Prises.OrderBy(n => n).SequenceEqual([19, 20, 27]) == true
                && ReglesDames.TrouverCoupPdn(deuxRafles, "15x31") == null,
                "15x33x31 -> 20,27,28 ; 15x24x31 -> 19,20,27 ; 15x31 ambigu, refusé");
        Verifie("Rejeu : « 28x17 » est refusé quand il n'y a pas de prise, « 32-28 » quand il y en a une",
                ReglesDames.TrouverCoupPdn(Position.DepuisFen("W:W28:B1"), "28x17") == null && ReglesDames.TrouverCoupPdn(Position.DepuisFen("W:W28:B22"), "28-23") == null, "28x17 sans prise, 28-23 avec prise");

        // Une rafle abrégée « 44x24 » qui désigne deux rafles de 3 pièces (39, 18, 19 ou 39, 12, 19) : refusée par défaut, la première est jouée avec premierSiAmbigu
        PartiePdn ambigue = new PartiePdn { Fen = "W:WK44:B12,18,19,39" };
        ambigue.Coups.Add("44x24");
        RejeuPdn rejeuStrict = ambigue.Rejouer();
        RejeuPdn rejeuTolerant = ambigue.Rejouer(premierSiAmbigu: true);
        Verifie("Rejeu : « 44x24 » ambigu est refusé par défaut, avec 0 coup rejoué", rejeuStrict.Erreur != null && rejeuStrict.Erreur.Contains("ambigu") && rejeuStrict.Coups.Count == 0, rejeuStrict.Erreur ?? "(pas d'erreur)");
        Verifie("Rejeu : avec premierSiAmbigu, le coup ambigu est joué (première rafle de 3 pièces) et signalé",
                rejeuTolerant.Erreur == null && rejeuTolerant.Coups.Count == 1 && rejeuTolerant.Coups[0].Depart == 44 && rejeuTolerant.Coups[0].Arrivee == 24 && rejeuTolerant.Coups[0].Prises.Count == 3
                && rejeuTolerant.Ambiguites?.Count == 1, rejeuTolerant.Erreur ?? string.Join(" | ", rejeuTolerant.Ambiguites ?? []));
        PartiePdn illegaleAuMilieu = new PartiePdn();
        illegaleAuMilieu.Coups.AddRange(["32-28", "17-22", "28x17", "12x21", "33-28", "28-23"]);      // Le 6e coup est illégal : c'est aux noirs de jouer et il n'y a aucun pion noir en 28
        RejeuPdn rejeuPartiel = illegaleAuMilieu.Rejouer(premierSiAmbigu: true);
        Verifie("Rejeu : un coup illégal arrête le rejeu au dernier coup légal, avec son numéro (la partie est chargée jusque-là)",
                rejeuPartiel.Erreur != null && rejeuPartiel.Coups.Count == 5 && rejeuPartiel.Positions.Count == 6 && rejeuPartiel.Erreur.StartsWith("Coup 6 "), rejeuPartiel.Erreur ?? "(pas d'erreur)");
        Verifie("Rejeu : un coup absurde (« 0-0 ») est une erreur de partie, pas une exception", new PartiePdn { Coups = { "0-0" } }.Rejouer(premierSiAmbigu: true).Erreur != null, "0-0");

        // ═══════════════ PDN : fichiers d'exemple du projet ═══════════════
        Console.WriteLine("── PDN : fichiers d'exemple ──");

        string dossierParties = Path.Combine(AppContext.BaseDirectory, "Parties");
        string[] fichiers = Directory.Exists(dossierParties) ? Directory.GetFiles(dossierParties, "*.pdn").OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray() : [];
        Verifie("Fichiers PDN d'exemple trouvés à côté du programme de test", fichiers.Length >= 20, $"{fichiers.Length} fichiers dans {dossierParties}");
        // Nombre de parties de chaque fichier, vérifié à la main dans le texte. Dans quatre fichiers il y a une partie de plus que de
        // balises « [Event » parce que le fichier lui-même est irrégulier (c'est le fichier qui est défectueux, pas la lecture) :
        // une partie dont la seule balise est [Empty] (Polish), une partie aux balises en français avec entités HTML (&#x00c9;v&#x00e9;nement,
        // BLITZ), une balise écrite sans son « [ » (Surinam) et une balise coupée par une fin de ligne (TOURNOI).
        Dictionary<string, int> partiesAttendues = new()
        {
            ["1er Open International Allinges 2007.pdn"] = 45, ["29eme Open d'Avignon 2014.pdn"] = 97, ["2ème Open national du Léman.pdn"] = 147,
            ["48th Polish Team Championship.pdn"] = 82, ["4th African BLITZ Championship OUAGA ZIIRI 2024.pdn"] = 48, ["Brigitte.pdn"] = 1,
            ["BrunoOuverture.pdn"] = 1, ["BrunoVsScan31.pdn"] = 11, ["Championnat de France 2021.pdn"] = 58, ["ChapionnatSurinam2025.pdn"] = 98,
            ["Match Samb-Buggy2.pdn"] = 6, ["Mini.pdn"] = 6, ["Moby_vs_Programs.pdn"] = 9, ["Montpellier Open Serie A.pdn"] = 56,
            ["Montpellier Open Serie B.pdn"] = 49, ["Partie_avec_FEN1.pdn"] = 4, ["Sijbrands_vs_Andreiko.pdn"] = 20, ["test.pdn"] = 2,
            ["TOURNOI DES CHAMPION REGIONAUX.pdn"] = 12, ["Van der Wal_vs_Wiersma.pdn"] = 20, ["World Title Match Women 2024.pdn"] = 2
        };
        // Parties qui ne se rejouent pas, par fichier (numéros à partir de 1). La partie 11 de BrunoVsScan31.pdn est un export de
        // l'application : balises d'une autre partie (Championnat de France 2021, dont les coups sont tout autres dès le coup 2), et
        // au coup 64 « 17-11 » joué quand aucun pion n'est sur la case 17 : la donnée est fausse, aucune règle ne la rend légale.
        Dictionary<string, int[]> partiesNonRejouables = new() { ["BrunoVsScan31.pdn"] = [11] };

        int totalParties = 0, totalRejouees = 0, totalCoups = 0;
        foreach (string fichier in fichiers)
        {
            string nomFichier = Path.GetFileName(fichier);
            List<PartiePdn> parties = LecturePdn.LireFichier(fichier);
            bool connu = partiesAttendues.TryGetValue(nomFichier, out int attendues);
            Verifie($"{nomFichier} : {(connu ? attendues.ToString() : "?")} parties lues", connu && parties.Count == attendues, $"{parties.Count} lues");

            List<int> echouees = [];
            List<string> echecs = [];
            for (int k = 0; k < parties.Count; k++)
            {
                RejeuPdn r = parties[k].Rejouer();
                if (r.Erreur == null) totalCoups += r.Coups.Count;
                else
                {
                    echouees.Add(k + 1);
                    echecs.Add($"partie {k + 1} ({parties[k].White} - {parties[k].Black}) : {r.Erreur}");
                }
            }
            int[] attenduesEchouees = partiesNonRejouables.GetValueOrDefault(nomFichier) ?? [];
            totalParties += parties.Count;
            totalRejouees += parties.Count - echouees.Count;
            Verifie($"{nomFichier} : toutes les parties se rejouent avec les règles{(attenduesEchouees.Length > 0 ? $" sauf la partie {string.Join(", ", attenduesEchouees)} (donnée fausse)" : "")}",
                    echouees.SequenceEqual(attenduesEchouees),
                    $"{parties.Count - echouees.Count}/{parties.Count}" + (echecs.Count > 0 ? " — " + string.Join(" | ", echecs.Take(3)) : ""));
        }
        Verifie("Ensemble des fichiers d'exemple : toutes les parties lues, toutes rejouées sauf une", totalParties == partiesAttendues.Values.Sum() && totalRejouees == totalParties - 1,
                $"{totalRejouees}/{totalParties} parties rejouées, {totalCoups} coups");

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

        // ═══════════════ Moteur Scan ═══════════════
        Console.WriteLine("── Moteur Scan ──");

        MoteurDamesScan.Quitte();       // Sans effet : le moteur n'est pas démarré
        Verifie("Moteur : arrêter un moteur non démarré est sans effet", !MoteurDamesScan.EstDemarre, "");
        ReponseMoteur? sansMoteur = MoteurDamesScan.ReflechirAsync(Position.Initiale().VersHub2(), 1).GetAwaiter().GetResult();
        Verifie("Moteur : une demande sans moteur démarré rend null tout de suite (pas d'attente sans fin)", sansMoteur == null, "");
        bool moteurIntrouvable = false;
        try { MoteurDamesScan.Demarre(Path.Combine(AppContext.BaseDirectory, "scan_inexistant.exe")); }
        catch (FileNotFoundException) { moteurIntrouvable = true; }
        Verifie("Moteur : un moteur introuvable lève FileNotFoundException", moteurIntrouvable && !MoteurDamesScan.EstDemarre, "");

        if (complet)
        {   // Avec le vrai Scan (dossier Dames_Scan31_FabienLetouzey du dépôt) : il répond par un coup que ReglesDames trouve légal
            string? scan = null;
            for (string? dossier = AppContext.BaseDirectory; dossier != null && scan == null; dossier = Path.GetDirectoryName(dossier))
            {
                string candidat = Path.Combine(dossier, "Dames_Scan31_FabienLetouzey", "scan.exe");
                if (File.Exists(candidat)) scan = candidat;
            }
            if (scan == null)
            {
                Verifie("Moteur : scan.exe trouvé dans le dépôt", false, "Dames_Scan31_FabienLetouzey\\scan.exe introuvable au-dessus de " + AppContext.BaseDirectory);
            }
            else
            {
                MoteurDamesScan.Demarre(scan);
                try
                {
                    Position positionDeDepart = Position.Initiale();
                    ReponseMoteur? premiere = MoteurDamesScan.ReflechirAsync(positionDeDepart.VersHub2(), 1).GetAwaiter().GetResult();
                    Coup? coupBlanc = premiere == null ? null : ReglesDames.TrouverCoup(positionDeDepart, premiere.Coup);
                    Verifie("Moteur : Scan répond à la position initiale par un coup légal", coupBlanc != null, premiere?.Coup ?? "(pas de réponse)");
                    Position apres = coupBlanc == null ? positionDeDepart : ReglesDames.Jouer(positionDeDepart, coupBlanc);
                    ReponseMoteur? seconde = MoteurDamesScan.ReflechirAsync(apres.VersHub2(), 1).GetAwaiter().GetResult();
                    Coup? coupNoir = seconde == null ? null : ReglesDames.TrouverCoup(apres, seconde.Coup);
                    Verifie("Moteur : une seconde demande (les noirs au trait) reçoit aussi un coup légal", coupNoir != null, seconde?.Coup ?? "(pas de réponse)");
                }
                finally
                {
                    MoteurDamesScan.Quitte();
                }
                Verifie("Moteur : après Quitte, le moteur n'est plus démarré", !MoteurDamesScan.EstDemarre, "");
            }
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
