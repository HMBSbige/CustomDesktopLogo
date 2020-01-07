// Custom Desktop Logo 1.0 - By: 2008 Eric Wong
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

using System;
using System.Reflection;
using System.Windows.Forms;

namespace CustomDesktopLogo
{
    partial class AboutBox : Form
    {
        MainForm theParent;

        public AboutBox(MainForm parent)
        {
            theParent = parent;
            InitializeComponent();
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string AssemblyDescription
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void AboutBox_Load(object sender, EventArgs e)
        {
            Text = MainForm.language.helpAbout.aboutWindowTitle;
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = string.Format("{0}", AssemblyVersion);
            labelCopyright.Text = AssemblyCopyright;

            linkLabelSupportForum.Text = MainForm.language.helpAbout.officialSupportForum;
            linkLabelOfficialWebsite.Text = MainForm.language.helpAbout.officialWebsite;
            donateLinkLabel.Text = MainForm.language.helpAbout.donateProjectDevelopment;
            linkLabelEmailAuthor.Text = MainForm.language.helpAbout.emailAuthor;

            programDescriptionLabel.Text = MainForm.language.helpAbout.programDescription;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkLabelSupportForum_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utils.Utils.OpenUrl("http://www.donationcoder.com/Forums/bb/index.php?board=240.0");
        }

        private void linkLabelOfficialWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
	        Utils.Utils.OpenUrl("http://customdesktoplogo.wikidot.com/");
        }

        private void linkLabelEmailAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
	        Utils.Utils.OpenUrl("mailto:VideoInPicture@gmail.com");
        }

        private void donateLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
	        Utils.Utils.OpenUrl("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=VideoInPicture%40gmail%2ecom&lc=CA&item_name=Window%20Custom%20Desktop%20Logo%20Development&currency_code=CAD&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted");
        }

        private void AboutBox_Activated(object sender, EventArgs e)
        {
            //MemoryUtility.ClearUnusedMemory();
        }

        private void AboutBox_FormClosed(object sender, FormClosedEventArgs e)
        {
            //MemoryUtility.ClearUnusedMemory();
        }

    }
}
