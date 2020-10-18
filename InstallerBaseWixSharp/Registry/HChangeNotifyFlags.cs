/*
MIT License

Copyright(c) 2020 Petteri Kautonen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#region public enum HChangeNotifyFlags

using System;
using System.Diagnostics.CodeAnalysis;

// (C)::https://pinvoke.net/default.aspx/shell32/HChangeNotifyFlags.html

/// <summary>
/// Flags that indicate the meaning of the <i>dwItem1</i> and <i>dwItem2</i> parameters.
/// The uFlags parameter must be one of the following values.
/// </summary>
[Flags]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "CommentTypo")]
// ReSharper disable once CheckNamespace
public enum HChangeNotifyFlags
{
    /// <summary>
    /// The <i>dwItem1</i> and <i>dwItem2</i> parameters are DWORD values.
    /// </summary>
    SHCNF_DWORD = 0x0003,

    /// <summary>
    /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of ITEMIDLIST structures that
    /// represent the item(s) affected by the change.
    /// Each ITEMIDLIST must be relative to the desktop folder.
    /// </summary>
    SHCNF_IDLIST = 0x0000,

    /// <summary>
    /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
    /// maximum length MAX_PATH that contain the full path names
    /// of the items affected by the change.
    /// </summary>
    SHCNF_PATHA = 0x0001,

    /// <summary>
    /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
    /// maximum length MAX_PATH that contain the full path names
    /// of the items affected by the change.
    /// </summary>
    SHCNF_PATHW = 0x0005,

    /// <summary>
    /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
    /// represent the friendly names of the printer(s) affected by the change.
    /// </summary>
    SHCNF_PRINTERA = 0x0002,

    /// <summary>
    /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
    /// represent the friendly names of the printer(s) affected by the change.
    /// </summary>
    SHCNF_PRINTERW = 0x0006,

    /// <summary>
    /// The function should not return until the notification
    /// has been delivered to all affected components.
    /// As this flag modifies other data-type flags, it cannot by used by itself.
    /// </summary>
    SHCNF_FLUSH = 0x1000,

    /// <summary>
    /// The function should begin delivering notifications to all affected components
    /// but should return as soon as the notification process has begun.
    /// As this flag modifies other data-type flags, it cannot by used by itself.
    /// </summary>
    SHCNF_FLUSHNOWAIT = 0x2000
}
#endregion // enum HChangeNotifyFlags