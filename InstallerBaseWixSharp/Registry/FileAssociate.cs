﻿/*
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

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using WixSharp;
using FileAssociation = InstallerBaseWixSharp.Files.Dialogs.DialogClasses.FileAssociation;

// ((C), modified from): https://github.com/oleg-shilo/wixsharp/blob/master/Source/src/WixSharp.Samples/Wix%23%20Samples/RegisterFileType/With%20DTF/setup.cs

namespace InstallerBaseWixSharp.Registry
{
    class FileAssociate
    {
        private static RegistryKey OpenOrCreateKey(string path)
        {
            var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(path, true);
            if (key == null)
            {
                Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(path);
                key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(path, true);
            }
            return key;
        }

        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(HChangeNotifyEventID wEventId,
            HChangeNotifyFlags uFlags,
            IntPtr dwItem1,
            IntPtr dwItem2);

        public static bool RegisterFileTypes(string appName, string company, string exeFile,
            string associationList)
        {
            try
            {
                var changeNotify = false;

                var associationStrings = associationList.Split(';');

                foreach (var associationString in associationStrings)
                {
                    var association = FileAssociation.FromSerializeString(associationString);

                    changeNotify |= RegisterFileType(association, exeFile);
                }

                var appRegistryTree = @"SOFTWARE\" +
                                      company + @"\" +
                                      appName;

                // register the registered file types..
                using (var key = CommonCalls.OpenOrCreateKeyHKLM(appRegistryTree))
                {
                    key.SetValue("Associations", associationList);
                }

                if (changeNotify)
                {
                    // ReSharper disable once CommentTypo
                    // credits to pinvoke.net..
                    SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED, HChangeNotifyFlags.SHCNF_IDLIST,
                        IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool UnRegisterFileTypes(string appName, string company)
        {
            try
            {
                var appRegistryTree = @"SOFTWARE\" +
                                      company + @"\" +
                                      appName;

                // register the registered file types..
                using (var key = CommonCalls.OpenOrCreateKeyHKLM(appRegistryTree))
                {
                    var registryValue = key.GetValue("Associations").ToString();
                    var associationStrings = registryValue.Split(';');

                    foreach (var associationString in associationStrings)
                    {
                        UnRegisterFileType(FileAssociation.FromSerializeString(associationString));
                    }
                }

                Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(appRegistryTree);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool UnRegisterFileType(FileAssociation association)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(association.Extension,
                    true)) 
                {
                    if (key == null) // this shouldn't happen..
                    {
                        return false;
                    }

                    // get the association value..
                    var extensionAssociationName = key.GetValue("").ToString();

                    key.DeleteValue("");

                    if (key.ValueCount == 0) // only clear if no other values exist..
                    {
                        Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(association.Extension);
                    }

                    // delete the association value..
                    Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(extensionAssociationName);
                }

            }
            catch
            {
                return false;
            }
            return true;
        }

        private static bool RegisterFileType(FileAssociation association, string exeFileName)
        {
            try
            {
                // associate the extension to a named string value..
                using (var key = OpenOrCreateKey(association.Extension))
                {
                    key.SetValue("", association.AssociationName);
                }

                // set the icon for the association name..
                using (var key = OpenOrCreateKey(association.AssociationName + @"\DefaultIcon"))
                {
                    key.SetValue("", exeFileName + "," + 0);
                }

                // write open the root of the command shell "path"..
                using (var key = OpenOrCreateKey(association.AssociationName + @"\shell"))
                {
                    key.SetValue("", "open");
                }

                // set the shell open command..
                using (var key = OpenOrCreateKey(association.AssociationName + @"\shell\open\command"))
                {
                    key.SetValue("", "\"" + exeFileName + "\" \"%1\"");
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
