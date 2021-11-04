using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RevitPlugin
{
    public static class PluginLoader
    {
        /// <summary>
        /// Load Add-on sets
        /// </summary>
        /// <param name="revitapp"></param>
        public static void LoadAllPlugins(UIControlledApplication revitapp)
        {
            List<string> allTabName = new List<string>();
            string[] files = Directory.GetFiles(Path.Combine(GetAssemblyPath()));
            int i = 0;
            foreach (string file in files)
            {
                string ext = file.Substring(file.LastIndexOf("."));
                if (ext != ".dll") continue;
                try
                {
                    Assembly tmp = Assembly.LoadFrom(file);
                    Type[] types = tmp.GetTypes();
                    SortedDictionary<int, SortedDictionary<int, IRevitPlugin>> pluginTypeDic = new SortedDictionary<int, SortedDictionary<int, IRevitPlugin>>();
                    foreach (Type t in types)
                    {
                        if (IsValidPlugin(t))
                        {
                            IRevitPlugin plugin = (IRevitPlugin)tmp.CreateInstance(t.FullName);
                            if (!pluginTypeDic.ContainsKey(plugin.PanelIndex))
                            {
                                SortedDictionary<int, IRevitPlugin> temp = new SortedDictionary<int, IRevitPlugin>();
                                temp.Add(plugin.MenuIndex, plugin);
                                pluginTypeDic.Add(plugin.PanelIndex, temp);

                            }
                            else
                            {
                                if (!pluginTypeDic[plugin.PanelIndex].ContainsKey(plugin.MenuIndex))
                                {
                                    pluginTypeDic[plugin.PanelIndex].Add(plugin.MenuIndex, plugin);
                                }
                            }
                        }
                    }

                    foreach (KeyValuePair<int, SortedDictionary<int, IRevitPlugin>> tabDic in pluginTypeDic)
                    {
                        SortedDictionary<int, IRevitPlugin> paneldic = tabDic.Value;
                        foreach (KeyValuePair<int, IRevitPlugin> pluginpair in paneldic)
                        {
                            Type t = pluginpair.Value.GetType();
                            if (IsValidPlugin(pluginpair.Value.GetType()))
                            {
                                IRevitPlugin plugin = (IRevitPlugin)tmp.CreateInstance(t.FullName);
                                if (allTabName.Contains(plugin.GroupName) == false)
                                {
                                    revitapp.CreateRibbonTab(plugin.GroupName);
                                    allTabName.Add(plugin.GroupName);
                                }
                                List<string> allPanelName = new List<string>();
                                List<RibbonPanel> panels = revitapp.GetRibbonPanels(plugin.GroupName);

                                panels.ForEach(x => allPanelName.Add(x.Name));
                                if (!allPanelName.Contains(plugin.PanelName))
                                {
                                    RibbonPanel panel = revitapp.CreateRibbonPanel(plugin.GroupName, plugin.PanelName);
                                    CreateButton(plugin, panel);
                                }
                                else
                                {
                                    List<RibbonPanel> panelList = revitapp.GetRibbonPanels(plugin.GroupName);
                                    RibbonPanel panel = null;
                                    panelList.ForEach(x =>
                                    {
                                        if (x.Name == plugin.PanelName)
                                        {
                                            panel = x;
                                        }
                                    });
                                    CreateButton(plugin, panel);
                                }
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }

        private static bool IsValidPlugin(Type t)
        {
            bool ret = false;
            Type[] interfaces = t.GetInterfaces();
            foreach (Type theInterface in interfaces)
            {
                if (theInterface.FullName == "RevitPlugin.IRevitPlugin")
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        public static BitmapImage GetBitmapImage(string imageName)
        {
            string strpath = GetAssemblyPath() + @"\Images\" + imageName;
            if (File.Exists(strpath))
                return new BitmapImage(new Uri(GetAssemblyPath() + @"\Images\" + imageName));
            return null;
        }

        /// <summary>
        /// Get the running path of the Assembly
        /// </summary>
        ///<returns></returns>
        private static string GetAssemblyPath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }


        private static void CreateButton(IRevitPlugin plugin, RibbonPanel panel)
        {
            PushButtonData data = new PushButtonData(plugin.Name, plugin.AliasName, plugin.GetType().Assembly.Location, plugin.ModelName);
            PushButton ps = panel.AddItem(data) as PushButton;
            ps.LargeImage = plugin.Image;
            ps.Image = plugin.Image;

        }
    }
}
