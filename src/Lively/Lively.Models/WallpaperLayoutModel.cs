﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows;
using Newtonsoft.Json;

namespace Lively.Models
{
    /// <summary>
    /// Wallpaper arragement on display.
    /// </summary>
    [Serializable]
    public class WallpaperLayoutModel : IWallpaperLayoutModel
    {
        public IDisplayMonitor Display { get; set; }
        public string LivelyInfoPath { get; set; }

        /*
        [JsonConstructor]
        public WallpaperLayoutModel(string DeviceId, string DeviceName, int BitsPerPixel, Rectangle Bounds, Rectangle WorkingArea, string livelyInfoPath)
        {
            Display = new DisplayMonitor(DeviceId, DeviceName, BitsPerPixel, Bounds, WorkingArea);
            this.LivelyInfoPath = livelyInfoPath;
        }
        */

        public WallpaperLayoutModel(IDisplayMonitor Display, string livelyInfoPath)
        {
            this.Display = Display;
            this.LivelyInfoPath = livelyInfoPath;
        }
    }
}