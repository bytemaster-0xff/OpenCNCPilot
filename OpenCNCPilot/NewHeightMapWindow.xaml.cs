﻿using LagoVista.Core.Models.Drawing;
using System;
using System.Windows;

namespace LagoVista.GCode.Sender.Application
{
	/// <summary>
	/// Interaction logic for NewHeightMapWindow.xaml
	/// </summary>
	public partial class NewHeightMapWindow : Window
	{
		public event Action SelectedSizeChanged;
		public event Action Size_Ok;

		public Vector2 Min;
		public Vector2 Max;

		public bool Ok { get; set; } = false;
		public bool GenerateTestPattern { get; set; } = false;
		public string TestPattern { get; set; } = "(x * x + y * y) / 1000.0";

		public double MinX
		{
			get { return Min.X; }
			set
			{
				if (Min.X == value)
					return;

				Min.X = value;

				if (SelectedSizeChanged != null)
					SelectedSizeChanged.Invoke();
			}
		}

		public double MinY
		{
			get { return Min.Y; }
			set
			{
				if (Min.Y == value)
					return;
				Min.Y = value;
				if (SelectedSizeChanged != null)
					SelectedSizeChanged.Invoke();
			}
		}

		public double MaxX
		{
			get { return Max.X; }
			set
			{
				if (Max.X == value)
					return;
				Max.X = value;
				if (SelectedSizeChanged != null)
					SelectedSizeChanged.Invoke();
			}
		}

		public double MaxY
		{
			get { return Max.Y; }
			set
			{
				if (Max.Y == value)
					return;
				Max.Y = value;
				if (SelectedSizeChanged != null)
					SelectedSizeChanged.Invoke();
			}
		}

		private double _gridSize = 5;
		public double GridSize
		{
			get { return _gridSize; }
			set
			{
				if (_gridSize == value)
					return;
				if (value == 0)
					return;
				_gridSize = Math.Abs(value);

				if (SelectedSizeChanged != null)
					SelectedSizeChanged.Invoke();
			}
		}

		public NewHeightMapWindow(Vector2 min, Vector2 max)
		{
			Min = min;
			Max = max;

			InitializeComponent();
		}

		public NewHeightMapWindow() : this(new Vector2(), new Vector2(100, 50))
		{

		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if(Min.X > Max.X)
			{
				double a = Min.X;
				Min.X = Max.X;
				Max.X = a;
			}

			if (Min.Y > Max.Y)
			{
				double a = Min.Y;
				Min.Y = Max.Y;
				Max.Y = a;
			}

			Ok = true;

			if (Size_Ok != null)
				Size_Ok.Invoke();

			Close();
		}
	}
}
