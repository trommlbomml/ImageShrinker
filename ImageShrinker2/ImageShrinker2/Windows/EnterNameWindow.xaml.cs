﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageShrinker2.Windows
{
    /// <summary>
    /// Interaction logic for EnterNameWindow.xaml
    /// </summary>
    public partial class EnterNameWindow : Window
    {
        public EnterNameWindow()
        {
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}