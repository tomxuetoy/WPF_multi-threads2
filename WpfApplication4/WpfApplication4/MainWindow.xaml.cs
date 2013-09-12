using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WpfApplication4
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 消息事件
        /// </summary>
        private event EventHandler MessaegRecevied = delegate { };

        /// <summary>
        /// 线程消息
        /// </summary>
        readonly List<string> _threadMsg = new List<string>();

        /// <summary>
        /// 消息互斥锁
        /// </summary>
        Mutex _mutex = new Mutex();

        /// <summary>
        /// 写入消息
        /// </summary>
        /// <param name="msg"></param>
        private void PushMessage(string msg)
        {
            _mutex.WaitOne();
            _threadMsg.Add(msg);
            //TODO
            Debug.WriteLine(msg);
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// 弹出消息
        /// </summary>
        /// <returns></returns>
        private string PopMessage()
        {
            string ret = null;
            _mutex.WaitOne();
            if (_threadMsg.Count > 0)
            {
                ret = _threadMsg[0];
                _threadMsg.RemoveAt(0);
            }
            _mutex.ReleaseMutex();
            return ret;
        }

        /// <summary>
        /// 线程列表
        /// </summary>
        List<Thread> _threads = new List<Thread>();

        /// <summary>
        /// 构造
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            MessaegRecevied += OnMessaegRecevied;

            //
            var t = new DispatcherTimer();
            t.Tick += UpdateMessage;
            t.IsEnabled = true;
            t.Start();
        }

        /// <summary>
        /// 收到消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMessaegRecevied(object sender, EventArgs e)
        {
            //TODO
            //Debug.WriteLine(PopMessage());
        }

        /// <summary>
        /// 更新消息控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateMessage(object sender, EventArgs e)
        {
            //这里面不知道多线程访问是怎么实现的，回来再看
            richTextBox1.AppendText(PopMessage());
            richTextBox1.ScrollToEnd();
        }

        /// <summary>
        /// 是否启动
        /// </summary>
        private bool _bStarted = false;

        /// <summary>
        /// 启动的线程数
        /// </summary>
        private const int ThreadNumber = 2;

        /// <summary>
        /// 启动多线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //已经启动
            if (_bStarted) return;

            _bStarted = true;

            //创建
            for (var i = 0; i < ThreadNumber; i++)
            {
                _threads.Add(new Thread(new ThreadStart(Run)));
            }

            //启动
            foreach (var thread in _threads)
            {
                thread.Start();
            }

            UpdateUi();
        }

        /// <summary>
        /// 更新界面
        /// </summary>
        private void UpdateUi()
        {
            //显示
            listView1.ItemsSource = _threads.Select(obj => new
            {
                Name = obj.Name,
                ThreadState = obj.ThreadState,
                ManagedThreadId = obj.ManagedThreadId,
            });
        }

        /// <summary>
        /// 线程回调
        /// </summary>
        public void Run()
        {
            while (true)
            {
                PushMessage(Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture) + " 发来消息");
                Thread.Sleep(200);
            }
            // ReSharper disable FunctionNeverReturns
        }
        // ReSharper restore FunctionNeverReturns

        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DestroyThreads();
        }

        /// <summary>
        /// 关闭线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            DestroyThreads();
        }

        /// <summary>
        /// 销毁线程
        /// </summary>
        private void DestroyThreads()
        {
            //销毁
            for (int index = _threads.Count - 1; index >= 0; index--)
            {
                var thread = _threads[index];
                thread.Abort();
                _threads.Remove(thread);
            }

            UpdateUi();

            _bStarted = false;
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }       
    }
}