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

namespace EarTrainer
{
    /// <summary>
    /// Interaction logic for Gui.xaml
    /// </summary>
    public partial class Gui : Window
    {
        private const int DEFAULT_INDEX = 0;

        private readonly Model model;

        public Gui()
        {
            InitializeComponent();
            model = new Model();
            NoteCombo.ItemsSource = model.GetNoteNames();
            NoteCombo.SelectedIndex = DEFAULT_INDEX;
            IntervalCombo.ItemsSource = model.GetIntervalNames();
            IntervalCombo.SelectedIndex = DEFAULT_INDEX;
            Direction.ItemsSource = model.GetDirectionNames();
            Direction.SelectedIndex = DEFAULT_INDEX;
            model.ResultChanged += model_ResultChanged;
        }

        void model_ResultChanged(object sender, string e)
        {
            Result.Text = e;
        }

        private void NoteButton_Click(object sender, RoutedEventArgs e)
        {
            model.OnNote();
        }

        private void IntervalButton_Click(object sender, RoutedEventArgs e)
        {
            model.OnInterval();
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            model.OnRepeat();
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            model.OnShow();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            model.OnPlay(NoteCombo.Text, IntervalCombo.Text, Direction.Text);
        }
    }
}
