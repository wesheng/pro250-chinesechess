using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseChess
{
    // 帅/将 = 1
    // 兵/卒 = 2
    // 仕/士 = 3
    // 炮/砲 = 4
    // 相/象 = 5
    // 车/车 = 6
    // 马/马 = 7
    public enum Piece
    {
        RedKing     = -1, BlackKing     = 1,
        RedPawn     = -2, BlackPawn     = 2,
        RedGuard    = -3, BlackGuard    = 3,
        RedCannon   = -4, BlackCannon   = 4,
        RedElephant = -5, BlackElephant = 5,
        RedRook     = -6, BlackRook     = 6,
        RedKnight   = -7, BlackKnight   = 7,
        None        = 0
    }

    public static class PieceExt
    {
        public static Piece Invert(this Piece p)
        {
            return (Piece) (-(int) p);
        }
    }
}
