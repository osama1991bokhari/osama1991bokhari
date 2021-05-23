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
            if (!blinkYouAreHere & rotationCounter == gf_idx)
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
        void PictureBox_Paint_super(object sender, PaintEventArgs e)
        {
            if (!blink1)
            {
                using (Pen p = new Pen(Color.IndianRed, borderSize))
                {
                    for(int i =0;i < nodeCount; i++)
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


        private void Form1_Load(object sender, EventArgs e)
        {
            InitTimer();
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            if (clrHist.Checked)
                System.IO.File.WriteAllText(logFile, string.Empty);
            Application.Exit();
        }

        private Timer timer100;//for boarder blinking
        private Timer timer700;//for youAreHere blinking
        private Timer timer3000;//for floor rotation

        static int currNodeIndx;
        static string currNode;
        static string currFACP = "";
        static int portPanelType = 0;
        static int panelType = 0;
        static int restartTimer = 0;
        static string defaults = "";
        static int nodeCount = 1;
        static bool blink1 = true;
        static bool blinkYouAreHere = true;
        static int gf_idx = 0;
        static int rotationCounter = 1;
        LinkedList<int> priorityFloor = new LinkedList<int>();

        static int borderSize = 10;
        static int led_size = 9;
        static int drawing_count;
        static int lineCount = 0;

        Regex modNum3030 = new Regex(@"L\d{2}([D,M])\d{3}");
        Regex modNum640 = new Regex(@"\d{1}([D,M])\d{3}");

        Regex separator3030 = new Regex(@"\d{2},\s\d{4}");//???
        Regex separator640 = new Regex(@"\d{6}");
        Regex troubles = new Regex(@"TROUBL");
        Regex clrTroubles = new Regex(@"CLR TB|CLEARED TROUBL");

        Regex modCode = new Regex(@"GEN ALARM|GEN TROUBL|TRACK SUPERV");

        private static string logFile = "LogFile.txt";
        private static string database = "";
        private Image youAreHere = Image.FromFile("pin-icon-460px.png");
        List<string> images = Directory.GetFiles("img", "*.*", SearchOption.AllDirectories).OrderBy(f => f).ToList();
        List<string> zonesCSV = Directory.GetFiles("zones", "*.*", SearchOption.AllDirectories).OrderBy(f => f).ToList();
        List<string> nodes = new List<string>();

        List <List<List<RectangleF>>> alarmLEDs = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> troubleLEDs = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> superLEDs = new List<List<List<RectangleF>>>();

        List<List<List<RectangleF>>> alarmLEDs_temp = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> troubleLEDs_temp = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> superLEDs_temp = new List<List<List<RectangleF>>>();

        List<List<List<RectangleF>>> alarmSquare = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> troubleSquare = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> superSquare = new List<List<List<RectangleF>>>();

        List<List<List<RectangleF>>> alarmSquare_temp = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> troubleSquare_temp = new List<List<List<RectangleF>>>();
        List<List<List<RectangleF>>> superSquare_temp = new List<List<List<RectangleF>>>();

        List<List<List<PointF[]>>> alarmMultiPoint = new List<List<List<PointF[]>>>();
        List<List<List<PointF[]>>> troubleMultiPoint = new List<List<List<PointF[]>>>();
        List<List<List<PointF[]>>> superMultiPoint = new List<List<List<PointF[]>>>();

        List<List<List<PointF[]>>> alarmMultiPoint_temp = new List<List<List<PointF[]>>>();
        List<List<List<PointF[]>>> troubleMultiPoint_temp = new List<List<List<PointF[]>>>();
        List<List<List<PointF[]>>> superMultiPoint_temp = new List<List<List<PointF[]>>>();



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
        //string[] floors = { "GF", "M1", "B1" };
        List<string> powerFail = new List<string>();

        List<int> troubleCount = new List<int>();
        List<int> superCount = new List<int>();
        RectangleF resolution = Screen.PrimaryScreen.Bounds;
        public void InitTimer()
        {
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
                foreach (string imgDir in images)
                {
                    pngImg.Add(Image.FromFile(imgDir));
                    pngName.Add(imgDir.Split('_')[1].Split('.')[0].Replace('^','.'));
                }
                gf_idx = pngName.IndexOf("Ground Floor");
                database = Directory.GetFiles("database", "*.csv")[0];
                using (var reader = new StreamReader(database))
                {

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        first_column.Add(values[0]);
                        areaID.Add(values[1]);
                        address.Add(values[2]);
                        addressLabel.Add(values[3]);
                        areaLabel.Add(values[4]);
                        if (values[1].Contains("ZONE")) { 
                        string nodeNum = values[2].Substring(1, values[2].IndexOf("L") - 1);
                        if (!nodes.Contains(nodeNum))
                            nodes.Add(nodeNum);}
                    }
                    label6.Text = label6.Text + nodes[0];
                    building_label.Text = first_column[0].Replace('|', ',');
                    databaseLED.BackColor = System.Drawing.Color.Lime;
                    databaseLED.Text = "DataBase is OK";
                }
            }
            catch
            {
                MessageBox.Show("Unable to read Database file or Drawings'" + database + "'", "Error reading database file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            drawing_count = images.Count;
            // initialize the alarm, trouble and super for the LED, uniform and unUniform
            for(int i = 0; i < nodeCount; i++)
            {
                alarmLEDs.Add(new List<List<RectangleF>>());
                troubleLEDs.Add(new List<List<RectangleF>>());
                superLEDs.Add(new List<List<RectangleF>>());

                alarmLEDs_temp.Add(new List<List<RectangleF>>());
                troubleLEDs_temp.Add(new List<List<RectangleF>>());
                superLEDs_temp.Add(new List<List<RectangleF>>());

                alarmSquare.Add(new List<List<RectangleF>>());
                troubleSquare.Add(new List<List<RectangleF>>());
                superSquare.Add(new List<List<RectangleF>>());

                alarmSquare_temp.Add(new List<List<RectangleF>>());
                troubleSquare_temp.Add(new List<List<RectangleF>>());
                superSquare_temp.Add(new List<List<RectangleF>>());

                alarmMultiPoint.Add(new List<List<PointF[]>>());
                troubleMultiPoint.Add(new List<List<PointF[]>>());
                superMultiPoint.Add(new List<List<PointF[]>>());

                alarmMultiPoint_temp.Add(new List<List<PointF[]>>());
                troubleMultiPoint_temp.Add(new List<List<PointF[]>>());
                superMultiPoint_temp.Add(new List<List<PointF[]>>());
            }
            // Loading Zones Border, Led and youAreHere. NEED TO MAKE THIS DYNAMIC (Done!!!) //Make this to not relay on order
            try
            {
                for(int i = 0; i < drawing_count; i++)
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
                    troubleCount.Add(new int());
                    superCount.Add(new int());
                    

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
            catch
            {
                MessageBox.Show("Unable to process Database data'" + database + "'", "Error reading database file", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            try
            {
                using (var reader = new StreamReader(zonesCSV[drawing_count * 3]))
                {

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
            }
            catch
            {
                MessageBox.Show("Unable to process data YouAreHere'" + database + "'", "Error reading youAreHere file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try 
            {
                // Initialize the list of lists with 3 lists, MAKE THIS DYNAMIC (MODYFY THE LOOP COUNT !!!!!)
                for (int i = 0; i < drawing_count; i++)
                {
                    for (int j = 0; j < nodeCount; j++)
                    {
                        alarmLEDs[j].Add(new List<RectangleF>());
                        troubleLEDs[j].Add(new List<RectangleF>());
                        superLEDs[j].Add(new List<RectangleF>());

                        alarmLEDs_temp[j].Add(new List<RectangleF>());
                        troubleLEDs_temp[j].Add(new List<RectangleF>());
                        superLEDs_temp[j].Add(new List<RectangleF>());

                        alarmSquare[j].Add(new List<RectangleF>());
                        troubleSquare[j].Add(new List<RectangleF>());
                        superSquare[j].Add(new List<RectangleF>());

                        alarmSquare_temp[j].Add(new List<RectangleF>());
                        troubleSquare_temp[j].Add(new List<RectangleF>());
                        superSquare_temp[j].Add(new List<RectangleF>());


                        alarmMultiPoint[j].Add(new List<PointF[]>());
                        troubleMultiPoint[j].Add(new List<PointF[]>());
                        superMultiPoint[j].Add(new List<PointF[]>());


                        alarmMultiPoint_temp[j].Add(new List<PointF[]>());
                        troubleMultiPoint_temp[j].Add(new List<PointF[]>());
                        superMultiPoint_temp[j].Add(new List<PointF[]>());
                    }     
                }
            }
            catch
            { }
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

            float x = resolution.Width / 1920;
            float y = resolution.Height / 1080;
            // Dictionary for zone locations on the drawing, MAKE THIS DYNAMIC
            try
            {
                for(int i = 0; i < drawing_count; i++)
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
            catch(Exception e)
            {
                MessageBox.Show("No COM Ports found "+e, "Error connecting to COM Port ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                if(com1.Text.Contains("640"))
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
                

                if (port1 != null) { 
                port1.Handshake = Handshake.RequestToSendXOnXOff;
                // Method to be called when there is data waiting in the port1's buffer 
                port1.DataReceived += new SerialDataReceivedEventHandler(Port1_DataReceived);
                // Begin communications 
                port1.Open();
                // Enter an application loop to keep this thread alive 
                Console.ReadLine();
                closePort.Text = "Close connection\n (" + com1.Text.Substring(0,4) +  ")";
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
            drawing.Paint += new System.Windows.Forms.PaintEventHandler(PictureBox_Paint_super);
            drawing.Paint += new System.Windows.Forms.PaintEventHandler(PictureBox_Paint_troubl);
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
                using (StreamWriter sw = File.AppendText("alarmEvnt.txt"))
                {
                    sw.Write(alarmEvnt.Text);
                    sw.Close();
                }
                using (StreamWriter sw = File.AppendText("statusPanel.txt"))
                {
                    sw.Write(statusPanel.Text);
                    sw.Close();
                }
                try
                {
                    File.Copy(logFile, Path.Combine(@"logs", "LogFile_" + date.Text + " - " + time.Text.Replace(':', '_') + ".txt"), true);
                }
                catch (IOException iox)
                {
                    Console.WriteLine(iox.Message);
                }
                System.IO.File.WriteAllText(logFile, string.Empty);
                lineCom1 = "";
                //Application.Restart();
                //Environment.Exit(0);//don't know if we need this.
            }
            resolution = Screen.PrimaryScreen.Bounds;
            // Making LED at bottom panel blink
            if (blink1 == false)
            {
                drawing.Refresh();
                if (alarm.Contains(true))
                    FireAlarmLED.Show();
                for(int i = 0;i < nodeCount;i++)
                {
                    if (troubleCount[i] > 0)
                    {
                        troubleLED.Show();
                        break;
                    }
                    else
                        troubleCount[i] = 0;
                }

                for (int i = 0; i < nodeCount; i++)
                {
                    if (superCount[i] > 0)
                    {
                        supervisoryLED.Show();
                        break;
                    }
                    else
                        superCount[i] = 0;
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
                if(port1 != null)
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
            catch(Exception ex) {
                using (StreamWriter sw = File.AppendText("COMErrorSignal.txt"))
                {
                    sw.WriteLine(ex + "\n checking CD and CTS Line: [!727] " + time.Text + ", " + date.Text + "\n\n");
                    sw.Close();
                }
            }
        }
        private void Timer3_Tick_Rotation(object sender, EventArgs e)
        {
            drawingRotate();
        }
        private void Minimize(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void Port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Show all the incoming data in the port1's buffer
            currNode = nodes[0];
            currNodeIndx = 0;
            if (!is_Com1_3030)
                portPanelType = 0;
            else
                portPanelType = 1;
            try
            {
            string signal1 = port1.ReadExisting();
            lineCom1 = lineCom1 + signal1;

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
                bool line_640 = !is_Com1_3030 & (lineCom1.Contains("\n") | lineCom1.Contains("\r") | Regex.IsMatch(lineCom1, @"\d{6}"));
                bool line_3030_no_module = is_Com1_3030 & Regex.IsMatch(lineCom1, @"\d{2},\s\d{4}") & !lineCom1.Contains("L0");
                bool line_3030_contains_moduel = false;
            try {
                    line_3030_contains_moduel = is_Com1_3030 & Regex.IsMatch(lineCom1, @"\d{2},\s\d{4}") &
                        Regex.IsMatch(lineCom1.Substring(separator3030.Match(lineCom1).Index), @"L\d{2}([D,M])\d{3}");
                }
            catch { }
                // check if we have a complete signal
                if (line_640 | line_3030_no_module | line_3030_contains_moduel| lineCom1.Contains("NORMAL")| lineCom1.Contains("RESET"))
                {
                    bool clear = true;
                    if (lineCom1.Contains("NORMAL"))
                        if((Regex.IsMatch(lineCom1.Substring(lineCom1.IndexOf("NORMAL")), @"\d{2},\s\d{4}")) |
                        Regex.IsMatch(lineCom1.Substring(lineCom1.IndexOf("NORMAL")), @"\d{6}"))
                            lineCom1 = "SYSTEM NORMAL                             "+defaultTime;
                    if (lineCom1.Contains("RESET"))
                        if((Regex.IsMatch(lineCom1.Substring(lineCom1.IndexOf("RESET")), @"\d{2},\s\d{4}") |
                        Regex.IsMatch(lineCom1.Substring(lineCom1.IndexOf("RESET")), @"\d{6}")))
                            lineCom1 = "SYSTEM RESET                             "+defaultTime;
                    if(lineCom1.Contains("PLEASE WAIT"))
                        lineCom1 = "SYSTEM POWER UP       PLEASE WAIT                             " + defaultTime;
                    List<string> sb = new List<string>();
                    if (clrTroubles.Matches(lineCom1).Count > 1)
                    {
                        foreach (Match match in clrTroubles.Matches(lineCom1))
                            if (Regex.IsMatch(lineCom1.Substring(match.Index), @"\d{2},\s\d{4}") | Regex.IsMatch(lineCom1.Substring(match.Index), @"\d{6}"))
                                Process_signal(lineCom1.Substring(match.Index));
                            else
                            {
                                clear = false;
                                lineCom1 = lineCom1.Substring(match.Index);
                            }

                        if (clear)
                            lineCom1 = "";
                    }
                    else if (troubles.Matches(lineCom1).Count > 1) {
                        foreach (Match match in troubles.Matches(lineCom1))
                            if (!lineCom1.Substring(match.Index).StartsWith("TROUBLE_MON"))
                            { 
                                if (Regex.IsMatch(lineCom1.Substring(match.Index), @"\d{2},\s\d{4}")| Regex.IsMatch(lineCom1.Substring(match.Index), @"\d{6}"))
                                    Process_signal(lineCom1.Substring(match.Index));
                                else
                                {
                                    clear = false;
                                    lineCom1 = lineCom1.Substring(match.Index);
                                }
                            }
                        if(clear)
                            lineCom1 = "";
                    }
                    else if(Regex.IsMatch(lineCom1, @"\d{2},\s\d{4}") | Regex.IsMatch(lineCom1, @"\d{6}"))
                    lineCom1 = Process_signal(lineCom1);
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText("port1ErrorSignal.txt"))
                {
                    sw.WriteLine(ex + "\nLine: [!782]" + time.Text + ", " + date.Text+"\n\n");
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
            completeSignal = Regex.Replace(completeSignal, @"\n|\r|IN SYSTEM|GEN ALARM|GEN TROUBL|TRACK SUPERV", "");//remove space and some words

            if (completeSignal[0] == '|')
                completeSignal = completeSignal.Substring(1);
            string[] lines = completeSignal.Split('|');
            //filter signal code type and store it in a variable
            //check content here
            if(is_Com1_3030)
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
                        sw.WriteLine(ex + "\nLine: [!828]" + time.Text + ", " + date.Text + "\n\n");
                        sw.Close();
                    }
                }

            // Check which mode we are in
            if (completeSignal.StartsWith("ALARM") & !completeSignal.Contains("ACKN"))
            {
                alarm[currNodeIndx] = true;
                trouble[currNodeIndx] = false;
                super[currNodeIndx] = false;
                // add one for track
            }
            if (completeSignal.StartsWith("TROUBL") & !completeSignal.StartsWith("TROUBLE_MON") &
                !completeSignal.Contains("ACKN") & !completeSignal.Contains("CLEARED") & !completeSignal.Contains("CLR"))
            {
                if (modNum3030.IsMatch(completeSignal) | modNum640.IsMatch(completeSignal))
                {
                    alarm[currNodeIndx] = false;
                    trouble[currNodeIndx] = true;
                    super[currNodeIndx] = false;
                }
                else
                {
                    statusPanel.Invoke(new Action(() => { 
                    Helper.TxtColor2(statusPanel, Color.Orange);
                    if (completeSignal.Contains("BATTERY") | completeSignal.Contains("M157"))
                    {
                        powerLED.BackColor = Color.Red;
                        powerLED.Text = "BATTERY FAIL";
                        powerFail.Add("battry");
                        statusPanel.AppendText("TROUBLE, BATTERY FAILURE\n");
                    }
                    else if (completeSignal.Contains("AC FAIL") | completeSignal.Contains("M156"))
                    {
                        powerLED.BackColor = Color.Red;
                        powerLED.Text = "AC FAIL";
                        powerFail.Add("AC");
                        statusPanel.AppendText("TROUBLE, AC FAILURE\n");
                    }
                    else if (completeSignal.Contains("GENERAL MON") | completeSignal.Contains("M155"))
                    {
                        powerLED.BackColor = Color.Red;
                        powerLED.Text = "GENERAL MON";
                        powerFail.Add("GENERAL MON");
                        statusPanel.AppendText("TROUBLE, GENERAL MONITOR\n");
                    }
                    else if (completeSignal.Contains("EARTH") | completeSignal.Contains("M158"))
                    {
                        powerLED.BackColor = Color.Red;
                        powerLED.Text = "EARTH FAIL";
                        powerFail.Add("EARTH");
                        statusPanel.AppendText("TROUBLE, EARTH FAILURE\n");
                    }
                    else if (completeSignal.Contains("CHARGER FAIL") | completeSignal.Contains("M159"))
                    {
                        powerLED.BackColor = Color.Red;
                        powerLED.Text = "CHARGER FAIL";
                        powerFail.Add("CHARGER");
                        statusPanel.AppendText("TROUBLE, CHARGER FAILURE\n");
                    }
                    else if (completeSignal.Contains("NETWORK FAIL") | completeSignal.Contains("NCM COMM FAILURE"))
                    {
                        statusPanel.AppendText("TROUBLE, COMMUNICATION FAILURE\n");
                    }
                    else
                        statusPanel.AppendText("TROUBLE, " + lines[1] + "\n");// check this port panel type variable
                    }));
                }

                troubleCount[currNodeIndx]++;
            }
            if ((completeSignal.Contains("SUPERV") | completeSignal.Contains("ACTIVE TAMPER")) & !completeSignal.Contains("CLR") &
                !completeSignal.Contains("ACKN") & !completeSignal.Contains("CLEARED"))//this should be separated for track superv
            {
                if (modNum3030.IsMatch(completeSignal) | modNum640.IsMatch(completeSignal))
                {
                    alarm[currNodeIndx] = false;
                    trouble[currNodeIndx] = false;
                    super[currNodeIndx] = true;
                }
                superCount[currNodeIndx]++;
            }
            if (completeSignal.Contains("CLR TB") | completeSignal.Contains("CLEARED TROUBL"))
            {
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
                    if (completeSignal.Contains("BATT") | completeSignal.Contains("M157"))
                    {
                        powerFail.Remove("battry");
                        removeLine("BATT", "TROUBLE");
                    } 
                    if (completeSignal.Contains("AC FAIL") | completeSignal.Contains("M156"))
                    {
                        powerFail.Remove("AC");
                        removeLine("AC FAIL", "TROUBLE");
                    }
                    if (completeSignal.Contains("EARTH") | completeSignal.Contains("M158"))
                    {
                        powerFail.Remove("EARTH");
                        removeLine("EARTH", "TROUBLE");
                    }
                    if (completeSignal.Contains("CHARGER") | completeSignal.Contains("M159"))
                    {
                        powerFail.Remove("CHARGER");
                        removeLine("CHARGER", "TROUBLE");
                    }
                    if (completeSignal.Contains("GENERAL MON") | completeSignal.Contains("M155"))
                    {
                        powerFail.Remove("GENERAL MON");
                        removeLine("GENERAL MON", "TROUBLE");
                    }
                    if (completeSignal.Contains("NCM")| completeSignal.Contains("NETWORK"))
                    {
                        removeLine("COMMUNICATION", "TROUBLE");
                    }
                    if (!powerFail.Any())
                    {
                        powerLED.Invoke(new Action(() => {
                            powerLED.BackColor = Color.Lime;
                            powerLED.Text = "";}));
 
                    }
                    removeLine(completeSignal.Split('|')[1], "TROUBLE");

                }
                troubleCount[currNodeIndx]--;
                drawing.Invalidate();

            }
            if ((completeSignal.Contains("CLR") | completeSignal.Contains("CLEARED")) & ((completeSignal.Contains("SUPERV") |
                completeSignal.Contains("ACTIVE TAMPER"))))
            {
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
            if (completeSignal.Contains("RESET"))
            {
                for(int i = 0;i < nodeCount; i++)
                {
                    alarm[i] = false;
                    //trouble[i] = false;
                    //super[i] = false;
                }
                
                drawing.Invalidate();

                var rectToReset = new List<List<List<RectangleF>>>();
                var pointToReset = new List<List<List<PointF[]>>>();

                for(int i = 0;i < nodeCount; i++)
                {
                rectToReset = new List<List<List<RectangleF>>>() {alarmLEDs[i], alarmLEDs_temp[i],
                    alarmSquare[i], alarmSquare_temp[i]};

                pointToReset = new List<List<List<PointF[]>>>() {alarmMultiPoint[i], alarmMultiPoint_temp[i]};

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

                for(int i = 0;i < nodeCount; i++)
                {
                //troubleCount[currNodeIndx] = 0;
                //superCount[currNodeIndx] = 0;
                }
                priorityFloor.Clear();
                statusPanel.Invoke(new Action(() => { 
                    List<string> finalLines = statusPanel.Lines.ToList();
                    finalLines.RemoveAll(x => x.StartsWith("ALARM") || x.StartsWith("TAMPER"));
                    statusPanel.Lines = finalLines.ToArray();}));
                alarmEvnt.Invoke(new Action(() => {
                    alarmEvnt.Text = "";}));
                alarmLog.Invoke(new Action(() => {
                    alarmLog.ResetText();
                    alarmLog.ForeColor = Color.Turquoise;
                    alarmLog.Select(alarmLog.TextLength, alarmLog.TextLength);
                    alarmLog.ScrollToCaret();}));

            }
            if (completeSignal.Contains("NORMAL"))
            {
                    alarm[currNodeIndx] = false;
                    trouble[currNodeIndx] = false;
                    super[currNodeIndx] = false;

                    var rectToReset = new List<List<List<RectangleF>>>();
                    var pointToReset = new List<List<List<PointF[]>>>();

                    rectToReset = new List<List<List<RectangleF>>>() {alarmLEDs[currNodeIndx], troubleLEDs[currNodeIndx], superLEDs[currNodeIndx], alarmLEDs_temp[currNodeIndx], troubleLEDs_temp[currNodeIndx],
                    superLEDs_temp[currNodeIndx], alarmSquare[currNodeIndx], troubleSquare[currNodeIndx], superSquare[currNodeIndx], alarmSquare_temp[currNodeIndx], troubleSquare_temp[currNodeIndx], superSquare_temp[currNodeIndx]};

                    pointToReset = new List<List<List<PointF[]>>>() {alarmMultiPoint[currNodeIndx], troubleMultiPoint[currNodeIndx], superMultiPoint[currNodeIndx], alarmMultiPoint_temp[currNodeIndx],
                    troubleMultiPoint_temp[currNodeIndx], superMultiPoint_temp[currNodeIndx]};

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
                if(!alarm.Contains(false))
                    priorityFloor.Clear();
                statusPanel.Invoke(new Action(() => {
                    statusPanel.Text = ""; //!!! this for only single panels
                }));
                
            }

            if (alarm[currNodeIndx] | trouble[currNodeIndx] | super[currNodeIndx])
            {
                string fm200 = "";
                if (completeSignal.Contains("-Z1"))
                    fm200 = " Alarm1";
                else if (completeSignal.Contains("-Z2"))
                    fm200 = " Alarm2";
                else if (completeSignal.Contains("-T"))
                    fm200 = " Trouble";
                else if (completeSignal.Contains("-GD"))
                    fm200 = " GAS DISCHARGE";
                if (!completeSignal.Contains("CLR") & !completeSignal.Contains("ACKN") & !completeSignal.Contains("CLEARED"))
                    if (modNum640.IsMatch(completeSignal))
                    {
                        try
                        {
                            if(alarm[currNodeIndx] & !trouble[currNodeIndx] & !super[currNodeIndx])
                                for (int i = 1; i < areaID.Count(); i++)
                                {//extracting module zone and room lable and writing to alarmEvent if alarm.
                                    if (address[i].Contains(signal_module) & address[i].Contains(currNode))
                                    {
                                        signal_zone = areaID[i];
                                        zone_label = areaLabel[i];
                                        alarmEvnt.Invoke(new Action(() => {
                                            alarmEvnt.AppendText(signal_zone.Replace("ZONE ", "Z") + ", " + zone_label + ", " + signal_floor + "\n"); }));
                                    
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
                                }
                            }
                            else
                            {
                                statusPanel.Invoke(new Action(() => { 
                                Helper.TxtColor2(statusPanel, Color.Orange);
                                statusPanel.AppendText("Device: " + signal_module + ", is not found in database.\n");
                                statusPanel.ScrollToCaret();}));
                            }

                            if (completeSignal.StartsWith("ALARM") & !completeSignal.Contains("CLEARED") & !completeSignal.Contains("ACKED"))
                            {   //Writing to AlarmEvnt
                                if (signal_module != "")
                                for (int i = 1; i < areaID.Count(); i++)
                                {  
                                        if (address[i].Contains(signal_module) & address[i].Contains(currNode))
                                        {
                                            statusPanel.Invoke(new Action(() => { 
                                                Helper.TxtColor2(statusPanel, Color.Red);
                                                statusPanel.AppendText("ALARM,  " + address[i] + ",  " +
                                                areaID[i].Replace("ZONE ", "Z") + ", " + areaLabel[i] + ", " + addressLabel[i] + fm200 + "\n");
                                                statusPanel.ScrollToCaret();
                                                if (statusPanel.Lines.Count() < 30)
                                                    Helper.ColorLine("ALARM", statusPanel);
                                            }));
                                            break;
                                        }
                                }
                            }
                            string signal_module_sp = "";
                            signal_module_sp = modNum640.Match(completeSignal).ToString();
                            if (completeSignal.StartsWith("TROUBL") & !completeSignal.StartsWith("TROUBLE_MON") & !completeSignal.Contains("CLEARED") & !completeSignal.Contains("ACKED"))
                            {

                                for (int i = 1; i < areaID.Count(); i++)
                                {
                                    if (signal_module_sp == "")
                                    {
                                        statusPanel.Invoke(new Action(() => { 
                                            string panelNum = address[4].Substring(0, 4);
                                            Helper.TxtColor2(statusPanel, Color.Orange);
                                            statusPanel.AppendText("TROUBLE,  " + panelNum + ",  " + lines[1+portPanelType] + fm200 + "\n");
                                            statusPanel.ScrollToCaret();
                                            if (statusPanel.Lines.Count() < 30)
                                                Helper.ColorLine("TROUBLE", statusPanel);
                                        }));
                                        break;
                                    }
                                    if (address[i].Contains(signal_module_sp) & address[i].Contains(currNode))
                                    {
                                        statusPanel.Invoke(new Action(() => { 
                                        Helper.TxtColor2(statusPanel, Color.Orange);
                                        statusPanel.AppendText("TROUBLE,  " + ",  " + address[i] + ",  " +
                                            areaID[i].Replace("ZONE ", "Z") + ", " + areaLabel[i] + ", " + addressLabel[i] + fm200 + "\n");
                                            statusPanel.ScrollToCaret();
                                            if (statusPanel.Lines.Count() < 30)
                                                Helper.ColorLine("TROUBLE", statusPanel);
                                        }));
                                        break;
                                    }

                                }
                            }
                            if ((completeSignal.Contains("SUPERV") | completeSignal.Contains("ACTIVE TAMPER")) & !completeSignal.Contains("CLEARED") & !completeSignal.Contains("ACKED"))
                            {
                                
                                for (int i = 1; i < areaID.Count(); i++)
                                {
                                    if (signal_module_sp == "")
                                    {
                                        statusPanel.Invoke(new Action(() => { 
                                            string panelNum = address[4].Substring(0, 4);
                                            Helper.TxtColor2(statusPanel, Color.IndianRed);
                                            statusPanel.AppendText("SUPERVISORY,  " + panelNum + ",  " + lines[1+portPanelType] + fm200 + "\n");
                                            statusPanel.ScrollToCaret();
                                            if (statusPanel.Lines.Count() < 30)
                                                Helper.ColorLine("SUPERVISORY", statusPanel);
                                        }));
                                        break;
                                    }
                                    if (address[i].Contains(signal_module_sp) & address[i].Contains(currNode))
                                    {
                                        statusPanel.Invoke(new Action(() => { 
                                            Helper.TxtColor2(statusPanel, Color.IndianRed);
                                            statusPanel.AppendText("SUPERVISORY,  " + ",  " + address[i] + ",  " +
                                            areaID[i].Replace("ZONE ", "Z") + ", " + areaLabel[i] + ", " + addressLabel[i] + fm200 + "\n");
                                            statusPanel.ScrollToCaret();
                                            if (statusPanel.Lines.Count() < 30)
                                                Helper.ColorLine("SUPERVISORY", statusPanel);
                                        }));
                                        break;
                                    }
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            using (StreamWriter sw = File.AppendText("signalProcessErrorSignal.txt"))
                            {
                                sw.WriteLine(ex + "\nHandling Signals\nLine: 1621" + time.Text + ", " + date.Text + "\n\n");
                                sw.Close();
                            }
                        }
                    }
            }

            // Writning to log file
            try
                { 
                    using (StreamWriter sw = File.AppendText(logFile))
                    {
                        string[] line_list = completeSignal.Split('|');
                        line_list[line_list.Count() - 1] = "  "+time.Text + " " + date.Text;
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
                        alarmLog.Invoke(new Action(() => {
                            if (alarmLog.Lines.Count() > 6)
                            {
                                Helper.HighlightLineContaining(alarmLog, alarmLog.Lines.Count() - 5, "WAITING", Color.Black);
                                Helper.HighlightLineContaining(alarmLog, alarmLog.Lines.Count() - 4, "WAITING", Color.Black);
                                Helper.HighlightLineContaining(alarmLog, alarmLog.Lines.Count() - 3, "WAITING", Color.Black);
                                Helper.HighlightLineContaining(alarmLog, alarmLog.Lines.Count() - 2, "WAITING", Color.Black);
                                Helper.HighlightLineContaining(alarmLog, alarmLog.Lines.Count() - 1, "WAITING", Color.Black);

                            }
                            string line = lineCount + "|" + completeSignal + " " + signal_module;

                            line_list = line.Split('|');
                            string line_sig_type = "";
                            if (int.TryParse(line_list[0], out _))
                                line_sig_type = line_list[1].Trim()+ " " +line_list[2].Trim();
                            else
                                line_sig_type = line_list[0].Trim()+ " " +line_list[1].Trim();
                            if (is_Com1_3030 & line_list.Count() > 3)
                                line_sig_type = line_sig_type + " " + line_list[2].Trim();
                            string loop = "1";
                            string module = modNum640.Match(line).ToString();
                            if (module.Contains("L"))
                                loop = module[2] + "";
                            if (currNode == "221")
                                currFACP = "2";
                            else if (currNode == "222")
                                currFACP = "3";
                            else
                                currFACP = "1";

                            alarmLog.WordWrap = false;
                            Helper.TxtColor(alarmLog, Color.Turquoise);
                            alarmLog.AppendText("Date: " + date.Text + "\nTime: " + time.Text + "\nData Signal #: " + (lineCount - 1) + "\n");
                            Helper.TxtColor(alarmLog, Color.Orange);
                            if (line.Contains("NORMAL"))
                                alarmLog.AppendText("Event Name: " + line_sig_type + " SYSTEM NORMAL\nNode Address: " + currNode +
                                    " FACP-0" + currFACP + "\nLoop Number: " + loop + "\n\n");
                            else
                                alarmLog.AppendText("Event Name: " + line_sig_type + " " + module_code+ " " +signal_module + "\nNode Address: " + currNode + " FACP-0" + currFACP +
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
                catch{}
            return "";//return ramaining string instead

        }
        /**/
        private void ClosePort_Click(object sender, EventArgs e)
        {
            if (!testLds.Checked)
            {
                if (closePort.Text.Contains("Open"))
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
                        port1.Handshake = Handshake.RequestToSendXOnXOff;
                        // Method to be called when there is data waiting in the port1's buffer 
                        port1.DataReceived += new SerialDataReceivedEventHandler(Port1_DataReceived);
                        // Begin communications 
                        port1.Open();
                        // Enter an application loop to keep this thread alive 
                        Console.ReadLine();
                        closePort.Text = "Close connection\n (" + com1.Text.Substring(0, 4) +")";
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
                    closePort.Text = "Open connection\n (" + com1.Text.Substring(0, 4) +  ")";
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
            else {
                MessageBox.Show("Please stop the test LED first", "Error connecting to " + com1.Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);}
        }
        private void Com1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(com1.Text.Length > 3)
                closePort.Text = "Open connection\n (" + com1.Text.Substring(0, 4) + ")";
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
        
        private void drawingRotate()
        {
            //if(!clrHist.Checked)
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
            }

            if (rotationCounter >= images.Count)
                rotationCounter = 0;
            drawing.Invoke(new Action(() => { drawing.Image = pngImg[rotationCounter];}));
            floor.Invoke(new Action(() => {floor.Text = pngName[rotationCounter];}));
        }
        private void removeLine(string word, string sigType)
        {
            statusPanel.Invoke(new Action(() => {
                List<string> finalLines = statusPanel.Lines.ToList();
                finalLines.RemoveAll(x => x.Contains(word) & x.Contains(sigType));
                statusPanel.Lines = finalLines.ToArray();
                if (statusPanel.Lines.Count() < 30)
                    Helper.ColorLine(sigType, statusPanel);
            }));

        }
    }
}