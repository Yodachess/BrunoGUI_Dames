// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
using System.Collections.Generic;

namespace BrunoGUI_Dames
{
    // La partie en cours : les positions successives (l'indice 0 est la position de départ) et les coups joués, dans trois formats
    public static class Partie
    {
        public static List<string> ListeCoupsFen = new List<string>();      // https://pdn.fmjd.org/fen.html (position après chaque demi-coup, enrobée : [FEN "W:W31-50:B1-20"])
        public static List<string> ListeCoupsPdn = new List<string>();      // https://pdn.fmjd.org/index.html (« 28x19 » ; le résultat est ajouté à la fin avec un espace devant)
        public static List<string> ListeCoupsHub2 = new List<string>();     // format protocole Hub2 (position après chaque demi-coup)
        /*    format protocole Hub2
        Le format de position est un caractère pour le côté à déplacer ('W' ou 'B') + un caractère par case dans l'ordre standard, donc 51 au total.
        Pour chaque case :       'w' : homme blanc  --  'b' : homme noir  --  'W' : roi (dame) blanc  --  'B' : roi (dame) noir  --  'e' : vide  --
        pos pos=Wbbbbbbbbbbbbbbbbbbbbbeeeeeeeeeewwwwwwwwwwwwwwwwwwww
        pos pos=BeeeWWeeeeeeeeeeeeeeeeeeeweeeeeeeeeewweBeeeeeeeeeBee
         */

        // --- La position Hub2 (celle qu'attend Scan) d'une position FEN, enrobée ou non ---
        public static string Hub2DepuisFen(string fen)
        {
            return Position.DepuisFen(fen).VersHub2();
        }
    }
}