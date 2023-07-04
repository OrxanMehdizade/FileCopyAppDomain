using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace FileCopyAppDomain
{
    public partial class MainWindow : Window
    {
        private string sourceFilePath;
        private string destinationFilePath;
        private BackgroundWorker copyWorker;
        private Thread thread;

        public MainWindow()
        {
            InitializeComponent();
            copyWorker = new BackgroundWorker();
            copyWorker.WorkerReportsProgress = true;
            copyWorker.ProgressChanged += CopyWorker_ProgressChanged;
            copyWorker.RunWorkerCompleted += CopyWorker_RunWorkerCompleted;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(sourceTextBox.Text))
                {
                    suspendButton.IsEnabled = true;
                    string text = File.ReadAllText(sourceTextBox.Text);
                    progressBar.Maximum = text.Length;
                    thread = new Thread(
                        () =>
                        {
                            StringBuilder writeText = new StringBuilder();
                            foreach (var c in text)
                            {
                                writeText.Append(c);
                                Dispatcher.Invoke(() =>
                                {
                                    File.WriteAllText(destinationTextBox.Text, writeText.ToString());
                                    progressBar.Value++;
                                });
                                Thread.Sleep(20);
                            }

                            Dispatcher.Invoke(() =>
                            {
                                progressBar.Value = 0;
                                sourceTextBox.Clear();
                                destinationTextBox.Clear();
                                startButton.IsEnabled = true;
                            });
                        });
                    thread.Start();
                    startButton.IsEnabled = false;
                }
                else
                {
                    throw new FileNotFoundException("Fayl mövcud deyil.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                sourceTextBox.Clear();
                destinationTextBox.Clear();
            }
        }

        private void CopyWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = e.ProgressPercentage;
            });
        }

        private void CopyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Kopyalama əməliyyatı dayandırıldı.");
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Kopyalama əməliyyatı zamanı xəta oldu-> " + e.Error.Message);
            }
            else
            {
                MessageBox.Show("Kopyalama basa çatdı.");
            }
        }

        private void OpenSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                sourceFilePath = openFileDialog.FileName;
                sourceTextBox.Text = sourceFilePath;
            }
        }

        private void OpenDestinationButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                destinationFilePath = saveFileDialog.FileName;
                destinationTextBox.Text = destinationFilePath;
            }
        }

        private void SuspendButton_Click(object sender, RoutedEventArgs e)
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Suspend();
            }
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (thread != null && thread.ThreadState == ThreadState.Suspended)
            {
                thread.Resume();
            }
        }
    }
}