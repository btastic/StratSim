﻿using DataSources.DataConnections;
using MyFlowLayout;
using StratSim.Model;
using StratSim.Model.Files;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StratSim.View.Panels
{
    public class GridBasePanel : MyPanel
    {
        internal ListBox GridOrder;
        Button MoveDriverUp, MoveDriverDown, MoveUp5, MoveDown5;
        Button Confirm;

        public GridBasePanel(MainForm parent, int[] gridOrder)
            : base(200, 370, "Race", parent, Properties.Resources.Grid)
        {
            InitialiseControls();
            PopulateListBox(gridOrder);
            AddControls();

            SetPanelProperties(DockTypes.TopLeft, AutosizeTypes.AutoHeight, FillStyles.None, this.Size);
        }

        private void AddControls()
        {
            this.Controls.Add(MoveUp5);
            this.Controls.Add(MoveDriverUp);
            this.Controls.Add(MoveDriverDown);
            this.Controls.Add(MoveDown5);
            this.Controls.Add(Confirm);
            this.Controls.Add(GridOrder);
        }

        private void InitialiseControls()
        {
            MoveUp5 = new Button
            {
                Image = Properties.Resources.UpFive,
                Width = 21,
                Height = 21,
                Left = 160,
                Top = 118,
                FlatStyle = FlatStyle.Flat
            };
            MoveUp5.Click += MoveUp5_Click;

            MoveDriverUp = new Button
            {
                Image = Properties.Resources.Stint_Up,
                Width = 21,
                Height = 21,
                Left = 160,
                Top = 149,
                FlatStyle = FlatStyle.Flat
            };
            MoveDriverUp.Click += MoveDriverUp_Click;

            MoveDriverDown = new Button
            {
                Image = Properties.Resources.Stint_Down,
                Width = 21,
                Height = 21,
                Left = 160,
                Top = 180,
                FlatStyle = FlatStyle.Flat
            };
            MoveDriverDown.Click += MoveDriverDown_Click;

            MoveDown5 = new Button
            {
                Image = Properties.Resources.DownFive,
                Width = 21,
                Height = 21,
                Left = 160,
                Top = 211,
                FlatStyle = FlatStyle.Flat
            };
            MoveDown5.Click += MoveDown5_Click;

            Confirm = new Button
            {
                Text = GetConfirmButtonText(),
                Width = 100,
                Height = 25,
                Left = 50,
                Top = 335,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle
            };
            Confirm.Click += Confirm_Click;

            GridOrder = new ListBox
            {
                Width = 125,
                Height = 300,
                Left = 25,
                Top = 25
            };

        }

        protected virtual string GetConfirmButtonText()
        {
            return "Confirm";
        }

        void PopulateListBox(int[] gridOrder)
        {
            bool firstZeroFound = false;
            int driverIndex = 0;
            int[] gridOrderItems = new int[Data.NumberOfDrivers];
            for (int position = 0; position < gridOrder.Length; position++)
            {
                driverIndex = gridOrder[position];
                if ((driverIndex != 0) || (!firstZeroFound))
                {
                    if (driverIndex == 0)
                        firstZeroFound = true;
                    GridOrder.Items.Add(Data.Drivers[driverIndex].DriverName);
                }
            }
        }

        void Confirm_Click(object sender, EventArgs e)
        {
            OnConfirmClicked();
        }

        protected virtual void OnConfirmClicked()
        {
        }

        void MoveDown5_Click(object sender, EventArgs e)
        {
            int currentPosition = GridOrder.SelectedIndex;
            MoveListBoxItem(currentPosition, +5);
        }

        void MoveDriverDown_Click(object sender, EventArgs e)
        {
            int currentPosition = GridOrder.SelectedIndex;
            MoveListBoxItem(currentPosition, +1);
        }

        void MoveDriverUp_Click(object sender, EventArgs e)
        {
            int currentPosition = GridOrder.SelectedIndex;
            MoveListBoxItem(currentPosition, -1);
        }

        void MoveUp5_Click(object sender, EventArgs e)
        {
            int currentPosition = GridOrder.SelectedIndex;
            MoveListBoxItem(currentPosition, -5);
        }

        void MoveListBoxItem(int currentPosition, int positionDelta)
        {
            int finishingPosition = currentPosition + positionDelta;
            var originalElement = GridOrder.Items[currentPosition];

            if (positionDelta > 0)
            {
                while (currentPosition < finishingPosition && currentPosition < GridOrder.Items.Count - 1)
                {
                    GridOrder.Items[currentPosition] = GridOrder.Items[currentPosition + 1];
                    ++currentPosition;
                }
                GridOrder.Items[currentPosition] = originalElement;
                GridOrder.SelectedIndex = currentPosition;
            }
            else if (positionDelta < 0)
            {
                while (currentPosition > finishingPosition && currentPosition > 0)
                {
                    GridOrder.Items[currentPosition] = GridOrder.Items[currentPosition - 1];
                    --currentPosition;
                }
                GridOrder.Items[currentPosition] = originalElement;
                GridOrder.SelectedIndex = currentPosition;
            }

        }
    }
}
