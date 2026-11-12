// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Les vérifications de Scenario.cs vues comme des tests xUnit : un test par vérification, avec son nom, dans l'Explorateur de
// tests de Visual Studio (Test > Explorateur de tests) ou avec "dotnet test Tests". Le scénario s'exécute UNE fois, puis chaque
// test lit son résultat.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class VerificationsTests
{
    private static readonly Lazy<List<Verification>> Resultats = new(() => Scenario.Executer(complet: false));

    // Numéro (deux vérifications peuvent avoir le même nom) et nom de chaque vérification
    public static IEnumerable<object[]> Verifications() => Resultats.Value.Select((v, numero) => new object[] { numero + 1, v.Nom });

    [Theory]
    [MemberData(nameof(Verifications))]
    public void Verification(int numero, string nom)
    {
        Verification resultat = Resultats.Value[numero - 1];
        Assert.True(resultat.Ok, $"{nom} -> {resultat.Detail}");
    }

    [Fact]
    public void ToutesLesVerificationsOntEteFaites()
    {   // garde-fou : un scénario qui s'arrêterait en route (exception) ne doit pas passer pour un succès
        Assert.True(Resultats.Value.Count > 40, $"seulement {Resultats.Value.Count} vérifications");
    }
}
