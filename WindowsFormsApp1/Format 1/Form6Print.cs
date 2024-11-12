﻿using AForge.Controls;
using AForge.Imaging.Filters;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using CsvHelper;
using PictureBox = System.Windows.Forms.PictureBox;


namespace WindowsFormsApp1
{
    public partial class Form6Print : Form
    {
        //string dir = @"D:\";
        string dirLogo = @"D:\GLEndoscope\LogoKOP\";
        string csvFilePath = "D:\\GLEndoscope\\Database\\dataPasien\\dataDefault.csv";

        public delegate void TransfDelegate(String value);
        public event TransfDelegate TransfEventttt;
        public event TransfDelegate TransfEventPrint1;
        public event TransfDelegate TransfEventPrint6;
        public event TransfDelegate TransfEventPrint6G;

        string gabung1, gabung, jam, tanggal, id, kondisiPDF, splitBulan, splitTahun, noRM, nameFix, action, Date, selectedDate, tanggal1, monthName, year;
        string logoValue, jenisValue;
        private Dictionary<PictureBox, PictureBoxControls> pictureBoxControls = new Dictionary<PictureBox, PictureBoxControls>();


        public Form6Print()
        {
            InitializeComponent();
            FillListBox();
            comboBox1.SelectedIndex = -1; // Ensure no printer is selected by default
            InitializeThumbnails();
            InitializeMainPictureBoxes();

            comboBox1.KeyPress += new KeyPressEventHandler(ComboBox_KeyPress);
            comboBox2.KeyPress += new KeyPressEventHandler(ComboBox_KeyPress);
            comboBox3.KeyPress += new KeyPressEventHandler(ComboBox_KeyPress);

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

        private void ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; // Mencegah karakter yang diketik ditampilkan di ComboBox
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
            PictureBox[] pictureBoxes = { pictureBox1, pictureBox2, pictureBox3, pictureBox4, pictureBox5, pictureBox6 };

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

                            pictureBox.Image = Image.FromFile(filePath);
                        }
                    }
                }
            }
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

        private void printDocument2_PrintPage(object sender, PrintPageEventArgs e)
        {

            // Check the value of logoValue
            if (logoValue == "1")
            {
                //AdjustPictureBoxSize(e.Graphics, "Persegi");
                //e.Graphics.DrawImage(picLogo1.Image, 19, 0, picLogo1.Width, picLogo1.Height);

                if (jenisValue == "Persegi Panjang")
                {
                    // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, jenisValue);
                    e.Graphics.DrawImage(picLogo2.Image, 10, 5, picLogo2.Width, picLogo2.Height);

                    //MessageBox.Show("Persegi Panjang");
                }
                else
                {
                    // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, "Persegi");
                    e.Graphics.DrawImage(picLogo1.Image, 10, 5, picLogo1.Width, picLogo1.Height);
                    //MessageBox.Show("Persegi");
                }
            }
            else if (logoValue == "2")
            {
                AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
                e.Graphics.DrawImage(picLogo1.Image, 5, 5, picLogo1.Width, picLogo1.Height);

                // Adjust the coordinates based on jenisValue
                if (jenisValue == "Persegi")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 695, 5, picLogo2.Width, picLogo2.Height);
                }
                else if (jenisValue == "Persegi Panjang")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 595, 5, picLogo2.Width, picLogo2.Height);
                }
            }


            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label1.Text, new Font("Montserrat", 12, FontStyle.Bold), Brushes.Black, 410, 35, sf);

            StringFormat sff = new StringFormat();
            sff.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label2.Text, new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 410, 60, sff);

            //kotak foto
            Color reddd = Color.Black;
            Pen redddPen = new Pen(reddd);
            redddPen.Width = 1;
            e.Graphics.DrawRectangle(redddPen, 180, 120, 59, 15);
            e.Graphics.DrawString("KANAN", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 180, 120);


            //kotak foto
            Color red = Color.Black;
            Pen redPen = new Pen(red);
            redPen.Width = 1;

            e.Graphics.DrawRectangle(redPen, 590, 120, 35, 15);
            e.Graphics.DrawString("KIRI", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 590, 120);

            e.Graphics.DrawImage(pictureBox1.Image, 9, 145, 383, 213);
            e.Graphics.DrawImage(pictureBox4.Image, 410, 145, 383, 213);
            e.Graphics.DrawImage(pictureBox2.Image, 9, 378, 383, 213);
            e.Graphics.DrawImage(pictureBox5.Image, 410, 378, 383, 213);
            e.Graphics.DrawImage(pictureBox3.Image, 9, 611, 383, 213);
            e.Graphics.DrawImage(pictureBox6.Image, 410, 611, 383, 213);
            //e.Graphics.DrawImage(picLogo.Image, 5, 0, 100, 100);
        }

        private void printDocument12_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Check the value of logoValue
            if (logoValue == "1")
            {
                if (jenisValue == "Persegi Panjang")
                {
                    // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, jenisValue);
                    e.Graphics.DrawImage(picLogo2.Image, 10, 3, picLogo2.Width, picLogo2.Height);

                    //MessageBox.Show("Persegi Panjang");
                }
                else
                {
                    // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, "Persegi");
                    e.Graphics.DrawImage(picLogo1.Image, 10, 3, picLogo1.Width, picLogo1.Height);
                    //MessageBox.Show("Persegi");
                }
            }
            else if (logoValue == "2")
            {
                AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
                e.Graphics.DrawImage(picLogo1.Image, 10, 3, picLogo1.Width, picLogo1.Height);

                // Adjust the coordinates based on jenisValue
                if (jenisValue == "Persegi")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 695, 3, picLogo2.Width, picLogo2.Height);
                }
                else if (jenisValue == "Persegi Panjang")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 595, 3, picLogo2.Width, picLogo2.Height);
                }
            }

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label1.Text, new Font("Montserrat", 12, FontStyle.Bold), Brushes.Black, 410, 35, sf);

            StringFormat sff = new StringFormat();
            sff.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label2.Text, new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 410, 60, sff);

            //kotak foto
            Color reddd = Color.Black;
            Pen redddPen = new Pen(reddd);
            redddPen.Width = 1;
            e.Graphics.DrawRectangle(redddPen, 180, 120, 59, 15);
            e.Graphics.DrawString("KANAN", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 180, 120);


            //kotak foto
            Color red = Color.Black;
            Pen redPen = new Pen(red);
            redPen.Width = 1;
            e.Graphics.DrawRectangle(redPen, 590, 120, 35, 15);
            e.Graphics.DrawString("KIRI", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 590, 120);

            //e.Graphics.DrawImage(pictureBox1.Image, 10, 145, 383, 228);
            //e.Graphics.DrawImage(pictureBox4.Image, 411, 145, 383, 228);
            //e.Graphics.DrawImage(pictureBox2.Image, 10, 393, 383, 228);
            //e.Graphics.DrawImage(pictureBox5.Image, 411, 393, 383, 228);
            //e.Graphics.DrawImage(pictureBox3.Image, 10, 641, 383, 228);
            //e.Graphics.DrawImage(pictureBox6.Image, 411, 641, 383, 228); 


            e.Graphics.DrawImage(pictureBox1.Image, 33, 145, 383, 213);
            e.Graphics.DrawImage(pictureBox4.Image, 434, 145, 383, 213);
            e.Graphics.DrawImage(pictureBox2.Image, 33, 378, 383, 213);
            e.Graphics.DrawImage(pictureBox5.Image, 434, 378, 383, 213);
            e.Graphics.DrawImage(pictureBox3.Image, 33, 611, 383, 213);
            e.Graphics.DrawImage(pictureBox6.Image, 434, 611, 383, 213);
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

        private void printDocument3_PrintPage(object sender, PrintPageEventArgs e)
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label1.Text, new Font("Montserrat", 12, FontStyle.Bold), Brushes.Black, 410, 35, sf);

            StringFormat sff = new StringFormat();
            sff.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label2.Text, new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 410, 60, sff);

            //kotak foto
            Color reddd = Color.Black;
            Pen redddPen = new Pen(reddd);
            redddPen.Width = 1;
            e.Graphics.DrawRectangle(redddPen, 180, 120, 59, 15);
            e.Graphics.DrawString("KANAN", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 180, 120);


            //kotak foto
            Color red = Color.Black;
            Pen redPen = new Pen(red);
            redPen.Width = 1;
            e.Graphics.DrawRectangle(redPen, 590, 120, 35, 15);
            e.Graphics.DrawString("KIRI", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 590, 120);

            //Config 1

            float contrast = 1.02f;
            float gamma = 0.78f;
            float redd = 1.56f;
            float green = 1.35f;
            float blue = 1.28f;

            //Config 2

            //float contrast = 1.41f;
            //float gamma = 0.715f;

            ImageAttributes ia = new ImageAttributes();
            float[][] ptsarray = {
                        new float[] { contrast + redd, 0f, 0f, 0f, 0f},
                        new float[] { 0f, contrast + green, 0f, 0f, 0f},
                        new float[] { 0f, 0f, contrast + blue, 0f, 0f},
                        new float[] { 0f, 0f,       0f, 1f, 0f},
                        new float[] {   0, 0,        0, 1f, 1f},
                };
            ia.ClearColorMatrix();
            ia.SetColorMatrix(new ColorMatrix(ptsarray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            ia.SetGamma(gamma, ColorAdjustType.Bitmap);
            e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(10, 145, 383, 228), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox4.Image, new Rectangle(411, 145, 383, 228), 0, 0, pictureBox4.Image.Width, pictureBox4.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox2.Image, new Rectangle(10, 393, 383, 228), 0, 0, pictureBox2.Image.Width, pictureBox2.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox5.Image, new Rectangle(411, 393, 383, 228), 0, 0, pictureBox5.Image.Width, pictureBox5.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox3.Image, new Rectangle(10, 641, 383, 228), 0, 0, pictureBox3.Image.Width, pictureBox3.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox6.Image, new Rectangle(411, 641, 383, 228), 0, 0, pictureBox6.Image.Width, pictureBox6.Image.Height, GraphicsUnit.Pixel, ia);
            //e.Graphics.DrawImage(picLogo.Image, 5, 0, 100, 100);
            ia.Dispose();

            //e.Graphics.DrawImage(pictureBox1.Image, 10, 145, 383, 228);
            //e.Graphics.DrawImage(pictureBox4.Image, 411, 145, 383, 228);
            //e.Graphics.DrawImage(pictureBox2.Image, 10, 393, 383, 228);
            //e.Graphics.DrawImage(pictureBox5.Image, 411, 393, 383, 228);
            //e.Graphics.DrawImage(pictureBox3.Image, 10, 641, 383, 228);
            //e.Graphics.DrawImage(pictureBox6.Image, 411, 641, 383, 228);
            //e.Graphics.DrawImage(picLogo.Image, 5, 0, 100, 100);
        }

        private void printDocument4_PrintPage(object sender, PrintPageEventArgs e)
        {

            // Check the value of logoValue
            if (logoValue == "1")
            {
                //AdjustPictureBoxSize(e.Graphics, "Persegi");
                //e.Graphics.DrawImage(picLogo1.Image, 19, 0, picLogo1.Width, picLogo1.Height);

                if (jenisValue == "Persegi Panjang")
                {
                    // Handle case when logoValue is "1" and jenisValue is "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, jenisValue);
                    e.Graphics.DrawImage(picLogo2.Image, 10, 5, picLogo2.Width, picLogo2.Height);

                    //MessageBox.Show("Persegi Panjang");
                }
                else
                {
                    // Handle case when logoValue is "1" and jenisValue is not "Persegi Panjang"
                    AdjustPictureBoxSize(e.Graphics, "Persegi");
                    e.Graphics.DrawImage(picLogo1.Image, 10, 5, picLogo1.Width, picLogo1.Height);
                    //MessageBox.Show("Persegi");
                }
            }
            else if (logoValue == "2")
            {
                AdjustPictureBoxSize(e.Graphics, jenisValue); // Adjust the size based on jenisValue
                e.Graphics.DrawImage(picLogo1.Image, 5, 5, picLogo1.Width, picLogo1.Height);

                // Adjust the coordinates based on jenisValue
                if (jenisValue == "Persegi")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 695, 5, picLogo2.Width, picLogo2.Height);
                }
                else if (jenisValue == "Persegi Panjang")
                {
                    e.Graphics.DrawImage(picLogo2.Image, 595, 5, picLogo2.Width, picLogo2.Height);
                }

            }
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label1.Text, new Font("Montserrat", 12, FontStyle.Bold), Brushes.Black, 410, 35, sf);

            StringFormat sff = new StringFormat();
            sff.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(label2.Text, new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 410, 60, sff);

            //kotak foto
            Color reddd = Color.Black;
            Pen redddPen = new Pen(reddd);
            redddPen.Width = 1;
            e.Graphics.DrawRectangle(redddPen, 180, 120, 59, 15);
            e.Graphics.DrawString("KANAN", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 180, 120);


            //kotak foto
            Color red = Color.Black;
            Pen redPen = new Pen(red);
            redPen.Width = 1;
            e.Graphics.DrawRectangle(redPen, 590, 120, 35, 15);
            e.Graphics.DrawString("KIRI", new Font("Montserrat", 10, FontStyle.Bold), Brushes.Black, 590, 120);

            //Config 1

            //float contrast = 1.02f;
            //float gamma = 0.78f;

            //Config 2

            //float contrast = 1.41f;
            float contrast = 1.00f;
            float gamma = 0.715f;
            float redd = 0.56f;
            float green = 0.35f;
            float blue = 0.28f;

            ImageAttributes ia = new ImageAttributes();
            float[][] ptsarray = {
                        new float[] { contrast + redd, 0f, 0f, 0f, 0f},
                        new float[] { 0f, contrast + green, 0f, 0f, 0f},
                        new float[] { 0f, 0f, contrast + blue, 0f, 0f},
                        new float[] { 0f, 0f,       0f, 1f, 0f},
                        new float[] {   0, 0,        0, 1f, 1f},
                };
            ia.ClearColorMatrix();
            ia.SetColorMatrix(new ColorMatrix(ptsarray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            ia.SetGamma(gamma, ColorAdjustType.Bitmap);

            e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(9, 145, 383, 213), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox4.Image, new Rectangle(410, 145, 383, 213), 0, 0, pictureBox4.Image.Width, pictureBox4.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox2.Image, new Rectangle(9, 378, 383, 213), 0, 0, pictureBox2.Image.Width, pictureBox2.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox5.Image, new Rectangle(410, 378, 383, 213), 0, 0, pictureBox5.Image.Width, pictureBox5.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox3.Image, new Rectangle(9, 611, 383, 213), 0, 0, pictureBox3.Image.Width, pictureBox3.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox6.Image, new Rectangle(410, 611, 383, 213), 0, 0, pictureBox6.Image.Width, pictureBox6.Image.Height, GraphicsUnit.Pixel, ia);
            //e.Graphics.DrawImage(picLogo.Image, 5, 0, 100, 100);
            ia.Dispose();

            //e.Graphics.DrawImage(pictureBox1.Image, 9, 145, 383, 213);
            //e.Graphics.DrawImage(pictureBox4.Image, 410, 145, 383, 213);
            //e.Graphics.DrawImage(pictureBox2.Image, 9, 378, 383, 213);
            //e.Graphics.DrawImage(pictureBox5.Image, 410, 378, 383, 213);
            //e.Graphics.DrawImage(pictureBox3.Image, 9, 611, 383, 213);
            //e.Graphics.DrawImage(pictureBox6.Image, 410, 611, 383, 213);
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(pictureBox1.Image, 18, 38, 177, 100);
            e.Graphics.DrawImage(pictureBox4.Image, 198, 38, 177, 100);
            e.Graphics.DrawImage(pictureBox2.Image, 18, 141, 177, 100);
            e.Graphics.DrawImage(pictureBox5.Image, 198, 141, 177, 100);
            e.Graphics.DrawImage(pictureBox3.Image, 18, 244, 177, 100);
            e.Graphics.DrawImage(pictureBox6.Image, 198, 244, 177, 100);
        }

        private void printDocument5_PrintPage(object sender, PrintPageEventArgs e)
        {
            //e.Graphics.DrawImage(pictureBox1.Image, 18, 38, 177, 100);
            //e.Graphics.DrawImage(pictureBox4.Image, 198, 38, 177, 100);
            //e.Graphics.DrawImage(pictureBox2.Image, 18, 141, 177, 100);
            //e.Graphics.DrawImage(pictureBox5.Image, 198, 141, 177, 100);
            //e.Graphics.DrawImage(pictureBox3.Image, 18, 244, 177, 100);
            //e.Graphics.DrawImage(pictureBox6.Image, 198, 244, 177, 100);

            //Config 1

            //float contrast = 1.02f;
            //float gamma = 0.78f;

            //Config 2

            //float contrast = 1.41f;
            float contrast = 1.00f;
            float gamma = 0.715f;
            float redd = 0.56f;
            float green = 0.35f;
            float blue = 0.28f;

            ImageAttributes ia = new ImageAttributes();
            float[][] ptsarray = {
                        new float[] { contrast + redd, 0f, 0f, 0f, 0f},
                        new float[] { 0f, contrast + green, 0f, 0f, 0f},
                        new float[] { 0f, 0f, contrast + blue, 0f, 0f},
                        new float[] { 0f, 0f,       0f, 1f, 0f},
                        new float[] {   0, 0,        0, 1f, 1f},
                };
            ia.ClearColorMatrix();
            ia.SetColorMatrix(new ColorMatrix(ptsarray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            ia.SetGamma(gamma, ColorAdjustType.Bitmap);
            e.Graphics.DrawImage(pictureBox1.Image, new Rectangle(18, 38, 177, 100), 0, 0, pictureBox1.Image.Width, pictureBox1.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox4.Image, new Rectangle(198, 38, 177, 100), 0, 0, pictureBox4.Image.Width, pictureBox4.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox2.Image, new Rectangle(18, 141, 177, 100), 0, 0, pictureBox2.Image.Width, pictureBox2.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox5.Image, new Rectangle(198, 141, 177, 100), 0, 0, pictureBox5.Image.Width, pictureBox5.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox3.Image, new Rectangle(18, 244, 177, 100), 0, 0, pictureBox3.Image.Width, pictureBox3.Image.Height, GraphicsUnit.Pixel, ia);
            e.Graphics.DrawImage(pictureBox6.Image, new Rectangle(198, 244, 177, 100), 0, 0, pictureBox6.Image.Width, pictureBox6.Image.Height, GraphicsUnit.Pixel, ia);
            //e.Graphics.DrawImage(picLogo.Image, 5, 0, 100, 100);
            ia.Dispose();
        }

        private void button4_MouseMove(object sender, MouseEventArgs e)
        {
            button4.BackColor = Color.FromArgb(255, 153, 153);
        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            button4.BackColor = Color.White;
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
        //        textBox4.Text = print1.ToString();
        //    }
        //    else
        //    {
        //        int print2 = 2;
        //        textBox2.Text = print2.ToString();
        //        textBox4.Text = print2.ToString();
        //    }
        //}

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            string Pname = comboBox1.SelectedItem.ToString();
            printer.SetDefaultPrinter(Pname);

            if (comboBox1.Text == "Canon SELPHY CP1300")
            {
                // Sembunyikan item "A4" di comboBox2
                //if (comboBox2.Items.Contains("A4"))
                //{
                //    comboBox2.Items.Remove("A4");
                //}
                // Sembunyikan elemen-elemen UI tertentu
                picLogo1.Visible = false;
                picLogo2.Visible = false;
                panel4.Visible = false;
                panel5.Visible = false;
                textBox8.Visible = false;
                textBox9.Visible = false;
                label1.Visible = false;
                label2.Visible = false;

                int print1 = 1;
                textBox2.Text = print1.ToString();
                textBox4.Text = print1.ToString();
            }
            else
            {
                // Jika printer lain dipilih, pastikan item "A4" ada di comboBox2
                //if (!comboBox2.Items.Contains("A4"))
                //{
                //    comboBox2.Items.Add("A4");
                //}
                // Tampilkan kembali elemen-elemen UI tersebut jika pilihan printer berbeda
                picLogo1.Visible = true;
                panel4.Visible = true;
                panel5.Visible = true;
                textBox8.Visible = true;
                textBox9.Visible = true;
                label1.Visible = true;
                label2.Visible = true;

                int print2 = 2;
                textBox2.Text = print2.ToString();
                textBox4.Text = print2.ToString();
            }
        }

        //private void btlPrint_Click(object sender, EventArgs e)
        //{
        //    if (pictureBox1.Image == null || pictureBox2.Image == null || pictureBox3.Image == null || pictureBox4.Image == null || pictureBox5.Image == null || pictureBox6.Image == null)
        //    {
        //        MessageBox.Show("Foto diisi Dahulu ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //    else if (comboBox1.SelectedIndex == -1)
        //    {
        //        MessageBox.Show("Pilih printer terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //    else
        //    {
        //        comboBox2.SelectedIndex = -1; // Reset comboBox2 selection

        //        PrintDocument pd = new PrintDocument();
        //        pd.DefaultPageSettings.Landscape = false;

        //        if (textBox2.Text == "1")
        //        {
        //            if (comboBox3.SelectedIndex == -1)
        //            {
        //                MessageBox.Show("Pilih profil terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //                return;
        //            }

        //            pd.DefaultPageSettings.Landscape = true;

        //            if (comboBox3.Text == "Default")
        //            {
        //                pd.PrintPage += new PrintPageEventHandler(this.printDocument1_PrintPage);
        //            }
        //            else if (comboBox3.Text == "Adjust Brightness")
        //            {
        //                pd.PrintPage += new PrintPageEventHandler(this.printDocument5_PrintPage);
        //            }
        //            pd.Print();
        //            HistoryPrint4R(comboBox3.Text);
        //        }
        //        else if (textBox2.Text == "2")
        //        {
        //            if (comboBox3.SelectedIndex == -1)
        //            {
        //                MessageBox.Show("Pilih profil terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //                return;
        //            }

        //            pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
        //            pd.DefaultPageSettings.Landscape = false;

        //            if (comboBox3.Text == "Default")
        //            {
        //                pd.PrintPage += new PrintPageEventHandler(this.printDocument2_PrintPage);
        //            }
        //            else if (comboBox3.Text == "Adjust Brightness")
        //            {
        //                pd.PrintPage += new PrintPageEventHandler(this.printDocument4_PrintPage);
        //            }
        //            pd.Print();

        //            //printPreviewDialog1.Document = pd;
        //            //printPreviewDialog1.ShowDialog();
        //            HistoryPrintA4(comboBox3.Text);
        //        }
        //        else
        //        {
        //            MessageBox.Show("Nomor print tidak valid", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        // Reset UI elements after printing
        //        comboBox1.Items.Clear();
        //        comboBox1.ResetText();
        //        pictureBox1.Image.Dispose();
        //        pictureBox1.Image = null;
        //        pictureBox2.Image.Dispose();
        //        pictureBox2.Image = null;
        //        pictureBox3.Image.Dispose();
        //        pictureBox3.Image = null;
        //        pictureBox4.Image.Dispose();
        //        pictureBox4.Image = null;
        //        pictureBox5.Image.Dispose();
        //        pictureBox5.Image = null;
        //        pictureBox6.Image.Dispose();
        //        pictureBox6.Image = null;
        //        buttobDeleteFalse();
        //        buttobAddTrue();
        //        button4.PerformClick();
        //        int kondisi1 = 3;
        //        TransfEventPrint6G(kondisi1.ToString());
        //        MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //}


        private void btlPrint_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null || pictureBox2.Image == null || pictureBox3.Image == null || pictureBox4.Image == null || pictureBox5.Image == null || pictureBox6.Image == null)
            {
                MessageBox.Show("Foto diisi Dahulu ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                comboBox2.SelectedIndex = -1;
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
                        PrintDocument pd = new PrintDocument();
                        pd.DefaultPageSettings.Landscape = false;
                        if (comboBox3.Text == "Default")
                        {
                            pd.PrintPage += new PrintPageEventHandler(this.printDocument1_PrintPage);
                            pd.Print();
                            //printPreviewDialog1.Document = pd;
                            //printPreviewDialog1.ShowDialog();
                            HistoryPrint4R(comboBox3.Text);
                            comboBox1.Items.Clear();
                            comboBox1.ResetText();
                            FillListBox();
                            pictureBox1.Image.Dispose();
                            pictureBox1.Image = null;
                            pictureBox2.Image.Dispose();
                            pictureBox2.Image = null;
                            pictureBox3.Image.Dispose();
                            pictureBox3.Image = null;
                            pictureBox4.Image.Dispose();
                            pictureBox4.Image = null;
                            pictureBox5.Image.Dispose();
                            pictureBox5.Image = null;
                            pictureBox6.Image.Dispose();
                            pictureBox6.Image = null;

                            buttobDeleteFalse();
                            //buttobAddTrue();

                            button4.PerformClick();
                            int kondisi1 = 3;
                            TransfEventPrint6G(kondisi1.ToString());
                            MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (comboBox3.Text == "Adjust Brightness")
                        {
                            pd.PrintPage += new PrintPageEventHandler(this.printDocument5_PrintPage);
                            pd.Print();
                            //printPreviewDialog1.Document = pd;
                            //printPreviewDialog1.ShowDialog();
                            HistoryPrint4R(comboBox3.Text);
                            comboBox1.Items.Clear();
                            comboBox1.ResetText();
                            FillListBox();
                            pictureBox1.Image.Dispose();
                            pictureBox1.Image = null;
                            pictureBox2.Image.Dispose();
                            pictureBox2.Image = null;
                            pictureBox3.Image.Dispose();
                            pictureBox3.Image = null;
                            pictureBox4.Image.Dispose();
                            pictureBox4.Image = null;
                            pictureBox5.Image.Dispose();
                            pictureBox5.Image = null;
                            pictureBox6.Image.Dispose();
                            pictureBox6.Image = null;

                            buttobDeleteFalse();
                            //buttobAddTrue();

                            button4.PerformClick();
                            int kondisi1 = 3;
                            TransfEventPrint6G(kondisi1.ToString());
                            MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        if (comboBox3.SelectedIndex == -1)
                        {
                            MessageBox.Show("Pilih profil terlebih dahulu ", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        PrintDocument pd = new PrintDocument();
                        pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("PaperA4", 840, 1180);
                        pd.DefaultPageSettings.Landscape = false;

                        if (comboBox3.Text == "Default")
                        {
                            pd.PrintPage += new PrintPageEventHandler(this.printDocument2_PrintPage);
                            pd.Print();
                            //printPreviewDialog1.Document = pd;
                            //printPreviewDialog1.ShowDialog();
                            HistoryPrintA4(comboBox3.Text);
                            comboBox1.Items.Clear();
                            comboBox1.ResetText();
                            FillListBox();
                            pictureBox1.Image.Dispose();
                            pictureBox1.Image = null;
                            pictureBox2.Image.Dispose();
                            pictureBox2.Image = null;
                            pictureBox3.Image.Dispose();
                            pictureBox3.Image = null;
                            pictureBox4.Image.Dispose();
                            pictureBox4.Image = null;
                            pictureBox5.Image.Dispose();
                            pictureBox5.Image = null;
                            pictureBox6.Image.Dispose();
                            pictureBox6.Image = null;
                            buttobDeleteFalse();
                            //buttobAddTrue();
                            button4.PerformClick();
                            int kondisi1 = 3;
                            TransfEventPrint6G(kondisi1.ToString());
                            MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (comboBox3.Text == "Adjust Brightness")
                        {
                            pd.PrintPage += new PrintPageEventHandler(this.printDocument4_PrintPage);
                            pd.Print();
                            //printPreviewDialog1.Document = pd;
                            //printPreviewDialog1.ShowDialog();
                            HistoryPrintA4(comboBox3.Text);
                            comboBox1.Items.Clear();
                            comboBox1.ResetText();
                            FillListBox();
                            pictureBox1.Image.Dispose();
                            pictureBox1.Image = null;
                            pictureBox2.Image.Dispose();
                            pictureBox2.Image = null;
                            pictureBox3.Image.Dispose();
                            pictureBox3.Image = null;
                            pictureBox4.Image.Dispose();
                            pictureBox4.Image = null;
                            pictureBox5.Image.Dispose();
                            pictureBox5.Image = null;
                            pictureBox6.Image.Dispose();
                            pictureBox6.Image = null;
                            buttobDeleteFalse();
                            //buttobAddTrue();
                            button4.PerformClick();
                            int kondisi1 = 3;
                            TransfEventPrint6G(kondisi1.ToString());
                            MessageBox.Show("Dokumen berhasil diprint.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void buttobDeleteFalse()
        {
            close1.Visible = false;
            close2.Visible = false;
            close3.Visible = false;
            close4.Visible = false;
            close5.Visible = false;
            close6.Visible = false;

            btlPrint.Focus();
        }

        //private void buttobAddTrue()
        //{
        //    add1.Visible = true;
        //    add2.Visible = true;
        //    add3.Visible = true;
        //    add4.Visible = true;
        //    add5.Visible = true;


        //}
        private void button8_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null || pictureBox2.Image == null || pictureBox3.Image == null || pictureBox4.Image == null || pictureBox5.Image == null || pictureBox6.Image == null)
            {
                MessageBox.Show("Foto diisi Dahulu ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (comboBox2.Text == "4R" || comboBox2.Text == "A4")
                {
                    int s = 1;
                    int r = 2;
                    if (kondisiPDF == s.ToString())
                    {
                        savePDF4R();
                        comboBox2.SelectedIndex = -1;
                        comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                        FillListBox();
                        buttobDeleteFalse();
                        //buttobAddTrue();
                        button4.PerformClick();
                        int kondisi1 = 3;
                        TransfEventPrint6G(kondisi1.ToString());
                        MessageBox.Show("Export PDF berhasil", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (kondisiPDF == r.ToString())
                    {
                        savePDFA4();
                        comboBox2.SelectedIndex = -1;
                        comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                        FillListBox();
                        buttobDeleteFalse();
                        //buttobAddTrue();
                        button4.PerformClick();
                        int kondisi1 = 3;
                        TransfEventPrint6G(kondisi1.ToString());
                        MessageBox.Show("Export PDF berhasil", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Pilih ukuran kertas", " ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void Form6Print_Load(object sender, EventArgs e)
        {
            this.ActiveControl = label5;
            string dirlogo1 = dirLogo + "logo1.png";
            //string dirlogo1 = dir + "1160358.png";
            if (!Directory.Exists(dirlogo1))
            {
                picLogo1.Image = Image.FromFile(dirLogo + "logo1.png");
                picLogo2.Image = Image.FromFile(dirLogo + "logo2.png");
            }

            buttobDeleteFalse();

            if (comboBox4.Items.Count > 0) // Pastikan ComboBox memiliki item
            {
                comboBox4.SelectedIndex = comboBox4.Items.Count - 1; // Pilih item terakhir
            }

            // Menonaktifkan dropdown
            comboBox4.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;

            pictureBox1.Image = Properties.Resources.icon;
            pictureBox2.Image = Properties.Resources.icon;
            pictureBox3.Image = Properties.Resources.icon;
            pictureBox4.Image = Properties.Resources.icon;
            pictureBox5.Image = Properties.Resources.icon;
            pictureBox6.Image = Properties.Resources.icon;

            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            pictureBox2.Image.Dispose();
            pictureBox2.Image = null;
            pictureBox3.Image.Dispose();
            pictureBox3.Image = null;
            pictureBox4.Image.Dispose();
            pictureBox4.Image = null;
            pictureBox5.Image.Dispose();
            pictureBox5.Image = null;
            pictureBox6.Image.Dispose();
            pictureBox6.Image = null;
            comboBox1.Items.Clear();
            comboBox1.ResetText();
            FillListBox();
            int kondisi = 5;
            TransfEventttt(kondisi.ToString());
            this.Close();
        }

        private void add1_Click(object sender, EventArgs e)
        {
            tanggal = DateTime.Now.ToString("ddMMyyy");
            string text = DateTime.Now.ToString("Y");
            string[] arr = text.Split(' ');
            splitBulan = arr[0];
            splitTahun = arr[1];

            comboBox2.SelectedIndex = -1;
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";
            of.InitialDirectory = "D:\\GLEndoscope\\" + splitTahun + "\\" + splitBulan + "\\" + tanggal + "\\" + gabung + "\\Image";

            if (of.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.ImageLocation = of.FileName;
                close1.Visible = true;
                //add1.Visible = false;
            }
        }

        private void close1_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            //add1.Visible = true;
            close1.Visible = false;
            btlPrint.Focus();
        }

        private void add2_Click(object sender, EventArgs e)
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
                pictureBox2.ImageLocation = of.FileName;
                close2.Visible = true;
                //add2.Visible = false;
            }
        }

        private void close2_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;
            pictureBox2.Image.Dispose();
            pictureBox2.Image = null;
            //add2.Visible = true;
            close2.Visible = false;
            btlPrint.Focus();

        }

        private void add3_Click(object sender, EventArgs e)
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
                pictureBox3.ImageLocation = of.FileName;
                close3.Visible = true;
                //add3.Visible = false;
            }
        }

        private void close3_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;
            pictureBox3.Image.Dispose();
            pictureBox3.Image = null;
            //add3.Visible = true;
            close3.Visible = false;
            btlPrint.Focus();

        }

        private void add4_Click(object sender, EventArgs e)
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
                pictureBox4.ImageLocation = of.FileName;
                close4.Visible = true;
                //add4.Visible = false;
            }
        }

        private void close4_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;
            pictureBox4.Image.Dispose();
            pictureBox4.Image = null;
            //add4.Visible = true;
            close4.Visible = false;
            btlPrint.Focus();

        }

        private void add5_Click(object sender, EventArgs e)
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
                pictureBox5.ImageLocation = of.FileName;
                close5.Visible = true;
                //add5.Visible = false;
            }
        }

        private void close6_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;
            pictureBox6.Image.Dispose();
            pictureBox6.Image = null;
            //add6.Visible = true;
            close6.Visible = false;
            btlPrint.Focus();

        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {
            if (panel6.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 2;//it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.Black, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness, halfThickness, panel6.ClientSize.Width - thickness, panel6.ClientSize.Height - thickness));
                }
            }
        }

        private void add6_Click(object sender, EventArgs e)
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
                pictureBox6.ImageLocation = of.FileName;
                close6.Visible = true;
                //add6.Visible = false;

            }
        }

        private void close5_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;
            pictureBox5.Image.Dispose();
            pictureBox5.Image = null;

            //add5.Visible = true;
            close5.Visible = false;
            btlPrint.Focus();

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

        private void button5_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;

            pictureBox1.Image = Properties.Resources.icon;
            pictureBox2.Image = Properties.Resources.icon;
            pictureBox3.Image = Properties.Resources.icon;
            pictureBox4.Image = Properties.Resources.icon;
            pictureBox5.Image = Properties.Resources.icon;
            pictureBox6.Image = Properties.Resources.icon;

            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            pictureBox2.Image.Dispose();
            pictureBox2.Image = null;
            pictureBox3.Image.Dispose();
            pictureBox3.Image = null;
            pictureBox4.Image.Dispose();
            pictureBox4.Image = null;
            pictureBox5.Image.Dispose();
            pictureBox5.Image = null;
            pictureBox6.Image.Dispose();
            pictureBox6.Image = null;

            comboBox1.Items.Clear();
            comboBox1.ResetText();
            FillListBox();
            int kondisi1 = 1;
            TransfEventPrint1(kondisi1.ToString());
            this.Close();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1;

            pictureBox1.Image = Properties.Resources.icon;
            pictureBox2.Image = Properties.Resources.icon;
            pictureBox3.Image = Properties.Resources.icon;
            pictureBox4.Image = Properties.Resources.icon;
            pictureBox5.Image = Properties.Resources.icon;
            pictureBox6.Image = Properties.Resources.icon;

            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            pictureBox2.Image.Dispose();
            pictureBox2.Image = null;
            pictureBox3.Image.Dispose();
            pictureBox3.Image = null;
            pictureBox4.Image.Dispose();
            pictureBox4.Image = null;
            pictureBox5.Image.Dispose();
            pictureBox5.Image = null;
            pictureBox6.Image.Dispose();
            pictureBox6.Image = null;
            comboBox1.Items.Clear();
            comboBox1.ResetText();

            FillListBox();
            int kondisi1 = 2;
            TransfEventPrint6(kondisi1.ToString());
            this.Close();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "")
            {
                // Specify the path for the CSV file
                string csvFilePath = "D:\\GLEndoscope\\Database\\dataPasien\\dataDefault.csv";

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
                        picLogo2.Size = new Size(100, 100);

                        //label1.Size = new Size(538, 23);
                        //label1.Location = new Point(164, 27);

                        //label2.Size = new Size(613, 23);
                        //label2.Location = new Point(127, 50);

                    }
                    else if (jenisValue == "Persegi Panjang")
                    {
                        picLogo1.Size = new Size(200, 100);
                        picLogo2.Size = new Size(200, 100);
                        picLogo2.Location = new Point(647, 10);

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

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            if (comboBox2.Text == "4R")
            {
                int print1 = 1;
                kondisiPDF = print1.ToString();
            }
            else
            {
                int print2 = 2;
                kondisiPDF = print2.ToString();
            }
        }
        private void savePDF4R()
        {
            string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\EksporPDF\Format-1\6-Gambar\4R\";
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
                pdoc.PrintPage += printDocument1_PrintPage;
                pdoc.Print();
            }
        }

        private void HistoryPrint4R(string profile)
        {
            string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\History Print\Format-1\6-Gambar\4R\";
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
                pdoc.PrintPage += printDocument1_PrintPage;
                pdoc.Print();
            }
        }

        private void savePDFA4()
        {
            string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\EksporPDF\Format-1\6-Gambar\A4\";
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
            string dir = @"D:\GLEndoscope\" + splitTahun + @"\" + splitBulan + @"\" + tanggal + @"\" + gabung + @"\History Print\Format-1\6-Gambar\A4\";

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