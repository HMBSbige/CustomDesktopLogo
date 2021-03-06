﻿// Circle Dock 0.9.2 - Copyright 2008 Eric Wong
// Circle Dock is open source software licensed under GNU GENERAL PUBLIC LICENSE V3. See included Licence.txt file.
// http://circledock.wikidot.com
// VideoInPicture@gmail.com

// This file contains the classes used to load the settings information for the dock using the .ini file format
// to allow for high legibility for the end user.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using AMS.Profile;
using CustomDesktopLogo;
using SettingsInformation; // Allows for .ini file manipulation

namespace SettingsLoader
{
    /// <summary>    
    /// Loads the global settings for the dock. Does not include the individual settings for the dock items.
    /// Uses the .ini file format.
    /// </summary>  
    public class SettingsLoader
    {
        #region Variables

        /// <summary>    
        /// Allows for .ini file manipulation.
        /// </summary>  
        private Profile SettingsINI;
        private string DataVerifier;

        public LogoProperties LogoProperties;
        public Language Language;
        public General General;
        public GeneralAnimation GeneralAnimation;
        public FolderPaths FolderPaths;

        #endregion

        #region Constructor

        /// <summary>        
        /// Loads the global system settings into memory and provides access to the read/write of them.
        /// The reading of the settings should be done using the structs instances declared in this class
        /// for faster access. Reading/writing to the .ini file all the time can lead to low performance.
        /// 
        /// This class is independent of the file version of the .ini file. Bounds checking and automatic repair are done.
        /// </summary>        
        /// <param name="FilePath">System path to the .ini file to where the global settings are stored.</param>        
        public SettingsLoader(string FilePath)
        {
            SettingsINI = new Ini(FilePath);

            //string[] sectionNames = SettingsINI.GetSectionNames();
            //for (int i = 0; i < sectionNames.Length; i++)
            //{
            //    Console.WriteLine("section " + i + " : '" + sectionNames[i] + "'");
            //}

            //string[] entryNames = SettingsINI.GetEntryNames("FolderPaths");
            //for (int i = 0; i < entryNames.Length; i++)
            //    Console.WriteLine("entry " + i + " : '" + entryNames[i] + "'");

            //InitializeSettingsInfoStructs();
            SettingsLoaderLoadData();
        }

        #endregion

        #region Methods

        /// <summary>        
        /// Initializes the structs used to store the settings data. Can be used to clear the settings from memory.
        /// </summary>        
        public void InitializeSettingsInfoStructs()
        {
            LogoProperties = new LogoProperties();
            Language = new Language();
            General = new General();
            GeneralAnimation = new GeneralAnimation();
            FolderPaths = new FolderPaths();
        }

        /// <summary>        
        /// Loads the global settings from the .ini file into the structs of this class. Can be used to refresh the settings
        /// data from the settings file into memory.
        /// </summary>  
        public void SettingsLoaderLoadData()
        {
            LoadFolderPaths();
            LoadLogoProperties();
            LoadLanguage();
            LoadGeneral();
            LoadGeneralAnimation();

        }

        /// <summary>        
        /// Retrieves the setting value at the given Section with the name of EntryName.
        /// </summary>        
        /// <param name="Section">The section in the .ini file to look at.</param>        
        /// <returns>A string representing the setting value or null if not found.</returns> 
        public string GetEntry(string Section, string EntryName)
        {
            return (string)SettingsINI.GetValue(Section, EntryName);
        }

        /// <summary>        
        /// Retrieves all the section names in the settings file.
        /// </summary>               
        /// <returns>A string array containing the section names null if not found.</returns> 
        public string[] GetSectionNames()
        {
            return SettingsINI.GetSectionNames();
        }

        /// <summary>        
        /// Retrieves the all the names of the settings entries under the given Section
        /// </summary>        
        /// <param name="Section">The section in the .ini file to look at.</param>        
        /// <returns>A string array containing all the entry names in the given section or null if not found.</returns> 
        public string[] GetEntryNames(string Section)
        {
            return SettingsINI.GetEntryNames(Section);
        }

        /// <summary>        
        /// Sets the value of a setting.
        /// </summary>        
        /// <param name="Section">The section in the .ini file to write to. It is created if not found.</param>  
        /// <param name="EntryName">The entry name to store the setting value under. It is created if not found.</param>
        /// <param name="Value">A string representing the value to be stored.</param>
        public void SetEntry(string Section, string EntryName, string Value)
        {
            SettingsINI.SetValue(Section, EntryName, Value);
        }

        /// <summary>        
        /// Removes an entry from the settings file.
        /// </summary>        
        /// <param name="Section">The section in the .ini file to search in.</param>  
        /// <param name="EntryName">The entry to remove.</param>
        public void RemoveEntry(string Section, string EntryName)
        {
            SettingsINI.RemoveEntry(Section, EntryName);
        }

        /// <summary>        
        /// Removes an entire section from the settings file, including the entries.
        /// </summary>        
        /// <param name="Section">The section in the .ini file to remove.</param>  
        public void RemoveSection(string Section)
        {
            SettingsINI.RemoveSection(Section);
        }

        #endregion

        #region Load Each Section for LanguageInformation

        private void LoadLogoProperties()
        {
            const string theSection = "LogoProperties";

            #region path

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "path");

            if (DataVerifier == null)
            {
                DataVerifier = @"";
                SetEntry(theSection, "path", DataVerifier);
            }

            LogoProperties.path = DataVerifier;

            #endregion

            #region defaultOpacity

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "defaultOpacity");

            if (DataVerifier == null)
            {
                DataVerifier = @"255";
                SetEntry(theSection, "defaultOpacity", DataVerifier);
            }

            try
            {
                LogoProperties.defaultOpacity = int.Parse(DataVerifier);
            }
            catch (Exception)
            {
                LogoProperties.defaultOpacity = 255;
                SetEntry(theSection, "defaultOpacity", @"255");
            }

            if (LogoProperties.defaultOpacity < 0)
            {
                LogoProperties.defaultOpacity = 0;
                SetEntry(theSection, "defaultOpacity", @"0");
            }
            else if (LogoProperties.defaultOpacity > 255)
            {
                LogoProperties.defaultOpacity = 255;
                SetEntry(theSection, "defaultOpacity", @"255");
            }

            #endregion

            #region scaleImagesFactor

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "scaleImagesFactor");

            if (DataVerifier == null)
            {
                DataVerifier = @"0";
                SetEntry(theSection, "scaleImagesFactor", DataVerifier);
            }

            try
            {
                LogoProperties.scaleImagesFactor = int.Parse(DataVerifier);
            }
            catch (Exception)
            {
                LogoProperties.scaleImagesFactor = 0;
                SetEntry(theSection, "scaleImagesFactor", @"0");
            }

            if (LogoProperties.scaleImagesFactor < -100)
            {
                LogoProperties.scaleImagesFactor = -100;
                SetEntry(theSection, "scaleImagesFactor", @"-100");
            }
            else if (LogoProperties.scaleImagesFactor > 100)
            {
                LogoProperties.scaleImagesFactor = 100;
                SetEntry(theSection, "scaleImagesFactor", @"100");
            }

            #endregion

            #region windowLevel

            var windowLevelConverter = TypeDescriptor.GetConverter(typeof(WindowLevelTypes));

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "windowLevel");

            if (DataVerifier == null)
            {
                DataVerifier = WindowLevelTypes.Topmost.ToString();
                SetEntry(theSection, "windowLevel", DataVerifier);
            }

            try
            {
                LogoProperties.windowLevel = (WindowLevelTypes)windowLevelConverter.ConvertFromString(DataVerifier);
            }
            catch (Exception)
            {
                LogoProperties.windowLevel = WindowLevelTypes.Topmost;
                SetEntry(theSection, "windowLevel", WindowLevelTypes.Topmost.ToString());
            }

            #endregion

            #region multiMonitorDisplayMode

            var multiMonitorDisplayModeConverter = TypeDescriptor.GetConverter(typeof(MultiMonitorDisplayModes));

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "multiMonitorDisplayMode");

            if (DataVerifier == null)
            {
                DataVerifier = MultiMonitorDisplayModes.AllSame.ToString();
                SetEntry(theSection, "multiMonitorDisplayMode", DataVerifier);
            }

            try
            {
                LogoProperties.multiMonitorDisplayMode = (MultiMonitorDisplayModes)multiMonitorDisplayModeConverter.ConvertFromString(DataVerifier);
            }
            catch (Exception)
            {
                LogoProperties.multiMonitorDisplayMode = MultiMonitorDisplayModes.AllSame;
                SetEntry(theSection, "multiMonitorDisplayMode", MultiMonitorDisplayModes.AllSame.ToString());
            }

            #endregion

            #region displayLocation

            var displayLocationConverter = TypeDescriptor.GetConverter(typeof(LocationTypes));

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "displayLocation");

            if (DataVerifier == null)
            {
                DataVerifier = LocationTypes.BottomRightCorner.ToString();
                SetEntry(theSection, "displayLocation", DataVerifier);
            }

            try
            {
                LogoProperties.displayLocation = (LocationTypes)displayLocationConverter.ConvertFromString(DataVerifier);
            }
            catch (Exception)
            {
                LogoProperties.displayLocation = LocationTypes.BottomRightCorner;
                SetEntry(theSection, "displayLocation", LocationTypes.BottomRightCorner.ToString());
            }

            #endregion

            #region xOffset

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "xOffset");

            if (DataVerifier == null)
            {
                DataVerifier = @"30";
                SetEntry(theSection, "xOffset", DataVerifier);
            }

            try
            {
                LogoProperties.xOffset = int.Parse(DataVerifier);
            }
            catch (Exception)
            {
                LogoProperties.xOffset = 30;
                SetEntry(theSection, "xOffset", @"30");
            }

            #endregion

            #region yOffset

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "YOffset");

            if (DataVerifier == null)
            {
                DataVerifier = @"30";
                SetEntry(theSection, "YOffset", DataVerifier);
            }

            try
            {
                LogoProperties.yOffset = int.Parse(DataVerifier);
            }
            catch (Exception)
            {
                LogoProperties.yOffset = 30;
                SetEntry(theSection, "YOffset", @"30");
            }

            #endregion

            #region disableMovement

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "disableMovement");

            if (DataVerifier == null)
            {
                DataVerifier = false.ToString();
                SetEntry(theSection, "disableMovement", DataVerifier);
            }

            try
            {
                LogoProperties.disableMovement = bool.Parse(DataVerifier);
            }
            catch (Exception)
            {
                LogoProperties.disableMovement = false;
                SetEntry(theSection, "disableMovement", false.ToString());
            }

            #endregion
        }


        private void LoadLanguage()
        {
            const string theSection = "Language";

            #region Path

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "path");

            if (DataVerifier == null)
            {
                DataVerifier = @"";
                SetEntry(theSection, "path", DataVerifier);
            }

            Language.path = DataVerifier;

            #endregion
        }


        private void LoadGeneral()
        {
            const string theSection = "General";
        }

        private void LoadGeneralAnimation()
        {
            const string theSection = "GeneralAnimation";

            #region framesPerSecond

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "framesPerSecond");

            if (DataVerifier == null)
            {
                DataVerifier = @"20";
                SetEntry(theSection, "framesPerSecond", DataVerifier);
            }

            try
            {
                GeneralAnimation.framesPerSecond = int.Parse(DataVerifier);
            }
            catch (Exception)
            {
                GeneralAnimation.framesPerSecond = 20;
                SetEntry(theSection, "framesPerSecond", @"20");
            }

            #endregion

            #region delayBetweenAnimations

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "delayBetweenAnimations");

            if (DataVerifier == null)
            {
                DataVerifier = @"30";
                SetEntry(theSection, "delayBetweenAnimations", DataVerifier);
            }

            try
            {
                GeneralAnimation.delayBetweenAnimations = int.Parse(DataVerifier);
            }
            catch (Exception)
            {
                GeneralAnimation.delayBetweenAnimations = 30;
                SetEntry(theSection, "delayBetweenAnimations", @"30");
            }

            #endregion
        }


        private void LoadFolderPaths()
        {
            const string theSection = "FolderPaths";
            FolderPaths.folderPaths = new List<FolderPathItem>();

            var numFolderPaths = 20;
            string DataVerifier;

            #region Use as drop folder

            DataVerifier = null;
            DataVerifier = GetEntry(theSection, "useAsDropFolder");

            if (DataVerifier == null)
            {
                DataVerifier = false.ToString();
                SetEntry(theSection, "useAsDropFolder", DataVerifier);
            }

            try
            {
                FolderPaths.useAsDropFolder = bool.Parse(DataVerifier);
            }
            catch (Exception)
            {
                FolderPaths.useAsDropFolder = false;
                SetEntry(theSection, "useAsDropFolder", false.ToString());
            }

            #endregion

            // Ensure data integrity
            if (!SettingsINI.HasSection(theSection))
            {
                for (var i = 0; i < numFolderPaths; i++)
                {
                    SetEntry(theSection, i + @"-FolderPath", "");
                    SetEntry(theSection, i + @"-DisplayName", "");
                }
            }
            else
            {
                for (var i = 0; i < numFolderPaths; i++)
                {
                    if (!SettingsINI.HasEntry(theSection, i + @"-FolderPath"))
                    {
                        SetEntry(theSection, i + @"-FolderPath", "");
                    }

                    if (!SettingsINI.HasEntry(theSection, i + @"-DisplayName"))
                    {
                        SetEntry(theSection, i + @"-DisplayName", "");
                    }
                }
            }


            for (var i = 0; i < numFolderPaths; i++)
            {
                var newFolderPathItem = new FolderPathItem();
                newFolderPathItem.path = GetEntry(theSection, i + @"-FolderPath");
                newFolderPathItem.displayName = GetEntry(theSection, i + @"-DisplayName");
                FolderPaths.folderPaths.Add(newFolderPathItem);
            }
            
        }


        #endregion
    }
}
