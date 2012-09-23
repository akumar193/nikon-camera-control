﻿#region License
/*
WPF Sharp Globalizer - A project deisgned to make localization and styling
                       easier by decoupling both process from the build.

Copyright (c) 2012, Rhyous.com
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
   granted under a separate license by Rhyous.com.
4. Source code enhancements or additions are the property of the author until
   the source code is contributed to this project. By contributing the source
   code to this project, the author immediately grants all rights to
   the contributed source code to Rhyous.com.
 
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

using System;
using System.Collections.Generic;
using WPFSharp.Globalizer.Base;

namespace WPFSharp.Globalizer
{
    public abstract class ResourceDictionaryManagerBase : IManageResourceDictionaries
    {

        #region IResourceDictionariesManager Members

        public event EventHandler ResourceDictionaryChangedEvent;

        public ILoadResourceDictionaries DictionaryManager { get; set; }

        public virtual List<String> FileNames
        {
            get { return _FileNames ?? (_FileNames = new List<string>()); }
            set { _FileNames = value; }
        } private List<string> _FileNames;

        public virtual void Add(string inResourceDictionaryFileName)
        {
            FileNames.Add(inResourceDictionaryFileName);
        }

        public virtual void Remove(string inResourceDictionaryName)
        {
            // TODO: Add ability to remove a resource dictionary
            throw new NotImplementedException();
        }

        public virtual void NotifyResourceDictionaryChanged()
        {
            if (null != ResourceDictionaryChangedEvent)
            {
                ResourceDictionaryChangedEvent(this, new EventArgs());
            }
        }

        #endregion
    }
}
