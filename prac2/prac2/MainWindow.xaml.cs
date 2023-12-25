using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using static System.Net.Mime.MediaTypeNames;
using MyApp;

namespace prac2
{
    public partial class MainWindow : Window
    {
        static int count = 0;
        public MainWindow()
        {
            InitializeComponent();
            var token = new CancellationTokenSource();
            var answer = new Library(token.Token);
            answer.Init_model();
        }

        private void button_create_new_tab_Click(object sender, RoutedEventArgs e)
        {
            TabItem New_Tab = new TabItem();
            New_Tab.Header = "Tab " + (++count).ToString();
            var token = new CancellationTokenSource();
            var model = new Library(token.Token);
            model.Init_model();
            New_Tab.Content = new Tab(token, model);
            Tab_Control.Items.Add(New_Tab);
        }
    }
}
