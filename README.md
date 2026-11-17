# BrunoGUI Dames

BrunoGUI Dames v1.12 - Copyright 2024-2026 Bruno Courtois - licence GPL 3.0 ou ultérieure (voir LICENSE.txt)

Interface graphique Windows pour jouer et analyser des parties de **dames internationales** (damier 10x10, notation Manoury).
Application WinForms en C# 14 sous .NET 10 (Visual Studio 2022), composants Krypton.

Pour le moteur, je me suis tourné vers le logiciel de dames **Scan 3.1** de Fabien Letouzey (Fruit, Seipan, ...), dont je connais la qualité.
Scan utilise le protocole Hub2, proche du protocole UCI des échecs (github.com/rhalbersma/scan).

## Installation

Téléchargez l'archive `BrunoGUI_Dames-vX.YY.zip` de la dernière version sur la page des Releases
(github.com/Yodachess/BrunoGUI_Dames/releases).
Avant de la décompresser : clic droit sur le zip > Propriétés > « Débloquer ». Décompressez, puis lancez `BrunoGUI_Dames.exe`.
Rien d'autre à installer : .NET et le moteur Scan 3.1 sont inclus. Windows peut afficher un avertissement SmartScreen
(le programme n'est pas signé) : « Informations complémentaires » puis « Exécuter quand même ».
L'aide s'ouvre par le bouton « Aide (F1) » ou la touche F1 ; elle existe en français et en anglais (l'anglais si Windows est en anglais, avec un lien vers l'autre langue en haut de la page).

## Fonctions

Le damier est prêt à jouer au lancement. On clique sur la pièce à jouer, le programme montre ses déplacements possibles,
on clique sur la case d'arrivée. Les règles sont celles des dames internationales : les pions prennent en avant et en arrière,
les dames sont volantes, la prise est obligatoire et majoritaire. Quand plusieurs rafles de mêmes départ et arrivée prennent
des pièces différentes, une fenêtre laisse choisir en montrant les pièces prises de chacune.
Une série de boutons à droite donne accès aux options (sauf pendant la réflexion du moteur).

Boutons disponibles :
* "Nouvelle Partie" : démarre une nouvelle partie
* "Ordinateur joue" : lance la réflexion du moteur et joue son coup sur le damier
* "Retour Arrière" : recule la partie d'un demi-coup
* "Tourner damier" : fait pivoter le damier de 180° (par défaut, vue côté blancs)
* Clic droit sur le damier : enregistrer ou copier l'image du damier, tourner le damier. Raccourcis : Ctrl+N (nouvelle partie), Ctrl+Z (retour arrière), Ctrl+E (image), Ctrl+F (tourner)
* "Analyse position" : lance la réflexion du moteur et donne le résultat SANS jouer le coup sur le damier
* "Liste des coups" : affiche la liste des coups joués, aux formats PDN, FEN et Hub2
* "Parcours partie" (Début, Préc., Suiv., Fin) : parcourt la partie coup par coup (au clavier : flèches ← →, touches Début et Fin) ; "Lecture" la déroule avec ses animations (bouton "Pause" pendant la lecture, curseur de vitesse)
* "Charge Parties" : lecture d'un fichier PDN et affichage de la liste de ses parties (double-clic pour dérouler une partie)
* "Masque parties" : cache ou montre la liste des parties
* "Sauve Partie" : sauvegarde de la partie au format PDN, peut être ajoutée à la fin d'un fichier existant
* "Charge position" : lecture d'un fichier FEN et mise en place de la position
* "Sauve position" : sauvegarde de la position au format FEN
* "Protocole" : affiche les échanges du protocole Hub2 entre l'interface et le moteur
* "A propos de BIG" : version du logiciel et du moteur
* "Aide (F1)" : ouvre l'aide
* "Quitter"

On peut jouer entre humains en décochant "Ordinateur". Si "Son" est coché, un son est émis lorsque le moteur a joué.
Le temps de réflexion du moteur se règle de 1 à 25 secondes (5 par défaut).
Une pendule est proposée avec les cadences de la FMJD (1 h 20 + 30 s, 90 min/45 coups, 2 h/50 coups, rapide 15 min + 5 s, blitz 5 min + 3 s) :
désactivée par défaut, elle démarre au premier coup (non décompté) et le joueur dont le drapeau tombe perd la partie, le moteur y compris.
Un bandeau en haut de la fenêtre indique le statut (trait aux blancs ou aux noirs, résultat éventuel) ;
sous le bandeau, le score estimé (en centipions) et la ligne du moteur. Les noms des joueurs sont modifiables et repris dans le PDN.

Fin de partie : le programme détecte le gain (plus de pièce, ou plus aucun coup possible) et les nulles par répétition (trois fois la même
position), par 25 coups de dames de chaque joueur, et par les finales à peu de pièces (16 et 5 coups, règles FMJD 6.3 et 6.4).
La nulle par accord, l'abandon, etc. se saisissent en cochant le résultat (1-0, 0-1, 1/2-1/2) : il est ajouté à la liste des coups PDN.

Les préférences (temps de réflexion, noms, son, ordinateur, cadence de la pendule, sens du damier, durée d'animation, couleurs du damier) sont retenues
dans `BrunoGUI_Dames.preferences.ini` à côté du programme. Les erreurs et problèmes sont notés dans `BrunoGUI_Dames.log` (un ancien journal est gardé en `.log.old` au-delà de 1 Mo).

## Compiler les sources

Il faut le SDK .NET 10 sous Windows. Les tests vérifient les règles (dont un perft de profondeur 7), le PDN, les nulles et le dialogue avec le moteur Scan.

    dotnet build BrunoGUI_Dames.sln
    dotnet run --project BrunoGUI_Dames.csproj
    dotnet test Tests

Les parties PDN et positions FEN d'exemple sont dans `Dames_PartiesPdn_FEN`. Le moteur Scan, avec ses sources et sa licence, est dans `Dames_Scan31_FabienLetouzey`.

Le protocole Hub2 et les notes du moteur sont dans `Docs`. L'historique des versions est dans `CHANGELOG.md`.

Toute remarque ou signalement d'anomalie est bienvenu (joindre `BrunoGUI_Dames.log` s'il existe).

---

# English version

BrunoGUI Dames v1.12 - Copyright 2024-2026 Bruno Courtois - GPL 3.0 or later license (see LICENSE.txt)

A Windows graphical interface for playing and analysing **international draughts** (10x10 board, Manoury notation).
WinForms application written in C# 14 on .NET 10 (Visual Studio 2022), with Krypton components.

Knowing the quality of Fabien Letouzey's software (Fruit, Seipan, ...), I chose his draughts engine **Scan 3.1**.
Scan uses the Hub2 protocol, which is similar to the UCI protocol in chess (github.com/rhalbersma/scan).

## Installation

Download `BrunoGUI_Dames-vX.YY.zip` of the latest version from the Releases page (github.com/Yodachess/BrunoGUI_Dames/releases).
Before extracting: right-click the zip > Properties > “Unblock”. Extract it, then run `BrunoGUI_Dames.exe`.
Nothing else to install: .NET and the Scan 3.1 engine are included. Windows may show a SmartScreen warning
(the program is not signed): “More info”, then “Run anyway”.
The help opens with the "Aide (F1)" button or the F1 key. It is available in English and French (English when Windows is set to English, with a link to the other language at the top of the page). The program's screen itself is in French.

## Features

The board is ready to play at startup. Click on the piece to play, the program shows its possible moves, then click on the destination square.
The rules are those of international draughts: men capture forwards and backwards, kings are flying, capturing is mandatory and
the maximum number of pieces must be taken. When several captures with the same start and end squares take different pieces, a dialog
lets you choose, showing the captured pieces of each one.
A series of buttons on the right gives access to the options (except while the engine is thinking).

Available buttons:
* "Nouvelle Partie" (New game): starts a new game
* "Ordinateur joue" (Computer plays): starts the engine's calculation and plays its move on the board
* "Retour Arrière" (Undo): reverts the game by half a move
* "Tourner damier" (Rotate board): rotates the board by 180° (default: White's perspective)
* Right-click on the board: save or copy the board image, rotate the board. Shortcuts: Ctrl+N (new game), Ctrl+Z (take back), Ctrl+E (image), Ctrl+F (rotate)
* "Analyse position" (Analyse position): runs the engine's calculation but does NOT play the move on the board
* "Liste des coups" (Move list): displays the moves played in PDN, FEN and Hub2 formats
* "Parcours partie" (Début, Préc., Suiv., Fin): steps through the game move by move (on the keyboard: ← → arrows, Home and End keys); "Lecture" (Play) plays it back with animations ("Pause" while playing, speed slider)
* "Charge Parties" (Load games): reads a PDN file and displays the list of its games (double-click to play a game back)
* "Masque parties" (Hide games): hides or shows the list of games
* "Sauve Partie" (Save game): saves the game in PDN format, with the option to append it to an existing file
* "Charge position" (Load position): reads a FEN file and sets up the position
* "Sauve position" (Save position): saves the current position in FEN format
* "Protocole" (Protocol): displays the Hub2 protocol exchanges between the GUI and the engine
* "A propos de BIG" (About): software and engine versions
* "Aide (F1)" (Help): opens the help
* "Quitter" (Quit)

Human vs. human play is possible by unchecking "Ordinateur". If "Son" is checked, a sound is played when the engine moves.
Engine thinking time can be set from 1 to 25 seconds (default: 5).
A game clock is available with FMJD time controls (1 h 20 + 30 s, 90 min/45 moves, 2 h/50 moves, rapid 15 min + 5 s, blitz 5 min + 3 s):
off by default, it starts at the first move (not counted) and the player whose flag falls loses the game, the engine included.
A status bar at the top shows whose turn it is and the result, if any; below it, the engine's evaluation (in centipawns) and its best line.
Player names are editable and used in the PDN.

End of game: the program detects wins (no pieces left, or no legal move) and draws by repetition (same position three times),
by 25 king moves per player, and by few-piece endings (16 and 5 moves, FMJD rules 6.3 and 6.4). A draw by agreement, a resignation, etc.
are entered by ticking the result (1-0, 0-1, 1/2-1/2): it is added to the PDN move list.

Preferences (thinking time, names, sound, computer, clock time control, board orientation, animation speed, board colours) are kept in
`BrunoGUI_Dames.preferences.ini` next to the program. Errors and problems are logged in `BrunoGUI_Dames.log` (an older log is kept as `.log.old` beyond 1 MB).

## Building from source

You need the .NET 10 SDK on Windows. The tests check the rules (including a depth-7 perft), PDN, draws and the dialogue with the real Scan.

    dotnet build BrunoGUI_Dames.sln
    dotnet run --project BrunoGUI_Dames.csproj
    dotnet test Tests

Sample PDN games and FEN positions are in `Dames_PartiesPdn_FEN`. The Scan engine, with its sources and license, is in `Dames_Scan31_FabienLetouzey`.

The Hub2 protocol and the engine notes are in `Docs`. The version history is in `CHANGELOG.md` (in French).

Any remark or bug report is welcome (please attach `BrunoGUI_Dames.log` if it exists).
