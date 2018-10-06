using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Principal;
using System.Text;
using System.Threading;
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
using JaebeMusicStudio.Sound;
using JaebeMusicStudio.UI;

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Mixer : Page
    {
        SoundLineUI selectedLine = null;
        public Mixer()
        {
            InitializeComponent();
            showContent();
            Thread t2 = new Thread(delegate ()
            {
                while (true)
                {
                    Thread.Sleep(10);
                    Dispatcher?.Invoke(() =>
                    {
                        foreach (var x in SoundLinesList.Children)
                        {
                            (x as SoundLineUI)?.Refresh();
                            (x as LiveSoundLineUI)?.Refresh();
                        }
                    });
                }
            });
            t2.Start();
        }

        void showContent()
        {
            SoundLinesList.Children.RemoveRange(0, SoundLinesList.Children.Count - 2);
            int index = 0;
            foreach (var line in Sound.Project.current.lines)
            {
                project_lineAdded(index, line);
                index++;
            }

            var liveLinesList = Project.current.liveLines.getAvaibleInputs();
            foreach (var line in liveLinesList)
            {
                project_liveLineAdded(index, line);
                index++;
            }

        }

        private void project_lineAdded(int index, SoundLine line)
        {
            var lineUI = new SoundLineUI(line);
            SoundLinesList.Children.Insert(index, lineUI);
            lineUI.MouseDown += LineUI_MouseDown;
        }
        private void project_liveLineAdded(int index, LiveSoundLine line)
        {
            var lineUI = new LiveSoundLineUI(line);
            SoundLinesList.Children.Insert(index, lineUI);
        }

        private void LineUI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedLine != null)
            {
                selectedLine.Background = Brushes.White;
                selectedLine.Line.effectAdded -= Line_effectAdded;
                selectedLine.Line.effectRemoved -= Line_effectRemoved;
            }
            if (sender is SoundLineUI)
                selectedLine = sender as SoundLineUI;
            selectedLine.Background = new SolidColorBrush(Color.FromRgb(230, 230, 255));
            selectedLine.Line.effectAdded += Line_effectAdded;
            selectedLine.Line.effectRemoved += Line_effectRemoved; ;
            ShowEffect();
            ShowInput();
            LineTitle.Text = selectedLine.Line.Title;

            LineInput_AddSelect.Generate();
        }

        private void Line_effectRemoved(int index)
        {
            // var removed = EffectsList.Children[index];
            EffectsList.Children.RemoveAt(index);
        }

        private void Line_effectAdded(int index, Effect effect)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var ui = new EffectMini(effect);
                ui.WantDelete += Ui_WantDelete;
                EffectsList.Children.Insert(index, ui);
                ui.MouseDown += Ui_MouseDown;
            }));
        }
        private void Line_inputAdded(int index, SoundLineConnection input)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var ui = new SoundLineConnectionUI(input);
                InputsList.Children.Insert(index, ui);
            }));
        }

        private void Ui_WantDelete(EffectMini obj)
        {
            lock (selectedLine.Line)
            {
                selectedLine.Line.effects.Remove(obj.effect);
            }
        }

        private void Ui_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragDrop.DoDragDrop((sender as EffectMini), (sender as EffectMini).effect as Effect, DragDropEffects.Copy | DragDropEffects.Move);
        }

        void ShowEffect()
        {
            InputsList.Children.Clear();
            int index = 0;
            foreach (var input in selectedLine.Line.inputs)
            {
                Line_inputAdded(index, input);
                index++;
            };
        }
        void ShowInput()
        {
            EffectsList.Children.Clear();
            int index = 0;
            foreach (var effect in selectedLine.Line.effects)
            {
                Line_effectAdded(index, effect);
                index++;
            }
            var button = new ButtonPretty() { Text = "Dodaj" };
            button.Click += AddEffect_Click;
            EffectsList.Children.Add(button);
        }

        private void AddEffect_Click(object sender, RoutedEventArgs e)
        {
            EffectsListScroll.ContextMenu.IsOpen = true;
        }



        private void AddReverb_Click(object sender, RoutedEventArgs e)
        {
            if (selectedLine?.Line != null)
            {
                var effect = new Reverb();
                selectedLine.Line.AddEffect(effect);
            }
        }

        private void AddNonlinearDistortion_Click(object sender, RoutedEventArgs e)
        {
            if (selectedLine?.Line != null)
            {
                var effect = new NonlinearDistortion();
                selectedLine.Line.AddEffect(effect);
            }
        }

        private void AddFlanger_Click(object sender, RoutedEventArgs e)
        {
            if (selectedLine?.Line != null)
            {
                var effect = new Flanger();
                selectedLine.Line.AddEffect(effect);
            }
        }

        private void EffectsListScroll_OnDrop(object sender, DragEventArgs e)
        {
            var newEffect = e.Data.GetData(e.Data.GetFormats()[0]) as Effect;
            if ((e.Effects & DragDropEffects.Move) != 0)
            {
                // EffectsList.Children.Remove(e.Source as EffectMini);
                selectedLine.Line.RemoveEffect(newEffect);
            }
            int index = 0;//todo dorobić sprawdzanie

            // EffectsList.Children.Insert(index,e.Source as EffectMini);
            selectedLine.Line.AddEffect(index, newEffect);
        }

        private void AddSimpleFilter_Click(object sender, RoutedEventArgs e)
        {

            if (selectedLine?.Line != null)
            {
                var effect = new SimpleFilter();
                selectedLine.Line.AddEffect(effect);
            }
        }

        private void LineTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedLine?.Line != null)
            {
                selectedLine.Line.Title = LineTitle.Text;
            }
        }

        private void LineInput_AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedLine?.Line != null)
            {
                var connection = new SoundLineConnection(Project.current.lines.IndexOf(selectedLine.Line), LineInput_AddSelect.Selected, 1);
                connection.input.outputs.Add(connection);
                connection.output.inputs.Add(connection);
            }
        }

        private void AddLinButton_Click(object sender, RoutedEventArgs e)
        {
            Project.current.lines.Add(new SoundLine());
        }
    }
}
