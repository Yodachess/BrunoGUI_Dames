// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
namespace BrunoGUI_Dames
{
    // La partie en cours (la seule : l'interface n'en montre qu'une à la fois) : l'historique des positions, les coups, le résultat et le parcours
    public static class Partie
    {
        public static SessionPartie Courante = new SessionPartie();
        /*    format protocole Hub2 (SessionPartie.Hub2 et Position.VersHub2)
        Le format de position est un caractère pour le côté à déplacer ('W' ou 'B') + un caractère par case dans l'ordre standard, donc 51 au total.
        Pour chaque case :       'w' : homme blanc  --  'b' : homme noir  --  'W' : roi (dame) blanc  --  'B' : roi (dame) noir  --  'e' : vide  --
        pos pos=Wbbbbbbbbbbbbbbbbbbbbbeeeeeeeeeewwwwwwwwwwwwwwwwwwww
        pos pos=BeeeWWeeeeeeeeeeeeeeeeeeeweeeeeeeeeewweBeeeeeeeeeBee
         */
    }
}
