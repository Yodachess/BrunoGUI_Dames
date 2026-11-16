# Historique des versions

Une section par version publiée, la plus récente en premier. Les changements qui ne sont pas encore dans une version publiée sont sous « Non publié ».

## Non publié

- **Couleurs** : les numéros de case sont blancs cernés de noir (lisibles sur toutes les cases, sur les pièces blanches comme sur les noires ; l'ancien cyan disparaissait sur le vert de départ) ; le fond de la fenêtre est une couleur fixe (celle que Windows 10 et 11 donnent par défaut) au lieu d'une couleur système. Les couleurs des surbrillances restent celles des versions précédentes ; seules les couleurs changées par le joueur sont écrites.
- **Interface** : nouvelles couleurs hors damier, accordées au bois (« noyer et crème ») : fond sable, bandeaux noyer à texte crème, panneaux crème, boutons espresso (plus clairs au survol) ; avant, bleu acier, or et pêche se mélangeaient. Les fenêtres secondaires (liste des coups, liste des parties, protocole, choix de la rafle) ont le même thème. Le damier et ses surbrillances ne changent pas.
- **Animations plus vives** : 250 ms par étape (900 ms avant, trois attentes par pièce prise) ; le coup du moteur glisse d'une case à l'autre au lieu d'apparaître d'un coup ; une rafle de 3 pièces passe d'environ 8 s à 1 s, et les pièces prises disparaissent ensemble à la fin. Le réglage devient `DureeEtape` dans le fichier de préférences (l'ancienne clé `DureeAnimation` est ignorée).
- **Rejeu d'une partie** : une partie PDN chargée entre tout de suite dans l'historique et le damier montre sa position de départ (plus de déroulé qui bloquait la fenêtre) ; le bouton **Lecture** du groupe « Parcours partie » la déroule avec ses animations, **Pause** l'arrête après le coup en cours, un curseur règle la vitesse (cinq niveaux, retenue dans les préférences). Pendant la lecture, les autres boutons sont inactifs et les clics sur le damier ignorés.
- Code simplifié, sans changement de comportement (vérifié par les tests de l'interface, dont les couleurs et les cadres gris des cases) : un seul chemin (`JoueCoupSurLeDamier`) pour jouer un coup au clic, par le moteur ou en rejouant un PDN, et la promotion en dame vient des règles au lieu d'être refaite à l'affichage ; une seule méthode (`TermineLaPartie`) pour terminer une partie ; la lecture des lignes du moteur est une classe testée (`LigneMoteur`) ; un seul vocabulaire de pièces (`Piece` et `Couleur`) pour les règles et l'affichage ; les trois listes de cases à décolorier sont remplacées par la reconnaissance des cadres gris ; code mort supprimé ; `PartieCourante` devient `BalisesPartie`
- **Fuite corrigée** : le curseur « pièce en main » créait à chaque sélection une icône Windows jamais détruite (un handle GDI perdu par sélection) ; elle est maintenant libérée quand la pièce est posée (test : 150 sélections, plus aucune fuite).
- Fenêtre du protocole : un espace sépare « [SCAN 3.1] » des données (comme « [BrunoGUI_Dames] »).
- Deux libellés du moteur corrigés (« SCNA » devenu « SCAN », double deux-points de « Coup suggéré par l'analyse »).

## 1.11 (2026-11-15)

Pendule, nouvelle mise en page, parcours de partie plus sûr, et des tests automatiques de l'interface qui ont fait corriger plusieurs défauts.

- **Pendule** avec les cadences de la FMJD (1 h 20 + 30 s, 90 min pour 45 coups, 2 h pour 50 coups, rapide 15 min + 5 s, blitz 5 min + 3 s), désactivée par défaut : choix de la cadence dans le cadre « Pendule et temps de réflexion », temps des blancs et des noirs en haut à droite, drapeau tombé = partie perdue au temps, moteur compris (son temps de réflexion est limité par ce qui lui reste). La cadence est retenue dans les préférences. La pendule décompte le camp qui joue réellement.
- **Mise en page** : le coup joué, les rafles possibles, les compteurs, les cases Ordinateur et Son, le cadre du temps de réflexion et le résultat sont au-dessus du damier, sous la ligne du moteur ; la colonne de boutons est régulièrement espacée.
- **Parcours de partie** : un coup (au damier ou par « Ordinateur joue ») n'est plus accepté tant que le parcours n'est pas revenu sur la dernière position. Avant, il s'ajoutait à la fin de l'historique sans en être la suite, et « Ordinateur joue » jouait sur une autre position que celle affichée. « Retour arrière » pendant un parcours replace le parcours sur la dernière position et affiche le coup retiré (et non le résultat) quand la partie était finie.
- **Corrections** : « Nouvelle partie » après une partie finie laissait le damier inactif ; une fin au temps pendant la réflexion du moteur réactivait des cases.
- **Tests automatiques de l'interface** : la vraie fenêtre est créée hors de l'écran et pilotée par les tests (clics, boutons, pendule à horloge simulée, fin au temps, parcours, préférences, et le moteur Scan en CI). Ils ont aussi fait corriger un défaut apparu pendant ce travail : « Préc. », « Suiv. », « Début », « Fin » et « Retour arrière » effaçaient l'historique de la partie.
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
