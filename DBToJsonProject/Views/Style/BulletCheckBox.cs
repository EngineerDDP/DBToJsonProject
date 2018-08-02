using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DBToJsonProject.Views.Style
{
	/// <summary>
	/// BulletCheckBox.xaml 的交互逻辑
	/// </summary>
	public class BulletCheckBox : CheckBox
	{
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof(string), typeof(BulletCheckBox), new PropertyMetadata("Off"));
		/// <summary>
		/// 默认文本（未选中）
		/// </summary>
		public string Text
		{
			get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty CheckedTextProperty = DependencyProperty.Register(
			"CheckedText", typeof(string), typeof(BulletCheckBox), new PropertyMetadata("On"));
		/// <summary>
		/// 选中状态文本
		/// </summary>
		public string CheckedText
		{
			get => (string)GetValue(CheckedTextProperty); set => SetValue(CheckedTextProperty, value);
		}

		public static readonly DependencyProperty CheckedForegroundProperty =
			DependencyProperty.Register("CheckedForeground", typeof(Brush), typeof(BulletCheckBox), new PropertyMetadata(Brushes.WhiteSmoke));
		/// <summary>
		/// 选中状态前景样式
		/// </summary>
		public Brush CheckedForeground
		{
			get => (Brush)GetValue(CheckedForegroundProperty); set => SetValue(CheckedForegroundProperty, value);
		}

		public static readonly DependencyProperty CheckedBackgroundProperty =
			DependencyProperty.Register("CheckedBackground", typeof(Brush), typeof(BulletCheckBox), new PropertyMetadata(Brushes.LightBlue));
		/// <summary>
		/// 选中状态背景色
		/// </summary>
		public Brush CheckedBackground
		{
			get => (Brush)GetValue(CheckedBackgroundProperty); set => SetValue(CheckedBackgroundProperty, value);
		}

		static BulletCheckBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(BulletCheckBox), new FrameworkPropertyMetadata(typeof(BulletCheckBox)));
		}
	}
}
