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
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;




namespace ProjectCheetos
{
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string openedfile = "none";
           
        public MainWindow()
        {
            InitializeComponent();
        }

        

        private void MenuItem_SaveAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Python file (*.py)|*.py|Text file (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, StringFromRichTextBox(TextField));
                openedfile = saveFileDialog.FileName;
                this.Title = System.IO.Path.GetFileName(openedfile) + " | Cheetos IDE";
        }

        string StringFromRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                // TextPointer to the start of content in the RichTextBox.
                rtb.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                rtb.Document.ContentEnd
            );

            // The Text property on a TextRange object returns a string
            // representing the plain text content of the TextRange.
            return textRange.Text;
        }

        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Python file (*.py)|*.py|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                TextField.Document.Blocks.Clear();
                TextField.AppendText(File.ReadAllText(openFileDialog.FileName));
                openedfile =  openFileDialog.FileName;
                this.Title = System.IO.Path.GetFileName(openedfile) + " | Cheetos IDE";
               //Console.WriteLine(openedfile);
        }

        private void Run(object sender, RoutedEventArgs e)
        {
            if (openedfile == "none")
            {
                MessageBox.Show("Please save the file first");
            }
            else
            {
                System.Diagnostics.Process.Start(openedfile);
            }
            
            
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (openedfile == "none")
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Python file (*.py)|*.py|Text file (*.txt)|*.txt";
                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllText(saveFileDialog.FileName, StringFromRichTextBox(TextField));
                openedfile = saveFileDialog.FileName;
                this.Title = System.IO.Path.GetFileName(openedfile) + " | Cheetos IDE";
            }
            else
            {
                File.WriteAllText(openedfile, StringFromRichTextBox(TextField));
            }
           
        }
    }
}
