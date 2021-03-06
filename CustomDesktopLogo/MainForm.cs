﻿// Custom Desktop Logo 2.2 - By: 2008 Eric Wong
// October 18th, 2008
// Custom Desktop Logo is open source software licensed under GNU GENERAL PUBLIC LICENSE V3. 
// Use it as you wish, but you must share your source code under the terms of use of the license.

// Custom Desktop Logo allows you to create custom static and animated logos from PNG images.
// Version 2.0 adds the drop folder capabilities.
// Version 2.2 adds the ability to use Windows default move/copy dialogs and fixes the unicode compatibility.

// Copyright (C) 2008 by Eric Wong. 
// VideoInPicture@gmail.com
// http://customdesktoplogo.wikidot.com
// http://easyunicodepaster.wikidot.com
// http://circledock.wikidot.com
// http://videoinpicture.wikidot.com
// http://webcamsignature.wikidot.com
// http://windowextractor.wikidot.com

// Uses AMS.Profile from http://www.codeproject.com/KB/cs/readwritexmlini.aspx for .ini file operations (Open source, non-specific license)
// Uses hotkey selector component from http://www.codeproject.com/KB/miscctrl/systemhotkey.aspx (Open source, non-specific license)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Hook;
using ImageOperations;
using MemoryManagement;

namespace CustomDesktopLogo
{
    public enum WindowLevelTypes
    {
        Topmost,
        Normal,
        AlwaysOnBottom
    }

    public enum LocationTypes
    {
        CustomPosition,
        Centre,
        BottomMiddle,
        TopMiddle,
        LeftMiddle,
        RightMiddle,
        BottomRightCorner,
        BottomLeftCorner,
        TopRightCorner,
        TopLeftCorner
    }

    public enum MultiMonitorDisplayModes
    {
        AllSame,
        DisplayOnPrimaryOnly,
        DisplayOnAllButPrimary,
        TreatMonitorsAsOneScreen
    }

    public enum FileLocationActions
    {
        Move,
        Copy,
        Open,
        AddTargetFolder
    }

    public partial class MainForm : Form
    {
        #region Variables

        // The directory and file names used to store the resources for the program.
        static readonly string imagesDirectoryName = @"Images";
        static readonly string systemFilesDirectoryName = @"System";
        static readonly string languagesDirectoryName = @"Languages";
        static readonly string settingsDirectoryName = @"Settings";
        static readonly string configINIFileName = @"Config.ini";
        static readonly int numFolderPaths = 20;

        /// <summary>
        /// The language collection used for displayed words.
        /// </summary>
        public static LanguageLoader.LanguageLoader language;

        /// <summary>
        /// The general settings for the program.
        /// </summary>
        public static SettingsLoader.SettingsLoader settingsINI;

        /// <summary>
        /// The list of all the logos currently active.
        /// </summary>
        static List<LogoObject> allLogos = new List<LogoObject>();

        /// <summary>
        /// Stores all the images used for the logos.
        /// </summary>
        static List<Bitmap> imageBitmaps = new List<Bitmap>();

        static int imageBitmapsIndex;

        static int elapsedTime;

        static bool loaded;

        static Size desiredLogoSize = new Size(1, 1);

        static bool scaleImageFactorChanged;

        static bool isUpdatingLogoGraphics;

        static Hooks windowsHook = new Hooks();

        // These delegates enables asynchronous calls 
        delegate void ShowHideCallback();
        delegate void UpdatePositionsCallback();
        delegate void AnimationCallback();
        delegate void CloseLogoCallback();
        delegate void UpdateSizeOpacityCallback();

        private static MainForm instance;
        public static MainForm Instance => instance;

        #endregion

        #region Constructor and Initialization

        static MainForm()
        {
             instance = new MainForm();
        }

        /// <summary>

        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;
            checkSystemFoldersExist();

            // Load settings from files
            settingsINI = new SettingsLoader.SettingsLoader(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName +
                Path.DirectorySeparatorChar + settingsDirectoryName + Path.DirectorySeparatorChar + configINIFileName);
            loadLanguage();

            DoubleBuffered = true;

            // We require this hook to correct a Windows bug where a topmost window will become not topmost in some cases
            // when other programs refresh the screen.
            windowsHook.OnForegroundWindowChanged += window_ForegroundChanged;
            GC.KeepAlive(windowsHook);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Hide();
            ShowInTaskbar = true;
            Opacity = 1.0;
            MemoryUtility.ClearUnusedMemory();
        }

        private void checkSystemFoldersExist()
        {
            // Make sure the "System" folder exists
            if (!Directory.Exists(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName))
            {
                try
                {
                    Directory.CreateDirectory(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName);
                }
                catch (Exception)
                {
                    MessageBox.Show(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + @" is missing.", @"Custom Desktop Logo");
                }
            }

            // Make sure the "Images" folder exists
            if (!Directory.Exists(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar
                + imagesDirectoryName))
            {
                try
                {
                    Directory.CreateDirectory(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar
                        + imagesDirectoryName);
                }
                catch (Exception)
                {
                    MessageBox.Show(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar
                        + imagesDirectoryName + @" is missing.", @"Custom Desktop Logo");
                }
            }

            // Make sure the "Languages" folder exists
            if (!Directory.Exists(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar
                + languagesDirectoryName))
            {
                try
                {
                    Directory.CreateDirectory(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar
                        + languagesDirectoryName);
                }
                catch (Exception)
                {
                    MessageBox.Show(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar
                        + languagesDirectoryName + @" is missing.", @"Custom Desktop Logo");
                }
            }

            // Make sure the "Settings" folder exists
            if (!Directory.Exists(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar
                + settingsDirectoryName))
            {
                try
                {
                    Directory.CreateDirectory(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar
                        + settingsDirectoryName);
                }
                catch (Exception)
                {
                    MessageBox.Show(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar
                        + settingsDirectoryName + @" is missing.", @"Custom Desktop Logo");
                }
            }
        }

        #endregion

        #region Main Methods

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainFormTrayIcon.Visible = true;

            loadLanguage();
            loadImageList();

            // Location Tab settings
            if (settingsINI.LogoProperties.windowLevel == WindowLevelTypes.AlwaysOnBottom)
                alwaysOnBottomRadioButton.Checked = true;
            else if (settingsINI.LogoProperties.windowLevel == WindowLevelTypes.Normal)
                normalRadioButton.Checked = true;
            else
                topmostRadioButton.Checked = true;

            switch (settingsINI.LogoProperties.multiMonitorDisplayMode)
            {
                case MultiMonitorDisplayModes.TreatMonitorsAsOneScreen:
                    virtualMonitorRadioButton.Checked = true;
                    break;
                case MultiMonitorDisplayModes.DisplayOnPrimaryOnly:
                    primaryOnlyRadioButton.Checked = true;
                    break;
                case MultiMonitorDisplayModes.DisplayOnAllButPrimary:
                    allButPrimaryRadioButton.Checked = true;
                    break;
                case MultiMonitorDisplayModes.AllSame:
                    allSameRadioButton.Checked = true;
                    break;
                default:
                    allSameRadioButton.Checked = true;
                    break;
            }

            switch (settingsINI.LogoProperties.displayLocation)
            {
                case LocationTypes.Centre:
                    centreRadioButton.Checked = true;
                    break;
                case LocationTypes.BottomLeftCorner:
                    bottomLeftRadioButton.Checked = true;
                    break;
                case LocationTypes.BottomMiddle:
                    bottomMiddleRadioButton.Checked = true;
                    break;
                case LocationTypes.BottomRightCorner:
                    bottomRightRadioButton.Checked = true;
                    break;
                case LocationTypes.LeftMiddle:
                    leftMiddleRadioButton.Checked = true;
                    break;
                case LocationTypes.CustomPosition:
                    displayAtLocationOffsetRadioButton.Checked = true;
                    break;
                case LocationTypes.TopLeftCorner:
                    topLeftRadioButton.Checked = true;
                    break;
                case LocationTypes.TopMiddle:
                    topMiddleRadioButton.Checked = true;
                    break;
                case LocationTypes.TopRightCorner:
                    topRightRadioButton.Checked = true;
                    break;
                case LocationTypes.RightMiddle:
                    rightMiddleRadioButton.Checked = true;
                    break;
                default:
                    bottomRightRadioButton.Checked = true;
                    break;
            }

            xOffsetNumericUpDown.Value = settingsINI.LogoProperties.xOffset;
            yOffsetNumericUpDown.Value = settingsINI.LogoProperties.yOffset;

            // Size tab
            try
            {
                scaleImageFactorTrackBar.Value = settingsINI.LogoProperties.scaleImagesFactor;
            }
            catch (Exception)
            {
                scaleImageFactorTrackBar.Value = 0;
            }
            scaleImageFactorValueLabel.Text = ((settingsINI.LogoProperties.scaleImagesFactor + 100) / 100.0).ToString();

            // Animation/Graphics tab
            try
            {
                fpsTrackBar.Value = settingsINI.GeneralAnimation.framesPerSecond;
            }
            catch (Exception)
            {
                fpsTrackBar.Value = 0;
            }
            fpsValueLabel.Text = fpsTrackBar.Value.ToString();

            try
            {
                delayBetweenAnimationsTrackBar.Value = settingsINI.GeneralAnimation.delayBetweenAnimations;
            }
            catch (Exception)
            {
                delayBetweenAnimationsTrackBar.Value = 0;
            }
            delayBetweenAnimationsValueLabel.Text = delayBetweenAnimationsTrackBar.Value.ToString();

            try
            {
                opacityTrackBar.Value = settingsINI.LogoProperties.defaultOpacity;
            }
            catch (Exception)
            {
                opacityTrackBar.Value = 255;
            }
            opacityValueLabel.Text = opacityTrackBar.Value.ToString();


            try
            {
                AnimationTimer.Interval = 1000 / settingsINI.GeneralAnimation.framesPerSecond;
            }
            catch (Exception)
            {
                AnimationTimer.Interval = 100;
            }

            TargetFolderBrowserDialog.SelectedPath = Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName
                + Path.DirectorySeparatorChar + imagesDirectoryName + Path.DirectorySeparatorChar;

            string[] directoryList = {};

            try
            {
                directoryList = Directory.GetDirectories(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName
                    + Path.DirectorySeparatorChar + imagesDirectoryName + Path.DirectorySeparatorChar);
            }
            catch (Exception)
            { }

            if (directoryList != null && directoryList.Length >= 1)
            {
                TargetFolderBrowserDialog.SelectedPath = directoryList[0];
            }

            // For the Folder Paths tab
            useAsDropFolderCheckBox.Checked = settingsINI.FolderPaths.useAsDropFolder;
            disableLogoMovementCB.Checked = settingsINI.LogoProperties.disableMovement;

            filePathsDataGridView.Rows.Add(numFolderPaths);

            for (var i = 0; i < numFolderPaths; i++)
            {
                filePathsDataGridView[0, i].Value = settingsINI.FolderPaths.folderPaths[i].displayName;
                filePathsDataGridView[1, i].Value = settingsINI.FolderPaths.folderPaths[i].path;
            }

            populateFilePathsContextMenuStrip(filePathsContextMenuStrip, FileLocationActions.Copy);

            loaded = true;
        }

        private void loadImageList()
        {
            imagesListBox.Items.Clear();
            imageBitmaps.Clear();

            try
            {
                string[] imageFileDirectory;
                if (settingsINI.LogoProperties.path.StartsWith(@"." + Path.DirectorySeparatorChar))
                    imageFileDirectory = Directory.GetFiles(Application.StartupPath + settingsINI.LogoProperties.path.Substring(1));
                else
                    imageFileDirectory = Directory.GetFiles(settingsINI.LogoProperties.path);

                var counter = 0;
                foreach (var aFile in imageFileDirectory)
                {
                    if (aFile.ToUpper().EndsWith(@"PNG"))
                    {
                        Bitmap anImage;

                        try
                        {
                            anImage = new Bitmap(aFile);
                            anImage = BitmapOperations.ScaleByFactors(ref anImage, (settingsINI.LogoProperties.scaleImagesFactor + 100) / 100.0f,
                                (settingsINI.LogoProperties.scaleImagesFactor + 100) / 100.0f);
                            imageBitmaps.Add(anImage);
                            imagesListBox.Items.Add(aFile);
                            counter++;
                        }
                        catch (Exception)
                        { }
                    }

                    // Prevent the program from freezing the computer and taking too much memory.
                    if (counter > 20)
                    {
                        counter = 0;
                        GC.Collect();
                        var memoryCounter = new Pinvoke.Win32.PROCESS_MEMORY_COUNTERS();
                        Pinvoke.Win32.GetProcessMemoryInfo(Pinvoke.Win32.GetCurrentProcess(), out memoryCounter, Marshal.SizeOf(memoryCounter));

                        if (memoryCounter.WorkingSetSize > 100000000)
                        {
                            if (MessageBox.Show(language.errorMessages.usingTooMuchMemoryContinueQuestion, language.errorMessages.customDesktopLogo, MessageBoxButtons.YesNo) == DialogResult.No)
                            {
                                imagesListBox.Items.Clear();
                                imageBitmaps.Clear();
                                GC.Collect();

                                settingsINI.LogoProperties.path = @"";
                                settingsINI.SetEntry("LogoProperties", "path", @"");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            { 
            }

            if (imageBitmaps.Count <= 0)
                imageBitmaps.Add((Bitmap)FancyText.ImageFromText(@"?????", new Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold), Color.Black, Color.White, 6, 10));
            
            loadLogos();

            GC.Collect();
            MemoryUtility.ClearUnusedMemory();
        }

        private void closeAllLogos()
        {
            for (var i = 0; i < allLogos.Count; i++)
            {
                if (allLogos[i].InvokeRequired)
                {
                    var d = new CloseLogoCallback(closeAllLogos);
                    allLogos[i].Invoke(d, new object[] { });
                }
                else
                {
                    allLogos[i].Dispose();
                } 
            }

            allLogos.Clear();
        }

        private void loadLogos()
        {
            AnimationTimer.Stop();
            elapsedTime = settingsINI.GeneralAnimation.delayBetweenAnimations * 1000;

            closeAllLogos();
            imageBitmapsIndex = 0;

            if (imageBitmaps == null || imageBitmaps.Count <= 0)
                return;

            switch (settingsINI.LogoProperties.multiMonitorDisplayMode)
            {
                case MultiMonitorDisplayModes.TreatMonitorsAsOneScreen:
                    switch (settingsINI.LogoProperties.displayLocation)
                    {
                        case LocationTypes.Centre:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point((SystemInformation.VirtualScreen.Right - SystemInformation.VirtualScreen.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                    (SystemInformation.VirtualScreen.Bottom - SystemInformation.VirtualScreen.Top) / 2 - imageBitmaps[0].Height / 2 + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.BottomLeftCorner:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(SystemInformation.VirtualScreen.Left + settingsINI.LogoProperties.xOffset,
                                    SystemInformation.VirtualScreen.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.BottomMiddle:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point((SystemInformation.VirtualScreen.Right - SystemInformation.VirtualScreen.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                    SystemInformation.VirtualScreen.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.BottomRightCorner:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(SystemInformation.VirtualScreen.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                    SystemInformation.VirtualScreen.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.LeftMiddle:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(SystemInformation.VirtualScreen.Left + settingsINI.LogoProperties.xOffset,
                                    (SystemInformation.VirtualScreen.Bottom - SystemInformation.VirtualScreen.Top)/2 - imageBitmaps[0].Height/2 + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.CustomPosition:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(settingsINI.LogoProperties.xOffset, settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.TopLeftCorner:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(SystemInformation.VirtualScreen.Left + settingsINI.LogoProperties.xOffset,
                                    SystemInformation.VirtualScreen.Top + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.TopMiddle:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point((SystemInformation.VirtualScreen.Right - SystemInformation.VirtualScreen.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                    SystemInformation.VirtualScreen.Top + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.TopRightCorner:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(SystemInformation.VirtualScreen.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                    SystemInformation.VirtualScreen.Top + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.RightMiddle:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(SystemInformation.VirtualScreen.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                    (SystemInformation.VirtualScreen.Bottom - SystemInformation.VirtualScreen.Top)/2 - imageBitmaps[0].Height / 2 + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        default:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(SystemInformation.VirtualScreen.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                    SystemInformation.VirtualScreen.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                    }
                    break;
                case MultiMonitorDisplayModes.DisplayOnPrimaryOnly:
                    switch (settingsINI.LogoProperties.displayLocation)
                    {
                        case LocationTypes.Centre:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point((Screen.PrimaryScreen.Bounds.Right - Screen.PrimaryScreen.Bounds.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                    (Screen.PrimaryScreen.Bounds.Bottom - Screen.PrimaryScreen.Bounds.Top) / 2 - imageBitmaps[0].Height / 2 + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.BottomLeftCorner:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(Screen.PrimaryScreen.Bounds.Left + settingsINI.LogoProperties.xOffset,
                                    Screen.PrimaryScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.BottomMiddle:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point((Screen.PrimaryScreen.Bounds.Right - Screen.PrimaryScreen.Bounds.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                    Screen.PrimaryScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.BottomRightCorner:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(Screen.PrimaryScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                    Screen.PrimaryScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.LeftMiddle:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(Screen.PrimaryScreen.Bounds.Left + settingsINI.LogoProperties.xOffset,
                                    (Screen.PrimaryScreen.Bounds.Bottom - Screen.PrimaryScreen.Bounds.Top)/2 - imageBitmaps[0].Height/2 + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.CustomPosition:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(settingsINI.LogoProperties.xOffset, settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.TopLeftCorner:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(Screen.PrimaryScreen.Bounds.Left + settingsINI.LogoProperties.xOffset,
                                    Screen.PrimaryScreen.Bounds.Top + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.TopMiddle:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point((Screen.PrimaryScreen.Bounds.Right - Screen.PrimaryScreen.Bounds.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                    Screen.PrimaryScreen.Bounds.Top + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.TopRightCorner:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(Screen.PrimaryScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                    Screen.PrimaryScreen.Bounds.Top + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        case LocationTypes.RightMiddle:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(Screen.PrimaryScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                    (Screen.PrimaryScreen.Bounds.Bottom - Screen.PrimaryScreen.Bounds.Top)/2 - imageBitmaps[0].Height / 2 + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                        default:
                            allLogos.Add(new LogoObject(imageBitmaps[0], 
                                new Point(Screen.PrimaryScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                    Screen.PrimaryScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                            break;
                    }
                    break;
                case MultiMonitorDisplayModes.DisplayOnAllButPrimary:
                    foreach (var aScreen in Screen.AllScreens)
                    {
                        if (!aScreen.Primary)
                        {
                            switch (settingsINI.LogoProperties.displayLocation)
                            {
                                case LocationTypes.Centre:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point((aScreen.Bounds.Right - aScreen.Bounds.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                            (aScreen.Bounds.Bottom - aScreen.Bounds.Top) / 2 - imageBitmaps[0].Height / 2 + settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                                case LocationTypes.BottomLeftCorner:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point(aScreen.Bounds.Left + settingsINI.LogoProperties.xOffset,
                                            aScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                                case LocationTypes.BottomMiddle:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point((aScreen.Bounds.Right - aScreen.Bounds.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                            aScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                                case LocationTypes.BottomRightCorner:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point(aScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                            aScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                                case LocationTypes.LeftMiddle:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point(aScreen.Bounds.Left + settingsINI.LogoProperties.xOffset,
                                            (aScreen.Bounds.Bottom - aScreen.Bounds.Top) / 2 - imageBitmaps[0].Height / 2 + settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                                case LocationTypes.CustomPosition:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point(settingsINI.LogoProperties.xOffset, settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                                case LocationTypes.TopLeftCorner:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point(aScreen.Bounds.Left + settingsINI.LogoProperties.xOffset,
                                            aScreen.Bounds.Top + settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                                case LocationTypes.TopMiddle:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point((aScreen.Bounds.Right - aScreen.Bounds.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                            aScreen.Bounds.Top + settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                                case LocationTypes.TopRightCorner:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point(aScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                            aScreen.Bounds.Top + settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                                case LocationTypes.RightMiddle:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point(aScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                            (aScreen.Bounds.Bottom - aScreen.Bounds.Top) / 2 - imageBitmaps[0].Height / 2 + settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                                default:
                                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                                        new Point(aScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                            aScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                    break;
                            }
                        }
                    }
                    break;
                case MultiMonitorDisplayModes.AllSame:
                    foreach (var aScreen in Screen.AllScreens)
                    {
                        switch (settingsINI.LogoProperties.displayLocation)
                        {
                            case LocationTypes.Centre:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point((aScreen.Bounds.Right - aScreen.Bounds.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                        (aScreen.Bounds.Bottom - aScreen.Bounds.Top) / 2 - imageBitmaps[0].Height / 2 + settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                            case LocationTypes.BottomLeftCorner:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point(aScreen.Bounds.Left + settingsINI.LogoProperties.xOffset,
                                        aScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                            case LocationTypes.BottomMiddle:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point((aScreen.Bounds.Right - aScreen.Bounds.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                        aScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                            case LocationTypes.BottomRightCorner:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point(aScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                        aScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                            case LocationTypes.LeftMiddle:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point(aScreen.Bounds.Left + settingsINI.LogoProperties.xOffset,
                                        (aScreen.Bounds.Bottom - aScreen.Bounds.Top) / 2 - imageBitmaps[0].Height / 2 + settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                            case LocationTypes.CustomPosition:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point(settingsINI.LogoProperties.xOffset, settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                            case LocationTypes.TopLeftCorner:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point(aScreen.Bounds.Left + settingsINI.LogoProperties.xOffset,
                                        aScreen.Bounds.Top + settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                            case LocationTypes.TopMiddle:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point((aScreen.Bounds.Right - aScreen.Bounds.Left) / 2 - imageBitmaps[0].Width / 2 + settingsINI.LogoProperties.xOffset,
                                        aScreen.Bounds.Top + settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                            case LocationTypes.TopRightCorner:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point(aScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                        aScreen.Bounds.Top + settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                            case LocationTypes.RightMiddle:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point(aScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                        (aScreen.Bounds.Bottom - aScreen.Bounds.Top) / 2 - imageBitmaps[0].Height / 2 + settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                            default:
                                allLogos.Add(new LogoObject(imageBitmaps[0], 
                                    new Point(aScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                                        aScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                                    settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                                break;
                        }                 
                    }
                    break;
                default:
                    allLogos.Add(new LogoObject(imageBitmaps[0], 
                        new Point(Screen.PrimaryScreen.Bounds.Right - imageBitmaps[0].Width + settingsINI.LogoProperties.xOffset,
                            Screen.PrimaryScreen.Bounds.Bottom - imageBitmaps[0].Height + settingsINI.LogoProperties.yOffset),
                        settingsINI.LogoProperties.defaultOpacity, settingsINI.LogoProperties.windowLevel));
                    break;
            }

            hideLogosToolStripMenuItem.Checked = false;

            // Set whether the logos respond to input
            for (var i = 0; allLogos != null && i < allLogos.Count; i++)
            {
                try
                {
                    allLogos[i].SetTransparencyToInput(settingsINI.FolderPaths.useAsDropFolder);
                }
                catch (Exception)
                { }
            }

            // initialize the animation
            if (imageBitmaps != null && imageBitmaps.Count > 1 && settingsINI.GeneralAnimation.framesPerSecond > 0)
            {
                AnimationTimer.Stop();
                elapsedTime = 0;
                AnimationTimer.Interval = 1000 / settingsINI.GeneralAnimation.framesPerSecond;
                AnimationTimer.Start();
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (elapsedTime < settingsINI.GeneralAnimation.delayBetweenAnimations * 1000)
            {
                elapsedTime += AnimationTimer.Interval;
                return;
            }

            if (++imageBitmapsIndex < imageBitmaps.Count)
            {
                // just need to updateLogoGraphics
            }
            else
            {
                imageBitmapsIndex = 0;
                elapsedTime = 0;
            }

            if (isUpdatingLogoGraphics == false)
            {
                isUpdatingLogoGraphics = true;
                ThreadPool.QueueUserWorkItem(updateLogoGraphics);
            }
        }

        private void updateLogoGraphics(object o)
        {
            try
            {
               // This method is unsafe for multi-threaded coded
                for (var i = 0; i < allLogos.Count; i++)
                {
                    allLogos[i].SetBitmap(true, imageBitmaps[imageBitmapsIndex], true, (byte)settingsINI.LogoProperties.defaultOpacity, false, 0, 0);
                }
            }
            catch (Exception)
            { }

            isUpdatingLogoGraphics = false;
        }

        private void window_ForegroundChanged(IntPtr hWnd)
        {
            if (settingsINI.LogoProperties.windowLevel == WindowLevelTypes.AlwaysOnBottom || settingsINI.LogoProperties.windowLevel == WindowLevelTypes.Topmost)
            {
                for (var i = 0; allLogos != null && i < allLogos.Count; i++)
                {
                    try
                    {
                        allLogos[i].SetZLevel(settingsINI.LogoProperties.windowLevel);
                    }
                    catch (Exception)
                    { }
                }
            }
        }

        #endregion

        #region Tray Icon Context Menu

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            settingsTabControl.Invalidate();
            Show();
            BringToFront();
        }

        private void MainFormTrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            settingsTabControl.Invalidate();
            Show();
            BringToFront();
        }

        public void helpAboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var HelpAbout = new AboutBox(this);
            HelpAbout.Show();
        }

        public void dropFolderModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            useAsDropFolderCheckBox.Checked = dropFolderModeToolStripMenuItem.Checked;
        }

        public bool useAsDropFolderCheckBoxChecked
        {
            get => useAsDropFolderCheckBox.Checked;
            set => useAsDropFolderCheckBox.Checked = value;
        }

        public bool disableMovementCheckBoxChecked
        {
            get => disableLogoMovementCB.Checked;
            set => disableLogoMovementCB.Checked = value;
        }

        public void hideLogosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hideLogosToolStripMenuItem.Checked)
            {
                AnimationTimer.Stop();
                elapsedTime = 0;
                hideAllLogos();
            }
            else
            {
                AnimationTimer.Stop();
                elapsedTime = 0;

                if (settingsINI.GeneralAnimation.framesPerSecond > 0 && imageBitmaps.Count > 1)
                {
                    AnimationTimer.Interval = 1000 / settingsINI.GeneralAnimation.framesPerSecond;
                    AnimationTimer.Start();
                }
                showAllLogos();
            }
        }

        public void disableMovementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingsINI.LogoProperties.disableMovement = disableLogoMovementCB.Checked;
            settingsINI.SetEntry("LogoProperties", "disableMovement", disableLogoMovementCB.Checked.ToString());
        }

        public bool hideLogosToolStripMenuItemChecked
        {
            get => hideLogosToolStripMenuItem.Checked;
            set => hideLogosToolStripMenuItem.Checked = value;
        }

        private void hideAllLogos()
        {
            for (var i = 0; i < allLogos.Count; i++)
            {
                if (allLogos[i].InvokeRequired)
                {
                    var d = new ShowHideCallback(hideAllLogos);
                    allLogos[i].Invoke(d, new object[] { });
                }
                else
                {
                    AnimationTimer.Stop();
                    allLogos[i].Hide();
                }
            }
        }

        private void showAllLogos()
        {
            for (var i = 0; i < allLogos.Count; i++)
            {
                if (allLogos[i].InvokeRequired)
                {
                    var d = new ShowHideCallback(showAllLogos);
                    allLogos[i].Invoke(d, new object[] { });
                }
                else
                {
                    allLogos[i].Show();
                    allLogos[i].WindowState = FormWindowState.Normal;
                    allLogos[i].Show();
                    allLogos[i].BringToFront();
                }
            }
        }

        private void MainFormContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            dropFolderModeToolStripMenuItem.Checked = useAsDropFolderCheckBox.Checked;
            disableMovementToolStripMenuItem.Checked = disableLogoMovementCB.Checked;
            if (useAsDropFolderCheckBox.Checked)
                disableMovementToolStripMenuItem.Enabled = true;
            else
                disableMovementToolStripMenuItem.Enabled = false;
            //if (AnimationTimer.Enabled == false)
            //    MemoryUtility.ClearUnusedMemory();
        }

        private void MainFormContextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (AnimationTimer.Enabled == false)
                MemoryUtility.ClearUnusedMemory();
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Finalizes things before closing.
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AnimationTimer.Stop();
            MainFormTrayIcon.Dispose();
            //Environment.Exit(0);
            //closeAllLogos();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Select(true, true);
                Hide();

                //if (AnimationTimer.Enabled == false)
                //    MemoryUtility.ClearUnusedMemory();
            }
        }

        #endregion

        #region Language Methods

        private void languageBrowseButton_Click(object sender, EventArgs e)
        {
            languageFileDialog.InitialDirectory = Application.StartupPath + Path.DirectorySeparatorChar + languagesDirectoryName;
            languageFileDialog.ShowDialog();
        }

        private void languageFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            string newLanguagePath;
            if (languageFileDialog.FileName.StartsWith(Application.StartupPath + Path.DirectorySeparatorChar))
            {
                newLanguagePath = @"." + languageFileDialog.FileName.Substring(Application.StartupPath.Length);
            }
            else
            {
                newLanguagePath = languageFileDialog.FileName;
            }

            settingsINI.SetEntry("Language", "path", newLanguagePath);

            loadLanguage();
        }

        private void loadLanguage()
        {
            #region Load Language Data from File

            var languagePath = settingsINI.GetEntry("Language", "path");

            if (languagePath == null || languagePath.Length <= 0)
            {
                if (Directory.Exists(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar + languagesDirectoryName) == false)
                {
                    try
                    {
                        Directory.CreateDirectory(Application.StartupPath + Path.DirectorySeparatorChar + systemFilesDirectoryName + Path.DirectorySeparatorChar + languagesDirectoryName);
                    }
                    catch (Exception)
                    { }
                }

                settingsINI.SetEntry("Language", "path", $@".{Path.DirectorySeparatorChar}{systemFilesDirectoryName}{Path.DirectorySeparatorChar}{languagesDirectoryName}{Path.DirectorySeparatorChar}English.ini");
                language = new LanguageLoader.LanguageLoader($@"{Application.StartupPath}{Path.DirectorySeparatorChar}{systemFilesDirectoryName}{Path.DirectorySeparatorChar}{languagesDirectoryName}{Path.DirectorySeparatorChar}English.ini");

                languageFilePathTextBox.Text = $@".{Path.DirectorySeparatorChar}{systemFilesDirectoryName}{Path.DirectorySeparatorChar}{languagesDirectoryName}{Path.DirectorySeparatorChar}English.ini";
            }
            else
            {
                languageFilePathTextBox.Text = languagePath;
                if (languagePath.StartsWith(@"." + Path.DirectorySeparatorChar))
                {
                    languagePath = Application.StartupPath + languagePath.Substring(1);
                }
                language = new LanguageLoader.LanguageLoader(languagePath);
            }

            if (LanguageFileIsValid())
                languageFilePathTextBox.BackColor = Color.LightGray;
            else
                languageFilePathTextBox.BackColor = Color.Red;

            #endregion

            Text = language.general.customDesktopLogo;
            MainFormTrayIcon.Text = language.general.customDesktopLogo;

            quitToolStripMenuItem.Text = language.mainContextMenu.quit;
            helpAboutToolStripMenuItem.Text = language.mainContextMenu.helpabout;
            hideLogosToolStripMenuItem.Text = language.mainContextMenu.hideLogo;
            settingsToolStripMenuItem.Text = language.mainContextMenu.settings;
            dropFolderModeToolStripMenuItem.Text = language.mainContextMenu.dropFolderMode;
            disableMovementToolStripMenuItem.Text = language.mainContextMenu.disableMovement;

            // Select Images tab
            selectImagesTabPage.Text = language.general.selectImages;
            changeImagesButton.Text = language.general.changeImageFolder;
            refreshImageListButton.Text = language.general.refreshImageList;
            selectImagesInstructionsLabel.Text = language.general.instructions + "\r\r" + language.general.instruction1 + "\r"
                + language.general.instruction2 + "\r" + language.general.instruction3 + "\r" + language.general.instruction4
                + "\r" + language.general.instruction5;
            helpAboutButton.Text = language.general.helpabout;

            // Location tab
            locationTabPage.Text = language.general.location;
            zLevelGroupBox.Text = language.general.zLevel;
            normalRadioButton.Text = language.general.normal;
            topmostRadioButton.Text = language.general.topmost;
            alwaysOnBottomRadioButton.Text = language.general.alwaysOnBottom;
            multiMonitorDisplayModsGroupBox.Text = language.general.multiMonitorDisplayModes;
            allSameRadioButton.Text = language.general.allSame;
            primaryOnlyRadioButton.Text = language.general.primaryOnly;
            allButPrimaryRadioButton.Text = language.general.allButPrimary;
            virtualMonitorRadioButton.Text = language.general.virtualMonitor;

            displayLocationGroupBox.Text = language.general.displayLocation;
            centreRadioButton.Text = language.general.centre;
            bottomLeftRadioButton.Text = language.general.bottomLeft;
            bottomMiddleRadioButton.Text = language.general.bottomMiddle;
            bottomRightRadioButton.Text = language.general.bottomRight;
            leftMiddleRadioButton.Text = language.general.leftMiddle;
            displayAtLocationOffsetRadioButton.Text = language.general.displayAtLocationOffset;
            topLeftRadioButton.Text = language.general.topLeft;
            topMiddleRadioButton.Text = language.general.topMiddle;
            topRightRadioButton.Text = language.general.topRight;
            rightMiddleRadioButton.Text = language.general.rightMiddle;

            locationOffsetGroupBox.Text = language.general.locationOffset;
            xLabel.Text = language.general.xCoordinate;
            yLabel.Text = language.general.yCoordinate;
            
            // Size tab
            sizeTabPage.Text = language.general.size;
            scaleImagesFactorGroupBox.Text = language.general.scaleImagesByFactorOf;

            // Animation / Graphics tab
            animationTabPage.Text = language.general.animationAndGraphics;
            framesPerSecondGroupBox.Text = language.general.framesPerSecond;
            delayBetweenAnimationsGroupBox.Text = language.general.delayBetweenAnimationsSeconds;
            opacityGroupBox.Text = language.general.opacity;

            // Language tab
            LanguageTabPage.Text = language.general.language;
            languageGroupBox.Text = language.general.language;
            languageNameAndVersionLabel.Text = language.languageFile.languageName + " " + language.languageFile.languageFileVersion;
            languageAuthorLabel.Text = language.general.createdBy + " " + language.languageFile.fileAuthor;

            // Drop Folder tab
            dropFolderTab.Text = language.general.dropFolder;
            disableLogoMovementCB.Text = language.mainContextMenu.disableMovement;
            useAsDropFolderCheckBox.Text = language.general.useAsDropFolderWithExplanation;
            folderName.HeaderText = language.general.folderName;
            folderPath.HeaderText = language.general.folderPath;
            browseButton.HeaderText = language.general.browse;
        }

        // A string used to hold the version identifier of this build
        public readonly string programVersion = @"2.0";

        public bool LanguageFileIsValid()
        {
            string[] compatibleProgramVersions = { programVersion };
            var versionStated = language.languageFile.intendedForProgramVersion.Trim().ToUpper();
            foreach (var compareVersion in compatibleProgramVersions)
            {
                if (compareVersion.Trim().ToUpper() == versionStated)
                    return true;
            }

            return false;
        }

        #endregion

        #region Change Image List

        private void changeImagesButton_Click(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (TargetFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string NewFilePath;
                if (TargetFolderBrowserDialog.SelectedPath.StartsWith(Application.StartupPath + Path.DirectorySeparatorChar))
                {
                    NewFilePath = @"." + TargetFolderBrowserDialog.SelectedPath.Substring(Application.StartupPath.Length);
                }
                else
                {
                    NewFilePath = TargetFolderBrowserDialog.SelectedPath;
                }

                settingsINI.SetEntry("LogoProperties", "path", NewFilePath);
                settingsINI.LogoProperties.path = NewFilePath;

                loadImageList();
            }
        }

        private void refreshImageListButton_Click(object sender, EventArgs e)
        {
            AnimationTimer.Stop();
            elapsedTime = 0;
            loadImageList();
        }

        private void helpAboutButton_Click(object sender, EventArgs e)
        {
            var HelpAbout = new AboutBox(this);
            HelpAbout.Show();
        }

        #endregion

        #region Window Level and Multi-Monitor Display Modes

        private void topmostRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (topmostRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "windowLevel", WindowLevelTypes.Topmost.ToString());
            settingsINI.LogoProperties.windowLevel = WindowLevelTypes.Topmost;

            loadLogos();
        }

        private void alwaysOnBottomRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (alwaysOnBottomRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "windowLevel", WindowLevelTypes.AlwaysOnBottom.ToString());
            settingsINI.LogoProperties.windowLevel = WindowLevelTypes.AlwaysOnBottom;

            loadLogos();
        }

        private void normalRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (normalRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "windowLevel", WindowLevelTypes.Normal.ToString());
            settingsINI.LogoProperties.windowLevel = WindowLevelTypes.Normal;

            loadLogos();
        }

        private void allSameRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (allSameRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "multiMonitorDisplayMode", MultiMonitorDisplayModes.AllSame.ToString());
            settingsINI.LogoProperties.multiMonitorDisplayMode = MultiMonitorDisplayModes.AllSame;

            loadLogos();
        }

        private void allButPrimaryRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (allButPrimaryRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "multiMonitorDisplayMode", MultiMonitorDisplayModes.DisplayOnAllButPrimary.ToString());
            settingsINI.LogoProperties.multiMonitorDisplayMode = MultiMonitorDisplayModes.DisplayOnAllButPrimary;

            loadLogos();
        }

        private void primaryOnlyRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (primaryOnlyRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "multiMonitorDisplayMode", MultiMonitorDisplayModes.DisplayOnPrimaryOnly.ToString());
            settingsINI.LogoProperties.multiMonitorDisplayMode = MultiMonitorDisplayModes.DisplayOnPrimaryOnly;

            loadLogos();
        }

        private void virtualMonitorRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (virtualMonitorRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "multiMonitorDisplayMode", MultiMonitorDisplayModes.TreatMonitorsAsOneScreen.ToString());
            settingsINI.LogoProperties.multiMonitorDisplayMode = MultiMonitorDisplayModes.TreatMonitorsAsOneScreen;

            loadLogos();
        }

        #endregion

        #region Display Location Radio Buttons

        private void centreRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (centreRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "displayLocation", LocationTypes.Centre.ToString());
            settingsINI.LogoProperties.displayLocation = LocationTypes.Centre;

            loadLogos();
        }

        private void bottomLeftRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (bottomLeftRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "displayLocation", LocationTypes.BottomLeftCorner.ToString());
            settingsINI.LogoProperties.displayLocation = LocationTypes.BottomLeftCorner;

            loadLogos();
        }

        private void bottomMiddleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (bottomMiddleRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "displayLocation", LocationTypes.BottomMiddle.ToString());
            settingsINI.LogoProperties.displayLocation = LocationTypes.BottomMiddle;

            loadLogos();
        }

        private void bottomRightRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (bottomRightRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "displayLocation", LocationTypes.BottomRightCorner.ToString());
            settingsINI.LogoProperties.displayLocation = LocationTypes.BottomRightCorner;

            loadLogos();
        }

        private void leftMiddleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (leftMiddleRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "displayLocation", LocationTypes.LeftMiddle.ToString());
            settingsINI.LogoProperties.displayLocation = LocationTypes.LeftMiddle;

            loadLogos();
        }

        private void displayAtLocationOffsetRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (displayAtLocationOffsetRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "displayLocation", LocationTypes.CustomPosition.ToString());
            settingsINI.LogoProperties.displayLocation = LocationTypes.CustomPosition;

            loadLogos();
        }

        private void topLeftRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (topLeftRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "displayLocation", LocationTypes.TopLeftCorner.ToString());
            settingsINI.LogoProperties.displayLocation = LocationTypes.TopLeftCorner;

            loadLogos();
        }

        private void topMiddleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (topMiddleRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "displayLocation", LocationTypes.TopMiddle.ToString());
            settingsINI.LogoProperties.displayLocation = LocationTypes.TopMiddle;

            loadLogos();
        }

        private void topRightRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (topRightRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "displayLocation", LocationTypes.TopRightCorner.ToString());
            settingsINI.LogoProperties.displayLocation = LocationTypes.TopRightCorner;

            loadLogos();
        }

        private void rightMiddleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (rightMiddleRadioButton.Checked == false)
                return;

            settingsINI.SetEntry("LogoProperties", "displayLocation", LocationTypes.RightMiddle.ToString());
            settingsINI.LogoProperties.displayLocation = LocationTypes.RightMiddle;

            loadLogos();
        }

        private void xOffsetNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            settingsINI.SetEntry("LogoProperties", "xOffset", xOffsetNumericUpDown.Value.ToString());
            settingsINI.LogoProperties.xOffset = (int)xOffsetNumericUpDown.Value;

            loadLogos();
        }

        private void yOffsetNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            settingsINI.SetEntry("LogoProperties", "yOffset", yOffsetNumericUpDown.Value.ToString());
            settingsINI.LogoProperties.yOffset = (int)yOffsetNumericUpDown.Value;

            loadLogos();
        }

        #endregion

        #region Animation / Graphics Settings

        private void delayBetweenAnimationsTrackBar_Scroll(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            settingsINI.SetEntry("GeneralAnimation", "delayBetweenAnimations", delayBetweenAnimationsTrackBar.Value.ToString());
            settingsINI.GeneralAnimation.delayBetweenAnimations = delayBetweenAnimationsTrackBar.Value;
            delayBetweenAnimationsValueLabel.Text = delayBetweenAnimationsTrackBar.Value.ToString();
        }

        private void fpsTrackBar_Scroll(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            settingsINI.SetEntry("GeneralAnimation", "framesPerSecond", fpsTrackBar.Value.ToString());
            settingsINI.GeneralAnimation.framesPerSecond = fpsTrackBar.Value;
            fpsValueLabel.Text = fpsTrackBar.Value.ToString();

            if (settingsINI.GeneralAnimation.framesPerSecond <= 0 || imageBitmaps.Count <= 0)
            {
                AnimationTimer.Stop();
                elapsedTime = settingsINI.GeneralAnimation.delayBetweenAnimations * 1000;
            }
            else
            {
                AnimationTimer.Stop();
                elapsedTime = 0;
                AnimationTimer.Interval = 1000 / settingsINI.GeneralAnimation.framesPerSecond;
                AnimationTimer.Start();
            }
        }

        private void scaleImageFactorTrackBar_Scroll(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            settingsINI.SetEntry("LogoProperties", "scaleImagesFactor", scaleImageFactorTrackBar.Value.ToString());
            settingsINI.LogoProperties.scaleImagesFactor = scaleImageFactorTrackBar.Value;
            scaleImageFactorValueLabel.Text = ((settingsINI.LogoProperties.scaleImagesFactor + 100) / 100.0).ToString();

            scaleImageFactorChanged = true;
        }

        private void scaleImageFactorTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (loaded == false)
                return;

            if (scaleImageFactorChanged)
            {
                scaleImageFactorChanged = false;
                loadImageList();
            }
        }

        private void scaleImageFactorTrackBar_Leave(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            if (scaleImageFactorChanged)
            {
                scaleImageFactorChanged = false;
                loadImageList();
            }
        }

        private void opacityTrackBar_Scroll(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            settingsINI.SetEntry("LogoProperties", "defaultOpacity", opacityTrackBar.Value.ToString());
            settingsINI.LogoProperties.defaultOpacity = opacityTrackBar.Value;
            opacityValueLabel.Text = opacityTrackBar.Value.ToString();

            updateLogoOpacity();
        }

        private void updateLogoOpacity()
        {
            for (var i = 0; i < allLogos.Count; i++)
            {
                if (allLogos[i].InvokeRequired)
                {
                    var d = new UpdateSizeOpacityCallback(updateLogoOpacity);
                    allLogos[i].Invoke(d, new object[] { });
                }
                else
                {
                    try
                    {
                        allLogos[i].SetBitmap(false, null, true, (byte)settingsINI.LogoProperties.defaultOpacity, false, 0, 0);
                    }
                    catch (Exception)
                    { }
                }
            }
        }

        private void sizeTabPage_Click(object sender, EventArgs e)
        {
            Select(true, true);
        }

        #endregion

        public void filePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sendToolStripMenuItem = (ToolStripMenuItem)sender;

            if (Directory.Exists((string)sendToolStripMenuItem.Tag))
            {
                if (action == FileLocationActions.Open)
                {
                    try
                    {
                        var startInfo = new ProcessStartInfo((string)sendToolStripMenuItem.Tag);
                        //startInfo.WindowStyle = ProcessWindowStyle.Normal;
                        //startInfo.Arguments = argsParams;
                        startInfo.CreateNoWindow = false;
                        startInfo.ErrorDialog = true;
                        //try
                        //{
                        //    LogoObject senderObject = (LogoObject)sender;
                        //    startInfo.ErrorDialogParentHandle = senderObject.Parent.Handle;
                        //}
                        //catch (Exception)
                        //{
                        //    startInfo.ErrorDialogParentHandle = IntPtr.Zero;
                        //}

                        startInfo.UseShellExecute = true;

                        Process.Start(startInfo);
                    }
                    catch (Exception ee1)
                    {
                    }
                }
                else
                {
                    var performActionOnFiles = new moveCopyFiles(files, (string)sendToolStripMenuItem.Tag, action);
                    var performAction = new Thread(performActionOnFiles.performAction);
                    performAction.Start();
                }
            }
            else
            {
                MessageBox.Show((string)sendToolStripMenuItem.Tag + @" :" + language.errorMessages.folderDoesNotExist, language.errorMessages.customDesktopLogo);
            }
        }

        private void useAsDropFolderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            settingsINI.FolderPaths.useAsDropFolder = useAsDropFolderCheckBox.Checked;
            settingsINI.SetEntry("FolderPaths", "useAsDropFolder", useAsDropFolderCheckBox.Checked.ToString());

            // Set whether the logos respond to input
            for (var i = 0; allLogos != null && i < allLogos.Count; i++)
            {
                try
                {
                    allLogos[i].SetTransparencyToInput(settingsINI.FolderPaths.useAsDropFolder);
                }
                catch (Exception)
                { }
            }
        }

        private void disableLogoMovementCB_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == false)
                return;

            settingsINI.LogoProperties.disableMovement = disableLogoMovementCB.Checked;
            settingsINI.SetEntry("LogoProperties", "disableMovement", disableLogoMovementCB.Checked.ToString());
        }

        string[] files = { };
        FileLocationActions action = FileLocationActions.Copy;

        public void moveCopyToFilePathOptions(LogoObject sender, string[] filesList, FileLocationActions fileAction)
        {
            files = filesList;
            action = fileAction;

            populateFilePathsContextMenuStrip(sender.filePathsContextMenuStrip, fileAction);
            sender.Activate();
            sender.showContextMenu();
        }

        public void addTargetFolder(string[] newTargetFolders)
        {
            Show();
            WindowState = FormWindowState.Normal;
            settingsTabControl.Invalidate();
            settingsTabControl.SelectedTab = dropFolderTab;
            BringToFront();

            var index = 0;

            while (index < newTargetFolders.Length && !Directory.Exists(newTargetFolders[index]))
            {
                index++;
            }

            for (var i = 0; i < filePathsDataGridView.RowCount && index < newTargetFolders.Length; i++)
            {
                if ((filePathsDataGridView[0, i].Value == null || filePathsDataGridView[0, i].Value.ToString().Length <= 0) &&
                    (filePathsDataGridView[1, i].Value == null || filePathsDataGridView[1, i].Value.ToString().Length <= 0))
                {
                    filePathsDataGridView[1, i].Value = newTargetFolders[index];
                    var e = new DataGridViewCellEventArgs(1, i);
                    filePathsDataGridView_RowValidated(this, e);
                    index++;

                    while (index < newTargetFolders.Length && !Directory.Exists(newTargetFolders[index]))
                    {
                        index++;
                    }

                    if (index >= newTargetFolders.Length)
                    {                      
                        filePathsDataGridView[1, i].Selected = true;   
                        break;
                    }
                }
            }
        }

        private void filePathsDataGridView_RowLeave(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void filePathsContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            populateFilePathsContextMenuStrip(filePathsContextMenuStrip, action);
        }

        public void populateFilePathsContextMenuStrip(ContextMenuStrip aFilePathsContextMenuStrip, FileLocationActions fileAction)
        {
            aFilePathsContextMenuStrip.Items.Clear();

            var copyMoveToolStripMenuItem = new ToolStripMenuItem();
            copyMoveToolStripMenuItem.Name = "newToolStripMenuItem";
            copyMoveToolStripMenuItem.Size = new Size(152, 22);

            if (action == FileLocationActions.Copy)
                copyMoveToolStripMenuItem.Text = @"Copy To";
            else if (action == FileLocationActions.Move)
                copyMoveToolStripMenuItem.Text = @"Move To";
            else
                copyMoveToolStripMenuItem.Text = @"Open";

            copyMoveToolStripMenuItem.Enabled = false;
            copyMoveToolStripMenuItem.Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);

            aFilePathsContextMenuStrip.Items.Add(copyMoveToolStripMenuItem);


            for (var i = 0; i < settingsINI.FolderPaths.folderPaths.Count; i++)
            {
                var folderPath = settingsINI.FolderPaths.folderPaths[i].path;
                var displayName = settingsINI.FolderPaths.folderPaths[i].displayName;

                if (folderPath != null && folderPath.Length > 0)
                {
                    var newToolStripMenuItem = new ToolStripMenuItem();
                    newToolStripMenuItem.Name = "newToolStripMenuItem";
                    newToolStripMenuItem.Size = new Size(152, 22);

                    if (displayName != null && displayName.Length > 0)
                        newToolStripMenuItem.Text = displayName;
                    else
                        newToolStripMenuItem.Text = folderPath;

                    newToolStripMenuItem.Tag = folderPath;

                    newToolStripMenuItem.Click += filePathToolStripMenuItem_Click;
                    aFilePathsContextMenuStrip.Items.Add(newToolStripMenuItem);
                }
            }
        }

        private void filePathsDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                folderPathsFolderBrowserDialog.SelectedPath = (string)filePathsDataGridView[1, e.RowIndex].Value;

                if (folderPathsFolderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    filePathsDataGridView[1, e.RowIndex].Value = folderPathsFolderBrowserDialog.SelectedPath;

                    filePathsDataGridView_RowValidated(sender, e);

                    //String theSection = @"FolderPaths";
                    //settingsINI.SetEntry(theSection, e.RowIndex.ToString() + @"-FolderPath", filePathsDataGridView[1, e.RowIndex].Value.ToString());
                    //settingsINI.SetEntry(theSection, e.RowIndex.ToString() + @"-DisplayName", filePathsDataGridView[0, e.RowIndex].Value.ToString());

                    //SettingsInformation.FolderPathItem newFolderPathItem = new SettingsInformation.FolderPathItem();
                    //newFolderPathItem.path = settingsINI.GetEntry(theSection, e.RowIndex.ToString() + @"-FolderPath");
                    //newFolderPathItem.displayName = settingsINI.GetEntry(theSection, e.RowIndex.ToString() + @"-DisplayName");
                    //settingsINI.FolderPaths.folderPaths[e.RowIndex] = newFolderPathItem;
                }
            }
        }

        private void filePathsDataGridView_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (filePathsDataGridView[0, e.RowIndex].Value == null)
                filePathsDataGridView[0, e.RowIndex].Value = "";

            if (filePathsDataGridView[1, e.RowIndex].Value == null)
                filePathsDataGridView[1, e.RowIndex].Value = "";

            //Console.WriteLine("Row Index: " + e.RowIndex + filePathsDataGridView[0, e.RowIndex].Value.ToString());

            var theSection = @"FolderPaths";
            settingsINI.SetEntry(theSection, e.RowIndex + @"-FolderPath", filePathsDataGridView[1, e.RowIndex].Value.ToString());
            settingsINI.SetEntry(theSection, e.RowIndex + @"-DisplayName", filePathsDataGridView[0, e.RowIndex].Value.ToString());

            var newFolderPathItem = new SettingsInformation.FolderPathItem();
            newFolderPathItem.path = settingsINI.GetEntry(theSection, e.RowIndex + @"-FolderPath");
            newFolderPathItem.displayName = settingsINI.GetEntry(theSection, e.RowIndex + @"-DisplayName");
            settingsINI.FolderPaths.folderPaths[e.RowIndex] = newFolderPathItem;
        }
    }

    public class moveCopyFiles
    {
        string[] files = { };
        string destinationFolder = "";
        FileLocationActions action = FileLocationActions.Copy;

        public moveCopyFiles(string[] filesList, string destination, FileLocationActions fileAction)
        {
            files = (string[])filesList.Clone();
            destinationFolder = (string)destination.Clone();
            action = fileAction;
        }

        public void performAction()
        {
            if (destinationFolder[destinationFolder.Length - 1] != Path.DirectorySeparatorChar) destinationFolder += Path.DirectorySeparatorChar;

            if (action == FileLocationActions.Copy)
            {
                var filesString = new StringBuilder();
                for (var i = 0; i < files.Length; i++)
                {
                    filesString.Append(files[i] + '\0');
                    
                    // Old Method of copying - no dialogs
                    //if (System.IO.Directory.Exists(files[i]))
                    //{
                    //    copyDirectory(files[i], destinationFolder + Path.GetFileName(files[i]));
                    //}
                    //else if (System.IO.File.Exists(files[i]))
                    //{
                    //    try
                    //    {
                    //        System.IO.File.Copy(files[i], destinationFolder + Path.GetFileName(files[i]), true);
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        Debug.WriteLine("Error copying file: " + e.Message);
                    //    }
                    //}
                }
                shCopyFiles(filesString.ToString(), destinationFolder);
            }
            else
            {
                var filesString = new StringBuilder();
                for (var i = 0; i < files.Length; i++)
                {
                    filesString.Append(files[i] + '\0');
                    
                    // Old method of moving - no dialogs
                    //if (System.IO.Directory.Exists(files[i]))
                    //{
                    //    moveDirectory(files[i], destinationFolder + Path.GetFileName(files[i]));
                    //    System.IO.Directory.Delete(files[i]);
                    //}
                    //else if (System.IO.File.Exists(files[i]))
                    //{
                    //    try
                    //    {
                    //        System.IO.File.Copy(files[i], destinationFolder + Path.GetFileName(files[i]), true);
                    //        System.IO.File.Delete(files[i]);
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        Debug.WriteLine("Error moving file: " + e.Message);
                    //    }
                    //}
                }
                shMoveFiles(filesString.ToString(), destinationFolder);
            }
        }

        // Function to copy directory structure recursively
        public void copyDirectory(string Src, string Dst)
        {
            //Application.DoEvents();
            string[] Files;

            if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar) Dst += Path.DirectorySeparatorChar;
            if (!Directory.Exists(Dst))
            {
                try
                {
                    Directory.CreateDirectory(Dst);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error in CreateDirectory function (copyDirectory): " + e.Message);
                }
            }
            Files = Directory.GetFileSystemEntries(Src);
            foreach (var Element in Files)
            {
                // Sub directories
                if (Directory.Exists(Element))
                {
                    copyDirectory(Element, Dst + Path.GetFileName(Element));
                }
                // Files in directory
                else
                {
                    try
                    {
                        File.Copy(Element, Dst + Path.GetFileName(Element), true);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error in copyDirectory function: " + e.Message);
                    }
                }
            }
        }

        // Function to move directory structure recursively
        public void moveDirectory(string Src, string Dst)
        {
            //Application.DoEvents();
            string[] Files;

            if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar) Dst += Path.DirectorySeparatorChar;
            if (!Directory.Exists(Dst))
            {
                try
                {
                    Directory.CreateDirectory(Dst);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error in CreateDirectory function (copyDirectory): " + e.Message);
                }
            }
            Files = Directory.GetFileSystemEntries(Src);
            foreach (var Element in Files)
            {
                // Sub directories
                if (Directory.Exists(Element))
                {
                    moveDirectory(Element, Dst + Path.GetFileName(Element));
                }
                // Files in directory
                else
                {
                    try
                    {
                        File.Copy(Element, Dst + Path.GetFileName(Element), true);
                        File.Delete(Element);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error in moveDirectory function: " + e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Copies the files from source to target, showing the Progress Dialog
        /// </summary>
        /// <param name="sSource">Source from where the File(s) will be copied</param>
        /// <param name="sTarget">Target or Destination</param>
        /// <returns>True or False</returns>
        public void shCopyFiles(string sSource, string sTarget)
        {
            try
            {         
                var _ShFile = new Pinvoke.Win32.SHFILEOPSTRUCT();
                _ShFile.wFunc = Pinvoke.Win32.FO_Func.FO_COPY;
                _ShFile.fFlags = (ushort)Pinvoke.Win32.FO_Func.FOF_ALLOWUNDO;
                _ShFile.pFrom = sSource;
                _ShFile.pTo = sTarget;
                Pinvoke.Win32.SHFileOperation(ref _ShFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);            
            }         
        }

        /// <summary>
        /// Moves the files from source to target, showing the Progress Dialog
        /// </summary>
        /// <param name="sSource">Source from where the File(s) will be moved</param>
        /// <param name="sTarget">Target or Destination</param>
        /// <returns>True or False</returns>
        public void shMoveFiles(string sSource, string sTarget)
        {
            try
            {
                var _ShFile = new Pinvoke.Win32.SHFILEOPSTRUCT();
                _ShFile.wFunc = Pinvoke.Win32.FO_Func.FO_MOVE;
                _ShFile.fFlags = (ushort)Pinvoke.Win32.FO_Func.FOF_ALLOWUNDO;
                _ShFile.pFrom = sSource;
                _ShFile.pTo = sTarget;
                Pinvoke.Win32.SHFileOperation(ref _ShFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

}
