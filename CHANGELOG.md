# Historique des versions

Une section par version publiée, la plus récente en premier. Les changements qui ne sont pas encore dans une version publiée sont sous « Non publié ».

## Non publié

- Style du code décrit dans `.editorconfig` ; les avertissements de compilation font échouer la compilation (`Directory.Build.props`).
- Dependabot surveille les paquets NuGet et les actions GitHub ; xunit 2.9.3, Microsoft.NET.Test.Sdk 18.12.0 et xunit.runner.visualstudio 4.0.1 (projet de tests seulement).
- README : un seul fichier (`BrunoGUI_Readme.txt` supprimé) ; documents du moteur dans `Docs`.
- Suppression de deux icônes non utilisées.
- L'historique de la partie est tenu par une classe testée (`SessionPartie`) au lieu de trois listes à synchroniser à la main. Au passage : « Retour arrière » pendant un parcours de partie replace le parcours sur la dernière position, et affiche le coup retiré (et non le résultat) quand la partie était finie.

## 1.10 (2026-11-14)

Première version numérotée depuis la 1.04, publiée sur GitHub (archive autonome : .NET et le moteur Scan sont inclus).

- Passage à .NET 10 et Krypton 95.
- Règles du jeu refaites dans une couche testée (perft jusqu'à la profondeur 7, 21 fichiers PDN d'exemple) : prise obligatoire et majoritaire, dames volantes, rafles en boucle.
- Choix entre plusieurs rafles de mêmes départ et arrivée qui prennent des pièces différentes ; le PDN écrit garde la rafle choisie.
- Fin de partie : le joueur qui ne peut plus bouger perd ; nulles par répétition (trois fois), par 25 coups de dames de chaque joueur et par les finales à peu de pièces (16 et 5 coups, règles FMJD 6.3 et 6.4).
- Lecture et écriture du PDN plus tolérantes ; une partie non rejouable ne fait plus tomber l'application ; le déroulé d'une partie chargée est plus rapide.
- Dialogue avec le moteur Scan plus sûr (demande asynchrone, arrêt propre) ; le coup du moteur est celui que Scan a joué.
- Préférences enregistrées dans `BrunoGUI_Dames.preferences.ini` (temps de réflexion, noms, son, sens et couleurs du damier, durée d'animation) ; anomalies notées dans `Journal.log`.
- Parcours de partie : le premier clic après un changement de sens (Préc. puis Suiv.) n'est plus perdu.
- Aide en français (bouton « Aide (F1) » ou touche F1).

## 1.04

Première version publiée (.NET Framework 4.8).

## 1.03 (2025-03-01)

- Analyse de position : le moteur réfléchit et donne son évaluation sans jouer le coup.

## 1.0 (2025-02-22)

- Chargement de parties PDN, cases colorées, partie jouable de bout en bout ; un double-clic sur une partie la rejoue jusqu'à la fin.
