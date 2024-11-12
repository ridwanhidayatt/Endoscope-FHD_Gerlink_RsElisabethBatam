using AForge.Controls;
using AForge.Imaging.Filters;
//using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using PictureBox = System.Windows.Forms.PictureBox;
using CsvHelper;

namespace WindowsFormsApp1
{
    public partial class Form1Print : Form
    {
        //string dir = @"D:\";
        string dirLogo = @"D:\GLEndoscope\LogoKOP\";
        string csvFilePath = "D:\\GLEndoscope\\Database\\dataPasien\\dataDefault.csv";

        public delegate void TransfDelegate(String value);
        public event TransfDelegate TransfEventt;
        public event TransfDelegate TransfEvenPrint4;
        public event TransfDelegate TransfEvenPrint6;
        public event TransfDelegate TransfEventPrint1G;
        string jam, Code, Date, action, tanggal, id, gabung1, gabung, splitBulan, splitTahun, noRM, nameFix, selectedDate, tanggal1, monthName, year;
        string logoValue, jenisValue;
        private Dictionary<PictureBox, PictureBoxControls> pictureBoxControls = new Dictionary<PictureBox, PictureBoxControls>();


        public Form1Print()
        {
            InitializeComponent();
            FillListBox();
            comboBox1.SelectedIndex = -1; // Ensure no printer is selected by default
            InitializeThumbnails();
            InitializeMainPictureBoxes();

            comboBox1.KeyPress += new KeyPressEventHandler(ComboBox_KeyPress);
            comboBox2.KeyPress += new KeyPressEventHandler(ComboBox_KeyPress);
            comboBox3.KeyPress += new KeyPressEventHandler(ComboBox_KeyPress); 

            //PopulatePrinterComboBox();
            // Kosongkan ComboBox

            // Kosongkan ComboBox dengan mengatur SelectedIndex ke -1
            comboBox1.SelectedIndex = -1;

            // Atau, kosongkan ComboBox dengan mengatur SelectedItem atau Text menjadi string kosong
            //comboBox1.SelectedItem = null;
            //comboBox1.Text = string.Empty;

            // Inisialisasi pencarian folder dan nama target
            string baseFolder = @"D:\GLEndoscope";  // Base folder utama
            string targetName = gabung;
            LoadDatesToComboBox(baseFolder, targetName);

        }

        private void LoadDatesToComboBox(string baseFolder, string targetName)
        {
            // Cari semua subfolder berdasarkan pola nama target
            var directories = Directory.GetDirectories(baseFolder, "*", SearchOption.AllDirectories)
                .Where(dir => dir.Contains(targetName))
                .ToList();

            // List untuk menampung tanggal-tanggal yang valid
            List<string> tanggalList = new List<string>();

            // Ekstrak tanggal dari path dan tambahkan ke List
            foreach (var dir in directories)
            {
                // Ambil folder di atas yang mengandung nama target
                string parentFolder = Directory.GetParent(dir).Name;

                // Cek apakah folder tersebut berbentuk tanggal (8 karakter dan semuanya angka)
                if (parentFolder.Length == 8 && parentFolder.All(char.IsDigit))
                {
                    // Tambahkan hanya tanggal ke list
                    tanggalList.Add(parentFolder);
                }
            }

            // Urutkan list berdasarkan tanggal (paling lama ke paling baru)
            var tanggalUrut = tanggalList
                .Select(t => DateTime.ParseExact(t, "ddMMyyyy", null))
                .OrderBy(t => t)
                .Select(t => t.ToString("ddMMyyyy"))
                .ToList();

            // Kosongkan ComboBox dan tambahkan tanggal yang sudah diurutkan
            comboBox4.Items.Clear();
            foreach (var tanggal in tanggalUrut)
            {
                comboBox4.Items.Add(tanggal);
            }
        }

        public class PictureBoxControls
        {
            public Control CloseControl { get; set; }
            public Control AddControl { get; set; }
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }



        private void InitializeThumbnails()
        {
            // Call the method to read data from the CSV file
            ReadDataFromCSV(csvFilePath);

            // Ambil tanggal saat ini
            //tanggal = DateTime.Now.ToString("ddMMyyyy");

            //string text = DateTime.Now.ToString("Y");
            //textBox5.Text = text;
            //string[] arr = text.Split(' ');
            //splitBulan = arr[0];
            //splitTahun = arr[1];

            // Gabungkan string menjadi path lengkap
            string folderPath = $@"D:\GLEndoscope\{year}\{monthName}\{tanggal1}\{gabung}\Image";

            if (Directory.Exists(folderPath))
            {
                string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);

                foreach (string file in imageFiles)
                {
                    try
                    {
                        Image image = Image.FromFile(file);
                        PictureBox thumbnail = new PictureBox
                        {
                            Image = ResizeImage(image, 169, 102), // Adjusted size of the image by reducing an additional 5%
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Size = new Size(161, 89), // Adjusted size of PictureBox to avoid horizontal scroll
                            Margin = new Padding(5),
                            Tag = file
                        };

                        // Tambahkan event handler untuk mengubah visibilitas


                        thumbnail.MouseDown += Thumbnail_MouseDown;
                        flowLayoutPanel1.Controls.Add(thumbnail);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image {file}: {ex.Message}");
                    }
                }
            }
            else
            {
                //MessageBox.Show("Folder not found.");
            }
        }



        private void Thumbnail_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox thumbnail = sender as PictureBox;
            if (thumbnail != null)
            {
                thumbnail.DoDragDrop(thumbnail.Tag, DragDropEffects.Copy);
            }
        }



        private void InitializeMainPictureBoxes()
        {
            // Daftar semua PictureBox yang akan digunakan
            PictureBox[] pictureBoxes = { pictureBox1 };

            // Inisialisasi kontrol untuk setiap PictureBox
            for (int i = 0; i < pictureBoxes.Length; i++)
            {
                PictureBox pictureBox = pictureBoxes[i];

                // Temukan kontrol close dan add dengan nama yang sesuai
                var closeControl = this.Controls.Find($"close{i + 1}", true).FirstOrDefault();
                var addControl = this.Controls.Find($"add{i + 1}", true).FirstOrDefault();

                pictureBoxControls[pictureBox] = new PictureBoxControls
                {
                    CloseControl = closeControl,
                    AddControl = addControl
                };

                pictureBox.AllowDrop = true;
                pictureBox.DragEnter += PictureBox_DragEnter;
                pictureBox.DragDrop += PictureBox_DragDrop;
            }
        }


        private void PictureBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void PictureBox_DragDrop(object sender, DragEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            if (pictureBox != null)
            {
                if (e.Data.GetDataPresent(DataFormats.StringFormat))
                {
                    string filePath = e.Data.GetData(DataFormats.StringFormat) as string;
                    if (filePath != null && File.Exists(filePath))
                    {
                        PictureBoxControls controls;
                        if (pictureBoxControls.TryGetValue(pictureBox, out controls))
                        {
                            if (controls.CloseControl != null) controls.CloseControl.Visible = true;
                            if (controls.AddControl != null) controls.AddControl.Visible = false;

                            btn_Delete.Visible = true;
                            pictureBox.Image = Image.FromFile(filePath);
                        }
                    }
                }
            }
        }







        private void button4_Click(object sender, EventArgs e)
        { 
            comboBox2.SelectedIndex = -1; 
            pictureBox1.Image = Properties.Resources.icon; 
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            comboBox1.Items.Clear();
            comboBox2.SelectedIndex = -1;
            comboBox1.ResetText();
            FillListBox();
            int kondisi = 9;
            TransfEventt(kondisi.ToString()); 
            this.Close(); 
        }

        

        void FillListBox()
        {
            foreach (var p in PrinterSettings.InstalledPrinters)
            {
                comboBox1.Items.Add(p);
            }
        }
        
        public static class printer
        {
            [DllImport("winspool.drv",
              CharSet = CharSet.Auto,
              SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern Boolean SetDefaultPrinter(String name);
        }

        //private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    //comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        //    string Pname = comboBox1.SelectedItem.ToString();
        //    printer.SetDefaultPrinter(Pname);

        //    if (comboBox1.Text == "Canon SELPHY CP1300")
        //    {
        //        int print1 = 1;
        //        textBox2.Text = print1.ToString();
        //        //MessageBox.Show("1");
        //    }
        //    else
        //    {
        //        int print2 = 2;
        //        textBox2.Text = print2.ToString();
        //    }
        //}

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            string Pname = comboBox1.SelectedItem.ToString();
            printer.SetDefaultPrinter(Pname);

            if (comboBox1.Text == "Canon SELPHY CP1300")
            {
                //if (comboBox2.Items.Contains("A4"))
                //{
                //    comboBox2.Items.Remove("A4");
                //}

                // Sembunyikan elemen-elemen UI tertentu
                picLogo1.Visible = false;
                pictureBox2.Visible = false;
                label1.Visible = false;
                label2.Visible = false;

                int print1 = 1;
                textBox2.Text = print1.ToString();
            }
            else
            {
                //if (!comboBox2.Items.Contains("A4"))
                //{
                //    comboBox2.Items.Add("A4");
                //}
                // Tampilkan kembali elemen-elemen UI tersebut jika pilihan printer berbeda
                picLogo1.Visible = true;
                label1.Visible = true;
                label2.Visible = true;

                int print2 = 2;
                textBox2.Text = print2.ToString();
            }
        }


        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            if (comboBox2.Text == "4R")
            {
                int print1 = 1;
                textBox6.Text = print1.ToString();
            }
            else
            {
                int print2 = 2;
                textBox6.Text = print2.ToString();
            }
            
        }
        private void ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; // Mencegah karakter yang diketik ditampilkan di ComboBox
        }



        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            //buttonAdd.Visible = true;
            btn_Delete.Visible = false;
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            if (panel4.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 2;//it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.Black, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness, halfThickness, panel4.ClientSize.Width - thickness, panel4.ClientSize.Height - thickness));
                }
            }
        } 

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;

            tanggal = DateTime.Now.ToString("ddMMyyy");
            string text = DateTime.Now.ToString("Y");
            string[] arr = text.Split(' ');
            splitBulan = arr[0];
            splitTahun = arr[1];

            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";
            of.InitialDirectory = "D:\\GLEndoscope\\" + splitTahun + "\\" + splitBulan + "\\" + tanggal + "\\" + gabung + "\\Image";
            if (of.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.ImageLocation = of.FileName;
                btn_Delete.Visible = true;
                //buttonAdd.Visible = false;

            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Foto diisi Dahulu ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (comboBox2.Text == "4R" || comboBox2.Text == "A4")
                {
                    int s = 1;
                    int r = 2;
                    if (textBox6.Text == s.ToString())
                    {
                        savePDF4R();
                        textBox6.Clear();
                        comboBox2.SelectedIndex = -1;
                        comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                        //FillListBox();
                        buttobDeleteFalse();
                        //buttobAddTrue();

                        button4.PerformClick();
                        int kondisi1 = 1;
                        TransfEventPrint1G(kondisi1.ToString());
                        MessageBox.Show("Ekspor PDF berhasil", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (textBox6.Text == r.ToString())
                    {
                        savePDFA4();
                        textBox6.Clear();
                        comboBox2.SelectedIndex = -1;
                        comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                        //FillListBox();
                        buttobDeleteFalse();
                        //buttobAddTrue();

                        button4.PerformClick();
                        int kondisi1 = 1;
                        TransfEventPrint1G(kondisi1.ToString());
                        MessageBox.Show("Ekspor PDF berhasil", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                } 
                else
                {
                    MessageBox.Show("Pilih ukuran kertas", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            } 
        }

        private void button3_Click(object sender, EventArgs e)
        { 
            comboBox2.SelectedIndex = -1; 
            pictureBox1.Image = Properties.Resources.icon;  
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null; 
            comboBox1.Items.Clear();
            comboBox1.ResetText();
            //FillListBox();
            int kondisi1 = 2;
            TransfEvenPrint4(kondisi1.ToString()); 
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        { 
            comboBox2.SelectedIndex = -1; 
            pictureBox1.Image = Properties.Resources.icon; 
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null; 
            comboBox1.Items.Clear();
            comboBox1.ResetText();
            //FillListBox();
            int kondisi1 = 3;
            TransfEvenPrint6(kondisi1.ToString()); 
            this.Close();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

            if (textBox3.Text != "")
            {
                // Specify the path for the CSV file
                //string csvFilePath = "D:\\GLEndoscope\\Database\\dataPasien\\dataDefault.csv";

                // Call the method to read data from the CSV file
                ReadDataFromCSV(csvFilePath);
                LoadAndSetValues();
                textBox3.Clear(); 
            }
        }

        private void LoadAndSetValues()
        {
            string filePath = @"D:\GLEndoscope\LogoKOP\logo.xml";

            try
            {
                // Check if the file exists
                if (File.Exists(filePath))
                {
                    // Load the XML document from the file
                    XDocument xd = XDocument.Load(filePath);

                    // Retrieve the logo value from the XML document
                    logoValue = xd.Element("userdata")?.Element("logo")?.Value;


                    // Set the logo value to the TextBox
                    //textBoxLogo.Text = logoValue;

                    // Retrieve the jenis value from the XML document
                    jenisValue = xd.Element("userdata")?.Element("jenis")?.Value;


                    // Check the logoValue and show/hide PictureBox controls accordingly
                    if (logoValue == "1")
                    {
                        picLogo1.Visible = true;
                        pictureBox2.Visible = false;
                    }
                    else if (logoValue == "2")
                    {
                        picLogo1.Visible = true;
                        pictureBox2.Visible = true;
                    }
                    else
                    {
                        // Handle other cases if needed
                        picLogo1.Visible = false;
                        pictureBox2.Visible = false;
                    }

                    // Check the jenisValue and adjust the size of PictureBox controls accordingly
                    if (jenisValue == "Persegi")
                    {
                        picLogo1.Size = new Size(100, 100);
                        pictureBox2.Size = new Size(100, 100);

                        //label1.Size = new Size(538, 23);
                        //label1.Location = new Point(164, 27);

                        //label2.Size = new Size(613, 23);
                        //label2.Location = new Point(127, 50);

                    }
                    else if (jenisValue == "Persegi Panjang")
                    {
                        picLogo1.Size = new Size(200, 100);
                        pictureBox2.Size = new Size(200, 100);
                        pictureBox2.Location = new Point(665, 9);

                        //label1.Size = new Size(343, 23);
                        //label1.Location = new Point(262, 27);

                        //label2.Size = new Size(418, 23);
                        //label2.Location = new Point(225, 50);
                    }
                }
                else
                {
                    MessageBox.Show("The XML file does not exist.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        //private void ReadDataFromCSV(string filePath)
        //{
        //    try
        //    {
        //        // Read all lines from the CSV file
        //        string[] lines = File.ReadAllLines(filePath);

        //        // Check if there are any lines in the file
        //        if (lines.Length < 2)
        //        {
        //            MessageBox.Show("The CSV file is empty or does not contain the expected header.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        // Extract column names from the header
        //        string[] headers = lines[0].Split(',');

        //        // Output header names for debugging
        //        //MessageBox.Show($"Header names: {string.Join(", ", headers)}", "Debugging", MessageBoxButtons.OK, MessageBoxIcon.Information);

        //        // Find the indices of the desired columns 
        //        int noRMIndex = Array.IndexOf(headers, "Rm");
        //        int nameIndex = Array.IndexOf(headers, "Nama");
        //        int actionIndex = Array.IndexOf(headers, "JenisPemeriksaan");
        //        int dateIndex = Array.IndexOf(headers, "Tanggal Kunjungan");

        //        // Check if all required columns are present in the CSV file
        //        //if (noRMIndex == -1 || nameIndex == -1 || actionIndex == -1 || dateIndex == -1)
        //        //{
        //        //    MessageBox.Show("One or more required columns not found in the CSV file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        //    return;
        //        //}

        //        // Extract data from the first row (assuming it's the only row based on your MySQL query)
        //        string[] values = lines[1].Split(',');

        //        // Use extracted data as needed 
        //        noRM = values[noRMIndex].Trim();
        //        Name = values[nameIndex].Trim();
        //        action = values[actionIndex].Trim();
        //        Date = values[dateIndex].Trim();

        //        DateTime today = DateTime.Now;
        //        jam = today.ToString("hhmmss");
        //        nameFix = GetFirstTwoWords(Name);
        //        string varia = nameFix + " - " + noRM + " - " + Date;
        //        label1.Text = action;
        //        label2.Text = varia;
        //        gabung = noRM + "-" + Name;
        //        gabung1 = noRM + "-" + Name + "-" + jam;

        //        // Now you can use the extracted data as needed
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }

        //}


        private void ReadDataFromCSV(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader))
                {
                    // Baca record dari file CSV satu per satu
                    while (csv.Read())
                    {
                        // Ambil data dari record saat ini
                        var noRMIndex = csv.GetField<string>("Rm");
                        var nameIndex = csv.GetField<string>("Nama");
                        var actionIndex = csv.GetField<string>("Jenis Pemeriksaan");
                        var dateIndex = csv.GetField<string>("Tanggal Kunjungan");
                        var tanggalLahir = csv.GetField<string>("Tanggal Lahir");
                        var umur = csv.GetField<string>("Umur");
                        var alamat = csv.GetField<string>("Alamat");
                        var dokterNama = csv.GetField<string>("Dokter");



                        DateTime today = DateTime.Now;
                        jam = today.ToString("hhmmss");
                        nameFix = GetFirstTwoWords(nameIndex);
                        string varia = nameFix + " - " + noRMIndex + " - " + dateIndex;
                        label1.Text = actionIndex;
                        label2.Text = varia;
                        gabung = noRMIndex + "-" + nameIndex;
                        gabung1 = noRMIndex + "-" + nameIndex + "-" + jam;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tidak ada data yang tersedia. Mohon isi data Pasien terlebih dahulu.", "Informasi!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();

            // Ketika user memilih tanggal dari ComboBox
            selectedDate = comboBox4.SelectedItem.ToString(); // Mengambil nilai yang dipilih 

            // Sekarang Anda bisa menggunakan selectedDate sebagai pengganti DateTime.Now
            tanggal1 = selectedDate;

            // Pisahkan tanggal, bulan, dan tahun menggunakan Substring
            string day = selectedDate.Substring(0, 2);   // Bagian tanggal (16)
            string monthNumber = selectedDate.Substring(2, 2); // Bagian bulan (10)
            year = selectedDate.Substring(4, 4);  // Bagian tahun (2024)

            // Array nama-nama bulan dalam bahasa Inggris
            string[] monthNames = {
                 "January", "February", "March", "April", "May", "June",
                 "July", "August", "September", "October", "November", "December"
             };

            // Konversi bulan dari angka ke nama bulan
            int monthIndex = int.Parse(monthNumber) - 1; // Mengonversi ke indeks (0-11)
            monthName = monthNames[monthIndex];

            InitializeThumbnails();
        }

        private string GetFirstTwoWords(string inputText)
        {
            string[] words = inputText.Split(' ');

            if (words.Length >= 2)
            {
                return words[0] + " " + words[1];
            }
            else if (words.Length == 1)
            {
                return words[0];
            }
            else
            {
                return string.Empty;
            }
        }
        void otherForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Show();
        }

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    if (pictureBox1.Image == null)
        //    {
        //        MessageBox.Show("Foto diisi Dahulu ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //    else
        //    {
        //        comboBox2.SelectedIndex = -1;  // Reset comboBox2 selection
        //        if (comboBox1.SelectedIndex == -1)
        //        {
        //            MessageBox.Show("Pilih printer terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        }
        //        else
        //        {
        //            if (comboBox3.SelectedIndex == -1)
        //            {
        //                MessageBox.Show("Pilih profil terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            }
        //            else
        //            {
        //                PrintDocument pd = new PrintDocument();

        //                // Menentukan printer yang dipilih dari ComboBox
        //                pd.PrinterSettings.PrinterName = comboBox1.SelectedItem.ToString();

        //                if (textBox2.Text == "1")
        //                {
        //                    // Setting up for the specific profile
        //                    if (comboBox3.Text == "Default")
        //                    {
        //                        pd.PrintPage += new PrintPageEventHandler(this.printDocument1_PrintPage);
        //                    }
        //                    else if (comboBox3.Text == "Adjust Brightness")
        //                    {
        //                        pd.PrintPage += new PrintPageEventHandler(this.printDocument4_PrintPage);
        //                    }
        //                    pd.Print();
        //                    //printPreviewDialog1.Document = pd;
        //                    //printPreviewDialog1.ShowDialog();
        //                    HistoryPrint4R(comboBox3.Text); // Menambahkan HistoryPrint4R yang sesuai
        //                }
        //                else
        //                {
        //                    // Set kertas menjadi A4
        //                    pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);

        //                    if (comboBox3.Text == "Default")
        //                    {
        //                        pd.PrintPage += new PrintPageEventHandler(this.printDocument2_PrintPage);
        //                    }
        //                    else if (comboBox3.Text == "Adjust Brightness")
        //                    {
        //                        pd.PrintPage += new PrintPageEventHandler(this.printDocument3_PrintPage);
        //                    }
        //                    pd.Print();
        //                    //printPreviewDialog1.Document = pd;
        //                    //printPreviewDialog1.ShowDialog();
        //                    HistoryPrintA4(comboBox3.Text); // Menambahkan HistoryPrintA4 yang sesuai
        //                }

        //                // Setelah pencetakan berhasil
        //                comboBox1.Items.Clear();
        //                comboBox1.ResetText();
        //                pictureBox1.Image.Dispose();
        //                pictureBox1.Image = null;
        //                buttobDeleteFalse();
        //                buttobAddTrue();
        //                button4.PerformClick();
        //                int kondisi1 = 1;
        //                TransfEventPrint1G(kondisi1.ToString());
        //                MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            }
        //        }
        //    }
        //}


        //private void button2_Click(object sender, EventArgs e)
        //{
        //    if (pictureBox1.Image == null)
        //    {
        //        MessageBox.Show("Foto diisi Dahulu ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //    else if (comboBox1.SelectedIndex == -1)
        //    {
        //        MessageBox.Show("Pilih printer terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //    else if (comboBox3.SelectedIndex == -1)
        //    {
        //        MessageBox.Show("Pilih profil terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //    else
        //    {
        //        PrintDocument pd = new PrintDocument();

        //        // Menentukan printer yang dipilih dari ComboBox
        //        pd.PrinterSettings.PrinterName = comboBox1.SelectedItem.ToString();

        //        if (textBox2.Text == "1")
        //        {
        //            if (comboBox3.Text == "Default")
        //            {
        //                pd.PrintPage += new PrintPageEventHandler(this.printDocument1_PrintPage);
        //            }
        //            else if (comboBox3.Text == "Adjust Brightness")
        //            {
        //                pd.PrintPage += new PrintPageEventHandler(this.printDocument4_PrintPage);
        //            }
        //        }
        //        else
        //        {
        //            // Set kertas menjadi A4
        //            pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);

        //            if (comboBox3.Text == "Default")
        //            {
        //                pd.PrintPage += new PrintPageEventHandler(this.printDocument2_PrintPage);
        //            }
        //            else if (comboBox3.Text == "Adjust Brightness")
        //            {
        //                pd.PrintPage += new PrintPageEventHandler(this.printDocument3_PrintPage);
        //            }
        //        }

        //        try
        //        {
        //            // Melakukan pencetakan
        //            pd.Print();

        //            // Setelah pencetakan berhasil
        //            comboBox1.Items.Clear();
        //            comboBox1.ResetText();
        //            pictureBox1.Image.Dispose();
        //            pictureBox1.Image = null;
        //            buttobDeleteFalse(); // Perhatikan, mungkin maksudnya buttobDeleteFalse()
        //            buttobAddTrue();     // dan buttobAddTrue() dari kode ke-1
        //            button4.PerformClick();
        //            int kondisi1 = 1;
        //            TransfEventPrint1G(kondisi1.ToString());
        //            MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Gagal melakukan pencetakan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}


        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Foto diisi Dahulu ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                //comboBox2.SelectedIndex = -1;

                if (comboBox1.SelectedIndex == -1)
                {
                    MessageBox.Show("Pilih printer terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    int print1 = 1;
                    int print2 = 2;

                    if (textBox2.Text == print1.ToString())
                    {
                        if (comboBox3.SelectedIndex == -1)
                        {
                            MessageBox.Show("Pilih profil terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            // Pilihan printer dari ComboBox
                            string selectedPrinter = comboBox1.SelectedItem.ToString();

                            // Pilih printer default jika ComboBox3 adalah "Default"
                            if (comboBox3.Text == "Default")
                            {
                                PrintDocument pd = new PrintDocument();
                                pd.PrinterSettings.PrinterName = selectedPrinter;
                                pd.PrintPage += new PrintPageEventHandler(this.printDocument1_PrintPage);
                                pd.Print();
                                // Tampilkan pratinjau pencetakan
                                //printPreviewDialog1.Document = pd;
                                //printPreviewDialog1.ShowDialog();

                                // Lakukan tindakan setelah pencetakan
                                HistoryPrint4R(comboBox3.Text);
                                comboBox1.Items.Clear();
                                comboBox1.ResetText();
                                pictureBox1.Image.Dispose();
                                pictureBox1.Image = null;
                                buttobDeleteFalse();
                                FillListBox();
                                //buttobAddTrue();
                                button4.PerformClick();
                                int kondisi1 = 1;
                                TransfEventPrint1G(kondisi1.ToString());
                                MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (comboBox3.Text == "Adjust Brightness")
                            {
                                PrintDocument pd = new PrintDocument();
                                pd.PrinterSettings.PrinterName = selectedPrinter;

                                pd.PrintPage += new PrintPageEventHandler(this.printDocument5_PrintPage);

                                // Langsung mencetak
                                pd.Print();

                                // Lakukan tindakan setelah pencetakan
                                HistoryPrint4R(comboBox3.Text);
                                comboBox1.Items.Clear();
                                comboBox1.ResetText();
                                pictureBox1.Image.Dispose();
                                pictureBox1.Image = null;
                                buttobDeleteFalse();
                            FillListBox();
                                //buttobAddTrue();
                                button4.PerformClick();
                                int kondisi1 = 1;
                                TransfEventPrint1G(kondisi1.ToString());
                                MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    else
                    {
                        if (comboBox3.SelectedIndex == -1)
                        {
                            MessageBox.Show("Pilih profil terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            // Pilihan printer dari ComboBox
                            string selectedPrinter = comboBox1.SelectedItem.ToString();

                            // Pilih printer default jika ComboBox3 adalah "Default"
                            if (comboBox3.Text == "Default")
                            {
                                PrintDocument pd = new PrintDocument();
                                pd.PrinterSettings.PrinterName = selectedPrinter;

                                pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);

                                pd.PrintPage += new PrintPageEventHandler(this.printDocument2_PrintPage);

                                // Langsung mencetak
                                pd.Print();
                                // Tampilkan pratinjau pencetakan
                                //printPreviewDialog1.Document = pd;
                                //printPreviewDialog1.ShowDialog();

                                // Lakukan tindakan setelah pencetakan
                                HistoryPrintA4(comboBox3.Text);
                                comboBox1.Items.Clear();
                                comboBox1.ResetText();
                                pictureBox1.Image.Dispose();
                                pictureBox1.Image = null;
                                buttobDeleteFalse();
                            FillListBox();
                                //buttobAddTrue();
                                button4.PerformClick();
                                int kondisi1 = 1;
                                TransfEventPrint1G(kondisi1.ToString());
                                MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (comboBox3.Text == "Adjust Brightness")
                            {
                                PrintDocument pd = new PrintDocument();
                                pd.PrinterSettings.PrinterName = selectedPrinter;

                                pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);

                                pd.PrintPage += new PrintPageEventHandler(this.printDocument4_PrintPage);

                                pd.Print();
                                //printPreviewDialog1.Document = pd;
                                //printPreviewDialog1.ShowDialog();


                                // Lakukan tindakan setelah pencetakan

                                HistoryPrintA4(comboBox3.Text);
                                comboBox1.Items.Clear();
                                comboBox1.ResetText();
                                pictureBox1.Image.Dispose();
                                pictureBox1.Image = null;
                                buttobDeleteFalse();
                            FillListBox();
                                //buttobAddTrue();
                                button4.PerformClick();
                                int kondisi1 = 1;
                                TransfEventPrint1G(kondisi1.ToString());
                                MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
        }



        private void buttobDeleteFalse()
        {
            btn_Delete.Visible = false;
        }

        //private void buttobAddTrue()
        //{
        //    buttonAdd.Visible = true;
        //}

        private void printDocument4_PrintPage(object sender, PrintPageEventArgs e)
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label1.Text, new Font("Montserrat", 12, FontStyle.Bold), Brushes.Black, 414, 35, sf);
            StringFormat sff = new StringFormat();
            sff.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label2.Text, new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 415, 60, sff);
            //e.Graphics.DrawImage(picLogo1.Image, 5, 0, 100, 100);

            // Check the value of logoValue
            //if (logoValue == "1")
            //{
            //    AdjustPictureBoxSize(e.Graphics, "Persegi");
            //    e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);
            //}
            //else if (logoValue == "2")
            //{
            //    AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
            //    e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);

            //    // Adjust the coordinates based on jenisValue
            //    if (jenisValue == "Persegi")
            //    {
            //        e.Graphics.DrawImage(pictureBox2.Image, 695, 0, pictureBox2.Width, pictureBox2.Height);
            //    }
            //    else if (jenisValue == "Persegi Panjang")
            //    {
            //        e.Graphics.DrawImage(pictureBox2.Image, 595, 0, pictureBox2.Width, pictureBox2.Height);
            //    }
            //}

            if (logoValue == "1")
            {
                //AdjustPictureBoxSize(e.Graphics, "Persegi");
                //e.Graphics.DrawImage(picLogo1.Image, 30, 3, picLogo1.Width, picLogo1.Height);

                if (jenisValue == "Persegi Panjang")
                {
                    // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, jenisValue);
                    e.Graphics.DrawImage(pictureBox2.Image, 5, 0, pictureBox2.Width, pictureBox2.Height);

                    //MessageBox.Show("Persegi Panjang");
                }
                else
                {
                    // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, "Persegi");
                    e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);
                    //MessageBox.Show("Persegi");
                }
            }
            else if (logoValue == "2")
            {
                AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
                e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);

                // Adjust the coordinates based on jenisValue
                if (jenisValue == "Persegi")
                {
                    e.Graphics.DrawImage(pictureBox2.Image, 695, 0, pictureBox2.Width, pictureBox2.Height);
                }
                else if (jenisValue == "Persegi Panjang")
                {
                    e.Graphics.DrawImage(pictureBox2.Image, 595, 0, pictureBox2.Width, pictureBox2.Height);
                }
            }

            //e.Graphics.DrawImage(pictureBox1.Image, 5, 120, 790, 433);

            //Bitmap bmp = new Bitmap(pictureBox1.Image.Width, pictureBox1.Image.Height, PixelFormat.Format32bppArgb);
            //Bitmap bmp2 = new Bitmap(pictureBox3.Image.Width, pictureBox3.Image.Height, PixelFormat.Format32bppArgb);

            //Graphics g = Graphics.FromImage(bmp);

            //Config 1

            //float contrast = 1.02f;
            //float gamma = 0.78f;

            //Config 2

            //float contrast = 1.41f;
            float contrast = 1.00f;
            float gamma = 0.715f;
            float red = 0.56f;
            float green = 0.35f;
            float blue = 0.28f;

            ImageAttributes ia = new ImageAttributes();
            float[][] ptsarray = {
                        new float[] { contrast+red, 0f, 0f, 0f, 0f},
                        new float[] { 0f, contrast+green, 0f, 0f, 0f},
                        new float[] { 0f, 0f, contrast+blue, 0f, 0f},
                        new float[] { 0f, 0f,       0f, 1f, 0f},
                        new float[] {   0, 0,        0, 1f, 1f},
                };
            ia.ClearColorMatrix();
            ia.SetColorMatrix(new ColorMatrix(ptsarray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            ia.SetGamma(gamma, ColorAdjustType.Bitmap);
            e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(5, 120, 790, 433), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
            ia.Dispose();


            //e.Graphics.DrawImage(bmp2, new Rectangle(0, 0, bmp2.Width, bmp2.Height), 0, 0, bmp2.Width, bmp2.Height, GraphicsUnit.Pixel, ia);
            //g.Dispose();
        }

        private void printDocument3_PrintPage(object sender, PrintPageEventArgs e)
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label1.Text, new Font("Montserrat", 12, FontStyle.Bold), Brushes.Black, 414, 35, sf);
            StringFormat sff = new StringFormat();
            sff.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label2.Text, new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 415, 60, sff);
            e.Graphics.DrawImage(picLogo1.Image, 5, 0, 100, 100);

            //e.Graphics.DrawImage(pictureBox1.Image, 5, 120, 790, 433);

            //Bitmap bmp = new Bitmap(pictureBox1.Image.Width, pictureBox1.Image.Height, PixelFormat.Format32bppArgb);
            //Bitmap bmp2 = new Bitmap(pictureBox3.Image.Width, pictureBox3.Image.Height, PixelFormat.Format32bppArgb);

            //Graphics g = Graphics.FromImage(bmp);

            //Config 1

            float contrast = 1.02f;
            float gamma = 0.78f;
            float red = 0.56f;
            float green = 0.35f;
            float blue = 0.28f;

            //Config 2

            //float contrast = 1.41f;
            //float gamma = 0.715f;

            ImageAttributes ia = new ImageAttributes();
            float[][] ptsarray = {
                        new float[] { contrast+red, 0f, 0f, 0f, 0f},
                        new float[] { 0f, contrast+green, 0f, 0f, 0f},
                        new float[] { 0f, 0f, contrast+blue, 0f, 0f},
                        new float[] { 0f, 0f,       0f, 1f, 0f},
                        new float[] {   0, 0,        0, 1f, 1f},
                };
            ia.ClearColorMatrix();
            ia.SetColorMatrix(new ColorMatrix(ptsarray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            ia.SetGamma(gamma, ColorAdjustType.Bitmap);
            e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(5, 120, 790, 433), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
            ia.Dispose();

            //e.Graphics.DrawImage(bmp2, new Rectangle(0, 0, bmp2.Width, bmp2.Height), 0, 0, bmp2.Width, bmp2.Height, GraphicsUnit.Pixel, ia);
            //g.Dispose();
        }

        private void printDocument2_PrintPage(object sender, PrintPageEventArgs e)
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label1.Text, new Font("Montserrat", 12, FontStyle.Bold), Brushes.Black, 414, 35, sf);
            StringFormat sff = new StringFormat();
            sff.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label2.Text, new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 415, 60, sff);
            //e.Graphics.DrawImage(picLogo1.Image, 5, 0, 100, 100);


            //// Check the value of logoValue
            //if (logoValue == "1")
            //{
            //    AdjustPictureBoxSize(e.Graphics, "Persegi");
            //    e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);
            //}
            //else if (logoValue == "2")
            //{
            //    AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
            //    e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);

            //    // Adjust the coordinates based on jenisValue
            //    if (jenisValue == "Persegi")
            //    {
            //        e.Graphics.DrawImage(pictureBox2.Image, 695, 0, pictureBox2.Width, pictureBox2.Height);
            //    }
            //    else if (jenisValue == "Persegi Panjang")
            //    {
            //        e.Graphics.DrawImage(pictureBox2.Image, 595, 0, pictureBox2.Width, pictureBox2.Height);
            //    }
            //}


            if (logoValue == "1")
            {
                //AdjustPictureBoxSize(e.Graphics, "Persegi");
                //e.Graphics.DrawImage(picLogo1.Image, 30, 3, picLogo1.Width, picLogo1.Height);

                if (jenisValue == "Persegi Panjang")
                {
                    // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, jenisValue);
                    e.Graphics.DrawImage(pictureBox2.Image, 5, 0, pictureBox2.Width, pictureBox2.Height);

                    //MessageBox.Show("Persegi Panjang");
                }
                else
                {
                    // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, "Persegi");
                    e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);
                    //MessageBox.Show("Persegi");
                }
            }
            else if (logoValue == "2")
            {
                AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
                e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);

                // Adjust the coordinates based on jenisValue
                if (jenisValue == "Persegi")
                {
                    e.Graphics.DrawImage(pictureBox2.Image, 695, 0, pictureBox2.Width, pictureBox2.Height);
                }
                else if (jenisValue == "Persegi Panjang")
                {
                    e.Graphics.DrawImage(pictureBox2.Image, 595, 0, pictureBox2.Width, pictureBox2.Height);
                }
            }



            e.Graphics.DrawImage(pictureBox1.Image, 5, 120, 790, 433);

        }

        private void printDocument12_PrintPage(object sender, PrintPageEventArgs e)
        {
            //StringFormat sf = new StringFormat(); 
            //sf.Alignment = StringAlignment.Center;
            //e.Graphics.DrawString(label1.Text, new Font("Montserrat", 12, FontStyle.Bold), Brushes.Black, 414, 35, sf);
            //StringFormat sff = new StringFormat(); 
            //sff.Alignment = StringAlignment.Center;
            //e.Graphics.DrawString(label2.Text, new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 415, 60, sff);
            //e.Graphics.DrawImage(picLogo1.Image, 5, 0, 100, 100);
            //e.Graphics.DrawImage(pictureBox1.Image, 5, 120, 790, 433);  

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;

            e.Graphics.DrawString(label1.Text, new Font("Montserrat", 12, FontStyle.Bold), Brushes.Black, 414, 35, sf);

            StringFormat sff = new StringFormat();
            sff.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label2.Text, new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 415, 60, sff);


            // Check the value of logoValue
            if (logoValue == "1")
            {
                //AdjustPictureBoxSize(e.Graphics, "Persegi");
                //e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);

                if (jenisValue == "Persegi Panjang")
                {
                    // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, jenisValue);
                    e.Graphics.DrawImage(pictureBox2.Image, 5, 0, pictureBox2.Width, pictureBox2.Height);

                    //MessageBox.Show("Persegi Panjang");
                }
                else
                {
                    // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, "Persegi");
                    e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);
                    //MessageBox.Show("Persegi");
                }


            }
            else if (logoValue == "2")
            {
                AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
                e.Graphics.DrawImage(picLogo1.Image, 5, 0, picLogo1.Width, picLogo1.Height);

                // Adjust the coordinates based on jenisValue
                if (jenisValue == "Persegi")
                {
                    e.Graphics.DrawImage(pictureBox2.Image, 695, 0, pictureBox2.Width, pictureBox2.Height);
                }
                else if (jenisValue == "Persegi Panjang")
                {
                    e.Graphics.DrawImage(pictureBox2.Image, 595, 0, pictureBox2.Width, pictureBox2.Height);
                }
            }


            e.Graphics.DrawImage(pictureBox1.Image, 25, 120, 790, 433);
        }



        private void AdjustPictureBoxSize(Graphics graphics, string jenis)
        {
            // Check the value of jenis and adjust the size of PictureBox controls accordingly
            if (jenis == "Persegi")
            {
                picLogo1.Size = new Size(100, 100);
                pictureBox2.Size = new Size(100, 100);
            }
            else if (jenis == "Persegi Panjang")
            {
                picLogo1.Size = new Size(200, 100);
                pictureBox2.Size = new Size(200, 100);
            }
            // Add more conditions as needed
        }




        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(pictureBox1.Image, 14, 43, 548, 312);
        }

        private void printDocument5_PrintPage(object sender, PrintPageEventArgs e)
        {
            //e.Graphics.DrawImage(pictureBox1.Image, 14, 43, 548, 312);

            //Config 1

            float contrast = 1.02f;
            float gamma = 0.78f;
            float red = 0.56f;
            float green = 0.35f;
            float blue = 0.28f;

            //Config 2

            //float contrast = 1.41f;
            //float gamma = 0.715f;

            ImageAttributes ia = new ImageAttributes();
            float[][] ptsarray = {
                        new float[] { contrast+red, 0f, 0f, 0f, 0f},
                        new float[] { 0f, contrast+green, 0f, 0f, 0f},
                        new float[] { 0f, 0f, contrast+blue, 0f, 0f},
                        new float[] { 0f, 0f,       0f, 1f, 0f},
                        new float[] {   0, 0,        0, 1f, 1f},
                };
            ia.ClearColorMatrix();
            ia.SetColorMatrix(new ColorMatrix(ptsarray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            ia.SetGamma(gamma, ColorAdjustType.Bitmap);
            e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(14, 43, 548, 312), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
            ia.Dispose();
        }

        private void Form1Print_Load(object sender, EventArgs e)
        {
            this.ActiveControl = label5;
            string dirlogo1 = dirLogo + "logo1.png";
            //string dirlogo1 = dir + "1160358.png";
            if (!Directory.Exists(dirlogo1))
            {
                picLogo1.Image = Image.FromFile(dirLogo + "logo1.png");
                pictureBox2.Image = Image.FromFile(dirLogo + "logo2.png");
            }

            buttobDeleteFalse();

            if (comboBox4.Items.Count > 0) // Pastikan ComboBox memiliki item
            {
                comboBox4.SelectedIndex = comboBox4.Items.Count - 1; // Pilih item terakhir
            }

            // Menonaktifkan dropdown
            comboBox4.DropDownStyle = ComboBoxStyle.DropDownList;

        }

        private void savePDF4R()
        {
            string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\EksporPDF\Format-1\1-Gambar\4R\";

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string existingPathName = dir;
            string notExistingFileName = dir + gabung1 + "_4R.pdf";
            if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
            {
                PrintDocument pdoc = new PrintDocument();
                pdoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                pdoc.PrinterSettings.PrintFileName = notExistingFileName;
                pdoc.PrinterSettings.PrintToFile = true;
                pdoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                pdoc.DefaultPageSettings.Landscape = true;
                pdoc.PrintPage += printDocument1_PrintPage;
                pdoc.Print();
            }
        }

        private void HistoryPrint4R(string profile)
        {
            string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\History Print\Format-1\1-Gambar\4R\";

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string existingPathName = dir;
            string notExistingFileName;

            if (profile == "Default")
            {
                notExistingFileName = dir + gabung1 + "_4R.pdf";
            }
            else if (profile == "Adjust Brightness")
            {
                notExistingFileName = dir + gabung1 + "_Adjust_Brightness_4R.pdf";
            }
            else
            {
                // Handle the case where the profile is not recognized
                MessageBox.Show("Profile tidak dikenali", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
            {
                PrintDocument pdoc = new PrintDocument();
                pdoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                pdoc.PrinterSettings.PrintFileName = notExistingFileName;
                pdoc.PrinterSettings.PrintToFile = true;
                pdoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                pdoc.DefaultPageSettings.Landscape = true;
                pdoc.PrintPage += printDocument1_PrintPage;
                pdoc.Print();
            }
        }

        private void savePDFA4()
        {
            string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\EksporPDF\Format-1\1-Gambar\A4\";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string existingPathName = dir;
            string notExistingFileName = dir + gabung1 + "_A4.pdf";

            if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
            {
                PrintDocument pdoc = new PrintDocument();
                pdoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                pdoc.PrinterSettings.PrintFileName = notExistingFileName;
                pdoc.PrinterSettings.PrintToFile = true;
                pdoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                pdoc.DefaultPageSettings.Landscape = false;
                pdoc.PrintPage += printDocument12_PrintPage;
                pdoc.Print();
            }
        }

        private void HistoryPrintA4(string profile)
        {
            string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\History Print\Format-1\1-Gambar\A4\";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string existingPathName = dir;
            string notExistingFileName;

            if (profile == "Default")
            {
                notExistingFileName = dir + gabung1 + "_A4.pdf";
            }
            else if (profile == "Adjust Brightness")
            {
                notExistingFileName = dir + gabung1 + "_Adjust_Brightness_A4.pdf";
            }
            else
            {
                // Handle the case where the profile is not recognized
                MessageBox.Show("Profile tidak dikenali", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
            {
                PrintDocument pdoc = new PrintDocument();
                pdoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                pdoc.PrinterSettings.PrintFileName = notExistingFileName;
                pdoc.PrinterSettings.PrintToFile = true;
                pdoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                pdoc.DefaultPageSettings.Landscape = false;
                pdoc.PrintPage += printDocument12_PrintPage;
                pdoc.Print();
            }
        }
    }
}
