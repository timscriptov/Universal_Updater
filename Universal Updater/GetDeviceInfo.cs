﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Universal_Updater
{
    class GetDeviceInfo
    {
        static Process getDeviceInfoProcess;
        static readonly string[] deviceDetails = { "PhoneFriendlyName", "PhoneManufacturer", "PhoneManufacturerDisplayName", "PhoneManufacturerModelName", "PhoneHardwareVariant", "PhoneModelName", "PhoneHardwareRevision", "PhoneFirmwareRevision", "PhoneMobileOperatorName", "PhoneRadioSoftwareRevision", "PhoneROMLanguage", "PhoneSOCVersion" };
        static readonly string[] spaceAlign = { "                 ", "             ", "           ", "           ", "", "  ", "        ", "              ", "              ", "       ", "       ", "     ", "  ", "            ", "             ", "             " };
        static string[] deviceInfo;
        public static string[] OSVersion { get; private set; }
        public static string[] SerialNumber { get; private set; }

        public static int GetLog()
        {
            getDeviceInfoProcess = new Process();
            getDeviceInfoProcess.StartInfo.FileName = @"C:\ProgramData\Universal Updater\getdulogs.exe";
            getDeviceInfoProcess.StartInfo.Arguments = @"-o ""C:\ProgramData\Universal Updater\DeviceLog.cab""";
            getDeviceInfoProcess.StartInfo.RedirectStandardOutput = true;
            getDeviceInfoProcess.StartInfo.RedirectStandardError = true;
            getDeviceInfoProcess.StartInfo.RedirectStandardInput = true;
            getDeviceInfoProcess.Start();
            getDeviceInfoProcess.WaitForExit();
            return getDeviceInfoProcess.ExitCode;
        }

        public static async Task<string[]> ReadInfo()
        {
            getDeviceInfoProcess.StartInfo.Arguments = @"-l";
            getDeviceInfoProcess.Start();
            getDeviceInfoProcess.WaitForExit();
            string logOutput = await getDeviceInfoProcess.StandardOutput.ReadToEndAsync();
            File.WriteAllText(@"C:\ProgramData\Universal Updater\DeviceInfo.txt", logOutput);
            CabExtractor.ExtractFile(@"C:\ProgramData\Universal Updater\DeviceLog.cab", "OEMDevicePlatform.xml", @"C:\ProgramData\Universal Updater\OEMDevicePlatform.xml");
            CabExtractor.ExtractFile(@"C:\ProgramData\Universal Updater\DeviceLog.cab", "UsoTroubleshooting.reg", @"C:\ProgramData\Universal Updater\UsoTroubleshooting.reg");
            CabExtractor.ExtractFile(@"C:\ProgramData\Universal Updater\DeviceLog.cab", "InstalledPackages.csv", @"C:\ProgramData\Universal Updater\InstalledPackages.csv");
            CabExtractor.ExtractFile(@"C:\ProgramData\Universal Updater\DeviceLog.cab", "OEMInput.xml", @"C:\ProgramData\Universal Updater\OEMInput.xml");
            deviceInfo = new string[deviceDetails.Length + 4];
            SerialNumber = File.ReadAllLines(@"C:\ProgramData\Universal Updater\DeviceInfo.txt").Where(i => i.Contains("Serial:")).ToArray();
            string[] platID = File.ReadAllLines(@"C:\ProgramData\Universal Updater\OEMDevicePlatform.xml").Where(i => i.Contains("<DevicePlatformID>")).ToArray();
            deviceInfo[0] = SerialNumber[0];
            SerialNumber[0] = $@"{SerialNumber[0].Split(':')[1].Replace(" ", null)}";
            deviceInfo[1] = "PlatformID: " + platID[0].Split('>')[1].Split('<')[0];
            for (int i = 0, j = 2; i < deviceDetails.Length; i++)
            {
                if (i == 6)
                {
                    OSVersion = File.ReadAllLines(@"C:\ProgramData\Universal Updater\InstalledPackages.csv").Where(k => k.Contains("Microsoft.DEVICELAYOUT_QC")).ToArray();
                    deviceInfo[i + j] = "OSVersion: " + OSVersion[0].Split(',')[2];
                    j++;
                }
                var details = File.ReadAllLines(@"C:\ProgramData\Universal Updater\UsoTroubleshooting.reg").Where(l => l.Contains(deviceDetails[i])).ToArray();
                deviceInfo[i + j] = details[0].Replace("    ", null).Replace("\"", null).Replace("=", ": ").Replace("Phone", null);
            }
            string[] res = File.ReadAllLines(@"C:\ProgramData\Universal Updater\OEMInput.xml").Where(i => i.Contains("<Resolution>")).ToArray();
            deviceInfo[deviceInfo.Length - 1] = "Resolution: " + res[0].Split('>')[1].Split('<')[0];
            if (Directory.Exists($@"{Environment.CurrentDirectory}\{SerialNumber[0]}"))
            {
                Directory.Delete($@"{Environment.CurrentDirectory}\{SerialNumber[0]}", true);
                Directory.CreateDirectory($@"{Environment.CurrentDirectory}\{SerialNumber[0]}\Packages");
            }
            else
            {
                Directory.CreateDirectory($@"{Environment.CurrentDirectory}\{SerialNumber[0]}\Packages");
            }
            File.Copy(@"C:\ProgramData\Universal Updater\DeviceLog.cab", $@"{Environment.CurrentDirectory}\{SerialNumber[0]}\DeviceLog.cab");
            for (int i = 0; i < deviceInfo.Length; i++)
            {
                var splitDetails = deviceInfo[i].Split(':');
                deviceInfo[i] = splitDetails[0] + ":" + spaceAlign[i] + splitDetails[1];
                File.AppendAllText($@"{Environment.CurrentDirectory}\{SerialNumber[0]}\DeviceInfo.txt", deviceInfo[i] + "\n");
            }
            return deviceInfo;
        }
    }
}
