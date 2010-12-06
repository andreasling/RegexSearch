using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RegexSearchWin
{
    /// <summary>
    /// Interaction logic for DisplayFileWindow.xaml
    /// </summary>
    public partial class DisplayFileWindow : Window
    {
        public DisplayFileWindow()
        {
            InitializeComponent();
        }

        public DisplayFileWindow(string text, IEnumerable<HighLight> highLights)
            : this()
        {
            textBlock1.Text = text;
            textBlock1.TextEffects = new TextEffectCollection(
                from highLight in highLights
                select new TextEffect(
                    Transform.Identity, 
                    Brushes.Red, 
                    null, 
                    highLight.Start, 
                    highLight.Count)
                );
        }
    }
}
