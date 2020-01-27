using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MineSweep_v1._1
{
    class Map
    {
        private Random rand_pos = new Random();//提供随机数

        public readonly static int grid_size = 25;//网格大小
        protected readonly int row = 10, column = 10;//网格数
        private int _mine_num = 20;//地雷数量
        
        public int BlockNumber { get { return row * column; } }//块数
        public int Width { get { return grid_size * column; } }//宽
        public int Height { get { return grid_size * row; } }//高
        public int MineNumber { get => _mine_num; set => _mine_num = value; }

        //地图的点击移动
        public MouseEventHandler OnGridClick = null, OnMineClick = null;//点击每个网格
        public EventHandler OnGridHover = null, OnGridLeave = null;

        public List<Label> grid;//网格

        //产生地图
        public Map(int column, int row)
        {
            this.column = column; this.row = row;
            grid = new List<Label>();
            bVisible = new bool[column * row];
            for (int i = 0; i < bVisible.Length; i++)
                bVisible[i] = false;
        }

        //初始化显示地图（不包含地雷）
        public void InitMap()
        {
            if (OnGridClick != null && OnGridHover != null
                && OnGridLeave != null)
            {
                //设置label位置、大小等属性
                for (int i = 0; i < row; i++)
                    for (int j = 0; j < column; j++)
                    {
                        Label temp = new Label();
                        temp.Location = new Point(j * Map.grid_size, i * Map.grid_size);
                        temp.Size = new Size(Map.grid_size, Map.grid_size);
                        temp.Tag = "grid";
                        temp.Visible = true;
                        temp.BackColor = SystemColors.GrayText;
                        temp.BorderStyle = BorderStyle.FixedSingle;

                        temp.MouseClick += OnGridClick;//点击网格
                        temp.MouseHover += OnGridHover;//移动到网格
                        temp.MouseLeave += OnGridLeave;//离开网格

                        grid.Add(temp);
                    }
            }
            else throw new Exception("Init Fail");
        }

        //将实际位置的点转换为地图上的点
        public Point MouseToPoint(Point mousePoint)
        {
            return new Point(mousePoint.X / Map.grid_size, mousePoint.Y / Map.grid_size);
        }

        //初始化地图（包含地雷的）
        public void InitMine(Point location)
        {
            if (OnMineClick == null) throw new Exception("Init Fail");
            //获取对应坐标对应的grid的对应值
            int index = location.Y * column + location.X;
            for (int i = index + 1; i < index + MineNumber + 1; i++)
            {
                grid[i % BlockNumber].Tag = "mine";
                grid[i % BlockNumber].MouseClick -= OnGridClick;
                grid[i % BlockNumber].MouseClick += OnMineClick;
            }

            //随机排列Knuth-Shuffle，即 Knuth 洗牌算法。
            for (int i = grid.Count - 1; i >= 0; i--)
            {
                if (index == i) continue;
                Label temp = grid[i];
                int j; do { j = rand_pos.Next(0, i + 1); } while (j == index);
                grid[i] = grid[j];
                grid[j] = temp;
            }
            //重新编号
            for (int i = 0; i < row; i++)//y
                for (int j = 0; j < column; j++)//x
                    grid[j + column * i].Location = new Point(j * Map.grid_size, i * Map.grid_size);
            InitNumber();
            //for debug
            //ForDebugShow();
        }

        //for debug
        private void ForDebugShow()
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    int index = j + column * i;
                    if (grid[index].Tag.ToString() == "mine")
                        Console.Write("* ");
                    else Console.Write(grid[index].Tag.ToString() + " ");
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }

        //给每个块标号
        private void InitNumber()
        {
            //四周判断，编号
            for (int i = 0; i < row; i++)//y
                for (int j = 0; j < column; j++)//x
                {
                    int k = j + column * i;
                    if (grid[k].Tag.ToString() != "mine")
                        grid[k].Tag = Count(new Point(j, i)).ToString();//中心点
                }
        }

        //是否胜利
        public bool IsWine()
        {
            int count = 0;
            for(int i=0;i<bVisible.Length;i++)
                if (!bVisible[i])
                    count++;
            return (count == MineNumber);//胜利
        }

        //计算周围炸弹的个数
        private int Count(Point pos)
        {
            int i = pos.Y, j = pos.X;
            int[] dir = { (i - 1) * column + j - 1 , (i - 1) * column + j ,//左上角//上边
                (i - 1) * column + j + 1 , i * column + j - 1,//右上角//左边
                i * column + j + 1,(i + 1) * column + j - 1,//右边//左下角
                (i + 1) * column + j,(i + 1) * column + j + 1};//下边//右下角 
            //越界处理
            if (i <= 0) dir[0] = dir[1] = dir[2] = -1;
            if (i >= row - 1) dir[5] = dir[6] = dir[7] = -1;
            if (j <= 0) dir[0] = dir[3] = dir[5] = -1;
            if (j >= column - 1) dir[2] = dir[4] = dir[7] = -1;
            //计数
            int count = 0;
            for (int k = 0; k < dir.Length; k++)
                if (dir[k] >= 0 && grid[dir[k]].Tag.ToString() == "mine")
                    count++;
            return count;
        }

        //取消点击事件
        private void ClearEventHandler(Label label)
        {
            label.BackColor = SystemColors.ButtonHighlight;
            foreach (Delegate dele in OnGridClick.GetInvocationList())
                label.MouseClick -= (MouseEventHandler)dele;
            foreach (Delegate dele in OnGridHover.GetInvocationList())
                label.MouseHover -= (EventHandler)dele;
            foreach (Delegate dele in OnGridLeave.GetInvocationList())
                label.MouseLeave -= (EventHandler)dele;
        }

        //清除路障
        public void ClearGrid(Point location)
        {
            int i = location.Y, j = location.X;
            //消除周围
            int[] dir = { (i - 1) * column + j - 1 , (i - 1) * column + j ,//左上角//上边
                (i - 1) * column + j + 1 , i * column + j - 1,//右上角//左边
                i * column + j + 1,(i + 1) * column + j - 1,//右边//左下角
                (i + 1) * column + j,(i + 1) * column + j + 1,j + column * i};//下边//右下角 
            //越界处理
            if (i <= 0) dir[0] = dir[1] = dir[2] = -1;
            if (i >= row - 1) dir[5] = dir[6] = dir[7] = -1;
            if (j <= 0) dir[0] = dir[3] = dir[5] = -1;
            if (j >= column - 1) dir[2] = dir[4] = dir[7] = -1;
            //保存需要清除的label
            List<Label> clear_lab = new List<Label>();
            //先清除周围的8个路障
            for(int k = 0; k < dir.Length; k++)
            {
                if (dir[k] >= 0 && grid[dir[k]].Tag.ToString() != "mine" && !bVisible[dir[k]])
                {
                    ClearEventHandler(grid[dir[k]]);
                    //连带清除的路障
                    if ((grid[dir[k]].Tag.ToString() == "0" || grid[dir[k]].Tag.ToString() == "1") && k != dir.Length - 1)
                        clear_lab.Add(grid[dir[k]]);
                    else bVisible[dir[k]] = true;
                    //显示文字
                    if (grid[dir[k]].Tag.ToString() != "0")//不显示0
                        grid[dir[k]].Text = grid[dir[k]].Tag.ToString();
                }
            }
            //连带清除
            foreach(Label temp in clear_lab)
            {
                Point clp = MouseToPoint(temp.Location);//被清除标签位置
                int cli = clp.X + column * clp.Y;//被清除标签index
                if (!bVisible[cli]) ClearGrid(clp);//递归
            }
        }

        public bool[] bVisible;

        //通过位置指示标签（必须为实际位置）
        public Label FindByLocation(Point location)
        {
            for(int i=0;i<grid.Count;i++)
                if (grid[i].Location == location)
                    return grid[i];
            return null;
        }
    }
}



//int temp_mine_num = index < MineNumber ? MineNumber + 1 : MineNumber;
//for (int i = 0; i < temp_mine_num; i++)
//{
//    if (index == i) continue;
//    grid[i].Tag = "mine";
//    grid[i].MouseClick += OnMineClick;
//}


//do
//{
//    //将地雷顺序排列
//    for (int i = 0; i < MineNumber; i++)
//    {
//        grid[i].Tag = "mine";
//        grid[i].MouseClick += OnMineClick;
//    }
//    //随机排列Knuth-Shuffle，即 Knuth 洗牌算法。
//    for (int i = grid.Count - 1; i >= 0; i--)
//    {
//        int j = rand_pos.Next(0, i);
//        Label _shuffle = grid[i];
//        grid[i] = grid[j];
//        grid[j] = _shuffle;
//    }
//    //判断位置上是否为地雷
//} while (grid[index].Tag.ToString() == "mine");//这里可以稍微改进下，否则可能会一直运行