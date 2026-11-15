// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Les vérifications de la vraie fenêtre : le formulaire est créé hors de l'écran, les tests cliquent sur les cases par le chemin de la souris (CaseMouseDown),
// appuient sur les boutons par leurs gestionnaires, remplacent l'horloge de la pendule et les boîtes de message (ReglagesDeTest) et regardent l'état du
// damier, des cadres, de l'historique et de la pendule. Une seule fenêtre est créée (le formulaire a des champs statiques, dont les 100 cases du damier),
// dans un thread à part (Windows Forms l'exige : STA). Avec « complet », le moteur Scan joue aussi.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrunoGUI_Dames;

public static class ScenarioInterface
{
    private const BindingFlags Tous = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private static BrunoInterfaceGraphiqueDames fenetre = null!;
    private static WindowsFormsSynchronizationContext? contexteFenetre;      // Voir InstalleContexte
    private static Action<string, bool, string> verifie = (_, _, _) => { };
    private static TimeSpan maintenant;                         // L'heure de la pendule, avancée à la main
    private static readonly List<string> messages = new();      // Les boîtes de message que la fenêtre a voulu afficher

    // --- Lance les vérifications dans un thread STA ; un scénario qui dépasse trois minutes ou lève une exception est signalé comme un échec ---
    public static void Executer(Action<string, bool, string> rapporte, bool complet)
    {
        verifie = rapporte;
        Exception? erreur = null;
        Thread fil = new Thread(() =>
        {
            try { Corps(complet); }
            catch (Exception ex) { erreur = ex; }
        });
        fil.SetApartmentState(ApartmentState.STA);
        fil.IsBackground = true;
        fil.Start();
        if (!fil.Join(TimeSpan.FromMinutes(3)))
            rapporte("Interface : le scénario se termine dans le délai (3 minutes)", false, "délai dépassé : la fenêtre est restée bloquée");
        else if (erreur != null)
            rapporte("Interface : le scénario se déroule sans exception", false, erreur.ToString());
    }

    private static void Corps(bool complet)
    {
        Console.WriteLine("── Interface (la vraie fenêtre) ──");
        string? scan = complet ? TrouveScan() : null;
        string dossier = Path.Combine(Path.GetTempPath(), "dames_interface_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dossier);
        string fichierPreferences = Path.Combine(dossier, "preferences.ini");
        ReglagesDeTest.Message = (texte, titre, boutons, icone) => { messages.Add(titre + " | " + texte); return DialogResult.OK; };
        ReglagesDeTest.HorlogePendule = () => maintenant;
        ReglagesDeTest.FichierPreferences = fichierPreferences;
        ReglagesDeTest.CheminMoteur = scan ?? Path.Combine(dossier, "scan_absent.exe");
        try
        {
            try { Application.SetDefaultFont(new Font("Microsoft Sans Serif", 8.25F)); }     // Comme Program.cs : la géométrie du Designer en dépend
            catch (InvalidOperationException) { }                                              // Déjà fait, ou une fenêtre existe déjà
            InstalleContexte();
            fenetre = new BrunoInterfaceGraphiqueDames();
            fenetre.ShowInTaskbar = false;
            fenetre.StartPosition = FormStartPosition.Manual;
            fenetre.Location = new Point(-32000, -32000);       // Hors de l'écran
            fenetre.Show();
            Pompe(300);
            BrunoInterfaceGraphiqueDames.DureeEtape = 0;    // Pas d'attente ni de glissement : les rafles et les coups du moteur sont immédiats
            Scenarios(complet && scan != null, fichierPreferences);
        }
        finally
        {
            try { fenetre?.Close(); } catch (Exception ex) { Console.WriteLine("Fermeture de la fenêtre de test : " + ex.Message); }
            try { Directory.Delete(dossier, true); } catch (IOException) { }
        }
    }

    private static void Scenarios(bool avecMoteur, string fichierPreferences)
    {
        Position pos() => GestionDamier.PositionCourante;
        SessionPartie session() => Partie.Courante;
        Pendule? pendule() => Champ<Pendule?>(fenetre, "pendule");
        string temps(string cadre) => Trouve(cadre).Text;

        // ═══════════════ Démarrage ═══════════════
        verifie("Interface : la fenêtre démarre avec 100 cases, la position initiale, le trait aux blancs et aucun message",
                BrunoInterfaceGraphiqueDames.CaseDamier.Count == 100 && pos().Compte(Piece.PionBlanc) == 20 && pos().Compte(Piece.PionNoir) == 20 && pos()[32] == Piece.PionBlanc && pos()[1] == Piece.PionNoir
                && BrunoInterfaceGraphiqueDames.CouleurAuTrait == Couleur.Blanc && session().Positions.Count == 1 && messages.Count == 0,
                $"{BrunoInterfaceGraphiqueDames.CaseDamier.Count} cases, {pos().Compte(Piece.PionBlanc)} pions blancs, messages : {string.Join(" / ", messages)}");
        verifie("Interface : sans pendule, les deux cadres affichent « --:-- » et la liste propose « Sans pendule »",
                temps("LabelPenduleBlanc") == "B --:--" && temps("LabelPenduleNoir") == "N --:--" && ((ComboBox)Trouve("ComboCadence")).SelectedIndex == 0 && pendule() == null,
                $"{temps("LabelPenduleBlanc")} / {temps("LabelPenduleNoir")}");

        // ═══════════════ Un coup au clic ═══════════════
        Nouvelle();
        Clique(32);
        verifie("Interface : un premier clic sélectionne la pièce (son image quitte la case, elle est « en main »)",
                !ImagePresente(32) && Champ<bool>(fenetre, "pieceEnMain") && pos()[32] == Piece.PionBlanc && !Statique<bool>("clickCaseSource"), "");
        verifie("Interface : la sélection montre les destinations possibles (cadres gris sur les cases vides 27 et 28, couleur du trajet) et la case de départ en couleur de départ",
                ImagePresente(27) && ImagePresente(28) && pos()[27] == Piece.Vide && pos()[28] == Piece.Vide && CouleurCase(27) == BrunoInterfaceGraphiqueDames.CouleurTrajetSuivi && CouleurCase(28) == BrunoInterfaceGraphiqueDames.CouleurTrajetSuivi
                && CouleurCase(32) == BrunoInterfaceGraphiqueDames.CouleurCaseDepart, $"27 : {CouleurCase(27)}, départ : {CouleurCase(32)}");
        Clique(28);
        verifie("Interface : après le coup, plus aucun cadre gris (chaque image est celle de la pièce de sa case) et les couleurs sont remises : arrivée, trajet, le reste en couleur de base",
                ImagesCoherentes() && CouleurCase(28) == BrunoInterfaceGraphiqueDames.CouleurCaseArrivee && CouleurCase(32) == BrunoInterfaceGraphiqueDames.CouleurTrajetSuivi && CouleurCase(27) == BrunoInterfaceGraphiqueDames.CouleurCaseSombre
                && Enumerable.Range(1, 50).Where(n => n != 28 && n != 32).All(n => CouleurCase(n) == BrunoInterfaceGraphiqueDames.CouleurCaseSombre), $"27 : {CouleurCase(27)}");
        verifie("Interface : le second clic joue 32-28 : position, images, historique et trait",
                pos()[28] == Piece.PionBlanc && pos()[32] == Piece.Vide && ImagePresente(28) && !ImagePresente(32) && session().CoupsPdn.SequenceEqual(["32-28"])
                && BrunoInterfaceGraphiqueDames.CouleurAuTrait == Couleur.Noir && Statique<bool>("clickCaseSource") && !Champ<bool>(fenetre, "pieceEnMain"),
                $"{string.Join(" ", session().CoupsPdn)}, trait {BrunoInterfaceGraphiqueDames.CouleurAuTrait}");
        verifie("Interface : l'historique garde la position après le coup, avec le trait aux noirs", session().DernierePosition[28] == Piece.PionBlanc && session().DernierePosition.Trait == Couleur.Noir, "");

        Clique(19);
        Clique(14);         // Un pion noir ne recule pas, et la case est occupée
        verifie("Interface : un coup illégal est refusé, la pièce revient sur sa case et l'historique ne change pas",
                pos()[19] == Piece.PionNoir && pos()[14] == Piece.PionNoir && ImagePresente(19) && session().CoupsPdn.Count == 1 && BrunoInterfaceGraphiqueDames.CouleurAuTrait == Couleur.Noir
                && Statique<bool>("clickCaseSource") && ImagesCoherentes(), "");
        Coup? coupNoir = JoueLegal();
        verifie("Interface : le coup légal suivant (noirs) s'ajoute et rend le trait aux blancs",
                coupNoir != null && session().CoupsPdn.Count == 2 && BrunoInterfaceGraphiqueDames.CouleurAuTrait == Couleur.Blanc && pos()[coupNoir.Arrivee] == Piece.PionNoir, coupNoir?.Hub2() ?? "(aucun)");

        // ═══════════════ Une rafle animée, fin de partie par prise de la dernière pièce ═══════════════
        Nouvelle();
        BrunoInterfaceGraphiqueDames.MiseenplaceFen("[FEN \"W:W28:B22\"]", true);
        Coup? rafle = ReglesDames.CoupsLegaux(session().DernierePosition).FirstOrDefault();
        if (rafle == null)
        {
            verifie("Interface : une rafle est possible dans la position d'essai", false, "aucun coup légal pour W:W28:B22");
        }
        else
        {
            Appelle(fenetre, "AnalyseCoupObligatoire", Couleur.Blanc);      // La prise obligatoire est signalée : cadre gris sur la case d'arrivée
            bool prisesSignalees = ImagePresente(rafle.Arrivee) && pos()[rafle.Arrivee] == Piece.Vide && CouleurCase(rafle.Depart) == BrunoInterfaceGraphiqueDames.CouleurCaseDepart;
            Clique(rafle.Depart);
            Clique(rafle.Arrivee);
            bool jouee = Attendre(() => session().CoupsPdn.Count == 1, 5000);
            verifie("Interface : la prise obligatoire est signalée avant le coup (départ coloré, cadre gris à l'arrivée) et, après la rafle, aucun cadre gris ne reste",
                    prisesSignalees && jouee && ImagesCoherentes(), $"signalée : {prisesSignalees}, images cohérentes : {ImagesCoherentes()}");
            verifie("Interface : la rafle (async, animation à 0) est jouée : pièce prise retirée, pièce arrivée, coup « " + rafle.PdnComplet() + " » dans l'historique",
                    jouee && pos()[rafle.Arrivee] == Piece.PionBlanc && pos()[22] == Piece.Vide && pos()[rafle.Depart] == Piece.Vide && !ImagePresente(22) && ImagePresente(rafle.Arrivee) && session().CoupsPdn[0].Contains('x'),
                    jouee ? string.Join(" ", session().CoupsPdn) : "pas de coup joué");
            verifie("Interface : les noirs n'ont plus de pièce : partie terminée, résultat 1-0, message annoncé, case « Gain blanc » cochée",
                    BrunoInterfaceGraphiqueDames.PartieTerminee && session().Resultat == "1-0" && ((RadioButton)Trouve("RadioGainBlanc")).Checked && messages.Any(m => m.Contains("Les Blancs gagnent")),
                    $"résultat {session().Resultat}, messages : {string.Join(" / ", messages)}");
        }
        // ═══════════════ Animations : glissement des pièces et durée des étapes ═══════════════
        Nouvelle();
        BrunoInterfaceGraphiqueDames.DureeEtape = 120;
        Coup? coupGlisse = ReglesDames.CoupsLegaux(session().DernierePosition).FirstOrDefault(c => c.Depart == 32 && c.Arrivee == 28);
        Stopwatch chrono = Stopwatch.StartNew();
        Task<string> glissement = AppelleRetour<Task<string>>(fenetre, "JoueCoupSurLeDamier", coupGlisse!, true);
        Pompe(60);        // Au milieu du glissement
        PictureBox? vole = Champ<PictureBox?>(fenetre, "glisseur");
        bool enVol = vole != null && vole.Visible && !ImagePresente(32) && !ImagePresente(28) && vole.Location != BrunoInterfaceGraphiqueDames.CaseDamier[GestionDamier.ObtenirIndicePictureBox(32)].Location
                     && vole.Location != BrunoInterfaceGraphiqueDames.CaseDamier[GestionDamier.ObtenirIndicePictureBox(28)].Location && vole.Region != null && !vole.Enabled;
        bool glisseFini = Attendre(() => glissement.IsCompleted, 3000);
        chrono.Stop();
        verifie("Interface : un coup glissé (moteur, rejeu) : pendant le glissement la pièce est dans un contrôle à part, entre les deux cases (de la forme de la pièce, sans clic) ; à la fin elle est sur sa case et le contrôle disparaît",
                coupGlisse != null && enVol && glisseFini && !vole!.Visible && pos()[28] == Piece.PionBlanc && pos()[32] == Piece.Vide && ImagesCoherentes() && chrono.ElapsedMilliseconds >= 100,
                $"en vol : {enVol}, fini : {glisseFini}, {chrono.ElapsedMilliseconds} ms, tâche {glissement.Status} {glissement.Exception?.GetBaseException().Message}, contexte {SynchronizationContext.Current?.GetType().Name}");

        Nouvelle();
        BrunoInterfaceGraphiqueDames.MiseenplaceFen("[FEN \"W:W28:B22\"]", true);
        Coup? petiteRafle = ReglesDames.CoupsLegaux(session().DernierePosition).FirstOrDefault();
        chrono.Restart();
        Task<string> tacheRafle = AppelleRetour<Task<string>>(fenetre, "JoueCoupSurLeDamier", petiteRafle!, false);
        bool rafleHumaineFinie = petiteRafle != null && Attendre(() => tacheRafle.IsCompleted, 3000);
        chrono.Stop();
        verifie("Interface : une rafle d'une pièce dure deux étapes (une par prise, plus une pause) : 2 × 120 ms au moins pour le joueur, et aucune pièce prise ne reste",
                rafleHumaineFinie && chrono.ElapsedMilliseconds >= 200 && chrono.ElapsedMilliseconds < 2000 && pos().Compte(Piece.PionNoir) == 0 && ImagesCoherentes(), $"{chrono.ElapsedMilliseconds} ms");

        BrunoInterfaceGraphiqueDames.DureeEtape = 0;
        Nouvelle();
        verifie("Interface : « Nouvelle partie » après une partie finie remet tout en route : position initiale, cases actives, plus de résultat",
                pos().Compte(Piece.PionBlanc) == 20 && !BrunoInterfaceGraphiqueDames.PartieTerminee && session().Resultat == null && session().Positions.Count == 1
                && CaseActive(32),
                $"case 32 active : {CaseActive(32)}");

        // ═══════════════ Pendule : choix, décompte, affichage ═══════════════
        Nouvelle(5);        // Blitz 5 min + 3 s
        verifie("Interface : choisir « Blitz » affiche 5:00 pour chaque camp, la pendule est armée et n'avance pas",
                temps("LabelPenduleBlanc") == "B 05:00" && temps("LabelPenduleNoir") == "N 05:00" && pendule()?.Etat == EtatPendule.Armee && pendule()?.Cadence.Id == "blitz", $"{temps("LabelPenduleBlanc")} / {temps("LabelPenduleNoir")}");
        Joue(32, 28);
        verifie("Interface : le premier coup démarre la pendule sans être décompté ; les noirs sont au trait (cadre en vert)",
                pendule()?.Etat == EtatPendule.EnMarche && pendule()?.Trait == Couleur.Noir && temps("LabelPenduleBlanc") == "B 05:00" && Trouve("LabelPenduleNoir").BackColor == Color.DarkGreen, "");
        maintenant += TimeSpan.FromSeconds(30);
        Tick();
        verifie("Interface : 30 s plus tard, le cadre des noirs affiche 4:30, celui des blancs ne bouge pas", temps("LabelPenduleNoir") == "N 04:30" && temps("LabelPenduleBlanc") == "B 05:00", $"{temps("LabelPenduleBlanc")} / {temps("LabelPenduleNoir")}");
        JoueLegal();
        verifie("Interface : le coup des noirs (30 s) est décompté, ils gagnent 3 s : 4:33 ; les blancs sont au trait", temps("LabelPenduleNoir") == "N 04:33" && pendule()?.Trait == Couleur.Blanc, temps("LabelPenduleNoir"));
        maintenant += TimeSpan.FromSeconds(285);
        Tick();
        verifie("Interface : à moins de 20 s, le temps du camp au trait passe en rouge", temps("LabelPenduleBlanc") == "B 00:15" && Trouve("LabelPenduleBlanc").ForeColor == Color.Red, temps("LabelPenduleBlanc"));

        // ═══════════════ Pendule : le drapeau tombe ═══════════════
        maintenant += TimeSpan.FromSeconds(30);
        Tick();
        verifie("Interface : le drapeau des blancs tombe : les noirs gagnent au temps (résultat 0-1, message, statut, case « Gain noir » cochée)",
                BrunoInterfaceGraphiqueDames.PartieTerminee && session().Resultat == "0-1" && ((RadioButton)Trouve("RadioGainNoir")).Checked && Trouve("LabelInformationJoueur").Text == "Les Noirs gagnent au temps"
                && messages.Any(m => m.Contains("Les Noirs gagnent") && m.Contains("des blancs")) && temps("LabelPenduleBlanc") == "B 00:00",
                $"résultat {session().Resultat}, statut « {Trouve("LabelInformationJoueur").Text} »");
        verifie("Interface : après la chute du drapeau, le damier est inactif et la pendule arrêtée", BrunoInterfaceGraphiqueDames.CaseDamier.All(c => !c.Enabled) && pendule()?.Etat == EtatPendule.Arretee, "");
        maintenant += TimeSpan.FromMinutes(10);
        Tick();
        verifie("Interface : le temps qui passe ensuite ne change plus rien (un seul message de fin)", messages.Count(m => m.StartsWith("Partie terminée")) == 1, $"{messages.Count} messages");

        // La pièce sélectionnée au moment de la chute du drapeau revient sur sa case
        Nouvelle(5);
        Joue(32, 28);
        Clique(17);
        bool enMain = Champ<bool>(fenetre, "pieceEnMain") && !ImagePresente(17);
        maintenant += TimeSpan.FromMinutes(6);
        Tick();
        verifie("Interface : si le drapeau tombe pendant qu'une pièce est en main, elle revient sur sa case (image, curseur, sélection)",
                enMain && BrunoInterfaceGraphiqueDames.PartieTerminee && ImagePresente(17) && !Champ<bool>(fenetre, "pieceEnMain") && Statique<bool>("clickCaseSource") && fenetre.Cursor == Cursors.Default,
                $"enMain avant : {enMain}, image : {ImagePresente(17)}");

        // Le curseur « pièce en main » ne fait pas fuir de handle GDI (150 sélections suivies d'un clic qui repose la pièce)
        Nouvelle();
        Clique(32); Clique(32);                 // Un premier tour, pour que les caches soient en place
        int objetsGdiAvant = ObjetsGdi();
        for (int i = 0; i < 150; i++)
        {
            Clique(32);                         // Sélection : un curseur est créé
            Clique(32);                         // Clic sur sa propre case : la pièce revient, le curseur est libéré
        }
        int objetsGdiApres = ObjetsGdi();
        verifie("Interface : 150 sélections de pièce ne laissent pas de handle GDI (le curseur de la pièce est libéré)", objetsGdiApres - objetsGdiAvant < 20 && fenetre.Cursor == Cursors.Default,
                $"{objetsGdiAvant} puis {objetsGdiApres} objets GDI");

        // Retour arrière : les temps sont gardés, la pendule attend le prochain coup
        Nouvelle(5);
        Joue(32, 28);
        maintenant += TimeSpan.FromSeconds(10);
        JoueLegal();            // Noirs : 5:00 - 10 s + 3 s
        maintenant += TimeSpan.FromSeconds(5);
        Appelle(fenetre, "BoutonRetourArriere_Click", null, EventArgs.Empty);
        Tick();
        verifie("Interface : « Retour arrière » retire le coup, garde les temps (noirs 4:53), arme la pendule (elle n'avance plus) et redonne le trait aux noirs",
                session().CoupsPdn.Count == 1 && pendule()?.Etat == EtatPendule.Armee && temps("LabelPenduleNoir") == "N 04:53" && pendule()?.Trait == Couleur.Noir && BrunoInterfaceGraphiqueDames.CouleurAuTrait == Couleur.Noir,
                $"{temps("LabelPenduleNoir")}, {pendule()?.Etat}, {session().CoupsPdn.Count} coup(s), trait de la pendule {pendule()?.Trait}, trait du jeu {BrunoInterfaceGraphiqueDames.CouleurAuTrait}");
        maintenant += TimeSpan.FromSeconds(20);
        Tick();
        verifie("Interface : armée, la pendule ne décompte pas avant le prochain coup", temps("LabelPenduleNoir") == "N 04:53", temps("LabelPenduleNoir"));
        JoueLegal();
        verifie("Interface : le prochain coup remet la pendule en marche, sans être décompté (premier coup d'une pendule armée)", pendule()?.Etat == EtatPendule.EnMarche && temps("LabelPenduleNoir") == "N 04:53", temps("LabelPenduleNoir"));

        // Changer de cadence ou de partie remet les temps à zéro
        Nouvelle(1);
        verifie("Interface : une autre cadence (1 h 20 + 30 s) remet les temps à zéro : 1:20:00 chacun", temps("LabelPenduleBlanc") == "B 1:20:00" && temps("LabelPenduleNoir") == "N 1:20:00" && pendule()?.Etat == EtatPendule.Armee, temps("LabelPenduleBlanc"));
        Nouvelle(0);
        verifie("Interface : « Sans pendule » rend les cadres vides (« --:-- ») et supprime la pendule", temps("LabelPenduleBlanc") == "B --:--" && temps("LabelPenduleNoir") == "N --:--" && pendule() == null, "");

        // ═══════════════ Parcours : on ne joue que sur la dernière position ═══════════════
        Nouvelle();
        Joue(32, 28);
        JoueLegal();
        Appelle(fenetre, "BoutonPrecedent_Click", null, EventArgs.Empty);
        messages.Clear();
        Clique(31);
        verifie("Interface : sur une position parcourue (Préc.), un clic sur une pièce est refusé avec un message, rien n'est sélectionné ni joué",
                messages.Count == 1 && messages[0].StartsWith("Partie parcourue") && session().CoupsPdn.Count == 2 && Statique<bool>("clickCaseSource") && ImagePresente(31) && !Champ<bool>(fenetre, "pieceEnMain"),
                $"messages : {string.Join(" / ", messages)} ; {session().CoupsPdn.Count} coup(s), indice {session().IndexAffiche}/{session().Positions.Count - 1}, clickCaseSource {Statique<bool>("clickCaseSource")}, partie terminée {BrunoInterfaceGraphiqueDames.PartieTerminee}");
        messages.Clear();
        Appelle(fenetre, "OrdinateurJoue_Click", null, EventArgs.Empty);
        verifie("Interface : « Ordinateur joue » est refusé de la même façon sur une position parcourue", messages.Count == 1 && messages[0].StartsWith("Partie parcourue") && session().CoupsPdn.Count == 2, string.Join(" / ", messages));
        Appelle(fenetre, "BoutonFin_Click", null, EventArgs.Empty);
        Coup? apresFin = JoueLegal();
        verifie("Interface : après « Fin », on rejoue normalement", apresFin != null && session().CoupsPdn.Count == 3 && session().EstSurLaDernierePosition, session().CoupsPdn.Count.ToString());

        // ═══════════════ Charger une partie PDN, puis la lire ═══════════════
        List<FichierPartiePdn.PartieDamesPdn> partiesExemple = FichierPartiePdn.LireParties(Path.Combine(AppContext.BaseDirectory, "Parties", "test.pdn"));
        Nouvelle();
        messages.Clear();
        fenetre.ChargerPartieDepuisPdn(partiesExemple[0]);      // 12 demi-coups (6 coups de chaque camp), dont plusieurs prises
        verifie("Interface : une partie PDN chargée entre tout de suite dans l'historique (12 demi-coups, 13 positions), le damier montre la position de départ, sans message ni attente",
                session().Positions.Count == 13 && session().CoupsPdn.Count == 12 && session().IndexAffiche == 0 && pos().Compte(Piece.PionBlanc) == 20 && pos().Compte(Piece.PionNoir) == 20 && ImagesCoherentes()
                && messages.Count == 0 && BrunoInterfaceGraphiqueDames.BalisesPartie.White == "Bruno" && Trouve("LabelCoupJoue").Text == "Position initiale" && BrunoInterfaceGraphiqueDames.CouleurAuTrait == Couleur.Blanc,
                $"{session().Positions.Count} positions, indice {session().IndexAffiche}, messages : {string.Join(" / ", messages)}");
        Clique(31);
        verifie("Interface : sur la position de départ d'une partie chargée, on ne joue pas (message « Partie parcourue »)", messages.Count == 1 && messages[0].StartsWith("Partie parcourue") && session().CoupsPdn.Count == 12, string.Join(" / ", messages));

        ((TrackBar)Trouve("TrackBarVitesseLecture")).Value = 5;
        messages.Clear();
        Appelle(fenetre, "BoutonLecture_Click", null, EventArgs.Empty);
        Pompe(30);
        bool enLecture = Champ<CancellationTokenSource?>(fenetre, "lecture") != null;
        bool boutonsBloques = !Trouve("BoutonPrecedent").Enabled && !Trouve("BoutonFin").Enabled && !Trouve("NouvellePartie").Enabled && Trouve("BoutonLecture").Enabled && Trouve("BoutonLecture").Text == "Pause";
        Clique(31);       // Un clic sur le damier est ignoré pendant la lecture
        bool clicIgnore = messages.Count == 0 && Statique<bool>("clickCaseSource");
        bool luJusquAuBout = Attendre(() => Champ<CancellationTokenSource?>(fenetre, "lecture") == null, 20000);
        Position visible = pos().Copie();
        visible.Trait = BrunoInterfaceGraphiqueDames.CouleurAuTrait;
        verifie("Interface : pendant la lecture, les boutons sont inactifs sauf Pause et la vitesse, et un clic sur le damier est ignoré sans message",
                enLecture && boutonsBloques && clicIgnore, $"en lecture : {enLecture}, boutons bloqués : {boutonsBloques}, clic ignoré : {clicIgnore}");
        verifie("Interface : la lecture déroule toute la partie : dernière position, damier identique à l'historique, boutons de nouveau actifs, « Lecture » sur le bouton, dernier coup affiché",
                luJusquAuBout && session().EstSurLaDernierePosition && visible.VersFen() == session().DernierePosition.VersFen() && ImagesCoherentes() && Trouve("BoutonPrecedent").Enabled && Trouve("NouvellePartie").Enabled
                && Trouve("BoutonLecture").Text == "Lecture" && Trouve("LabelCoupJoue").Text.StartsWith("Coup 12 :") && messages.Count == 0,
                $"fini : {luJusquAuBout}, indice {session().IndexAffiche}, {Trouve("LabelCoupJoue").Text}, messages : {string.Join(" / ", messages)}");

        Appelle(fenetre, "BoutonDebut_Click", null, EventArgs.Empty);
        Appelle(fenetre, "BoutonLecture_Click", null, EventArgs.Empty);
        Pompe(200);
        Appelle(fenetre, "BoutonLecture_Click", null, EventArgs.Empty);       // Pause
        bool arretee = Attendre(() => Champ<CancellationTokenSource?>(fenetre, "lecture") == null, 20000);
        int indexPause = session().IndexAffiche;
        Position apresPause = pos().Copie();
        apresPause.Trait = BrunoInterfaceGraphiqueDames.CouleurAuTrait;
        verifie("Interface : « Pause » arrête la lecture après le coup en cours : on est entre le début et la fin, le damier montre la position de l'historique à cet indice et le bouton redevient « Lecture »",
                arretee && indexPause > 0 && indexPause < 12 && apresPause.VersFen() == session().Positions[indexPause].VersFen() && ImagesCoherentes() && Trouve("BoutonLecture").Text == "Lecture" && Trouve("BoutonPrecedent").Enabled,
                $"arrêtée : {arretee}, indice {indexPause}");
        Appelle(fenetre, "BoutonLecture_Click", null, EventArgs.Empty);        // Reprise là où l'on s'était arrêté
        bool repris = Attendre(() => Champ<CancellationTokenSource?>(fenetre, "lecture") == null, 20000);
        verifie("Interface : « Lecture » après une pause reprend où l'on s'était arrêté et va jusqu'à la fin", repris && session().EstSurLaDernierePosition && ImagesCoherentes(), $"indice {session().IndexAffiche}");

        Nouvelle();
        Appelle(fenetre, "BoutonLecture_Click", null, EventArgs.Empty);
        verifie("Interface : lire une partie sans aucun coup est refusé avec un message", messages.Count == 1 && messages[0].StartsWith("Lecture") && Champ<CancellationTokenSource?>(fenetre, "lecture") == null, string.Join(" / ", messages));

        // ═══════════════ Préférences : la cadence est retenue ═══════════════
        Nouvelle(5);
        Appelle(fenetre, "EnregistrePreferences");
        verifie("Interface : la cadence choisie est écrite dans le fichier de préférences", Preferences.Lire(fichierPreferences).CadenceId == "blitz", File.Exists(fichierPreferences) ? "fichier écrit" : "pas de fichier");
        Nouvelle(0);
        Appelle(fenetre, "EnregistrePreferences");
        verifie("Interface : « Sans pendule » est écrit aussi (pas de cadence au prochain lancement)", Preferences.Lire(fichierPreferences).CadenceId == null, "");

        if (!avecMoteur)
            return;

        // ═══════════════ Avec le moteur Scan ═══════════════
        ((TrackBar)Trouve("trackBarTempsReflexion")).Value = 1;     // 1 seconde de réflexion
        Nouvelle(5, avecMoteur: true);
        Joue(32, 28, attendCoups: false);
        bool repondu = Attendre(() => session().CoupsPdn.Count == 2 && !Statique<bool>("jeuMoteurEnCours"), 30000);
        Position affichee = pos().Copie();
        affichee.Trait = BrunoInterfaceGraphiqueDames.CouleurAuTrait;
        verifie("Interface + Scan : après le coup de l'humain, le moteur répond : deux coups, trait aux blancs, la position du damier est celle de l'historique",
                repondu && BrunoInterfaceGraphiqueDames.CouleurAuTrait == Couleur.Blanc && affichee.VersFen() == session().DernierePosition.VersFen(), $"{string.Join(" ", session().CoupsPdn)}");
        verifie("Interface + Scan : la pendule a compté le coup du moteur (noirs : 1 coup, 5:00 + 3 s puisque l'horloge simulée n'a pas bougé) ; les blancs sont au trait",
                pendule()?.CoupsJoues(Couleur.Noir) == 1 && temps("LabelPenduleNoir") == "N 05:03" && pendule()?.Trait == Couleur.Blanc, $"{temps("LabelPenduleNoir")}, noirs : {pendule()?.CoupsJoues(Couleur.Noir)} coup(s)");

        // Le drapeau du moteur tombe pendant qu'il réfléchit : il perd, son coup n'est pas joué
        Nouvelle(5, avecMoteur: true);
        Joue(32, 28, attendCoups: false);
        bool reflechit = Statique<bool>("jeuMoteurEnCours");
        maintenant += TimeSpan.FromMinutes(10);
        Tick();
        bool fini = Attendre(() => !Statique<bool>("jeuMoteurEnCours"), 30000);
        Pompe(300);
        verifie("Interface + Scan : le drapeau du moteur tombe pendant sa réflexion : les blancs gagnent au temps, le moteur est arrêté et son coup n'est pas joué",
                reflechit && fini && BrunoInterfaceGraphiqueDames.PartieTerminee && session().Resultat == "1-0" && session().CoupsPdn.Count == 1 && messages.Any(m => m.Contains("Les Blancs gagnent") && m.Contains("des noirs")),
                $"réfléchissait : {reflechit}, fini : {fini}, coups : {string.Join(" ", session().CoupsPdn)}, résultat {session().Resultat}");
        verifie("Interface + Scan : après cette fin, le damier reste inactif (le retour du moteur ne le réactive pas)", BrunoInterfaceGraphiqueDames.CaseDamier.All(c => !c.Enabled), "");
    }

    // ───────────────────────────── Aides ─────────────────────────────

    // Nouvelle partie : position initiale, sans moteur par défaut, avec la cadence d'indice donné dans la liste (0 : sans pendule)
    private static void Nouvelle(int cadence = 0, bool avecMoteur = false)
    {
        messages.Clear();
        maintenant = TimeSpan.Zero;
        ((CheckBox)Trouve("OrdinateurBosse")).Checked = avecMoteur;
        ((ComboBox)Trouve("ComboCadence")).SelectedIndex = cadence;
        Appelle(fenetre, "NouvellePartie_Click", null, EventArgs.Empty);      // Remet aussi la pendule à zéro, même si la cadence n'a pas changé
        Pompe(30);
    }

    // Le nombre d'objets GDI (bitmaps, icônes, pinceaux…) du processus
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetGuiResources(IntPtr processus, uint type);
    private static int ObjetsGdi() => (int)GetGuiResources(System.Diagnostics.Process.GetCurrentProcess().Handle, 0);

    // Un clic de souris sur la case Manoury : le chemin du vrai clic (CaseMouseDown)
    private static void Clique(int caseManoury)
    {
        PictureBox caseClic = BrunoInterfaceGraphiqueDames.CaseDamier[GestionDamier.ObtenirIndicePictureBox(caseManoury)];
        Appelle(fenetre, "CaseMouseDown", caseClic, new MouseEventArgs(MouseButtons.Left, 1, 10, 10, 0));
    }

    // Deux clics (départ puis arrivée) ; attend que le coup soit dans l'historique (les rafles et le moteur sont asynchrones)
    private static void Joue(int depart, int arrivee, bool attendCoups = true)
    {
        int avant = Partie.Courante.CoupsPdn.Count;
        Clique(depart);
        bool selectionne = !Statique<bool>("clickCaseSource");
        Clique(arrivee);
        if (attendCoups && !Attendre(() => Partie.Courante.CoupsPdn.Count == avant + 1, 5000))
            Console.WriteLine($"      (coup {depart}-{arrivee} non joué : sélection faite {selectionne}, {Partie.Courante.CoupsPdn.Count} coup(s), trait {BrunoInterfaceGraphiqueDames.CouleurAuTrait}, partie terminée {BrunoInterfaceGraphiqueDames.PartieTerminee}, messages : {string.Join(" / ", messages)})");
    }

    // Joue le premier coup légal de la dernière position (deux clics) ; null s'il n'y en a pas
    private static Coup? JoueLegal()
    {
        Coup? coup = ReglesDames.CoupsLegaux(Partie.Courante.DernierePosition).FirstOrDefault();
        if (coup != null)
            Joue(coup.Depart, coup.Arrivee);
        return coup;
    }

    private static void Tick() => Appelle(fenetre, "MinuteriePendule_Tick", null, EventArgs.Empty);
    private static bool ImagePresente(int caseManoury) => BrunoInterfaceGraphiqueDames.CaseDamier[GestionDamier.ObtenirIndicePictureBox(caseManoury)].Image != null;
    private static Color CouleurCase(int caseManoury) => BrunoInterfaceGraphiqueDames.CaseDamier[GestionDamier.ObtenirIndicePictureBox(caseManoury)].BackColor;
    // Chaque case sombre porte l'image de sa pièce, ou aucune image si elle est vide : pas de cadre gris oublié, pas de pièce fantôme
    private static bool ImagesCoherentes()
    {
        for (int numero = 1; numero <= 50; numero++)
        {
            Piece piece = GestionDamier.PositionCourante[numero];
            Image? attendue = piece == Piece.Vide ? null : BrunoInterfaceGraphiqueDames.ListeBitmapsPiece[piece];
            if (!ReferenceEquals(BrunoInterfaceGraphiqueDames.CaseDamier[GestionDamier.ObtenirIndicePictureBox(numero)].Image, attendue))
                return false;
        }
        return true;
    }
    private static bool CaseActive(int caseManoury) => BrunoInterfaceGraphiqueDames.CaseDamier[GestionDamier.ObtenirIndicePictureBox(caseManoury)].Enabled;

    private static Control Trouve(string nom)
    {
        Control[] trouves = fenetre.Controls.Find(nom, true);
        if (trouves.Length != 1)
            throw new InvalidOperationException($"Contrôle « {nom} » : {trouves.Length} trouvé(s)");
        return trouves[0];
    }

    private static T Champ<T>(object objet, string nom)
    {
        FieldInfo champ = objet.GetType().GetField(nom, Tous) ?? throw new InvalidOperationException($"Champ « {nom} » introuvable");
        return (T)champ.GetValue(objet)!;
    }
    private static T Statique<T>(string nom)
    {
        FieldInfo champ = typeof(BrunoInterfaceGraphiqueDames).GetField(nom, Tous) ?? throw new InvalidOperationException($"Champ statique « {nom} » introuvable");
        return (T)champ.GetValue(null)!;
    }

    // Comme Application.Run : les suites des « await » doivent revenir sur le thread de la fenêtre, quand la boucle de messages tourne (Pompe, Attendre). Application.DoEvents remet à chaque
    // appel le contexte de base, d'où cette réinstallation avant chaque appel et chaque pompage : sans elle, les suites s'exécuteraient sur un autre thread (et toucheraient les
    // contrôles hors de leur thread, ce que Windows Forms ne signale que sous débogueur)
    private static void InstalleContexte()
    {
        contexteFenetre ??= new WindowsFormsSynchronizationContext();
        if (!ReferenceEquals(SynchronizationContext.Current, contexteFenetre))
            SynchronizationContext.SetSynchronizationContext(contexteFenetre);
    }
    private static T AppelleRetour<T>(object objet, string nom, params object?[] arguments)
    {
        InstalleContexte();
        MethodInfo methode = objet.GetType().GetMethod(nom, Tous) ?? throw new InvalidOperationException($"Méthode « {nom} » introuvable");
        try { return (T)methode.Invoke(objet, arguments)!; }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private static void Appelle(object objet, string nom, params object?[]? arguments)
    {
        InstalleContexte();
        MethodInfo methode = objet.GetType().GetMethod(nom, Tous) ?? throw new InvalidOperationException($"Méthode « {nom} » introuvable");
        try { methode.Invoke(objet, arguments ?? Array.Empty<object?>()); }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
        }
    }

    // Fait tourner la boucle de messages (les suites des méthodes async et la minuterie en dépendent)
    private static void Pompe(int millisecondes)
    {
        long fin = Environment.TickCount64 + millisecondes;
        while (Environment.TickCount64 < fin)
        {
            InstalleContexte();
            Application.DoEvents();
            Thread.Sleep(5);
        }
    }
    private static bool Attendre(Func<bool> condition, int millisecondes)
    {
        long fin = Environment.TickCount64 + millisecondes;
        while (!condition())
        {
            if (Environment.TickCount64 > fin)
                return false;
            InstalleContexte();
            Application.DoEvents();
            Thread.Sleep(5);
        }
        return true;
    }

    private static string? TrouveScan()
    {
        for (string? dossier = AppContext.BaseDirectory; dossier != null; dossier = Path.GetDirectoryName(dossier))
        {
            string candidat = Path.Combine(dossier, "Dames_Scan31_FabienLetouzey", "scan.exe");
            if (File.Exists(candidat))
                return candidat;
        }
        return null;
    }
}
