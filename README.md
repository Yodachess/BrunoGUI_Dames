BrunoGUI_dames V1.xx Copyright 2024-2025 Bruno Courtois

Ce projet est une interface graphique sous Windows permettant de jouer aux dames internationales (10x10).
C'est une application Winforms développé en langage C# 7.3 sous .NET 4.7 avec Visual Studio 2022.
(Mis à jour en C# 12.0 sous .NET 8.0 en 2025))

Connaissant la qualité des logiciels de Fabien Letouzey (Fruit, Seipan, ...), je me suis tourné pour le 
moteur vers son logiciel de dames SCAN dans sa version 3.1 (la dernière à ma conaissance), qui utilise 
un protocole hub2 similaire au protocole UCI des echecs (github.com/rhalbersma/scan ).

Mon projet comporte un damier complet prêt à jouer au lancement (utilisation et affichage notation Manoury), 
l'utilisateur clique sur le pion qu'il veut jouer et l'application lui montre les déplacements possibles (s'il y en a)
Une série de boutons sur la droite permet de sélectionner des options, sauf pendant la réflexion du moteur.

Boutons disponibles :
* "Nouvelle Partie" : démarre une nouvelle partie
* "Ordinateur joue" : lance la réflexion du moteur et joue son coup sur le damier
* "Retour Arrière" : recule la partie d'un demi coup
* "Tourner damier" : fait pivoter le damier de 180°, par défaut vue côté blancs 
* "Analyse position" : lance la réflexion du moteur et donne le résultat SANS jouer le coup sur le damier
* "Liste des coups" : Affiche la liste des coups joués, aux formats Pdn, FEN et Hub2
* "Parcours partie" : boutons permettant de parcourir la partie 
* "Charger parties" : lecture de fichiers au format Pdn et affichage de la liste des partie(s) 
* "Masquer partie" : bouton cachant/affichant la liste des partie(s) 
* "Sauve partie" : Sauvegarde de la partie au format Pdn, peut être ajouté à la fin d'un fichier existant
* "Charge position" : lecture de fichier au format FEN et mise en place de la position 
* "Sauve position" : Sauvegarde de la position au format FEN
* "Protocole" : affiche les échanges protocole hub2 entre l'interface graphique et le moteur
* "A propos" : Version ddu logiciel et du moteur
* "Quitter"

Il est possible de jouer entre humains, en décochant le bouton "Ordinateur"
Si le bouton "Son" est coché, un son est émis lorsque le moteur a joué
Il est possible de définir la fin/résultat de la partie (cocher 1-0, 0-1 ou 1/2-1/2), 
la liste de coups Pdn sera mis à jour avec le résultat.
Le temps de réflexion du moteur est réglable de 1 à 25 secondes (défaut = 5 secondes)

Un bandeau en haut de fenêtre indique le statut (trait aux blanc, trait aux noirs, gain éventuel, ...)
Sous le bandeau, le score estimé (en centipions) et la ligne du moteur sont affichés
Les noms des joueurs peuvent être édités et sont mis à jour dans la liste de coups Pdn.

Améliorations possibles :
* robustesse de l'expérience utilisateur (quelques bugs encore possible  ...)
* Mise en place de position pour étudier des problèmes (déjà possible avec format FEN)

Toute remarque ou signalement de bug est bienvenue

English version :

BrunoGUI_dames V1.xx - Copyright 2024-2025 Bruno Courtois
This project is a Windows graphical interface for playing international draughts (10x10).
It is a WinForms application developed in C# 7.3 under .NET 4.7 using Visual Studio 2022.
(Updated to C# 12.0 under .NET 8.0 in 2025))

Knowing the quality of Fabien Letouzey's software (Fruit, Seipan, ...), 
I chose to use his draughts engine SCAN 3.1 (the latest version to my knowledge).
SCAN uses the Hub2 protocol, which is similar to the UCI protocol in chess (github.com/rhalbersma/scan).

Features
The application starts with a fully set up draughts board (using Manoury notation for display).
The player clicks on a piece, and the application highlights possible moves (if any).
A series of buttons on the right allows selecting various options, except during the engine's thinking time.

Available buttons:
"New Game": Starts a new game.
"Computer plays": Starts the engine's calculation and plays the move on the board.
"Undo move": Reverts the game by half a move.
"Rotate board": Rotates the board 180° (default: White's perspective).
"Analyze position": Runs the engine’s calculation but does not play the move on the board.
"Move list": Displays the move history in Pdn, FEN, and Hub2 formats.
"Game navigation": Buttons to navigate through the game.
"Load games": Reads Pdn files and displays the list of available games.
"Hide games": Hides or shows the list of games.
"Save game": Saves the game in Pdn format, with the option to append it to an existing file.
"Load position": Reads a FEN file and sets up the board position.
"Save position": Saves the current board position in FEN format.
"Protocol": Displays Hub2 protocol exchanges between the GUI and the engine.
"About": Shows the software and engine version.
"Quit"

Additional Features
Human vs. Human play is possible by unchecking the "Computer plays" button.
If the "Sound" button is checked, a sound is played when the engine moves.
You can manually set the game result (1-0, 0-1, or ½-½). The result is updated in the Pdn move list.
Engine thinking time can be adjusted between 1 and 25 seconds (default: 5 seconds).
A status bar at the top indicates whose turn it is (White or Black), possible wins, etc.
Below the status bar, the engine's evaluation (in centipawns) and PV (best line) are displayed.
Player names are editable and updated in the Pdn move list.

Possible improvements:
Better user experience & stability (some bugs may still exist...).
Position setup for problem-solving (already possible using FEN format).

Any remark or bug input is welcome
