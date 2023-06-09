﻿using livelywpf.Core.API;
using livelywpf.Helpers.Pinvoke;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Threading;
using livelywpf.Models;
using livelywpf.Views.Wallpapers;
using livelywpf.Helpers.Shell;

namespace livelywpf.Core.Wallpapers
{
    class WebEdge : IWallpaper
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public event EventHandler<WindowInitializedArgs> WindowInitialized;
        private IntPtr hwndWindow, hwndWebView;
        private readonly WebView2Element player;
        private readonly ILibraryModel model;
        private ILivelyScreen display;

        public bool IsLoaded { get; private set; } = false;

        public WallpaperType Category => model.LivelyInfo.Type;

        public ILibraryModel Model => model;

        public IntPtr Handle => hwndWindow;

        public IntPtr InputHandle => hwndWebView;

        public Process Proc => null;

        public ILivelyScreen Screen { get => display; set => display = value; }

        public string LivelyPropertyCopyPath { get; }

        public WebEdge(string path, ILibraryModel model, ILivelyScreen display, string livelyPropertyPath)
        {
            LivelyPropertyCopyPath = livelyPropertyPath;

            player = new WebView2Element(path, model.LivelyInfo.Type, LivelyPropertyCopyPath);
            this.model = model;
            this.display = display;
        }

        public void Close()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(delegate
            {
                player.Close();
            }));
        }

        public void Pause()
        {
            //minimize browser.
            NativeMethods.ShowWindow(hwndWebView, (uint)NativeMethods.SHOWWINDOW.SW_SHOWMINNOACTIVE);
        }

        public void Play()
        {
            //show minimized browser.
            NativeMethods.ShowWindow(hwndWebView, (uint)NativeMethods.SHOWWINDOW.SW_SHOWNOACTIVATE);
        }

        public void SendMessage(string msg)
        {
            player?.MessageProcess(msg);
        }

        public void SetVolume(int volume)
        {
            //todo
        }

        public async void Show()
        {
            if (player != null)
            {
                player.LivelyPropertiesInitialized += Player_LivelyPropertiesInitialized;
                player.Closed += Player_Closed;
                player.Show();
                //visible window..
                this.hwndWindow = new WindowInteropHelper(player).Handle;

                bool status = true;
                Exception error = null;
                string message = null;
                try
                {
                    var tmpHwnd = await player.InitializeWebView();
                    //input window..
                    var parentHwnd = NativeMethods.FindWindowEx(tmpHwnd, IntPtr.Zero, "Chrome_WidgetWin_0", null);
                    if (!parentHwnd.Equals(IntPtr.Zero))
                    {
                        this.hwndWebView = NativeMethods.FindWindowEx(parentHwnd, IntPtr.Zero, "Chrome_WidgetWin_1", null);
                    }

                    if (this.hwndWebView.Equals(IntPtr.Zero))
                    {
                        throw new Exception("Webview input handle not found.");
                    }
                }
                catch (Exception e)
                {
                    error = e;
                    status = false;
                    message = "WebView initialization fail.";
                }
                finally
                {
                    WindowInitialized?.Invoke(this, new WindowInitializedArgs() { Success = status, Error = error, Msg = message });
                }
            }
        }

        private void Player_LivelyPropertiesInitialized(object sender, EventArgs e)
        {
            IsLoaded = true;
            player.LivelyPropertiesInitialized -= Player_LivelyPropertiesInitialized;
        }

        private void Player_Closed(object sender, EventArgs e)
        {
            DesktopUtil.RefreshDesktop();
        }

        public void Stop()
        {

        }

        public void Terminate()
        {
            Close();
        }

        public void SetPlaybackPos(float pos, PlaybackPosType type)
        {
            if (pos == 0 && type != PlaybackPosType.relativePercent)
            {
                SendMessage(new LivelyReloadCmd());
            }
        }

        public async Task ScreenCapture(string filePath)
        {
            await player?.CaptureScreenshot(Path.GetExtension(filePath) != ".jpg" ? filePath + ".jpg" : filePath, ScreenshotFormat.jpeg);
        }

        public void SendMessage(IpcMessage obj)
        {
            player?.MessageProcess(obj);
        }
    }
}
