﻿using System.Windows;

namespace BallMusic.Creator;

public partial class LoadingBar : Window
{
    public IProgress<float> Progress { get; }
    public IProgress<string> LabelProgress { get; }

    public LoadingBar(bool isIndeterminate)
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        InitializeComponent();

        Bar.IsIndeterminate = isIndeterminate;
        Progress = new Progress<float>(value => Bar.Value = value);
        LabelProgress = new Progress<string>(txt => ActionLabel.Content = txt);
    }
}
