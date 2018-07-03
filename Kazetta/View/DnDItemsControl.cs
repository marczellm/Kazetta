using System.Windows;
using System.Windows.Controls;

namespace Kazetta.View
{
    public class DnDItemsControl : HeaderedItemsControl
    {
        static DnDItemsControl()
        {
            // Metadata needs to be overriden in static constructor to indicate that the style is declared under Themes/Generic.xaml.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DnDItemsControl), new FrameworkPropertyMetadata(typeof(DnDItemsControl)));
        }
        public bool ColorUjoncs { get; set; } = false;
        public static readonly DependencyProperty ColorUjoncsProperty =
            DependencyProperty.Register("ColorUjoncs", typeof(bool), typeof(DnDItemsControl));

        public bool ColorLeaders { get; set; } = false;
        public static readonly DependencyProperty ColorLeadersProperty =
            DependencyProperty.Register("ColorLeaders", typeof(bool), typeof(DnDItemsControl));

        public bool ColorKiscsoports { get; set; } = false;
        public static readonly DependencyProperty ColorKiscsoportsProperty =
            DependencyProperty.Register("ColorKiscsoports", typeof(bool), typeof(DnDItemsControl));

        public bool VisualizeConflicts { get; set; } = false;
        public static readonly DependencyProperty VisualizeConflictsProperty =
            DependencyProperty.Register("VisualizeConflicts", typeof(bool), typeof(DnDItemsControl));        

        public bool Pinnable { get; set; } = false;
        public static readonly DependencyProperty PinnableProperty =
            DependencyProperty.Register("Pinnable", typeof(bool), typeof(DnDItemsControl));
    }
}
