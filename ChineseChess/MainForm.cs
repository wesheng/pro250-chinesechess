using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace ChineseChess
{
    public partial class MainForm : Form
    {
        private GameController gameController;
        private Dictionary<int, Image> chessmanImagePair;   //����int-Image�Ķ�Ӧ
        private const int RowSum = 10;  //������
        private const int ColSum = 9;   //������
        private int[] lastStep; //��¼��һ���Ķ��������ڳ�������
        private PictureBox currentChosenPictureBox = null;  //��ǰѡ�е�����
        private Image currentChosenImage = null;    //��ǰѡ�е����ӵ�ͼ����������Ч����ʵ��
        private System.Timers.Timer flickerTimer;   //��ʱ������������Ч����ʵ��
        private int[][] moveHelper; //���ڴ��浱ǰѡ�������ӿɽӴ����������ƶ�������λ��״̬

        public MainForm()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.skipToolStripMenuItem.Enabled = false;
            this.undoToolStripMenuItem.Enabled = false;
        }

        //�Ҽ��˵�-����Ϸ
        private void NewGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //����Ϸ�ĳ�ʼ������
            if (this.panelChessman.Controls.Count == 0)
            {
                gameController = new GameController();

                //���û�����ɹ�����PictureBox����ô����10 * 9��
                for (int j = 0; j < RowSum; ++j)
                {
                    for (int i = 0; i < ColSum; ++i)
                    {
                        PictureBox pictureBox = new PictureBox();
                        pictureBox.Size = new Size(70, 70);
                        if (j < 5)
                            pictureBox.Location = new Point(i * 83, 10 + j * 83);
                        else if (j >= 5)
                            pictureBox.Location = new Point(i * 83, 15 + j * 83);
                        pictureBox.Parent = panelChessman;
                        pictureBox.BackColor = Color.Transparent;
                        pictureBox.Click += new EventHandler(ClickChessEvent);
                        pictureBox.MouseEnter += new EventHandler(MouseEnterEvent);
                        pictureBox.MouseLeave += new EventHandler(MouseExitEvent);
                        panelChessman.Controls.Add(pictureBox);
                    }
                }

                flickerTimer = new System.Timers.Timer();
                flickerTimer.Elapsed += new ElapsedEventHandler(TimerPictureBoxFlicker);
                flickerTimer.Interval = 500;
                flickerTimer.AutoReset = true;

                //��ӱ�ź�Image��Ӧ���ֵ�
                chessmanImagePair = new Dictionary<int, Image>();
                chessmanImagePair.Add(-7, Properties.Resources.enemy7); chessmanImagePair.Add(7, Properties.Resources.friend7);
                chessmanImagePair.Add(-6, Properties.Resources.enemy6); chessmanImagePair.Add(6, Properties.Resources.friend6);
                chessmanImagePair.Add(-5, Properties.Resources.enemy5); chessmanImagePair.Add(5, Properties.Resources.friend5);
                chessmanImagePair.Add(-4, Properties.Resources.enemy4); chessmanImagePair.Add(4, Properties.Resources.friend4);
                chessmanImagePair.Add(-3, Properties.Resources.enemy3); chessmanImagePair.Add(3, Properties.Resources.friend3);
                chessmanImagePair.Add(-2, Properties.Resources.enemy2); chessmanImagePair.Add(2, Properties.Resources.friend2);
                chessmanImagePair.Add(-1, Properties.Resources.enemy1); chessmanImagePair.Add(1, Properties.Resources.friend1);
                chessmanImagePair.Add(0, null);
                chessmanImagePair.Add(10, Properties.Resources.green);  chessmanImagePair.Add(-10, Properties.Resources.red);

                //��̬��������ռ�
                lastStep = new int[6];
            }

            //������Ϸ�����°ڷ�����
            gameController.Reset();
            this.ResetAllChessman();
            //��ʼ��������ʱ��
            DisableFlickerTimer();
            //�����Ҽ��˵�
            this.skipToolStripMenuItem.Enabled = true;
            this.undoToolStripMenuItem.Enabled = true;
            //��ʼ������
            SetLastStep(-1, -1, -1, -1, -1, -1);
        }

        //������������
        private void ResetAllChessman()
        {
            int index = 0;
            foreach (Control control in this.panelChessman.Controls)
            {
                PictureBox pictureBox = (PictureBox)control;
                pictureBox.Image = chessmanImagePair[gameController.GetChessman(index / ColSum, index % ColSum)];
                index++;
            }
        }

        //�ƶ���������
        private bool ResetAChessman(int lasti, int lastj, int i, int j)
        {
            //��Ϸ�Ƿ񼴽�����
            bool gameWillOver = false;
            if (true == gameController.CanMove(lasti, lastj, i, j) && true == gameController.IsGameOver(i, j))
                gameWillOver = true;
            //�ƶ�
            SetLastStep(lasti, lastj, i, j, gameController.GetChessman(lasti, lastj), gameController.GetChessman(i, j));
            gameController.SetChessman(lasti, lastj, i, j);
            PictureBox pictureBox = (PictureBox)this.panelChessman.Controls[i * ColSum + j];
            pictureBox.Image = chessmanImagePair[gameController.GetChessman(i, j)];
            PictureBox oldPictureBox = (PictureBox)this.panelChessman.Controls[lasti * ColSum + lastj];
            oldPictureBox.Image = chessmanImagePair[0];
            if (gameWillOver)
            {
                bool winner = gameController.GameOver();
                currentChosenImage = null;
                DisableFlickerTimer();
                this.skipToolStripMenuItem.Enabled = false;
                this.undoToolStripMenuItem.Enabled = false;
                this.Cursor = Cursors.Default;
                MessageBox.Show("��Ϸ������" + (winner == false ? "��" : "��") + "��ʤ��");
            }
            return true;
        }

        //��������
        private void UpdateChessPanel()
        {
            for(int i = 0; i < RowSum; ++i)
                for(int j = 0; j < ColSum; ++j)
                    if(gameController.GetMoveHelper(i, j) == 1)
                        ((PictureBox)this.panelChessman.Controls[i * ColSum + j]).BackgroundImage = chessmanImagePair[-10];
                    else if (gameController.GetMoveHelper(i, j) == -1)
                        ((PictureBox)this.panelChessman.Controls[i * ColSum + j]).BackgroundImage = chessmanImagePair[0];
                    else if (gameController.GetMoveHelper(i, j) == 2)
                        ((PictureBox)this.panelChessman.Controls[i * ColSum + j]).BackgroundImage = chessmanImagePair[10];
        }

        //��¼��һ������Ϣ
        private void SetLastStep(int lasti, int lastj, int i, int j, int lastValue, int value)
        {
            lastStep[0] = lasti;
            lastStep[1] = lastj;
            lastStep[2] = i;
            lastStep[3] = j;
            lastStep[4] = lastValue;
            lastStep[5] = value;
        }

        //�Ҽ��˵�-�����غ�
        private void SkipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveHelper();
            DisableFlickerTimer();
            gameController.SkipTurn();
        }

        //�Ҽ��˵�-����
        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lastStep[0] == -1)
                return;
            RemoveHelper();
            DisableFlickerTimer();
            gameController.ResetChessman(lastStep[0], lastStep[1], lastStep[2], lastStep[3], lastStep[4], lastStep[5]);
            PictureBox pictureBox = (PictureBox)this.panelChessman.Controls[lastStep[2] * ColSum + lastStep[3]];
            pictureBox.Image = chessmanImagePair[lastStep[5]];
            PictureBox oldPictureBox = (PictureBox)this.panelChessman.Controls[lastStep[0] * ColSum + lastStep[1]];
            oldPictureBox.Image = chessmanImagePair[lastStep[4]];
            SetLastStep(-1, -1, -1, -1, -1, -1);
        }

        //�Ҽ��˵�-�˳�
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //������������¼�
        private void MouseEnterEvent(object sender, EventArgs e)
        {
            //��ȡ��ǰ��PictureBox
            PictureBox pictureBox = (PictureBox)sender;
            //��ǰû��ѡ���Ҹõ�Ϊ��
            if(currentChosenPictureBox == null && pictureBox.Image == null)
            {
                this.Cursor = Cursors.No;
            }
            //��ǰû��ѡ��
            else if(currentChosenPictureBox == null)
            {
                int index = 0;
                foreach (Control control in this.panelChessman.Controls)
                {
                    if (pictureBox == (PictureBox)control)
                        break;
                    index++;
                }
                if (gameController.IsAvaliable(index / ColSum, index % ColSum) == false)
                {
                    this.Cursor = Cursors.No;
                }
                else
                {
                    this.Cursor = Cursors.Hand;
                }
            }
            //��ǰ��ѡ�е�
            else
            {
                //�����ѡ�У����ж��ǲ���ͬһ�ߵģ�����ǣ�ָ��ĳ�Hand
                //�����ѡ�У���ô�����ƶ����Ȼ�ȡ��ЩPictureBox��λ��
                PictureBox lastPictureBox = currentChosenPictureBox;
                int index = 0, thisIndex = 0, lastIndex = 0;
                foreach (Control control in this.panelChessman.Controls)
                {
                    if (lastPictureBox == (PictureBox)control)
                        lastIndex = index;
                    if (pictureBox == (PictureBox)control)
                        thisIndex = index;
                    index++;
                }
                //�����ƶ����鿴��������ǲ�ʵ���ƶ�
                /*if ((gameController.GetChessman(lastIndex / ColSum, lastIndex % ColSum) < 0 && gameController.GetChessman(thisIndex / ColSum, thisIndex % ColSum) < 0)
                 || (gameController.GetChessman(lastIndex / ColSum, lastIndex % ColSum) > 0 && gameController.GetChessman(thisIndex / ColSum, thisIndex % ColSum) > 0)
                 || true == gameController.CanMove(lastIndex / ColSum, lastIndex % ColSum, thisIndex / ColSum, thisIndex % ColSum))*/
                if(gameController.GetMoveHelper(thisIndex / ColSum, thisIndex % ColSum) != 0)
                    this.Cursor = Cursors.Hand;
                else
                    this.Cursor = Cursors.No;
            }
        }

        //����Ƴ������¼�
        private void MouseExitEvent(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        //����������¼�
        private void ClickChessEvent(object sender, EventArgs e)
        {
            //��ȡ��ǰ��PictureBox
            PictureBox pictureBox = (PictureBox)sender;
            //�ж��Ƿ��Ѿ���ѡ��
            if (currentChosenPictureBox == null && pictureBox.Image == null)
                return;
            if (currentChosenPictureBox == null)
            {
                int index = 0;
                foreach (Control control in this.panelChessman.Controls)
                {
                    if (pictureBox == (PictureBox)control)
                        break;
                    index++;
                }
                if (gameController.IsAvaliable(index / ColSum, index % ColSum))
                {
                    SetHelper(index / ColSum, index % ColSum);
                    currentChosenPictureBox = pictureBox;
                    flickerTimer.Enabled = true;
                }
            }
            else if (currentChosenPictureBox == pictureBox)
            {
                RemoveHelper();
                DisableFlickerTimer();
            }
            else
            {
                //�����ѡ�У���ô�����ƶ����Ȼ�ȡ��ЩPictureBox��λ��
                PictureBox lastPictureBox = currentChosenPictureBox;
                int index = 0, thisIndex = 0, lastIndex = 0;
                foreach (Control control in this.panelChessman.Controls)
                {
                    if (lastPictureBox == (PictureBox)control)
                        lastIndex = index;
                    if (pictureBox == (PictureBox)control)
                        thisIndex = index;
                    index++;
                }
                //�����ƶ�����������ƶ�����ô�ȸı��ٽ��ã���������ƶ�����ô�Ƚ����ٸı�
                //���ƶ�֮ǰ�Ƴ���ʾ�򣬷�ֹ������
                RemoveHelper();
                if (true == gameController.CanMove(lastIndex / ColSum, lastIndex % ColSum, thisIndex / ColSum, thisIndex % ColSum))
                {
                    this.ResetAChessman(lastIndex / ColSum, lastIndex % ColSum, thisIndex / ColSum, thisIndex % ColSum);
                    this.Cursor = Cursors.No;
                    currentChosenPictureBox = pictureBox;
                    DisableFlickerTimer();
                }
                else
                {
                    DisableFlickerTimer();
                    currentChosenPictureBox = null;
                }
            }
        }

        //������ʾ��
        private void SetHelper(int i, int j)
        {
            gameController.SetMoveHelper(i, j);
            UpdateChessPanel();
        }

        //�Ƴ���ʾ��
        private void RemoveHelper()
        {
            gameController.SetMoveHelper(-1, -1);
            UpdateChessPanel();
        }

        //����������ʱ����û��ѡȡ�����ӻ���ѡȡ�������Ѿ�����˱����ƶ�
        private void DisableFlickerTimer()
        {
            if(currentChosenImage != null)
                currentChosenPictureBox.Image = currentChosenImage;
            flickerTimer.Enabled = false;
            currentChosenImage = null;
            currentChosenPictureBox = null;
        }

        //�ؼ������¼�����Image��NULL֮�佻���Դﵽ����Ч��
        private void TimerPictureBoxFlicker(object sender, EventArgs e)
        {
            if (currentChosenPictureBox != null && currentChosenImage == null)
                currentChosenImage = currentChosenPictureBox.Image;
            if (currentChosenPictureBox.Image == null)
                currentChosenPictureBox.Image = currentChosenImage;
            else
                currentChosenPictureBox.Image = null;
        }

        //����ܼ��غ���
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.panelChessman.Parent = this.pictureBoxChessPanel;
        }
    }
}
