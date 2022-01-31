using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JaebeMusicStudio.Sound;
using JaebeMusicStudio.Sound.FileFormat;
using JaebeMusicStudio.UI;
using JaebeMusicStudio.Widgets;
using Microsoft.Win32;

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for FileExport.xaml
    /// </summary>
    public partial class FileExport : Window
    {
        public FileExport()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog() { Filter = "mp3|*.mp3|wave|*.wav" };
            dialog.ShowDialog();
            if (dialog.FileName == "")
                return;

            var file = new FileInfo(dialog.FileName);
            var start = DateTime.Now;
            var rendering = new Rendering() { renderingStart = 0, renderingLength = Project.current.length, project = Project.current, type = RenderngType.block, frequency = int.Parse((frequency.SelectedValue as ComboBoxItem)?.Content.ToString() ?? "48000") };
            var sound = await rendering.project.Render(rendering);
            var end = DateTime.Now;
            SaveFileEnd(sound, file);
            rendering.project.Clear(rendering);
            MessageBox.Show((end - start).TotalSeconds.ToString());
        }
        public void SaveFileEnd(float[,] data, FileInfo file)
        {

            var str = file.OpenWrite();
            if (file.Extension == "mp3")
            {
                // format = new Mp3();
            }
            else
            {
                // format = new Wave();
            }
            var format = new Wave();
            format.Write(str, data);
            str.Close();
        }
    }
}
