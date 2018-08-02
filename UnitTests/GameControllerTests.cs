using System;
using System.Collections.Generic;
using NUnit.Framework;
using ChineseChess;

namespace UnitTests
{
    [TestFixture]
    public class GameControllerTests
    {
        private GameController gameController;

        const int Row = 10;
        const int Col = 9;

        private Piece[][] traditionalBoard = new Piece[Row][]
        {
            new Piece[Col]
            {
                Piece.RedRook, Piece.RedKnight,
                Piece.RedElephant, Piece.RedGuard,
                Piece.RedKing, Piece.RedGuard,
                Piece.RedElephant, Piece.RedKnight,
                Piece.RedRook
            },
            // 1
            new Piece[Col]
            {
                Piece.None, Piece.None, Piece.None,
                Piece.None, Piece.None, Piece.None,
                Piece.None, Piece.None, Piece.None
            }, 
            // 2
            new Piece[Col]
            {
                Piece.None, Piece.RedCannon, Piece.None,
                Piece.None, Piece.None, Piece.None,
                Piece.None, Piece.RedCannon, Piece.None
            }, 
            // 3
            new Piece[Col]
            {
                Piece.RedPawn, Piece.None, Piece.RedPawn,
                Piece.None, Piece.RedPawn, Piece.None,
                Piece.RedPawn, Piece.None, Piece.RedPawn
            }, 
            // 4
            new Piece[Col]
            {
                Piece.None, Piece.None, Piece.None,
                Piece.None, Piece.None, Piece.None,
                Piece.None, Piece.None, Piece.None
            }, 
            // 5
            new Piece[Col]
            {
                Piece.None, Piece.None, Piece.None,
                Piece.None, Piece.None, Piece.None,
                Piece.None, Piece.None, Piece.None
            }, 
            // 6
            new Piece[Col]
            {
                Piece.BlackPawn, Piece.None, Piece.BlackPawn,
                Piece.None, Piece.BlackPawn, Piece.None,
                Piece.BlackPawn, Piece.None, Piece.BlackPawn
            }, 
            // 7
            new Piece[Col]
            {
                Piece.None, Piece.BlackCannon, Piece.None,
                Piece.None, Piece.None, Piece.None,
                Piece.None, Piece.BlackCannon, Piece.None
            }, 
            // 8
            new Piece[Col]
            {
                Piece.None, Piece.None, Piece.None,
                Piece.None, Piece.None, Piece.None,
                Piece.None, Piece.None, Piece.None
            }, 
            // 9
            new Piece[Col]
            {
                Piece.BlackRook, Piece.BlackKnight, Piece.BlackElephant,
                Piece.BlackGuard, Piece.BlackKing, Piece.BlackGuard,
                Piece.BlackElephant, Piece.BlackKnight, Piece.BlackRook
            }
        };

        [SetUp]
        public void SetUp()
        {
            gameController = new GameController();
        }


        /// <summary>
        /// Can reset to traditional board state
        /// </summary>
        [Test]
        public void ResetTraditional()
        {
            gameController.Reset();


            for (int i = 0; i < Row; ++i)
            {
                for (int j = 0; j < Col; ++j)
                {
                    Assert.AreEqual(
                        traditionalBoard[i][j],
                        gameController.GetChessman(i, j), 
                        "Expect at [{0}][{1}]",
                        i, j);
                }
            }
        }

        /// <summary>
        /// Test if Reset960 creates a unique board outside of traditional
        /// </summary>
        [Test]
        public void Reset960Unique()
        {
            gameController.Reset960();
            bool isUniqueSetup = false;
            for (int i = 0; i < Row; ++i)
            {
                for (int j = 0; j < Col; ++j)
                {
                    if (gameController.GetChessman(i, j) != traditionalBoard[i][j])
                    {
                        isUniqueSetup = true;
                        break;
                    }
                }
            }
            Assert.IsTrue(isUniqueSetup);
        }

        /// <summary>
        /// Test if reset has expected pieces
        /// </summary>
        [Test]
        public void Reset960HasPieces()
        {
            gameController.Reset960();
            var pieceList = new List<Piece>();
            for (var i = 0; i < GameController.RowSum; ++i)
            {
                for (var j = 0; j < GameController.ColSum; ++j)
                {
                    Piece p = gameController.GetChessman(i, j);
                    if (p != Piece.None)
                    {
                        pieceList.Add(p);
                    }
                }
            }

            var expectedL = new List<Piece>()
            {
                Piece.RedRook, Piece.RedKnight, Piece.RedElephant,
                Piece.RedGuard, Piece.RedKing, Piece.RedGuard,
                Piece.RedElephant, Piece.RedKnight, Piece.RedRook,
                Piece.RedCannon, Piece.RedCannon, Piece.RedPawn,
                Piece.RedPawn, Piece.RedPawn, Piece.RedPawn,
                Piece.RedPawn,

                Piece.BlackRook, Piece.BlackKnight, Piece.BlackElephant,
                Piece.BlackGuard, Piece.BlackKing, Piece.BlackGuard,
                Piece.BlackElephant, Piece.BlackKnight, Piece.BlackRook,
                Piece.BlackCannon, Piece.BlackCannon, Piece.BlackPawn,
                Piece.BlackPawn, Piece.BlackPawn, Piece.BlackPawn,
                Piece.BlackPawn
            };
            var expected = new Queue<Piece>(expectedL);

            // test if pieces are present
            while (expected.Count > 0)
            {
                var piece = expected.Dequeue();
                Assert.Contains(piece, pieceList);
                pieceList.Remove(piece);
            }



        }

        /// <summary>
        /// If chessman changes
        /// </summary>
        [Test]
        public void SetChessman()
        {
            gameController.Reset();
            gameController.SetChessman(0, 0, 1, 0);
            Assert.AreEqual(Piece.RedRook, gameController.GetChessman(1, 0));
        }

        /// <summary>
        /// Gets moves of a piece.
        /// </summary>
        [Test]
        public void SetMoveHelper()
        {
            gameController.Reset();
            gameController.SetMoveHelper(2, 1); // select cannon
            Assert.AreEqual(2, gameController.GetMoveHelper(2, 2));
            Assert.AreEqual(1, gameController.GetMoveHelper(9, 1));
            Assert.AreEqual(-1, gameController.GetMoveHelper(0, 0));

            gameController.SetMoveHelper(0, 0); // select car
            Assert.AreEqual(-1, gameController.GetMoveHelper(0, 1));
            Assert.AreEqual(-1, gameController.GetMoveHelper(3, 0));
        }

        /// <summary>
        /// Get number of chessman in path
        /// </summary>
        [Test]
        [TestCase(0, 0, 9, 0, 2)] // three piece in front of rook
        [TestCase(0, 0, 0, 8, 7)] // seven chessman to other side of board
        [TestCase(0, 1, 0, 7, 5)] // five chessman to other side of board
        [TestCase(0, 0, 1, 0, 0)] // two pawn in front of rook
        [TestCase(0, 5, 0, 4, 0)]
        public void GetChessmanCountInPath(int curX, int curY, int toX, int toY, int expect)
        {
            gameController.Reset();
            Assert.AreEqual(expect, gameController.GetChessmanCountInPath(curX, curY, toX, toY));
        }
    }
}
