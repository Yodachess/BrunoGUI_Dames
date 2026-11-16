// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Dames - Interface graphique du jeu de Dames en C# WinForms    █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// █ Use of SCAN 3.1 engine from Fabien Letouzey via Hub2's protocol        █
// █ Scan is available at : github.com/rhalbersma/scan                      █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    public partial class BrunoInterfaceGraphiqueDames
    {
        // Le glissement d'une pièce d'une case à l'autre (le coup du moteur, le rejeu d'une partie, les étapes d'une rafle) : une image de pièce, de la forme de la pièce,
        // se déplace au-dessus des cases pendant DureeEtape millisecondes. Les cases ne sont touchées qu'au début (la pièce quitte son départ) et à la fin (elle arrive).
        private PictureBox? glisseur;
        private readonly Dictionary<Piece, (Bitmap image, Region forme)> formesDesPieces = new Dictionary<Piece, (Bitmap, Region)>();

        // --- L'image de la pièce à la taille d'une case et sa silhouette (les points assez opaques), calculées une fois par type de pièce ---
        private (Bitmap image, Region forme) FormeDeLaPiece(Piece piece)
        {
            if (formesDesPieces.TryGetValue(piece, out (Bitmap image, Region forme) connue))
                return connue;
            const int cote = 60;                        // La taille des cases du damier
            Bitmap image = new Bitmap(cote, cote, PixelFormat.Format32bppArgb);
            using (Graphics dessin = Graphics.FromImage(image))
            {
                dessin.InterpolationMode = InterpolationMode.HighQualityBicubic;
                dessin.DrawImage(ListeBitmapsPiece[piece], 0, 0, cote, cote);       // Étirée comme dans les cases (SizeMode StretchImage)
            }
            Region forme = new Region();
            forme.MakeEmpty();
            for (int y = 0; y < cote; y++)
            {
                int x = 0;
                while (x < cote)
                {
                    while (x < cote && image.GetPixel(x, y).A < 128) x++;          // Saute les points transparents
                    int debut = x;
                    while (x < cote && image.GetPixel(x, y).A >= 128) x++;         // Une série de points opaques
                    if (x > debut)
                        forme.Union(new Rectangle(debut, y, x - debut, 1));
                }
            }
            (Bitmap image, Region forme) nouvelle = (image, forme);
            formesDesPieces[piece] = nouvelle;
            return nouvelle;
        }

        // --- Fait glisser la pièce de la case d'indice « indexDepart » à celle d'indice « indexArrivee » en « ms » millisecondes (avec une accélération au départ et un
        // ralentissement à l'arrivée) : la pièce quitte son départ au début et est dessinée à l'arrivée à la fin. Sans durée (0), elle saute simplement d'une case à l'autre ---
        private async Task GlisseUnePiece(int indexDepart, int indexArrivee, Piece piece, int ms)
        {
            if (ms <= 0)
            {
                EffacePiece(indexDepart);
                DessinePiece(indexArrivee, piece);
                return;
            }
            (Bitmap image, Region forme) = FormeDeLaPiece(piece);
            if (glisseur == null)
            {
                glisseur = new PictureBox { Enabled = false, Size = new Size(60, 60), Visible = false };      // Désactivé : il ne reçoit jamais de clic
                Damier10x10.Controls.Add(glisseur);
            }
            Point depart = CaseDamier[indexDepart].Location;
            Point arrivee = CaseDamier[indexArrivee].Location;
            glisseur.Image = image;
            glisseur.Region = forme.Clone();
            glisseur.Location = depart;
            glisseur.Visible = true;
            glisseur.BringToFront();
            EffacePiece(indexDepart);           // La pièce est maintenant dans le glisseur
            int etapes = Math.Max(2, ms / 16);
            int attente = Math.Max(1, ms / etapes);
            for (int k = 1; k <= etapes; k++)
            {
                double t = k / (double)etapes;
                t = t * t * (3 - 2 * t);        // Accélère puis ralentit
                glisseur.Location = new Point(depart.X + (int)Math.Round((arrivee.X - depart.X) * t), depart.Y + (int)Math.Round((arrivee.Y - depart.Y) * t));
                await Task.Delay(attente);
            }
            DessinePiece(indexArrivee, piece);
            glisseur.Visible = false;
        }
    }
}
