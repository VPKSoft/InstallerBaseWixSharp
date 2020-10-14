#region License
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
#endregion

#define UseRunProgramDialog
// define this to use the custom association dialog..
//#define UseAssociationDialog

using System;
using System.Diagnostics;
using InstallerBaseWixSharp.CustomActions;
using InstallerBaseWixSharp.Files.Dialogs;
using WixSharp;
using WixSharp.Forms;
using File = WixSharp.File;

namespace InstallerBaseWixSharp
{
    class Program
    {
        const string AppName = "#APPLICATION#";
        internal static readonly string Executable = $"{AppName}.exe";
        const string  Company = "VPKSoft";
        private static readonly string InstallDirectory = $@"%ProgramFiles%\{Company}\{AppName}";
        const string  ApplicationIcon = @".\Files\FileResources\replace_this_ico.ico";

        static void Main()
        {
            var project = new ManagedProject("#APPLICATION#",
                new Dir(InstallDirectory,
                    new WixSharp.Files(@"..\#APPLICATION#\bin\Release\*.*"),
                    new File("Program.cs")),
                new Dir($@"%ProgramMenu%\{Company}\{AppName}",
                    // ReSharper disable three times StringLiteralTypo
                    new ExeFileShortcut(AppName, $"[INSTALLDIR]{Executable}", "")
                    {
                        WorkingDirectory = "[INSTALLDIR]", IconFile = ApplicationIcon
                    }),
                new CloseApplication($"[INSTALLDIR]{Executable}", true), 
                new Property("Executable", Executable),
                new Property("AppName", AppName),
                new Property("Company", Company))
            {
                GUID = new Guid("3E320290-4AB2-4DA5-9F90-C3C775EDA03C"),
                ManagedUI = new ManagedUI(),
                ControlPanelInfo = 
                {
                    Manufacturer = Company, 
                    UrlInfoAbout  = "https://www.vpksoft.net", 
                    Name = $"Installer for the {AppName} application", 
                    HelpLink = "https://www.vpksoft.net", 
                },
                Platform = Platform.x64,
            };

            project.Package.Name = $"Installer for the {AppName} application";

            //project.ManagedUI = ManagedUI.Empty;    //no standard UI dialogs
            //project.ManagedUI = ManagedUI.Default;  //all standard UI dialogs

            //custom set of standard UI dialogs

            project.ManagedUI.InstallDialogs.Add(Dialogs.Welcome)
                                            .Add(Dialogs.Licence)
#if UseRunProgramDialog
                                            .Add<RunProgramDialog>()
#endif
#if UseAssociationDialog
                                            .Add<AssociationsDialog>()
#endif                                            
                                            .Add<ProgressDialog>()
                                            .Add(Dialogs.Exit);

            project.ManagedUI.ModifyDialogs.Add(Dialogs.MaintenanceType)
                                           .Add(Dialogs.Features)
                                           .Add(Dialogs.Progress)
                                           .Add(Dialogs.Exit);

            project.ControlPanelInfo.ProductIcon = ApplicationIcon;

            AutoElements.UACWarning = "";


            project.BannerImage = @"Files\install_top.png";
            project.BackgroundImage = @"Files\install_side.png";
            project.LicenceFile = @"Files\MIT.License.rtf";

            project.Load += Msi_Load;
            project.BeforeInstall += Msi_BeforeInstall;
            project.AfterInstall += Msi_AfterInstall;


            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

            ValidateAssemblyCompatibility();

            project.DefaultDeferredProperties += ",RUNEXE,PIDPARAM,ASSOCIATIONS";

            project.Localize();

            project.BuildMsi();
        }

        static void Msi_Load(SetupEventArgs e)
        {
            //if (!e.IsUninstalling) MessageBox.Show(e.ToString(), "Load");
        }

        static void Msi_BeforeInstall(SetupEventArgs e)
        {
            //if (!e.IsUninstalling) MessageBox.Show(e.ToString(), "BeforeInstall");
        }

        static void Msi_AfterInstall(SetupEventArgs e)
        {
            // run the executable after the install with delay (wait PID to )..
            if (e.IsInstalling)
            {
                try 
                {
                    if (System.IO.File.Exists(e.Session.Property("RUNEXE")))
                    {
                        Process.Start(e.Session.Property("RUNEXE"), $"--waitPid {e.Session.Property("PIDPARAM")}");
                    }
                    else
                    {
                        Console.WriteLine(e.Session.Property("RUNEXE"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"Post run failed: '{ex.Message}'...");
                }

                CustomActionFileAssociate.RegisterFileTypes(e.Session);
            }

            if (e.IsUninstalling)
            {
                CustomActionFileAssociate.UnRegisterFileTypes(e.Session);
            }
        }

        static void ValidateAssemblyCompatibility()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            if (!assembly.ImageRuntimeVersion.StartsWith("v2."))
            {
                Console.WriteLine(
                    $@"Warning: assembly '{assembly.GetName().Name}' is compiled for {assembly.ImageRuntimeVersion}" +
                    @" runtime, which may not be compatible with the CLR version hosted by MSI. " +
                    @"The incompatibility is particularly possible for the EmbeddedUI scenarios. " +
                    @"The safest way to solve the problem is to compile the assembly for v3.5 Target Framework.");
            }
        }
    }
}

