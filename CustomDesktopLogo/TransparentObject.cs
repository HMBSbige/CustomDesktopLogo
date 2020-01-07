// Custom Desktop Logo 2.0 - By: 2008 Eric Wong
// September 20th, 2008
// Custom Desktop Logo is open source software licensed under GNU GENERAL PUBLIC LICENSE V3. 
// Use it as you wish, but you must share your source code under the terms of use of the license.

// Custom Desktop Logo allows you to create custom static and animated logos from PNG images.

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

// This file contains the logo object class that we use to create logo objects with in the main form of this program.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Constants;
using PerPixelAlphaForms;

namespace CustomDesktopLogo
{
    public class LogoObject : LogoPerPixelAlphaForm
    {
        #region Windows Form Designer

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            var resources = new ComponentResourceManager(typeof(LogoObject));
            filePathsContextMenuStrip = new ContextMenuStrip(components);
            filePathsContextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // filePathsContextMenuStrip
            // 
            filePathsContextMenuStrip.Items.AddRange(new ToolStripItem[] {});
            filePathsContextMenuStrip.Opening += filePathsContextMenuStrip_Opening;
            filePathsContextMenuStrip.Name = "filePathsContextMenuStrip";
            filePathsContextMenuStrip.Size = new Size(153, 70);
            // 
            // TransparentObject
            // 
            
            ClientSize = new Size(1, 1);
            Name = "TransparentObject";
            Activated += LogoObject_Activated;
            MouseMove += LogoObject_MouseMove;
            ContextMenuStrip = filePathsContextMenuStrip;
            MouseUp += LogoObject_MouseUp;
            MouseDown += LogoObject_MouseDown;
            filePathsContextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
        }
  
        void filePathsContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
        }

        #endregion

        public ContextMenuStrip filePathsContextMenuStrip;

        #region Constructor

        /// <summary> 
        /// Create an alpha blended form with a transparent background.
        /// </summary>
        public LogoObject(Bitmap images, Point initialLocation, int initialOpacity, WindowLevelTypes initialLevel)
        {
            currWindowLevel = initialLevel;
            InitializeComponent();
            AllowDrop = true;
            Show();
            SetBitmap(true, images, true, (byte)initialOpacity, true, initialLocation.X, initialLocation.Y);
            SetZLevel(initialLevel);
            SetTransparencyToInput(MainForm.settingsINI.FolderPaths.useAsDropFolder);
        }

        public void SetZLevel(WindowLevelTypes level)
        {
            if (filePathsContextMenuStrip.Visible)
                return;
            if (level == WindowLevelTypes.AlwaysOnBottom)
                Pinvoke.Win32.SetWindowPos(Handle, (IntPtr)Pinvoke.Win32.HWND_BOTTOM, 0, 0, 0, 0, Pinvoke.Win32.SWP_NOMOVE | Pinvoke.Win32.SWP_NOSIZE
                    | Pinvoke.Win32.SWP_NOACTIVATE | Pinvoke.Win32.SWP_NOOWNERZORDER | Pinvoke.Win32.SWP_NOREDRAW | Pinvoke.Win32.SWP_NOSENDCHANGING);
            else if (level == WindowLevelTypes.Normal)
                Pinvoke.Win32.SetWindowPos(Handle, (IntPtr)Pinvoke.Win32.HWND_NOTOPMOST, 0, 0, 0, 0, Pinvoke.Win32.SWP_NOMOVE | Pinvoke.Win32.SWP_NOSIZE
                    | Pinvoke.Win32.SWP_NOACTIVATE | Pinvoke.Win32.SWP_NOOWNERZORDER | Pinvoke.Win32.SWP_NOREDRAW | Pinvoke.Win32.SWP_NOSENDCHANGING);
            else  // Topmost
                Pinvoke.Win32.SetWindowPos(Handle, (IntPtr)Pinvoke.Win32.HWND_TOPMOST, 0, 0, 0, 0, Pinvoke.Win32.SWP_NOMOVE | Pinvoke.Win32.SWP_NOSIZE
                    | Pinvoke.Win32.SWP_NOACTIVATE | Pinvoke.Win32.SWP_NOOWNERZORDER | Pinvoke.Win32.SWP_NOREDRAW | Pinvoke.Win32.SWP_NOSENDCHANGING);
        }

        public void SetTransparencyToInput(bool respond)
        {
            //long originalLong = Pinvoke.Win32.GetWindowLong(this.Handle, Constants.GWLConstants.GWL_EXSTYLE);

            if (respond)
            {
                Pinvoke.Win32.SetWindowLong(Handle, GWLConstants.GWL_EXSTYLE, WindowExStyles.WS_EX_LAYERED | WindowExStyles.WS_EX_TOOLWINDOW);
            }
            else
            {
                Pinvoke.Win32.SetWindowLong(Handle, GWLConstants.GWL_EXSTYLE, WindowExStyles.WS_EX_LAYERED | WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_TRANSPARENT);
            }
        }

        WindowLevelTypes currWindowLevel;
        void LogoObject_Activated(object sender, EventArgs e)
        {
            //SetZLevel(currWindowLevel);
        }

        FileLocationActions action = FileLocationActions.Copy;

        /// <summary> 
        /// Change the cursor when using Drag and Drop
        /// </summary>
        protected override void OnDragEnter(DragEventArgs e)
        {
            if (e.KeyState == 12) // Ctrl + Shift is down
            {
                e.Effect = DragDropEffects.Link;
                action = FileLocationActions.AddTargetFolder;
            }
            else if (e.KeyState == 8) // Ctrl key is down
            {
                e.Effect = DragDropEffects.Copy;
                action = FileLocationActions.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.Move;
                action = FileLocationActions.Move;
            }
        }

        /// <summary> 
        /// Add files to the dock using Drag and Drop
        /// </summary>
        protected override void OnDragDrop(DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                //Console.WriteLine(e.KeyState);
                if (e.KeyState == 12) // Ctrl + Shift is down
                {
                    MainForm.Instance.addTargetFolder(files);
                }
                else
                {
                    if (e.KeyState == 8)
                    {
                        action = FileLocationActions.Copy;
                    }
                    else
                    {
                        action = FileLocationActions.Move;
                    }
                    MainForm.Instance.moveCopyToFilePathOptions(this, files, action);
                }
            }
        }

        private void filePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aToolStripMenuItem = (ToolStripMenuItem)sender;

            switch ((string)aToolStripMenuItem.Tag)
            {
                case "DropFolderMode":
                    MainForm.Instance.useAsDropFolderCheckBoxChecked = aToolStripMenuItem.Checked;
                    break;
                case "DisableMovement":
                    MainForm.Instance.disableMovementCheckBoxChecked = aToolStripMenuItem.Checked;
                    MainForm.Instance.disableMovementToolStripMenuItem_Click(this, null);
                    break;
                case "HideLogos":
                    MainForm.Instance.hideLogosToolStripMenuItemChecked = aToolStripMenuItem.Checked;
                    MainForm.Instance.hideLogosToolStripMenuItem_Click(this, null);
                    break;
                case "Settings":
                    MainForm.Instance.settingsToolStripMenuItem_Click(this, null);
                    break;
                case "HelpAbout":
                    MainForm.Instance.helpAboutToolStripMenuItem_Click(this, null);
                    break;
                case "Quit":
                    MainForm.Instance.Close();
                    break;
            }
        }

        bool LeftMouseButtonDown;
        bool ThisObjectMovedWithLeftMouse;
        Point StoredMouseOffset = new Point(0, 0);
        Point previousMousePosition = new Point(0, 0);

        void LogoObject_MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine(MainForm.settingsINI.LogoProperties.disableMovement);
            if (LeftMouseButtonDown)
            {
                if (!MainForm.settingsINI.LogoProperties.disableMovement && (ThisObjectMovedWithLeftMouse || Math.Abs(Cursor.Position.X - previousMousePosition.X) > 5
                                                                                                          || Math.Abs(Cursor.Position.Y - previousMousePosition.Y) > 5))
                {
                    ThisObjectMovedWithLeftMouse = true;
                    SetBitmap(false, null, false, 255, true, Cursor.Position.X - StoredMouseOffset.X, Cursor.Position.Y - StoredMouseOffset.Y);
                }
            }
        }

        public void showContextMenu()
        {
            filePathsContextMenuStrip.Show(MousePosition.X, MousePosition.Y);
            filePathsContextMenuStrip.BringToFront();
        }

        void LogoObject_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (ThisObjectMovedWithLeftMouse == false)
                {
                    MainForm.Instance.moveCopyToFilePathOptions(this, null, FileLocationActions.Open);
                }
                LeftMouseButtonDown = false;
                ThisObjectMovedWithLeftMouse = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                filePathsContextMenuStrip.Items.Clear();

                var dropFolderModeToolStripMenuItem = new ToolStripMenuItem();
                dropFolderModeToolStripMenuItem.Name = "dropFolderModeToolStripMenuItem";
                dropFolderModeToolStripMenuItem.Size = new Size(152, 22);
                dropFolderModeToolStripMenuItem.Text = MainForm.language.mainContextMenu.dropFolderMode;
                dropFolderModeToolStripMenuItem.Tag = "DropFolderMode";
                dropFolderModeToolStripMenuItem.CheckOnClick = true;
                dropFolderModeToolStripMenuItem.Checked = MainForm.Instance.useAsDropFolderCheckBoxChecked;
                dropFolderModeToolStripMenuItem.Click += filePathToolStripMenuItem_Click;
                filePathsContextMenuStrip.Items.Add(dropFolderModeToolStripMenuItem);

                var disableMovementModeToolStripMenuItem = new ToolStripMenuItem();
                disableMovementModeToolStripMenuItem.Name = "disableMovementModeToolStripMenuItem";
                disableMovementModeToolStripMenuItem.Size = new Size(152, 22);
                disableMovementModeToolStripMenuItem.Text = MainForm.language.mainContextMenu.disableMovement;
                disableMovementModeToolStripMenuItem.Tag = "DisableMovement";
                if (MainForm.Instance.useAsDropFolderCheckBoxChecked)
                    disableMovementModeToolStripMenuItem.Enabled = true;
                else
                    disableMovementModeToolStripMenuItem.Enabled = false;
                disableMovementModeToolStripMenuItem.CheckOnClick = true;
                disableMovementModeToolStripMenuItem.Checked = MainForm.Instance.disableMovementCheckBoxChecked;
                disableMovementModeToolStripMenuItem.Click += filePathToolStripMenuItem_Click;
                filePathsContextMenuStrip.Items.Add(disableMovementModeToolStripMenuItem);

                var hideLogosToolStripMenuItem = new ToolStripMenuItem();
                hideLogosToolStripMenuItem.Name = "hideLogosToolStripMenuItem";
                hideLogosToolStripMenuItem.Size = new Size(152, 22);
                hideLogosToolStripMenuItem.Text = MainForm.language.mainContextMenu.hideLogo;
                hideLogosToolStripMenuItem.Tag = "HideLogos";
                hideLogosToolStripMenuItem.CheckOnClick = true;
                hideLogosToolStripMenuItem.Checked = MainForm.Instance.hideLogosToolStripMenuItemChecked;
                hideLogosToolStripMenuItem.Click += filePathToolStripMenuItem_Click;
                filePathsContextMenuStrip.Items.Add(hideLogosToolStripMenuItem);

                var settingsToolStripMenuItem = new ToolStripMenuItem();
                settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
                settingsToolStripMenuItem.Size = new Size(152, 22);
                settingsToolStripMenuItem.Text = MainForm.language.mainContextMenu.settings;
                settingsToolStripMenuItem.Tag = "Settings";
                settingsToolStripMenuItem.Click += filePathToolStripMenuItem_Click;
                filePathsContextMenuStrip.Items.Add(settingsToolStripMenuItem);

                var helpAboutToolStripMenuItem = new ToolStripMenuItem();
                helpAboutToolStripMenuItem.Name = "helpAboutToolStripMenuItem";
                helpAboutToolStripMenuItem.Size = new Size(152, 22);
                helpAboutToolStripMenuItem.Text = MainForm.language.mainContextMenu.helpabout;
                helpAboutToolStripMenuItem.Tag = "HelpAbout";
                helpAboutToolStripMenuItem.Click += filePathToolStripMenuItem_Click;
                filePathsContextMenuStrip.Items.Add(helpAboutToolStripMenuItem);

                var quitToolStripMenuItem = new ToolStripMenuItem();
                quitToolStripMenuItem.Name = "quitToolStripMenuItem";
                quitToolStripMenuItem.Size = new Size(152, 22);
                quitToolStripMenuItem.Text = MainForm.language.mainContextMenu.quit;
                quitToolStripMenuItem.Tag = "Quit";
                quitToolStripMenuItem.Click += filePathToolStripMenuItem_Click;
                filePathsContextMenuStrip.Items.Add(quitToolStripMenuItem);

                filePathsContextMenuStrip.Show();
                filePathsContextMenuStrip.Left = MousePosition.X;
                filePathsContextMenuStrip.Top = MousePosition.Y;
            }
        }

        void LogoObject_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                LeftMouseButtonDown = true;
                ThisObjectMovedWithLeftMouse = false;
                StoredMouseOffset = new Point(Cursor.Position.X - _Location.X, Cursor.Position.Y - _Location.Y);
                previousMousePosition = Cursor.Position;
            }
        }
        #endregion
    }

}
