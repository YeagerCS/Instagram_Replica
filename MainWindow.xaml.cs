using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using MySql.Data.MySqlClient;
using System.Security.RightsManagement;
using System.Printing;
using System.Windows.Threading;

namespace Instagram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection conn = new Config().GetStandardConnection();
        private string ipAddress = "192.168.1.1";
        public MainWindow()
        {
            InitializeComponent();
            InitImages();
            this.ResizeMode = ResizeMode.NoResize;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public void InsertImage(string filename)
        {
            string query = "INSERT INTO image (likes, name) VALUES (@1, @2)";

            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.AddWithValue("@1", 0);
                command.Parameters.AddWithValue("@2", $@"https://{ipAddress}/public_html/{Path.GetFileName(filename)}");

                if(command.ExecuteNonQuery() > 0)
                {
                }
            }

        }

        public void InitImages()
        {
            panel.Children.Clear();
            string query = "SELECT * FROM image";
            using (MySqlCommand command = new MySqlCommand(query, conn))

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    StackPanel container = new StackPanel();
                    container.Orientation = Orientation.Vertical;

                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    WebClient client = new WebClient();
                    byte[] imageBytes = client.DownloadData(reader.GetString("name"));
                    MemoryStream memoryStream = new MemoryStream(imageBytes);
                    var imageSource = new BitmapImage();
                    imageSource.BeginInit();
                    imageSource.StreamSource = memoryStream;
                    imageSource.CacheOption = BitmapCacheOption.OnLoad;
                    imageSource.EndInit();

                    Image image = new Image();
                    image.Source = imageSource;
                    image.Width = 420;
                    image.Height = double.NaN;

                    StackPanel subContainer = new StackPanel();
                    subContainer.Orientation = Orientation.Horizontal;

                    Button button = new Button();
                    button.Content = "Like";
                    button.Margin = new Thickness(0, 0, 0, 10);
                    button.Width = 150;
                    button.Height = image.Height;
                    button.VerticalAlignment = VerticalAlignment.Top;
                    button.HorizontalAlignment = HorizontalAlignment.Left;
                    button.FontSize = 14;
                    button.FontWeight = FontWeights.Bold;
                    button.Style = getButtonStyle();

                    Label label = new Label();
                    label.Content = "Likes: " + reader.GetInt32(1);
                    label.FontFamily = new FontFamily("Arial Rounded MT Bold");
                    label.FontSize = 18;
                    label.Margin = new Thickness(170, -5, 0, 0);

                    subContainer.Children.Add(button);
                    subContainer.Children.Add(label);

                    button.Tag = reader.GetInt32(0);

                    container.Children.Add(image);
                    container.Children.Add(subContainer);
                    panel.Children.Add(container);

                    image.MouseLeftButtonDown += (sender, e) =>
                    {
                        Image clickedImage = (Image)sender;

                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "Image files|*.png;*.jpg;*.jpeg";

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            BitmapImage imageSource = (BitmapImage)clickedImage.Source;

                            using (WebClient webClient = new WebClient())
                            {
                                byte[] imageBytes = webClient.DownloadData(imageSource.UriSource.AbsoluteUri);

                                File.WriteAllBytes(saveFileDialog.FileName, imageBytes);
                            }
                        }
                    };

                    button.Click += Button_Click;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            int imageId = (int)button.Tag;
            UpdateLikes(imageId);
        }

        public Style getButtonStyle()
        {
            Style buttonStyle = new Style(typeof(Button));

            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.White));

            buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.DarkBlue));

            buttonStyle.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Black));

            buttonStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(2)));


            return buttonStyle;

        }

        public void UpdateLikes(int id)
        {
            string query = "UPDATE image SET likes = likes + 1 WHERE id = @1";

            using(MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.AddWithValue("@1", id);

                if(command.ExecuteNonQuery() > 0)
                {
                }
            }

            InitImages();
        }

        public void MoveUploadedImage(string source)
        {
            string sourcePath = source;
            if(source != null && source != "")
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential("username", "password");

                    string ftpServer = $"ftp://{ipAddress}";
                    string ftpFolder = "/";
                    string ftpFullPath = ftpServer + ftpFolder + Path.GetFileName(source);
                    client.UploadFile(ftpFullPath, "STOR", sourcePath);
                }

                InsertImage($"http://{ipAddress}/public_html/{Path.GetFileName(source)}");
            }

        }

        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select an Image to upload.";
            dialog.Filter = "Image files|*.png;*.jpg;*.jpeg";
            string path = null;

            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
            }
            
            MoveUploadedImage(path);
            InitImages();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            InitImages();
            timer.Stop();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
    }
}
