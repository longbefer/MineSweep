using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MineSweep_v1._1
{
    public partial class Start : Form
    {
        private Map _map;
        private bool _bStart = false;
        public Start()
        {
            InitializeComponent();
            InitGame();
        }

        public void InitGame()
        {
            _map = new Map(10, 10);
            _map.MineNumber = _map.BlockNumber / 3;
            _map.OnGridClick += OnGridClick;
            _map.OnGridHover += OnGridHover;
            _map.OnGridLeave += OnGridLeave;
            _map.OnMineClick += OnMineClick;
            _map.InitMap();
            OnShowMap(_map.grid);
            this.ClientSize = new Size(_map.Width, _map.Height);
        }

        //游戏展示
        public void OnShowMap(List<Label> each_label)
        {
            this.Controls.Clear();
            foreach (Label temp in each_label)
                this.Controls.Add(temp);
        }

        //鼠标控制
        public void OnGridClick(object sender, MouseEventArgs e)
        {
            Label current_grid = (Label)sender;
            Point org_point = current_grid.Location;
            Point fix_point = _map.MouseToPoint(org_point);

            if (e.Button == MouseButtons.Left)
            {
                if (!_bStart)
                {
                    _map.InitMine(fix_point);
                    OnShowMap(_map.grid);
                    _bStart = true;
                }
                //消去路障
                _map.ClearGrid(fix_point);
            }
            if (_bStart && e.Button == MouseButtons.Right)
                current_grid.BackColor = current_grid.BackColor != Color.OrangeRed ? Color.OrangeRed : SystemColors.GrayText;

            if (_map.IsWine())
                if (MessageBox.Show("胜利了", "游戏提示", MessageBoxButtons.OK, 0) == DialogResult.OK)
                {
                    //新开局
                    Start _start = new Start();
                    _start.FormClosed += _start_FormClosed;
                    _start.Show();
                    this.Hide();
                }
        }

        public void OnGridHover(object sender, EventArgs e)
        {
            Label current_grid = (Label)sender;
            current_grid.BackColor = current_grid.BackColor == Color.OrangeRed ? Color.OrangeRed : SystemColors.ActiveBorder;
        }

        public void OnGridLeave(object sender, EventArgs e)
        {
            Label current_grid = (Label)sender;
            current_grid.BackColor = current_grid.BackColor == Color.OrangeRed ? Color.OrangeRed : SystemColors.GrayText;
        }

        public void OnMineClick(object sender, MouseEventArgs e)
        {
            Label current_mine = (Label)sender;
            if (e.Button == MouseButtons.Left)
            {
                current_mine.BackColor = Color.OrangeRed;
                //使所有全部消失
                if (MessageBox.Show("游戏结束！", "游戏提示", MessageBoxButtons.OK, 0) == DialogResult.OK)
                {
                    //游戏结束
                    Start _start = new Start();
                    _start.FormClosed += _start_FormClosed;
                    _start.Show();
                    this.Hide();
                }
            }
            if (_bStart && e.Button == MouseButtons.Right)
                current_mine.BackColor = current_mine.BackColor != Color.OrangeRed ? Color.OrangeRed : SystemColors.GrayText;
        }

        private void _start_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }
    }
}
