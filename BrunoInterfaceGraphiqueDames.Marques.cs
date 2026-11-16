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
using System.Windows.Forms;

namespace BrunoGUI_Dames
{
    public partial class BrunoInterfaceGraphiqueDames
    {
        // Les repères dessinés par-dessus les cases (par PictureBox_Paint), qui ne dépendent pas de la couleur de fond des cases : un repère sur les deux cases du dernier coup (RepereDernierCoup : un triangle dans un coin, un cadre, ou rien),
        // et le numéro d'ordre des étapes d'une rafle. Ils ne changent pas les couleurs du damier.
        private static readonly Color CouleurDernierCoup = Color.FromArgb(0x4A, 0x2A, 0x12);       // Repère du dernier coup : brun foncé (choisi le 2026-11-16 ; un bleu puis un jaune vif avaient été essayés)
        private static readonly Color CouleurBordDernierCoup = Color.FromArgb(0xFF, 0xF6, 0xE3);    // Son liseré : crème, pour qu'il se lise aussi sur les pièces noires et les cases foncées
        private int dernierDepartManoury;                        // Les cases du dernier coup (0 : aucun)
        private int dernierArriveeManoury;
        private readonly Dictionary<int, string> numerosEtape = new Dictionary<int, string>();     // Numéro d'ordre des cases d'arrivée d'une rafle : « 1 », « 2 » (« 1,3 » si elle repasse sur une case)

        // --- Le dernier coup (0, 0 : aucun) ---
        private void MarqueDernierCoup(int depart, int arrivee)
        {
            dernierDepartManoury = depart;
            dernierArriveeManoury = arrivee;
            RafraichitDamier();
        }
        // --- Le dernier coup de la position affichée par le parcours de la partie (aucun au départ) : appelée après la mise en place d'une position ---
        private void MarqueDernierCoupAffiche()
        {
            SessionPartie session = Session;
            int i = session.IndexAffiche;
            Coup? coup = i > 0 && i - 1 < session.CoupsPdn.Count ? ReglesDames.TrouverCoupPdn(session.Positions[i - 1], session.CoupsPdn[i - 1]) : null;
            MarqueDernierCoup(coup?.Depart ?? 0, coup?.Arrivee ?? 0);
        }
        // --- Les numéros d'ordre des étapes d'une rafle ; une rafle d'une seule étape n'en a pas besoin ---
        private void MarqueEtapes(Coup rafle)
        {
            numerosEtape.Clear();
            if (rafle.Chemin.Count >= 2)
                for (int i = 0; i < rafle.Chemin.Count; i++)
                    numerosEtape[rafle.Chemin[i]] = numerosEtape.TryGetValue(rafle.Chemin[i], out string? deja) ? deja + "," + (i + 1) : (i + 1).ToString();
            RafraichitDamier();
        }
        // --- Efface les numéros d'étapes (pas le dernier coup) ---
        private void EffaceNumerosEtape()
        {
            if (numerosEtape.Count == 0)
                return;
            numerosEtape.Clear();
            RafraichitDamier();
        }
        private void RafraichitDamier()
        {
            foreach (PictureBox c in CaseDamier)
                c.Invalidate();
        }

        // --- Dessine les repères de la case Manoury numero (taille largeur × hauteur) par-dessus son image ---
        private void DessineMarques(Graphics g, int numero, int largeur, int hauteur)
        {
            SmoothingMode ancien = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (numero == dernierDepartManoury || numero == dernierArriveeManoury)
            {
                if (RepereDernierCoup == "cadre")
                    using (Pen cadre = new Pen(CouleurDernierCoup, 4f))
                        g.DrawRectangle(cadre, 2, 2, largeur - 5, hauteur - 5);
                else if (RepereDernierCoup == "coin")
                {   // Un triangle brun foncé cerné de crème dans le coin en bas à gauche (le numéro de la case est en haut à droite) : il ne ressemble pas aux cadres des destinations
                    PointF[] triangle = { new PointF(1, hauteur - 1), new PointF(1, hauteur - 1 - 22), new PointF(1 + 22, hauteur - 1) };
                    using (SolidBrush fond = new SolidBrush(CouleurDernierCoup))
                        g.FillPolygon(fond, triangle);
                    using (Pen bord = new Pen(CouleurBordDernierCoup, 2f) { LineJoin = LineJoin.Round })
                        g.DrawPolygon(bord, triangle);
                }
            }
            if (numerosEtape.TryGetValue(numero, out string? texte))
            {   // Le numéro d'ordre dans une pastille foncée, au centre de la case
                using (Font police = new Font("Arial", 9f, FontStyle.Bold))
                {
                    SizeF taille = g.MeasureString(texte, police);
                    float diametre = Math.Max(22f, taille.Width + 8f);
                    RectangleF pastille = new RectangleF((largeur - diametre) / 2f, (hauteur - 22f) / 2f, diametre, 22f);
                    using (SolidBrush fond = new SolidBrush(ThemeInterface.Bouton))
                        g.FillEllipse(fond, pastille);
                    using (Pen bord = new Pen(Color.White, 1.5f))
                        g.DrawEllipse(bord, pastille);
                    using (StringFormat centre = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(texte, police, Brushes.White, pastille, centre);
                }
            }
            g.SmoothingMode = ancien;
        }
    }
}
