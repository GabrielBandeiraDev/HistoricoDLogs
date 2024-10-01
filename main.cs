/*
 
  Por: Pedro Lucas - Última atualização em: 25/04/2024
  
  JIG ODU -> Segue a mesma lógica do FCT GAMA e FCT HARMAN;
  O LOG É GERADO NO TRACK MODE On e Off.

  ADICIONADO: Opções de modelo -> ODU e ODU Quente Frio
  
 */

using Limilabs.Barcode;
using System;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FCT_FACTORY
{
    public partial class Form1 : Form
    {
        public Bitmap bitmap;
        public Graphics graphics;
        public Rectangle rect;
        public Bitmap cropped;
        public string recebeFormSerial;
        string channel = "", snum = "", addr = "", snum2 = "";
        private EAN13 ean13 = new EAN13();
        double temp_ini;
        double temp_ini2;
        double v_short_ac_max, /*v_short_dc_max, v_voltage_33_max, v_voltage_12_max, v_voltage_15_max,*/ v_current_ac_max, v_discharge2/* v_motor_max, v_condenser_max,*/, v_voltage_33_33, v_voltage_12_15, v_temperature1, v_temperature2, v_temperature3, v_motor, v_condensador, v_discharge, v_inverter;

        //array usado para percorrer os testes - necessidade: pular testes
        int[] teste127_220 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        int[] teste127_220_oduPadrao = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        bool msgDeErro = false;
        public bool instanciaEmerg = false;
        bool inicio2, fail2;
        int indexTest2 = 0;
        // int contaFalhaTP = 0;


        int linhasTotal = 0;
        bool linux = false;
        // string cmdAT = "";
        string caminho = Application.StartupPath.ToString() + "/config.txt";
        string mac;
        string system;
        // string executar;
        public bool login = false;
        // string testStatusReport = "";

        // string IP;
        // int PORTA;
        // int SN_LEN = 0;
        string testStatus = "FAIL";
        //string testShortCurt = "FAIL";
        string test_100vac_4a = "NULL", test_efficiency = "NULL", test_ripple = "NULL", test_overload = "NULL", test_powerOff = "NULL", test_high_vac4a = "NULL", test_high_vac75a = "NULL", test_high_vac0a = "NULL", test_discharger = "NULL", comandoTest = "NULL";
        string GAC = "";
        string GVT = "";
        string GAP = "";
        string GEF = "";
        public string modeloSelecionado = "";
        string[,] TESTS = new string[3, 20];
        string[,] TESTS2 = new string[3, 20];
        double[] limits = new double[21];
        double[] limitsMin = new double[21];
        double[] limitsMax = new double[21];
        bool trackingTest;
        string _mode;
        string modeloPassando;
        string strcon = "";
        public string UserName = "";
        string directory = "";
        int contador = 0;
        int contaFalha = 1;
        Boolean inicio = false;
        Boolean fail = false;
        bool reteste = false;
        bool[] TEST = new bool[21];
        bool[] TEST2 = new bool[21];
        bool[] RES_TEST = new bool[21];
        bool[] RES_TEST2 = new bool[21];
        bool getSerial = true;
        bool getSerial2 = true;
        string statusTest = "";
        int indexTest = 0;
        //string cicliTimerStr = "NULL";
        //bool testRead = false;
        int ms = 0;
        string[] str;
        string validaTeste;
        bool testPass = true;
        int lineReteste = 0;
        


        /// //////////////////
        string naoFazteste2;
        string naoFazTeste1;
        string modeloPassando2;
        int[] teste127_220_1 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        int[] teste127_220_oduPadrao_2 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        /////////////////////


        Version versao = Assembly.GetExecutingAssembly().GetName().Version;

        public Form1()
        {
            InitializeComponent();
        }

        private void CreateEan13()
        {
            ean13.CountryCode = "00";
            ean13.ManufacturerCode = "00";
            ean13.ProductCode = "12345";
        }

        Process cmd = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();

        OleDbConnection conexao;
        OleDbCommand dbcmd;

        private void cmdExec(string comando)
        {
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + comando;
            cmd.StartInfo = startInfo;
            cmd.Start();
            cmd.Close();
        }

        public void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Enabled = false;
            dataGridView2.Enabled = false;
            textBox7.Enabled = false;

            if (Directory.Exists("/home"))
            {
                string[] lines = File.ReadAllLines(caminho);

                foreach (string line in lines)
                {
                    // Use a tab to indent each line of the file.

                    string[] str = line.Split('=');
                    if (str[0] == "port") { serialPort1.PortName = str[1]; }
                    if (str[0] == "port2") { serialPort2.PortName = str[1]; }
                    if (str[0] == "speed") { timer1.Interval = int.Parse(str[1]); }
                    if (str[0] == "baudRate") { serialPort1.BaudRate = int.Parse(str[1]); }
                    if (str[0] == "baudRate2") { serialPort2.BaudRate = int.Parse(str[1]); }
                    if (str[0] == "script") { caminho = str[1]; }

                    StreamReader sr = new StreamReader(caminho);
                    richTextBox1.Text = sr.ReadToEnd();
                    sr.Close();

                }

                linux = true;
                checkBox1.Enabled = false;
                textBox3.Enabled = false;
            }
            else
            {
                IniFile _myIni = new IniFile(Application.StartupPath.ToString() + "\\Setup.ini");
                strcon = _myIni.Read("strCon", "dataBase");
                directory = _myIni.Read("dirExport", "dataBase");

                serialPort1.PortName = _myIni.Read("com", "Port");
                serialPort1.BaudRate = int.Parse(_myIni.Read("baudRate", "Port"));

                trackingTest = bool.Parse(_myIni.Read("track", "dataBase"));
                _mode = _myIni.Read("mode", "dataBase");
                modeloPassando = _myIni.Read("modelLine", "dataBase");
                modeloPassando2 = _myIni.Read("modelLine2", "dataBase");

                serialPort2.PortName = _myIni.Read("com2", "Port");
                serialPort2.BaudRate = int.Parse(_myIni.Read("baudRate2", "Port"));
                timer1.Interval = int.Parse(_myIni.Read("Speed", "setSpeed"));

                string cm = _myIni.Read("SCRIPT", "dataBase");
                string posto = _myIni.Read("ID", "dataBase");

                txtID.Text = "TEST " + posto;
                StreamReader sr = new StreamReader(cm);
                richTextBox1.Text = sr.ReadToEnd();
                sr.Close();


                //puxando os valores do arquivo ini - referente a range
                limits[0] = double.Parse(_myIni.Read("discharge_min", "dataBase"));
                v_discharge = double.Parse(_myIni.Read("discharge_max", "dataBase"));

                limits[1] = double.Parse(_myIni.Read("short_ac_min", "dataBase"));
                v_short_ac_max = double.Parse(_myIni.Read("short_ac_max", "dataBase"));

                limits[2] = double.Parse(_myIni.Read("voltage_33_min", "dataBase"));
                v_voltage_33_33 = double.Parse(_myIni.Read("voltage_33_max", "dataBase"));

                limits[3] = double.Parse(_myIni.Read("voltage_12_15_min", "dataBase"));
                v_voltage_12_15 = double.Parse(_myIni.Read("voltage_12_15_max", "dataBase"));

                limits[4] = double.Parse(_myIni.Read("current_ac_min", "dataBase"));
                v_current_ac_max = double.Parse(_myIni.Read("current_ac_max", "dataBase"));

                limits[5] = double.Parse(_myIni.Read("temperature1_min", "dataBase"));
                v_temperature1 = double.Parse(_myIni.Read("temperature1_max", "dataBase"));

                limits[6] = double.Parse(_myIni.Read("temperature2_min", "dataBase"));
                v_temperature2 = double.Parse(_myIni.Read("temperature2_max", "dataBase"));

                limits[7] = double.Parse(_myIni.Read("temperature3_min", "dataBase"));
                v_temperature3 = double.Parse(_myIni.Read("temperature3_max", "dataBase"));

                limits[8] = double.Parse(_myIni.Read("motor_min", "dataBase"));
                v_motor = double.Parse(_myIni.Read("motor_max", "dataBase"));

                limits[9] = double.Parse(_myIni.Read("condenser_min", "dataBase"));
                v_condensador = double.Parse(_myIni.Read("condenser_max", "dataBase"));

                limits[10] = double.Parse(_myIni.Read("inverter_min", "dataBase"));
                v_inverter = double.Parse(_myIni.Read("inverter_max", "dataBase"));

                limits[11] = double.Parse(_myIni.Read("discharge_min2", "dataBase"));
                v_discharge2 = double.Parse(_myIni.Read("discharge_max2", "dataBase"));

                naoFazTeste1 = _myIni.Read("testplaca1", "dataBase");

                naoFazteste2 = _myIni.Read("testplaca2", "dataBase");

                txtID.Text = serialPort1.PortName;

                if (trackingTest)
                {
                    this.Visible = false;
                    UserId user = new UserId(this);
                    user.ShowDialog();
                    login = true;

                    if (!login)
                    {
                        this.Close();
                    }

                    this.Visible = true;


                }
            }




            //Modelo do DataGrid1 
            if (modeloPassando == "model_quenteFrio")
            {
                linhasTotal = 13;
                for (int i = 0; i < linhasTotal; i++)
                {
                    dataGridView1.Rows.Add();

                    if (naoFazTeste1 == "NaoFazTeste")
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    }

                }
            }

            if (modeloPassando == "model_padrao")
            {
                linhasTotal = 12;
                for (int i = 0; i < linhasTotal; i++)
                {
                    dataGridView1.Rows.Add();


                    if (naoFazTeste1 == "NaoFazTeste")
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    }

                }
            }

            //Modelo do DataGrid2
            if (modeloPassando2 == "model_quenteFrio")
            {
                linhasTotal = 13;
                for (int i = 0; i < linhasTotal; i++)
                {
                    dataGridView2.Rows.Add();


                    if (naoFazteste2 == "NaoFazTeste")
                    {
                        dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    }

                }
            }

            if (modeloPassando2 == "model_padrao")
            {
                linhasTotal = 12;
                for (int i = 0; i < linhasTotal; i++)
                {
                    dataGridView2.Rows.Add();


                    if (naoFazteste2 == "NaoFazTeste")
                    {
                        dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    }

                }
            }

            //Modelo do DataGrid1 
            if (modeloPassando == "model_padrao")
            {


                dataGridView1.Rows[0].Cells[0].Value = "DISCHARGE";
                dataGridView1.Rows[0].Cells[2].Value = limits[0] + " a " + v_discharge;

                dataGridView1.Rows[1].Cells[0].Value = "SHORT AC";
                dataGridView1.Rows[1].Cells[2].Value = limits[1] + " a " + v_short_ac_max;

                dataGridView1.Rows[2].Cells[0].Value = "VOLTAGE 3.3v";
                dataGridView1.Rows[2].Cells[2].Value = limits[2] + " a " + v_voltage_33_33;

                dataGridView1.Rows[3].Cells[0].Value = "VOLTAGE 12v e 15v";
                dataGridView1.Rows[3].Cells[2].Value = limits[3] + " a " + v_voltage_12_15;

                dataGridView1.Rows[4].Cells[0].Value = "CURRENT AC";
                dataGridView1.Rows[4].Cells[2].Value = limits[4] + " a " + v_current_ac_max + " (Amp)";

                dataGridView1.Rows[5].Cells[0].Value = "TEMPERATURE 1";
                dataGridView1.Rows[5].Cells[2].Value = limits[5] + " a " + v_temperature1;

                dataGridView1.Rows[6].Cells[0].Value = "TEMPERATURE 2";
                dataGridView1.Rows[6].Cells[2].Value = limits[6] + " a " + v_temperature2;

                dataGridView1.Rows[7].Cells[0].Value = "TEMPERATURE 3";
                dataGridView1.Rows[7].Cells[2].Value = limits[7] + " a " + v_temperature3;

                dataGridView1.Rows[8].Cells[0].Value = "MOTOR";
                dataGridView1.Rows[8].Cells[2].Value = limits[8] + " a " + v_motor;

                dataGridView1.Rows[9].Cells[0].Value = "CONDENSER";
                dataGridView1.Rows[9].Cells[2].Value = limits[9] + " a " + v_condensador;

                dataGridView1.Rows[10].Cells[0].Value = "CHECK OUT";
                dataGridView1.Rows[10].Cells[2].Value = limits[11] + " a " + v_discharge2;

                dataGridView1.Rows[11].Cells[0].Value = "FINISHED";

            }

            if (modeloPassando2 == "model_padrao")
            {
                dataGridView2.Rows[0].Cells[0].Value = "DISCHARGE";
                dataGridView2.Rows[0].Cells[2].Value = limits[0] + " a " + v_discharge;

                dataGridView2.Rows[1].Cells[0].Value = "SHORT AC";
                dataGridView2.Rows[1].Cells[2].Value = limits[1] + " a " + v_short_ac_max;

                dataGridView2.Rows[2].Cells[0].Value = "VOLTAGE 3.3v";
                dataGridView2.Rows[2].Cells[2].Value = limits[2] + " a " + v_voltage_33_33;

                dataGridView2.Rows[3].Cells[0].Value = "VOLTAGE 12v e 15v";
                dataGridView2.Rows[3].Cells[2].Value = limits[3] + " a " + v_voltage_12_15;

                dataGridView2.Rows[4].Cells[0].Value = "CURRENT AC";
                dataGridView2.Rows[4].Cells[2].Value = limits[4] + " a " + v_current_ac_max + " (Amp)";

                dataGridView2.Rows[5].Cells[0].Value = "TEMPERATURE 1";
                dataGridView2.Rows[5].Cells[2].Value = limits[5] + " a " + v_temperature1;

                dataGridView2.Rows[6].Cells[0].Value = "TEMPERATURE 2";
                dataGridView2.Rows[6].Cells[2].Value = limits[6] + " a " + v_temperature2;

                dataGridView2.Rows[7].Cells[0].Value = "TEMPERATURE 3";
                dataGridView2.Rows[7].Cells[2].Value = limits[7] + " a " + v_temperature3;

                dataGridView2.Rows[8].Cells[0].Value = "MOTOR";
                dataGridView2.Rows[8].Cells[2].Value = limits[8] + " a " + v_motor;

                dataGridView2.Rows[9].Cells[0].Value = "CONDENSER";
                dataGridView2.Rows[9].Cells[2].Value = limits[9] + " a " + v_condensador;

                dataGridView2.Rows[10].Cells[0].Value = "CHECK OUT";
                dataGridView2.Rows[10].Cells[2].Value = limits[11] + " a " + v_discharge2;

                dataGridView2.Rows[11].Cells[0].Value = "FINISHED";
            }

            //Modelo do DataGrid2
            if (modeloPassando == "model_quenteFrio")

            {
                dataGridView1.Rows[0].Cells[0].Value = "DISCHARGE";
                dataGridView1.Rows[0].Cells[2].Value = limits[0] + " a " + v_discharge;

                dataGridView1.Rows[1].Cells[0].Value = "SHORT AC";
                dataGridView1.Rows[1].Cells[2].Value = limits[1] + " a " + v_short_ac_max;

                dataGridView1.Rows[2].Cells[0].Value = "VOLTAGE 3.3v";
                dataGridView1.Rows[2].Cells[2].Value = limits[2] + " a " + v_voltage_33_33;

                dataGridView1.Rows[3].Cells[0].Value = "VOLTAGE 12v e 15v";
                dataGridView1.Rows[3].Cells[2].Value = limits[3] + " a " + v_voltage_12_15;

                dataGridView1.Rows[4].Cells[0].Value = "CURRENT AC";
                dataGridView1.Rows[4].Cells[2].Value = limits[4] + " a " + v_current_ac_max + " (Amp)";

                dataGridView1.Rows[5].Cells[0].Value = "TEMPERATURE 1";
                dataGridView1.Rows[5].Cells[2].Value = limits[5] + " a " + v_temperature1;

                dataGridView1.Rows[6].Cells[0].Value = "TEMPERATURE 2";
                dataGridView1.Rows[6].Cells[2].Value = limits[6] + " a " + v_temperature2;

                dataGridView1.Rows[7].Cells[0].Value = "TEMPERATURE 3";
                dataGridView1.Rows[7].Cells[2].Value = limits[7] + " a " + v_temperature3;

                dataGridView1.Rows[8].Cells[0].Value = "MOTOR";
                dataGridView1.Rows[8].Cells[2].Value = limits[8] + " a " + v_motor;

                dataGridView1.Rows[9].Cells[0].Value = "CONDENSER";
                dataGridView1.Rows[9].Cells[2].Value = limits[9] + " a " + v_condensador;

                dataGridView1.Rows[10].Cells[0].Value = "INVERTER";
                dataGridView1.Rows[10].Cells[2].Value = limits[10] + " a " + v_inverter;

                dataGridView1.Rows[11].Cells[0].Value = "CHECK OUT";
                dataGridView1.Rows[11].Cells[2].Value = limits[11] + " a " + v_discharge2;

                dataGridView1.Rows[12].Cells[0].Value = "FINISHED";
            }

            if (modeloPassando2 == "model_quenteFrio")
            {

                dataGridView2.Rows[0].Cells[0].Value = "DISCHARGE";
                dataGridView2.Rows[0].Cells[2].Value = limits[0] + " a " + v_discharge;

                dataGridView2.Rows[1].Cells[0].Value = "SHORT AC";
                dataGridView2.Rows[1].Cells[2].Value = limits[1] + " a " + v_short_ac_max;

                dataGridView2.Rows[2].Cells[0].Value = "VOLTAGE 3.3v";
                dataGridView2.Rows[2].Cells[2].Value = limits[2] + " a " + v_voltage_33_33;

                dataGridView2.Rows[3].Cells[0].Value = "VOLTAGE 12v e 15v";
                dataGridView2.Rows[3].Cells[2].Value = limits[3] + " a " + v_voltage_12_15;

                dataGridView2.Rows[4].Cells[0].Value = "CURRENT AC";
                dataGridView2.Rows[4].Cells[2].Value = limits[4] + " a " + v_current_ac_max + " (Amp)";

                dataGridView2.Rows[5].Cells[0].Value = "TEMPERATURE 1";
                dataGridView2.Rows[5].Cells[2].Value = limits[5] + " a " + v_temperature1;

                dataGridView2.Rows[6].Cells[0].Value = "TEMPERATURE 2";
                dataGridView2.Rows[6].Cells[2].Value = limits[6] + " a " + v_temperature2;

                dataGridView2.Rows[7].Cells[0].Value = "TEMPERATURE 3";
                dataGridView2.Rows[7].Cells[2].Value = limits[7] + " a " + v_temperature3;

                dataGridView2.Rows[8].Cells[0].Value = "MOTOR";
                dataGridView2.Rows[8].Cells[2].Value = limits[8] + " a " + v_motor;

                dataGridView2.Rows[9].Cells[0].Value = "CONDENSER";
                dataGridView2.Rows[9].Cells[2].Value = limits[9] + " a " + v_condensador;

                dataGridView2.Rows[10].Cells[0].Value = "INVERTER";
                dataGridView2.Rows[10].Cells[2].Value = limits[10] + " a " + v_inverter;

                dataGridView2.Rows[11].Cells[0].Value = "CHECK OUT";
                dataGridView2.Rows[11].Cells[2].Value = limits[11] + " a " + v_discharge2;

                dataGridView2.Rows[12].Cells[0].Value = "FINISHED";
            }

            //INACABADO AINDA

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 10; j++) // Corrigido para j++
                {
                    TESTS2[i, j] = "NULL";
                }
            }


            defineLimitsMinMax();
            textBox4.Text = UserName;
            richTextBox1.Enabled = false;
            button1.Focus();

            try
            {
                if (!serialPort1.IsOpen)
                {
                    serialPort1.Open();
                    timer3.Start();
                }
            }
            catch (Exception)
            {
                fail = true;
                
                textBox7.BackColor = Color.Red;
                textBox7.Text = "SERIAL POR FAIL";
               
                textBox7.Visible = true;
               
            }





            for (int i = 0; i < 3; i++)
            {

                for (int j = 0; j < 10; j++)
                {

                    TESTS[i, j] = "NULL";
                }
            }

            defineLimitsMinMax();
            textBox4.Text = UserName;
            richTextBox1.Enabled = false;
            button1.Focus();

            try
            {
                if (!serialPort1.IsOpen)
                {
                    serialPort1.Open();
                    timer3.Start();
                }
            }
            catch (Exception)
            {
                fail = true;
                textBox6.BackColor = Color.Red;
               
                textBox6.Text = "SERIAL PORT FAIL";
               
                textBox6.Visible = true;
            }



            if (modeloPassando == "model_quenteFrio" && modeloPassando2 == "model_quenteFrio")
            {

                this.Text = "Software - Version: " + versao.ToString() + " - Modelo | Quente-Frio | Simultâneo";
            }

            if (modeloPassando == "model_padrao" && modeloPassando == "model_padrao")

            {
                this.Text = "Software - Version: " + versao.ToString() + " - Modelo | Padrão | Simultâneo ";
            }

            if (modeloPassando == "model_quenteFrio" && modeloPassando2 == "model Padrao")

            {
                this.Text = "Software - Version: " + versao.ToString() + " - Modelo | Quente-Frio | Padrão";
            }

            if (modeloPassando == "model_padrao" && modeloPassando2 == "model_quenteFrio")
            {
                this.Text = "Software - Version: " + versao.ToString() + " - Modelo | Padrao | Quente-frio";
            }
        }





        private void delay(int ms)
        {
            DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(ms);
            while (DateTime.Now < dateTimeTarget)
            {
                Application.DoEvents();
            }
        }

        private void clearDataGrid1()
        {
            if (modeloPassando == "model_quenteFrio")
            {
                for (int i = 0; i < 13; i++)
                {
                    dataGridView1.Rows[i].Cells[1].Value = "";
                    dataGridView1.Rows[i].Cells[3].Value = "";
                    dataGridView1.Rows[i].Cells[4].Value = "";

                }
            }

            if (modeloPassando == "model_padrao")
            {
                for (int i = 0; i < 12; i++)
                {
                    dataGridView1.Rows[i].Cells[1].Value = "";
                    dataGridView1.Rows[i].Cells[3].Value = "";
                    dataGridView1.Rows[i].Cells[4].Value = "";

                }
            }
        }

        private void clearDataGrid2()
        {
            if (modeloPassando2 == "model_quenteFrio")
            {
                for (int i = 0; i < 13; i++)
                {
                    dataGridView2.Rows[i].Cells[1].Value = "";
                    dataGridView2.Rows[i].Cells[3].Value = "";
                    dataGridView2.Rows[i].Cells[4].Value = "";
                }
            }

            if (modeloPassando2 == "model_padrao")
            {
                for (int i = 0; i < 12; i++)
                {
                    dataGridView2.Rows[i].Cells[1].Value = "";
                    dataGridView2.Rows[i].Cells[3].Value = "";
                    dataGridView2.Rows[i].Cells[4].Value = "";
                }
            }
        }


        private void initTest1()
        {
            clearDataGrid1();
            clearDataGrid2();
            timer1.Stop();
            rxData = "";
            rxData2 = "";
            getSerial = true;
            getSerial2 = true;
            inicio = false;
            fail = false;
            indexTest2 = 0;
            indexTest = 0;
            dataGridView1.Rows[indexTest].Selected = false;
            dataGridView2.Rows[indexTest2].Selected = false;
            richTextBox2.Text = "";
            textBox1.Text = "";
            textBox2.Text = "";

            inicioTeste();
        }

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public void ConnectServer(Int32 port, string ips)
        {
            IPAddress host = IPAddress.Parse(ips);
            IPEndPoint ipendpoint = new IPEndPoint(host, port);
            if (!socket.Connected)
            {
                try
                {
                    socket.Connect(ipendpoint);
                    this.Text += " (Connected)";
                }
                catch (SocketException e)
                {
                    this.Text += " (Offline)";
                    MessageBox.Show(e.Message);
                    socket.Close();
                    return;
                }
            }
            else
            {
                socket.Close();
                this.Text += " ( Offline)";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            initTest1();
        }


        //definir ranges
        private void defineLimitsMinMax()
        {
            //DISCHARGE 1
            limitsMin[0] = limits[0];
            limitsMax[0] = v_discharge;

            //SHORT AC
            limitsMin[1] = limits[1];
            limitsMax[1] = v_short_ac_max;

            //VOLTAGE 3.3			
            limitsMin[2] = limits[2];
            limitsMax[2] = v_voltage_33_33;

            //VOLTAGE 12 e 15
            limitsMin[3] = limits[3];
            limitsMax[3] = v_voltage_12_15;

            //CURRENT AC
            limitsMin[4] = limits[4];
            limitsMax[4] = v_current_ac_max;

            //TEMPERATURE1
            limitsMin[5] = limits[5];
            limitsMax[5] = v_temperature1;

            //TEMPERATURE2
            limitsMin[6] = limits[6];
            limitsMax[6] = v_temperature2;

            //TEMPERATURE3
            limitsMin[7] = limits[7];
            limitsMax[7] = v_temperature3;

            //MOTOR
            limitsMin[8] = limits[8];
            limitsMax[8] = v_motor;

            //CONDENSADOR
            limitsMin[9] = limits[9];
            limitsMax[9] = v_condensador;

            //INVERTER  
            limitsMin[10] = limits[10];
            limitsMax[10] = v_inverter;

            //DISCHARGE 2
            limitsMin[11] = limits[11];
            limitsMax[11] = v_discharge2;
        }

        // bool statusPassOrFail = false;

        private bool checkTest(int position, double value, string valor)
        {
            bool statusPassOrFail;
            value = value / 100;

            if (value >= limitsMin[position] && value <= limitsMax[position])
            {
                statusPassOrFail = true;
            }
            else
            {
                statusPassOrFail = false;
            }

            if (position == 5)
            {
                // if (led1 == valor)
                // {
                //     statusPassOrFail = true;
                // }
            }

            if (position == 8)
            {
                // if (led2 == valor)
                // {
                //     statusPassOrFail = true;
                // }
            }

            return statusPassOrFail;
        }


        // int valorRecebido = 0;
        bool resultadoDoCheck = false;


        private bool checkTest2(int position, double value, string valor)
        {
            bool statusPassOrFail;
            value = value / 100;

            if (value >= limitsMin[position] && value <= limitsMax[position])
            {
                statusPassOrFail = true;
            }
            else
            {
                statusPassOrFail = false;
            }

            if (position == 5)
            {
                // if (led1 == valor)
                // {
                //     statusPassOrFail = true;
                // }
            }

            if (position == 8)
            {
                // if (led2 == valor)
                // {
                //     statusPassOrFail = true;
                // }
            }

            return statusPassOrFail;
        }

        bool resultadoDoCheck2 = false;


        private bool TesteGrid1 = true; // Controle para dataGridView1
        private bool TesteGrid2 = true; // Controle para dataGridView2

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (fail == false)
            {
                if (inicio == false)
                {
                    //inicia temporizador de teste
                    initTimer();
                    inicio = true;
                }

                if (TesteGrid1 && getSerial && getSerial2)
                {
                    TEST[0] = true;
                    TEST2[0] = true;

                    serialPort1.Write("0");

                    getSerial = false;
                    getSerial2 = false;



                    
                    
                    {
                        if (naoFazTeste1 != "NaoFazTeste")
                        {
                            textBox6.Visible = true;
                            textBox6.Text = "TESTING...";
                            textBox6.BackColor = Color.Yellow;
                            Console.WriteLine(rxData);
                        }
                    }
                   

                    if (naoFazteste2 != "NaoFazTeste")
                    {
                        textBox7.Visible = true;
                        textBox7.Text = "TESTING...";
                        textBox7.BackColor = Color.Yellow;
                        Console.WriteLine(rxData2);
                    }

                    

                }

                // Verificando e atualizando o primeiro DataGridView
                if (TEST[indexTest])
                {
                    if (naoFazTeste1 != "NaoFazTeste")
                    {
                        dataGridView1.Rows[indexTest].Selected = true;
                        dataGridView1.Rows[indexTest].Cells[4].Value = "TESTING...";
                        TEST[indexTest] = false;
                        RES_TEST[indexTest] = true;
                    }
                }

                // Verificando e atualizando o segundo DataGridView
                if (TEST2[indexTest2])
                {
                    if (naoFazteste2 != "NaoFazTeste")
                    {
                        dataGridView2.Rows[indexTest2].Selected = true;
                        dataGridView2.Rows[indexTest2].Cells[4].Value = "TESTING...";
                        TEST2[indexTest2] = false;
                        RES_TEST2[indexTest2] = true;
                    }
                }
            
        
   

                if (RES_TEST[indexTest])
                {
                    textBox2.Text = rxData;
                    if (textBox2.Text.Contains("X"))
                    {
                        string[] dados = textBox2.Text.Split(';');
                       

                        try
                        {
                            if (indexTest == 3)
                            {
                                // temp1Sensor = double.Parse(dados[1]);
                                // temp2Sensor = double.Parse(dados[2]);
                            }
                        }
                        catch (Exception)
                        {

                            if (!msgDeErro)
                            {
                                resultadoDoCheck = false;
                            }
                        }

                        if (modeloPassando == "model_quenteFrio")
                        {
                            try
                            {
                                resultadoDoCheck = checkTest(teste127_220[indexTest], double.Parse(dados[2]), dados[2]);
                            }
                            catch (Exception)
                            {

                                Console.WriteLine("Eroo");
                            }
                        }

                        if (modeloPassando == "model_padrao")
                        {
                            resultadoDoCheck = checkTest(teste127_220_oduPadrao[indexTest], double.Parse(dados[2]), dados[2]);

                        }

                        if (resultadoDoCheck)
                        {
                            //TESTE CURTO CIRCUITO
                            if (indexTest == 0)
                            {
                                TESTS[1, indexTest] = "APROVADO";
                                temp_ini = double.Parse(dados[2]);
                                
                                temp_ini = temp_ini / 100;
                            }
                            else if (indexTest == 2 || indexTest == 8) { /*TESTS[1, indexTest] = dados[2];*/ }   //TEST EFICIENCIA 100 e 230AC
                            else
                            {
                               // TESTS[1, indexTest] = dados[1] + "VAC, " + dados[2] + "VDC, " + dados[3] + "A";
                            }

                            statusTest = "PASS";
                            dataGridView1.Rows[indexTest].Cells[1].Value = dados[1];
                            dataGridView1.Rows[indexTest].Cells[3].Value = dados[2];
                            dataGridView1.Rows[indexTest].Cells[4].Value = statusTest;

                            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Green;
                            dataGridView1.Rows[indexTest].Selected = false;
                            indexTest++;
                            TEST[indexTest] = true;
                            dados[0] = "";
                            RES_TEST[indexTest] = false;
                            rxData = "";
                            textBox2.Text = "";
                            if (statusTest == "FAIL")
                            {
                                statusTest = "FAIL";
                                dataGridView1.Rows[indexTest].Cells[4].Value = statusTest;
                                TesteGrid1 = false; // Para o teste de grid 1
                            }
                            

                            if (modeloPassando == "model_quenteFrio")
                            {
                                serialPort1.Write(teste127_220[indexTest].ToString());
                                Console.WriteLine("QuenteFrio: " + teste127_220[indexTest]);
                            }

                            if (modeloPassando == "model_padrao")
                            {
                                try
                                {
                                    serialPort1.Write(teste127_220_oduPadrao[indexTest].ToString());
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Erro " + ex.ToString());
                                    
                                }

                                
                                Console.WriteLine("Padrao: " + teste127_220_oduPadrao[indexTest]);
                            }

                            if (indexTest == 7)
                            {
                                button1.Text = "PRESSIONE A TECLA LIG/DESL DO CONTROLE (AO ACENDER O DISPLAY)";
                            }


                            if (indexTest == 9)
                            {
                                button1.Text = "Aguarde...";
                            }
                        }
                        else
                        {
                            //TESTE CURTO CIRCUITO
                            if (indexTest == 0) { TESTS[1, indexTest] = "FALHA"; }
                            else if (indexTest == 2 || indexTest == 8) { TESTS[1, indexTest] = dados[2]; }   //TEST EFICIENCIA 100 e 230AC
                            else
                            {
                                TESTS[1, indexTest] = dados[1] + "VAC, " + dados[2] + "VDC, " + dados[3] + "A";
                            }

                            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Red;
                            textBox6.Visible = true;
                            textBox6.Text = "FAIL";
                            textBox6.BackColor = Color.Red;
                            timer1.Stop();

                            inicio = false;

                            statusTest = "FAIL";
                            dataGridView1.Rows[indexTest].Cells[1].Value = dados[1];
                            dataGridView1.Rows[indexTest].Cells[3].Value = dados[2];
                            dataGridView1.Rows[indexTest].Cells[4].Value = statusTest;

                            button1.Text = "O Teste: " + " | " + indexTest.ToString() + " | " + "Falhou";

                            createLogs(1);

                            if (trackingTest)
                            {
                                if (_mode == "txt")
                                {
                                    createLogs(1);
                                }
                            }
                        }
                    }
                }


                if (RES_TEST2[indexTest2])
                {
                    textBox2.Text = rxData2;
                    if (textBox2.Text.Contains("X"))
                    {
                        string[] dados2 = textBox2.Text.Split(';');


                        try
                        {
                            if (indexTest2 == 3)
                            {
                                // temp1Sensor = double.Parse(dados[1]);
                                // temp2Sensor = double.Parse(dados[2]);
                            }
                        }
                        catch (Exception)
                        {

                            if (!msgDeErro)
                            {
                                resultadoDoCheck2 = false;
                            }
                        }
                        if (modeloPassando2 == "model_quenteFrio")
                        {
                            try
                            {
                                resultadoDoCheck2 = checkTest2(teste127_220_1[indexTest2], double.Parse(dados2[2]), dados2[2]);
                            }
                            catch (Exception)
                            {

                                Console.WriteLine("Error");
                            }
                        }

                        if (modeloPassando2 == "model_padrao")
                        {
                            resultadoDoCheck2 = checkTest2(teste127_220_oduPadrao_2[indexTest2], double.Parse(dados2[2]), dados2[2]);

                        }

                        if (resultadoDoCheck2)
                        {
                            {
                                //TESTE CURTO CIRCUITO
                                if (indexTest2 == 0)
                                {
                                    TESTS2[1, indexTest2] = "APROVADO";
                                    temp_ini2 = double.Parse(dados2[2]);

                                    temp_ini2 = temp_ini2 / 100;
                                }
                                else if (indexTest2 == 2 || indexTest2 == 8) { /*TESTS[1, indexTest] = dados[2];*/ }   //TEST EFICIENCIA 100 e 230AC
                                else
                                {
                                    // TESTS[1, indexTest] = dados[1] + "VAC, " + dados[2] + "VDC, " + dados[3] + "A";
                                }
                                statusTest = "PASS";
                                dataGridView2.Rows[indexTest2].Cells[1].Value = dados2[1];
                                dataGridView2.Rows[indexTest2].Cells[3].Value = dados2[2];
                                dataGridView2.Rows[indexTest2].Cells[4].Value = statusTest;

                                dataGridView2.DefaultCellStyle.SelectionForeColor = Color.Green;
                                dataGridView2.Rows[indexTest2].Selected = false;
                                indexTest2++;
                                TEST2[indexTest2] = true;
                                dados2[0] = "";

                                RES_TEST2[indexTest2] = false;
                                rxData2 = "";
                                textBox2.Text = "";

                                if (modeloPassando2 == "model_quenteFrio")
                                {
                                    serialPort1.Write(teste127_220_1[indexTest2].ToString());
                                    Console.WriteLine("QuenteFrio: " + teste127_220_1[indexTest2]);
                                }

                                if (modeloPassando2 == "model_padrao")
                                {
                                    try
                                    {
                                        serialPort1.Write(teste127_220_oduPadrao_2[indexTest2].ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Erro " + ex.ToString());

                                    }


                                    Console.WriteLine("Padrao: " + teste127_220_oduPadrao_2[indexTest2]);
                                }
                                if (indexTest2 == 7)
                                {
                                    button1.Text = "PRESSIONE A TECLA LIG/DESL DO CONTROLE (AO ACENDER O DISPLAY)";
                                }


                                if (indexTest2 == 9)
                                {
                                    button1.Text = "Aguarde...";
                                }

                                else
                                {
                                    //TESTE CURTO CIRCUITO
                                    if (indexTest2 == 0) { TESTS2[1, indexTest2] = "FALHA"; }
                                    else if (indexTest2 == 2 || indexTest2 == 8) { TESTS2[1, indexTest2] = dados2[2]; }   //TEST EFICIENCIA 100 e 230AC
                                    else
                                    {
                                        TESTS2[1, indexTest2] = dados2[1] + "VAC, " + dados2[2] + "VDC, " + dados2[3] + "A";
                                    }

                                    dataGridView2.DefaultCellStyle.SelectionForeColor = Color.Red;
                                    textBox7.Visible = true;
                                    textBox7.Text = "FAIL";
                                    textBox7.BackColor = Color.Red;
                                   

                                    inicio = false;

                                    statusTest = "FAIL";
                                    dataGridView2.Rows[indexTest2].Cells[1].Value = dados2[1];
                                    dataGridView2.Rows[indexTest2].Cells[3].Value = dados2[2];
                                    dataGridView2.Rows[indexTest2].Cells[4].Value = statusTest;

                                    button1.Text = "O Teste: " + " | " + indexTest2.ToString() + " | " + "Falhou";


                                }
                                createLogs2(1);

                                if (trackingTest)
                                {
                                    if (_mode == "txt")
                                    {
                                        createLogs2(1);

                                    }
                                }
                            }


                                    
                        }
                    }
                }







                if (!TesteGrid1 && !TesteGrid2 || !!TesteGrid1 )
                {
                    if (indexTest > linhasTotal - 1 && indexTest2 > linhasTotal - 1)
                    {
                        serialPort1.Write("12");
                        timer1.Stop();
                        endTimer();
                        textBox6.Visible = true;
                        textBox6.Text = "PASS - " + fim;
                        textBox6.BackColor = Color.GreenYellow;
                        FIM();
                        button1.Text = "Finalizado";
                    }
                }
            }
        }

                 

        int contaTeste = 1;

        














        public void parametro(String comando)
        {
            if (comando.Contains("cmdFIM"))
            {
                FIM();
            }

            else if (comando.Contains("cmdMSG"))
            {
                lineReteste = contador;
                Console.WriteLine("LineReteste: " + lineReteste);

                textBox1.ForeColor = Color.Yellow;
                textBox2.Text = "";
                int l = comando.Length;
                comando = comando.Substring(6, l - 6);

                if (!reteste)
                {
                    textBox1.Text = comando;
                }
                else
                {
                    textBox1.Text = comando + " [RETESTE] " + "[" + contaFalha + "]";
                }


                if (testPass == false)
                {
                    testPass = true;
                    richTextBox2.Text += contaTeste.ToString() + " - " + validaTeste + "\t[PASS]\r\n";
                    contaTeste++;
                }

                validaTeste = comando;
                testPass = false;

            }

            //LIGA PORTA DIGITAL----------------------------
            else if (comando.Substring(0, 2) == "LD")
            {
                textBox2.Text = "";
                serialPort1.WriteLine(comando);
            }

            //DESLIGA PORTA DIGITAL----------------------------
            else if (comando.Substring(0, 2) == "DD")
            {
                textBox2.Text = "";
                serialPort1.WriteLine(comando);
            }

            //calibração----------------------------
            else if (comando.Substring(0, 6) == "cmdCHW")
            {
                if (!serialPort2.IsOpen) { serialPort2.Open(); }
                textBox2.Text = "";
                serialPort2.WriteLine("CKW");

                DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(5000);
                while (DateTime.Now < dateTimeTarget)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                //MessageBox.Show("Aguarde...");
                if (textBox2.Text.Contains("FALHA"))
                {
                    textBox1.Text = "FALHA NO TESTE DE CALIBRAÇÃO";
                    falha();
                }
            }

            //ativa escritas na tela //send commander
            else if (comando.Substring(0, 6) == "cmdSCM")
            {
                if (!serialPort2.IsOpen) { serialPort2.Open(); }
                string send = comando.Substring(7, comando.Length - 7);
                serialPort2.WriteLine(send);
                //textBox2.Text = send;
            }

            //procura uma informação no testbox
            else if (comando.Substring(0, 6) == "cmdFND")
            {
                if (!serialPort2.IsOpen) { serialPort2.Open(); }
                string find = comando.Substring(7, comando.Length - 7);

                //MessageBox.Show("Aguarde...");
                if (!textBox2.Text.Contains(find))
                {
                    textBox1.Text = "FALHA NOS PARAMETROS";
                    falha();
                }
            }

            //ENVIA COMANDO AT----------------------------
            else if (comando.Substring(0, 5) == "cmdAT")
            {
                //cmdAT = comando;
                textBox2.Text = "";
                timer1.Stop();
                //cmdAT AT+LEDR,OK
                if (!serialPort2.IsOpen) { serialPort2.Open(); }

                string[] at = comando.Split(',');
                string msgAt = comando.Substring(6, at[0].Length - 6);
                serialPort2.WriteLine(msgAt);

                if (linux)
                {
                    textBox2.Text = serialPort2.ReadLine();
                }
                int tempo = 3000;
                if (comando.Contains("ANG"))
                {
                    tempo = 3000;
                }

                DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(tempo);
                while (DateTime.Now < dateTimeTarget)
                {
                    System.Windows.Forms.Application.DoEvents();
                }

                if (!textBox2.Text.Contains(at[1]))
                {
                    textBox1.Text = "FALHA NO TESTE:  " + comando.Substring(6, at[0].Length - 6);
                    falha();
                }
                else
                {
                    timer1.Start();
                }
            }

            //checa serial number
            else if (comando == "cmdCSN")
            {
                if (snum == "") { falha(); }

            }

            //PRINT SERIAL NUMBER
            else if (comando == "cmdPSN")
            {
                textBox2.Text = "";
                timer1.Stop();
                if (!serialPort2.IsOpen)
                {
                    serialPort2.Open();
                }
                serialPort2.WriteLine("AT+SNUM");

                DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(1000);
                while (DateTime.Now < dateTimeTarget)
                {
                    System.Windows.Forms.Application.DoEvents();
                }

                if (linux) { textBox2.Text = serialPort2.ReadLine(); }

                if (textBox2.Text.Contains("0x00"))
                {
                    string etq = textBox2.Text.Substring(textBox2.Text.Length - 6, 6);
                    textBox2.Text = etq;
                    printSn(etq);

                    MessageBox.Show(etq, "ETIQUETA");
                    timer1.Start();
                }
                else
                {
                    falha();
                }
            }


            else if (comando == "cmdCAD")
            {
                textBox2.Text = "";
                timer1.Stop();
                if (!serialPort2.IsOpen)
                {
                    serialPort2.Open();
                }
                serialPort2.WriteLine("AT+ADD");

                DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(1000);
                while (DateTime.Now < dateTimeTarget)
                {
                    System.Windows.Forms.Application.DoEvents();
                }

                //LINUX
                if (linux) { textBox2.Text = serialPort2.ReadLine(); }

                if (!textBox2.Text.Contains(addr))
                {
                    falha();
                }
                else
                {
                    timer1.Start();
                }

            }

            else if (comando == "cmdCNW")
            {
                textBox2.Text = "";
                timer1.Stop();
                if (!serialPort2.IsOpen)
                {
                    serialPort2.Open();
                }
                serialPort2.WriteLine("AT+NW");

                DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(1000);
                while (DateTime.Now < dateTimeTarget)
                {
                    System.Windows.Forms.Application.DoEvents();
                }

                //LINUX
                if (linux) { textBox2.Text = serialPort2.ReadLine(); }
                if (!textBox2.Text.Contains(channel))
                {
                    falha();
                }
                else
                {
                    timer1.Start();
                }

            }

            //*********************  MAC LOGIN *********************************

            else if (comando == "cmdMAC")
            {
                try
                {
                    timer1.Stop();

                    recebeFormSerial = "";
                    snum = "";
                    Serial sr = new Serial(this)
                    {
                        Text = "Etiqueta MAC"
                    };
                    sr.ShowDialog();
                    snum = recebeFormSerial;

                    mac = snum.Substring(0, 2) + "-";
                    mac += snum.Substring(2, 2) + "-";
                    mac += snum.Substring(4, 2) + "-";
                    mac += snum.Substring(6, 2) + "-";
                    mac += snum.Substring(8, 2) + "-";
                    mac += snum.Substring(10, 2);
                    timer1.Start();
                }
                catch (Exception)
                {
                    falha();
                }
            }
            //**************** READ MAC
            else if (comando == "cmdRMC")
            {
                timer1.Stop();
                cmdExec("getmac > mac.txt");
                Thread.Sleep(2000);

                StreamReader sr = new StreamReader("mac.txt");
                textBox2.Text = sr.ReadToEnd();
                sr.Close();

                if (!textBox2.Text.Contains(mac))
                {
                    falha();
                }
                else
                {
                    timer1.Start();
                }
            }

            //************** CHECK MEMORY ********************
            else if (comando.Contains("cmdMEM"))
            {
                string mem = comando.Substring(7, comando.Length - 7);
                textBox2.Text = mem;
                timer1.Stop();

                StreamReader sr = new StreamReader("system.txt");
                system = sr.ReadToEnd();
                sr.Close();

                if (!system.Contains(mem))
                {
                    falha();
                }
                else
                {
                    timer1.Start();
                }
            }

            //************** CHECK BLUETOOTH ********************
            else if (comando.Contains("cmdBTH"))
            {
                string bth = comando.Substring(7, comando.Length - 7);
                textBox2.Text = bth;
                timer1.Stop();

                StreamReader sr = new StreamReader("system.txt");
                system = sr.ReadToEnd();
                sr.Close();

                if (!system.Contains(bth))
                {
                    falha();
                }
                else
                {
                    timer1.Start();
                }
            }

            //************** CHECK BIOS ********************
            else if (comando.Contains("cmdBIOS"))
            {
                string bios = comando.Substring(8, comando.Length - 8);
                textBox2.Text = bios;
                timer1.Stop();

                StreamReader sr = new StreamReader("system.txt");
                system = sr.ReadToEnd();
                sr.Close();

                if (!system.Contains(bios))
                {
                    falha();
                }
                else
                {
                    timer1.Start();
                }
            }


            //************** CHECK PCI ********************
            else if (comando.Contains("cmdPCI"))
            {
                string PCI = comando.Substring(7, comando.Length - 7);
                textBox2.Text = PCI;
                timer1.Stop();

                StreamReader sr = new StreamReader("system.txt");
                system = sr.ReadToEnd();
                sr.Close();

                if (!system.Contains(PCI))
                {
                    falha();
                }
                else
                {
                    timer1.Start();
                }
            }

            //************** CHECK AUDIO ********************
            else if (comando.Contains("cmdAUDIO"))
            {
                timer1.Stop();
                string caminho = comando.Substring(9, comando.Length - 9);

                Process.Start("AUDIO.bat");
                Thread.Sleep(9000);



                if (File.Exists(caminho + "Fail.txt"))
                {
                    falha();
                }
                else
                {
                    timer1.Start();
                }
            }

            //************** CHECK ALERT ********************
            else if (comando.Contains("cmdALT"))
            {
                timer1.Stop();
                ALERT alert = new ALERT();
                alert.ShowDialog();
                timer1.Start();
            }

            //************** CHECK ODD ********************
            else if (comando.Contains("cmdODD") || comando.Contains("cmdSSD"))
            {
                string ODD = comando.Substring(7, comando.Length - 7);
                textBox2.Text = ODD;
                timer1.Stop();

                StreamReader sr = new StreamReader("ODD.txt");
                system = sr.ReadToEnd();
                sr.Close();

                if (!system.Contains(ODD))
                {
                    falha();
                }
                else
                {
                    timer1.Start();
                }
            }

            //**************** ETHERNET ************************
            else if (comando.Contains("cmdETH"))
            {

                try
                {
                    timer1.Stop();
                    Process.Start("eth.bat");
                    Thread.Sleep(3000);
                    string ping = comando.Substring(7, comando.Length - 7);

                    if (File.Exists(ping + "fail.txt"))
                    {
                        falha();
                    }
                    else
                    {
                        timer1.Start();
                    }
                }
                catch (Exception)
                {

                    falha();
                }
            }

            //************** CHECK BEEP ********************
            else if (comando.Contains("cmdBEEP"))
            {
                timer1.Stop();
                Process.Start("Beep.exe");
                DialogResult result = MessageBox.Show("O som do BEEP foi reproduzido?", "TESTE BEEP", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    timer1.Start();
                }
                else
                {
                    falha();
                }
            }

            //************** CHECK UNIDADE ********************
            else if (comando.Contains("cmdUSB"))
            {
                string usb = comando.Substring(7, comando.Length - 7);
                textBox2.Text = usb;
                if (Directory.Exists(usb) || File.Exists(usb))
                {
                    textBox1.ForeColor = Color.GreenYellow;
                    textBox1.Text += " OK!";
                }
                else
                {
                    falha();
                }
            }

            //GRAVA SERIAL NUMBER 1
            else if (comando == "cmdGSN1")
            {
                textBox2.Text = "";

                recebeFormSerial = "";
                snum = "";
                Serial sr = new Serial(this);
                sr.Text = "SERIAL NUMBER 1";
                sr.ShowDialog();
                snum = recebeFormSerial;

                if (snum.Length < 3) { fail = true; }
                else
                {
                    timer1.Start();
                }
            }
            //GRAVA SERIAL NUMBER 2
            else if (comando == "cmdGSN2")
            {
                textBox2.Text = "";

                recebeFormSerial = "";
                snum2 = "";
                Serial sr = new Serial(this);
                sr.Text = "SERIAL NUMBER 2";
                sr.ShowDialog();
                snum2 = recebeFormSerial;

                if (snum2.Length < 3) { fail2 = true; }
                else
                {
                    //timer2.Start();
                }
            }
            //GRAVA CHANNEL
            else if (comando == "cmdCHN")
            {
                textBox2.Text = "";
                if (!serialPort2.IsOpen)
                {
                    serialPort2.Open();
                }

                timer1.Stop();
                recebeFormSerial = "";
                channel = "";
                Serial sr = new Serial(this);
                sr.Text = "CHANNEL";
                sr.ShowDialog();
                channel = recebeFormSerial;

                if (channel.Length > 0)
                {
                    int ch = int.Parse(channel);
                    channel = ch.ToString();

                    serialPort2.WriteLine("AT+NW=" + channel);

                    DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(1000);
                    while (DateTime.Now < dateTimeTarget)
                    {
                        Application.DoEvents();
                    }

                    //LINUX
                    if (linux) { textBox2.Text = serialPort2.ReadLine(); }

                    if (textBox2.Text.Contains(channel) || textBox2.Text.Contains("OK"))
                    {
                        timer1.Start();
                    }
                    else
                    {
                        textBox1.Text = "FALHA NA GRAVAÇÃO DO NW";
                        falha();
                    }
                }
                else
                {
                    textBox1.Text = "FALHA NA GRAVAÇÃO DO NW";
                    falha();
                }
            }
            //GRAVA ADDRESS
            else if (comando == "cmdADD")
            {
                textBox2.Text = "";
                if (!serialPort2.IsOpen)
                {
                    serialPort2.Open();
                }


                timer1.Stop();
                recebeFormSerial = "";
                addr = "";
                Serial sr = new Serial(this);
                sr.Text = "ADDRESS";
                sr.ShowDialog();

                addr = recebeFormSerial;
                if (addr.Length > 0)
                {

                    int ad = int.Parse(addr);
                    addr = ad.ToString();

                    serialPort2.WriteLine("AT+ADD=" + addr);

                    DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(1000);
                    while (DateTime.Now < dateTimeTarget)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }

                    //LINUX
                    if (linux) { textBox2.Text = serialPort2.ReadLine(); }

                    if (textBox2.Text.Contains("OK") || textBox2.Text.Contains(addr))
                    {
                        timer1.Start();
                    }
                    else
                    {
                        textBox1.Text = "FALHA NA GRAVAÇÃO DO ADDRESS";
                        falha();
                    }
                }
                else
                {
                    textBox1.Text = "FALHA NA GRAVAÇÃO DO ADDRESS";
                    falha();
                }
            }


            //PAUSA NO TESTE
            else if (comando.Contains("cmdDLY"))
            {
                timer1.Stop();
                try
                {
                    ms = int.Parse(comando.Substring(7, comando.Length - 7));
                    DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(ms);
                    while (DateTime.Now < dateTimeTarget)
                    {
                        Application.DoEvents();
                    }
                    timer1.Start();
                }
                catch
                {
                    textBox1.Text = "FALHA NO COMANDO cmdDLY";
                    falha();
                }

            }


            //LER PORTA DIGITAL  [cmdRDR A0 0.00 0.00----------------------------
            else if (comando.Contains("RA") || comando.Contains("RB") || comando.Contains("RD") || comando.Contains("cmdRNG"))
            {
                textBox2.Text = "";
                analogRead(comando);
            }
            //READ VOLTAGE [ V,0.00,0.00
            else if (comando.Contains("cmdRVL"))
            {
                comando = comando.Substring(7, comando.Length - 7);

                analogRead(comando);
            }

            else if (comando.Contains("cmdCMD"))
            {
                comando = comando.Substring(7, comando.Length - 7);
                comandoTest = comando;

                serialPort1.Write(comando);
            }


            else if (comando.Contains("cmdREAD"))
            {
                string busca = comando.Substring(7, comando.Length - 1);
                MessageBox.Show(busca);
            }

            else if (comando.Contains("cmdUSB"))
            {
                Process.Start("usb.bat");

            }


            else if (comando.Contains("cmdFWV"))
            {
                serialPort1.WriteLine("SHOW");
            }


            //Read a file txt e get value
            else if (comando.Contains("cmdFILE"))
            {
                StreamReader sr = new StreamReader("usb.txt");
                string read = sr.ReadToEnd();
                sr.Close();
                //MessageBox.Show(read);
                string busca = comando.Substring(7, comando.Length - 7);
                if (!read.Contains(busca))
                {
                    textBox1.Text = "FALHA NA USB " + busca;
                    falha();
                }
            }
            // testRead = false;     
        }

        public void Await(int milliseconds, string comando)
        {
            serialPort1.Write(comando);

            DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(milliseconds);
            while (DateTime.Now < dateTimeTarget)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        string msgRead = "";
        float v;

        private void analogRead(string comando)
        {
            if (!serialPort1.IsOpen) { serialPort1.Open(); }

            msgRead = comando;
            timer1.Stop();
            contaFalha++;
            textBox2.Text = "";
            str = comando.Split(',');
            float min = float.Parse(str[2]);
            float max = float.Parse(str[3]);

            serialPort1.WriteLine(str[1]);

            DateTime dateTimeTarget = DateTime.Now.AddMilliseconds(1000);
            while (DateTime.Now < dateTimeTarget)
            {
                System.Windows.Forms.Application.DoEvents();
            }

            //cmdRVL Tx, GAC,90,110
            //cmdRVL Tx, GVT,0.00,0.20
            //cmdRVL T5, GAP,0.00,0.08

            //LINUX
            if (linux) { textBox2.Text = serialPort1.ReadLine(); }

            try
            {
                /************   EXTRUCTURE FOR TESTS UNITS    ********************/
                if (str[1] == "TINP" || str[1] == "TINPF") { GAC = textBox2.Text; }
                if (str[1] == "VOUT" || str[1] == "TOL") { GVT = textBox2.Text; }
                if (str[1] == "OUTVPP") { GVT = textBox2.Text; }
                if (str[1] == "COUT" || str[1] == "COUT4F") { GAP = textBox2.Text; }
                if (str[1] == "GTR5") { GEF = textBox2.Text; }

                if (comandoTest == "TEST3") { test_100vac_4a = GAC + "Vac, " + GVT + " V, " + GAP + "amp"; }
                if (comandoTest == "TEST5") { test_efficiency = GEF + " %"; }
                if (comandoTest == "TOSC") { test_ripple = GVT + " Vpp"; }
                if (comandoTest == "TEST7" || comandoTest == "TESTF7") { test_overload = GAC + " Vac, " + GVT + " V, " + GAP + " A"; }
                if (comandoTest == "TEST8") { test_high_vac4a = GAC + " Vac, " + GVT + " V, " + GAP + " A"; }
                if (comandoTest == "TEST0") { test_high_vac0a = GAC + " Vac, " + GVT + " V, " + GAP + " A"; }
                if (comandoTest == "TEST9") { test_discharger = GAC + " Vac, " + GVT + " V"; }

                v = float.Parse("0" + textBox2.Text);
                Console.WriteLine(v.ToString());

                Console.WriteLine(comandoTest + " - " + test_100vac_4a);


                if (v >= min && v <= max)
                {
                    if (comandoTest == "TEST1")
                    {
                        // testShortCurt = "PASS"; 
                    }

                    if (str[0] == "OUT")
                    {
                        serialPort1.WriteLine(str[0]);
                    }
                    else
                    {
                        serialPort1.WriteLine(str[0] + "PASS");
                    }

                    //testRead = true;
                    timer1.Start();
                    contaFalha = 0;
                    reteste = false;
                }
                else
                {
                    if (contaFalha >= 5)
                    {
                        reteste = false;
                        falha();
                    }
                    else
                    {
                        reteste = true;
                        textBox2.Text = "";
                        contaFalha += 1;
                        contador = lineReteste - 1;
                        Console.WriteLine(contaFalha.ToString());
                        timer1.Start();
                    }
                }
            }
            catch (Exception)
            {
                contaFalha += 1;
                analogRead(msgRead);
            }
            string mm = "  [" + str[2] + " | " + textBox2.Text + " | " + str[3] + "] ";
            textBox1.Text = mm;

            if (contaFalha == 6)
            {
                falha();
            }
        }


        private void clearValuesDb(int position)
        {
            dbcmd.Parameters.Clear();
            for (int i = 0; i < 17; i++)
            {
                TESTS[position, i] = "NULL";
            }

        }


        private void clearValuesDb2 (int position)
        {
            dbcmd.Parameters.Clear();
            for(int i = 0;i < 17; i++)
            {
                TESTS2[position, i]="NULL";
            }
        }



        public void FIM()
        {
            try
            {
                timer1.Stop();
                testStatus = "PASS";
                inicio = false;

                if (trackingTest)
                {
                    if (_mode == "ACCDB")
                    {
                        //insertDataBase(1);
                        clearValuesDb(1);
                    }
                    if (_mode == "txt")
                    {
                        createLogs(1);
                    }
                }

                createLogs(1);

                textBox1.ForeColor = Color.GreenYellow;
                textBox1.Text = "FIM";
                richTextBox2.Text += contaTeste.ToString() + " - " + validaTeste + "\t[PASS]\r\n";
                testStatus = "FAIL";
                textBox1.Text = fim.ToString();

            }
            catch (Exception)
            {

                //MessageBox.Show(ex.ToString());
            }

        }

        string testStatus2 = "FAIL";


        public string msgFail = "";
        public void falha()
        {
            if (serialPort1.IsOpen)
            {
                Console.WriteLine("TURNOFF");
                serialPort1.WriteLine("CHAING");
                serialPort1.WriteLine("TURNOFF");

            }
            if (snum.Length > 2)
            {
                snum = "";
                endTimer();
            }
            serialPort2.Close();
            Thread.Sleep(200);
            textBox1.BackColor = Color.Red;
            timer1.Stop();
            msgFail = textBox1.Text;
            richTextBox2.Text += contaTeste.ToString() + " - " + validaTeste + "\t[FAIL]\r\n";
            FAIL fim = new FAIL(this);
        }

        public void inicioTeste()
        {
            try
            {
                contaTeste = 1;
                //contaFalhaTP = 0;
                if (!serialPort1.IsOpen)
                {
                    serialPort1.Open();
                }

                if (trackingTest)
                {
                    parametro("cmdGSN1");
                }
                else
                {
                    timer1.Start();

                }


            }
            catch (Exception)
            {
                fail = true;
                textBox6.BackColor = Color.Red;
                textBox6.Text = "SERIAL PORT FAIL";
                textBox6.Visible = true;
            }
        }

        public void inicioTeste2()
        {
            try
            {
                if (!serialPort2.IsOpen)
                {
                    serialPort2.Open();
                }

                if (trackingTest)
                {
                    parametro("cmdGSN2");
                }
                else
                {
                    //  timer2.Start();
                }
            }
            catch (Exception)
            {
                fail2 = true;
            }
        }

        private String _tempo;
        public String tempo
        {
            get { return _tempo; }
            set { _tempo = contador.ToString(); }
        }

        private void simulatorToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        string rxData;
        string rxData2;

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (!linux)
            {
                // Lê os dados recebidos
                string receivedData = serialPort1.ReadExisting();

                
                rxData = receivedData;
                rxData2 = receivedData; 


                this.Invoke(new EventHandler(dataReceived));
            }
        }

        private void dataReceived(object sender, EventArgs e)
        {
            textBox2.AppendText(rxData);
            textBox2.AppendText(rxData2);


        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[] lines = System.IO.File.ReadAllLines("teste.txt");

            foreach (string line in lines)
            {
                // Use a tab to indent each line of the file.
                if (line.Contains("RA00"))
                {
                    textBox1.Text = line;
                }
            }
        }

        //bool getSerial2;
        //private void timer2_Tick(object sender, EventArgs e)
        //{
        //    if (fail2 == false)
        //    {
        //        if (inicio2 == false)
        //        {
        //            //inicia temporizador de teste
        //            initTimer2();
        //            inicio2 = true;
        //        }

        //        if (getSerial2)
        //        {
        //            //parametro("cmdGSN");
        //            TEST2[0] = true;
        //            serialPort2.Write("0");
        //            getSerial2 = false;
        //        }

        //        if (TEST2[indexTest2])
        //        {
        //            TEST2[indexTest2] = false;
        //            RES_TEST2[indexTest2] = true;
        //        }

        //        if (RES_TEST2[indexTest2])
        //        {
        //            textBox8.Text = rxData2;
        //            if (textBox8.Text.Contains("X"))
        //            {
        //                string[] dados = textBox8.Text.Split(';');
        //                if (dados[0] == "1")
        //                {
        //                    // statusTest2 = "PASS";
        //                }
        //                else
        //                {
        //                    // statusTest2 = "FAIL";
        //                }

        //                RES_TEST2[indexTest2] = false;
        //                rxData2 = "";
        //                textBox8.Text = "";

        //                if (dados[0] == "1")
        //                {
        //                    //TESTE CURTO CIRCUITO
        //                    if (indexTest2 == 0) { TESTS[2, indexTest2] = "PASS"; }
        //                    else if (indexTest2 == 2 || indexTest2 == 8) { TESTS[2, indexTest2] = dados[2]; }   //TEST EFICIENCIA 100 e 230AC
        //                    else
        //                    {
        //                        TESTS[2, indexTest2] = dados[1] + "VAC, " + dados[2] + "VDC, " + dados[3] + "A";
        //                    }

        //                    indexTest2++;
        //                    TEST2[indexTest2] = true;
        //                    dados[0] = "";
        //                    serialPort2.Write(indexTest2.ToString());
        //                }
        //                else
        //                {
        //                    //TESTE CURTO CIRCUITO
        //                    if (indexTest2 == 0) { TESTS[2, indexTest] = "FAIL"; }
        //                    else if (indexTest2 == 2 || indexTest2 == 8) { TESTS[2, indexTest2] = dados[2]; }   //TEST EFICIENCIA 100 e 230AC
        //                    else
        //                    {
        //                        TESTS[2, indexTest2] = dados[1] + "VAC, " + dados[2] + "VDC, " + dados[3] + "A";
        //                    }

        //                    //timer2.Stop();
        //                    inicio2 = false;

        //                    if (trackingTest)
        //                    {
        //                        // insertDataBase(2);
        //                        clearValuesDb(2);
        //                    }
        //                }
        //            }
        //        }

        //        if (indexTest2 > 16)
        //        {
        //            // FIM2();
        //        }
        //    }
        //}

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (Directory.Exists("/home/"))
            {
                string[] lines = System.IO.File.ReadAllLines(caminho);
                string local = "";
                foreach (string line in lines)
                {
                    // Use a tab to indent each line of the file.
                    string[] str = line.Split('=');
                    if (str[0] == "script") { local = str[1]; }
                }

                StreamWriter sw = new StreamWriter(local);
                sw.WriteLine(richTextBox1.Text);
                sw.Close();

                MessageBox.Show("Salvo com sucesso!!");
            }
            else
            {
                try
                {
                    StreamWriter sw = new StreamWriter("teste.txt");
                    sw.Write(richTextBox1.Text);
                    sw.Close();
                    checkBox1.Checked = false;
                    MessageBox.Show("DATA SAVED SUCESSFULLY!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (linux)
            {
                Process.Start(System.Windows.Forms.Application.StartupPath.ToString() + "/readme.txt");
            }
            else
            {
                Process.Start(System.Windows.Forms.Application.StartupPath.ToString() + "\\readme.txt");
            }
        }



        private void button3_Click(object sender, EventArgs e)
        {
            BaseBarcode barcode = BarcodeFactory.GetBarcode(Symbology.Code128);
            barcode.Number = textBox1.Text;

            barcode.ForeColor = Color.Black;

            Bitmap bitmap = barcode.Render();
            picBarcode.Image = bitmap;

            rect = new System.Drawing.Rectangle(0, 40, picBarcode.Image.Width - 2, picBarcode.Image.Height - 40);
            cropped = bitmap.Clone(rect, bitmap.PixelFormat);
            picBarcode.Image = cropped;

            // You can also save it to file:

            barcode.Save("D:\\barcode.gif", ImageType.Gif);
        }

        public void imprimir(object o, PrintPageEventArgs e)
        {
            System.Drawing.Image i = picBarcode.Image;
            //local de impressão 50 x, 50 y.
            e.Graphics.DrawImage(i, 70, 50);
        }

        private void printSn(string str)
        {
            BaseBarcode barcode = BarcodeFactory.GetBarcode(Symbology.Code128);
            barcode.Number = textBox2.Text;

            barcode.ForeColor = Color.Black;

            Bitmap bitmap = barcode.Render();
            picBarcode.Image = bitmap;

            rect = new System.Drawing.Rectangle(0, 40, picBarcode.Image.Width - 2, picBarcode.Image.Height - 40);
            cropped = bitmap.Clone(rect, bitmap.PixelFormat);
            picBarcode.Image = cropped;
        }


        private void pd_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs ev)
        {
            CreateEan13();
            ean13.Scale = (float)Convert.ToDecimal("0,8");
            ean13.DrawEan13Barcode(ev.Graphics, new System.Drawing.Point(0, 0));
            // Add Code here to print other information.
            ev.Graphics.Dispose();
        }

        //private void serialPort2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
       // {
        //    rxData2 = serialPort2.ReadExisting();
        //    this.Invoke(new EventHandler(dataReceived2));
       //
       //}

        TimeSpan dtIni;
        private void initTimer()
        {
            dtIni = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        }

        TimeSpan dtIni2;
        private void initTimer2()
        {
            dtIni2 = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        }

        //private void initTest2()
        //{
        //    clearDataGrid2();
        //    //timer2.Stop();
        //    //rxData2 = "";
        //    //getSerial2 = true;
        //    inicio2 = false;
        //    fail2 = false;
        //    indexTest2 = 0;
        //    textBox8.Text = "";
        //    inicioTeste2();
        //}

        //private void button4_Click_1(object sender, EventArgs e)
        //{
        //    initTest2();
        //}

        private void timer4_Tick(object sender, EventArgs e)
        {
            if (textBox2.Text.Contains("EMERG OFF"))
            {
                indexTest = 0;
                instanciaEmerg = false;
                timer4.Stop();
                textBox2.Text = "";
                textBox6.Visible = false;
                indexTest = 0;
                clearDataGrid1();
            }
        }

        private void timer2_Tick_1(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D1)
            {
                initTest1();
            }

            if (e.KeyCode == Keys.D2)
            {
                //   initTest2();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

            if (Directory.Exists("/home"))
            {
                string[] lines = System.IO.File.ReadAllLines(caminho);

                foreach (string line in lines)
                {
                    string[] str = line.Split('=');
                    if (str[0] == "port") { serialPort1.PortName = str[1]; }
                    if (str[0] == "baudRate") { serialPort1.BaudRate = int.Parse(str[1]); }
                }
            }
            else
            {
                IniFile _myIni = new IniFile(Application.StartupPath.ToString() + "\\Setup.ini");

                serialPort1.PortName = _myIni.Read("com", "Port");
                serialPort1.BaudRate = int.Parse(_myIni.Read("baudRate", "Port"));
            }

            try
            {
                if (!serialPort1.IsOpen)
                {
                    serialPort1.Open();
                    timer3.Start();
                    textBox6.Visible = false;
                }
            }
            catch
            {

                MessageBox.Show("Check connections USB, close program and reload.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public TimeSpan fim;

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            textBox6.Enabled = false;
            

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            textBox6.Enabled = false;
        }

        private bool isPaused = false;

        private void button4_Click_4(object sender, EventArgs e)
        {
            if (naoFazTeste1 != "NaoFazTeste")
            {
                textBox6.Visible = true;
                textBox6.Text = "TESTING...";
                textBox6.BackColor = Color.Yellow;
                Console.WriteLine(rxData);
            }

        }



        private void timer2_Tick_2(object sender, EventArgs e)
        {






        }

        private void button4_Click_3(object sender, EventArgs e)
        {
            //
        }

        private void endTimer()
        {
            TimeSpan dtFim = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            fim = dtFim.Subtract(dtIni);
        }

        private void timer5_Tick(object sender, EventArgs e)
        {

        }

        int contaenvio = 0;
        bool sendDataServer = false;
        private void timer3_Tick(object sender, EventArgs e)
        {



            textBox2.Text = rxData2;
            textBox2.Text = rxData;
            if (textBox2.Text.Contains("start"))
            {
                textBox2.Text = "";
                initTest1();
                button1.Text = "Aguarde...";
            }

            if (textBox2.Text.Contains("EMERG ON"))
            {
                timer1.Stop();
                timer3.Stop();
                timer4.Start();
                textBox2.Text = "";
                instanciaEmerg = true;
                frmEmergencia emergencia = new frmEmergencia(this);
                emergencia.ShowDialog();
                timer3.Start();
            }
        }

        public void sendData(string data)
        {
            if (socket.Connected)
            {
                try
                {
                    socket.Send(Encoding.ASCII.GetBytes(data));
                }
                catch (SocketException e)
                {
                    MessageBox.Show(e.Message);
                    socket.Close();
                    return;
                }
            }
            else
            {
                MessageBox.Show("Dispositivo Offline", "Erro de comunicação", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen) { serialPort1.Close(); }
            Config cfg = new Config();
            cfg.ShowDialog();
        }

        private void button4_Click_2(object sender, EventArgs e)
        {
            sendData(textBox4.Text);
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        public TimeSpan fim2;
        private void endTimer2()
        {
            TimeSpan dtFim = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            fim2 = dtFim.Subtract(dtIni2);
        }

        public TimeSpan endCicle;
        private void endCicleTimer()
        {
            TimeSpan _endCicle = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            endCicle = _endCicle.Subtract(dtIni);

            //cicliTimerStr = endCicle.ToString();
            Console.WriteLine("Cicle Timer: " + endCicle);
        }

        private void button4_Click(object sender, EventArgs e)
        {


        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void simulatorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Config cf = new Config();
            cf.ShowDialog();
        }

        private void setupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setup st = new Setup();
            st.ShowDialog();

        }

        private void dataTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataTest dt = new dataTest();
            dt.ShowDialog();
        }

        //private void dataReceived2(object sender, EventArgs e)
        //{
        //    textBox8.AppendText(rxData2);
        //}

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                  new float[] {.3f, .3f, .3f, 0, 0},
                  new float[] {.59f, .59f, .59f, 0, 0},
                  new float[] {.11f, .11f, .11f, 0, 0},
                  new float[] {0, 0, 0, 1, 0},
                  new float[] {0, 0, 0, 0, 1}
                });

                //create some image attributes
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //set the color matrix attribute
                    attributes.SetColorMatrix(colorMatrix);

                    //draw the original image on the new image
                    //using the grayscale color matrix
                    g.DrawImage(original, new System.Drawing.Rectangle(0, 0, original.Width, original.Height),
                                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        string serialRead = "";
        private void createLogs(int testNumber)
        {
            try
            {
                //SE NÃO EXISTE DIRETORIO DATALOG, CRIA 1
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                //SE NÃO EXISTE DIRETORIO DATA DE HOJE, CRIA 1
                string namePath = "LOG_ODU";
                string msg = "";


                if (modeloPassando == "model_padrao")
                {
                    if (testNumber == 1)
                    {
                        msg = snum
                            + "DataTestx:" + DateTime.Now
                            + ";Timer:" + fim.ToString()
                            + ";Status:" + testStatus;

                        serialRead = snum;
                    }

                    msg += ";SHORT_AC[" + TESTS[testNumber, 1] + "]"
                           + ";VOLTAGE_3_3v[" + TESTS[testNumber, 2] + "]"
                           + ";VOLTAGE_12v_15v[" + TESTS[testNumber, 3] + "]"
                           + ";CURRENT_AC[" + TESTS[testNumber, 4] + "]"
                           + ";TEMPERATURE_1[" + TESTS[testNumber, 5] + "]"
                           + ";TEMPERATURE_2[" + TESTS[testNumber, 6] + "]"
                           + ";TEMPERATURE_3[" + TESTS[testNumber, 7] + "]"
                           + ";MOTOR[" + TESTS[testNumber, 8] + "]"
                           + ";CONDENSER[" + TESTS[testNumber, 9] + "]"
                           + ";CHECK_OUT[" + TESTS[testNumber, 10] + "]\n";
                }

                if (modeloPassando == "model_quenteFrio")
                {
                    if (testNumber == 1)
                    {
                        msg = snum
                            + "DataTestx:" + DateTime.Now
                            + ";Timer:" + fim.ToString()
                            + ";Status:" + testStatus;

                        serialRead = snum;
                    }

                    msg += ";SHORT_AC[" + TESTS[testNumber, 1] + "]"
                           + ";VOLTAGE_3_3v[" + TESTS[testNumber, 2] + "]"
                           + ";VOLTAGE_12v_15v[" + TESTS[testNumber, 3] + "]"
                           + ";CURRENT_AC[" + TESTS[testNumber, 4] + "]"
                           + ";TEMPERATURE_1[" + TESTS[testNumber, 5] + "]"
                           + ";TEMPERATURE_2[" + TESTS[testNumber, 6] + "]"
                           + ";TEMPERATURE_3[" + TESTS[testNumber, 7] + "]"
                           + ";MOTOR[" + TESTS[testNumber, 8] + "]"
                           + ";CONDENSER[" + TESTS[testNumber, 9] + "]"
                           + ";INVERTER[" + TESTS[testNumber, 10] + "]"
                           + ";CHECK_OUT[" + TESTS[testNumber, 11] + "]\n";
                }

                using (StreamWriter writer = new StreamWriter(directory + namePath + ".txt", true))
                {
                    writer.WriteLine(msg);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }

            


        }

        private void createLogs2(int testNumber)
        {
            try
            {
                //SE NÃO EXISTE DIRETORIO DATALOG, CRIA 1
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                //SE NÃO EXISTE DIRETORIO DATA DE HOJE, CRIA 1
                string namePath = "LOG_ODU_2";
                string msg = "";


                if (modeloPassando2 == "model_padrao")
                {
                    if (testNumber == 1)
                    {
                        msg = snum
                            + "DataTestx:" + DateTime.Now
                            + ";Timer:" + fim.ToString()
                            + ";Status:" + testStatus;

                        serialRead = snum;
                    }

                    msg += ";SHORT_AC[" + TESTS2[testNumber, 1] + "]"
                           + ";VOLTAGE_3_3v[" + TESTS2[testNumber, 2] + "]"
                           + ";VOLTAGE_12v_15v[" + TESTS2[testNumber, 3] + "]"
                           + ";CURRENT_AC[" + TESTS2[testNumber, 4] + "]"
                           + ";TEMPERATURE_1[" + TESTS2[testNumber, 5] + "]"
                           + ";TEMPERATURE_2[" + TESTS2[testNumber, 6] + "]"
                           + ";TEMPERATURE_3[" + TESTS2[testNumber, 7] + "]"
                           + ";MOTOR[" + TESTS2[testNumber, 8] + "]"
                           + ";CONDENSER[" + TESTS2[testNumber, 9] + "]"
                           + ";CHECK_OUT[" + TESTS2[testNumber, 10] + "]\n";
                }

                if (modeloPassando2 == "model_quenteFrio")
                {
                    if (testNumber == 1)
                    {
                        msg = snum
                            + "DataTestx:" + DateTime.Now
                            + ";Timer:" + fim.ToString()
                            + ";Status:" + testStatus;

                        serialRead = snum;
                    }

                    msg += ";SHORT_AC[" + TESTS2[testNumber, 1] + "]"
                           + ";VOLTAGE_3_3v[" + TESTS2[testNumber, 2] + "]"
                           + ";VOLTAGE_12v_15v[" + TESTS2[testNumber, 3] + "]"
                           + ";CURRENT_AC[" + TESTS2[testNumber, 4] + "]"
                           + ";TEMPERATURE_1[" + TESTS2[testNumber, 5] + "]"
                           + ";TEMPERATURE_2[" + TESTS2[testNumber, 6] + "]"
                           + ";TEMPERATURE_3[" + TESTS2[testNumber, 7] + "]"
                           + ";MOTOR[" + TESTS2[testNumber, 8] + "]"
                           + ";CONDENSER[" + TESTS2[testNumber, 9] + "]"
                           + ";INVERTER[" + TESTS2[testNumber, 10] + "]"
                           + ";CHECK_OUT[" + TESTS2[testNumber, 11] + "]\n";
                }

                using (StreamWriter writer = new StreamWriter(directory + namePath + ".txt", true))
                {
                    writer.WriteLine(msg);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }
        }
    }
}






        
