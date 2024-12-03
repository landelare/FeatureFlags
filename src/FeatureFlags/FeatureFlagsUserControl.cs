﻿// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FeatureFlags
{
    internal partial class FeatureFlagsUserControl : UserControl
    {
        private readonly FeatureFlagsDataModel _dataModel;

        public FeatureFlagsUserControl(FeatureFlagsDataModel dataModel)
        {
            _dataModel = dataModel;
            InitializeComponent();
            warningIcon.Image = SystemIcons.Warning.ToBitmap();
        }

        public void Initialize()
        {
            allFeatureFlagsListBox.Items.Clear();
            foreach (var featureFlag in _dataModel.GetFlags())
            {
                allFeatureFlagsListBox.Items.Add(featureFlag, featureFlag.IsEnabled);
            }
        }

        public void WriteChanges()
        {
            for (int i = 0; i < allFeatureFlagsListBox.Items.Count; i++)
            {
                var featureName = allFeatureFlagsListBox.Items[i].ToString();
                bool currentSetting = _dataModel.IsFeatureEnabled(featureName);
                bool desiredSetting = allFeatureFlagsListBox.GetItemChecked(i);
                if (currentSetting != desiredSetting)
                {
                    _dataModel.EnableFeature(featureName, desiredSetting);
                }
            }
        }

        private void OnResetAllButtonClicked(object sender, EventArgs e)
        {
            for (int i = 0; i < allFeatureFlagsListBox.Items.Count; i++)
            {
                var featureName = allFeatureFlagsListBox.Items[i].ToString();
                allFeatureFlagsListBox.SetItemChecked(i, _dataModel.IsFeatureEnabledByDefault(featureName));
            }
        }

        private void AllFeatureFlagsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (allFeatureFlagsListBox.SelectedItem is FeatureFlag featureFlag)
            {
                descriptionLabel.Text = GetDescription(featureFlag) ?? "No description provided.";
            }
        }

        private string GetDescription(FeatureFlag featureFlag)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return featureFlag.TryParseDescriptionResourceId(out uint resourceId, out Guid packageGuid)
                && GetService(typeof(SVsShell)) is IVsShell shell
                && ErrorHandler.Succeeded(shell.LoadPackageString(ref packageGuid, resourceId, out string description))
                ? description
                : featureFlag.Description;
        }
    }
}
