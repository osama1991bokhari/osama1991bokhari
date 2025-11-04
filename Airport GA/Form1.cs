using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Airport_GA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Cursor.Position = new Point((int)resolution.Width, (int)resolution.Height / 2);
        }

        void PictureBox_Paint_alarm(object sender, PaintEventArgs e)
        {
            if (!blink1)
            {

                using (Pen p = new Pen(Color.Red, borderSize))
                {
                    for (int i = 0; i < nodeCount; i++)
                    {
                        foreach (List<RectangleF> Led in alarmLEDs[i].ToList())
                        {
                            foreach (RectangleF r in Led.ToList())
                            {
                                e.Graphics.FillRectangle(Brushes.Red, r);
                            }
                        }
                        foreach (List<RectangleF> uniform in alarmSquare[i].ToList())
                        {
                            foreach (RectangleF r in uniform.ToList())
                            {
                                e.Graphics.DrawRectangle(p, Rectangle.Round(r));
                            }
                        }
                        foreach (List<PointF[]> unUniform in alarmMultiPoint[i].ToList())
                        {
                            foreach (PointF[] r in unUniform.ToList())
                            {
                                e.Graphics.DrawLines(p, r);
                            }
                        }
                    }

                }

            }
            if (!blinkYouAreHere & rotationCounter == ga_idx)
            {
                float x = resolution.Width / 1920;
                float y = resolution.Height / 1080;
                e.Graphics.DrawImage(youAreHere, new RectangleF(youAreHereInt[0] * x, youAreHereInt[1] * y, youAreHereInt[2] * x, youAreHereInt[3] * y));
            }
            //if (rotationCounter > images.Count-1)
            //    rotationCounter = 0;
            //if (rotationCounter != 1)
            //    e.Graphics.DrawImage(pngImg[rotationCounter], new RectangleF(0, 0, drawing.Size.Width, drawing.Size.Height));
        }
        void PictureBox_Paint_super(object sender, PaintEventArgs e)
        {
            if (!blink1)
            {
                using (Pen p = new Pen(Color.IndianRed, borderSize))
                {
                    for (int i = 0; i < nodeCount; i++)
                    {
                        foreach (List<RectangleF> Led in superLEDs[i].ToList())
                        {
                            foreach (RectangleF r in Led.ToList())
                            {
                                e.Graphics.FillRectangle(Brushes.IndianRed, r);
                            }
                        }
                        foreach (List<RectangleF> uniform in superSquare[i].ToList())
                        {
                            foreach (RectangleF r in uniform.ToList())
                            {
                                e.Graphics.DrawRectangle(p, Rectangle.Round(r));
                            }
                        }
                        foreach (List<PointF[]> unUniform in superMultiPoint[i].ToList())
                        {
                            foreach (PointF[] r in unUniform.ToList())
                            {
                                e.Graphics.DrawLines(p, r);
                            }
                        }
                    }
                }
            }
        }
        void PictureBox_Paint_active(object sender, PaintEventArgs e)
        {
            if (!blink1)
            {
                using (Pen p = new Pen(Color.IndianRed, borderSize))
                {
                    for (int i = 0; i < nodeCount; i++)
                    {
                        foreach (List<RectangleF> Led in activeLEDs[i].ToList())
                        {
                            foreach (RectangleF r in Led.ToList())
                            {
                                e.Graphics.FillRectangle(Brushes.IndianRed, r);
                            }
                        }
                        foreach (List<RectangleF> uniform in activeSquare[i].ToList())
                        {
                            foreach (RectangleF r in uniform.ToList())
                            {
                                e.Graphics.DrawRectangle(p, Rectangle.Round(r));
                            }
                        }
                        foreach (List<PointF[]> unUniform in activeMultiPoint[i].ToList())
                        {
                            foreach (PointF[] r in unUniform.ToList())
                            {
                                e.Graphics.DrawLines(p, r);
                            }
                        }
                    }
                }
            }
        }
        void PictureBox_Paint_troubl(object sender, PaintEventArgs e)
        {
            if (!blink1)
            {
                using (Pen p = new Pen(Color.Yellow, borderSize))
                {
                    for (int i = 0; i < nodeCount; i++)
                    {
                        foreach (List<RectangleF> Led in troubleLEDs[i].ToList())
                        {
                            foreach (RectangleF r in Led.ToList())
                            {
                                e.Graphics.FillRectangle(Brushes.Yellow, r);
                            }
                        }
                        foreach (List<RectangleF> uniform in troubleSquare[i].ToList())
                        {
                            foreach (RectangleF r in uniform.ToList())
                            {
                                e.Graphics.DrawRectangle(p, Rectangle.Round(r));
                            }
                        }
                        foreach (List<PointF[]> unUniform in troubleMultiPoint[i].ToList())
                        {
                            foreach (PointF[] r in unUniform.ToList())
                            {
                                e.Graphics.DrawLines(p, r);
                            }
                        }
                    }


                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InitTimer();
        }

        private Timer timer100;//for boarder blinking
        private Timer timer700;//for youAreHere blinking
        private Timer timer3000;//for floor rotation

        static int currNodeIndx;
        static string currNode;
        static string currFACP = "";
        static int panelType = 0;
        static int restartTimer = 0;
        static string defaults = "";
        static int nodeCount = 1;
        static bool blink1 = true;
        static bool blinkYouAreHere = true;
        static bool pauseRotat = false;
        static bool busy = false;
        static int ga_idx = 0;
        static int rotationCounter = 0;
        static int signalIdelCounter = 0;
        LinkedList<int> priorityFloor = new LinkedList<int>();

        static int borderSize = 10;
        static int led_size = 9;
        static int drawing_count;
        static int lineCount = 0;

        Regex modNum3030 = new Regex(@"L\d{2}([D,M])\d{3}");
        Regex modNum640 = new Regex(@"\d{1}([D,M])\d{3}");

        Regex modCode = new Regex(@"GEN ALARM|GEN TROUBL");
        Regex powerModule = new Regex(@"L01M15[5-9]");

        Regex sep3030 = new Regex(@"\d{2},\s\d{4}\s{2,}L\d{2}[M,D]\d{3}|\d{2},\s\d{4}\s{2,}");
        Regex sep640 = new Regex(@"\d{6}\s{1,}[A-Z][a-z][a-z]|\d{6}\s{1,}\d[M,D]\d{3}");

        private static string logFile = "LogFile.txt";
        private static string database = "";
        private Image youAreHere = Image.FromFile("pin-icon-460px.png");
        List<string> images;
        List<string> zonesCSV;
        List<string> nodes;

        List<List<List<RectangleF>>> alarmLEDs = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> troubleLEDs = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> superLEDs = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> activeLEDs = new List<List<List<RectangleF>>>();

        List<List<List<RectangleF>>> alarmLEDs_temp = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> troubleLEDs_temp = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> superLEDs_temp = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> activeLEDs_temp = new List<List<List<RectangleF>>>();

        List<List<List<RectangleF>>> alarmSquare = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> troubleSquare = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> superSquare = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> activeSquare = new List<List<List<RectangleF>>>();

        List<List<List<RectangleF>>> alarmSquare_temp = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> troubleSquare_temp = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> superSquare_temp = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> activeSquare_temp = new List<List<List<RectangleF>>>();

        List<List<List<PointF[]>>> alarmMultiPoint = new List<List<List<PointF[]>>>();
        List<List<List<PointF[]>>> troubleMultiPoint = new List<List<List<PointF[]>>>();
        List<List<List<PointF[]>>> superMultiPoint = new List<List<List<PointF[]>>>();
        List<List<List<PointF[]>>> activeMultiPoint = new List<List<List<PointF[]>>>();

        List<List<List<PointF[]>>> alarmMultiPoint_temp = new List<List<List<PointF[]>>>();
        List<List<List<PointF[]>>> troubleMultiPoint_temp = new List<List<List<PointF[]>>>();
        List<List<List<PointF[]>>> superMultiPoint_temp = new List<List<List<PointF[]>>>();
        List<List<List<PointF[]>>> activeMultiPoint_temp = new List<List<List<PointF[]>>>();



        List<Image> pngImg = new List<Image>();
        List<string> pngName = new List<string>();

        // Dictionaries for storing the zone label and coordinates MAKE THIS DYNAMIC

        List<Dictionary<string, RectangleF>> squareZoneCoord = new List<Dictionary<string, RectangleF>>();
        List<Dictionary<string, PointF[]>> multiPointZoneCoord = new List<Dictionary<string, PointF[]>>();
        List<Dictionary<string, RectangleF>> zoneLEDCoord = new List<Dictionary<string, RectangleF>>();

        // Dicitionary for matching floor with drowing number
        Dictionary<string, int> floorInDrawing = new Dictionary<string, int>();


        private SerialPort port1;
        private static bool is_Com1_3030 = false;
        private static string defaultTime = "";
        List<bool> alarm = new List<bool>();
        List<bool> trouble = new List<bool>();
        List<bool> super = new List<bool>();
        List<bool> active = new List<bool>();

        string lineCom1;

        // Lists to hold the zone number, model id and the zone name from the CSVs, NEED TO MAKE THIS DYNAMIC
        List<string> first_column = new List<string>();
        List<string> areaID = new List<string>();
        List<string> address = new List<string>();
        List<string> addressLabel = new List<string>();
        List<string> areaLabel = new List<string>();

        List<List<string>> ledZoneLabel = new List<List<string>>();
        List<List<string>> ledZone = new List<List<string>>();
        List<List<string>> squareZoneLabel = new List<List<string>>();
        List<List<string>> squareZone = new List<List<string>>();
        List<List<string>> multiPointZoneLabel = new List<List<string>>();
        List<List<string>> multiPointZone = new List<List<string>>();

        List<string> youAreHereCoord = new List<string>();
        List<int> youAreHereInt = new List<int>();
        List<string> powerFail = new List<string>();

        List<int> troubleCount = new List<int>();
        List<int> superCount = new List<int>();
        List<int> activeCount = new List<int>();
        RectangleF resolution = Screen.PrimaryScreen.Bounds;



        /// <summary>
        /// 
        /// Functions for GA
        /// 
        /// </summary>


        public void InitTimer()
        {
            Directory.CreateDirectory("logs");
            LoadAllData();
            timer100 = new Timer();   // timer for refreshing time and date every second and blinking
            timer100.Tick += new EventHandler(Timer1_Tick_Alarm);
            timer100.Interval = 100; // in milliseconds 
            timer100.Start();
            timer700 = new Timer();   // timer for you are here logo
            timer700.Tick += new EventHandler(Timer2_Tick_YouAreHere);
            timer700.Interval = 350;
            timer700.Start();
            timer3000 = new Timer();   // timer for Drawing Rotations
            timer3000.Tick += new EventHandler(Timer3_Tick_Rotation);
            timer3000.Interval = 3000;
            timer3000.Start();



            date.Text = DateTime.Now.ToString("dddd,  dd-MMMM-yyyy");
            time.Text = DateTime.Now.ToString("hh:mm:ss tt");

            drawing.Paint += new System.Windows.Forms.PaintEventHandler(PictureBox_Paint_troubl);
            drawing.Paint += new System.Windows.Forms.PaintEventHandler(PictureBox_Paint_super);
            drawing.Paint += new System.Windows.Forms.PaintEventHandler(PictureBox_Paint_active);
            drawing.Paint += new System.Windows.Forms.PaintEventHandler(PictureBox_Paint_alarm);
        }
        private void Timer1_Tick_Alarm(object sender, EventArgs e)
        {
            // TIMER 1 

            if (restartTimer < 10)
                restartTimer++;
            date.Text = DateTime.Now.ToString("dddd, dd-MMMM-yyyy");
            time.Text = DateTime.Now.ToString("hh:mm:ss tt");
            if ((date.Text.Contains(", 10") | date.Text.Contains(", 20") | date.Text.Contains(", 28"))
                & time.Text.Contains("03:03:03 AM") & restartTimer > 9)
            { // play with this to restart the application at 03:03:03 AM (use AND for AM)
                if (alarmLog.Text.Contains("MIMIC"))
                    using (StreamWriter sw = File.AppendText("alarmLog.txt"))
                    {
                        sw.WriteLine(alarmLog.Text.Substring(99));
                        sw.Close();
                    }
                try
                {
                    if (!File.Exists(Path.Combine(@"logs", "LogFile_" + date.Text + " - " + time.Text.Replace(':', '_') + ".txt")))
                    {
                        File.Copy(logFile, Path.Combine(@"logs", "LogFile_" + date.Text + " - " + time.Text.Replace(':', '_') + ".txt"), false);
                    }
                }
                catch (IOException iox)
                {
                    using (StreamWriter sw = File.AppendText("RestartBackupError.txt"))
                    {
                        sw.WriteLine(iox + "\n restartingError: [!727] " + time.Text + ", " + date.Text + "\n\n");
                        sw.Close();
                    }
                }
                System.IO.File.WriteAllText(logFile, string.Empty);
                lineCom1 = "";

                //Application.Restart();
                //Environment.Exit(0);//this is to close current application to avoid having multilpe instances.
            }
            resolution = Screen.PrimaryScreen.Bounds;
            // Making LED at bottom panel blink
            if (blink1 == false)
            {
                drawing.Refresh();
                if (alarm.Contains(true) | (statusPanel.Text.Contains("ALARM,")))
                    FireAlarmLED.Show();
                for (int i = 0; i < nodeCount; i++)
                {
                    if (troubleCount[i] > 0 | statusPanel.Text.Contains("TROUBLE,"))//!!! READING StatusPanel ALL THE TIME CAN BE AN ISSUE
                    {
                        troubleLED.Show();
                        break;
                    }
                    else
                        troubleCount[i] = 0;
                }

                for (int i = 0; i < nodeCount; i++)
                {
                    if (active[i])
                    {
                        supervisoryLED.Show();
                        break;
                    }
                    if (superCount[i] > 0 | statusPanel.Text.Contains("SUPERVISORY,"))
                    {
                        supervisoryLED.Show();
                        break;
                    }
                    else { 
                        superCount[i] = 0; activeCount[i] = 0;
                    }
                }

                blink1 = true;
            }
            else
            {
                drawing.Refresh();
                FireAlarmLED.Hide();
                troubleLED.Hide();
                supervisoryLED.Hide();
                blink1 = false;
            }


            // TIMER 1 END
        }
        private void Timer2_Tick_YouAreHere(object sender, EventArgs e)
        {
            if (blinkYouAreHere == false)
            {
                blinkYouAreHere = true;
            }
            else
            {
                blinkYouAreHere = false;
            }
            if (alarmLog.Lines.Count() > 5000)
                alarmLog.Text = "";
            try
            {
                if (port1 != null)
                    if (port1.IsOpen)
                    {
                        if ((port1.CtsHolding | port1.CDHolding))
                        {
                            comStatusLED.Text = "Connected";
                            comStatusLED.BackColor = Color.Lime;
                        }
                        else if (!(port1.CtsHolding | port1.CDHolding))
                        {
                            comStatusLED.Text = "Lost Connection";
                            comStatusLED.BackColor = Color.Red;
                        }

                    }
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText("COMErrorSignal.txt"))
                {
                    sw.WriteLine(ex + "\n checking CD and CTS Line: [!727] " + time.Text + ", " + date.Text + "\n\n");
                    sw.Close();
                }
            }
        }
        private void Timer3_Tick_Rotation(object sender, EventArgs e)
        {
            if (drawing_count != 1 & !busy)
                drawingRotate();
            signalIdelCounter++;
            if (signalIdelCounter > 15) //refresh the connection every 45 seconds. number is * 3 seconds
            {
                port1.Close();
                lineCom1 = "";
                if (com1.Text.Contains("640"))
                    port1 = new SerialPort(com1.Text.Substring(0, com1.Text.IndexOf('_')), 9600, Parity.Even, 7, StopBits.One);
                if (com1.Text.Contains("3030"))
                {
                    port1 = new SerialPort(com1.Text.Substring(0, com1.Text.IndexOf('_')), 9600, Parity.None, 8, StopBits.One);
                    is_Com1_3030 = true;
                    defaultTime = "11:51:50A WED APR 14, 2021";
                }
                else
                {
                    is_Com1_3030 = false;
                    defaultTime = "07:27A 042821 Wed";
                }
                port1.Handshake = Handshake.RequestToSendXOnXOff;
                // Method to be called when there is data waiting in the port1's buffer 
                port1.DataReceived += new SerialDataReceivedEventHandler(Port1_DataReceived);
                // Begin communications 
                port1.Open();
                signalIdelCounter = 0;
            }

        }

        private void Port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Show all the incoming data in the port1's buffer
            currNode = nodes[0];
            currNodeIndx = 0;
            try
            {
                string signal1 = port1.ReadExisting();
                lineCom1 = lineCom1 + signal1;
                signalIdelCounter = signalIdelCounter / 2;
                lineCom1 = lineCom1.Replace("/n", "\n");// remove this in production version !!!

                using (StreamWriter sw = File.AppendText("signal_parts_port1.txt"))
                {
                    sw.Write("<" + signal1 + ">\n");
                    sw.Close();
                }
                using (StreamWriter sw = File.AppendText("ActualSignalPort1.txt"))
                {
                    sw.Write(signal1);
                    sw.Close();
                }
                bool line_640 = !is_Com1_3030 & sep640.IsMatch(lineCom1);
                bool line_3030 = is_Com1_3030 & sep3030.IsMatch(lineCom1) & lineCom1.Substring(sep3030.Match(lineCom1).Index).Contains("\n");
                // check if we have a complete signal
                if (line_640 | line_3030)
                {
                    if (lineCom1.Contains("PLEASE WAIT"))
                    {
                        Process_signal("SYSTEM NORMAL                             " + defaultTime);
                        if (!is_Com1_3030)
                        {
                            lineCom1 = Process_signal(lineCom1);
                            lineCom1 = "TROUBL IN SYSTEM    Sys Initialization                       03:18P 060321 Thu  ";
                        }
                    }
                    int i = 0;
                    int q = 0;
                    string remainder = "";
                    if (sep3030.Matches(lineCom1).Count > 1)
                    {
                        foreach (Match match in sep3030.Matches(lineCom1))
                        {
                            i = match.Index;
                            if (lineCom1.Substring(i).Contains("\n"))
                            {
                                Process_signal(lineCom1.Substring(q, i + match.Length - q));
                                remainder = lineCom1.Substring(i + match.Length);
                                q = i + match.Length;
                            }
                        }
                    }
                    else if (sep640.Matches(lineCom1).Count > 1)
                    {
                        foreach (Match match in sep640.Matches(lineCom1))
                        {
                            i = match.Index;
                            Process_signal(lineCom1.Substring(q, i + match.Length - q));
                            remainder = lineCom1.Substring(i + match.Length);
                            q = i + match.Length;
                        }
                    }
                    else if (sep640.IsMatch(lineCom1))
                    {
                        Process_signal(lineCom1);
                        remainder = lineCom1.Substring(sep640.Match(lineCom1).Index + sep640.Match(lineCom1).Length);
                    }
                    else if (sep3030.IsMatch(lineCom1))
                    {
                        Process_signal(lineCom1);
                        remainder = lineCom1.Substring(sep3030.Match(lineCom1).Index + sep3030.Match(lineCom1).Length);
                    }
                    lineCom1 = remainder;
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText("port1ErrorSignal.txt"))
                {
                    sw.WriteLine(ex + "\nLine: [!782]" + time.Text + ", " + date.Text + "\n\n");
                    sw.Close();
                    lineCom1 = "";
                }
            }
        }
        private string Process_signal(string completeSignal)
        {

            string signal_zone = "";
            string signal_floor = "";
            string zone_label = "";
            string signal_module = "";
            string module_code = "";

            int drawingNum = 0;
            using (StreamWriter sw = File.AppendText("rawSignal.txt"))
            {
                sw.WriteLine("<" + completeSignal + ">\n");
                sw.Close();
            }

            module_code = modCode.Match(completeSignal).ToString();
            completeSignal.Trim();
            completeSignal = Regex.Replace(completeSignal, @"\d{2}:\d{2}:\d{2}[A-Z]\s[A-Z]{3}\s[A-Z]{3}\s\d{2},\s\d{4}", "");//removes panel time3030
            completeSignal = Regex.Replace(completeSignal, @"\d{2}:\d{2}[A-Z]\s\d{6}\s[A-z]{3}", "");//removes panel time640
            completeSignal = Regex.Replace(completeSignal, @"\s{2,}", "|");//replace 2 or more white space with bars
            completeSignal = Regex.Replace(completeSignal, @"\n|\r|IN SYSTEM|GEN ALARM|GEN TROUBL", "");//remove space and some words
            completeSignal = Regex.Replace(completeSignal, @"TROUBLE REMINDER", "REMINDER");//remove space and some words
            completeSignal = Regex.Replace(completeSignal, @"L\d{2}1M1[4,5]5", "GENERAL MON");//remove space and some words
            completeSignal = Regex.Replace(completeSignal, @"L\d{2}1M1[4,5]6", "AC FAIL");//remove space and some words
            completeSignal = Regex.Replace(completeSignal, @"L\d{2}1M1[4,5]7", "BATTERY");//remove space and some words
            completeSignal = Regex.Replace(completeSignal, @"L\d{2}1M1[4,5]8", "EARTH");//remove space and some words
            completeSignal = Regex.Replace(completeSignal, @"L\d{2}1M1[4,5]9", "CHARGER FAIL");// !!! error with 640
            completeSignal = Regex.Replace(completeSignal, @"Z\d{2,}", ""); // removing Z501 appearing before  clear trouble
            if (!completeSignal.Contains("ACTIVE TRACK SUPERV") & !completeSignal.Contains("CLR ACT"))
                completeSignal = completeSignal.Replace("TRACK SUPERV", "TRACK SUPRVISORY");
            if (completeSignal[0] == '|')
                completeSignal = completeSignal.Substring(1);
            string[] lines = completeSignal.Split('|');
            bool startWithAlarm = completeSignal.StartsWith("ALARM") | completeSignal.StartsWith("FIRE ALARM");
            bool startWithTrouble = completeSignal.StartsWith("TROUBL") & (!completeSignal.StartsWith("TROUBLE_MON") | !completeSignal.StartsWith("TROUBLE MON"));
            bool containSuper = completeSignal.Contains("SUPERV");
            bool containActive = completeSignal.Contains("ACTIVE WATERFLOW_S") | completeSignal.Contains("ACTIVE TAMPER");
            bool containAck = completeSignal.Contains("ACKN") | completeSignal.Contains("ACKED");
            bool containCLRTB = completeSignal.Contains("CLR TB") | completeSignal.Contains("CLEARED TROUBL");
            bool containCLR = completeSignal.Contains("CLEARED") | completeSignal.Contains("CLR");
            //filter signal code type and store it in a variable
            //check content here
            if (is_Com1_3030)
                signal_module = modNum3030.Match(completeSignal).ToString();
            else
                signal_module = modNum640.Match(completeSignal).ToString();

            if (signal_module != "")
                try
                {
                    for (int i = 1; i < areaID.Count(); i++)
                    {
                        if (address[i].Contains(signal_module) & address[i].Contains(currNode))
                        {
                            signal_zone = areaID[i];
                            signal_floor = addressLabel[i].Split('-')[1];
                            drawingNum = floorInDrawing[signal_floor];
                            break;//check modify this if you want to keep searching
                        }
                    }

                }
                catch (Exception ex)
                {
                    using (StreamWriter sw = File.AppendText("moduleFindingError.txt"))
                    {
                        sw.WriteLine(ex + signal_module + "\nLine: [!828]" + time.Text + ", " + date.Text + "\n\n");
                        sw.Close();
                    }
                }

            // Check which mode we are in
            if (startWithAlarm & !containAck & !containCLR)
            {
                alarm[currNodeIndx] = true;
                trouble[currNodeIndx] = false;
                super[currNodeIndx] = false;
                active[currNodeIndx] = false;
                // add one for track
            }
            if (startWithTrouble & !containAck & !containCLR)
            {
                if (((modNum3030.IsMatch(completeSignal) & !powerModule.IsMatch(completeSignal) ) | (modNum640.IsMatch(completeSignal)&!is_Com1_3030 )))
                {
                    alarm[currNodeIndx] = false;
                    trouble[currNodeIndx] = true;
                    super[currNodeIndx] = false;
                    active[currNodeIndx] = false;
                }
                else
                {
                    statusPanel.Invoke(new Action(() =>
                    {
                        Helper.TxtColor2(statusPanel, Color.Orange);
                        if (completeSignal.Contains("BATTERY") | completeSignal.Contains("L01M157"))
                        {
                            powerLED.Text = "BATTERY FAIL";
                            powerFail.Add("BATTERY FAIL");
                            if (is_Com1_3030)
                                statusPanel.AppendText("TROUBLE, BATTERY FAILURE, L01M157\n");
                            else
                                statusPanel.AppendText("TROUBLE, BATTERY FAILURE\n");
                        }
                        else if (completeSignal.Contains("AC FAIL") | completeSignal.Contains("1M156"))
                        {
                            powerLED.Text = "AC FAIL";
                            powerFail.Add("AC FAIL");
                            if (is_Com1_3030)
                                statusPanel.AppendText("TROUBLE, AC FAILURE, L01M156\n");
                            else
                                statusPanel.AppendText("TROUBLE, AC FAILURE\n");
                        }
                        else if (completeSignal.Contains("GENERAL MON") | completeSignal.Contains("1M155"))
                        {
                            powerLED.Text = "GENERAL MON";
                            powerFail.Add("GENERAL MON");
                            if (is_Com1_3030)
                                statusPanel.AppendText("TROUBLE, GENERAL MONITOR, L01M155\n");
                            else
                                statusPanel.AppendText("TROUBLE, GENERAL MONITOR\n");

                        }
                        else if (completeSignal.Contains("EARTH") | completeSignal.Contains("1M158"))
                        {
                            powerLED.Text = "EARTH FAIL";
                            powerFail.Add("EARTH FAIL");
                            if (is_Com1_3030)
                                statusPanel.AppendText("TROUBLE, EARTH FAILURE, L01M158\n");
                            else
                                statusPanel.AppendText("TROUBLE, EARTH FAILURE\n");

                        }
                        else if (completeSignal.Contains("CHARGER FAIL") | completeSignal.Contains("1M159"))
                        {
                            powerLED.Text = "CHARGER FAIL";
                            powerFail.Add("CHARGER FAIL");
                            if (is_Com1_3030)
                                statusPanel.AppendText("TROUBLE, CHARGER FAILURE, L01M159\n");
                            else
                                statusPanel.AppendText("TROUBLE, CHARGER FAILURE\n");

                        }
                        else if (completeSignal.Contains("NCM COMM FAILURE"))
                        {
                            statusPanel.AppendText("TROUBLE, COMMUNICATION FAILURE (NCM)\n");
                        }
                        else if (completeSignal.Contains("NETWORK FAIL PORT A"))
                        {
                            statusPanel.AppendText("TROUBLE, COMMUNICATION FAILURE (PORT A)\n");
                        }
                        else if (completeSignal.Contains("NETWORK FAIL PORT B"))
                        {
                            statusPanel.AppendText("TROUBLE, COMMUNICATION FAILURE (PORT B)\n");
                        }
                        else if (completeSignal.Contains("NETWORK FAIL"))
                        {
                            statusPanel.AppendText("TROUBLE, COMMUNICATION FAILURE\n");
                        }
                        else
                            statusPanel.AppendText("TROUBLE, " + lines[1] + "\n");
                        if( powerFail.Any())
                        {
                            powerLED.BackColor = Color.Red;
                        }
                    }));
                }

                troubleCount[currNodeIndx]++;
            }
            if (containSuper & !containAck & !containCLR)
            {
                if (modNum3030.IsMatch(completeSignal) | modNum640.IsMatch(completeSignal))
                {
                    alarm[currNodeIndx] = false;
                    trouble[currNodeIndx] = false;
                    super[currNodeIndx] = true;
                    active[currNodeIndx] = false;
                    superCount[currNodeIndx]++;
                }
            }
            if (containActive & !containAck & !containCLR)//this should be separated for track superv
            {
                if (modNum3030.IsMatch(completeSignal) | modNum640.IsMatch(completeSignal))
                {
                    alarm[currNodeIndx] = false;
                    trouble[currNodeIndx] = false;
                    super[currNodeIndx] = false;
                    active[currNodeIndx] = true;
                    activeCount[currNodeIndx]++;
                }
            }
            if (containCLRTB)
            {
                busy = true;
                if (signal_zone != "")//if database contains battry, AC, etc.. remove it.
                {
                    if (multiPointZoneLabel[drawingNum].Contains(signal_zone))
                    {// remove from both rect and temp
                        troubleMultiPoint[currNodeIndx][drawingNum].Remove(multiPointZoneCoord[drawingNum][signal_zone]);
                        troubleMultiPoint_temp[currNodeIndx][drawingNum].Remove(multiPointZoneCoord[drawingNum][signal_zone]);
                        troubleLEDs[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                        troubleLEDs_temp[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                        drawing.Invalidate();
                        if (!troubleLEDs[currNodeIndx][drawingNum].Any())
                            trouble[currNodeIndx] = false; // set this to false after all floors have 0 trouble
                    }
                    else// if you want to remove uniform and unUni in one drawing change else to if
                    {

                        {
                            troubleSquare[currNodeIndx][drawingNum].Remove(squareZoneCoord[drawingNum][signal_zone]);
                            troubleSquare_temp[currNodeIndx][drawingNum].Remove(squareZoneCoord[drawingNum][signal_zone]);
                            troubleLEDs[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                            troubleLEDs_temp[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                            drawing.Invalidate();
                            if (!troubleLEDs[currNodeIndx][drawingNum].Any())
                                trouble[currNodeIndx] = false;
                        }
                    }
                    removeLine(signal_module, "TROUBLE");
                }
                else
                {
                    if (completeSignal.Contains("BATT") | completeSignal.Contains("1M157"))
                    {
                        powerFail.Remove("BATTERY FAIL");
                        removeLine("BATT", "TROUBLE");
                    }
                    else if (completeSignal.Contains("AC FAIL") | completeSignal.Contains("1M156"))
                    {
                        powerFail.Remove("AC FAIL");
                        removeLine("AC FAIL", "TROUBLE");
                    }
                    else if (completeSignal.Contains("EARTH") | completeSignal.Contains("1M158"))
                    {
                        powerFail.Remove("EARTH FAIL");
                        removeLine("EARTH", "TROUBLE");
                    }
                    else if (completeSignal.Contains("CHARGER") | completeSignal.Contains("1M159"))
                    {
                        powerFail.Remove("CHARGER FAIL");
                        removeLine("CHARGER", "TROUBLE");
                    }
                    else if (completeSignal.Contains("GENERAL MON") | completeSignal.Contains("1M155"))
                    {
                        powerFail.Remove("GENERAL MON");
                        removeLine("GENERAL MON", "TROUBLE");
                    }
                    else if(completeSignal.Contains("NCM COMM FAILURE"))
                    {
                        removeLine("COMMUNICATION FAILURE (NCM)", "TROUBLE");
                    }
                    else if (completeSignal.Contains("NETWORK FAIL PORT A"))
                    {
                        removeLine("COMMUNICATION FAILURE (PORT A)", "TROUBLE");
                    }
                    else if (completeSignal.Contains("NETWORK FAIL PORT B"))
                    {
                        removeLine("COMMUNICATION FAILURE (PORT B)", "TROUBLE");
                    }
                    else if (completeSignal.Contains("NCM") | completeSignal.Contains("NETWORK"))
                    {
                        removeLine("COMMUNICATION", "TROUBLE");
                    }
                    if (!powerFail.Any())
                    {
                        powerLED.Invoke(new Action(() =>
                        {
                            powerLED.BackColor = Color.Lime;
                            powerLED.Text = "";
                        }));
                    }
                    else
                        powerLED.Invoke(new Action(() =>
                        {
                            powerLED.BackColor = Color.Red;
                            powerLED.Text = powerFail[0];
                        }));
                    removeLine(completeSignal.Split('|')[1], "TROUBLE");

                }
                troubleCount[currNodeIndx]--;
                drawing.Invalidate();

            }
            if (containCLR & containSuper)
            {
                busy = true;
                if (signal_zone != "")
                {
                    if (multiPointZoneLabel[drawingNum].Contains(signal_zone))
                    {// remove from both rect and temp
                        {
                            superMultiPoint[currNodeIndx][drawingNum].Remove(multiPointZoneCoord[drawingNum][signal_zone]);
                            superMultiPoint_temp[currNodeIndx][drawingNum].Remove(multiPointZoneCoord[drawingNum][signal_zone]);
                            superLEDs[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                            superLEDs_temp[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                            drawing.Invalidate();
                            if (!superLEDs[currNodeIndx][drawingNum].Any())
                                super[currNodeIndx] = false; // set this to false after all floors have 0 superv
                        }
                    }
                    else
                    {
                        {
                            superSquare[currNodeIndx][drawingNum].Remove(squareZoneCoord[drawingNum][signal_zone]);
                            superSquare_temp[currNodeIndx][drawingNum].Remove(squareZoneCoord[drawingNum][signal_zone]);
                            superLEDs[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                            superLEDs_temp[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                            drawing.Invalidate();
                            if (!superLEDs[currNodeIndx][drawingNum].Any())
                                super[currNodeIndx] = false;
                        }
                    }
                    removeLine(signal_module, "SUPERVISORY");
                }
                superCount[currNodeIndx]--;
                drawing.Invalidate();
            }
            if (containCLR & containActive)
            {
                try
                {
                    busy = true;
                    if (signal_zone != "")
                    {
                        if (multiPointZoneLabel[drawingNum].Contains(signal_zone))
                        {// remove from both rect and temp
                            {
                                activeMultiPoint[currNodeIndx][drawingNum].Remove(multiPointZoneCoord[drawingNum][signal_zone]);
                                activeMultiPoint_temp[currNodeIndx][drawingNum].Remove(multiPointZoneCoord[drawingNum][signal_zone]);
                                activeLEDs[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                                activeLEDs_temp[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                                drawing.Invalidate();
                                if (!activeLEDs[currNodeIndx][drawingNum].Any())
                                    active[currNodeIndx] = false; // set this to false after all floors have 0 active
                            }
                        }
                        else
                        {
                            {
                                activeSquare[currNodeIndx][drawingNum].Remove(squareZoneCoord[drawingNum][signal_zone]);
                                activeSquare_temp[currNodeIndx][drawingNum].Remove(squareZoneCoord[drawingNum][signal_zone]);
                                activeLEDs[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                                activeLEDs_temp[currNodeIndx][drawingNum].Remove(zoneLEDCoord[drawingNum][signal_zone]);
                                drawing.Invalidate();
                                if (!activeLEDs[currNodeIndx][drawingNum].Any())
                                    active[currNodeIndx] = false;
                            }
                        }
                        removeLine(signal_module, "ACTIVE SUPERVISORY");
                    }
                    activeCount[currNodeIndx]--;
                    drawing.Invalidate();
                }
                catch { }
            }
            if (completeSignal.Contains("RESET"))
            {
                for (int i = 0; i < nodeCount; i++)
                {
                    alarm[i] = false;
                    active[i] = false;
                    //trouble[i] = false;
                    //super[i] = false;
                }

                drawing.Invalidate();

                var rectToReset = new List<List<List<RectangleF>>>();
                var pointToReset = new List<List<List<PointF[]>>>();

                for (int i = 0; i < nodeCount; i++)
                {
                    rectToReset = new List<List<List<RectangleF>>>() {alarmLEDs[i], alarmLEDs_temp[i], alarmSquare[i],
                    alarmSquare_temp[i],activeLEDs[i], activeLEDs_temp[i], activeSquare[i], activeSquare_temp[i]};

                    pointToReset = new List<List<List<PointF[]>>>() {alarmMultiPoint[i], alarmMultiPoint_temp[i],
                activeMultiPoint[i], activeMultiPoint_temp[i]};

                    foreach (List<List<RectangleF>> rectVar in rectToReset)
                    {
                        foreach (List<RectangleF> rectangles in rectVar)
                        {
                            rectangles.Clear();
                        }
                    }
                    foreach (List<List<PointF[]>> pointVar in pointToReset)
                    {
                        foreach (List<PointF[]> point in pointVar)
                        {
                            point.Clear();
                        }
                    }

                }

                priorityFloor.Clear();
                statusPanel.Invoke(new Action(() =>
                {
                    List<string> finalLines = statusPanel.Lines.ToList();
                    finalLines.RemoveAll(x => x.StartsWith("ALARM") || x.StartsWith("ACTIVE SUPERVISORY"));
                    statusPanel.Lines = finalLines.ToArray();
                    Helper.ColorLine(statusPanel);
                }));
                alarmEvnt.Invoke(new Action(() =>
                {
                    alarmEvnt.Text = "";
                }));
                alarmLog.Invoke(new Action(() =>
                {
                    alarmLog.ResetText();
                    alarmLog.ForeColor = Color.Turquoise;
                    alarmLog.Select(alarmLog.TextLength, alarmLog.TextLength);
                    alarmLog.ScrollToCaret();
                }));
            }
            if (completeSignal.Contains("NORMAL"))
            {
                alarm[currNodeIndx] = false;
                trouble[currNodeIndx] = false;
                super[currNodeIndx] = false;
                active[currNodeIndx] = false;

                var rectToReset = new List<List<List<RectangleF>>>();
                var pointToReset = new List<List<List<PointF[]>>>();

                rectToReset = new List<List<List<RectangleF>>>() {alarmLEDs[currNodeIndx], troubleLEDs[currNodeIndx],
                    superLEDs[currNodeIndx], activeLEDs[currNodeIndx], alarmLEDs_temp[currNodeIndx], troubleLEDs_temp[currNodeIndx],
                    superLEDs_temp[currNodeIndx], activeLEDs_temp[currNodeIndx], alarmSquare[currNodeIndx], troubleSquare[currNodeIndx],
                    superSquare[currNodeIndx], activeSquare[currNodeIndx], alarmSquare_temp[currNodeIndx], troubleSquare_temp[currNodeIndx],
                    superSquare_temp[currNodeIndx], activeSquare_temp[currNodeIndx]};

                pointToReset = new List<List<List<PointF[]>>>() {alarmMultiPoint[currNodeIndx], troubleMultiPoint[currNodeIndx],
                    superMultiPoint[currNodeIndx], activeMultiPoint[currNodeIndx], alarmMultiPoint_temp[currNodeIndx],
                    troubleMultiPoint_temp[currNodeIndx], superMultiPoint_temp[currNodeIndx], activeMultiPoint_temp[currNodeIndx]};

                foreach (List<List<RectangleF>> rectVar in rectToReset)
                {
                    foreach (List<RectangleF> rectangles in rectVar)
                    {
                        rectangles.Clear();
                    }
                }
                foreach (List<List<PointF[]>> pointVar in pointToReset)
                {
                    foreach (List<PointF[]> point in pointVar)
                    {
                        point.Clear();
                    }
                }

                drawing.Invalidate();

                troubleCount[currNodeIndx] = 0;
                superCount[currNodeIndx] = 0;
                activeCount[currNodeIndx] = 0;
                if (!alarm.Contains(false))
                    priorityFloor.Clear();
                statusPanel.Invoke(new Action(() =>
                {
                    statusPanel.Text = ""; //!!! this for only single panels
                }));
                powerFail.Clear();//!!! this for only single panels
                powerLED.Invoke(new Action(() =>
                {
                    powerLED.BackColor = Color.Lime;
                    powerLED.Text = "";
                }));


            }
            busy = true;
            if (alarm[currNodeIndx] | trouble[currNodeIndx] | super[currNodeIndx] | active[currNodeIndx])
            {
                string fm200 = "";
                if (completeSignal.Contains("GS-Z1"))
                    fm200 = " Alarm1";
                else if (completeSignal.Contains("GS-Z2"))
                    fm200 = " Alarm2";
                else if (completeSignal.Contains("GS-T"))
                    fm200 = " Trouble";
                else if (completeSignal.Contains("GS-GD"))
                    fm200 = " GAS DISCHARGE";
                if (!containAck & !containCLR)
                    if (modNum640.IsMatch(completeSignal))
                    {
                        try
                        {
                            if (alarm[currNodeIndx] & !trouble[currNodeIndx] & !super[currNodeIndx] & !active[currNodeIndx])
                                for (int i = 1; i < areaID.Count(); i++)
                                {//extracting module zone and room lable and writing to alarmEvent if alarm.
                                    if (address[i].Contains(signal_module) & address[i].Contains(currNode))
                                    {
                                        signal_zone = areaID[i];
                                        zone_label = areaLabel[i];
                                        alarmEvnt.Invoke(new Action(() =>
                                        {
                                            alarmEvnt.AppendText(signal_zone.Replace("ZONE ", "Z") + ", " + zone_label + ", " + signal_floor + "\n");
                                        }));

                                        break;
                                    }
                                }
                            if (signal_zone != "") // check if device is in database to make its zone blink
                            {
                                int corresponNum = (drawingNum) % drawing_count;
                                if (multiPointZoneLabel[drawingNum].Contains(signal_zone))
                                {
                                    if (alarm[currNodeIndx])
                                    {
                                        if (!priorityFloor.Contains(corresponNum))
                                            priorityFloor.AddFirst(corresponNum);
                                        drawingRotate();
                                        alarmMultiPoint[currNodeIndx][drawingNum].Add(multiPointZoneCoord[corresponNum][signal_zone]);
                                        alarmLEDs[currNodeIndx][drawingNum].Add(zoneLEDCoord[corresponNum][signal_zone]);
                                    }
                                    if (trouble[currNodeIndx])
                                    {
                                        troubleMultiPoint[currNodeIndx][drawingNum].Add(multiPointZoneCoord[corresponNum][signal_zone]);
                                        troubleLEDs[currNodeIndx][drawingNum].Add(zoneLEDCoord[corresponNum][signal_zone]);
                                    }
                                    if (super[currNodeIndx])
                                    {
                                        superMultiPoint[currNodeIndx][drawingNum].Add(multiPointZoneCoord[corresponNum][signal_zone]);
                                        superLEDs[currNodeIndx][drawingNum].Add(zoneLEDCoord[corresponNum][signal_zone]);
                                    }
                                    if (active[currNodeIndx])
                                    {
                                        activeMultiPoint[currNodeIndx][drawingNum].Add(multiPointZoneCoord[corresponNum][signal_zone]);
                                        activeLEDs[currNodeIndx][drawingNum].Add(zoneLEDCoord[corresponNum][signal_zone]);
                                    }

                                }
                                if (squareZoneLabel[drawingNum].Contains(signal_zone))
                                {
                                    if (alarm[currNodeIndx])
                                    {
                                        if (!priorityFloor.Contains(corresponNum))
                                            priorityFloor.AddFirst(corresponNum);
                                        drawingRotate();
                                        alarmSquare[currNodeIndx][drawingNum].Add(squareZoneCoord[corresponNum][signal_zone]);
                                        alarmLEDs[currNodeIndx][drawingNum].Add(zoneLEDCoord[corresponNum][signal_zone]);
                                    }
                                    if (trouble[currNodeIndx])
                                    {
                                        troubleSquare[currNodeIndx][drawingNum].Add(squareZoneCoord[corresponNum][signal_zone]);
                                        troubleLEDs[currNodeIndx][drawingNum].Add(zoneLEDCoord[corresponNum][signal_zone]);
                                    }
                                    if (super[currNodeIndx])
                                    {
                                        superSquare[currNodeIndx][drawingNum].Add(squareZoneCoord[corresponNum][signal_zone]);
                                        superLEDs[currNodeIndx][drawingNum].Add(zoneLEDCoord[corresponNum][signal_zone]);
                                    }
                                    if (active[currNodeIndx])
                                    {
                                        activeSquare[currNodeIndx][drawingNum].Add(squareZoneCoord[corresponNum][signal_zone]);
                                        activeLEDs[currNodeIndx][drawingNum].Add(zoneLEDCoord[corresponNum][signal_zone]);
                                    }
                                }
                            }
                            else
                            {
                                statusPanel.Invoke(new Action(() =>
                                {
                                    Helper.TxtColor2(statusPanel, Color.Orange);
                                    statusPanel.AppendText("Device: " + signal_module + ", is not found in database.\n");
                                    statusPanel.ScrollToCaret();
                                }));
                                using (StreamWriter sw = File.AppendText("Missing Devices.txt"))
                                {
                                    sw.WriteLine(signal_module);
                                    sw.Close();
                                }
                            }

                            if (startWithAlarm & !containCLR & !containAck)
                            {   //Writing to StatusPanel
                                if (signal_module != "")
                                    for (int i = 1; i < areaID.Count(); i++)
                                    {
                                        if (address[i].Contains(signal_module) & address[i].Contains(currNode))
                                        {
                                            statusPanel.Invoke(new Action(() =>
                                            {
                                                Helper.TxtColor2(statusPanel, Color.Red);
                                                statusPanel.AppendText("ALARM,  " + address[i] + ",  " +
                                                areaID[i].Replace("ZONE ", "Z") + ", " + areaLabel[i] + ", " + addressLabel[i] + fm200 + "\n");
                                                statusPanel.ScrollToCaret();
                                            }));
                                            break;
                                        }
                                    }
                            }
                            string signal_module_sp = "";
                            signal_module_sp = modNum640.Match(completeSignal).ToString();
                            if (startWithTrouble & !containCLR & !containAck)
                            {

                                for (int i = 1; i < areaID.Count(); i++)
                                {
                                    if (signal_module_sp == "")
                                    {
                                        statusPanel.Invoke(new Action(() =>
                                        {
                                            string panelNum = address[4].Substring(0, 4);
                                            Helper.TxtColor2(statusPanel, Color.Orange);
                                            statusPanel.AppendText("TROUBLE,  " + panelNum + ",  " + lines[1 + panelType] + fm200 + "\n");
                                            statusPanel.ScrollToCaret();
                                        }));
                                        break;
                                    }
                                    if (address[i].Contains(signal_module_sp) & address[i].Contains(currNode))
                                    {
                                        statusPanel.Invoke(new Action(() =>
                                        {
                                            Helper.TxtColor2(statusPanel, Color.Orange);
                                            statusPanel.AppendText("TROUBLE,  " + address[i] + ",  " +
                                                areaID[i].Replace("ZONE ", "Z") + ", " + areaLabel[i] + ", " + addressLabel[i] + fm200 + "\n");
                                            statusPanel.ScrollToCaret();
                                        }));
                                        break;
                                    }

                                }
                            }
                            if (containSuper & !containCLR & !containAck)
                            {

                                for (int i = 1; i < areaID.Count(); i++)
                                {
                                    if (signal_module_sp == "")
                                    {
                                        statusPanel.Invoke(new Action(() =>
                                        {
                                            string panelNum = address[4].Substring(0, 4);
                                            Helper.TxtColor2(statusPanel, Color.IndianRed);
                                            statusPanel.AppendText("SUPERVISORY,  " + panelNum + ",  " + lines[1 + panelType] + fm200 + "\n");
                                            statusPanel.ScrollToCaret();
                                        }));
                                        break;
                                    }
                                    if (address[i].Contains(signal_module_sp) & address[i].Contains(currNode))
                                    {
                                        statusPanel.Invoke(new Action(() =>
                                        {
                                            Helper.TxtColor2(statusPanel, Color.IndianRed);
                                            statusPanel.AppendText("SUPERVISORY,  "  + address[i] + ",  " +
                                            areaID[i].Replace("ZONE ", "Z") + ", " + areaLabel[i] + ", " + addressLabel[i] + fm200 + "\n");
                                            statusPanel.ScrollToCaret();
                                        }));
                                        break;
                                    }
                                }
                            }
                            if (containActive & !containCLR & !containAck)
                            {

                                for (int i = 1; i < areaID.Count(); i++)
                                {
                                    if (signal_module_sp == "")
                                    {
                                        statusPanel.Invoke(new Action(() =>
                                        {
                                            string panelNum = address[4].Substring(0, 4);
                                            Helper.TxtColor2(statusPanel, Color.IndianRed);
                                            statusPanel.AppendText("ACTIVE SUPERVISORY,  " + panelNum + ",  " + lines[1 + panelType] + fm200 + "\n");
                                            statusPanel.ScrollToCaret();
                                        }));
                                        break;
                                    }
                                    if (address[i].Contains(signal_module_sp) & address[i].Contains(currNode))
                                    {
                                        statusPanel.Invoke(new Action(() =>
                                        {
                                            Helper.TxtColor2(statusPanel, Color.IndianRed);
                                            statusPanel.AppendText("ACTIVE SUPERVISORY,  " + ",  " + address[i] + ",  " +
                                            areaID[i].Replace("ZONE ", "Z") + ", " + areaLabel[i] + ", " + addressLabel[i] + fm200 + "\n");
                                            statusPanel.ScrollToCaret();
                                        }));
                                        break;
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            using (StreamWriter sw = File.AppendText("signalProcessErrorSignal.txt"))
                            {
                                sw.WriteLine(ex + "\nHandling Signals\nLine: 1621" + time.Text + ", " + date.Text + "\n\n");
                                sw.Close();
                            }
                        }
                    }
            }
            busy = false;
            // Writning to log file
            try
            {
                using (StreamWriter sw = File.AppendText(logFile))
                {
                    string[] line_list = completeSignal.Split('|');
                    line_list[line_list.Count() - 1] = "  " + time.Text + " " + date.Text;
                    if (Regex.IsMatch(line_list[0], @"^(L\d{2}([D,M])\d{3}|\d{2}([D,M])\d{3}|\dM\d{3}|([D,M])\d{3}|\d{1,3})"))
                    {
                        line_list[0] = Regex.Replace(line_list[0], @"^(L\d{2}([D,M])\d{3}|\d{2}([D,M])\d{3}|\d([D,M])\d{3}|([D,M])\d{3}|\d{1,3})", "");
                    }
                    if (Regex.IsMatch(line_list[1], @"^(L\d{2}([D,M])\d{3}|\d{2}([D,M])\d{3}|\dM\d{3}|([D,M])\d{3}|\d{1,3})"))
                    {
                        line_list[1] = Regex.Replace(line_list[1], @"^(L\d{2}([D,M])\d{3}|\d{2}([D,M])\d{3}|\d([D,M])\d{3}|([D,M])\d{3}|\d{1,3})", "");
                    }
                    completeSignal = string.Join("|", line_list);
                    if (completeSignal[0] == '|')
                        completeSignal = completeSignal.Substring(1);
                    sw.WriteLine(lineCount + "|" + completeSignal + " " + signal_module);
                    lineCount++;
                    sw.Close();
                    try
                    {
                        alarmLog.Invoke(new Action(() =>
                        {
                            if (alarmLog.Lines.Count() > 6)
                            {
                                if (alarmLog.Lines[alarmLog.Lines.Count() - 2].Contains("WAITING"))
                                {
                                    int firstCharIndx = alarmLog.GetFirstCharIndexOfCurrentLine();
                                    int length = alarmLog.Lines[alarmLog.Lines.Count() - 2].Length;
                                    alarmLog.SelectionStart = firstCharIndx;
                                    alarmLog.SelectionLength = length;
                                    alarmLog.SelectionColor = Color.Black;
                                }
                            }
                            string line = lineCount + "|" + completeSignal + " " + signal_module;

                            line_list = line.Split('|');
                            string line_sig_type = "";
                            foreach (string s in line_list)
                                if (int.TryParse(s, out _) | s.Contains("day"))
                                    continue;
                                else
                                    line_sig_type += s.Trim() + " ";
                            string loop = "1";
                            string module = modNum640.Match(line).ToString();
                            if (module.Contains("L"))
                                loop = module[2] + "";
                            /*if (currNode == "221")
                                currFACP = "2";
                            else if (currNode == "222")
                                currFACP = "3";
                            else
                                currFACP = "1";*/

                            alarmLog.WordWrap = false;
                            Helper.TxtColor(alarmLog, Color.Turquoise);
                            alarmLog.AppendText("Date: " + date.Text + "\nTime: " + time.Text + "\nData Signal #: " + (lineCount - 1) + "\n");
                            Helper.TxtColor(alarmLog, Color.Orange);
                            if (line.Contains("NORMAL"))
                                alarmLog.AppendText("Event Name: " + line_sig_type + " SYSTEM NORMAL\nNode Address: " + currNode +
                                    " FACP-0" + currFACP + "\nLoop Number: " + loop + "\n\n");
                            else
                                alarmLog.AppendText("Event Name: " + line_sig_type + " " + module_code + " " + signal_module + "\nNode Address: " + currNode + " FACP-0" + currFACP +
                                    "\nLoop Number: " + loop + "\n\n");
                            Helper.TxtColor(alarmLog, Color.White);
                            alarmLog.AppendText("*** WAITING FOR A NEW EVENT ***\n");
                            alarmLog.WordWrap = true; ;
                            alarmLog.Select(alarmLog.Text.Length - 1, 0);
                            alarmLog.ScrollToCaret();
                        }));



                    }
                    catch (Exception ex)
                    {
                        using (StreamWriter sw2 = File.AppendText("alarmLogFillingError.txt"))
                        {
                            sw2.WriteLine(ex + "\nLine: [!685]" + time.Text + ", " + date.Text + "\n\n");
                            sw2.Close();
                        }
                    }
                }
            }
            catch { }
            alarm[currNodeIndx] = false;
            trouble[currNodeIndx] = false;
            super[currNodeIndx] = false;
            active[currNodeIndx] = false;
            return "";//return ramaining string instead

        }
        /**/
        private void drawingRotate()
        {

            if (!pauseRotat)
                if (priorityFloor.Count == 0 | priorityFloor.Count == images.Count)
                    rotationCounter++;
                else
                {
                    int first = priorityFloor.First.Value;
                    priorityFloor.RemoveFirst();
                    priorityFloor.AddLast(first);
                    rotationCounter = first;
                }

            for (int i = 0; i < nodeCount; i++)
            {
                int realRotationCounter = rotationCounter - 1;
                Helper.BackupRestoreZones(alarmLEDs[i], alarmLEDs_temp[i], alarmSquare[i], alarmSquare_temp[i], alarmMultiPoint[i], alarmMultiPoint_temp[i], realRotationCounter, drawing_count);
                Helper.BackupRestoreZones(troubleLEDs[i], troubleLEDs_temp[i], troubleSquare[i], troubleSquare_temp[i], troubleMultiPoint[i], troubleMultiPoint_temp[i], realRotationCounter, drawing_count);
                Helper.BackupRestoreZones(superLEDs[i], superLEDs_temp[i], superSquare[i], superSquare_temp[i], superMultiPoint[i], superMultiPoint_temp[i], realRotationCounter, drawing_count);
                Helper.BackupRestoreZones(activeLEDs[i], activeLEDs_temp[i], activeSquare[i], activeSquare_temp[i], activeMultiPoint[i], activeMultiPoint_temp[i], realRotationCounter, drawing_count);

            }

            if (rotationCounter >= images.Count)
                rotationCounter = 0;
            drawing.Invoke(new Action(() => { drawing.Image = pngImg[rotationCounter]; }));
            floor.Invoke(new Action(() => { floor.Text = pngName[rotationCounter]; }));
        }
        private void removeLine(string word, string sigType)
        {
            statusPanel.Invoke(new Action(() =>
            {
                List<string> finalLines = statusPanel.Lines.ToList();
                finalLines.RemoveAll(x => x.Contains(word) & x.Contains(sigType));
                statusPanel.Lines = finalLines.ToArray();
                if (statusPanel.Lines.Count() < 5000)
                    Helper.ColorLine(statusPanel);
                statusPanel.Select(statusPanel.Text.Length, 0);
                statusPanel.ScrollToCaret();
            }));

        }

        private void gaca_logo_Click(object sender, EventArgs e)
        {
            if (!pauseRotat)
                pauseRotat = true;
            else
                pauseRotat = false;
        }
        private void Exit_Click(object sender, EventArgs e)
        {
            if (clrHist.Checked)
                System.IO.File.WriteAllText(logFile, string.Empty);
            Application.Exit();
        }
        private void Minimize(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void TestLds_CheckedChanged(object sender, EventArgs e)
        {
            if (testLds.Checked)
            {
                for (int i = 0; i < nodeCount; i++)
                {
                    for (int j = 0; j < drawing_count; j++)
                    {
                        foreach (string z in zoneLEDCoord[(j) % drawing_count].Keys)
                            alarmLEDs[i][j].Add(zoneLEDCoord[(j) % drawing_count][z]);
                        foreach (string z in squareZoneCoord[(j) % drawing_count].Keys)
                            alarmSquare[i][j].Add(squareZoneCoord[(j) % drawing_count][z]);
                        foreach (string uz in multiPointZoneCoord[(j) % drawing_count].Keys)
                            alarmMultiPoint[i][j].Add(multiPointZoneCoord[(j) % drawing_count][uz]);
                    }
                }
            }

            else
            {
                for (int i = 0; i < nodeCount; i++)
                {
                    foreach (List<RectangleF> alarmleds in alarmLEDs[i])
                        alarmleds.Clear();
                    foreach (List<RectangleF> alarmleds in alarmLEDs_temp[i])
                        alarmleds.Clear();
                    foreach (List<RectangleF> alarmuniform in alarmSquare[i])
                        alarmuniform.Clear();
                    foreach (List<RectangleF> alarmuniform in alarmSquare_temp[i])
                        alarmuniform.Clear();
                    foreach (List<PointF[]> alarmununiform in alarmMultiPoint[i])
                        alarmununiform.Clear();
                    foreach (List<PointF[]> alarmununiform in alarmMultiPoint_temp[i])
                        alarmununiform.Clear();
                }

            }
        }
        private void Label16_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void ClosePort_Click(object sender, EventArgs e)
        {
            if (!testLds.Checked)
            {
                if (closePort.Text.Contains("Open"))
                    try
                    {
                        DialogResult dr = MessageBox.Show("Do you want to reload everythig?", "      Reloading data", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if (dr == DialogResult.Yes)
                        {
                            LoadAllData();
                        }
                        else if (dr == DialogResult.No)
                        {
                            if (com1.Text.Contains("640"))
                                port1 = new SerialPort(com1.Text.Substring(0, com1.Text.IndexOf('_')), 9600, Parity.Even, 7, StopBits.One);
                            if (com1.Text.Contains("3030"))
                            {
                                port1 = new SerialPort(com1.Text.Substring(0, com1.Text.IndexOf('_')), 9600, Parity.None, 8, StopBits.One);
                                is_Com1_3030 = true;
                                defaultTime = "11:51:50A WED APR 14, 2021";
                            }
                            else
                            {
                                is_Com1_3030 = false;
                                defaultTime = "07:27A 042821 Wed";
                            }

                            port1.Handshake = Handshake.RequestToSendXOnXOff;
                            // Method to be called when there is data waiting in the port1's buffer 
                            port1.DataReceived += new SerialDataReceivedEventHandler(Port1_DataReceived);
                            // Begin communications 
                            port1.Open();
                        }
                        else
                            return;
                            // Enter an application loop to keep this thread alive 
                        Console.ReadLine();
                        closePort.Text = "Close connection\n (" + com1.Text.Substring(0, 4) + ")";
                        com1.Enabled = false;
                        comStatusLED.BackColor = System.Drawing.Color.Lime;
                        comStatusLED.Text = "Connected";
                        testLds.Enabled = false;
                        alarmLog.WordWrap = false;
                        Helper.TxtColor(alarmLog, Color.LimeGreen);
                        alarmLog.AppendText("\n\n*** MIMIC PANEL Monitoring Openned by Supervisor ***\n\n");
                        alarmLog.ScrollToCaret();
                        alarmLog.WordWrap = true;
                        alarmLog.ScrollToCaret();
                    }
                    catch
                    {
                        MessageBox.Show("Unable to connect to " + com1.Text + ", check cable", "Error connecting to " + com1.Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                else if (closePort.Text.Contains("Close"))
                {
                    port1.Close();
                    closePort.Text = "Open connection\n (" + com1.Text.Substring(0, 4) + ")";
                    com1.Enabled = true;
                    comStatusLED.BackColor = System.Drawing.Color.IndianRed;
                    comStatusLED.Text = "Disconnected";
                    testLds.Enabled = true;
                    alarmLog.WordWrap = false;
                    Helper.TxtColor(alarmLog, Color.Red);
                    alarmLog.AppendText("\n\n*** MIMIC PANEL Monitoring Closed by Supervisor ***\n\n");
                    alarmLog.WordWrap = true;
                    alarmLog.ScrollToCaret();
                }
            }
            else
            {
                MessageBox.Show("Please stop the test LED first", "Error connecting to " + com1.Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Com1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com1.Text.Length > 3)
                closePort.Text = "Open connection\n (" + com1.Text.Substring(0, 4) + ")";
        }
        private void LoadAllData()
        {
            images = Directory.GetFiles("img", "*.png*", SearchOption.AllDirectories).OrderBy(f => f).ToList();
            zonesCSV = Directory.GetFiles("zones", "*.csv*", SearchOption.AllDirectories).OrderBy(f => f).ToList();
            nodes = new List<string>();
            floorInDrawing.Clear();
            using (var reader = new StreamReader("drawingMapping.txt"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split('|');
                    floorInDrawing.Add(values[0], Int32.Parse(values[1]));
                }
            }

            using (StreamReader sr = File.OpenText("Default.txt"))
            {
                defaults = sr.ReadLine();
                sr.Close();
            }
            string[] defaultSetting = defaults.Split('|');
            switch (defaultSetting[1])
            {
                case "3030":
                    panelType = 0;
                    break;
                case "640":
                    panelType = 1;
                    break;
            }

            //log_hash = Helper.GetMD5HashFromFile(logFile);
            try // Loading Database and Drawings
            {
                drawing.Image = Image.FromFile(images[0]);
                switch (defaultSetting[0])
                {
                    case "s":
                        drawing.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
                        break;
                    case "z":
                        drawing.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
                        break;
                    case "c":
                        var imageSize = drawing.Image.Size;
                        var fitSize = drawing.ClientSize;
                        drawing.SizeMode = imageSize.Width > fitSize.Width || imageSize.Height > fitSize.Height ?
                            PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage;
                        break;
                }
                ga_idx = floorInDrawing[defaultSetting[3]];
                pngImg.Clear(); pngName.Clear();
                foreach (string imgDir in images)
                {
                    if (imgDir.Contains("png"))
                    {
                        pngImg.Add(Image.FromFile(imgDir));
                        pngName.Add(imgDir.Split('_')[1].Split('.')[0].Replace('^', '.'));
                    }
                }
                
                database = Directory.GetFiles("database", "*.csv")[0];
                using (var reader = new StreamReader(database))
                {
                    //Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))"); this is a general split
                    //String[] Fields = CSVParser.Split(Test);
                    first_column.Clear(); areaID.Clear(); address.Clear(); addressLabel.Clear(); areaLabel.Clear(); nodes.Clear();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        first_column.Add(values[0]);
                        areaID.Add(values[1]);
                        address.Add(values[2]);
                        addressLabel.Add(values[3]);
                        areaLabel.Add(values[4]);
                        if (values[1].Contains("ZONE"))
                        {
                            string nodeNum = values[2].Substring(1, values[2].IndexOf("L") - 1);
                            if (!nodes.Contains(nodeNum))
                                nodes.Add(nodeNum);
                        }
                    }
                    label6.Text = label6.Text + nodes[0];
                    building_label.Text = first_column[0].Replace('|', ',');
                    databaseLED.BackColor = System.Drawing.Color.Lime;
                    databaseLED.Text = "DataBase is OK";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to read Database file, Defaults, Drawing Mapping or Drawings", "Error reading database file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                using (StreamWriter sw = File.AppendText("databaseDrawingError.txt"))
                {
                    sw.WriteLine(ex + "\n : [!377] " + time.Text + ", " + DateTime.Now.ToString("hh:mm:ss tt") + "\n\n");
                    sw.Close();
                }
            }
            currFACP = defaultSetting[2];
            drawing_count = images.Count;
            // initialize the alarm, trouble and super for the LED, uniform and unUniform
            alarmLEDs.Clear(); troubleLEDs.Clear(); superLEDs.Clear(); activeLEDs.Clear();
            alarmLEDs_temp.Clear(); troubleLEDs_temp.Clear(); superLEDs_temp.Clear(); activeLEDs_temp.Clear();
            alarmSquare.Clear(); troubleSquare.Clear(); superSquare.Clear(); activeSquare.Clear();
            alarmSquare_temp.Clear(); troubleSquare_temp.Clear(); superSquare_temp.Clear(); activeSquare_temp.Clear();
            alarmMultiPoint.Clear(); troubleMultiPoint.Clear(); superMultiPoint.Clear(); activeMultiPoint.Clear();
            alarmMultiPoint_temp.Clear(); troubleMultiPoint_temp.Clear(); superMultiPoint_temp.Clear(); activeMultiPoint_temp.Clear();
            for (int i = 0; i < nodeCount; i++)
            {
                alarmLEDs.Add(new List<List<RectangleF>>());
                troubleLEDs.Add(new List<List<RectangleF>>());
                superLEDs.Add(new List<List<RectangleF>>());
                activeLEDs.Add(new List<List<RectangleF>>());

                alarmLEDs_temp.Add(new List<List<RectangleF>>());
                troubleLEDs_temp.Add(new List<List<RectangleF>>());
                superLEDs_temp.Add(new List<List<RectangleF>>());
                activeLEDs_temp.Add(new List<List<RectangleF>>());

                alarmSquare.Add(new List<List<RectangleF>>());
                troubleSquare.Add(new List<List<RectangleF>>());
                superSquare.Add(new List<List<RectangleF>>());
                activeSquare.Add(new List<List<RectangleF>>());

                alarmSquare_temp.Add(new List<List<RectangleF>>());
                troubleSquare_temp.Add(new List<List<RectangleF>>());
                superSquare_temp.Add(new List<List<RectangleF>>());
                activeSquare_temp.Add(new List<List<RectangleF>>());

                alarmMultiPoint.Add(new List<List<PointF[]>>());
                troubleMultiPoint.Add(new List<List<PointF[]>>());
                superMultiPoint.Add(new List<List<PointF[]>>());
                activeMultiPoint.Add(new List<List<PointF[]>>());

                alarmMultiPoint_temp.Add(new List<List<PointF[]>>());
                troubleMultiPoint_temp.Add(new List<List<PointF[]>>());
                superMultiPoint_temp.Add(new List<List<PointF[]>>());
                activeMultiPoint_temp.Add(new List<List<PointF[]>>());
            }
            // Loading Zones Border, Led and youAreHere.
            try
            {
                zoneLEDCoord.Clear(); squareZoneCoord.Clear(); multiPointZoneCoord.Clear();
                ledZoneLabel.Clear(); ledZone.Clear(); squareZoneLabel.Clear(); squareZone.Clear(); multiPointZoneLabel.Clear(); multiPointZone.Clear();
                alarm.Clear(); trouble.Clear(); super.Clear(); active.Clear(); troubleCount.Clear(); superCount.Clear(); activeCount.Clear();
                for (int i = 0; i < drawing_count; i++)
                {
                    // initalize the dictionaries with the number of drawing (set the list size)
                    zoneLEDCoord.Add(new Dictionary<string, RectangleF>());
                    squareZoneCoord.Add(new Dictionary<string, RectangleF>());
                    multiPointZoneCoord.Add(new Dictionary<string, PointF[]>());
                    // initalize the List size with the number of drawing
                    ledZoneLabel.Add(new List<string>());
                    ledZone.Add(new List<string>());
                    squareZoneLabel.Add(new List<string>());
                    squareZone.Add(new List<string>());
                    multiPointZoneLabel.Add(new List<string>());
                    multiPointZone.Add(new List<string>());
                    alarm.Add(new bool());
                    trouble.Add(new bool());
                    super.Add(new bool());
                    active.Add(new bool());
                    troubleCount.Add(new int());
                    superCount.Add(new int());
                    activeCount.Add(new int());


                    using (var reader = new StreamReader(zonesCSV[i]))
                    {


                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var values = line.Split(',');
                            ledZoneLabel[i].Add(values[0]);
                            ledZone[i].Add(values[1]);
                        }
                    }
                    using (var reader = new StreamReader(zonesCSV[i + drawing_count]))
                    {

                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var values = line.Split(',');
                            squareZoneLabel[i].Add(values[0]);
                            squareZone[i].Add(values[1]);
                        }
                    }
                    using (var reader = new StreamReader(zonesCSV[i + drawing_count * 2]))
                    {

                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var values = line.Split(',');
                            multiPointZoneLabel[i].Add(values[0]);
                            multiPointZone[i].Add(values[1]);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to process Database data'" + database + "'", "Error reading database file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                using (StreamWriter sw = File.AppendText("databaseProcessingError.txt"))
                {
                    sw.WriteLine(ex + "\n : [!377] " + time.Text + ", " + DateTime.Now.ToString("hh:mm:ss tt") + "\n\n");
                    sw.Close();
                }
            }
            
                using (var reader = new StreamReader(zonesCSV[drawing_count * 3]))
                {
                    youAreHereCoord.Clear();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        youAreHereCoord.Add(values[0]);
                        var sizes = values[1].Split('|');
                        led_size = Int32.Parse(sizes[0]);
                        borderSize = Int32.Parse(sizes[1]);
                    }
                }
                youAreHereInt = youAreHereCoord[0].Split('|').Select(Int32.Parse).ToList();
            
            

            try
            {
                // Initialize the list of lists with 3 lists
                for (int i = 0; i < drawing_count; i++)
                {
                    for (int j = 0; j < nodeCount; j++)
                    {
                        alarmLEDs[j].Add(new List<RectangleF>());
                        troubleLEDs[j].Add(new List<RectangleF>());
                        superLEDs[j].Add(new List<RectangleF>());
                        activeLEDs[j].Add(new List<RectangleF>());

                        alarmLEDs_temp[j].Add(new List<RectangleF>());
                        troubleLEDs_temp[j].Add(new List<RectangleF>());
                        superLEDs_temp[j].Add(new List<RectangleF>());
                        activeLEDs_temp[j].Add(new List<RectangleF>());

                        alarmSquare[j].Add(new List<RectangleF>());
                        troubleSquare[j].Add(new List<RectangleF>());
                        superSquare[j].Add(new List<RectangleF>());
                        activeSquare[j].Add(new List<RectangleF>());

                        alarmSquare_temp[j].Add(new List<RectangleF>());
                        troubleSquare_temp[j].Add(new List<RectangleF>());
                        superSquare_temp[j].Add(new List<RectangleF>());
                        activeSquare_temp[j].Add(new List<RectangleF>());


                        alarmMultiPoint[j].Add(new List<PointF[]>());
                        troubleMultiPoint[j].Add(new List<PointF[]>());
                        superMultiPoint[j].Add(new List<PointF[]>());
                        activeMultiPoint[j].Add(new List<PointF[]>());


                        alarmMultiPoint_temp[j].Add(new List<PointF[]>());
                        troubleMultiPoint_temp[j].Add(new List<PointF[]>());
                        superMultiPoint_temp[j].Add(new List<PointF[]>());
                        activeMultiPoint_temp[j].Add(new List<PointF[]>());
                    }
                }
            }
            catch
            { }

            float x = resolution.Width / 1920;
            float y = resolution.Height / 1080;
            // Dictionary for zone locations on the drawing, MAKE THIS DYNAMIC
            try
            {
                for (int i = 0; i < drawing_count; i++)
                {
                    for (int j = 1; j < ledZone[i].Count; j++)
                    {
                        string[] coord = ledZone[i][j].Split('|');
                        zoneLEDCoord[i].Add(ledZoneLabel[i][j], new RectangleF(Int32.Parse(coord[0]) * x, Int32.Parse(coord[1]) * y, led_size * x, led_size * y));
                    }
                    for (int j = 1; j < squareZone[i].Count; j++)
                    {
                        string[] coord = squareZone[i][j].Split('|');
                        squareZoneCoord[i].Add(squareZoneLabel[i][j], new RectangleF(Int32.Parse(coord[0]) * x, Int32.Parse(coord[1]) * y, Int32.Parse(coord[2]) * x, Int32.Parse(coord[3]) * y));
                    }
                    for (int j = 1; j < multiPointZone[i].Count; j++)
                    {

                        string[] coord = multiPointZone[i][j].Split(';');
                        PointF[] zonePoints = new PointF[coord.Count()];
                        for (int k = 0; k < coord.Count(); k++)
                            zonePoints[k] = new PointF(Int32.Parse(coord[k].Split('|')[0]) * x, Int32.Parse(coord[k].Split('|')[1]) * y);
                        multiPointZoneCoord[i].Add(multiPointZoneLabel[i][j], zonePoints);
                    }

                }

            }
            catch
            {
                //statusPanel.AppendText(ledZone.Count+"");
            }

            string[] ports = SerialPort.GetPortNames();
            com1.Items.Clear();
            foreach (string port in ports)
            {
                com1.Items.Add(port + "_640");
                com1.Items.Add(port + "_3030");
            }
            supervisoryLED.Hide();
            Helper.TxtColor(alarmLog, Color.White);
            alarmLog.AppendText("Application just Started ............... \n\n");

            // If all buildings with 2 or more panles always have 3030 then no need to change this
            try
            {
                com1.SelectedIndex = panelType;
                //closePort.Text = "Open connection\n (" + com1.Text.Substring(0, 4) + ", "+ com2.Text.Substring(0, 4) + ")";
            }
            catch (Exception e)
            {
                MessageBox.Show("No COM Ports found " + e, "Error connecting to COM Port ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                if (com1.Text.Contains("640"))
                    port1 = new SerialPort(com1.Text.Substring(0, com1.Text.IndexOf('_')), 9600, Parity.Even, 7, StopBits.One);
                if (com1.Text.Contains("3030"))
                {
                    port1 = new SerialPort(com1.Text.Substring(0, com1.Text.IndexOf('_')), 9600, Parity.None, 8, StopBits.One);
                    is_Com1_3030 = true;
                    defaultTime = "11:51:50A WED APR 14, 2021";
                }
                else
                {
                    is_Com1_3030 = false;
                    defaultTime = "07:27A 042821 Wed";
                }


                if (port1 != null)
                {
                    port1.Handshake = Handshake.RequestToSendXOnXOff;
                    // Method to be called when there is data waiting in the port1's buffer 
                    port1.DataReceived += new SerialDataReceivedEventHandler(Port1_DataReceived);
                    // Begin communications 
                    port1.Open();
                    // Enter an application loop to keep this thread alive 
                    Console.ReadLine();
                    closePort.Text = "Close connection\n (" + com1.Text.Substring(0, 4) + ")";
                    com1.Enabled = false;
                    comStatusLED.BackColor = System.Drawing.Color.Lime;
                    comStatusLED.Text = "Connected";
                    testLds.Enabled = false;
                    alarmLog.WordWrap = false;
                    Helper.TxtColor(alarmLog, Color.LimeGreen);
                    alarmLog.AppendText("\n\n*** MIMIC PANEL Monitoring Openned by Supervisor ***\n\n");
                    alarmLog.ScrollToCaret();
                    alarmLog.WordWrap = true;
                    alarmLog.ScrollToCaret();
                }
            }
            catch
            {
                MessageBox.Show("Unable to connect to " + com1.Text + ", check cable", "Error connecting to " + com1.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            using (StreamReader r = new StreamReader(logFile))
            {
                while (r.ReadLine() != null)
                    lineCount++;
            }
        }
    }
}