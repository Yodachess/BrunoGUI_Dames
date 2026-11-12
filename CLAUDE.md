# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Projet

BrunoGUI_Dames : interface graphique Windows (C# WinForms, composants Krypton) pour jouer et analyser des parties de **dames internationales** (damier 10×10, notation Manoury), qui pilote le moteur **Scan 3.1** de Fabien Letouzey par le protocole **Hub 2** (`Protocole_Hub2.txt`, dossier `Dames_Scan31_FabienLetouzey`, copié à côté de l'exe). Parties au format **PDN** (`FichierPartiePdn.cs`, `AffichePdn.cs`, exemples dans `Dames_PartiesPdn_FEN`). Programme « frère » de BrunoGUI_GenII (échecs, `..\BrunoGUI_GenII`) : mêmes pratiques, à appliquer ici progressivement.

Tout le code, les identifiants et les commentaires sont en français : garder cette convention. Les **identifiants n'ont pas d'accents**, les commentaires et les textes affichés en ont.

## État au 2026-11-12 (avant tout travail avec Claude Code)

- **.NET 10** (`net10.0-windows`) depuis le 2026-11-12 (.NET 8 n'est plus pris en charge depuis le 2026-11-10) ; compile en Debug et en Release, à zéro avertissement (l'événement `AfficheCoupMoteur`, jamais déclenché, a été supprimé avec son gestionnaire).
- Krypton 95 (`95.25.10.293`, comme BrunoGUI_GenII) depuis le 2026-11-12. Seul `Krypton.Toolkit` est utilisé dans le code : `Docking`, `Navigator`, `Ribbon` et `Workspace` sont référencés sans usage.
- `<Nullable>enable</Nullable>` activé le 2026-11-12, à zéro avertissement nullable (aucun `!` ajouté).
- Pas de projet de tests, pas de CI (`.github\workflows` existe mais est vide), pas de `<Version>` (la version « 1.04 » est écrite en dur dans `Apropos_Click`).
- Un gros formulaire (`BrunoInterfaceGraphiqueDames.cs`, ~100 Ko) ; règles dans `LogiqueMouvementsDames.cs`, damier dans `GestionDamier.cs`, moteur dans `MoteurDamesScan.cs`.
- `.gitattributes` impose le CRLF dans le répertoire de travail pour `*.cs`, `*.csproj`, `*.sln`, `*.resx`, `*.md`, `*.ini`, `*.yml`, `*.tsv` (le dépôt les garde en LF) ; le reste (parties PDN, FEN, fichiers du moteur Scan) reste en `text=auto`.
- Architecture : voir la section ci-dessous (relevée par lecture du code le 2026-11-12, sans exécution ni test).

## Architecture

Un seul projet WinForms (`BrunoGUI_Dames.csproj`, `WinExe`), environ 3 000 lignes de C# hors `.Designer.cs`. **Tout l'état est statique** et les règles dépendent du formulaire principal (`RaflesPossibles`, `CouleurAuTrait`, `VisuCoteNoir`… y sont déclarés) : la logique n'est pas encore séparée de l'interface.

| Fichier | Rôle |
|---|---|
| `Program.cs` | Point d'entrée, lance `BrunoInterfaceGraphiqueDames`. |
| `BrunoInterfaceGraphiqueDames.cs` (~1 600 lignes) | Formulaire principal : création des 100 `PictureBox`, clics, animation des coups, dialogue avec le moteur, boutons, parcours de partie, chargement/sauvegarde. Contient aussi les listes statiques de cases à colorier. |
| `LogiqueMouvementsDames.cs` | Règles : génération des déplacements et des rafles, FEN ↔ Hub2, listes de pièces, listes de coups (`ListeCoupsFen/Pdn/Hub2`), énumérations `TypePiece` / `CouleurPiece`. |
| `GestionDamier.cs` | `ContenuCase` et tables de correspondance Manoury ↔ index `PictureBox`, `DamierContenu[10,10]`. |
| `MoteurDamesScan.cs` | Processus `scan.exe hub`, lecture de sa sortie, événements statiques vers le formulaire. |
| `FichierPartiePdn.cs` | Fenêtre « liste des parties » **et** analyseur PDN (`DecodeFichierPDN`, `DecodePartiePDN`, classe `PartieDamesPdn`). |
| `AffichePdn.cs` | Fenêtre d'affichage des coups (PDN, FEN, Hub2) et enregistrement PDN (`GenerationPdn`). |
| `DonneesBrutesDames.cs` | Fenêtre du protocole Hub2 (échanges bruts). Les trois fenêtres annexes se masquent au lieu de se fermer. |

**Damier et numérotation.** Cases Manoury 1..50 ↔ index `PictureBox` 0..99 (`ManouryVersIndexPictureBox[manoury-1]`, `PictureBoxVersManoury`) ; `DamierContenu[ligne,colonne]` décrit chaque case ; quatre listes de cases Manoury (`ListePionsBlancs`, `ListeDamesBlanches`, `ListePionsNoirs`, `ListeDamesNoires`) doublonnent ce contenu et ne restent synchronisées que par `DessinePiece`, `EffacePiece` et `MiseaJourListes`. « Tourner damier » recalcule les tables (index → 99 − index), repositionne les `PictureBox`, retourne `DamierContenu` et bascule `VisuCoteNoir` ; en vue côté noir, `CaseMouseDown` convertit le nom de la `PictureBox` par `IndiceVisuCoteNoir` (99 − n) ; le sens de marche des pions dépend de `VisuCoteNoir` et de `CouleurPieceCliquee` (chaîne `"Blanc"` / `"Noir"`).

**Règles.** `TrouverMouvementsPossibles(caseManoury)` renvoie (déplacements simples, rafles) pour **une** pièce, et a des effets de bord statiques (`RaflesPossibles`, `AucunCoupBlancPossible/NoirPossible`). La couleur qui prend est `CouleurAuTrait` (chaîne) : `AnalyseCoupObligatoireUnique` l'inverse temporairement pour cette raison. La prise majoritaire n'est pas dans le générateur : `AnalyseCoupObligatoire` calcule `RaflesMaximales` pour toutes les pièces du trait, colorie les cases et n'active que les cases de départ et d'arrivée des rafles les plus longues ; `DessineMouvements` filtre aussi par pièce.

**Un coup humain.** `CaseMouseDown` (gestionnaire commun des 100 cases) : premier clic = sélection (la pièce est effacée de la case, son image devient le curseur, `DessineMouvements` montre les destinations) ; second clic = destination → `AnimerEtExecuterRafle` (async, `DureeAnimation` = 900 ms par étape, les pièces prises ne sont enlevées qu'à la fin) ou `ExecuteDeplacement` ; puis `ChangerCouleurTrait`, ajout du FEN et de la position Hub2 aux listes (le coup PDN est ajouté dans les deux méthodes d'exécution) et, si `PartieEnCours` est vrai, réflexion du moteur. Attention au nom : `PartieEnCours` signifie « l'ordinateur joue » (case à cocher ; remis à vrai après un coup du moteur, à faux par « Retour arrière »).

**Le moteur.** `MoteurDamesScan` (tout statique) lance `scan.exe hub` ; sa sortie arrive sur un thread du pool (`OutputDataReceived`) qui remplit `DonneesScan`, déclenche les événements `AfficheScan` / `AfficheDonneesBrutes` (le formulaire passe par `Invoke`) et, sur `done move=… ponder=…`, pose `CoupScan`, `SuggestionScan` et `CoupScanJoue`. `LancementReflexionMoteur` envoie `pos pos=<Hub2>`, `level move-time=<s>`, `go think`, puis **scrute `CoupScanJoue` toutes les 100 ms** ; pas de numérotation des demandes ni d'annulation. « Analyse position » suit le même chemin avec `analyseEnCours` : score et variation sont affichés, rien n'est joué. Le coup du moteur est relu en « `a-b` » ou « `axb` » (case de départ et case d'arrivée finale).

**Historique de la partie.** Trois listes parallèles : `ListeCoupsFen` et `ListeCoupsHub2` (position après chaque demi-coup, l'indice 0 est la position initiale) et `ListeCoupsPdn` (« `28x19` », le résultat est ajouté à la fin avec un espace devant : `" 1-0"`). `indexFenCoupActuel` sert au parcours ; « Retour arrière » retire le dernier élément de chacune. Le FEN est gardé avec son enrobage `[FEN "W:W31-50:B1-20"]` ; `ConvertitFenVersHub2` produit les 51 caractères attendus par Scan (trait + 50 cases `w b W B e`).

**PDN.** `DecodeFichierPDN` coupe le fichier sur les lignes `[Event ` ; `DecodePartiePDN` lit les balises (Event, Site, Date, Round, White, Black, Result, PlyCount, FEN), retire les commentaires `{}` et `()` et garde les coups à partir de « 1. ». Le double-clic sur une partie appelle `ChargerPartieDepuisPdn` puis `GenerationListesCoups`, qui **rejoue chaque coup dans l'interface réelle**, avec animation. `GenerationPdn` écrit les balises puis cinq paires de coups par ligne.

**Fichiers hors code.** `Dames_Scan31_FabienLetouzey` : `scan.exe`, `scan.ini`, données (livres et évaluations, ~53 Mo), sources C++ et binaires Linux/Mac ; tout est copié à côté de l'exe (`Content` + `PreserveNewest`), sources et binaires Linux/Mac compris. `Dames_PartiesPdn_FEN` : exemples, non copiés. `Properties\PublishProfiles\FolderProfile.pubxml` : win-x64, autonome, fichier unique, vers `E:\Release_Dames_1_04`. `App.config` mentionne .NET Framework 4.8 : reliquat sans effet. `BrunoGUI_Readme.txt`, `README.md` (FR/EN) décrivent les boutons ; `Protocole_Hub2.txt` et `Readme_Fabien.txt` documentent le moteur.

### Points fragiles relevés à la lecture

À confirmer par des tests (chantier 2) avant de toucher au code ; ce sont des constats de lecture, pas des bugs reproduits.

1. `LancementReflexionMoteur` calcule `raflesFiltrees` (rafles qui finissent sur la case annoncée par le moteur) puis ne s'en sert pas : la rafle animée est la première de `RaflesMaximales`, pas forcément celle du moteur.
2. Une rafle est identifiée par sa seule case d'arrivée finale (`FirstOrDefault`, clic humain, coup du moteur, PDN « `28x19` ») : deux rafles qui finissent au même endroit en prenant des pièces différentes sont indiscernables.
3. Rafle de pion : la pièce prise est retirée temporairement du damier (`RechercherRaflesPion`), alors que la règle la garde jusqu'à la fin de la rafle ; rafle de dame : la case de départ de la pièce reste occupée pendant la rafle. Cas à tester : retour sur sa case de départ, passage sur la case d'une pièce déjà prise, rafles circulaires.
4. `VerifierGain` ne teste que « plus aucune pièce » ; joueur bloqué et parties nulles sont en TODO. Il affiche un `MessageBox` depuis la logique.
5. `DessinePiecesAvecListes` crée un `BrunoInterfaceGraphiqueDames` complet à chaque appel, uniquement pour appeler `DessinePiece`.
6. Moteur : chemin construit avec `Directory.GetCurrentDirectory()` (à remplacer par `Application.StartupPath`) ; `OutputDataReceived` est abonné après `Process.Start` ; `Quitte()` n'est appelé que par le bouton « Quitter » (pas par la croix de la fenêtre : `scan.exe` peut rester actif) ; `Process`, `ScanVersGui` et `CoupScanJoue` sont statiques et partagés entre threads ; la ligne `error` du moteur n'est écrite qu'à la console.
7. `GenerationListesCoups` est un `async void` qui lève des exceptions (`FormatException`, `InvalidOperationException`) : une partie PDN qui ne se rejoue pas fait planter l'application. `DecodeFichierPDN` supprime `?`, `!` et `..` de **toutes** les lignes, balises comprises (`[White "?"]` devient `[White ""]`).
8. Aucun journal des erreurs : tout passe par `Console.WriteLine` (invisible en `WinExe`) et les `catch` avalent les exceptions.
9. Aucune préférence enregistrée (couleurs, temps de réflexion, noms, son) ; le son est lu depuis `C:\Windows\Media\Windows Notify.wav` en dur.

## Commandes

```bash
dotnet build BrunoGUI_Dames.sln
```

```bash
dotnet run --project BrunoGUI_Dames.csproj
```

- `git` n'est pas dans le PATH de PowerShell sur cette machine : utiliser celui de Visual Studio 2022 (`C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Git\cmd\git.exe`). Remote `origin` = https://github.com/Yodachess/BrunoGUI_Dames, branche `master`.

## Pratiques (reprises de BrunoGUI_GenII)

**Manière de travailler**
- Avant de compiler : vérifier que `BrunoGUI_Dames.exe` ne tourne pas, sinon demander de la fermer (l'exe ouvert verrouille `bin`). Ne jamais lancer l'application sans prévenir.
- Commit seulement après un essai dans l'application ; push seulement sur demande explicite (« pousse les commits ») ; version publiée ou tag seulement sur demande.
- `git add` avec la liste explicite des fichiers modifiés, jamais `git add -A` ni `commit -a` : Visual Studio modifie aussi le dépôt, et un changement qu'on n'a pas fait n'est pas à commiter sans accord.
- Messages de commit en français ; jamais de nom de personne à la 3e personne dans le code, les tests ou ce fichier : écrire la raison, éventuellement datée.
- Tenir ce fichier à jour à chaque changement d'architecture ou de convention.

**Code**
- Zéro avertissement de compilation ; une fois activés, les types nullables restent à zéro avertissement.
- Fichiers du projet en CRLF (imposé par `.gitattributes`, comme dans BrunoGUI_GenII).
- La logique (règles, notation, PDN, moteur) n'utilise ni Windows Forms ni Krypton : elle ne montre jamais de fenêtre ni de message, et elle est testée (projet `Tests` : une console qui affiche les vérifications et un test xUnit par vérification, perft compris pour le générateur de coups). L'interface n'a pas de tests : la vérifier à la main.
- La position du jeu dans un objet que l'on peut copier, et les calculs « pour voir » faits sur une copie, jamais sur la partie.
- Le thread du moteur ne modifie jamais l'état de la partie : passage au thread de l'interface par un `Invoke` protégé ; chaque demande au moteur est numérotée et toute action qui change la partie abandonne la réflexion en cours.
- Journal des erreurs à côté de l'exe ; tout `catch` qui avale une erreur l'y écrit.
- Chemins cherchés à côté de l'exécutable (`Application.StartupPath`), jamais relatifs au dossier courant.
- Préférences : un `.ini` livré, jamais écrit, et un `.preferences.ini` personnel écrit à la fermeture.

**Publication** (quand elle existera)
- La balise `<Version>` du `.csproj` est le numéro de version ; une ligne d'historique dans « À propos » ; tag `vX.YY` seulement sur demande.
- Workflow GitHub Actions : tests à chaque push, et à chaque tag un zip **autonome** (`dotnet publish -r win-x64 --self-contained`) joint à la Release.
- Aide en HTML, en français et en anglais si l'interface est traduite ; toute fonction visible y est décrite.

## Chantiers proposés (dans l'ordre)

1. ~~.NET 10, Krypton 95, `.gitattributes` en CRLF, `<Nullable>` à zéro avertissement~~ (fait le 2026-11-12).
2. Projet `Tests` (règles des dames : prises obligatoires et majoritaires, rafles, promotion ; notation Manoury ; lecture et écriture PDN), puis CI.
3. Séparer la logique du gros formulaire, sur le modèle de BrunoGUI_GenII (`Position`, `Regles`, `Partie`, `PiloteMoteur`).
