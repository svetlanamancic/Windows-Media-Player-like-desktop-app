using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsMediaPlayer
{
    public partial class MainForm : Form
    {
        SeekForm seekForm;
        string path = "C:\\CVIDEO";
        string controlFile = "\\VIDEO.txt";
        string lastCommand = "";
        bool cmdRecognised = true;
        bool loaded = false;
        int playing;


        public MainForm()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            wmp.uiMode = "none";
            wmp.enableContextMenu = false;

            cm.Items.Add("Load");
            cm.Items.Add("Play");
            cm.Items.Add("Stop");
            cm.Items.Add("Pause");
            cm.Items.Add("Seek");
            cm.Items.Add("Snapshot");
            cm.Items.Add("Quit");

            timer.Interval = 300;
            timer.Start();
            timer.Tick += timer_Tick;

            seekForm = new SeekForm();
        }

        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP4 Files|*.mp4";
            openFileDialog.Title = "Open video file";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadVideo(openFileDialog.FileName);
            }
        }

        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Write, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return false;
        }

        private void LoadVideo(string videoPath)
        {
            if (!File.Exists(videoPath))
                return;
            loaded = true;
            wmp.URL = videoPath;
        }

        private void Play()
        {
            if (!loaded)
                return;

            playing = 1;
            wmp.Ctlcontrols.play();
            
        }

        private void Pause()
        {
            if (!loaded)
                return;
            
            playing = 0;
            wmp.Ctlcontrols.pause();
            
        }

        private void Stop()
        {
            if (!loaded)
                return;
            playing = 0;
            wmp.Ctlcontrols.stop();
        }

        private void Seek(double s)
        {
            if (!loaded)
                return;
            wmp.Ctlcontrols.currentPosition = s;
            wmp.Ctlcontrols.play();
            if (playing == 0)
            {
                wmp.Ctlcontrols.pause();
            }
        }

        private void Quit()
        {
            this.Close();
        }

        public void SnapShot()
        {
            if (!loaded)
                return;

            cm.Hide();
            Pause();
            this.Activate();
            this.Focus();
            SendKeys.Send("{PRTSC}");
            Image im = new Bitmap(Clipboard.GetImage());
            im.Save("C:\\CVIDEO\\VIMAGE.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            im.Dispose();
            Play();
        }

        private bool Command(string command, bool fromFile, string arg)
        {

            switch (command.ToLower())
            {
                case "load":
                    if (fromFile)
                    {
                        LoadVideo(arg);
                    }
                    else
                    {
                        OpenFile();
                    }
                    break;
                case "play":
                    Play();
                    break;
                case "stop":
                    Stop();
                    break;
                case "pause":
                    Pause();
                    break;
                case "snapshot":
                    SnapShot();
                    break;
                case "seek":
                    if (fromFile)
                    {
                        double s = Convert.ToDouble(arg) / 1000;
                        Seek(s);
                    }
                    else
                    {
                        seekForm.MaxTime = wmp.Ctlcontrols.currentItem.duration;
                        seekForm.SeekTime = wmp.Ctlcontrols.currentPosition;
                        if (seekForm.ShowDialog() == DialogResult.OK)
                            Seek(seekForm.SeekTime);
                    }
                    break;
                case "quit":
                    Quit();
                    break;
                default:
                    return false;
            }
            return true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {                
                string line;
                line = File.ReadLines(path+controlFile).First();

                if (String.Compare(line, lastCommand) != 0 && !String.IsNullOrEmpty(line))
                {
                    lastCommand = line;
                    string[] command = line.Split(' ');
                    if (String.Compare(command[0].ToLower(), "load") == 0)
                    {
                        command[1] = path + "\\" + command[1];
                    }
                    if(String.Compare(command[0].ToLower(),"load") == 0 || String.Compare(command[0].ToLower(), "seek") == 0)
                        cmdRecognised &= Command(command[0], true, command[1]);
                    else
                        cmdRecognised &= Command(command[0], true, null);

                }

            }
            catch
            {

            }
        }

        private void cm_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;
            Command(item.Text, false, null);
        }

        private void wmp_MouseUpEvent(object sender, AxWMPLib._WMPOCXEvents_MouseUpEvent e)
        {
            if (e.nButton == 2)
            {
                cm.Show(wmp, MousePosition);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(cmdRecognised)
            {
                try
                {
                    if (!File.Exists(path + controlFile))
                        return;

                    while (IsFileLocked(new FileInfo(path + controlFile)))
                    {

                    }

                    File.Delete(path + controlFile);
                }
                catch
                {

                }
            }
        }

        private void wmp_PlayStateChange_1(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (loaded && playing == 0)
                Pause();
        }
    }
}
