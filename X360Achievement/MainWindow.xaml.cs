using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace X360Achievement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();

        }

        private void OnCompleted(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
