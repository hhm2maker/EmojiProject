using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
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
using Emoji.Wpf;
using TextBlock = Emoji.Wpf.TextBlock;

namespace EmojiProject
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 卡牌长宽
        /// </summary>
        double width, height;
        /// <summary>
        /// 刷新次数
        /// </summary>
        private int RefreshCount = 0;
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private string iniPath = AppDomain.CurrentDomain.BaseDirectory + "\\setting.ini";
        /// <summary>
        /// 卡片数量
        /// </summary>
        private int emojiCount = 10;
        /// <summary>
        /// 卡片组量
        /// </summary>
        private int emojiGroup = 2;

        List<string> imgs = new List<string>();
        Dictionary<string, int> imgDic = new Dictionary<string, int>();

        bool isFirst = true;
        List<int> list;

        public MainWindow()
        {
            InitializeComponent();

            Height = SystemParameters.WorkArea.Size.Height * 0.8;
            Width = Height / 4 * 3 + 100;

            Left = SystemParameters.WorkArea.Size.Height * 0.1;
            Top = SystemParameters.WorkArea.Size.Height * 0.1;

            if (!File.Exists(iniPath))
            {
                File.Create(iniPath);
            }
        }

        static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                return;
            }

            RefreshCount = 0;
            tbRefreshCount.Text = "0";
            imgs.Clear();
            imgDic.Clear();
            cMain.Children.Clear();
            spBottom.Children.Clear();

            height = (ActualHeight - 100) / 12;
            width = height;

            Random rEmoji = new Random(GetRandomSeed());

            string iniEmoji = IniHelper.Read("main", "emoji", "", iniPath);

            if (int.TryParse(IniHelper.Read("main", "emojiCount", "", iniPath), out int iniEmojiCount))
            {
                emojiCount = iniEmojiCount;
            }
            if (int.TryParse(IniHelper.Read("main", "emojiGroup", "", iniPath), out int iniemojiGroup))
            {
                emojiGroup = iniemojiGroup;
            }

            tbEmoji.Text = iniEmoji;
            tbEmojiCount.Text = iniEmojiCount + "";
            tbEmojiGroup.Text = iniemojiGroup + "";


            string[] strs = iniEmoji.Split(' ');

            if (strs.Length > emojiCount)
            {
                imgs = strs.Take(emojiCount).ToList();
            }
            else
            {
                imgs = strs.ToList();
            }
            for (int i = imgs.Count - 1; i >= 0; i--)
            {
                try
                {
                    imgs[i] = char.ConvertFromUtf32(Convert.ToInt32(imgs[i], 16));
                    imgDic.Add(imgs[i], 3 * emojiGroup);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    imgs.RemoveAt(i);
                }
            }

            while (imgs.Count < emojiCount)
            {
                int num = rEmoji.Next(200);
                string str = char.ConvertFromUtf32(0x1F642 + num);
                if (!imgs.Contains(str))
                {
                    imgs.Add(str);
                    imgDic.Add(str, 3 * emojiGroup);
                }
            }
            Random r = new Random(GetRandomSeed());
            list = new List<int>();
            int allCount = 3 * emojiGroup * emojiCount;
            while (list.Count < allCount)
            {
                int i = r.Next(1000);

                if (!list.Contains(i))
                {
                    list.Add(i);
                }
            }
            //排序
            list.Sort();

            Rondom();
        }

        private void Rondom()
        {
            Random r = new Random(GetRandomSeed());

            //输出
            for (int i = 0; i < list.Count; i++)
            {
                if (imgDic.Count == 0)
                {
                    break;
                }
                //TextBlock tb = UIUtils.GetTextBlock(x + Environment.NewLine + list[i] + "");
                int p = r.Next(imgDic.Count);

                //Console.WriteLine(p + "===" + imgDic.Count + "===" + cMain.Children.Count);
                string str = imgs[p];
                if (imgDic.ContainsKey(str))
                {
                    imgDic[str] -= 1;

                    if (imgDic[str] == 0)
                    {
                        imgDic.Remove(str);
                        imgs.Remove(str);
                    }
                }

                //TextBlock tb = UIUtils.GetTextBlock(str);
                TextBlock tb = new TextBlock();
                tb.Text = str;
                tb.FontSize = 30;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.VerticalAlignment = VerticalAlignment.Center;

                Border border = new Border();
                border.MouseLeftButtonUp += Border_MouseLeftButtonUp;
                border.Tag = list[i];
                border.Background = new SolidColorBrush(Color.FromRgb(0x56, 0x73, 0xff));
                border.BorderThickness = new Thickness(2);
                border.CornerRadius = new CornerRadius(5);
                border.Width = width;
                border.Height = height;
                border.Child = tb;

                int x = list[i] / 100;
                int y = (list[i] % 100) / 10;
                int z = (list[i] % 100) % 10;

                double top = y * height;
                double left = z * height;

                Canvas.SetTop(border, x % 2 == 0 ? top : top + height / 2);
                Canvas.SetLeft(border, x % 2 == 0 ? left : left + width / 2);

                Canvas.SetZIndex(border, x);
                cMain.Children.Add(border);
            }

            RefreshEnable();
        }

        /// <summary>
        /// 刷新剩余图标
        /// </summary>
        private void RefreshOther_Click(object sender, MouseButtonEventArgs e)
        {
            List<string> strs = new List<string>();
            foreach (var item in cMain.Children)
            {
                strs.Add(((item as Border).Child as TextBlock).Text);
            }

            cMain.Children.Clear();

            Random r = new Random(GetRandomSeed());
            list = new List<int>();
            while (list.Count < strs.Count)
            {
                int i = r.Next(1000);

                if (!list.Contains(i))
                {
                    list.Add(i);
                }
            }

            //排序
            list.Sort();

            //输出
            for (int i = 0; i < list.Count; i++)
            {
                //TextBlock tb = UIUtils.GetTextBlock(str);
                TextBlock tb = new TextBlock();
                tb.Text = strs[i];
                tb.FontSize = 30;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.VerticalAlignment = VerticalAlignment.Center;

                Border border = new Border();
                border.MouseLeftButtonUp += Border_MouseLeftButtonUp;
                border.Tag = list[i];
                border.Background = new SolidColorBrush(Color.FromRgb(0x56, 0x73, 0xff));
                border.BorderThickness = new Thickness(2);
                border.CornerRadius = new CornerRadius(5);
                border.Width = width;
                border.Height = height;
                border.Child = tb;

                int x = list[i] / 100;
                int y = (list[i] % 100) / 10;
                int z = (list[i] % 100) % 10;

                double top = y * height;
                double left = z * height;

                Canvas.SetTop(border, x % 2 == 0 ? top : top + height / 2);
                Canvas.SetLeft(border, x % 2 == 0 ? left : left + width / 2);

                Canvas.SetZIndex(border, x);
                cMain.Children.Add(border);
            }

            RefreshEnable();
            RefreshCount++;
            tbRefreshCount.Text = RefreshCount + "";
        }

        private void RefreshEnable()
        {
            foreach (var item in cMain.Children)
            {
                Border border = item as Border;
                border.IsEnabled = true;
                border.Background = new SolidColorBrush(Color.FromRgb(0x56, 0x73, 0xff));

                int count = 0;
                foreach (var otherItem in cMain.Children)
                {
                    if (item == otherItem)
                    {
                        continue;
                    }

                    if (!IsEnable(border, otherItem as Border))
                    {
                        count++;
                    }
                }

                if (count > 0)
                {
                    border.IsEnabled = false;
                    border.Background = new SolidColorBrush(Color.FromArgb((byte)(255 / 5 * (5 - count)), 0x56, 0x73, 0xff));
                }
            }
        }


        private bool IsEnable(Border bThis, Border bOther)
        {
            int position = (int)bThis.Tag % 100;
            int otherPosition = (int)bOther.Tag % 100;
            int zIndex = Canvas.GetZIndex(bThis);
            int zIndexOther = Canvas.GetZIndex(bOther);

            if (otherPosition == position)
            {
                //同一个位置,下面的不可用
                if (zIndexOther > zIndex)
                {
                    return false;
                }
            }

            //不同层
            if (zIndexOther > zIndex)
            {
                int xNum = zIndexOther - zIndex;
                if (xNum % 2 == 1)
                {
                    if (zIndexOther % 2 == 1)
                    {
                        if (position % 10 != 0)
                        {
                            if (position - 11 == otherPosition || position - 1 == otherPosition)
                            {
                                return false;
                            }
                        }
                        if (position - 10 == otherPosition)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (position % 10 != 9)
                        {
                            if (position + 11 == otherPosition || position + 1 == otherPosition)
                            {
                                return false;
                            }
                        }
                        if (position + 10 == otherPosition)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (spBottom.Children.Count >= 7)
            {
                return;
            }

            Border border = sender as Border;
            if (border.Parent == spBottom)
            {
                return;
            }

            string text = (border.Child as TextBlock).Text;
            cMain.Children.Remove(border);
            if (spBottom.Children.Count != 0)
            {
                border.Margin = new Thickness(5, 0, 0, 0);
            }
            spBottom.Children.Add(border);
            List<Border> borders = new List<Border>();
            foreach (var item in spBottom.Children)
            {
                Border b = item as Border;
                if ((b.Child as TextBlock).Text.Equals(text))
                {
                    borders.Add(b);
                }
            }

            if (borders.Count == 3)
            {
                foreach (var item in borders)
                {
                    spBottom.Children.Remove(item);
                }

                if (spBottom.Children.Count > 0)
                {
                    (spBottom.Children[0] as Border).Margin = new Thickness(0);
                }

                if (cMain.Children.Count == 0)
                {
                    musicplay.PlayMusic(AppDomain.CurrentDomain.BaseDirectory + @"\Resources\music_success.mp3");
                    OpenSetting_Click(null, null);
                    tbSuccess.Visibility = Visibility.Visible;
                }
                else
                {
                    musicplay.PlayMusic(AppDomain.CurrentDomain.BaseDirectory + @"\Resources\music_clear.mp3");
                }
            }
            else
            {
                musicplay.PlayMusic(AppDomain.CurrentDomain.BaseDirectory + @"\Resources\music_select.mp3");
            }

            if (spBottom.Children.Count >= 7)
            {
                musicplay.PlayMusic(AppDomain.CurrentDomain.BaseDirectory + @"\Resources\music_fail.mp3");
                OpenSetting_Click(null, null);
                return;
            }

            RefreshEnable();
        }

        private void OpenSetting_Click(object sender, MouseButtonEventArgs e)
        {
            bSetting.Visibility = Visibility.Visible;
            gSetting.Visibility = Visibility.Visible;
            tbSuccess.Visibility = Visibility.Collapsed;
        }

        private void CloseSetting_Click(object sender, MouseButtonEventArgs e)
        {
            bSetting.Visibility = Visibility.Collapsed;
            gSetting.Visibility = Visibility.Collapsed;
            tbSuccess.Visibility = Visibility.Collapsed;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            IniHelper.Write("main", "emoji", tbEmoji.Text, iniPath);
            IniHelper.Write("main", "emojiCount", tbEmojiCount.Text, iniPath);
            IniHelper.Write("main", "emojiGroup", tbEmojiGroup.Text, iniPath);
        }

        private void RefreshAll_Click(object sender, MouseButtonEventArgs e)
        {
            TextBox_LostFocus(null, null);
            isFirst = true;
            UserControl_Loaded(null, null);
            CloseSetting_Click(null, null);
        }
    }
}
