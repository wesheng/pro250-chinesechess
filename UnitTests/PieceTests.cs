using ChineseChess;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class PieceTests
    {
        [Test]
        [TestCase(Piece.RedKnight, Piece.BlackKnight)]
        public void CanInvertPiece(Piece piece, Piece expect)
        {
            Piece inverted = piece.Invert();

            Assert.AreEqual(expect, inverted, "Piece should be inverted");
        }
    }
}