# Historique des versions

Une section par version publiée, la plus récente en premier. Les changements qui ne sont pas encore dans une version publiée sont sous « Non publié ».

## Non publié

## 1.11 (2026-11-15)

Pendule, nouvelle mise en page, parcours de partie plus sûr, et des tests automatiques de l'interface qui ont fait corriger plusieurs défauts.

- **Pendule** avec les cadences de la FMJD (1 h 20 + 30 s, 90 min pour 45 coups, 2 h pour 50 coups, rapide 15 min + 5 s, blitz 5 min + 3 s), désactivée par défaut : choix de la cadence dans le cadre « Pendule et temps de réflexion », temps des blancs et des noirs en haut à droite, drapeau tombé = partie perdue au temps, moteur compris (son temps de réflexion est limité par ce qui lui reste). La cadence est retenue dans les préférences. La pendule décompte le camp qui joue réellement.
- **Mise en page** : le coup joué, les rafles possibles, les compteurs, les cases Ordinateur et Son, le cadre du temps de réflexion et le résultat sont au-dessus du damier, sous la ligne du moteur ; la colonne de boutons est régulièrement espacée.
- **Parcours de partie** : un coup (au damier ou par « Ordinateur joue ») n'est plus accepté tant que le parcours n'est pas revenu sur la dernière position. Avant, il s'ajoutait à la fin de l'historique sans en être la suite, et « Ordinateur joue » jouait sur une autre position que celle affichée. « Retour arrière » pendant un parcours replace le parcours sur la dernière position et affiche le coup retiré (et non le résultat) quand la partie était finie.
- **Corrections** : « Nouvelle partie » après une partie finie laissait le damier inactif ; une fin au temps pendant la réflexion du moteur réactivait des cases.
- **Tests automatiques de l'interface** : la vraie fenêtre est créée hors de l'écran et pilotée par les tests (clics, boutons, pendule à horloge simulée, fin au temps, parcours, préférences, et le vrai Scan en CI). Ils ont aussi fait corriger un défaut apparu pendant ce travail : « Préc. », « Suiv. », « Début », « Fin » et « Retour arrière » effaçaient l'historique de la partie.
- **Code** : le formulaire principal est découpé en fichiers (classe partielle) ; le sens du damier (`OrientationDamier`) et l'historique de la partie (`SessionPartie`) sont des classes testées ; style du code décrit dans `.editorconfig`, avertissements de compilation traités comme des erreurs ; Dependabot surveille les paquets NuGet et les actions GitHub ; README unique, documents du moteur dans `Docs` ; deux icônes inutilisées supprimées.
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
