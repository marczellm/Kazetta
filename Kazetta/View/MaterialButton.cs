using System.Windows;
using System.Windows.Controls;

namespace Kazetta.View
{
    public class MaterialButton : Button
    {
        static MaterialButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MaterialButton), new FrameworkPropertyMetadata(typeof(MaterialButton)));
        }
    }
}
