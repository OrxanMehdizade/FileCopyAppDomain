using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace FileCopyAppDomain
{
    public partial class MainWindow : Window
    {
        private bool Suspend;
        private string SourcePath;
        private string DestinationPath;
        private BackgroundWorker CopyWorker;
        private long totalBytesdownloading;


        public MainWindow()
        {
            InitializeComponent();
            Suspend = false;
            CopyWorker = new BackgroundWorker();
            CopyWorker.WorkerReportsProgress = true;
            CopyWorker.DoWork+=CopyWorkerDowork;
            CopyWorker.ProgressChanged += CopyWorkerProgressChanged;
            CopyWorker.RunWorkerCompleted += CopyWorkerRunWorkerCompleted;
        }
        private void CopyWorkerDowork(object sender,DoWorkEventArgs e)
        {
            using (var sourceStream = new FileStream(SourcePath,
                FileMode.Open,FileAccess.Read))
            {
                using(var destinationStream= new FileStream(DestinationPath,
                    FileMode.Create, FileAccess.Write))
                {
                    var byt = new byte[1];
                    int bytRead;
                    totalBytesdownloading = 0;
                    int copytime = 200;
                    while((bytRead=sourceStream.Read(byt,0,byt.Length))>0) 
                    {

                        if (Suspend)
                        {
                            while (Suspend)
                            {
                                Thread.Sleep(200);
                            }
                            if (totalBytesdownloading >= sourceStream.Length)
                            {
                                return;
                            }
                        }
                        destinationStream.Write(byt, 0, bytRead);
                        totalBytesdownloading+= bytRead;

                        CopyWorker.ReportProgress((int)(totalBytesdownloading 
                            * 100 / sourceStream.Length));
                        Thread.Sleep(copytime);
                    }
                }

            }

        }

        private void CopyWorkerProgressChanged(object sender,ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void CopyWorkerRunWorkerCompleted(object sender,RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled) 
            {
                MessageBox.Show("Köçürmə əməliyyatı dayandırıldı");
            }
            else if (e.Error!= null) 
            {
                MessageBox.Show("Köçürmə əməliyyatı zamanı xəta oldu-> "+ e.Error.Message);
            }
            else
            {
                MessageBox.Show("Köçürmə əməliyyatı başa çatı");
            }
        }

        private void OpenSourceButton_Click(object sender, RoutedEventArgs e) 
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SourcePath = openFileDialog.FileName;
                sourceTextBox.Text = SourcePath;
            }
        }
        private void OpenDestinationButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            if(saveFileDialog.ShowDialog() == true)
            {
                DestinationPath = saveFileDialog.FileName;
                destinationTextBox.Text = DestinationPath;
            }
        }
        private void SuspendButton_Click(object sender, RoutedEventArgs e)
        {
            Suspend=true;
        }
        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            Suspend =false;
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(SourcePath) || string.IsNullOrEmpty(DestinationPath))
            {
                MessageBox.Show("Mənbə və məlumat faylı seç");
                return;
            }
            progressBar.Value = 0;
            Suspend = false;
            CopyWorker.RunWorkerAsync();
        }

    }

}