using Microsoft.Win32;
using ScintillaNET;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


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

        public int tab = 1;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;



        internal Color MediaColor(System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }


        public MainWindow()
        {
            InitializeComponent();

            scintilla.Styles[ScintillaNET.Style.Default].ForeColor = System.Drawing.Color.FromArgb(255, 230, 230, 225);
            scintilla.Styles[ScintillaNET.Style.Default].BackColor = System.Drawing.Color.FromArgb(255, 30, 30, 30);
            scintilla.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            scintilla.Styles[ScintillaNET.Style.Default].Size = 14;
            scintilla.StyleClearAll();



            scintilla.Margins[0].Width = 35;
            scintilla.Margins[0].Sensitive = true;

            scintilla.Styles[ScintillaNET.Style.LineNumber].ForeColor = System.Drawing.Color.FromArgb(255, 43, 145, 175);
            scintilla.Styles[ScintillaNET.Style.LineNumber].BackColor = System.Drawing.Color.FromArgb(255, 30, 30, 30);
            scintilla.ScrollWidthTracking = true;

            scintilla.CaretForeColor = System.Drawing.Color.FromArgb(255, 230, 230, 225);
           
            

        }


        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MenuItem_Open(sender, e);
        }
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save(sender, e);
        }
        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MenuItem_SaveAs(sender, e);
        }
        private void EncryptCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void EncryptCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void DecryptCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DecryptCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void SendFeedbackButton(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/RiadZX/ProjectCheetos/issues");
            }
            catch
            {
                MessageBox.Show("Cant open url");
            }

        }


        private void UndoButton(object sender, RoutedEventArgs e)
        {
            try
            {
                scintilla.Undo();
            }
            catch
            {
                MessageBox.Show("Cant undo");
            }

        }
        private void RedoButton(object sender, RoutedEventArgs e)
        {
            try
            {
                scintilla.Redo();
            }
            catch
            {
                MessageBox.Show("Cant undo");
            }

        }
        private void CopyButton(object sender, RoutedEventArgs e)
        {
            try
            {
                scintilla.Copy();
            }
            catch
            {
                MessageBox.Show("Cant copy");
            }

        }
        private void PasteButton(object sender, RoutedEventArgs e)
        {
            try
            {
                scintilla.Paste();
                
            }
            catch
            {
                MessageBox.Show("Cant paste");
            }

        }
        private void CutButton(object sender, RoutedEventArgs e)
        {
            try
            {
                scintilla.Cut();

            }
            catch
            {
                MessageBox.Show("Cant cut");
            }

        }
        private void SelectAllButton(object sender, RoutedEventArgs e)
        {
            try
            {
                scintilla.SelectAll();

            }
            catch
            {
                MessageBox.Show("Cant select all");
            }

        }



        private int maxLineNumberCharLength;
        private void scintilla_TextChanged(object sender, EventArgs e)
        {
            // Did the number of characters in the line number display change?
            // i.e. nnn VS nn, or nnnn VS nn, etc...
            var maxLineNumberCharLength = scintilla.Lines.Count.ToString().Length;
            if (maxLineNumberCharLength == this.maxLineNumberCharLength)
                return;

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            const int padding = 10;
            scintilla.Margins[0].Width = scintilla.TextWidth(ScintillaNET.Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) + padding;
            this.maxLineNumberCharLength = maxLineNumberCharLength;
        }


        private void UpdateLineNumbers(int startingAtLine)
        {
            // Starting at the specified line index, update each
            // subsequent line margin text with a hex line number.
            for (int i = startingAtLine; i < scintilla.Lines.Count; i++)
            {
                scintilla.Lines[i].MarginStyle = ScintillaNET.Style.LineNumber;
                scintilla.Lines[i].MarginText = "0x" + i.ToString("X2");
            }
        }

        private void scintilla_Insert(object sender, ModificationEventArgs e)
        {
            // Only update line numbers if the number of lines changed
            if (e.LinesAdded != 0)
                UpdateLineNumbers(scintilla.LineFromPosition(e.Position));
        }

        private void scintilla_Delete(object sender, ModificationEventArgs e)
        {
            // Only update line numbers if the number of lines changed
            if (e.LinesAdded != 0)
                UpdateLineNumbers(scintilla.LineFromPosition(e.Position));
        }

        private void Python()
        {
            // Reset the styles
            scintilla.StyleResetDefault();
            scintilla.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            scintilla.Styles[ScintillaNET.Style.Default].Size = 10;
            scintilla.StyleClearAll(); // i.e. Apply to all

            // Set the lexer
            scintilla.Lexer = Lexer.Python;

            // Known lexer properties:
            // "tab.timmy.whinge.level",
            // "lexer.python.literals.binary",
            // "lexer.python.strings.u",
            // "lexer.python.strings.b",
            // "lexer.python.strings.over.newline",
            // "lexer.python.keywords2.no.sub.identifiers",
            // "fold.quotes.python",
            // "fold.compact",
            // "fold"

            // Some properties we like
            scintilla.SetProperty("tab.timmy.whinge.level", "1");
            scintilla.SetProperty("fold", "1");

            // Use margin 2 for fold markers
            scintilla.Margins[2].Type = MarginType.Symbol;
            scintilla.Margins[2].Mask = Marker.MaskFolders;
            scintilla.Margins[2].Sensitive = true;
            scintilla.Margins[2].Width = 20;

            // Reset folder markers
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
            {
                scintilla.Markers[i].SetForeColor(System.Drawing.SystemColors.ControlLightLight);
                scintilla.Markers[i].SetBackColor(System.Drawing.SystemColors.ControlDark);
            }

            // Style the folder markers
            scintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            scintilla.Markers[Marker.Folder].SetBackColor(System.Drawing.SystemColors.ControlText);
            scintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            scintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            scintilla.Markers[Marker.FolderEnd].SetBackColor(System.Drawing.SystemColors.ControlText);
            scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            scintilla.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            scintilla.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

            // Set the styles
            scintilla.Styles[ScintillaNET.Style.Python.Default].ForeColor = System.Drawing.Color.FromArgb(0x80, 0x80, 0x80);
            scintilla.Styles[ScintillaNET.Style.Python.CommentLine].ForeColor = System.Drawing.Color.FromArgb(0x00, 0x7F, 0x00);
            scintilla.Styles[ScintillaNET.Style.Python.CommentLine].Italic = true;
            scintilla.Styles[ScintillaNET.Style.Python.Number].ForeColor = System.Drawing.Color.FromArgb(0x00, 0x7F, 0x7F);
            scintilla.Styles[ScintillaNET.Style.Python.String].ForeColor = System.Drawing.Color.FromArgb(0x7F, 0x00, 0x7F);
            scintilla.Styles[ScintillaNET.Style.Python.Character].ForeColor = System.Drawing.Color.FromArgb(0x7F, 0x00, 0x7F);
            scintilla.Styles[ScintillaNET.Style.Python.Word].ForeColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x7F);
            scintilla.Styles[ScintillaNET.Style.Python.Word].Bold = true;
            scintilla.Styles[ScintillaNET.Style.Python.Triple].ForeColor = System.Drawing.Color.FromArgb(0x7F, 0x00, 0x00);
            scintilla.Styles[ScintillaNET.Style.Python.TripleDouble].ForeColor = System.Drawing.Color.FromArgb(0x7F, 0x00, 0x00);
            scintilla.Styles[ScintillaNET.Style.Python.ClassName].ForeColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF);
            scintilla.Styles[ScintillaNET.Style.Python.ClassName].Bold = true;
            scintilla.Styles[ScintillaNET.Style.Python.DefName].ForeColor = System.Drawing.Color.FromArgb(0x00, 0x7F, 0x7F);
            scintilla.Styles[ScintillaNET.Style.Python.DefName].Bold = true;
            scintilla.Styles[ScintillaNET.Style.Python.Operator].Bold = true;
            // scintilla.Styles[Style.Python.Identifier] ... your keywords styled here
            scintilla.Styles[ScintillaNET.Style.Python.CommentBlock].ForeColor = System.Drawing.Color.FromArgb(0x7F, 0x7F, 0x7F);
            scintilla.Styles[ScintillaNET.Style.Python.CommentBlock].Italic = true;
            scintilla.Styles[ScintillaNET.Style.Python.StringEol].ForeColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x00);
            scintilla.Styles[ScintillaNET.Style.Python.StringEol].BackColor = System.Drawing.Color.FromArgb(0xE0, 0xC0, 0xE0);
            scintilla.Styles[ScintillaNET.Style.Python.StringEol].FillLine = true;
            scintilla.Styles[ScintillaNET.Style.Python.Word2].ForeColor = System.Drawing.Color.FromArgb(0x40, 0x70, 0x90);
            scintilla.Styles[ScintillaNET.Style.Python.Decorator].ForeColor = System.Drawing.Color.FromArgb(0x80, 0x50, 0x00);

            // Important for Python
            scintilla.ViewWhitespace = WhitespaceMode.VisibleAlways;

            // Keyword lists:
            // 0 "Keywords",
            // 1 "Highlighted identifiers"

            var python2 = "and as assert break class continue def del elif else except exec finally for from global if import in is lambda not or pass print raise return try while with yield";
            var python3 = "False None True and as assert break class continue def del elif else except finally for from global if import in is lambda nonlocal not or pass raise return try while with yield";
            var cython = "cdef cimport cpdef";

            scintilla.SetKeywords(0, python2 + " " + cython);
            // scintilla.SetKeywords(1, "add your own keywords here");
        }

        private void CSHighlighting()
        {
            // Configuring the default style with properties
            // we have common to every lexer style saves time.
            scintilla.Styles[ScintillaNET.Style.Default].Font = "Consolas";

            scintilla.Styles[ScintillaNET.Style.Default].Size = 16;
            scintilla.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            scintilla.Styles[ScintillaNET.Style.Cpp.Comment].ForeColor = System.Drawing.Color.FromArgb(0, 128, 0); // Green
            scintilla.Styles[ScintillaNET.Style.Cpp.CommentLine].ForeColor = System.Drawing.Color.FromArgb(0, 128, 0); // Green
            scintilla.Styles[ScintillaNET.Style.Cpp.CommentLineDoc].ForeColor = System.Drawing.Color.FromArgb(128, 128, 128); // Gray
            scintilla.Styles[ScintillaNET.Style.Cpp.Number].ForeColor = System.Drawing.Color.FromArgb(255, 181, 204, 146);
            scintilla.Styles[ScintillaNET.Style.Cpp.Word].ForeColor = System.Drawing.Color.FromArgb(255, 50, 86, 201);
            scintilla.Styles[ScintillaNET.Style.Cpp.Word2].ForeColor = System.Drawing.Color.FromArgb(255, 0, 61, 255);
            scintilla.Styles[ScintillaNET.Style.Cpp.String].ForeColor = System.Drawing.Color.FromArgb(255, 220, 255, 23); // Red
            scintilla.Styles[ScintillaNET.Style.Cpp.Character].ForeColor = System.Drawing.Color.FromArgb(255, 220, 255, 23); // Red
            scintilla.Styles[ScintillaNET.Style.Cpp.Verbatim].ForeColor = System.Drawing.Color.FromArgb(255, 189, 170, 0); // Red
            scintilla.Styles[ScintillaNET.Style.Cpp.StringEol].BackColor = System.Drawing.Color.FromArgb(255, 117, 138, 0);
            scintilla.Styles[ScintillaNET.Style.Cpp.Operator].ForeColor = System.Drawing.Color.FromArgb(255, 88, 255, 51);
            scintilla.Styles[ScintillaNET.Style.Cpp.Preprocessor].ForeColor = System.Drawing.Color.FromArgb(255, 230, 107, 255);
            scintilla.Lexer = Lexer.Cpp;

            // Set the keywords

            scintilla.SetKeywords(0, "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
            scintilla.SetKeywords(1, "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");
        }




        private void MenuItem_SaveAs(object sender, RoutedEventArgs e)

        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Python file (*.py)|*.py|Text file (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, scintilla.Text);
            openedfile = saveFileDialog.FileName;
            this.Title = System.IO.Path.GetFileName(openedfile) + " | Cheetos IDE";
        }




        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {



            OpenFileDialog openFileDialog = new OpenFileDialog();
            var dialogResult = openFileDialog.ShowDialog();
            openFileDialog.Filter = "Python file (*.py)|*.py|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (dialogResult == true)
            {
                scintilla.ClearAll();


                scintilla.AppendText(File.ReadAllText(openFileDialog.FileName));
                openedfile = openFileDialog.FileName;
                this.Title = System.IO.Path.GetFileName(openedfile) + " | Cheetos IDE";
                //Console.WriteLine(openedfile);

            }
            else
            {
                MessageBox.Show("Oh. No file selected!");
            }


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
                    File.WriteAllText(saveFileDialog.FileName, scintilla.Text);
                openedfile = saveFileDialog.FileName;
                this.Title = System.IO.Path.GetFileName(openedfile) + " | Cheetos IDE";
            }
            else
            {
                //File.WriteAllText(openedfile, StringFromRichTextBox(TextField));
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
            try
            {
                if(scintilla.SelectedText == String.Empty)
                {
                    scintilla.SelectAll();
                    string alltext = scintilla.SelectedText;
                   
                    scintilla.ReplaceSelection(Encrypt(scintilla.SelectedText, "chongoslol"));
                }
                else
                {
                    string encryptedtext = Encrypt(scintilla.SelectedText, "chongoslol");
                    scintilla.ReplaceSelection(encryptedtext);
                }
               
            }
            catch
            {
                MessageBox.Show("Cant encrypt this");
            }

        }
        private void DecryptButton(object sender, RoutedEventArgs e)
        {
            try
            {
                if (scintilla.SelectedText == String.Empty)
                {
                    scintilla.SelectAll();
                    string alltext = scintilla.SelectedText;

                    scintilla.ReplaceSelection(Decrypt(scintilla.SelectedText, "chongoslol"));
                }
                else
                {
                    string encryptedtext = Decrypt(scintilla.SelectedText, "chongoslol");
                    scintilla.ReplaceSelection(encryptedtext);
                }
            }
            catch
            {
                MessageBox.Show("Cant decrypt this");
            }
        }

    }

   
}