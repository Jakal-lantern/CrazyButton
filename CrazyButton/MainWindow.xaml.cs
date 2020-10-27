using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualBasic;

namespace CrazyButton
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Initilization
        System.Random rand = new Random();

        public int score = 0;
        public int mouseMultiplier = 10;
        public int currentMouseX;
        public int currentMouseY;

        public static string folderPath = @"C:/Users/Public/Documents/";
        public static string txtPath = @"C:/Users/Public/Documents/crazyButtonScores.txt";
        public static string userName = "";
        public static string highscoreTime = "";

        DispatcherTimer timer = new DispatcherTimer();
        Stopwatch stopWatch = new Stopwatch();
        public static string currentTime = string.Empty;


        public MainWindow()
        {
            InitializeComponent();
            scoreBlock.Text = "Score: " + score;

            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);

            WriteObjectsToMemory();
            highscoreText.Text = $"Highscore\n{userName} {highscoreTime}";
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (stopWatch.IsRunning)
            {
                TimeSpan ts = stopWatch.Elapsed;
                currentTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                timerLabel.Content = currentTime;
            }
        }

        private void gameButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameButton.Height <= 10)
            {
                stopWatch.Stop();
                timer.Stop();
                System.Windows.MessageBox.Show($"You won with a time of {currentTime}\nPress Enter");
                
                WriteObjectsToMemory();
                if (CompareTimes(highscoreTime, currentTime))
                {
                    userName = Microsoft.VisualBasic.Interaction.InputBox("Enter your name and hit ENTER", "New Highscore!", "Enter Name", -1, -1);
                }
                WriteToTxt();

                Environment.Exit(0);
            }


            // Start timer on first button press
            if (!stopWatch.IsRunning)
            {
                stopWatch.Start();
                timer.Start();
            }

            // Update and display score
            score++;
            scoreBlock.Text = "Score: " + score;

            // Change and apply new button variables
            ChangeButtonSize();
            ChangeButtonPosition();

            SporaticMouse();
            mouseMultiplier += 10;
        }

        // Changes size of button by dividing size by two
        public void ChangeButtonSize()
        {
            gameButton.Height = gameButton.Height / 1.5;
            gameButton.Width = gameButton.Width / 1.5;
        }

        // Changes position by changing margin using 'rand'
        public void ChangeButtonPosition()
        {
            int newPosX = rand.Next(10, 501);
            int newPosY = rand.Next(10, 501);

            gameButton.Margin = new Thickness(newPosX, newPosY, 0, 0);
        }

        public void SporaticMouse()
        {
            Thread mouseThread = new Thread(new ThreadStart(MouseThread));
            mouseThread.Start();
        }

        // Function to set cursor position
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        // Mouse movement function
        public void MouseThread()
        {
            int moveX = 0;
            int moveY = 0;

            while (true)
            {
                // Set random movement
                moveX = rand.Next(mouseMultiplier) - (mouseMultiplier / 2);
                moveY = rand.Next(mouseMultiplier) - (mouseMultiplier / 2);
                
                // Set current mouse value
                currentMouseX = GetMousePositionX();
                currentMouseY = GetMousePositionY();

                // Add current value to movement value
                moveX += currentMouseX;
                moveY += currentMouseY;

                // Set new movement value to current cursor position
                SetCursorPos(moveX, moveY);
                Thread.Sleep(50);
            }
        }

        // Get current mouse position
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static int GetMousePositionX()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return w32Mouse.X;
        }
        public static int GetMousePositionY()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return w32Mouse.Y;
        }

        // Fail safe
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.G)
            {
                Environment.Exit(0);
            }
        }

        // Write score to text file
        public static void WriteToTxt()
        {
            // Creates folder directory
            System.IO.Directory.CreateDirectory(folderPath);

            try
            {
                // Tests to see if file exists
                // If not, creates file
                if (!File.Exists(txtPath))
                {
                    using (StreamWriter sw = File.CreateText(txtPath)) ;
                }
                else
                {
                    using (StreamReader sr = File.OpenText(txtPath))
                    {
                        sr.Close();

                        StreamWriter sw = new StreamWriter(txtPath);
                        sw.WriteLine($"{userName},{currentTime}");
                        sw.Close();
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file was not found: {e}");
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine($"The directory was not found: {e}");
            }
            catch (IOException e)
            {
                Console.WriteLine($"The file could nto be opened: {e}");
            }
        }

        public static void WriteObjectsToMemory()
        {
            if (!File.Exists(txtPath))
            {
                using (StreamWriter sw = File.CreateText(txtPath)) ;
                System.IO.File.WriteAllText(txtPath, "Alex,99:99:99");
            }
            else
            {
                StreamReader sw = new StreamReader(txtPath);

                string line = sw.ReadLine();
                userName = line.Split(',')[0];
                highscoreTime = line.Split(',')[1];

                sw.Close();
            }
        }

        public static bool CompareTimes(string oldHighscore, string newScore)
        {
            // Initilization
            int OHminutes, OHseconds, OHmilliseconds, NSminutes, NSseconds, NSmilliseconds;

            OHminutes = Convert.ToInt32(oldHighscore.Split(':')[0]);
            OHseconds = Convert.ToInt32(oldHighscore.Split(':')[1]);
            OHmilliseconds = Convert.ToInt32(oldHighscore.Split(':')[2]);
            NSminutes = Convert.ToInt32(newScore.Split(':')[0]);
            NSseconds = Convert.ToInt32(newScore.Split(':')[1]);
            NSmilliseconds = Convert.ToInt32(newScore.Split(':')[2]);

            if (OHminutes > NSminutes)
            {
                return true;
            }
            else if (OHminutes > NSminutes && OHseconds > NSseconds)
            {
                return true;
            }
            else if (OHminutes > NSminutes && OHseconds > NSseconds && OHmilliseconds > NSmilliseconds)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /*
         * I spent way too long on a work around
         * because microsoft couldn't add that you needed
         * System.Drawing for the original CrazyPC Mouse
         * movement method to work. So I am keeping my
         * solution. Because I am stubborn and proud.
         * But mostly stubborn.
         */
    }
}
