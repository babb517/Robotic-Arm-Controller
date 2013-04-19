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
using System.Diagnostics;
using System.Windows.Forms;

namespace ArmController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /**************************************************************************/
        /* Private Members */
        /**************************************************************************/
        /// <summary>
        /// The actual main form.
        /// </summary>
        Form _main;

        /**************************************************************************/
        /* Constructors */
        /**************************************************************************/
        public MainWindow()
        {
            InitializeComponent();
        }

        /**************************************************************************/
        /* Event Handling */
        /**************************************************************************/

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            Hide();

            // Boot Strap hack.
            _main = new Form1();
            _main.Show();
        }

        /// <summary>
        /// Closes the main window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _main.Close();
        }

        /**************************************************************************/
        /* Private Members */
        /**************************************************************************/
    }
}
