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
            richTextBox1.Document.Blocks.Clear();
            richTextBox1.Document.Blocks.Add(new Paragraph(new Run(text.Replace("\r\n", "  "))));

            int i = 0;
            foreach (var highLight in highLights)
            {
                var start = richTextBox1.Document.ContentStart.GetPositionAtOffset(2 + highLight.Start);
                var stop = start.GetPositionAtOffset(highLight.Count);
                var range = new TextRange(start, stop);
                range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Pink);
                i += 10;
            }
        }
    }
}
