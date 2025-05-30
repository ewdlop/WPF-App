using System;
using System.Threading;
using System.Windows;

namespace WpfApp2;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Create and run the WPF application
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
} 