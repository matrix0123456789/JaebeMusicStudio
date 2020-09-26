﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JaebeMusicStudio.Sound;

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for SoundLineSelect.xaml
    /// </summary>
    public partial class SoundLineSelect : UserControl
    {
        public event Action<SoundLineSelect, SoundLineAbstract> Changed;
        public SoundLineSelect()
        {
            InitializeComponent();
        }

        public SoundLineAbstract Selected
        {
            get { return MainSelect.SelectedValue as SoundLineAbstract; }
            set { MainSelect.SelectedValue = value; }
        }
        private void MainSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Changed?.Invoke(this, MainSelect.SelectedValue as SoundLineAbstract);

        }

        public void Generate()
        {
            MainSelect.Items.Clear();
            foreach (var x in Project.current.lines)
            {
                MainSelect.Items.Add(x);
            }
            foreach (var x in Project.current.liveLines.getAvaibleInputs())
            {
                MainSelect.Items.Add(x);
            }
        }
    }
}
