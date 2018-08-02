using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseChess
{
    public class GameController
    {
        public const int RowSum = 10;  //行总数
        public const int ColSum = 9;   //列总数
        private Piece[][] chessMan;   //代表着全场象棋的数组
        private bool[][] avalChess; //选择棋子时的可选表
        private int[][] moveHelper; //移动棋子时的提示表
        private bool whosTurn;
        private const int minX = 0, minY = 0, maxX = 8, maxY = 9;

        private readonly List<Tuple<int, int, Piece>> traditionalSetup = new List<Tuple<int, int, Piece>>()
        {
            new Tuple<int, int, Piece>(0, 0, Piece.RedRook),
            new Tuple<int, int, Piece>(0, 0, Piece.RedRook),
            new Tuple<int, int, Piece>(0, 1, Piece.RedKnight),
            new Tuple<int, int, Piece>(0, 2, Piece.RedElephant),
            new Tuple<int, int, Piece>(0, 3, Piece.RedGuard),
            new Tuple<int, int, Piece>(0, 4, Piece.RedKing),
            new Tuple<int, int, Piece>(0, 5, Piece.RedGuard),
            new Tuple<int, int, Piece>(0, 6, Piece.RedElephant),
            new Tuple<int, int, Piece>(0, 7, Piece.RedKnight),
            new Tuple<int, int, Piece>(0, 8, Piece.RedRook),

            new Tuple<int, int, Piece>(2, 1, Piece.RedCannon),
            new Tuple<int, int, Piece>(2, 7, Piece.RedCannon),

            new Tuple<int, int, Piece>(3, 0, Piece.RedPawn),
            new Tuple<int, int, Piece>(3, 2, Piece.RedPawn),
            new Tuple<int, int, Piece>(3, 4, Piece.RedPawn),
            new Tuple<int, int, Piece>(3, 6, Piece.RedPawn),
            new Tuple<int, int, Piece>(3, 8, Piece.RedPawn),


            new Tuple<int, int, Piece>(9, 0, Piece.BlackRook),
            new Tuple<int, int, Piece>(9, 1, Piece.BlackKnight),
            new Tuple<int, int, Piece>(9, 2, Piece.BlackElephant),
            new Tuple<int, int, Piece>(9, 3, Piece.BlackGuard),
            new Tuple<int, int, Piece>(9, 4, Piece.BlackKing),
            new Tuple<int, int, Piece>(9, 5, Piece.BlackGuard),
            new Tuple<int, int, Piece>(9, 6, Piece.BlackElephant),
            new Tuple<int, int, Piece>(9, 7, Piece.BlackKnight),
            new Tuple<int, int, Piece>(9, 8, Piece.BlackRook),

            new Tuple<int, int, Piece>(7, 1, Piece.BlackCannon),
            new Tuple<int, int, Piece>(7, 7, Piece.BlackCannon),

            new Tuple<int, int, Piece>(6, 0, Piece.BlackPawn),
            new Tuple<int, int, Piece>(6, 2, Piece.BlackPawn),
            new Tuple<int, int, Piece>(6, 4, Piece.BlackPawn),
            new Tuple<int, int, Piece>(6, 6, Piece.BlackPawn),
            new Tuple<int, int, Piece>(6, 8, Piece.BlackPawn)
        };

        public GameController()
        {
            //申请空间
            chessMan = new Piece[RowSum][];
            avalChess = new bool[RowSum][];
            moveHelper = new int[RowSum][];
            for (int i = 0; i < RowSum; ++i)
            {
                chessMan[i] = new Piece[ColSum];
                avalChess[i] = new bool[ColSum];
                moveHelper[i] = new int[ColSum];
            }
        }

        public void Reset()
        {
            //初始化棋盘
            for (int i = 0; i < chessMan.Length; ++i)
            {
                for (int j = 0; j < chessMan[i].Length; ++j)
                {
                    chessMan[i][j] = Piece.None;
                }
            }

            foreach (var t in traditionalSetup)
            {
                chessMan[t.Item1][t.Item2] = t.Item3;
            }

            //初始化先手和可选棋表
            whosTurn = true;
            TransTurn();
        }

        // like chess960 but chinese chess
        public void Reset960()
        {
            bool isUnique()
            {
                bool isUniqueSetup = false;
                foreach (var t in traditionalSetup)
                {
                    if (chessMan[t.Item1][t.Item2] != t.Item3)
                    {
                        isUniqueSetup = true;
                        break;
                    }
                }

                return isUniqueSetup;
            }

            Random r = new Random();

            //初始化棋盘
            for (int i = 0; i < chessMan.Length; ++i)
            {
                for (int j = 0; j < chessMan[i].Length; ++j)
                {
                    chessMan[i][j] = Piece.None;
                }
            }

            do
            {
                // pool
                var list = new List<Piece>()
                {
                    Piece.RedRook,
                    Piece.RedRook,
                    Piece.RedElephant,
                    Piece.RedElephant,
                    Piece.RedKnight,
                    Piece.RedKnight,
                    Piece.RedCannon,
                    Piece.RedCannon
                };
                var redPool = new Stack<Piece>(list.OrderBy<Piece, int>(p => r.Next()));

                // setup red

                chessMan[0][0] = redPool.Pop();
                chessMan[0][1] = redPool.Pop();
                chessMan[0][2] = redPool.Pop();
                chessMan[0][6] = redPool.Pop();
                chessMan[0][7] = redPool.Pop();
                chessMan[0][8] = redPool.Pop();
                chessMan[2][1] = redPool.Pop();
                chessMan[2][7] = redPool.Pop();
                chessMan[3][0] = Piece.RedPawn;
                chessMan[3][2] = Piece.RedPawn;
                chessMan[3][4] = Piece.RedPawn;
                chessMan[3][6] = Piece.RedPawn;
                chessMan[3][8] = Piece.RedPawn;
                chessMan[0][3] = Piece.RedGuard;
                chessMan[0][4] = Piece.RedKing;
                chessMan[0][5] = Piece.RedGuard;

                // setup black

                chessMan[9][0] = chessMan[0][0].Invert();
                chessMan[9][1] = chessMan[0][1].Invert();
                chessMan[9][2] = chessMan[0][2].Invert();
                chessMan[9][6] = chessMan[0][6].Invert();
                chessMan[9][7] = chessMan[0][7].Invert();
                chessMan[9][8] = chessMan[0][8].Invert();
                chessMan[7][1] = chessMan[2][1].Invert();
                chessMan[7][7] = chessMan[2][7].Invert();
                chessMan[6][0] = chessMan[3][0].Invert();
                chessMan[6][2] = chessMan[3][2].Invert();
                chessMan[6][4] = chessMan[3][4].Invert();
                chessMan[6][6] = chessMan[3][6].Invert();
                chessMan[6][8] = chessMan[3][8].Invert();
                chessMan[9][3] = chessMan[0][3].Invert();
                chessMan[9][4] = chessMan[0][4].Invert();
                chessMan[9][5] = chessMan[0][5].Invert();
            } while (!isUnique()); // ensure dont end up traditional


            //初始化先手和可选棋表
            whosTurn = true;
            TransTurn();
        }

        //获取棋子
        public Piece GetChessman(int i, int j)
        {
            return chessMan[i][j];
        }

        //获取可移动状态
        public bool IsAvaliable(int i, int j)
        {
            return avalChess[i][j];
        }

        //获取移动目标状态
        public int GetMoveHelper(int i, int j)
        {
            return moveHelper[i][j];
        }

        //设置可移动状态，己方棋子及自身=3，可选敌方棋子=1，可选空位=2，不可选棋子0
        //own piece = 3, can capture = 1, can move = 2
        //3 沒用, -1 不能動
        public void SetMoveHelper(int i, int j)
        {
            for(int a = 0; a < RowSum; ++a)
            {
                for(int b = 0; b < ColSum; ++b)
                {
                    if (i == -1 && j == -1)
                    {
                        moveHelper[a][b] = -1;
                        continue;
                    }
                    if ((int) GetChessman(a, b) * (int) GetChessman(i, j) > 0)
                        moveHelper[a][b] = -1;
                    else if (CanMove(i, j, a, b) == true)
                    {
                        if (GetChessman(a, b) == 0)
                            moveHelper[a][b] = 2;
                        else
                            moveHelper[a][b] = 1;
                    }
                    else
                        moveHelper[a][b] = 0;
                }
            }
        }

        //游戏结束判断
        public bool IsGameOver(int i, int j)
        {
            if(Math.Abs((int) chessMan[i][j]) == 1)
                return true;
            return false;
        }

        //游戏结束
        public bool GameOver()
        {
            //使所有棋子不能再移动，并返回赢家
            for (int i = 0; i < avalChess.Length; ++i)
                for (int j = 0; j < avalChess[i].Length; ++j)
                    avalChess[i][j] = false;
            return !whosTurn;
        }

        //获取路径中棋子个数
        public int GetChessmanCountInPath(int oldi, int oldj, int i, int j)
        {
            if (i != oldi && j != oldj)
                return -1;
            int count = 0;
            if(i == oldi)
            {
                int maxer = Math.Max(oldj, j);
                int miner = Math.Min(oldj, j);
                for (int loop = miner + 1; loop < maxer; ++loop)
                    if (chessMan[i][loop] != 0)
                        count++;
            }
            else if(j == oldj)
            {
                int maxer = Math.Max(oldi, i);
                int miner = Math.Min(oldi, i);
                for (int loop = miner + 1; loop < maxer; ++loop)
                    if (chessMan[loop][j] != 0)
                        count++;
            }
            return count;
        }

        //判断是否可以移动
        public bool CanMove(int oldi, int oldj, int i, int j)
        {
            //是否越界
            if (i < minY || i > maxY || j < minX || j > maxX)
                return false;
            //是否移动到本方棋子上
            if ((chessMan[i][j] > 0 && chessMan[oldi][oldj] > 0) || (chessMan[i][j] < 0 && chessMan[oldi][oldj] < 0))
                return false;
            //是否满足特定棋子的规律
            Piece type = chessMan[oldi][oldj];
            switch (Math.Abs((int) type))
            {
                case 0: return false;
                //帅/将 必须在九宫格内，每次只能移动一步，或者移动到对方的将
                case 1:
                    //移动到对方的将，且中间没有阻挡
                    if ((int) chessMan[i][j] == (-((int) chessMan[oldi][oldj])) && GetChessmanCountInPath(oldi, oldj, i, j) == 0)
                        return true;
                    //目标仅移动一步
                    if (Math.Abs(i - oldi) + Math.Abs(j - oldj) != 1)
                        return false;
                    //目标在九宫格内
                    if (type < 0 && (i >= 0 && i <= 2 && j >= 3 && j <= 5))
                        return true;
                    else if (type > 0 && (i >= 7 && i <= 9 && j >= 3 && j <= 5))
                        return true;
                    break;
                //兵/卒 只能平移或者向对方移动
                case 2:
                    //目标仅移动一步
                    if (Math.Abs(i - oldi) + Math.Abs(j - oldj) != 1)
                        return false;
                    //目标平移或向对方移动
                    if (type < 0 && (i > oldi || (i == oldi && i >= 5)))
                        return true;
                    if (type > 0 && (i < oldi || (i == oldi && i < 5)))
                        return true;
                    break;
                //仕/士 必须在九宫格内，每次只能斜线一步
                case 3:
                    //目标仅斜线移动一步
                    if (!(Math.Abs(i - oldi) == 1 && Math.Abs(j - oldj) == 1))
                        return false;
                    //目标在九宫格内
                    if (type < 0 && (i >= 0 && i <= 2 && j >= 3 && j <= 5))
                        return true;
                    else if (type > 0 && (i >= 7 && i <= 9 && j >= 3 && j <= 5))
                        return true;
                    break;
                //炮/砲 仅可直线移动，且如果路径中有棋子(有且仅有1个)，必须移动到敌方棋子上
                case 4:
                    //仅可直线移动
                    if (i != oldi && j != oldj)
                        return false;
                    //如果目标为空位，那么路径中不能有棋子，否则必须有1个
                    if (GetChessman(i, j) == 0 && GetChessmanCountInPath(oldi, oldj, i, j) == 0)
                        return true;
                    if (GetChessman(i, j) != 0 && GetChessmanCountInPath(oldi, oldj, i, j) == 1)
                        return true;
                    break;
                //相/象 仅可田字型移动，且不可卡位，且不可过河
                case 5:
                    //不可过河
                    if (type < 0 && i >= 5)
                        return false;
                    if (type > 0 && i < 5)
                        return false;
                    //仅可田字型移动
                    if (!(Math.Abs(i - oldi) == 2 && Math.Abs(j - oldj) == 2))
                        return false;
                    //不可被卡位
                    if (i > oldi && j > oldj && chessMan[oldi + 1][oldj + 1] == 0)
                        return true;
                    if (i > oldi && j < oldj && chessMan[oldi + 1][oldj - 1] == 0)
                        return true;
                    if (i < oldi && j > oldj && chessMan[oldi - 1][oldj + 1] == 0)
                        return true;
                    if (i < oldi && j < oldj && chessMan[oldi - 1][oldj - 1] == 0)
                        return true;
                    break;
                //车/车 仅可直线移动，且路径中不能有棋子
                case 6:
                    //仅可直线移动
                    if (i != oldi && j != oldj)
                        return false;
                    //路径中不能有棋子
                    if (GetChessmanCountInPath(oldi, oldj, i, j) == 0)
                        return true;
                    break;
                //马/马 仅可日字型移动，且不能被卡位
                case 7:
                    //仅可日字型移动
                    if (!((Math.Abs(i - oldi) == 2 && Math.Abs(j - oldj) == 1) || (Math.Abs(i - oldi) == 1 && Math.Abs(j - oldj) == 2)))
                        return false;
                    //不能被卡位
                    if (Math.Abs(i - oldi) == 2)
                    {
                        if (i > oldi && j > oldj && chessMan[oldi + 1][oldj] == 0)
                            return true;
                        if (i > oldi && j < oldj && chessMan[oldi + 1][oldj] == 0)
                            return true;
                        if (i < oldi && j > oldj && chessMan[oldi - 1][oldj] == 0)
                            return true;
                        if (i < oldi && j < oldj && chessMan[oldi - 1][oldj] == 0)
                            return true;
                    }
                    else if(Math.Abs(j - oldj) == 2)
                    {
                        if (i > oldi && j > oldj && chessMan[oldi][oldj + 1] == 0)
                            return true;
                        if (i > oldi && j < oldj && chessMan[oldi][oldj - 1] == 0)
                            return true;
                        if (i < oldi && j > oldj && chessMan[oldi][oldj + 1] == 0)
                            return true;
                        if (i < oldi && j < oldj && chessMan[oldi][oldj - 1] == 0)
                            return true;
                    }
                    break;
                default:
                    return false;
            }
            return false;
        }

        //还原棋子
        public void ResetChessman(int oldi, int oldj, int i, int j, Piece oldValue, Piece value)
        {
            chessMan[i][j] = value;
            chessMan[oldi][oldj] = oldValue;
            TransTurn();
        }

        //移动棋子
        public void SetChessman(int oldi, int oldj, int i, int j)
        {
            chessMan[i][j] = chessMan[oldi][oldj];
            chessMan[oldi][oldj] = 0;
            TransTurn();
        }

        //跳过回合，防止TransTurn被滥用
        public void SkipTurn()
        {
            TransTurn();
        }

        //改变回合
        private void TransTurn()
        {
            whosTurn = !whosTurn;
            for (int i = 0; i < avalChess.Length; ++i)
                for (int j = 0; j < avalChess[i].Length; ++j)
                    if (chessMan[i][j] < 0 && whosTurn == false)
                        avalChess[i][j] = true;
                    else if (chessMan[i][j] > 0 && whosTurn == true)
                        avalChess[i][j] = true;
                    else avalChess[i][j] = false;
        }
        
    }
}
