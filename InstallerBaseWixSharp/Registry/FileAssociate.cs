using System;
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

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once IdentifierTypo
        private static RegistryKey OpenOrCreateKeyHKLM(string path)
        {
            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path, true);
            if (key == null)
            {
                Microsoft.Win32.Registry.LocalMachine.CreateSubKey(path);
                key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path, true);
            }
            return key;
        }

        public static bool RegisterFileTypes(string appName, string company, string exeFile,
            string associationList)
        {
            try
            {
                var associationStrings = associationList.Split(';');

                foreach (var associationString in associationStrings)
                {
                    var association = FileAssociation.FromSerializeString(associationString);

                    RegisterFileType(association, exeFile);
                }

                var appRegistryTree = @"SOFTWARE\" +
                                      company + @"\" +
                                      appName;

                // register the registered file types..
                using (var key = OpenOrCreateKeyHKLM(appRegistryTree))
                {
                    key.SetValue("Associations", associationList);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool DeleteCompanyKeyIfEmpty(string company)
        {
            try
            {
                var companyRegistryTree = @"SOFTWARE\" +
                                          company;

                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(companyRegistryTree);
                if (key?.ValueCount == 0)
                {
                    Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(companyRegistryTree);
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
                using (var key = OpenOrCreateKeyHKLM(appRegistryTree))
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

        private static void UnRegisterFileType(FileAssociation association)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(association.Extension,
                    true)) 
                {
                    if (key == null) // this shouldn't happen..
                    {
                        return;
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
                // ignored..
            }
        }

        private static void RegisterFileType(FileAssociation association, string exeFileName)
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
            }
            catch
            {
                // ignored..
            }
        }
    }
}

