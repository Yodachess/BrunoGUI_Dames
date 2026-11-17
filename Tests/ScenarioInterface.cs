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
// damier, des cadres, de l'historique et de la pendule. Une fenêtre est créée pour l'ensemble, et une seconde pour le test des deux fenêtres (ReglagesDeTest reste global),
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
            fenetre.DureeEtape = 0;    // Pas d'attente ni de glissement : les rafles et les coups du moteur sont immédiats
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
        Position pos() => fenetre.Damier.PositionCourante;
        SessionPartie session() => fenetre.Session;
        Pendule? pendule() => Champ<Pendule?>(fenetre, "pendule");
        string temps(string cadre) => Trouve(cadre).Text;

        // ═══════════════ Démarrage ═══════════════
        verifie("Interface : la fenêtre démarre avec 100 cases, la position initiale, le trait aux blancs et aucun message",
                fenetre.CaseDamier.Count == 100 && pos().Compte(Piece.PionBlanc) == 20 && pos().Compte(Piece.PionNoir) == 20 && pos()[32] == Piece.PionBlanc && pos()[1] == Piece.PionNoir
                && fenetre.CouleurAuTrait == Couleur.Blanc && session().Positions.Count == 1 && messages.Count == 0,
                $"{fenetre.CaseDamier.Count} cases, {pos().Compte(Piece.PionBlanc)} pions blancs, messages : {string.Join(" / ", messages)}");
        verifie("Interface : sans pendule, les deux cadres affichent « --:-- » et la liste propose « Sans pendule »",
                temps("LabelPenduleBlanc") == "B --:--" && temps("LabelPenduleNoir") == "N --:--" && ((ComboBox)Trouve("ComboCadence")).SelectedIndex == 0 && pendule() == null,
                $"{temps("LabelPenduleBlanc")} / {temps("LabelPenduleNoir")}");

        // ═══════════════ Un coup au clic ═══════════════
        Nouvelle();
        Clique(32);
        verifie("Interface : un premier clic sélectionne la pièce (son image quitte la case, elle est « en main »)",
                !ImagePresente(32) && Champ<bool>(fenetre, "pieceEnMain") && pos()[32] == Piece.PionBlanc && !Champ<bool>(fenetre, "clickCaseSource"), "");
        verifie("Interface : la sélection montre les destinations possibles (cadres gris sur les cases vides 27 et 28, couleur du trajet) et la case de départ en couleur de départ",
                ImagePresente(27) && ImagePresente(28) && pos()[27] == Piece.Vide && pos()[28] == Piece.Vide && CouleurCase(27) == fenetre.CouleurTrajetSuivi && CouleurCase(28) == fenetre.CouleurTrajetSuivi
                && CouleurCase(32) == fenetre.CouleurCaseDepart, $"27 : {CouleurCase(27)}, départ : {CouleurCase(32)}");
        Clique(28);
        verifie("Interface : après le coup, plus aucun cadre gris (chaque image est celle de la pièce de sa case) et les couleurs sont remises : arrivée, trajet, le reste en couleur de base",
                ImagesCoherentes() && CouleurCase(28) == fenetre.CouleurCaseArrivee && CouleurCase(32) == fenetre.CouleurTrajetSuivi && CouleurCase(27) == fenetre.CouleurCaseSombre
                && Enumerable.Range(1, 50).Where(n => n != 28 && n != 32).All(n => CouleurCase(n) == fenetre.CouleurCaseSombre), $"27 : {CouleurCase(27)}");
        verifie("Interface : le second clic joue 32-28 : position, images, historique et trait",
                pos()[28] == Piece.PionBlanc && pos()[32] == Piece.Vide && ImagePresente(28) && !ImagePresente(32) && session().CoupsPdn.SequenceEqual(["32-28"])
                && fenetre.CouleurAuTrait == Couleur.Noir && Champ<bool>(fenetre, "clickCaseSource") && !Champ<bool>(fenetre, "pieceEnMain"),
                $"{string.Join(" ", session().CoupsPdn)}, trait {fenetre.CouleurAuTrait}");
        verifie("Interface : l'historique garde la position après le coup, avec le trait aux noirs", session().DernierePosition[28] == Piece.PionBlanc && session().DernierePosition.Trait == Couleur.Noir, "");

        Clique(19);
        Clique(14);         // Un pion noir ne recule pas, et la case est occupée
        verifie("Interface : un coup illégal est refusé, la pièce revient sur sa case et l'historique ne change pas",
                pos()[19] == Piece.PionNoir && pos()[14] == Piece.PionNoir && ImagePresente(19) && session().CoupsPdn.Count == 1 && fenetre.CouleurAuTrait == Couleur.Noir
                && Champ<bool>(fenetre, "clickCaseSource") && ImagesCoherentes(), "");
        Coup? coupNoir = JoueLegal();
        verifie("Interface : le coup légal suivant (noirs) s'ajoute et rend le trait aux blancs",
                coupNoir != null && session().CoupsPdn.Count == 2 && fenetre.CouleurAuTrait == Couleur.Blanc && pos()[coupNoir.Arrivee] == Piece.PionNoir, coupNoir?.Hub2() ?? "(aucun)");

        // ═══════════════ Une rafle animée, fin de partie par prise de la dernière pièce ═══════════════
        Nouvelle();
        fenetre.MiseenplaceFen("[FEN \"W:W28:B22\"]", true);
        Coup? rafle = ReglesDames.CoupsLegaux(session().DernierePosition).FirstOrDefault();
        if (rafle == null)
        {
            verifie("Interface : une rafle est possible dans la position d'essai", false, "aucun coup légal pour W:W28:B22");
        }
        else
        {
            fenetre.ArriveesDebutTour = false;
            Appelle(fenetre, "AnalyseCoupObligatoire", Couleur.Blanc);      // Une seule prise possible : le coup est forcé, son départ ET son arrivée sont signalés (cadre gris sur la case d'arrivée)
            bool prisesSignalees = ImagePresente(rafle.Arrivee) && pos()[rafle.Arrivee] == Piece.Vide && CouleurCase(rafle.Depart) == fenetre.CouleurCaseDepart;            Clique(rafle.Depart);
            Clique(rafle.Arrivee);
            bool jouee = Attendre(() => session().CoupsPdn.Count == 1, 5000);
            verifie("Interface : la prise obligatoire est signalée avant le coup (coup forcé : départ coloré et cadre gris à l'arrivée) et, après la rafle, aucun cadre gris ne reste",
                    prisesSignalees && jouee && ImagesCoherentes(), $"signalée : {prisesSignalees}, images cohérentes : {ImagesCoherentes()}");
            verifie("Interface : la rafle (async, animation à 0) est jouée : pièce prise retirée, pièce arrivée, coup « " + rafle.PdnComplet() + " » dans l'historique",
                    jouee && pos()[rafle.Arrivee] == Piece.PionBlanc && pos()[22] == Piece.Vide && pos()[rafle.Depart] == Piece.Vide && !ImagePresente(22) && ImagePresente(rafle.Arrivee) && session().CoupsPdn[0].Contains('x'),
                    jouee ? string.Join(" ", session().CoupsPdn) : "pas de coup joué");
            verifie("Interface : les noirs n'ont plus de pièce : partie terminée, résultat 1-0, message annoncé, case « Gain blanc » cochée",
                    fenetre.PartieTerminee && session().Resultat == "1-0" && ((RadioButton)Trouve("RadioGainBlanc")).Checked && messages.Any(m => m.Contains("Les Blancs gagnent")),
                    $"résultat {session().Resultat}, messages : {string.Join(" / ", messages)}");
        }
        // ═══════════════ Animations : glissement des pièces et durée des étapes ═══════════════
        Nouvelle();
        fenetre.DureeEtape = 120;
        Coup? coupGlisse = ReglesDames.CoupsLegaux(session().DernierePosition).FirstOrDefault(c => c.Depart == 32 && c.Arrivee == 28);
        Stopwatch chrono = Stopwatch.StartNew();
        Task<string> glissement = AppelleRetour<Task<string>>(fenetre, "JoueCoupSurLeDamier", coupGlisse!, true);
        Pompe(60);        // Au milieu du glissement
        PictureBox? vole = Champ<PictureBox?>(fenetre, "glisseur");
        bool enVol = vole != null && vole.Visible && !ImagePresente(32) && !ImagePresente(28) && vole.Location != fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(32)].Location
                     && vole.Location != fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(28)].Location && vole.Region != null && !vole.Enabled;
        bool glisseFini = Attendre(() => glissement.IsCompleted, 3000);
        chrono.Stop();
        verifie("Interface : un coup glissé (moteur, rejeu) : pendant le glissement la pièce est dans un contrôle à part, entre les deux cases (de la forme de la pièce, sans clic) ; à la fin elle est sur sa case et le contrôle disparaît",
                coupGlisse != null && enVol && glisseFini && !vole!.Visible && pos()[28] == Piece.PionBlanc && pos()[32] == Piece.Vide && ImagesCoherentes() && chrono.ElapsedMilliseconds >= 100,
                $"en vol : {enVol}, fini : {glisseFini}, {chrono.ElapsedMilliseconds} ms, tâche {glissement.Status} {glissement.Exception?.GetBaseException().Message}, contexte {SynchronizationContext.Current?.GetType().Name}");

        Nouvelle();
        fenetre.MiseenplaceFen("[FEN \"W:W28:B22\"]", true);
        Coup? petiteRafle = ReglesDames.CoupsLegaux(session().DernierePosition).FirstOrDefault();
        chrono.Restart();
        Task<string> tacheRafle = AppelleRetour<Task<string>>(fenetre, "JoueCoupSurLeDamier", petiteRafle!, false);
        bool rafleHumaineFinie = petiteRafle != null && Attendre(() => tacheRafle.IsCompleted, 3000);
        chrono.Stop();
        verifie("Interface : une rafle d'une pièce dure deux étapes (une par prise, plus une pause) : 2 × 120 ms au moins pour le joueur, et aucune pièce prise ne reste",
                rafleHumaineFinie && chrono.ElapsedMilliseconds >= 200 && chrono.ElapsedMilliseconds < 2000 && pos().Compte(Piece.PionNoir) == 0 && ImagesCoherentes(), $"{chrono.ElapsedMilliseconds} ms");

        fenetre.DureeEtape = 0;
        Nouvelle();
        verifie("Interface : « Nouvelle partie » après une partie finie remet tout en route : position initiale, cases actives, plus de résultat",
                pos().Compte(Piece.PionBlanc) == 20 && !fenetre.PartieTerminee && session().Resultat == null && session().Positions.Count == 1
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
                fenetre.PartieTerminee && session().Resultat == "0-1" && ((RadioButton)Trouve("RadioGainNoir")).Checked && Trouve("LabelInformationJoueur").Text == "Les Noirs gagnent au temps"
                && messages.Any(m => m.Contains("Les Noirs gagnent") && m.Contains("des blancs")) && temps("LabelPenduleBlanc") == "B 00:00",
                $"résultat {session().Resultat}, statut « {Trouve("LabelInformationJoueur").Text} »");
        verifie("Interface : après la chute du drapeau, le damier est inactif et la pendule arrêtée", fenetre.CaseDamier.All(c => !c.Enabled) && pendule()?.Etat == EtatPendule.Arretee, "");
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
                enMain && fenetre.PartieTerminee && ImagePresente(17) && !Champ<bool>(fenetre, "pieceEnMain") && Champ<bool>(fenetre, "clickCaseSource") && fenetre.Cursor == Cursors.Default,
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

        // Les couleurs de l'interface (thème « noyer et crème ») : chaque texte est lisible sur son fond (contraste WCAG d'au moins 4,5)
        double Lum(Color c)
        {
            double Lin(byte v) { double s = v / 255.0; return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4); }
            return 0.2126 * Lin(c.R) + 0.7152 * Lin(c.G) + 0.0722 * Lin(c.B);
        }
        double Contraste(Color a, Color b) { double x = Lum(a), y = Lum(b); return (Math.Max(x, y) + 0.05) / (Math.Min(x, y) + 0.05); }
        string[] nomsThemes = { "LabelCoupJoue", "LabelInformationJoueur", "groupBox1", "groupParcoursPartie", "LabelAfficheScan", "LabelScore", "LabelPrisesPossibles", "LabelPionsBlancs", "LabelPionsNoirs",
                                "LabelDamesBlanches", "LabelDamesNoires", "OrdinateurBosse", "SonEmis", "GroupResultat", "BoutonPrecedent", "BoutonSuivant", "BoutonDebut", "BoutonFin", "BoutonLecture" };
        string illisibles = string.Join(", ", nomsThemes.Where(n => Contraste(Trouve(n).ForeColor, Trouve(n).BackColor) < 4.5).Select(n => n));
        Color Krypton(string nom, string etat, string propriete)
        {   // fenetre.<nom>.<etat>.<Back|Content.ShortText>.<Color1> par réflexion (le projet de tests ne référence pas Krypton)
            object? bouton = Trouve(nom), triple = bouton.GetType().GetProperty(etat)!.GetValue(bouton);
            object? partie = triple!.GetType().GetProperty(propriete == "Back" ? "Back" : "Content")!.GetValue(triple);
            if (propriete != "Back") partie = partie!.GetType().GetProperty("ShortText")!.GetValue(partie);
            return (Color)partie!.GetType().GetProperty("Color1")!.GetValue(partie)!;
        }
        string boutonsIllisibles = string.Join(", ", new[] { "NouvellePartie", "BoutonQuitterApplication", "BoutonAide", "BoutonMasqueAffiche" }
            .Where(n => new[] { "StateCommon", "StateNormal", "StateTracking", "StatePressed", "OverrideDefault" }.Any(e => Contraste(Krypton(n, e, "Texte"), Krypton(n, e, "Back")) < 4.5)));
        verifie("Interface : le thème est appliqué (fond du thème) et chaque texte est lisible sur son fond (contraste d'au moins 4,5), boutons Krypton compris dans tous leurs états",
                fenetre.BackColor == ThemeInterface.Fond && illisibles.Length == 0 && boutonsIllisibles.Length == 0, $"illisibles : {illisibles} / boutons : {boutonsIllisibles}");

        // Les fenêtres secondaires portent le même thème : fond du thème, et chaque texte lisible sur son fond
        List<string> fenetresIllisibles = new List<string>();
        void VerifieFenetre(string nom, Form f)
        {
            if (f.BackColor != ThemeInterface.Fond) fenetresIllisibles.Add($"{nom} : fond {f.BackColor.Name}");
            void Parcours(Control.ControlCollection cc)
            {
                foreach (Control c in cc)
                {
                    if (c is Button b && Contraste(b.ForeColor, b.BackColor) < 4.5) fenetresIllisibles.Add($"{nom} : bouton {b.Name}");
                    if (c is RichTextBox r && Contraste(r.ForeColor, r.BackColor) < 4.5) fenetresIllisibles.Add($"{nom} : texte {r.Name}");
                    if (c is Label l && l.BackColor != Color.Transparent && l.BackColor != SystemColors.Control && Contraste(l.ForeColor, l.BackColor) < 4.5) fenetresIllisibles.Add($"{nom} : étiquette {l.Name}");
                    if (c is DataGridView g && (Contraste(g.DefaultCellStyle.ForeColor, g.DefaultCellStyle.BackColor) < 4.5 || Contraste(g.ColumnHeadersDefaultCellStyle.ForeColor, g.ColumnHeadersDefaultCellStyle.BackColor) < 4.5
                                                  || Contraste(g.DefaultCellStyle.SelectionForeColor, g.DefaultCellStyle.SelectionBackColor) < 4.5)) fenetresIllisibles.Add($"{nom} : grille {g.Name}");
                    if (c.HasChildren) Parcours(c.Controls);
                }
            }
            Parcours(f.Controls);
        }
        VerifieFenetre("Liste des coups", Champ<Form>(fenetre, "affichePdn"));
        VerifieFenetre("Liste des parties", Champ<Form>(fenetre, "fichierPartiePdn"));
        VerifieFenetre("Protocole", Champ<Form>(fenetre, "donneesBrutesDames"));
        using (ChoixRafleForm choix = new ChoixRafleForm(new List<Coup> { new Coup(1, 2, Array.Empty<int>(), Array.Empty<int>()) }, _ => { }))
            VerifieFenetre("Choix de la rafle", choix);
        verifie("Interface : les fenêtres secondaires (liste des coups, liste des parties, protocole, choix de la rafle) ont le thème et des textes lisibles", fenetresIllisibles.Count == 0, string.Join(" / ", fenetresIllisibles));

        // La fenêtre du protocole sépare le nom de l'émetteur des données
        Appelle(fenetre, "AfficheDonneesBrutes", "pong", true);
        Appelle(fenetre, "AfficheDonneesBrutes", "ping", false);
        string protocole = Champ<Form>(fenetre, "donneesBrutesDames") is DonneesBrutesDames fenetreProtocole ? fenetreProtocole.DonneesBrutesVue.Text : "";
        verifie("Interface : dans la fenêtre du protocole, un espace sépare « [SCAN 3.1] » (comme « [BrunoGUI_Dames] ») des données", protocole.Contains("[SCAN 3.1]    pong") && protocole.Contains("[BrunoGUI_Dames]    ping"), protocole);

        // ═══════════════ Repères du damier : dernier coup, croix sur les pièces à prendre, numéros des étapes ═══════════════
        fenetre.DureeEtape = 0;
        Nouvelle();
        Joue(32, 28);
        bool traceApresCoup = Champ<int>(fenetre, "dernierDepartManoury") == 32 && Champ<int>(fenetre, "dernierArriveeManoury") == 28;
        JoueLegal();                            // Les noirs répondent : le dernier coup est le leur
        Coup? reponseNoire = null;
        if (session().CoupsPdn.Count == 2) reponseNoire = ReglesDames.TrouverCoupPdn(session().Positions[1], session().CoupsPdn[1]);
        bool traceReponse = reponseNoire != null && Champ<int>(fenetre, "dernierDepartManoury") == reponseNoire.Depart && Champ<int>(fenetre, "dernierArriveeManoury") == reponseNoire.Arrivee;
        Appelle(fenetre, "BoutonPrecedent_Click", null, EventArgs.Empty);
        bool traceParcours = Champ<int>(fenetre, "dernierDepartManoury") == 32 && Champ<int>(fenetre, "dernierArriveeManoury") == 28;       // Le parcours montre le coup de la position affichée
        Appelle(fenetre, "BoutonDebut_Click", null, EventArgs.Empty);
        bool traceDebut = Champ<int>(fenetre, "dernierDepartManoury") == 0 && Champ<int>(fenetre, "dernierArriveeManoury") == 0;
        verifie("Interface : le dernier coup est repéré par un cadre sur ses deux cases (celui du coup joué, de la réponse, du coup affiché par le parcours ; aucun au début)",
                traceApresCoup && traceReponse && traceParcours && traceDebut, $"après le coup {traceApresCoup}, réponse {traceReponse}, parcours {traceParcours}, début {traceDebut}");

        // Le style du repère du dernier coup (préférence RepereDernierCoup) : ce qui est dessiné dans la case de départ du dernier coup (32)
        Nouvelle();
        Joue(32, 28);
        PictureBox case32 = fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(32)];
        bool BrunA(Bitmap b, int x, int y) { Color c = b.GetPixel(x, y); return Math.Abs(c.R - 0x4A) < 12 && Math.Abs(c.G - 0x2A) < 12 && Math.Abs(c.B - 0x12) < 12; }
        (bool coin, bool milieuGauche) Reperes(string style)
        {
            fenetre.RepereDernierCoup = style;
            case32.Invalidate();
            using Bitmap dessin = new Bitmap(case32.Width, case32.Height);
            case32.DrawToBitmap(dessin, new Rectangle(0, 0, case32.Width, case32.Height));
            return (BrunA(dessin, 6, case32.Height - 7), BrunA(dessin, 3, case32.Height / 2));      // Près du coin en bas à gauche ; au milieu du bord gauche
        }
        (bool coin, bool milieuGauche) styleCoin = Reperes("coin"), styleCadre = Reperes("cadre"), styleAucun = Reperes("aucun");
        fenetre.RepereDernierCoup = "coin";
        verifie("Interface : le repère du dernier coup est un triangle dans un coin (par défaut), un cadre, ou rien selon la préférence RepereDernierCoup",
                styleCoin == (true, false) && styleCadre == (false, true) && styleAucun == (false, false), $"coin {styleCoin}, cadre {styleCadre}, aucun {styleAucun}");
        Nouvelle();
        fenetre.MiseenplaceFen("[FEN \"W:W32:B28,19\"]", true);       // Une rafle en deux étapes
        Appelle(fenetre, "AnalyseCoupObligatoire", Couleur.Blanc);
        Coup? deuxEtapes = ReglesDames.CoupsLegaux(session().DernierePosition).FirstOrDefault();
        bool forceMontre = deuxEtapes != null && ImagePresente(deuxEtapes.Arrivee) && Champ<Dictionary<int, string>>(fenetre, "numerosEtape").Count == 2;
        verifie("Interface : une rafle forcée est montrée dès le début du tour avec sa destination et l'ordre de ses étapes", forceMontre, $"numéros {Champ<Dictionary<int, string>>(fenetre, "numerosEtape").Count}");
        Clique(32);
        bool couleursEtapes = deuxEtapes != null && CouleurCase(deuxEtapes.Prises[0]) == fenetre.CouleurCasePrise && CouleurCase(deuxEtapes.Chemin[0]) == fenetre.CouleurCaseEtape
                              && CouleurCase(deuxEtapes.Arrivee) == fenetre.CouleurCaseArrivee;
        verifie("Interface : à la sélection d'une prise forcée, la pièce à prendre, la case d'étape et l'arrivée ont chacune leur couleur (malgré le cadre gris posé au début du tour)", couleursEtapes,
                deuxEtapes == null ? "pas de rafle" : $"prise {CouleurCase(deuxEtapes.Prises[0])}, étape {CouleurCase(deuxEtapes.Chemin[0])}, arrivée {CouleurCase(deuxEtapes.Arrivee)}");        Dictionary<int, string> etapes = Champ<Dictionary<int, string>>(fenetre, "numerosEtape");
        verifie("Interface : à la sélection d'une rafle unique, ses cases d'arrivée sont numérotées 1, 2…",
                deuxEtapes != null && deuxEtapes.Chemin.Count == 2 && etapes.Count == 2 && etapes[deuxEtapes.Chemin[0]] == "1" && etapes[deuxEtapes.Chemin[1]] == "2",
                $"rafle {deuxEtapes?.PdnComplet()} ({deuxEtapes?.Chemin.Count} étape(s)), étapes {string.Join(",", etapes)}");
        if (deuxEtapes != null)
            Clique(deuxEtapes.Arrivee);
        bool rafleJouee = Attendre(() => session().CoupsPdn.Count == 1, 5000);
        verifie("Interface : après la rafle, les numéros disparaissent et le dernier coup est celui de la rafle",
                rafleJouee && deuxEtapes != null && Champ<Dictionary<int, string>>(fenetre, "numerosEtape").Count == 0
                && Champ<int>(fenetre, "dernierDepartManoury") == deuxEtapes.Depart && Champ<int>(fenetre, "dernierArriveeManoury") == deuxEtapes.Arrivee,
                $"numéros {Champ<Dictionary<int, string>>(fenetre, "numerosEtape").Count}, dernier coup {Champ<int>(fenetre, "dernierDepartManoury")}-{Champ<int>(fenetre, "dernierArriveeManoury")}");
        Nouvelle();
        fenetre.MiseenplaceFen("[FEN \"W:WK44:B12,18,19,39\"]", true);     // Deux rafles possibles pour la même pièce
        Appelle(fenetre, "AnalyseCoupObligatoire", Couleur.Blanc);
        List<Coup> prisesPossibles = ReglesDames.CoupsLegaux(session().DernierePosition).ToList();
        bool departSeul = prisesPossibles.Count > 1 && CouleurCase(44) == fenetre.CouleurCaseDepart && prisesPossibles.All(c => !ImagePresente(c.Arrivee) && fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(c.Arrivee)].Enabled);
        fenetre.ArriveesDebutTour = true;
        Appelle(fenetre, "AnalyseCoupObligatoire", Couleur.Blanc);
        bool arriveesAvecPreference = prisesPossibles.All(c => ImagePresente(c.Arrivee));
        fenetre.ArriveesDebutTour = false;
        Appelle(fenetre, "AnalyseCoupObligatoire", Couleur.Blanc);
        verifie("Interface : avec plusieurs prises possibles, seule la pièce est signalée au début du tour (arrivées cliquables mais discrètes) ; la préférence ArriveesDebutTour montre toutes les arrivées",
                departSeul && arriveesAvecPreference, $"départ seul {departSeul}, arrivées avec la préférence {arriveesAvecPreference}");
        Clique(44);
        verifie("Interface : avec plusieurs rafles possibles, les étapes ne sont pas numérotées (leurs numéros se mélangeraient)",
                Champ<bool>(fenetre, "pieceEnMain") && Champ<Dictionary<int, string>>(fenetre, "numerosEtape").Count == 0,
                $"pièce en main {Champ<bool>(fenetre, "pieceEnMain")}, numéros {Champ<Dictionary<int, string>>(fenetre, "numerosEtape").Count}");        Clique(44);                             // Un clic sur sa propre case : la pièce revient (ce n'est pas une rafle en boucle)
        Pompe(30);
        Appelle(fenetre, "EffaceMouvements");

        // Retour arrière : les temps sont gardés, la pendule attend le prochain coup
        Nouvelle(5);
        Joue(32, 28);
        maintenant += TimeSpan.FromSeconds(10);
        JoueLegal();            // Noirs : 5:00 - 10 s + 3 s
        maintenant += TimeSpan.FromSeconds(5);
        Appelle(fenetre, "BoutonRetourArriere_Click", null, EventArgs.Empty);
        Tick();
        verifie("Interface : « Retour arrière » retire le coup, garde les temps (noirs 4:53), arme la pendule (elle n'avance plus) et redonne le trait aux noirs",
                session().CoupsPdn.Count == 1 && pendule()?.Etat == EtatPendule.Armee && temps("LabelPenduleNoir") == "N 04:53" && pendule()?.Trait == Couleur.Noir && fenetre.CouleurAuTrait == Couleur.Noir,
                $"{temps("LabelPenduleNoir")}, {pendule()?.Etat}, {session().CoupsPdn.Count} coup(s), trait de la pendule {pendule()?.Trait}, trait du jeu {fenetre.CouleurAuTrait}");
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
                messages.Count == 1 && messages[0].StartsWith("Partie parcourue") && session().CoupsPdn.Count == 2 && Champ<bool>(fenetre, "clickCaseSource") && ImagePresente(31) && !Champ<bool>(fenetre, "pieceEnMain"),
                $"messages : {string.Join(" / ", messages)} ; {session().CoupsPdn.Count} coup(s), indice {session().IndexAffiche}/{session().Positions.Count - 1}, clickCaseSource {Champ<bool>(fenetre, "clickCaseSource")}, partie terminée {fenetre.PartieTerminee}");
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
                && messages.Count == 0 && fenetre.BalisesPartie.White == "Bruno" && Trouve("LabelCoupJoue").Text == "Position initiale" && fenetre.CouleurAuTrait == Couleur.Blanc,
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
        bool clicIgnore = messages.Count == 0 && Champ<bool>(fenetre, "clickCaseSource");
        bool luJusquAuBout = Attendre(() => Champ<CancellationTokenSource?>(fenetre, "lecture") == null, 20000);
        Position visible = pos().Copie();
        visible.Trait = fenetre.CouleurAuTrait;
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
        apresPause.Trait = fenetre.CouleurAuTrait;
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

        // ═══════════════ Force du moteur : la liste des niveaux ═══════════════
        ComboBox niveaux = (ComboBox)Trouve("ComboNiveau");
        bool parDefautMaximum = niveaux.Items.Count == 6 && niveaux.SelectedIndex == 5 && (string)niveaux.SelectedItem! == "Maximum";
        niveaux.SelectedIndex = 1;
        Appelle(fenetre, "EnregistrePreferences");
        bool ecrit = Preferences.Lire(fichierPreferences).Niveau == 2;
        niveaux.SelectedIndex = 5;
        Appelle(fenetre, "EnregistrePreferences");
        verifie("Interface : la force du moteur est une liste de 6 niveaux (Maximum au départ), le niveau choisi est écrit dans les préférences",
                parDefautMaximum && ecrit && Preferences.Lire(fichierPreferences).Niveau == 6, $"{niveaux.Items.Count} niveaux, par défaut : {parDefautMaximum}");
        Preferences voulu = new Preferences { Niveau = 3 };
        voulu.Ecrire(fichierPreferences);
        Appelle(fenetre, "ChargePreferences");
        Appelle(fenetre, "AppliquePreferences");
        verifie("Interface : le niveau des préférences est appliqué à la liste", niveaux.SelectedIndex == 2, niveaux.SelectedIndex.ToString());
        niveaux.SelectedIndex = 5;
        Appelle(fenetre, "EnregistrePreferences");

        // ═══════════════ Retour arrière après une partie décidée ═══════════════
        RetourApresPartieDecidee();
        ParcoursAuClavier();
        RaccourcisEtImage();
        FlecheAnalyse();
        CaseOrdinateur();

        // ═══════════════ Deux fenêtres à la fois ═══════════════
        DeuxFenetres();

        if (!avecMoteur)
            return;

        // ═══════════════ Avec le moteur Scan ═══════════════
        ((TrackBar)Trouve("trackBarTempsReflexion")).Value = 1;     // 1 seconde de réflexion
        Nouvelle(5, avecMoteur: true);
        Joue(32, 28, attendCoups: false);
        bool repondu = Attendre(() => session().CoupsPdn.Count == 2 && !Champ<bool>(fenetre, "jeuMoteurEnCours"), 30000);
        Position affichee = pos().Copie();
        affichee.Trait = fenetre.CouleurAuTrait;
        verifie("Interface + Scan : après le coup de l'humain, le moteur répond : deux coups, trait aux blancs, la position du damier est celle de l'historique",
                repondu && fenetre.CouleurAuTrait == Couleur.Blanc && affichee.VersFen() == session().DernierePosition.VersFen(), $"{string.Join(" ", session().CoupsPdn)}");
        verifie("Interface + Scan : la pendule a compté le coup du moteur (noirs : 1 coup, 5:00 + 3 s puisque l'horloge simulée n'a pas bougé) ; les blancs sont au trait",
                pendule()?.CoupsJoues(Couleur.Noir) == 1 && temps("LabelPenduleNoir") == "N 05:03" && pendule()?.Trait == Couleur.Blanc, $"{temps("LabelPenduleNoir")}, noirs : {pendule()?.CoupsJoues(Couleur.Noir)} coup(s)");

        // Niveau 1 : le moteur répond tout de suite malgré 8 secondes de réflexion permises
        ((TrackBar)Trouve("trackBarTempsReflexion")).Value = 8;
        ((ComboBox)Trouve("ComboNiveau")).SelectedIndex = 0;
        Nouvelle(0);
        for (int i = 0; i < 40 && (i < 10 || ReglesDames.CoupsLegaux(session().DernierePosition).Count < 6 || ReglesDames.CoupsLegaux(session().DernierePosition)[0].Prises.Count > 0); i++)
        {   // Au moins dix demi-coups entre humains, sans prise forcée : hors du livre d'ouvertures, où le moteur répondrait tout de suite à tout niveau
            List<Coup> choix = ReglesDames.CoupsLegaux(session().DernierePosition);
            Joue(choix[choix.Count / 2].Depart, choix[choix.Count / 2].Arrivee);
        }
        ((CheckBox)Trouve("OrdinateurBosse")).Checked = true;
        int coupsAvant = session().CoupsPdn.Count;
        System.Diagnostics.Stopwatch chronoFaible = System.Diagnostics.Stopwatch.StartNew();
        List<Coup> coupsHumain = ReglesDames.CoupsLegaux(session().DernierePosition);
        Joue(coupsHumain[0].Depart, coupsHumain[0].Arrivee, attendCoups: false);
        bool repondFaible = Attendre(() => session().CoupsPdn.Count == coupsAvant + 2 && !Champ<bool>(fenetre, "jeuMoteurEnCours"), 30000);
        long msFaible = chronoFaible.ElapsedMilliseconds;
        verifie("Interface + Scan : au niveau 1, le moteur répond en moins de 3 s alors que 8 s sont permises",
                repondFaible && msFaible < 3000, $"{string.Join(" ", session().CoupsPdn)} en {msFaible} ms");
        ((ComboBox)Trouve("ComboNiveau")).SelectedIndex = 5;
        ((TrackBar)Trouve("trackBarTempsReflexion")).Value = 1;

        // Le drapeau du moteur tombe pendant qu'il réfléchit : il perd, son coup n'est pas joué
        Nouvelle(5, avecMoteur: true);
        Joue(32, 28, attendCoups: false);
        bool reflechit = Champ<bool>(fenetre, "jeuMoteurEnCours");
        maintenant += TimeSpan.FromMinutes(10);
        Tick();
        bool fini = Attendre(() => !Champ<bool>(fenetre, "jeuMoteurEnCours"), 30000);
        Pompe(300);
        verifie("Interface + Scan : le drapeau du moteur tombe pendant sa réflexion : les blancs gagnent au temps, le moteur est arrêté et son coup n'est pas joué",
                reflechit && fini && fenetre.PartieTerminee && session().Resultat == "1-0" && session().CoupsPdn.Count == 1 && messages.Any(m => m.Contains("Les Blancs gagnent") && m.Contains("des noirs")),
                $"réfléchissait : {reflechit}, fini : {fini}, coups : {string.Join(" ", session().CoupsPdn)}, résultat {session().Resultat}");
        verifie("Interface + Scan : après cette fin, le damier reste inactif (le retour du moteur ne le réactive pas)", fenetre.CaseDamier.All(c => !c.Enabled), "");
    }

    // Le parcours au clavier : ← → Début Fin font comme les boutons, sans message aux extrémités, et laissent leur rôle aux flèches d'une zone de texte
    private static void ParcoursAuClavier()
    {
        Nouvelle();
        Joue(32, 28);
        Joue(17, 22);
        Joue(28, 17);       // La prise est obligatoire
        bool Touche(Keys k) => AppelleRetour<bool>(fenetre, "ToucheParcours", k);
        int indice() => fenetre.Session.IndexAffiche;
        bool gauche = Touche(Keys.Left) && indice() == 2;
        bool debut = Touche(Keys.Home) && indice() == 0;
        bool auDebut = Touche(Keys.Left) && indice() == 0 && messages.Count == 0;
        bool droite = Touche(Keys.Right) && indice() == 1;
        bool fin = Touche(Keys.End) && indice() == 3;
        bool alaFin = Touche(Keys.Right) && indice() == 3 && messages.Count == 0;
        verifie("Interface : au clavier, ← → Début Fin parcourent la partie comme les boutons, sans message aux extrémités",
                gauche && debut && auDebut && droite && fin && alaFin && !Touche(Keys.A) && !Touche(Keys.Up), $"gauche {gauche}, début {debut}, au début {auDebut}, droite {droite}, fin {fin}, à la fin {alaFin} ; messages : {string.Join(" / ", messages)}");
        string traitAffiche() => Trouve("LabelInformationJoueur").Text;
        Touche(Keys.Left);      // Deux coups joués : trait aux blancs
        string t2 = traitAffiche();
        Touche(Keys.Left);      // Un coup : trait aux noirs
        string t1 = traitAffiche();
        Touche(Keys.Home);      // Départ : trait aux blancs
        string t0 = traitAffiche();
        Touche(Keys.End);
        verifie("Interface : le parcours met à jour le trait affiché en haut (Préc., Début, Fin)", t2 == "Trait aux Blancs" && t1 == "Trait aux Noirs" && t0 == "Trait aux Blancs" && indice() == 3, $"{t2} / {t1} / {t0}");        fenetre.ActiveControl = Trouve("BoxNomJoueurBlanc");
        bool saisie = !Touche(Keys.Left) && indice() == 3;
        fenetre.ActiveControl = null;
        verifie("Interface : dans la zone du nom d'un joueur, les flèches gardent leur rôle (le parcours ne bouge pas)", saisie, $"indice {indice()}");
        bool viaCommande = AppelleRetour<bool>(fenetre, "ProcessCmdKey", new Message(), Keys.Left) && indice() == 2;
        verifie("Interface : la touche passe par ProcessCmdKey (le chemin de la vraie touche)", viaCommande, $"indice {indice()}");
        Nouvelle();
    }

    // Les raccourcis (Ctrl+N, Ctrl+Z, Ctrl+F), le clic droit et l'image du damier
    private static void RaccourcisEtImage()
    {
        Nouvelle();
        Joue(32, 28);
        Joue(17, 22);
        bool Raccourci(Keys k) => AppelleRetour<bool>(fenetre, "ToucheRaccourci", k);
        bool retour = Raccourci(Keys.Control | Keys.Z) && fenetre.Session.CoupsPdn.Count == 1;
        bool tourne = Raccourci(Keys.Control | Keys.F) && fenetre.Damier.Orientation.CoteNoir;
        bool retourne = Raccourci(Keys.Control | Keys.F) && !fenetre.Damier.Orientation.CoteNoir;
        bool inconnue = !Raccourci(Keys.G) && !Raccourci(Keys.Control | Keys.A);
        verifie("Interface : Ctrl+Z revient en arrière d'un coup, Ctrl+F tourne le damier (et le remet), les autres touches ne sont pas prises",
                retour && tourne && retourne && inconnue, $"retour {retour}, tourne {tourne}, retourne {retourne}, inconnue {inconnue}");
        fenetre.ActiveControl = Trouve("BoxNomJoueurBlanc");
        bool saisie = !Raccourci(Keys.Control | Keys.F) && !Raccourci(Keys.Control | Keys.Z) && fenetre.Session.CoupsPdn.Count == 1 && !fenetre.Damier.Orientation.CoteNoir;
        fenetre.ActiveControl = null;
        verifie("Interface : dans la zone du nom d'un joueur, F et Ctrl+Z gardent leur rôle de saisie", saisie, "");
        bool nouvelle = Raccourci(Keys.Control | Keys.N) && fenetre.Session.CoupsPdn.Count == 0 && fenetre.Session.Positions.Count == 1;
        verifie("Interface : Ctrl+N commence une nouvelle partie", nouvelle, $"{fenetre.Session.CoupsPdn.Count} coup(s)");
        bool viaCommande = AppelleRetour<bool>(fenetre, "ProcessCmdKey", new Message(), Keys.Control | Keys.F) && fenetre.Damier.Orientation.CoteNoir;
        Raccourci(Keys.Control | Keys.F);
        verifie("Interface : le raccourci passe par ProcessCmdKey (le chemin de la vraie touche)", viaCommande && !fenetre.Damier.Orientation.CoteNoir, "");

        // Le clic droit n'est pas un clic de jeu
        PictureBox case32 = fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(32)];
        Appelle(fenetre, "CaseMouseDown", case32, new MouseEventArgs(MouseButtons.Right, 1, 10, 10, 0));
        verifie("Interface : un clic droit sur une pièce ne la sélectionne pas", Champ<bool>(fenetre, "clickCaseSource") && ImagePresente(32), "");

        // L'image du damier
        Joue(32, 28);
        string dossier = Path.Combine(Path.GetTempPath(), "dames_image_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dossier);
        try
        {
            string png = Path.Combine(dossier, "damier.png");
            bool ecrit = fenetre.EnregistreImageDamier(png);
            using (Bitmap lue = new Bitmap(png))
            {
                byte[] debut = File.ReadAllBytes(png).Take(4).ToArray();
                Point centre28 = fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(28)].Location;
                Color pixelPiece = lue.GetPixel(centre28.X + 30, centre28.Y + 30);
                Color pixelVide = lue.GetPixel(fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(32)].Location.X + 5, fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(32)].Location.Y + 30);
                verifie("Interface : l'image du damier est un PNG de 640 × 640 qui montre les pièces (case 28 occupée, case 32 vidée)",
                        ecrit && debut[1] == (byte)'P' && debut[2] == (byte)'N' && debut[3] == (byte)'G' && lue.Width == 640 && lue.Height == 640 && pixelPiece.ToArgb() != pixelVide.ToArgb(),
                        $"{lue.Width}x{lue.Height}, pièce {pixelPiece.Name}, vide {pixelVide.Name}");
            }
            messages.Clear();
            bool mauvaisChemin = !fenetre.EnregistreImageDamier(Path.Combine(dossier, "dossier_absent", "damier.png")) && messages.Count == 1 && messages[0].StartsWith("Image du damier");
            verifie("Interface : une image qu'on ne peut pas écrire est refusée avec un message, sans exception", mauvaisChemin, string.Join(" / ", messages));
            messages.Clear();
            bool copie = fenetre.CopieImageDamier();
            verifie("Interface : l'image se copie dans le presse-papiers (ou, s'il est occupé, un message le dit)", copie ? Clipboard.ContainsImage() : messages.Count == 1, copie ? "copiée" : string.Join(" / ", messages));
        }
        finally
        {
            try { Directory.Delete(dossier, true); } catch (IOException) { }
        }
        ContextMenuStrip? menu = Champ<ContextMenuStrip?>(fenetre, "menuDamier");
        verifie("Interface : le menu du clic droit (image, copie, tourner) est posé sur le damier et sur chaque case",
                menu != null && menu.Items.Count == 4 && Trouve("Damier10x10").ContextMenuStrip == menu && fenetre.CaseDamier.All(c => c.ContextMenuStrip == menu), menu?.Items.Count.ToString() ?? "pas de menu");
        Nouvelle();
    }
    // Les cases colorées du damier (autres que le fond normal) : numéros Manoury
    private static List<int> CasesColorees() => Enumerable.Range(1, 50).Where(n => { Color c = CouleurCase(n); return c != fenetre.CouleurCaseSombre && c != fenetre.CouleurCaseClaire; }).ToList();

    // Après une partie décidée (ici par le drapeau), revenir en arrière par « Précédent » ou « Retour arrière » efface les cases colorées du dernier coup ; le repère brun suit la position affichée
    // La flèche du meilleur coup de « Analyse position » : elle suit la première étape de la variation du moteur (seulement pendant une analyse), se voit sur le damier (cases sombres et claires), disparaît quand la position change
    private static void FlecheAnalyse()
    {
        Nouvelle();
        FieldInfo analyse = fenetre.GetType().GetField("analyseEnCours", Tous)!;
        Appelle(fenetre, "AnalyseLigneMoteur", "info depth=5 score=0.10 nodes=100 time=0.1 nps=1 pv=\"32-28 18-23\"");
        bool sansAnalyse = Champ<List<int>?>(fenetre, "fleche") == null;
        verifie("Interface : hors d'une analyse, la variation du moteur ne dessine pas de flèche", sansAnalyse, "");
        Point c32 = fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(32)].Location, c28 = fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(28)].Location;
        int mx = (c32.X + c28.X) / 2 + 30, my = (c32.Y + c28.Y) / 2 + 30;      // Le coin commun aux deux cases : il touche aussi deux cases claires
        Color avant;
        using (Bitmap image = AppelleRetour<Bitmap>(fenetre, "ImageDuDamier"))
            avant = image.GetPixel(mx, my);
        analyse.SetValue(fenetre, true);
        Appelle(fenetre, "AnalyseLigneMoteur", "info depth=5 score=0.10 nodes=100 time=0.1 nps=1 pv=\"32-28 18-23\"");
        List<int>? cases = Champ<List<int>?>(fenetre, "fleche");
        Color milieu; Color[] autour;
        using (Bitmap image = AppelleRetour<Bitmap>(fenetre, "ImageDuDamier"))
        {
            milieu = image.GetPixel(mx, my);
            autour = new[] { image.GetPixel(mx - 1, my - 1), image.GetPixel(mx + 1, my - 1), image.GetPixel(mx - 1, my + 1), image.GetPixel(mx + 1, my + 1) };      // Les quatre cases du coin : deux sont claires
        }
        bool Rouge(Color c) => c.R > 110 && c.G < 90;      // Le rouge brique de la flèche : le bois et les cases claires ont un vert bien plus élevé
        verifie("Interface : pendant l'analyse, la première étape de la variation est une flèche du départ à l'arrivée (32 → 28)", cases != null && cases.SequenceEqual(new[] { 32, 28 }), cases == null ? "aucune" : string.Join(",", cases));
        verifie("Interface : la flèche se voit sur le damier, y compris sur les cases claires qui touchent le coin entre les deux cases",
                !Rouge(avant) && Rouge(milieu) && autour.All(Rouge), $"avant {avant.Name}, milieu {milieu.Name}, autour {string.Join(" ", autour.Select(c => c.Name))}");
        Appelle(fenetre, "AnalyseLigneMoteur", "info depth=6 score=0.20 nodes=100 time=0.1 nps=1 pv=\"31-27 18-23\"");
        bool suit = Champ<List<int>?>(fenetre, "fleche") is List<int> suivante && suivante.SequenceEqual(new[] { 31, 27 });
        Appelle(fenetre, "AnalyseLigneMoteur", "info depth=7 score=0.20 nodes=100 time=0.1 nps=1 pv=\"11-12 18-23\"");
        bool illegal = Champ<List<int>?>(fenetre, "fleche") == null;
        verifie("Interface : la flèche suit la variation à chaque ligne du moteur ; un coup qui n'est pas légal l'efface", suit && illegal, $"suit {suit}, illégal effacé {illegal}");
        Appelle(fenetre, "AnalyseLigneMoteur", "info depth=8 score=0.20 nodes=100 time=0.1 nps=1 pv=\"32-28 18-23\"");
        analyse.SetValue(fenetre, false);
        Joue(32, 28);
        verifie("Interface : un coup joué efface la flèche", Champ<List<int>?>(fenetre, "fleche") == null, "");
    }

    // La case « Ordinateur » est la seule source de « l'ordinateur répond » : ni « Ordinateur joue » ni « Retour arrière » ne la désynchronisent (avant, ils forçaient le réglage interne : la case et le comportement divergeaient)
    private static void CaseOrdinateur()
    {
        CheckBox ordinateur = (CheckBox)Trouve("OrdinateurBosse");
        ((TrackBar)Trouve("trackBarTempsReflexion")).Value = 1;
        Nouvelle(0);
        ordinateur.Checked = false;
        bool decoche = !fenetre.OrdinateurActif;
        Appelle(fenetre, "OrdinateurJoue_Click", null, EventArgs.Empty);
        Pompe(100);
        Attendre(() => !Champ<bool>(fenetre, "jeuMoteurEnCours"), 30000);       // Avec le moteur Scan (console complète) il joue vraiment : on attend sa réponse
        verifie("Interface : case « Ordinateur » décochée, « Ordinateur joue » ne rend pas l'ordinateur actif", decoche && !ordinateur.Checked && !fenetre.OrdinateurActif, $"case {ordinateur.Checked}, actif {fenetre.OrdinateurActif}");
        Nouvelle(0);
        Joue(32, 28);
        ordinateur.Checked = true;
        Appelle(fenetre, "BoutonRetourArriere_Click", null, EventArgs.Empty);
        bool coche = fenetre.OrdinateurActif;
        ordinateur.Checked = false;
        Joue(32, 28);
        Appelle(fenetre, "BoutonRetourArriere_Click", null, EventArgs.Empty);
        verifie("Interface : « Retour arrière » laisse l'ordinateur actif quand la case est cochée, inactif quand elle est décochée", coche && !fenetre.OrdinateurActif, $"cochée : {coche}, décochée : {!fenetre.OrdinateurActif}");
        Nouvelle(0);
        Joue(32, 28);        // L'état que les tests suivants attendent : un coup joué
    }

    private static void RetourApresPartieDecidee()
    {
        Nouvelle(5);
        Joue(32, 28);
        Joue(17, 22);
        maintenant += TimeSpan.FromMinutes(10);
        Tick();
        Pompe(100);
        bool decidee = fenetre.PartieTerminee && CasesColorees().Count > 0;
        Appelle(fenetre, "BoutonPrecedent_Click", null, EventArgs.Empty);
        verifie("Interface : après une partie décidée, « Précédent » efface les cases colorées du dernier coup et le repère montre le coup de la position affichée",
                decidee && CasesColorees().Count == 0 && Champ<int>(fenetre, "dernierDepartManoury") == 32 && Champ<int>(fenetre, "dernierArriveeManoury") == 28, $"décidée : {decidee}, colorées : {string.Join(",", CasesColorees())}");
        Appelle(fenetre, "BoutonSuivant_Click", null, EventArgs.Empty);
        verifie("Interface : « Suivant » ne laisse pas non plus de cases colorées d'un coup précédent", CasesColorees().Count == 0 && Champ<int>(fenetre, "dernierArriveeManoury") == 22, string.Join(",", CasesColorees()));
        Appelle(fenetre, "BoutonRetourArriere_Click", null, EventArgs.Empty);
        verifie("Interface : après une partie décidée, « Retour arrière » efface aussi les cases colorées", !fenetre.PartieTerminee && CasesColorees().Count == 0 && Champ<int>(fenetre, "dernierArriveeManoury") == 28, string.Join(",", CasesColorees()));
        Nouvelle();
    }
    // Deux fenêtres ouvertes en même temps ne partagent plus rien (position, historique, sens du damier, moteur) : un coup, un damier tourné ou la fermeture de l'une ne touche pas l'autre.
    // Les aides du test visent la variable « fenetre » : on la fait pointer tour à tour sur chacune.
    private static void DeuxFenetres()
    {
        BrunoInterfaceGraphiqueDames a = fenetre;
        Nouvelle();
        Joue(32, 28);
        BrunoInterfaceGraphiqueDames b = new BrunoInterfaceGraphiqueDames();
        b.ShowInTaskbar = false;
        b.StartPosition = FormStartPosition.Manual;
        b.Location = new Point(-32000, -32000);
        b.Show();
        Pompe(300);
        b.DureeEtape = 0;
        try
        {
            fenetre = b;
            Nouvelle();
            Joue(34, 30);
            Appelle(b, "BoutonTournerDamier_Click", null, EventArgs.Empty);
            Pompe(30);
            fenetre = a;
            verifie("Interface, deux fenêtres : un coup joué dans la seconde ne change ni la position, ni l'historique, ni le trait de la première, et tourner le damier de la seconde laisse la première dans son sens",
                    string.Join(" ", a.Session.CoupsPdn) == "32-28" && string.Join(" ", b.Session.CoupsPdn) == "34-30" && a.Damier.PositionCourante[28] == Piece.PionBlanc && a.Damier.PositionCourante[34] == Piece.PionBlanc
                    && b.Damier.PositionCourante[30] == Piece.PionBlanc && b.Damier.PositionCourante[32] == Piece.PionBlanc && !a.Damier.Orientation.CoteNoir && b.Damier.Orientation.CoteNoir,
                    $"première : {string.Join(" ", a.Session.CoupsPdn)}, seconde : {string.Join(" ", b.Session.CoupsPdn)}");
            Joue(17, 22);
            verifie("Interface, deux fenêtres : un coup joué dans la première n'ajoute rien à l'historique de la seconde, et chacune a son propre moteur",
                    a.Session.CoupsPdn.Count == 2 && b.Session.CoupsPdn.Count == 1 && !ReferenceEquals(Champ<object>(a, "moteur"), Champ<object>(b, "moteur")), $"{a.Session.CoupsPdn.Count} / {b.Session.CoupsPdn.Count} coup(s)");
        }
        finally
        {
            fenetre = a;
            try { b.Close(); } catch (Exception ex) { Console.WriteLine("Fermeture de la seconde fenêtre de test : " + ex.Message); }
        }
        Pompe(100);
        verifie("Interface, deux fenêtres : après la fermeture de la seconde, la première est intacte et joue encore", !a.IsDisposed && b.IsDisposed && a.Session.CoupsPdn.Count == 2 && JoueLegal() != null && a.Session.CoupsPdn.Count == 3, $"{a.Session.CoupsPdn.Count} coup(s)");
        Nouvelle();
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
        PictureBox caseClic = fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(caseManoury)];
        Appelle(fenetre, "CaseMouseDown", caseClic, new MouseEventArgs(MouseButtons.Left, 1, 10, 10, 0));
    }

    // Deux clics (départ puis arrivée) ; attend que le coup soit dans l'historique (les rafles et le moteur sont asynchrones)
    private static void Joue(int depart, int arrivee, bool attendCoups = true)
    {
        int avant = fenetre.Session.CoupsPdn.Count;
        Clique(depart);
        bool selectionne = !Champ<bool>(fenetre, "clickCaseSource");
        Clique(arrivee);
        if (attendCoups && !Attendre(() => fenetre.Session.CoupsPdn.Count == avant + 1, 5000))
            Console.WriteLine($"      (coup {depart}-{arrivee} non joué : sélection faite {selectionne}, {fenetre.Session.CoupsPdn.Count} coup(s), trait {fenetre.CouleurAuTrait}, partie terminée {fenetre.PartieTerminee}, messages : {string.Join(" / ", messages)})");
    }

    // Joue le premier coup légal de la dernière position (deux clics) ; null s'il n'y en a pas
    private static Coup? JoueLegal()
    {
        Coup? coup = ReglesDames.CoupsLegaux(fenetre.Session.DernierePosition).FirstOrDefault();
        if (coup != null)
            Joue(coup.Depart, coup.Arrivee);
        return coup;
    }

    private static void Tick() => Appelle(fenetre, "MinuteriePendule_Tick", null, EventArgs.Empty);
    private static bool ImagePresente(int caseManoury) => fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(caseManoury)].Image != null;
    private static Color CouleurCase(int caseManoury) => fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(caseManoury)].BackColor;
    // Chaque case sombre porte l'image de sa pièce, ou aucune image si elle est vide : pas de cadre gris oublié, pas de pièce fantôme
    private static bool ImagesCoherentes()
    {
        for (int numero = 1; numero <= 50; numero++)
        {
            Piece piece = fenetre.Damier.PositionCourante[numero];
            Image? attendue = piece == Piece.Vide ? null : fenetre.ListeBitmapsPiece[piece];
            if (!ReferenceEquals(fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(numero)].Image, attendue))
                return false;
        }
        return true;
    }
    private static bool CaseActive(int caseManoury) => fenetre.CaseDamier[fenetre.Damier.ObtenirIndicePictureBox(caseManoury)].Enabled;

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
