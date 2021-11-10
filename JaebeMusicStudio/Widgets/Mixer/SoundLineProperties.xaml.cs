using JaebeMusicStudio.Sound;
using JaebeMusicStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JaebeMusicStudio.Widgets.Mixer
{
    /// <summary>
    /// Interaction logic for SoundLineProperties.xaml
    /// </summary>
    public partial class SoundLineProperties : UserControl
    {
        public SoundLine Line { get; }
        public SoundLineProperties(SoundLine line)
        {
            Line = line;
            InitializeComponent();

            Line.effectAdded += Line_effectAdded;
            Line.effectRemoved += Line_effectRemoved;
            Line.inputAdded += Line_inputAdded;
            Line.inputRemoved += Line_inputRemoved;
            ShowEffect();
            ShowInput();
            LineTitle.Text = Line.Title;

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
        private void Line_inputRemoved(int index)
        {
            // var removed = EffectsList.Children[index];
            InputsList.Children.RemoveAt(index);
        }
        void ShowInput()
        {
            InputsList.Children.Clear();
            int index = 0;
            foreach (var input in Line.inputs)
            {
                Line_inputAdded(index, input);
                index++;
            };
        }
        void ShowEffect()
        {
            EffectsList.Children.Clear();
            int index = 0;
            foreach (var effect in Line.effects)
            {
                Line_effectAdded(index, effect);
                index++;
            }
        }

        private void AddReverb_Click(object sender, RoutedEventArgs e)
        {
            if (Line != null)
            {
                var effect = new Reverb();
                Line.AddEffect(effect);
            }
        }

        private void AddNonlinearDistortion_Click(object sender, RoutedEventArgs e)
        {
            if (Line != null)
            {
                var effect = new NonlinearDistortion();
                Line.AddEffect(effect);
            }
        }

        private void AddFlanger_Click(object sender, RoutedEventArgs e)
        {
            if (Line != null)
            {
                var effect = new Flanger();
                Line.AddEffect(effect);
            }
        }

        private void EffectsListScroll_OnDrop(object sender, DragEventArgs e)
        {
            var newEffect = e.Data.GetData(e.Data.GetFormats()[0]) as Effect;
            if ((e.Effects & DragDropEffects.Move) != 0)
            {
                // EffectsList.Children.Remove(e.Source as EffectMini);
                Line.RemoveEffect(newEffect);
            }
            int index = 0;//todo dorobić sprawdzanie

            // EffectsList.Children.Insert(index,e.Source as EffectMini);
            Line.AddEffect(index, newEffect);
        }

        private void AddSimpleFilter_Click(object sender, RoutedEventArgs e)
        {

            if (Line != null)
            {
                var effect = new SimpleFilter();
                Line.AddEffect(effect);
            }
        }

        private void LineTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Line != null)
            {
                Line.Title = LineTitle.Text;
            }
        }

        private void LineInput_AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (Line != null)
            {
                var connection = new SoundLineConnection(Project.current.lines.IndexOf(Line), LineInput_AddSelect.Selected, 1);
                connection.input.outputs.Add(connection);
                connection.output.AddInput(connection);
            }
        }



        private void Ui_WantDelete(EffectMini obj)
        {
            lock (Line)
            {
                Line.effects.Remove(obj.effect);
            }
        }

        private void Ui_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragDrop.DoDragDrop((sender as EffectMini), (sender as EffectMini).effect as Effect, DragDropEffects.Copy | DragDropEffects.Move);
        }
    }
}
