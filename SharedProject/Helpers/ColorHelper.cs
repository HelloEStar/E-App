using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SharedProject
{
    /// <summary>
    /// 颜色助手
    /// </summary>
    public class ColorHelper
    {
        /// <summary>
        /// 创建颜色
        /// </summary>
        /// <param name="text">ARGB色值，以点号分隔，0-255</param>
        /// <returns></returns>
        public static Color Create(string text)
        {
            try
            {
                string[] colors = text.Split('.');
                byte red = byte.Parse(colors[0]);
                byte green = byte.Parse(colors[1]);
                byte blue = byte.Parse(colors[2]);
                byte alpha = byte.Parse(colors[3]);
                Color color = Color.FromArgb(alpha, red, green, blue);
                return color;
            }
            catch (Exception)
            {
                Color color = Color.FromArgb(255, 125, 125, 125);
                return color;
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="colorName"></param>
        /// <param name="c"></param>
        public static void SetColor(ResourceDictionary resource, string colorName, Color c)
        {
            resource.Remove(colorName);
            resource.Add(colorName, new SolidColorBrush(c));
        }

        /// <summary>
        /// 设置主题
        /// </summary>
        /// <param name="themePath">主题文件路径</param>
        public static void SetColors(ResourceDictionary resource, string themePath)
        {
            SetColor(resource, "一级字体颜色", Create(INIHelper.ReadIniKeys("Font", "Level_1", themePath)));
            SetColor(resource, "二级字体颜色", Create(INIHelper.ReadIniKeys("Font", "Level_2", themePath)));
            SetColor(resource, "三级字体颜色", Create(INIHelper.ReadIniKeys("Font", "Level_3", themePath)));

            SetColor(resource, "一级背景颜色", Create(INIHelper.ReadIniKeys("Background", "Level_1", themePath)));
            SetColor(resource, "二级背景颜色", Create(INIHelper.ReadIniKeys("Background", "Level_2", themePath)));
            SetColor(resource, "三级背景颜色", Create(INIHelper.ReadIniKeys("Background", "Level_3", themePath)));

            SetColor(resource, "一级边框颜色", Create(INIHelper.ReadIniKeys("Border", "Level_1", themePath)));

            SetColor(resource, "有焦点选中颜色", Create(INIHelper.ReadIniKeys("Highlight", "Focused", themePath)));
            SetColor(resource, "无焦点选中颜色", Create(INIHelper.ReadIniKeys("Highlight", "UnFocused", themePath)));
        }
    }
}
