﻿using AForge.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using static WindowsFormsApp1.Form1Print;
using PictureBox = System.Windows.Forms.PictureBox;

namespace WindowsFormsApp1.Format_2
{
    public partial class Form21Gambar : Form
    {
        string splitTahun, splitBulan, tanggal, noRM, id, jam, gabung1, gabung, tanggalDatabase, dataHasilPemeriksaan, code, getIdPasien, getIdDokter, ambilDaerah, action, Date, tanggalLahir, umur, alamat, dokterNama, selectedDate, tanggal1, monthName, year;
        string logoValue, jenisValue;

        string dirRtf = @"D:\GLEndoscope\FileRTF\";
        string dirLogo = @"D:\GLEndoscope\LogoKOP\";
        string csvFilePath = "D:\\GLEndoscope\\Database\\dataPasien\\dataDefault.csv";

        //string dir = @"D:\";

        public delegate void TransfDelegate(String value);
        public event TransfDelegate TEFormat2;
        public event TransfDelegate TEViewC2Gambar;
        public event TransfDelegate TEViewC4Gambar;
        public event TransfDelegate TEViewC6Gambar;
        public event TransfDelegate TEViewC21G;
        int result;
        private Dictionary<PictureBox, PictureBoxControls> pictureBoxControls = new Dictionary<PictureBox, PictureBoxControls>();




      


        public Form21Gambar()
        {
            InitializeComponent();
            FillListBox();
            /*PopulatePrinterComboBox*//*();*/ // Call to populate printers
            comboBox1.SelectedIndex = -1; // Ensure no printer is selected by default
            InitializeThumbnails();
            InitializeMainPictureBoxes();


            DisableEditing(richTextBoxNRS);
            DisableEditing(richTextBoxBE);
            DisableEditing(richTextBoxJalan);
            DisableEditing(richTextBoxEmail);
            DisableEditing(richTextBox2);


            // Inisialisasi pencarian folder dan nama target
            string baseFolder = @"D:\GLEndoscope";  // Base folder utama
            string targetName = gabung;
            LoadDatesToComboBox(baseFolder, targetName);

            jam = DateTime.Now.ToString("hhmmss");
            tanggal = DateTime.Now.ToString("ddMMyyy");
            tanggalDatabase = DateTime.Now.ToString("MMddyyyy_HHmmss");
            string text = DateTime.Now.ToString("Y");
            string[] arr = text.Split(' ');
            splitBulan = arr[0];
            splitTahun = arr[1];

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






        private void DisableEditing(RichTextBox richTextBox)
        {
            // Tangani event KeyPress, KeyDown, dan KeyUp
            richTextBox.KeyPress += (sender, e) => e.Handled = true;
            richTextBox.KeyDown += (sender, e) => e.Handled = true;
            richTextBox.KeyUp += (sender, e) => e.Handled = true;

            //// Tangani event MouseDown dan MouseUp
            //richTextBox.MouseDown += (sender, e) => e.Handled = true;
            //richTextBox.MouseUp += (sender, e) => e.Handled = true;

            // Tangani event untuk mencegah Paste
            richTextBox.ShortcutsEnabled = false;
        }
        //private void LoadLastSelectedResolution()
        //{
        //    // Membaca nilai lebar dan tinggi terakhir dari file teks
        //    string filePath = Path.Combine(Application.StartupPath, "last_selected_resolution.txt");
        //    if (File.Exists(filePath))
        //    {
        //        int width, height; // Deklarasi variabel sebelum blok if
        //        string[] resolution = File.ReadAllText(filePath).Split(',');
        //        if (resolution.Length == 2 && int.TryParse(resolution[0], out width) && int.TryParse(resolution[1], out height))
        //        {
        //            SetFormResolution(width, height);
        //            AdjustControlsLayout(width, height);
        //        }
        //    }
        //}

        //public void SetFormResolution(int lastSelectedWidth, int lastSelectedHeight)
        //{
        //    this.Width = lastSelectedWidth;
        //    this.Height = lastSelectedHeight;
        //}

        //private void AdjustControlsLayout(int screenWidth, int screenHeight)
        //{

        //    // Menghitung faktor skalasi untuk menyesuaikan tata letak komponen
        //    float scaleX = (float)screenWidth / 1107 * 0.76f; // Mengurangi ukuran sebesar 25%
        //    float scaleY = (float)screenHeight / 835 * 0.96f; // Mengurangi ukuran sebesar 25%


        //    // Mengatur posisi relatif setiap komponen berdasarkan faktor skalasi
        //    // Contoh: Memperbesar atau memperkecil posisi komponen
        //    //btnsetting.Location = new Point((int)(703 * scaleX), (int)(22 * scaleY)); // Default location untuk cb1
        //    //bt1.Location = new Point((int)(708 * scaleX), (int)(75 * scaleY));     // Default location untuk bt1
        //    //bt2.Location = new Point((int)(50 * scaleX), (int)(413 * scaleY));    // Default location untuk bt2
        //    //cmb1.Location = new Point((int)(50 * scaleX), (int)(40 * scaleY));

        //    panel1.Location = new Point((int)(12 * scaleX), (int)(12 * scaleY));
        //    panel1.Size = new Size((int)(1083 * scaleX), (int)(811 * scaleY));

        //    button4.Location = new Point((int)(1036 * scaleX), (int)(5 * scaleY));
        //    button4.Size = new Size((int)(40 * scaleX), (int)(40 * scaleY));

        //    button1.Location = new Point((int)(990 * scaleX), (int)(5 * scaleY));
        //    button1.Size = new Size((int)(40 * scaleX), (int)(40 * scaleY));

        //    button7.Location = new Point((int)(944 * scaleX), (int)(5 * scaleY));
        //    button7.Size = new Size((int)(40 * scaleX), (int)(40 * scaleY));

        //    button5.Location = new Point((int)(899 * scaleX), (int)(5 * scaleY));
        //    button5.Size = new Size((int)(40 * scaleX), (int)(40 * scaleY));

        //    panel2.Location = new Point((int)(4 * scaleX), (int)(5 * scaleY));
        //    panel2.Size = new Size((int)(864 * scaleX), (int)(735 * scaleY));

        //    picLogo2.Location = new Point((int)(759 * scaleX), (int)(3 * scaleY));
        //    picLogo2.Size = new Size((int)(100 * scaleX), (int)(100 * scaleY));

        //    richTextBoxNRS.Location = new Point((int)(112 * scaleX), (int)(9 * scaleY));
        //    richTextBoxNRS.Size = new Size((int)(636 * scaleX), (int)(20 * scaleY));

        //    richTextBoxBE.Location = new Point((int)(112 * scaleX), (int)(31 * scaleY));
        //    richTextBoxBE.Size = new Size((int)(636 * scaleX), (int)(20 * scaleY));

        //    richTextBoxJalan.Location = new Point((int)(112 * scaleX), (int)(52 * scaleY));
        //    richTextBoxJalan.Size = new Size((int)(636 * scaleX), (int)(18 * scaleY));

        //    richTextBoxEmail.Location = new Point((int)(112 * scaleX), (int)(68 * scaleY));
        //    richTextBoxEmail.Size = new Size((int)(636 * scaleX), (int)(18 * scaleY));

        //    picLogo1.Location = new Point((int)(3 * scaleX), (int)(3 * scaleY));
        //    picLogo1.Size = new Size((int)(100 * scaleX), (int)(100 * scaleY));

        //    panel3.Location = new Point((int)(3 * scaleX), (int)(107 * scaleY));
        //    panel3.Size = new Size((int)(856 * scaleX), (int)(33 * scaleY));

        //    richTextBox2.Location = new Point((int)(1 * scaleX), (int)(5 * scaleY));
        //    richTextBox2.Size = new Size((int)(730 * scaleX), (int)(20 * scaleY));

        //    panel4.Location = new Point((int)(3 * scaleX), (int)(137 * scaleY));
        //    panel4.Size = new Size((int)(856 * scaleX), (int)(127 * scaleY));

        //    label7.Location = new Point((int)(3 * scaleX), (int)(11 * scaleY));
        //    label7.Size = new Size((int)(37 * scaleX), (int)(15 * scaleY));

        //    label8.Location = new Point((int)(114 * scaleX), (int)(11 * scaleY));
        //    label8.Size = new Size((int)(10 * scaleX), (int)(15 * scaleY));

        //    labelNama.Location = new Point((int)(123 * scaleX), (int)(11 * scaleY));
        //    labelNama.Size = new Size((int)(37 * scaleX), (int)(15 * scaleY));

        //    label12.Location = new Point((int)(1 * scaleX), (int)(42 * scaleY));
        //    label12.Size = new Size((int)(113 * scaleX), (int)(15 * scaleY));

        //    label11.Location = new Point((int)(114 * scaleX), (int)(42 * scaleY));
        //    label11.Size = new Size((int)(10 * scaleX), (int)(15 * scaleY));

        //    labelTglUmur.Location = new Point((int)(123 * scaleX), (int)(42 * scaleY));
        //    labelTglUmur.Size = new Size((int)(113 * scaleX), (int)(15 * scaleY));

        //    label15.Location = new Point((int)(3 * scaleX), (int)(67 * scaleY));
        //    label15.Size = new Size((int)(46 * scaleX), (int)(15 * scaleY));

        //    label14.Location = new Point((int)(114 * scaleX), (int)(67 * scaleY));
        //    label14.Size = new Size((int)(10 * scaleX), (int)(15 * scaleY));

        //    labelNoMR.Location = new Point((int)(123 * scaleX), (int)(68 * scaleY));
        //    labelNoMR.Size = new Size((int)(10 * scaleX), (int)(15 * scaleY));

        //    label24.Location = new Point((int)(486 * scaleX), (int)(11 * scaleY));
        //    label24.Size = new Size((int)(103 * scaleX), (int)(15 * scaleY));

        //    label23.Location = new Point((int)(588 * scaleX), (int)(11 * scaleY));
        //    label23.Size = new Size((int)(103 * scaleX), (int)(15 * scaleY));

        //    labelJenisPemeriksaan.Location = new Point((int)(597 * scaleX), (int)(11 * scaleY));
        //    labelJenisPemeriksaan.Size = new Size((int)(103 * scaleX), (int)(15 * scaleY));

        //    label21.Location = new Point((int)(484 * scaleX), (int)(44 * scaleY));
        //    label21.Size = new Size((int)(103 * scaleX), (int)(15 * scaleY));

        //    label20.Location = new Point((int)(588 * scaleX), (int)(44 * scaleY));
        //    label20.Size = new Size((int)(10 * scaleX), (int)(15 * scaleY));

        //    textBoxKlinis.Location = new Point((int)(600 * scaleX), (int)(43 * scaleY));
        //    textBoxKlinis.Size = new Size((int)(249 * scaleX), (int)(21 * scaleY));

        //    label18.Location = new Point((int)(486 * scaleX), (int)(68 * scaleY));
        //    label18.Size = new Size((int)(45 * scaleX), (int)(15 * scaleY));

        //    label17.Location = new Point((int)(588 * scaleX), (int)(68 * scaleY));
        //    label17.Size = new Size((int)(10 * scaleX), (int)(15 * scaleY));

        //    labelAlamat.Location = new Point((int)(597 * scaleX), (int)(68 * scaleY));
        //    labelAlamat.Size = new Size((int)(45 * scaleX), (int)(15 * scaleY));

        //    panel5.Location = new Point((int)(3 * scaleX), (int)(262 * scaleY));
        //    panel5.Size = new Size((int)(856 * scaleX), (int)(397 * scaleY));

        //    label26.Location = new Point((int)(3 * scaleX), (int)(4 * scaleY));
        //    label26.Size = new Size((int)(68 * scaleX), (int)(17 * scaleY));

        //    label25.Location = new Point((int)(70 * scaleX), (int)(4 * scaleY));
        //    label25.Size = new Size((int)(11 * scaleX), (int)(17 * scaleY));

        //    pictureBox1.Location = new Point((int)(95 * scaleX), (int)(24 * scaleY));
        //    pictureBox1.Size = new Size((int)(666 * scaleX), (int)(363 * scaleY));

        //    pictureBox1.Location = new Point((int)(95 * scaleX), (int)(24 * scaleY));
        //    pictureBox1.Size = new Size((int)(666 * scaleX), (int)(363 * scaleY));

        //    btn_Delete.Location = new Point((int)(730 * scaleX), (int)(30 * scaleY));
        //    btn_Delete.Size = new Size((int)(25 * scaleX), (int)(25 * scaleY));

        //    //buttonAdd.Location = new Point((int)(414 * scaleX), (int)(189 * scaleY));
        //    //buttonAdd.Size = new Size((int)(25 * scaleX), (int)(25 * scaleY));

        //    panel6.Location = new Point((int)(3 * scaleX), (int)(658 * scaleY));
        //    panel6.Size = new Size((int)(428 * scaleX), (int)(72 * scaleY));

        //    label28.Location = new Point((int)(0 * scaleX), (int)(5 * scaleY));
        //    label28.Size = new Size((int)(118 * scaleX), (int)(17 * scaleY));

        //    label27.Location = new Point((int)(116 * scaleX), (int)(5 * scaleY));
        //    label27.Size = new Size((int)(11 * scaleX), (int)(17 * scaleY));

        //    richTextBox1.Location = new Point((int)(3 * scaleX), (int)(25 * scaleY));
        //    richTextBox1.Size = new Size((int)(417 * scaleX), (int)(42 * scaleY));

        //    panel8.Location = new Point((int)(430 * scaleX), (int)(658 * scaleY));
        //    panel8.Size = new Size((int)(429 * scaleX), (int)(72 * scaleY));

        //    labelLokTgl.Location = new Point((int)(152 * scaleX), (int)(2 * scaleY));
        //    labelLokTgl.Size = new Size((int)(68 * scaleX), (int)(15 * scaleY));

        //    label30.Location = new Point((int)(159 * scaleX), (int)(18 * scaleY));
        //    label30.Size = new Size((int)(93 * scaleX), (int)(15 * scaleY));

        //    labelNamaDokter.Location = new Point((int)(67 * scaleX), (int)(50 * scaleY));
        //    labelNamaDokter.Size = new Size((int)(284 * scaleX), (int)(22 * scaleY));

        //    comboBox3.Location = new Point((int)(446 * scaleX), (int)(742 * scaleY));
        //    comboBox3.Size = new Size((int)(130 * scaleX), (int)(24 * scaleY));

        //    comboBox1.Location = new Point((int)(588 * scaleX), (int)(742 * scaleY));
        //    comboBox1.Size = new Size((int)(280 * scaleX), (int)(24 * scaleY));

        //    buttonExportPdf.Location = new Point((int)(4 * scaleX), (int)(742 * scaleY));
        //    buttonExportPdf.Size = new Size((int)(136 * scaleX), (int)(36 * scaleY));

        //    buttonCancel.Location = new Point((int)(588 * scaleX), (int)(770 * scaleY));
        //    buttonCancel.Size = new Size((int)(136 * scaleX), (int)(36 * scaleY));

        //    buttonPrint.Location = new Point((int)(732 * scaleX), (int)(770 * scaleY));
        //    buttonPrint.Size = new Size((int)(136 * scaleX), (int)(36 * scaleY));

        //}


        void FillListBox()
        {
            // Clear existing items
            comboBox1.Items.Clear();

            // Loop through all installed printers
            foreach (var p in PrinterSettings.InstalledPrinters)
            {
                // Convert the printer name to a string
                string printerName = p.ToString();

                // Check if the printer is not the one you want to exclude
                if (printerName != "Canon SELPHY CP1300")
                {
                    // Add the printer to the ComboBox
                    comboBox1.Items.Add(printerName);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            string Pname = comboBox1.SelectedItem.ToString();
            //printer.SetDefaultPrinter(Pname);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Foto diisi Dahulu", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (textBoxKlinis.Text == "")
                {
                    MessageBox.Show("Klinis Belum diisi ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                }
                else
                {
                    if (richTextBox1.Text == "")
                    {
                        MessageBox.Show("Hasil Pemeriksaan Belum diisi ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (comboBox1.SelectedIndex == -1)
                        {
                            MessageBox.Show("Pilih printer terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            if (comboBox3.SelectedIndex == -1)
                            {
                                MessageBox.Show("Pilih profil terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }

                            //PrintDocument pd = new PrintDocument();
                            //pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                            //pd.DefaultPageSettings.Landscape = false;

                            // Set the default printer
                            string selectedPrinter = comboBox1.SelectedItem.ToString();
                            PrintDocument pd = new PrintDocument();
                            pd.PrinterSettings.PrinterName = selectedPrinter;

                            pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                            pd.DefaultPageSettings.Landscape = false;

                            if (comboBox3.Text == "Default")
                            {

                                pd.PrintPage += new PrintPageEventHandler(this.printDocument1_PrintPage);

                                pd.Print();


                                //printPreviewDialog1.Document = pd;
                                //printPreviewDialog1.ShowDialog();

                                comboBox1.Items.Clear();
                                comboBox1.ResetText();
                                FillListBox();
                                panel7.Size = new Size(0, 0);
                                HistoryPrintA4(comboBox3.Text);
                                clearTextboxPemeriksaan();
                                pictureBox1.Image.Dispose();
                                pictureBox1.Image = null;

                                buttobDeleteFalse();
                                //buttobAddTrue();

                                buttonCancel.PerformClick();

                                int kondisi1 = 4;
                                TEViewC21G(kondisi1.ToString());
                                MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (comboBox3.Text == "Adjust Brightness")
                            {
                                pd.PrintPage += new PrintPageEventHandler(this.printDocument2_PrintPage);
                                pd.Print();

                                //printPreviewDialog1.Document = pd;
                                //printPreviewDialog1.ShowDialog();


                                comboBox1.Items.Clear();
                                comboBox1.ResetText();
                                FillListBox();
                                panel7.Size = new Size(0, 0);
                                HistoryPrintA4(comboBox3.Text);
                                clearTextboxPemeriksaan();
                                pictureBox1.Image.Dispose();
                                pictureBox1.Image = null;

                                buttobDeleteFalse();
                                //buttobAddTrue();

                                buttonCancel.PerformClick();

                                int kondisi1 = 4;
                                TEViewC21G(kondisi1.ToString());
                                MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            //pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                            //pd.DefaultPageSettings.Landscape = false;
                            //pd.Print();
                            //printPreviewDialog1.Document = pd;
                            //printPreviewDialog1.ShowDialog();
                            //comboBox1.Items.Clear();
                            //comboBox1.ResetText();
                            //FillListBox();
                            //panel7.Size = new Size(0, 0);
                            //HistoryPrintA4();
                            //clearTextboxPemeriksaan();
                            //pictureBox1.Image.Dispose();
                            //pictureBox1.Image = null;

                            //buttobDeleteFalse();
                            //buttobAddTrue();

                            //buttonCancel.PerformClick();

                            //int kondisi1 = 4;
                            //TEViewC21G(kondisi1.ToString());

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

        private void HistoryPrintA4(string profile)
        {
            string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\History Print\Format-2" + @"\1-Gambar\";
            //string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\History Print\Format-2" + @"\1-Gambar\";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string existingPathName = dir;
            string notExistingFileName;

            if (profile == "Default")
            {
                notExistingFileName = dir + gabung1 + ".pdf";
            }
            else if (profile == "Adjust Brightness")
            {
                notExistingFileName = dir + gabung1 + "_Adjust_Brightness.pdf";
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
                pdoc.PrintPage += printDocument1_PrintPage;
                pdoc.Print();
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            //if (panel2.BorderStyle == BorderStyle.FixedSingle)
            //{
            //    int thickness = 2;//it's up to you
            //    int halfThickness = thickness / 2;
            //    using (Pen p = new Pen(Color.Black, thickness))
            //    {
            //        e.Graphics.DrawRectangle(p, new Rectangle(halfThickness, halfThickness, panel2.ClientSize.Width - thickness, panel2.ClientSize.Height - thickness));
            //    }
            //}
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (panel1.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 2;//it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.Black, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness, halfThickness, panel1.ClientSize.Width - thickness, panel1.ClientSize.Height - thickness));
                }
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

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (logoValue == "1")
            {
                //AdjustPictureBoxSize(e.Graphics, "Persegi");
                //e.Graphics.DrawImage(picLogo1.Image, 30, 3, picLogo1.Width, picLogo1.Height);

                if (jenisValue == "Persegi Panjang")
                {
                    // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, jenisValue);
                    e.Graphics.DrawImage(picLogo2.Image, 0, 5, picLogo2.Width, picLogo2.Height);

                    //MessageBox.Show("Persegi Panjang");
                }
                else
                {
                    // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, "Persegi");
                    e.Graphics.DrawImage(picLogo1.Image, 0, 5, picLogo1.Width, picLogo1.Height);
                    //MessageBox.Show("Persegi");
                }
            }
            else if (logoValue == "2")
            {
                AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
                e.Graphics.DrawImage(picLogo1.Image, 0, 5, picLogo1.Width, picLogo1.Height);

                // Adjust the coordinates based on jenisValue
                if (jenisValue == "Persegi")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 700, 5, picLogo2.Width, picLogo2.Height);
                }
                else if (jenisValue == "Persegi Panjang")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 625, 5, picLogo2.Width, picLogo2.Height);
                }
            }


            StringFormat SF1 = new StringFormat();
            SF1.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(richTextBoxNRS.Text, new Font("Montserrat", 16, FontStyle.Bold), Brushes.Black, 427, 18, SF1);
            e.Graphics.DrawString(richTextBoxBE.Text, new Font("Montserrat", 14, FontStyle.Bold), Brushes.Black, 427, 45, SF1);
            e.Graphics.DrawString(richTextBoxJalan.Text, new Font("Montserrat", 8, FontStyle.Bold), Brushes.Black, 427, 70, SF1);
            e.Graphics.DrawString(richTextBoxEmail.Text, new Font("Montserrat", 8, FontStyle.Bold), Brushes.Black, 427, 85, SF1);

            ////kotak poliklinik
            //Color red = Color.Black;
            //Pen redPen = new Pen(red);
            //redPen.Width = 1;
            //e.Graphics.DrawRectangle(redPen, 0, 110, 800, 35);
            //e.Graphics.DrawString(richTextBox2.Text, new Font("Montserrat", 14, FontStyle.Bold), Brushes.Black, 345, 115);

            //kotak poliklinik
            Color red = Color.Black;
            Pen redPen = new Pen(red);
            redPen.Width = 1;
            e.Graphics.DrawRectangle(redPen, 7, 110, 793, 35);

            Font font = new Font("Montserrat", 14, FontStyle.Bold);

            SizeF textSize = e.Graphics.MeasureString(richTextBox2.Text, font);

            float centerX = 7 + (793 - textSize.Width) / 2;
            float centerY = 110 + (35 - textSize.Height) / 2;

            e.Graphics.DrawString(richTextBox2.Text, font, Brushes.Black, centerX, centerY);

            //kotak data pasien
            Color redd = Color.Black;
            Pen redPenn = new Pen(redd);
            redPenn.Width = 1;
            e.Graphics.DrawRectangle(redPenn, 7, 145, 793, 90);
            e.Graphics.DrawString("Nama", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 10, 150);
            e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 135, 150);
            e.Graphics.DrawString(labelNama.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 145, 150);

            e.Graphics.DrawString("Tanggal Lahir", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 10, 180);
            e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 135, 180);
            e.Graphics.DrawString(labelTglUmur.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 145, 180);

            e.Graphics.DrawString("No RM", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 10, 195);
            e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 135, 195);
            e.Graphics.DrawString(labelNoMR.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 145, 195);

            e.Graphics.DrawString("Jenis Pemeriksaan", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 370, 150);
            e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 505, 150);
            e.Graphics.DrawString(labelJenisPemeriksaan.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 515, 150);

            //e.Graphics.DrawString("Keterangan Klinis", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 370, 165);
            //e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 505, 165);
            //e.Graphics.DrawString(textBoxKlinis.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 515, 165);


            //mulai
            // Mengambil teks dari textBoxKlinis
            string klinisText = textBoxKlinis.Text;

            // Maksimum karakter per baris
            int maxCharsPerLine = 33;

            // Font yang digunakan untuk teks
            Font regularFont = new Font("Montserrat", 9, FontStyle.Regular);
            Font boldFont = new Font("Montserrat", 9, FontStyle.Bold);

            // Posisi awal untuk teks
            float labelX = 370; // X untuk label "Keterangan Klinis"
            float colonX = 505; // X untuk tanda ":"
            float startX = 515; // X untuk teks klinis, buat ini konsisten
            float startY = 165; // Posisi vertikal untuk baris pertama
            float lineHeight = regularFont.GetHeight(e.Graphics);

            // Cetak label "Keterangan Klinis"
            e.Graphics.DrawString("Keterangan Klinis", boldFont, Brushes.Black, labelX, startY);
            e.Graphics.DrawString(":", boldFont, Brushes.Black, colonX, startY);

            // Memecah teks menjadi kata-kata
            string[] words = klinisText.Split(' ');
            string currentLine = "";
            List<string> lines = new List<string>();

            // Membagi teks berdasarkan kata, bukan karakter
            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 <= maxCharsPerLine)
                {
                    // Tambahkan kata ke baris saat ini
                    currentLine += (currentLine == "" ? "" : " ") + word;
                }
                else
                {
                    // Simpan baris dan mulai baris baru
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            // Tambahkan baris terakhir
            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            // Cetak setiap baris teks, dengan X yang sama untuk semua baris
            for (int i = 0; i < lines.Count; i++)
            {
                // Cetak baris teks di posisi X yang sama (startX) dan Y yang menurun setiap baris
                e.Graphics.DrawString(lines[i], regularFont, Brushes.Black, startX, startY + (i * lineHeight));
            }
            //berakhir


            e.Graphics.DrawString("Alamat", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 370, 195);
            e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 505, 195);
            e.Graphics.DrawString(labelAlamat.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 515, 195);

            //kotak foto
            Color reddd = Color.Black;
            Pen redddPen = new Pen(reddd);
            redddPen.Width = 1;
            e.Graphics.DrawRectangle(redddPen, 7, 235, 793, 515);
            e.Graphics.DrawString("Hasil Foto", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 10, 240);
            e.Graphics.DrawString(":", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 120, 240);

            e.Graphics.DrawImage(pictureBox1.Image, 26, 262, 752, 396);

            ////kotak hasil pemeriksaan
            //Color redddd = Color.Black;
            //Pen reddddPen = new Pen(redddd);
            //reddddPen.Width = 1;
            ////e.Graphics.DrawRectangle(reddddPen, 7, 915, 396, 160);
            //e.Graphics.DrawRectangle(reddddPen, 7, 915, 793, 160);

            ////kotak tanda tangan
            ////Pen redddddPen = new Pen(reddd);
            ////redddddPen.Width = 1;
            ////e.Graphics.DrawRectangle(redddddPen, 403, 915, 397, 160);

            //e.Graphics.DrawString("Hasil Pemeriksaan", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 10, 920);
            //e.Graphics.DrawString(":", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 161, 920);

            //// Set the RichTextBox text with line breaks if needed
            //string hasilPemeriksaan = AddNewlinesIfTooLong(richTextBox1.Text, 55);
            //e.Graphics.DrawString(hasilPemeriksaan, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 10, 940);
            //e.Graphics.DrawString(labelLokTgl.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 599, 920, SF1);
            //e.Graphics.DrawString(label30.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 599, 934, SF1);
            //StringFormat sff = new StringFormat();
            //sff.Alignment = StringAlignment.Center;
            //e.Graphics.DrawString(labelNamaDokter.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 599, 1055, sff);

            ////kotak hasil pemeriksaan
            Color redddd = Color.Black;
            Pen reddddPen = new Pen(redddd);
            reddddPen.Width = 1;
            e.Graphics.DrawRectangle(reddddPen, 7, 750, 793, 300);
            e.Graphics.DrawString("Hasil Pemeriksaan", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 10, 755);
            e.Graphics.DrawString(":", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 180, 755);
            e.Graphics.DrawString(richTextBox1.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 10, 775);
            e.Graphics.DrawString(labelLokTgl.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 525, 900);
            e.Graphics.DrawString(label30.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 530, 920);
            StringFormat sff = new StringFormat();
            sff.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(labelNamaDokter.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 600, 1020, sff);
        }

        //private void printDocument12_PrintPage(object sender, PrintPageEventArgs e)
        //{
        //    //e.Graphics.DrawImage(picLogo1.Image, 10, 5, 100, 100);

        //    if (logoValue == "1")
        //    {
        //        //AdjustPictureBoxSize(e.Graphics, "Persegi");
        //        //e.Graphics.DrawImage(picLogo1.Image, 30, 3, picLogo1.Width, picLogo1.Height);

        //        if (jenisValue == "Persegi Panjang")
        //        {
        //            // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
        //            AdjustPictureBoxSize(e.Graphics, jenisValue);
        //            e.Graphics.DrawImage(picLogo2.Image, 30, 3, picLogo2.Width, picLogo2.Height);

        //            //MessageBox.Show("Persegi Panjang");
        //        }
        //        else
        //        {
        //            // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
        //            AdjustPictureBoxSize(e.Graphics, "Persegi");
        //            e.Graphics.DrawImage(picLogo1.Image, 30, 3, picLogo1.Width, picLogo1.Height);
        //            //MessageBox.Show("Persegi");
        //        }
        //    }
        //    else if (logoValue == "2")
        //    {
        //        AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
        //        e.Graphics.DrawImage(picLogo1.Image, 30, 3, picLogo1.Width, picLogo1.Height);

        //        // Adjust the coordinates based on jenisValue
        //        if (jenisValue == "Persegi")
        //        {
        //            e.Graphics.DrawImage(picLogo2.Image, 690, 3, picLogo2.Width, picLogo2.Height);
        //        }
        //        else if (jenisValue == "Persegi Panjang")
        //        {
        //            e.Graphics.DrawImage(picLogo2.Image, 640, 3, picLogo2.Width, picLogo2.Height);
        //        }
        //    }


        //    StringFormat SF1 = new StringFormat();
        //    SF1.Alignment = StringAlignment.Center;
        //    e.Graphics.DrawString(richTextBoxNRS.Text, new Font("Montserrat", 16, FontStyle.Bold), Brushes.Black, 424, 18, SF1);
        //    e.Graphics.DrawString(richTextBoxBE.Text, new Font("Montserrat", 14, FontStyle.Bold), Brushes.Black, 424, 45, SF1);
        //    e.Graphics.DrawString(richTextBoxJalan.Text, new Font("Montserrat", 8, FontStyle.Bold), Brushes.Black, 424, 70, SF1);
        //    e.Graphics.DrawString(richTextBoxEmail.Text, new Font("Montserrat", 8, FontStyle.Bold), Brushes.Black, 424, 85, SF1);

        //    ////kotak poliklinik
        //    //Color red = Color.Black;
        //    //Pen redPen = new Pen(red);
        //    //redPen.Width = 1;
        //    //e.Graphics.DrawRectangle(redPen, 30, 110, 760, 35);
        //    //e.Graphics.DrawString(richTextBox2.Text, new Font("Montserrat", 14, FontStyle.Bold), Brushes.Black, 320, 115);


        //    //kotak poliklinik
        //    Color red = Color.Black;
        //    Pen redPen = new Pen(red);
        //    redPen.Width = 1;
        //    e.Graphics.DrawRectangle(redPen, 21, 110, 805, 35);

        //    Font font = new Font("Montserrat", 14, FontStyle.Bold);

        //    SizeF textSize = e.Graphics.MeasureString(richTextBox2.Text, font);

        //    float centerX = 21 + (805 - textSize.Width) / 2;
        //    float centerY = 110 + (35 - textSize.Height) / 2;

        //    e.Graphics.DrawString(richTextBox2.Text, font, Brushes.Black, centerX, centerY);

        //    //kotak data pasien
        //    Color redd = Color.Black;
        //    Pen redPenn = new Pen(redd);
        //    redPenn.Width = 1;
        //    e.Graphics.DrawRectangle(redPenn, 21, 145, 805, 90);
        //    e.Graphics.DrawString("Nama", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 24, 150);
        //    e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 135, 150);
        //    e.Graphics.DrawString(labelNama.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 145, 150);

        //    e.Graphics.DrawString("Tanggal Lahir", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 24, 180);
        //    e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 135, 180);
        //    e.Graphics.DrawString(labelTglUmur.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 145, 180);

        //    e.Graphics.DrawString("No RM", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 24, 195);
        //    e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 135, 195);
        //    e.Graphics.DrawString(labelNoMR.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 145, 195);

        //    e.Graphics.DrawString("Jenis Pemeriksaan", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 370, 150);
        //    e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 505, 150);
        //    e.Graphics.DrawString(labelJenisPemeriksaan.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 515, 150);

        //    //e.Graphics.DrawString("Keterangan Klinis", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 370, 165);
        //    //e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 505, 165);
        //    //e.Graphics.DrawString(textBoxKlinis.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 515, 165);

        //    //mulai
        //    // Mengambil teks dari textBoxKlinis
        //    string klinisText = textBoxKlinis.Text;

        //    // Maksimum karakter per baris
        //    int maxCharsPerLine = 33;

        //    // Font yang digunakan untuk teks
        //    Font regularFont = new Font("Montserrat", 9, FontStyle.Regular);
        //    Font boldFont = new Font("Montserrat", 9, FontStyle.Bold);

        //    // Posisi awal untuk teks
        //    float labelX = 370; // X untuk label "Keterangan Klinis"
        //    float colonX = 505; // X untuk tanda ":"
        //    float startX = 515; // X untuk teks klinis, buat ini konsisten
        //    float startY = 165; // Posisi vertikal untuk baris pertama
        //    float lineHeight = regularFont.GetHeight(e.Graphics);

        //    // Cetak label "Keterangan Klinis"
        //    e.Graphics.DrawString("Keterangan Klinis", boldFont, Brushes.Black, labelX, startY);
        //    e.Graphics.DrawString(":", boldFont, Brushes.Black, colonX, startY);

        //    // Memecah teks menjadi kata-kata
        //    string[] words = klinisText.Split(' ');
        //    string currentLine = "";
        //    List<string> lines = new List<string>();

        //    // Membagi teks berdasarkan kata, bukan karakter
        //    foreach (var word in words)
        //    {
        //        if (currentLine.Length + word.Length + 1 <= maxCharsPerLine)
        //        {
        //            // Tambahkan kata ke baris saat ini
        //            currentLine += (currentLine == "" ? "" : " ") + word;
        //        }
        //        else
        //        {
        //            // Simpan baris dan mulai baris baru
        //            lines.Add(currentLine);
        //            currentLine = word;
        //        }
        //    }

        //    // Tambahkan baris terakhir
        //    if (!string.IsNullOrEmpty(currentLine))
        //    {
        //        lines.Add(currentLine);
        //    }

        //    // Cetak setiap baris teks, dengan X yang sama untuk semua baris
        //    for (int i = 0; i < lines.Count; i++)
        //    {
        //        // Cetak baris teks di posisi X yang sama (startX) dan Y yang menurun setiap baris
        //        e.Graphics.DrawString(lines[i], regularFont, Brushes.Black, startX, startY + (i * lineHeight));
        //    }
        //    //berakhir


        //    e.Graphics.DrawString("Alamat", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 370, 195);
        //    e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 505, 195);
        //    e.Graphics.DrawString(labelAlamat.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 515, 195);

        //    //kotak foto
        //    Color reddd = Color.Black;
        //    Pen redddPen = new Pen(reddd);
        //    redddPen.Width = 1;
        //    e.Graphics.DrawRectangle(redddPen, 21, 235, 805, 680);
        //    e.Graphics.DrawString("Hasil Foto", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 24, 240);
        //    e.Graphics.DrawString(":", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 120, 240);

        //    e.Graphics.DrawImage(pictureBox1.Image, 40, 262, 768, 396);

        //    //kotak hasil pemeriksaan
        //    Color redddd = Color.Black;
        //    Pen reddddPen = new Pen(redddd);
        //    reddddPen.Width = 1;
        //    e.Graphics.DrawRectangle(reddddPen, 21, 915, 402, 160);

        //    //kotak tanda tangan
        //    Pen redddddPen = new Pen(reddd);
        //    redddddPen.Width = 1;
        //    e.Graphics.DrawRectangle(redddddPen, 423, 915, 403, 160);

        //    e.Graphics.DrawString("Hasil Pemeriksaan", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 24, 920);
        //    e.Graphics.DrawString(":", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 161, 920);

        //    // Set the RichTextBox text with line breaks if needed
        //    string hasilPemeriksaan = AddNewlinesIfTooLong(richTextBox1.Text, 55);
        //    e.Graphics.DrawString(hasilPemeriksaan, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 24, 940);
        //    e.Graphics.DrawString(labelLokTgl.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 624, 920, SF1);
        //    e.Graphics.DrawString(label30.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 624, 934, SF1);
        //    StringFormat sff = new StringFormat();
        //    sff.Alignment = StringAlignment.Center;
        //    e.Graphics.DrawString(labelNamaDokter.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 624, 1055, sff);
        //}

        private void printDocument2_PrintPage(object sender, PrintPageEventArgs e)
        {
            //e.Graphics.DrawImage(picLogo1.Image, 10, 5, 100, 100);

            if (logoValue == "1")
            {
                //AdjustPictureBoxSize(e.Graphics, "Persegi");
                //e.Graphics.DrawImage(picLogo1.Image, 30, 3, picLogo1.Width, picLogo1.Height);

                if (jenisValue == "Persegi Panjang")
                {
                    // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, jenisValue);
                    e.Graphics.DrawImage(picLogo2.Image, 0, 5, picLogo2.Width, picLogo2.Height);

                    //MessageBox.Show("Persegi Panjang");
                }
                else
                {
                    // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, "Persegi");
                    e.Graphics.DrawImage(picLogo1.Image, 0, 5, picLogo1.Width, picLogo1.Height);
                    //MessageBox.Show("Persegi");
                }
            }
            else if (logoValue == "2")
            {
                AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
                e.Graphics.DrawImage(picLogo1.Image, 0, 5, picLogo1.Width, picLogo1.Height);

                // Adjust the coordinates based on jenisValue
                if (jenisValue == "Persegi")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 700, 5, picLogo2.Width, picLogo2.Height);
                }
                else if (jenisValue == "Persegi Panjang")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 625, 5, picLogo2.Width, picLogo2.Height);
                }
            }


            StringFormat SF1 = new StringFormat();
            SF1.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(richTextBoxNRS.Text, new Font("Montserrat", 16, FontStyle.Bold), Brushes.Black, 427, 18, SF1);
            e.Graphics.DrawString(richTextBoxBE.Text, new Font("Montserrat", 14, FontStyle.Bold), Brushes.Black, 427, 45, SF1);
            e.Graphics.DrawString(richTextBoxJalan.Text, new Font("Montserrat", 8, FontStyle.Bold), Brushes.Black, 427, 70, SF1);
            e.Graphics.DrawString(richTextBoxEmail.Text, new Font("Montserrat", 8, FontStyle.Bold), Brushes.Black, 427, 85, SF1);

            ////kotak poliklinik
            //Color red = Color.Black;
            //Pen redPen = new Pen(red);
            //redPen.Width = 1;
            //e.Graphics.DrawRectangle(redPen, 0, 110, 800, 35);
            //e.Graphics.DrawString(richTextBox2.Text, new Font("Montserrat", 14, FontStyle.Bold), Brushes.Black, 345, 115);

            //kotak poliklinik
            Color red = Color.Black;
            Pen redPen = new Pen(red);
            redPen.Width = 1;
            e.Graphics.DrawRectangle(redPen, 7, 110, 793, 35);

            Font font = new Font("Montserrat", 14, FontStyle.Bold);

            SizeF textSize = e.Graphics.MeasureString(richTextBox2.Text, font);

            float centerX = 7 + (793 - textSize.Width) / 2;
            float centerY = 110 + (35 - textSize.Height) / 2;

            e.Graphics.DrawString(richTextBox2.Text, font, Brushes.Black, centerX, centerY);

            //kotak data pasien
            Color redd = Color.Black;
            Pen redPenn = new Pen(redd);
            redPenn.Width = 1;
            e.Graphics.DrawRectangle(redPenn, 7, 145, 793, 90);
            e.Graphics.DrawString("Nama", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 10, 150);
            e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 135, 150);
            e.Graphics.DrawString(labelNama.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 145, 150);

            e.Graphics.DrawString("Tanggal Lahir", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 10, 180);
            e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 135, 180);
            e.Graphics.DrawString(labelTglUmur.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 145, 180);

            e.Graphics.DrawString("No RM", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 10, 195);
            e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 135, 195);
            e.Graphics.DrawString(labelNoMR.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 145, 195);

            e.Graphics.DrawString("Jenis Pemeriksaan", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 370, 150);
            e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 505, 150);
            e.Graphics.DrawString(labelJenisPemeriksaan.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 515, 150);

            //e.Graphics.DrawString("Keterangan Klinis", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 370, 165);
            //e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 505, 165);
            //e.Graphics.DrawString(textBoxKlinis.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 515, 165);


            //mulai
            // Mengambil teks dari textBoxKlinis
            string klinisText = textBoxKlinis.Text;

            // Maksimum karakter per baris
            int maxCharsPerLine = 33;

            // Font yang digunakan untuk teks
            Font regularFont = new Font("Montserrat", 9, FontStyle.Regular);
            Font boldFont = new Font("Montserrat", 9, FontStyle.Bold);

            // Posisi awal untuk teks
            float labelX = 370; // X untuk label "Keterangan Klinis"
            float colonX = 505; // X untuk tanda ":"
            float startX = 515; // X untuk teks klinis, buat ini konsisten
            float startY = 165; // Posisi vertikal untuk baris pertama
            float lineHeight = regularFont.GetHeight(e.Graphics);

            // Cetak label "Keterangan Klinis"
            e.Graphics.DrawString("Keterangan Klinis", boldFont, Brushes.Black, labelX, startY);
            e.Graphics.DrawString(":", boldFont, Brushes.Black, colonX, startY);

            // Memecah teks menjadi kata-kata
            string[] words = klinisText.Split(' ');
            string currentLine = "";
            List<string> lines = new List<string>();

            // Membagi teks berdasarkan kata, bukan karakter
            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 <= maxCharsPerLine)
                {
                    // Tambahkan kata ke baris saat ini
                    currentLine += (currentLine == "" ? "" : " ") + word;
                }
                else
                {
                    // Simpan baris dan mulai baris baru
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            // Tambahkan baris terakhir
            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            // Cetak setiap baris teks, dengan X yang sama untuk semua baris
            for (int i = 0; i < lines.Count; i++)
            {
                // Cetak baris teks di posisi X yang sama (startX) dan Y yang menurun setiap baris
                e.Graphics.DrawString(lines[i], regularFont, Brushes.Black, startX, startY + (i * lineHeight));
            }
            //berakhir


            e.Graphics.DrawString("Alamat", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 370, 195);
            e.Graphics.DrawString(":", new Font("Montserrat", 9, FontStyle.Bold), Brushes.Black, 505, 195);
            e.Graphics.DrawString(labelAlamat.Text, new Font("Montserrat", 9, FontStyle.Regular), Brushes.Black, 515, 195);

            //kotak foto
            Color reddd = Color.Black;
            Pen redddPen = new Pen(reddd);
            redddPen.Width = 1;
            e.Graphics.DrawRectangle(redddPen, 7, 235, 793, 515);
            e.Graphics.DrawString("Hasil Foto", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 10, 240);
            e.Graphics.DrawString(":", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 120, 240);

            //e.Graphics.DrawImage(pictureBox1.Image, 34, 262, 752, 396);
            //e.Graphics.DrawImage(pictureBox1.Image, 24, 262, 752, 396);

            //float contrast = 1.41f;
            float contrast = 1.00f;
            float gamma = 0.715f;
            float reed = 0.56f;
            float green = 0.35f;
            float blue = 0.28f;

            ImageAttributes ia = new ImageAttributes();
            float[][] ptsarray = {
                        new float[] { contrast+reed, 0f, 0f, 0f, 0f},
                        new float[] { 0f, contrast+green, 0f, 0f, 0f},
                        new float[] { 0f, 0f, contrast+blue, 0f, 0f},
                        new float[] { 0f, 0f,       0f, 1f, 0f},
                        new float[] {   0, 0,        0, 1f, 1f},
                };
            ia.ClearColorMatrix();
            ia.SetColorMatrix(new ColorMatrix(ptsarray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            ia.SetGamma(gamma, ColorAdjustType.Bitmap);
            e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(26, 262, 752, 396), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
            ia.Dispose();


            ////kotak hasil pemeriksaan
            Color redddd = Color.Black;
            Pen reddddPen = new Pen(redddd);
            reddddPen.Width = 1;
            e.Graphics.DrawRectangle(reddddPen, 7, 750, 793, 300);
            e.Graphics.DrawString("Hasil Pemeriksaan", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 10, 755);
            e.Graphics.DrawString(":", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 180, 755);
            e.Graphics.DrawString(richTextBox1.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 10, 775);
            e.Graphics.DrawString(labelLokTgl.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 525, 900);
            e.Graphics.DrawString(label30.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 530, 920);
            StringFormat sff = new StringFormat();
            sff.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(labelNamaDokter.Text, new Font("Montserrat", 10, FontStyle.Regular), Brushes.Black, 600, 1020, sff);
        }

        private void AdjustPictureBoxSize(Graphics graphics, string jenis)
        {
            // Check the value of jenis and adjust the size of PictureBox controls accordingly
            if (jenis == "Persegi")
            {
                picLogo1.Size = new Size(100, 100);
                picLogo2.Size = new Size(100, 100);
            }
            else if (jenis == "Persegi Panjang")
            {
                picLogo1.Size = new Size(200, 100);
                picLogo2.Size = new Size(200, 100);
            }
            // Add more conditions as needed
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.icon;
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            comboBox1.Items.Clear();
            comboBox1.ResetText();
            FillListBox();
            int kondisi1 = 7;
            TEViewC6Gambar(kondisi1.ToString());
            this.Close();
        }

        private void FormFormat3_Load(object sender, EventArgs e)
        {
            this.ActiveControl = label10;
            richTextBox2.SelectionAlignment = HorizontalAlignment.Center;
            string dirlogo1 = dirLogo + "logo1.png";
            btn_Delete.Visible = false;
            //string dirlogo1 = dir + "1160358.png";
            if (!Directory.Exists(dirlogo1))
            {
                picLogo1.Image = Image.FromFile(dirLogo + "logo1.png");
                picLogo2.Image = Image.FromFile(dirLogo + "logo2.png");
            }
            LoadAndSetValues();
            buttobDeleteFalse();

            if (comboBox4.Items.Count > 0) // Pastikan ComboBox memiliki item
            {
                comboBox4.SelectedIndex = comboBox4.Items.Count - 1; // Pilih item terakhir
            }

            // Menonaktifkan dropdown
            comboBox4.DropDownStyle = ComboBoxStyle.DropDownList; 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.icon;
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            comboBox1.Items.Clear();
            comboBox1.ResetText();
            FillListBox();
            int kondisi1 = 5;
            TEViewC2Gambar(kondisi1.ToString());
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.icon;
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            comboBox1.Items.Clear();
            comboBox1.ResetText();
            FillListBox();
            int kondisi1 = 6;
            TEViewC4Gambar(kondisi1.ToString());
            this.Close();
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            //buttonAdd.Visible = true;
            btn_Delete.Visible = false;

        }

        private void buttonExportPdf_Click(object sender, EventArgs e)
        {

            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Foto diisi Dahulu", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (textBoxKlinis.Text == "")
                {
                    MessageBox.Show("Klinis Belum diisi ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                }
                else
                {
                    if (richTextBox1.Text == "")
                    {
                        MessageBox.Show("Hasil Pemeriksaan Belum diisi ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\EksporPDF\Format-2\1-Gambar\";
                        //string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\Document\Format-2\1-Gambar\";
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        string existingPathName = dir;
                        string notExistingFileName = dir + gabung1 + ".pdf";

                        if (Directory.Exists(existingPathName) && !File.Exists(notExistingFileName))
                        {
                            PrintDocument pdoc = new PrintDocument();
                            pdoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                            pdoc.PrinterSettings.PrintFileName = notExistingFileName;
                            pdoc.PrinterSettings.PrintToFile = true;
                            pdoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                            pdoc.DefaultPageSettings.Landscape = false;
                            pdoc.PrintPage += printDocument1_PrintPage;
                            pdoc.Print();

                            buttonCancel.PerformClick();

                            int kondisi1 = 4;
                            TEViewC21G(kondisi1.ToString());
                            MessageBox.Show("Export PDF berhasil", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";
            if (of.ShowDialog() == DialogResult.OK)
            {
                picLogo1.ImageLocation = of.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            picLogo1.Image.Dispose();
            picLogo1.Image = null;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.icon;
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            clearTextboxPemeriksaan();
            comboBox1.Items.Clear();
            comboBox1.ResetText();
            FillListBox();
            int kondisi = 9;
            TEFormat2(kondisi.ToString());
            this.Close();
        }

        private void clearTextboxPemeriksaan()
        {
            textBoxKlinis.Clear();
            richTextBox1.Clear();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                //string csvFilePath = "D:\\GLEndoscope\\Database\\dataPasien\\dataDefault.csv";
                ReadDataFromCSV(csvFilePath);
                LoadAndSetValues();
                textBox2.Clear();
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
                        picLogo2.Visible = false;
                    }
                    else if (logoValue == "2")
                    {
                        picLogo1.Visible = true;
                        picLogo2.Visible = true;
                    }
                    else
                    {
                        // Handle other cases if needed
                        picLogo1.Visible = false;
                        picLogo2.Visible = false;
                    }

                    // Check the jenisValue and adjust the size of PictureBox controls accordingly
                    if (jenisValue == "Persegi")
                    {
                        picLogo1.Size = new Size(100, 100);
                        picLogo1.Location = new Point(3, 3);

                        picLogo2.Size = new Size(100, 100);
                        picLogo2.Location = new Point(759, 3);

                        richTextBoxNRS.Size = new Size(639, 20);
                        richTextBoxNRS.Location = new Point(112, 13);

                        richTextBoxBE.Size = new Size(639, 20);
                        richTextBoxBE.Location = new Point(112, 35);

                        richTextBoxJalan.Size = new Size(639, 18);
                        richTextBoxJalan.Location = new Point(112, 56);

                        richTextBoxEmail.Size = new Size(639, 18);
                        richTextBoxEmail.Location = new Point(112, 72);

                        //label1.Size = new Size(538, 23);
                        //label1.Location = new Point(164, 27);

                        //label2.Size = new Size(613, 23);
                        //label2.Location = new Point(127, 50);
                    }
                    else if (jenisValue == "Persegi Panjang")
                    {
                        picLogo1.Size = new Size(200, 100);
                        picLogo1.Location = new Point(3, 3);

                        picLogo2.Size = new Size(200, 100);
                        picLogo2.Location = new Point(659, 3);

                        richTextBoxNRS.Size = new Size(444, 20);
                        richTextBoxBE.Size = new Size(444, 20);
                        richTextBoxJalan.Size = new Size(444, 18);
                        richTextBoxEmail.Size = new Size(444, 18);

                        richTextBoxNRS.Location = new Point(209, 13);
                        richTextBoxBE.Location = new Point(209, 35);
                        richTextBoxJalan.Location = new Point(209, 56);
                        richTextBoxEmail.Location = new Point(209, 72);


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

        /*     private void ReadDataFromCSV(string filePath)
            {
                try
                {
                    string[] lines = File.ReadAllLines(filePath);

                    if (lines.Length < 2)
                    {
                        MessageBox.Show("The CSV file is empty or does not contain the expected header.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string[] headers = lines[0].Split(',');

                    // Output header names for debugging
                    //MessageBox.Show($"Header names: {string.Join(", ", headers)}", "Debugging", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Find the indices of the desired columns 
                    int noRMIndex = Array.IndexOf(headers, "Rm");
                    int nameIndex = Array.IndexOf(headers, "Nama");
                    int actionIndex = Array.IndexOf(headers, "JenisPemeriksaan");
                    int dateIndex = Array.IndexOf(headers, "Tanggal Kunjungan");
                    int tangalLahirIndex = Array.IndexOf(headers, "Tanggal Lahir");
                    int umurIndex = Array.IndexOf(headers, "Umur");
                    int alamatIndex = Array.IndexOf(headers, "Alamat");
                    int namaDokterIndex = Array.IndexOf(headers, "Dokter");

                    // Check if all required columns are present in the CSV file
                    //if (noRMIndex == -1 || nameIndex == -1 || actionIndex == -1 || dateIndex == -1)
                    //{
                    //    MessageBox.Show("One or more required columns not found in the CSV file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //    return;
                    //}

                    string[] values = lines[1].Split(',');

                    noRM = values[noRMIndex].Trim();
                    Name = values[nameIndex].Trim();
                    action = values[actionIndex].Trim();
                    Date = values[dateIndex].Trim();
                    tanggalLahir = values[tangalLahirIndex].Trim();
                    umur = values[umurIndex].Trim();
                    alamat = values[alamatIndex].Trim();
                    dokterNama = values[namaDokterIndex].Trim();

                    gabung = noRM + "-" + Name;

                    DateTime today = DateTime.Now;
                    jam = today.ToString("hhmmss");
                    gabung1 = noRM + "-" + Name + "-" + jam;

                    string combinedText = Name;
                    labelNama.Text = AddNewlinesIfTooLong(combinedText, 30);

                    string tgl_lahir, tglKunjungan;
                    tgl_lahir = tanggalLahir;
                    labelTglUmur.Text = tgl_lahir + " - " + umur;
                    labelNoMR.Text = noRM;
                    labelJenisPemeriksaan.Text = action;
                    tglKunjungan = Date;

                    string combinedAlamat = alamat;
                    labelAlamat.Text = AddNewlinesIfTooLong(combinedAlamat, 40);

                    richTextBoxNRS.LoadFile(dirRtf + "RtfFile.rtf", RichTextBoxStreamType.RichText);
                    richTextBoxBE.LoadFile(dirRtf + "RtfFile1.rtf", RichTextBoxStreamType.RichText);
                    richTextBoxJalan.LoadFile(dirRtf + "RtfFile2.rtf", RichTextBoxStreamType.RichText);
                    richTextBoxEmail.LoadFile(dirRtf + "RtfFile3.rtf", RichTextBoxStreamType.RichText);
                    richTextBox2.LoadFile(dirRtf + "RtfFile5.rtf", RichTextBoxStreamType.RichText);
                    richTextBox5.LoadFile(dirRtf + "RtfFile4.rtf", RichTextBoxStreamType.RichText);

                    ambilDaerah = richTextBox5.Text;
                    labelLokTgl.Text = ambilDaerah + ", " + tglKunjungan;

                    string namaDokter = dokterNama;
                    labelNamaDokter.Text = "(" + namaDokter + ")";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }*/
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
                        var noRM = csv.GetField<string>("Rm");
                        var name = csv.GetField<string>("Nama");
                        var action = csv.GetField<string>("Jenis Pemeriksaan");
                        var date = csv.GetField<string>("Tanggal Kunjungan");
                        var tanggalLahir = csv.GetField<string>("Tanggal Lahir");
                        var umur = csv.GetField<string>("Umur");
                        var alamat = csv.GetField<string>("Alamat");
                        var dokterNama = csv.GetField<string>("Dokter");
                        gabung = noRM + "-" + name;

                        DateTime today = DateTime.Now;
                        jam = today.ToString("hhmmss");
                        gabung1 = noRM + "-" + name + "-" + jam;

                        string combinedText = name;
                        labelNama.Text = AddNewlinesIfTooLong(combinedText, 30);

                        string tgl_lahir, tglKunjungan;
                        tgl_lahir = tanggalLahir;
                        labelTglUmur.Text = tgl_lahir + " - " + umur;
                        labelNoMR.Text = noRM;
                        labelJenisPemeriksaan.Text = action;
                        tglKunjungan = date;

                        string combinedAlamat = alamat;
                        labelAlamat.Text = AddNewlinesIfTooLong(combinedAlamat, 40);

                        richTextBoxNRS.LoadFile(dirRtf + "RtfFile.rtf", RichTextBoxStreamType.RichText);
                        richTextBoxBE.LoadFile(dirRtf + "RtfFile1.rtf", RichTextBoxStreamType.RichText);
                        richTextBoxJalan.LoadFile(dirRtf + "RtfFile2.rtf", RichTextBoxStreamType.RichText);
                        richTextBoxEmail.LoadFile(dirRtf + "RtfFile3.rtf", RichTextBoxStreamType.RichText);
                        richTextBox2.LoadFile(dirRtf + "RtfFile5.rtf", RichTextBoxStreamType.RichText);
                        richTextBox5.LoadFile(dirRtf + "RtfFile4.rtf", RichTextBoxStreamType.RichText);

                        ambilDaerah = richTextBox5.Text;
                        labelLokTgl.Text = ambilDaerah + ", " + tglKunjungan;

                        string namaDokter = dokterNama;
                        labelNamaDokter.Text = "(" + namaDokter + ")";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tidak ada data yang tersedia. Mohon isi data Pasien terlebih dahulu.", "Informasi!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string AddNewlinesIfTooLong(string inputText, int maxLineLength)
        {
            StringBuilder result = new StringBuilder();
            string[] words = inputText.Split(' ');  // Memecah teks menjadi kata-kata
            int currentLineLength = 0;

            foreach (string word in words)
            {
                // Jika menambahkan kata akan melebihi batas, maka pindah ke baris berikutnya
                if (currentLineLength + word.Length + 1 > maxLineLength)
                {
                    result.AppendLine();  // Menambahkan newline
                    currentLineLength = 0; // Reset panjang baris saat ini
                }

                // Menambahkan kata ke baris dan memperbarui panjang baris
                if (currentLineLength > 0)
                {
                    result.Append(" ");  // Menambahkan spasi jika bukan kata pertama di baris
                    currentLineLength++;  // Menambah 1 untuk spasi
                }

                result.Append(word);  // Menambahkan kata
                currentLineLength += word.Length;  // Menambah panjang kata ke panjang baris
            }

            return result.ToString();
        }

        //private string AddNewlinesIfTooLong(string inputText, int maxLineLength)
        //{
        //    StringBuilder result = new StringBuilder();
        //    string[] words = inputText.Split(' ');

        //    int currentLineLength = 0;

        //    foreach (string word in words)
        //    {
        //        if (currentLineLength + word.Length + 1 <= maxLineLength)
        //        {
        //            result.Append(word + " ");
        //            currentLineLength += word.Length + 1;
        //        }
        //        else
        //        {
        //            result.AppendLine(); // Add a newline
        //            result.Append(word + " ");
        //            currentLineLength = word.Length + 1;
        //        }
        //    }

        //    return result.ToString();
        //}

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                btn_Delete.Visible = false;
                //buttonAdd.Visible = true;
            }
            else
            {
                btn_Delete.Visible = true;
                //buttonAdd.Visible = false;
            }

            if (pictureBox1.Image != null && picLogo1.Image != null && richTextBox1.Text != "" && textBoxKlinis.Text != "")
            {
                buttonExportPdf.Enabled = true;
                buttonPrint.Enabled = true;
                comboBox1.Enabled = true;
            }
            else
            {
                buttonExportPdf.Enabled = false;
                buttonPrint.Enabled = false;
                comboBox1.Enabled = false;
            }

            jam = DateTime.Now.ToString("hhmmss");
            tanggal = DateTime.Now.ToString("ddMMyyy");
            tanggalDatabase = DateTime.Now.ToString("MMddyyyy_HHmmss");
            string text = DateTime.Now.ToString("Y");
            string[] arr = text.Split(' ');
            splitBulan = arr[0];
            splitTahun = arr[1];
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
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
    }
}
