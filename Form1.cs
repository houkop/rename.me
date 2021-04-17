using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ExifLib;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Specialized;
using System.Globalization;



namespace rename.me
{

    public partial class Form1 : Form
    {
        public NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

        //объявляем списки 
        List<double> latit = new List<double>();
        List<double> longit = new List<double>();
        //каунтеры к спискам
        int numLat = 0;
        int numLong = 0;
        int debug = 0;

        public Form1()
        {
            InitializeComponent();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var remDrives = from i in allDrives
                            where i.DriveType == DriveType.Removable
                            where i.IsReady == true
                            select i;
            int rdC = remDrives.Count();
            if (rdC > 0)
            {
                if (Directory.Exists(remDrives.Last().Name + "DCIM"))
                {
                    textBox1.Text = remDrives.Last().Name + "DCIM";
                }
                else
                {
                    textBox1.Text = remDrives.Last().Name;
                }
            }
            textBox2.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.DoubleBuffered = true;
        }

        string mdfolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public void Button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog b1c = new FolderBrowserDialog();
            b1c.RootFolder = System.Environment.SpecialFolder.MyComputer;

            if (b1c.ShowDialog(this) == DialogResult.OK)
            {
                string startfolder = b1c.SelectedPath;
                textBox1.Text = startfolder;
            }
        }

        public void Button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog b2c = new FolderBrowserDialog();
            b2c.RootFolder = System.Environment.SpecialFolder.MyComputer;

            if (b2c.ShowDialog(this) == DialogResult.OK)
            {
                string startfolder = b2c.SelectedPath;
                textBox2.Text = startfolder;
            }
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            Rename();
        }


        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void TextBox2_TextChanged(object sender, EventArgs e)
        {

        }
        public void Rename()
        {
            //запускаем таймер
            Stopwatch stpwtch = Stopwatch.StartNew();
            latit = new List<double>();
            longit = new List<double>();
            //обнуляем каунтеры к спискам
            numLat = 0;
            numLong = 0;
            int pltbreak = 1;
            string togpx = "";
            string toplt = "";

            ref string refTogpx = ref togpx;
            ref string refToPlt = ref toplt;
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!DEBUG!!!!!!!!!!
            if (debug == 1)
            {
                //textBox1.Text = "C:\\Users\\entei\\YandexDisk\\LA";
                textBox1.Text = "D:\\test\\";
                textBox2.Text = "D:\\tmp\\";
                checkBox2.Checked = true; // проверять подпапки
                checkBox3.Checked = true; // созадать только файл трека
                checkBox4.Checked = true; // показать сразу трек
            }
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!DEBUG!!!!!!!!!!
            if (checkBox1.Checked)
            {
                stpwtch.Stop();
                ShowTrack();
                return;
            }
            string startfolder = textBox1.Text;
            string exportfolder = textBox2.Text;
            string prefix = textBox3.Text;
            progressBar1.Value = progressBar1.Minimum;
            
            //проверяем наличие исходной папки

            bool chkD = Directory.Exists(startfolder);

            if (!chkD)
            {
                MessageBox.Show("Папка не найдена, проверьте правильность указания исходной папки", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {

                DirectoryInfo d = new DirectoryInfo(startfolder);
                string today = DateTime.Today.ToString("yyyyMMdd");
                string gpxfile = exportfolder + "\\" + today + "copter.gpx";
                string pltfile = exportfolder + "\\" + today + "copter.plt";
                //проверяем наличие в папке-получателе файла gpx и plt
                bool folderexist = Directory.Exists(exportfolder);
                bool fexgpx = File.Exists(gpxfile);
                bool fexplt = File.Exists(pltfile);
                //если файла нет - создаем и пишем заголовок
                if (!folderexist)
                {
                    var ans = MessageBox.Show("Ошибка! \r\n" + "Папка назначения не найдена \r\n" + "Создать папку по данному пути?\r\n" + exportfolder, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if ( ans == DialogResult.Yes )
                    {
                        Directory.CreateDirectory(exportfolder);
                    }
                    else
                    {
                        return;
                    }
                    
                }
                if (!fexgpx)
                {
                    //File.Create(gpxfile);
                    FileStream fstream = new FileStream(gpxfile, FileMode.OpenOrCreate);
                    fstream.Close();
                    try
                    {
                        // записываем начало gpx файла 
                        refTogpx = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                            "<gpx version=\"1.0\" creator = \"Rename.Me by hompfull\">\r\n" +
                            "<trk>\r\n" +
                            "<name></name>\r\n" +
                            "<trkseg>\r\n";
                    }
                    catch (Exception exep)
                    {
                        MessageBox.Show("ошибка \r\n" + exep.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                //если файл gpx есть - удаляем последние 3 строчки чтоб начать дозапись
                else
                {
                    var lines = System.IO.File.ReadAllLines(gpxfile);
                    System.IO.File.WriteAllLines(gpxfile, lines.Take(lines.Length - 3).ToArray());
                }
                if (!fexplt)
                {
                    //File.Create(pltfile);
                    FileStream fstream = new FileStream(pltfile, FileMode.OpenOrCreate);
                    fstream.Close();
                    try
                    {
                        // записываем начало gpx файла 
                        refToPlt = "OziExplorer Track Point File Version 2.1\r\n" +
                            "WGS 84\r\n" +
                            "Altitude is in Feet\r\n" +
                            "Reserved 3\r\n" +
                            "0,2,16776960," + today + "copter,0,0,2,16776960,0\r\n" +
                            "0\r\n";
                    }
                    catch (Exception exep)
                    {
                        MessageBox.Show("ошибка \r\n" + exep.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                //проверяем что в целевой директории нет папки с текущей датой, если нужно - создаем
                bool moved = Directory.Exists(exportfolder + @"\" + today);
                if (!moved && !checkBox3.Checked)
                {
                    Directory.CreateDirectory(exportfolder + @"\" + today);
                }
                //обработка файлов;
                FileInfo[] diInfos;
                //если стоит галочка "проверить подпапки", ищем все JPG
                if (checkBox2.Checked)
                {
                    diInfos = d.GetFiles("*.JPG", SearchOption.AllDirectories);
                }
                //если галочка "проверить подпапки" не стоит
                else
                {
                    diInfos = d.GetFiles("*.JPG", SearchOption.TopDirectoryOnly);
                }
                //выставляем максимально значение прогрессбара
                progressBar1.Maximum = progressBar1.Maximum + diInfos.Length;
                //добавочное число
                int nums = 0;
                foreach (FileInfo f in diInfos)
                {
                    progressBar1.Value++;
                    ////берем все что после "DJI_"
                    //string rest = f.Name.Split('_')[1];
                    //проверяем что в папке назначения нет файла с таким именем
                    bool fileex = File.Exists(exportfolder + @"\" + today + @"\" + prefix + "_" + nums.ToString("D8"));
                    //если есть - увеличиваем число в названии файла
                    while (fileex)
                    {
                        nums++;
                        fileex = File.Exists(exportfolder + @"\" + today + @"\" + prefix + "_" + nums.ToString("D8"));
                    }
                    //собираем данные
                    using (var reader = new ExifReader(f.FullName))
                    {

                        if (reader.GetTagValue(ExifTags.GPSLatitude, out double[] gpslat))
                        { }
                        double reslat = Math.Round(gpslat[0] + gpslat[1] / 60 + gpslat[2] / 3600, 8);
                        string reslatplt = (Math.Round(reslat, 6)).ToString("F6", nfi);
                        if (reader.GetTagValue(ExifTags.GPSLongitude, out double[] gpslong))
                        { }
                        double reslong = Math.Round(gpslong[0] + gpslong[1] / 60 + gpslong[2] / 3600, 8);
                        string reslongplt = (Math.Round(reslong, 6)).ToString("F6", nfi);
                        
                        if (reader.GetTagValue(ExifTags.GPSAltitude, out double gpsalt))
                        { }

                        byte gpsaltref;
                        if (reader.GetTagValue(ExifTags.GPSAltitudeRef, out gpsaltref))
                        {
                            if (gpsaltref == 1)
                                gpsalt = -gpsalt;
                        }
                        string altInFeet = (Math.Round(gpsalt * 3.2808, 1)).ToString( "F1", nfi );
                        if (reader.GetTagValue(ExifTags.DateTimeOriginal, out DateTime dtorig))
                        {
                            dtorig = dtorig.AddHours(-3);
                        }
                        string dtorigstr = dtorig.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string dOrigStr = dtorig.ToString("dd-MMM-yy");
                        string tOrigStr = dtorig.ToString("H:mm:ss");
                        string delphiDate = (Math.Round(dtorig.ToOADate(),7)).ToString( "F7", nfi );

                        latit.Insert(numLat, reslat);
                        numLat++;
                        longit.Insert(numLong, reslong);
                        numLong++;

                        //основные данные трека
                        try
                        {
                            refTogpx = refTogpx + "<trkpt lat=\"" + reslat + "\" lon=\"" + reslong + "\">\r\n" +
                                "<time>" + dtorigstr + "</time>\r\n" +
                                "<ele>" + gpsalt + "</ele>\r\n" +
                                "</trkpt>\r\n";
                            refToPlt = refToPlt + reslatplt + "," + reslongplt + "," + pltbreak + "," + altInFeet + "," + delphiDate + "," + dOrigStr + "," + tOrigStr + "\r\n"; 
                            pltbreak = 0;

                        }
                        catch (Exception exep)
                        {
                            MessageBox.Show("ошибка \r\n" + exep.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                    }
                    //перемещаем обработаннный файл
                    if (!checkBox3.Checked)
                    {
                        File.Move(f.FullName, f.FullName.Replace(f.FullName, exportfolder + @"\" + today + @"\" + prefix + "_" + nums.ToString("D8")));
                        nums++;
                    }

                }

                //завершаем запись трека

                refTogpx = refTogpx + "</trkseg>\r\n" +
                    "</trk>\r\n" +
                    "</gpx>\r\n";

                progressBar1.Value = progressBar1.Maximum;
                string enc = "";
                string cp = "";

                //записываем трек в файл                
                try
                {
                    StreamWriter sw = new StreamWriter(gpxfile, true, System.Text.Encoding.UTF8);
                    sw.Write(refTogpx);
                    sw.Close();

                    StreamWriter swplt = new StreamWriter(pltfile, true, System.Text.Encoding.GetEncoding("windows-1251"));
                    swplt.Write(refToPlt);
                    swplt.Close();
                    stpwtch.Stop();
                }
                catch (Exception exep)
                {
                    MessageBox.Show("ошибка \r\n" + exep.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //отображаем трек если надо
                if (checkBox4.Checked)
                {
                    ShowTrack();
                }
                if (debug == 1)
                {
                    string strStpwtch = stpwtch.Elapsed.TotalMilliseconds.ToString();
                    MessageBox.Show(strStpwtch + " encoding = " + enc + " cp " + cp, "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    MessageBox.Show("Готово", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

        }

        public void ShowTrack()
        {

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            try
            {
                //если нет данных в массивах
                if (numLat == 0)
                {
                    try
                    {
                        latit = new List<double>();
                        longit = new List<double>();
                        //обнуляем каунтеры к спискам
                        numLat = 0;
                        numLong = 0;

                        string togpx = "";
                        ref string refTogpx = ref togpx;
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!DEBUG!!!!!!!!!!
                        if (debug == 1)
                        {
                            //textBox1.Text = "C:\\Users\\entei\\YandexDisk\\LA";
                            textBox1.Text = "D:\\test\\";
                            textBox2.Text = "D:\\";
                            checkBox2.Checked = true;
                            checkBox3.Checked = true;
                        }
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!DEBUG!!!!!!!!!!
                        string startfolder = textBox1.Text;
                        string exportfolder = textBox2.Text;
                        string prefix = textBox3.Text;
                        progressBar1.Value = progressBar1.Minimum;

                        //проверяем наличие исходной папки
                        bool chkD = Directory.Exists(startfolder);
                        if (!chkD)
                        {
                            MessageBox.Show("Папка не найдена, проверьте правильность указания исходной папки", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        //если папка есть
                        else
                        {
                            DirectoryInfo d = new DirectoryInfo(startfolder);

                            //обработка файлов;
                            FileInfo[] diInfos;
                            //находим все файлы JPG
                            if (checkBox2.Checked)
                            {
                                diInfos = d.GetFiles("*.JPG", SearchOption.AllDirectories);
                            }
                            else
                            {
                                diInfos = d.GetFiles("*.JPG", SearchOption.TopDirectoryOnly);
                            }

                            foreach (FileInfo f in diInfos)
                            {
                                //собираем данные в массивы
                                using (var reader = new ExifReader(f.FullName))
                                {

                                    if (reader.GetTagValue(ExifTags.GPSLatitude, out double[] gpslat))
                                    { }
                                    double reslat = Math.Round(gpslat[0] + gpslat[1] / 60 + gpslat[2] / 3600, 8);
                                    if (reader.GetTagValue(ExifTags.GPSLongitude, out double[] gpslong))
                                    { }

                                    double reslong = Math.Round(gpslong[0] + gpslong[1] / 60 + gpslong[2] / 3600, 8);

                                    latit.Insert(numLat, reslat);
                                    numLat++;
                                    longit.Insert(numLong, reslong);
                                    numLong++;
                                }

                            }
                            //завершаем запись трека
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("ошибка \r\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                //начинаем рисовать, объявляем переменные максимумов и минимумов
                double xMin, xMax, yMin, yMax;
                xMin = yMin = double.PositiveInfinity;
                xMax = yMax = double.NegativeInfinity;

                //ищем максимумы и минимумы для определения масштаба
                foreach (double x in longit)
                {
                    if (xMin > x) xMin = x;
                    if (xMax < x) xMax = x;
                }
                foreach (double y in latit)
                {
                    if (yMin > y) yMin = y;
                    if (yMax < y) yMax = y;
                }
                //берем модель от максимумов и минимумов
                xMin = xMin < 0 ? -xMin : xMin;
                xMax = xMax < 0 ? -xMax : xMax;
                yMin = yMin < 0 ? -yMin : yMin;
                yMax = yMax < 0 ? -yMax : yMax;
                //обнуляем оси
                chart1.ChartAreas[0].AxisX.Minimum = chart1.ChartAreas[0].AxisX.Maximum = chart1.ChartAreas[0].AxisY.Minimum = chart1.ChartAreas[0].AxisY.Maximum = 0;
                //считаем размер графика при развернутом на весь экран приложении
                int fwBefore = this.Width;
                if (this.WindowState != FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Maximized;
                    int fhAfter = this.Height;
                    int fwAfter = this.Width;
                    chart1.Height = fhAfter;
                    chart1.Width = fwAfter - fwBefore;
                }
                else
                {
                    chart1.Height = this.Height;
                    chart1.Width = this.Width - 560;
                }
                //добавляем зазор краям графика
                double xmid, ymid, xMidAdd, yMidAdd;
                xmid = xMax - xMin;
                ymid = yMax - yMin;
                xMidAdd = xmid / 20;
                yMidAdd = ymid / 20;
                chart1.ChartAreas[0].AxisX.Minimum = (xMin - xMidAdd);
                chart1.ChartAreas[0].AxisX.Maximum = (xMax + xMidAdd);
                chart1.ChartAreas[0].AxisY.Minimum = (yMin - yMidAdd);
                chart1.ChartAreas[0].AxisY.Maximum = (yMax + xMidAdd);

                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                chart1.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;

                for (int i = 0; i < numLat; i++)
                {
                    chart1.Series[0].Points.AddXY(longit[i], latit[i]);

                    chart1.Series[1].Points.AddXY(longit[i], latit[i]);
                    //MessageBox.Show("latit " + latit[i] + " longit " + longit[i], "info", MessageBoxButtons.OK);
                }
                chart1.Visible = true;
                //MessageBox.Show("form height = " + fhAfter + " form width = " + fwAfter);

            }
            catch (Exception e)
            {
                MessageBox.Show("ошибка \r\n" + e.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked && checkBox3.Checked)
            {
                checkBox1.Checked = false;
            }
            if (checkBox1.Checked && !checkBox3.Checked)
            {
                checkBox1.Checked = false;
            }
            change_label();
        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            change_label();
        }
        private void change_label()
        {
            if (checkBox1.Checked)
            {
                label4.Text = "Отобразить файл трека";
                //return;
            }
            else if (checkBox3.Checked && checkBox4.Checked)
            {
                label4.Text = "Создать файл трека и отобразить его";
            }
            else if (checkBox3.Checked && !checkBox4.Checked)
            {
                label4.Text = "Создать файл трека";
            }
            else if (!checkBox3.Checked && checkBox4.Checked)
            {
                label4.Text = "Переместить файлы, создать файл трека и отобразить его";
            }
            else if (!checkBox3.Checked && !checkBox4.Checked)
            {
                label4.Text = "Переместить файлы, создать файл трека";
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked && !checkBox3.Checked)
            {
                checkBox3.Checked = true;
                checkBox1.Checked = true;
            }
            change_label();
        }

    }
}
