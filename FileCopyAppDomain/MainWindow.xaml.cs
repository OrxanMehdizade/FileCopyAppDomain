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
        private bool suspendCopy;
        private string sourceFilePath;
        private string destinationFilePath;
        private BackgroundWorker copyWorker;
        private long totalBytesCopied;

        public MainWindow()
        {
            InitializeComponent();
            suspendCopy = false;
            copyWorker = new BackgroundWorker();
            copyWorker.WorkerReportsProgress = true;
            copyWorker.DoWork += CopyWorker_DoWork;
            copyWorker.ProgressChanged += CopyWorker_ProgressChanged;
            copyWorker.RunWorkerCompleted += CopyWorker_RunWorkerCompleted;
        }

        private void CopyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
                {
                    var buffer = new byte[1];
                    int bytesRead;
                    int copyDelay = 100; 
                    sourceStream.Seek(totalBytesCopied, SeekOrigin.Begin);

                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (suspendCopy)
                        {
                            e.Cancel = true;
                            return;
                        }

                        destinationStream.Write(buffer, 0, bytesRead);
                        totalBytesCopied += bytesRead;
                        copyWorker.ReportProgress((int)(totalBytesCopied * 100 / sourceStream.Length));
                        Thread.Sleep(copyDelay);
                    }
                }
            }
        }

        private void CopyWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void CopyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Köçürmə əməliyyatı dayandırıldı.");
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Köçürmə əməliyyatı zamanı xəta oldu-> " + e.Error.Message);
            }
            else
            {
                MessageBox.Show("Köçürmə əməliyyatı başa çatı.");
            }
        }

        private void OpenSourceButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                sourceFilePath = openFileDialog.FileName;
                sourceTextBox.Text = sourceFilePath;
            }
        }

        private void OpenDestinationButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                destinationFilePath = saveFileDialog.FileName;
                destinationTextBox.Text = destinationFilePath;
            }
        }

        private void SuspendButton_Click(object sender, RoutedEventArgs e)
        {
            suspendCopy = true;
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            suspendCopy = false;
            copyWorker.RunWorkerAsync(); 
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationFilePath))
            {
                MessageBox.Show("Mənbə və məlumat faylı seç.");
                return;
            }

            progressBar.Value = 0;
            suspendCopy = false;
            copyWorker.RunWorkerAsync();
        }
    }
}