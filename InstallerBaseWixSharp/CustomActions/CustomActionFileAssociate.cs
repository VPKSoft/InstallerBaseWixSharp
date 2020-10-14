using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InstallerBaseWixSharp.Files.Dialogs;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using WixSharp;
using FileAssociation = InstallerBaseWixSharp.Files.Dialogs.DialogClasses.FileAssociation;

// ((C), modified from): https://github.com/oleg-shilo/wixsharp/blob/master/Source/src/WixSharp.Samples/Wix%23%20Samples/RegisterFileType/With%20DTF/setup.cs

namespace InstallerBaseWixSharp.CustomActions
{
    class CustomActionFileAssociate
    {
        private static RegistryKey OpenOrCreateKey(string path)
        {
            var key = Registry.ClassesRoot.OpenSubKey(path, true);
            if (key == null)
            {
                Registry.ClassesRoot.CreateSubKey(path);
                key = Registry.ClassesRoot.OpenSubKey(path, true);
            }
            return key;
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once IdentifierTypo
        private static RegistryKey OpenOrCreateKeyHKLM(string path)
        {
            var key = Registry.LocalMachine.OpenSubKey(path, true);
            if (key == null)
            {
                Registry.LocalMachine.CreateSubKey(path);
                key = Registry.LocalMachine.OpenSubKey(path, true);
            }
            return key;
        }

        [CustomAction]
        public static ActionResult RegisterFileTypes(Session session)
        {
            try
            {
                // ReSharper disable once StringLiteralTypo
                var exeFile = session.Property("RUNEXE");

                var associationStrings = session.Property("ASSOCIATIONS").Split(';');

                foreach (var associationString in associationStrings)
                {
                    var association = FileAssociation.FromSerializeString(associationString);

                    RegisterFileType(association, exeFile);
                }

                var appRegistryPath = @"SOFTWARE\" + 
                    session.Property("AppName") + @"\" + 
                    session.Property("Company");

                // register the registered file types..
                using (var key = OpenOrCreateKeyHKLM(appRegistryPath))
                {
                    key.SetValue("Associations", associationStrings);
                }
            }
            catch
            {
                // ignored..
            }
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult UnRegisterFileTypes(Session session)
        {
            try
            {
                var appRegistryTree = @"SOFTWARE\" +
                                      session.Property("AppName") + @"\" +
                                      session.Property("Company");

                var appRegistryKey = appRegistryTree + @"\Associations";

                var associationStrings = Registry.LocalMachine.GetValue(appRegistryKey).ToString().Split(';');

                foreach (var associationString in associationStrings)
                {
                    UnRegisterFileType(FileAssociation.FromSerializeString(associationString));
                }

                Registry.LocalMachine.DeleteSubKeyTree(appRegistryTree);
            }
            catch
            {
                // ignored..
            }
            return ActionResult.Success;
        }

        private static void UnRegisterFileType(FileAssociation association)
        {
            try
            {
                var extensionAssociationName = Registry.ClassesRoot.GetValue(association.Extension).ToString();

                Registry.ClassesRoot.DeleteSubKeyTree(association.Extension);
                Registry.ClassesRoot.DeleteSubKeyTree(extensionAssociationName);
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
                    key.SetValue("", $"\"" + exeFileName + "\" \" %1\"");
                }
            }
            catch
            {
                // ignored..
            }
        }
    }
}
