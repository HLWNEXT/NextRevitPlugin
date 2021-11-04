using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RevitPlugin
{
    public interface IRevitPlugin
    {
        string GroupName { get; }
        string PanelName { get; }
        int PanelIndex { get; }
        int MenuIndex { get; }
        string Name { get; }
        string AliasName { get; }
        string ClassName { get; }
        string ModelName { get; }
        BitmapImage Image { get; }
    }
}
