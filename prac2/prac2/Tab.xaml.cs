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
using System.IO;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using MyApp;

namespace prac2
{
    /// <summary>
    /// Interaction logic for Tab.xaml
    /// </summary>
    public partial class Tab : UserControl
    {
        CancellationTokenSource token = new CancellationTokenSource();
        Library model;
        public Tab(CancellationTokenSource token_, Library model_)
        {
            InitializeComponent();
            token = token_;
            model = model_;
        }

        private void Open_File(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document"; 
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text documents (.txt)|*.txt";

            // Show open file dialog box
            bool? result = dialog.ShowDialog();
            string path = "", text;
            if (result == true)
            {
                // Open document
                path = dialog.FileName;
            }
            StreamReader reader = new StreamReader(path);
            text = reader.ReadToEnd();
            File_Text.Text = text;

        }

        private async void Get_Answer(object sender, RoutedEventArgs e)
        {
            Answer.Text = "";
            if(string.IsNullOrWhiteSpace(File_Text.Text))
            {
                Answer.Text = "enter the file in txt format";
            }
            else if(string.IsNullOrWhiteSpace(Question_Text.Text))
            {
                Answer.Text = "enter the question";
            }
            else
            {
                string ans = await model.Dialog(File_Text.Text, Question_Text.Text);
                Answer.Text = ans;
            }
        }
 
        private void Cansel_Task(object sender, RoutedEventArgs e)
        {
            token.Cancel();
        }
    }
}
