/*
Scan 3.1 Copyright (C) 2015-2019 Fabien Letouzey.
Ce programme est distribué sous la licence publique générale GNU version 3.
Voir license.txt pour plus de détails.

---

Nous sommes le 06/07/2019.
Scan est un moteur de jeu de dames international (10x10) qui utilise le protocole DamExchange (DXP) ou le mode texte. Le nom "Scan" vient du balayage en évaluation qui "divise" l'échiquier en 8 rectangles superposés (2-26, 3-27, ..., 25-49) pour juger les positions. Profitez de Scan !

Merci à Harm Jetten pour son aide concernant la compatibilité et la compilation Windows, les tests, l'hébergement, etc. (vous l'aurez compris, il l'a fait) ... Son moteur, Moby Dam, est également multiplateforme et open source !

Merci à Rein Halbersma pour son expertise dans les règles et la mise en œuvre du jeu de dames.

Merci à RoepStoep et BumperBalloonCars pour lidraughts.org !

Salutations aux autres programmeurs de jeux ; Gens una sumus.

Fabien Letouzey (fabien_letouzey@hotmail.com).

---

Exécution de Scan

Dans la terminologie Windows, Scan est une « application console » (pas de graphiques). Le mode texte est le mode par défaut ; un mode DXP est également disponible avec un argument de ligne de commande : « scan dxp ». Scan a besoin du fichier de configuration « scan.ini » (décrit ci-dessous) et des fichiers de données dans le répertoire « data » (livre d'ouverture, poids d'évaluation et bases de données). Notez que, en raison de leur taille, les bases de données nécessitent une copie séparée (à partir d'une version précédente de Scan) ou un téléchargement pour l'installation.

La plupart des commandes en mode texte se composent d'une seule lettre (minuscule) :

0-2 -> nombre de joueurs de l'ordinateur (par exemple 2 = jeu automatique)
(g)o -> faire jouer l'ordinateur de votre côté
(u)ndo -> reprendre un pli
(r)edo -> rejouer un précédent retour en arrière, si aucun autre coup n'a été joué

time <n> -> limite de temps fixe ; 10 s par défaut

(h)elp -> trouver quelques autres commandes

Et bien sûr, vous pouvez saisir un coup en notation standard. Le simple fait d'appuyer sur Entrée peut être utilisé pour les coups forcés.

Une note sur les scores. +/- 89.xx signifie atteindre bientôt une fin de partie gagnante/perdante. +/- 99.xx signifie atteindre bientôt la fin absolue de la partie.

Scan dispose également d'un mode Hub avec un nouveau protocole : " scan hub ", qui est utilisé par l'interface graphique Hub (téléchargement séparé). Les programmeurs peuvent l'utiliser pour contrôler Scan de manière automatisée ; la description du protocole se trouve dans " protocol.txt ".

---

Configuration

Vous pouvez éditer le fichier texte "scan.ini" pour changer les paramètres ; dans ce cas, vous devez relancer Scan. Voici les paramètres.

variante : sélectionne les règles à appliquer. "normal" pour les draughts internationales. Cependant, de nombreux matchs nuls se produisent avec ces règles, même avec des adversaires un peu plus faibles. "killer" (Killer draughts) et "bt" (breakthrough draughts : le premier joueur qui fait un roi gagne) sont des tentatives pour rendre le jeu plus intéressant à haut niveau. Scan devrait être très fort dans les draughts Killer et les règles "normales" ne sont en fait prises en charge que comme une fonctionnalité héritée (désolé pour les fans). En revanche, la prise en charge de BT est expérimentale et n'a pas été bien testée. IMPORTANT : changer les règles n'a de sens que si les deux joueurs en sont conscients (tout comme les échecs contre les draughts).

NOUVELLES variantes : "frison" et "losing" (aka antidraughts/giveaway/suicide). Pour jouer graphiquement aux dames frisonnes, vous aurez besoin de Hub 2.1 (téléchargement séparé) ; pour les autres variantes, la mise à niveau n'est pas nécessaire. Tout comme pour BT, la perte de prise en charge des dames est expérimentale et n'est pas bien testée.

book, book-ply, book-margin : vous pouvez (dés)activer le livre d'ouverture ici. Le caractère aléatoire ne sera appliqué qu'aux premiers plis "book-ply" (demi-coups) ; les coups suivants seront toujours les meilleurs. J'ai utilisé "book-ply = 4" pendant les Olympiades informatiques. "book-margin" agit comme un facteur de hasard, par exemple : 0 = meilleur coup (pour les tournois avec des positions d'ouverture présélectionnées), 1 = petit hasard (pour les parties sérieuses), 4 = assez aléatoire (pour les parties occasionnelles). Notez que les coups tout aussi bons sont toujours choisis au hasard, même après les premiers coups "book-ply". NOUVEAU : pour les dames frisonnes, je recommande des valeurs plus élevées pour le caractère aléatoire du livre ; peut-être "book-ply = 10" et "book-margin = 10". Si cela ne suffit pas, vous pouvez essayer des valeurs plus grandes.

threads : combien de cœurs utiliser pour la recherche (SMP). Évitez l'hyper-threading (non testé).

tt-size : le nombre d'entrées dans la table de transposition sera de 2 ^ tt-size. Chaque entrée occupe 16 octets, donc tt-size = 26 correspond à 1 Gio ; c'est ce que j'ai utilisé pendant l'Olympiade informatique. Utilisez des valeurs plus petites pour les parties rapides. Chaque fois que vous l'augmentez d'un, la taille de la table doublera.

bb-size : utilisez des bases de bits de fin de partie (victoire/perte/match nul uniquement) jusqu'à des pièces de "taille bb" (0 = pas de bases de bits). Si vous voulez une force maximale, utilisez 6 (7 pour la variante BT, 5 pour les dames frisonnes). Cela prendra cependant environ 2 Gio de RAM. Si Scan prend trop de temps à s'initialiser ou prend trop de mémoire, sélectionnez 5. Notez que les bases de données nécessitent une copie séparée (des versions précédentes de Scan) ou un téléchargement pour l'installation dans le répertoire « data ».

Les autres options sont toutes liées au protocole DamExchange (DXP) et sont les mêmes que dans les versions précédentes de Scan

dxp-server : pour que deux programmes puissent communiquer, l'un doit être le serveur et l'autre le client (« appelant » pour utiliser une analogie téléphonique).

dxp-host & dxp-port : dxp-host est l'adresse IP (sous forme numérique telle que 127.0.0.1) du serveur auquel se connecter (en mode client). Elle n'a aucun effet en mode serveur. dxp-port affecte les deux modes.

dxp-initiator : en plus du mode client/serveur, un programme doit démarrer les jeux (initiateur) et l'autre ne répond qu'aux requêtes (suiveur). Le mode initiateur de Scan est très basique. Il lancera une partie infinie à partir de la position de départ, en changeant de côté après chaque partie. Il est probable que d'autres programmes disposent d'un mode initiateur plus avancé et que vous devriez l'utiliser lorsque cela est possible.

dxp-time & dxp-moves : contrôle du temps (uniquement pour l'initiateur). Le temps est en minutes. 0 coup indique qu'il n'y a aucune limite de coups : le jeu sera joué jusqu'au bout (non recommandé).

dxp-board & dxp-search : si Scan doit afficher le plateau et/ou rechercher des informations après chaque coup. En définissant les deux sur true, vous pouvez suivre les parties en mode texte. Avec les deux définis sur false, Scan est plus silencieux.

---

Compilation

Le code source utilise C++14 et devrait être principalement multiplateforme. J'ai fourni le Makefile Clang que j'utilise sur Mac ; il est compatible avec Linux et GCC. Le code source est également connu pour fonctionner avec Visual Studio.

---

Historique

2015-04-10, version 1.0 (version privée)

2015-07-19, version 2.0
- ajout du livre d'ouverture
- ajout des tables de fin de partie (6 pièces)
- ajout du LMR (plus d'élagage)
- ajout de la recherche parallèle
- ajout de la phase de jeu dans l'évaluation
- ajout de la génération de mouvements de bitboard
- ajout de DXP

2017-07-11, version 3.0
- ajout des variantes Killer et BT
- amélioration de l'évaluation
- amélioration du QS (positions de capture de l'adversaire)
- amélioration de la vitesse
- amélioration du sondage de la base de bits (continuer à rechercher une victoire exacte après une victoire BB)
- amélioration du protocole Hub (voir protocol.txt)
- nettoyage du code (types plus stricts et classes de position immuables)

2019-07-06, version 3.1
- ajout des variantes frisonnes et perdantes
- modification du format du fichier d'évaluation (mais pas du contenu)
- amélioration de la recherche (aspiration fenêtres, extensions singulières)
- gestion du temps simplifiée
- ajout d'une limite de nœuds facultative
- chargement accéléré de la base de bits
- autorisé plus de 20 pièces par côté pour les compositions (non testé)
- code nettoyé (itérateurs de bitboard et modifications mineures)


*/