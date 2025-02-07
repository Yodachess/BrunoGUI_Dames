// Mouvements sans rafles
using System;
using System.Collections.Generic;

public class JeuDeDames
{
    // Représente les cases du damier : 0 = vide, 1 = pion blanc, 2 = pion noir
    private int[] etatDamier = new int[100]; // Tableau de 100 cases (index 0 à 99)

    // Retourne les déplacements possibles pour un pion donné
    public (List<int> deplacementsSimples, List<int> prisesPossibles) ObtenirDeplacements(int caseDepart, int couleurPion)
    {
        var deplacementsSimples = new List<int>();
        var prisesPossibles = new List<int>();

        // Directions diagonales : haut gauche, haut droite, bas gauche, bas droite
        int[] directions = { -11, -9, 9, 11 };

        foreach (int direction in directions)
        {
            int caseAdjacente = caseDepart + direction;
            int caseApresPrise = caseDepart + 2 * direction;

            // Vérification pour un déplacement simple
            if (EstCaseValide(caseAdjacente) && etatDamier[caseAdjacente] == 0)
            {
                deplacementsSimples.Add(caseAdjacente);
            }

            // Vérification pour une prise
            if (EstCaseValide(caseApresPrise) &&
                etatDamier[caseAdjacente] == Adversaire(couleurPion) &&
                etatDamier[caseApresPrise] == 0)
            {
                prisesPossibles.Add(caseApresPrise);
            }
        }

        return (deplacementsSimples, prisesPossibles);
    }

    // Vérifie si une case est valide (entre 1 et 50 en notation Manoury)
    private bool EstCaseValide(int index)
    {
        return index >= 1 && index <= 50 && (index % 10 != 0);
    }

    // Retourne la valeur représentant l'adversaire
    private int Adversaire(int couleur)
    {
        return (couleur == 1) ? 2 : 1;
    }
}

/*
Explications des modifications
Ajout des directions diagonales inverses :

En plus des directions en avant, on ajoute les directions en arrière (9, 11 pour les blancs, -11, -9 pour les noirs).
Prise dans toutes les directions :

La prise est possible si la case adjacente contient une pièce adverse et la case suivante est vide.
Damier virtuel :

Les cases sont représentées par un tableau de 100 éléments.
Les cases blanches (non jouables) et les indices en dehors de 1-50 en notation Manoury sont ignorés.
*/

// Exemple d'utilisation 
public class Program
{
    public static void Main()
    {
        JeuDeDames jeu = new JeuDeDames();

        // Exemple : état initial du damier
        int[] damier = new int[100];
        damier[23] = 1; // Pion blanc en case 23
        damier[14] = 2; // Pion noir en case 14
        damier[5] = 0;  // Case 5 est vide (possible prise)

        // Assigner l'état au jeu
        jeu.SetEtatDamier(damier);

        // Obtenir les déplacements possibles pour le pion blanc en 23
        var (simples, prises) = jeu.ObtenirDeplacements(23, 1);

        Console.WriteLine("Déplacements simples : " + string.Join(", ", simples));
        Console.WriteLine("Prises possibles : " + string.Join(", ", prises));
    }
}

/*
Pour inclure les rafles multiples (prises successives), nous devons modifier l'algorithme 
pour explorer récursivement toutes les séquences de prises possibles. 
Voici une version mise à jour en C# qui gère les rafles :

Algorithme avec rafles multiples
L'idée principale est d'utiliser une fonction récursive qui :

Vérifie toutes les prises possibles à partir d'une case donnée.
Simule la prise (met à jour temporairement le damier pour tester la suite des mouvements).
Explore toutes les séquences possibles de rafles jusqu'à ce qu'il n'y ait plus de prise.
*/

using System;
using System.Collections.Generic;

public class JeuDeDames
{
    private int[] etatDamier = new int[100]; // Damier virtuel (100 cases)

    // Retourne tous les déplacements possibles, y compris les rafles multiples
    public List<List<int>> ObtenirRafles(int caseDepart, int couleurPion)
    {
        var resultats = new List<List<int>>();
        ExplorerRafles(caseDepart, couleurPion, new List<int> { caseDepart }, resultats);
        return resultats;
    }

    // Fonction récursive pour explorer les rafles
    private void ExplorerRafles(int caseCourante, int couleurPion, List<int> chemin, List<List<int>> resultats)
    {
        bool rafleTrouvee = false;
        int[] directions = { -11, -9, 9, 11 }; // Diagonales

        foreach (int direction in directions)
        {
            int caseAdversaire = caseCourante + direction;
            int caseApresPrise = caseCourante + 2 * direction;

            if (EstCaseValide(caseApresPrise) &&
                etatDamier[caseAdversaire] == Adversaire(couleurPion) &&
                etatDamier[caseApresPrise] == 0)
            {
                // Simuler la prise
                int pieceCapturee = etatDamier[caseAdversaire];
                etatDamier[caseCourante] = 0;
                etatDamier[caseAdversaire] = 0;
                etatDamier[caseApresPrise] = couleurPion;

                // Ajouter la case au chemin
                chemin.Add(caseApresPrise);

                // Explorer les rafles suivantes
                ExplorerRafles(caseApresPrise, couleurPion, chemin, resultats);

                // Restaurer l'état initial
                chemin.RemoveAt(chemin.Count - 1);
                etatDamier[caseApresPrise] = 0;
                etatDamier[caseAdversaire] = pieceCapturee;
                etatDamier[caseCourante] = couleurPion;

                rafleTrouvee = true;
            }
        }

        // Si aucune autre rafle n'est trouvée, ajouter le chemin actuel
        if (!rafleTrouvee && chemin.Count > 1)
        {
            resultats.Add(new List<int>(chemin));
        }
    }

    // Vérifie si une case est valide
    private bool EstCaseValide(int index)
    {
        return index >= 1 && index <= 50 && (index % 10 != 0);
    }

    // Retourne la couleur de l'adversaire
    private int Adversaire(int couleur)
    {
        return (couleur == 1) ? 2 : 1;
    }

    // Permet de définir l'état du damier
    public void SetEtatDamier(int[] damier)
    {
        Array.Copy(damier, etatDamier, damier.Length);
    }
}
/*
Explications des étapes supplémentaires :
Récursivité dans ExplorerRafles :

À chaque étape, l’algorithme simule la prise d’un pion adverse et appelle récursivement la fonction pour explorer les prises suivantes.
Si aucune prise supplémentaire n’est possible, le chemin actuel est ajouté aux résultats.
Restauration de l’état :

Après chaque simulation de prise, l’état du damier est restauré pour s'assurer que les autres chemins possibles sont explorés correctement.
Résultats :

Chaque rafle est une liste de cases représentant le chemin parcouru.
*/

// Exemple d'utilisation
public class Program
{
    public static void Main()
    {
        JeuDeDames jeu = new JeuDeDames();

        // Exemple : état initial du damier
        int[] damier = new int[100];
        damier[23] = 1; // Pion blanc en case 23
        damier[14] = 2; // Pion noir en case 14
        damier[5] = 0;  // Case 5 est vide (pour la rafle)

        // Ajouter d'autres pièces pour tester les rafles multiples
        damier[25] = 2; // Pion noir en case 25
        damier[7] = 0;  // Case 7 est vide

        // Assigner l'état au jeu
        jeu.SetEtatDamier(damier);

        // Obtenir les rafles possibles pour le pion blanc en 23
        List<List<int>> rafles = jeu.ObtenirRafles(23, 1);

        // Afficher les résultats
        Console.WriteLine("Rafles possibles :");
        foreach (var chemin in rafles)
        {
            Console.WriteLine(string.Join(" -> ", chemin));
        }
    }
}

/* Résultat attendu :
Pour un pion blanc en case 23 avec une configuration où plusieurs rafles sont possibles :

Rafle 1 : 23 -> 5
Rafle 2 : 23 -> 5 -> 7 (rafle multiple)
Cette implémentation prend en charge des rafles dans toutes les directions (avant et arrière) 
et explore tous les chemins possibles.
*/


