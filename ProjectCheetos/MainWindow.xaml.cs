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
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ProjectCheetos
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        
        string openedfile = "none";
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public MainWindow()
        {
            InitializeComponent();
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in drives)
                trvStructure.Items.Add(CreateTreeItem(driveInfo));
            
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
            openedfile = openFileDialog.FileName;
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
                Process.Start(openedfile);
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }
        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }
        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
        private void EncryptButton(object sender, RoutedEventArgs e)
        {
            string encryptedtext = Encrypt(TextField.Selection.Text, "chongoslol");
            TextField.Selection.Text = encryptedtext;
        }
        private void DecryptButton(object sender, RoutedEventArgs e)
        {
            string decryptedtext = Decrypt(TextField.Selection.Text, "chongoslol");
            TextField.Selection.Text = decryptedtext;
        }

        // Create a List using Range    
        string[] pythonkw = { "False","class","from","or","None","continue","global","pass","True","def","if","raise","and","del","import","return","as","elif","in","try","assert","else","is","while","async","except","lambda","with","await","finally","nonlocal","yield","break","for","not","(", ")"};
        string[] htmlkw = { "Cow", "Camel", "Elephant" };
        string[] csskw = { "Cow", "Camel", "Elephant" };
        string[] jskw = { "Cow", "Camel", "Elephant" };

        private void TextField_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<string> python = new List<string>(pythonkw);
            List<string> html = new List<string>(htmlkw);
            List<string> css = new List<string>(csskw);
            List<string> js = new List<string>(jskw);
            IEnumerable<TextRange> wordRanges = GetAllWordRanges(TextField.Document);
            foreach (TextRange wordRange in wordRanges)
            {
                if (python.Contains(wordRange.Text))
                {
                    wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Lime);
                }
                else
                {
                    Console.WriteLine(wordRange.ToString());
                    wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                }
            }
        }




        public static IEnumerable<TextRange> GetAllWordRanges(FlowDocument document)
        {
            string pattern = @"[^\W\d](\w|[-']{1,2}(?=\w))*";
            TextPointer pointer = document.ContentStart;
            while (pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    MatchCollection matches = Regex.Matches(textRun, pattern);
                    foreach (Match match in matches)
                    {
                        int startIndex = match.Index;
                        int length = match.Length;
                        TextPointer start = pointer.GetPositionAtOffset(startIndex);
                        TextPointer end = start.GetPositionAtOffset(length);
                        yield return new TextRange(start, end);
                    }
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //tree viewer
        

        public void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;
            item.Foreground = Brushes.White;
            if ((item.Items.Count == 1) && (item.Items[0] is string))
            {
                item.Items.Clear();

                DirectoryInfo expandedDir = null;
                if (item.Tag is DriveInfo)
                    expandedDir = (item.Tag as DriveInfo).RootDirectory;
                if (item.Tag is DirectoryInfo)
                    expandedDir = (item.Tag as DirectoryInfo);
                try
                {
                    foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
                        item.Items.Add(CreateTreeItem(subDir));
                        item.Foreground = Brushes.White;
                }
                catch { }
            }
        }
        
        private TreeViewItem CreateTreeItem(object o)
        {
            TreeViewItem item = new TreeViewItem();
            item.Foreground = Brushes.White;
            item.Header = o.ToString();
            item.Tag = o;
            item.Items.Add("Loading...");
            return item;
        }

        //////ColorSyntax
        
        

    }
}
