﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace gosc.ProBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Controller controller;
        public MainWindow()
        {
            

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            controller = new Controller(statusBlock);
            controller.Start(LogBox.Text,PassBox.Text, CodeBox.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }

        private void MyBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            
        }       

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            controller.Exit();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
           
        }
    }
}
