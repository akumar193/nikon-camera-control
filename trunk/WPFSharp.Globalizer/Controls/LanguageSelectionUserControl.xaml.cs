﻿#region License
/*
<Project> <Project Description>
Copyright (c) <Year>, <Owner>
All rights reserved.
 
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
 
1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.
3. Use of the source code or binaries for a competing project, whether open
   source or commercial, is prohibited unless permission is specifically
   granted under a separate license by <Owner>.
4. Source code enhancements or additions are the property of the author until
   the source code is contributed to this project. By contributing the source
   code to this project, the author immediately grants all rights to
   the contributed source code to <Owner>.
 
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System.Globalization;
using System.Windows.Markup;

namespace WPFSharp.Globalizer.Controls
{
    /// <summary>
    /// Interaction logic for LanguageSelectionControl.xaml
    /// </summary>
    public partial class LanguageSelectionUserControl
    {
        public LanguageSelectionUserControl()
        {
            InitializeComponent();
            LanguageSelectionComboBox.SelectedItem = CultureInfo.CurrentCulture.Name;
        }

        private void LanguageSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string lang = e.AddedItems[0].ToString();
            Language = XmlLanguage.GetLanguage(new CultureInfo(lang).IetfLanguageTag);
            GlobalizedApplication.Instance.GlobalizationManager.SwitchLanguage(lang);
        }
    }
}
