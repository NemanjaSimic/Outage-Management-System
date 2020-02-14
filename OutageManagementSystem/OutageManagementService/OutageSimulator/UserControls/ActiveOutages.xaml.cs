﻿using OMS.OutageSimulator.BindingModels;
using System;
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

namespace OMS.OutageSimulator.UserControls
{
    /// <summary>
    /// Interaction logic for ActiveOutages.xaml
    /// </summary>
    public partial class ActiveOutages : UserControl
    {
       public List<OutageBindingModel> ActiveOutageList { get; set; }

        public ActiveOutages()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
