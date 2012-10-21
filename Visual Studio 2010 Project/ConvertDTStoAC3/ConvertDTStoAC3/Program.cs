using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ConvertDTStoAC3
{
    class Program
    {
        static String strFilePath = "";
        static String strMKVToolNixPath = AppDomain.CurrentDomain.BaseDirectory;
        static String strFFMPEGPath = AppDomain.CurrentDomain.BaseDirectory;
        static String strSEDPath = AppDomain.CurrentDomain.BaseDirectory;

        //static String strMKVToolNixPath = "D:\\CONVERT";
        //static String strFFMPEGPath = "D:\\CONVERT";
        //static String strSEDPath = "D:\\CONVERT";

        static bool Debug = false;
        static bool KeepOriginal = false;

        static String strDelay = ""; 

        static int Main(string[] args)
        {
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("ConvertDTStoAC3 - Written by Adam Bray.");
            Console.WriteLine("---------------------------------------");
            // Test if input arguments were supplied:
            if (args.Length == 0)
            {
                DisplayHelp();
                return 1;
            }

            int intArgCount = 0;
            //Parse the Commandline Options
            foreach (string s in args)
            {
                if (s == "-source")
                {
                    strFilePath = args[intArgCount+1];
                    Console.WriteLine("-- Source Path: " + strFilePath.ToString());                    
                }
                else if (s == "-verbose")
                {
                    Debug = true;
                    Console.WriteLine("-- VERBOSE: Enabled");  
                }
                else if (s == "-keeporiginal")
                {
                    KeepOriginal = true;
                    Console.WriteLine("-- KEEPORIGINAL: Enabled");
                }
                else if (s == "-mvktoolnixpath")
                {
                    strMKVToolNixPath = args[intArgCount + 1];                   
                }
                else if (s == "-ffmpegpath")
                {
                    strFFMPEGPath = args[intArgCount + 1];
                }
                else if (s == "-sedpath")
                {
                    strSEDPath = args[intArgCount + 1];
                }
                intArgCount++;
            }
            //Record the File Path
            //strFilePath = args[0].ToString();
            Console.WriteLine("-- MKVToolnix Path: " + strMKVToolNixPath);
            Console.WriteLine("-- FFMpeg Path: " + strFFMPEGPath);
            Console.WriteLine("-- Sed Path: " + strFFMPEGPath);

            //Let's see if the path exists
            if (Directory.Exists(strFilePath))
            {
                Console.WriteLine("-------------------------------");
                Console.WriteLine("- Searching " + strFilePath + " for MKV Files");
                Console.WriteLine("-------------------------------");
                //Path Exists
                DirSearch(strFilePath);

                Console.WriteLine("- Search Complete!");
            }
            else
            {
                Console.WriteLine("Path: " + strFilePath.ToString() + " - Does Not Exist");
                DisplayHelp();
                return 1;
            }
            return 0;
        }        
    
        static void DisplayHelp()
        {            
            Console.WriteLine("Usage: ConvertDTStoAC3 -source DIRECTORY-OF-FILES <options>");
            Console.WriteLine("Example: ConvertDTStoAC3 -source C:\\VIDEOS\\TO\\CONVERT\\DTS\\TO\\AC3\\ -keeporiginal -utilpath d:\\ConvertUtils");
            Console.WriteLine("");
            Console.WriteLine(" -source");
            Console.WriteLine("          Location of MKV file(s) to be converted");
            Console.WriteLine(" -keeporiginal");
            Console.WriteLine("          Keeps the original MKV");
            Console.WriteLine(" -mkvtoolnixpath");
            Console.WriteLine(" -ffmpegpath");
            Console.WriteLine(" -sedpath");
            Console.WriteLine("          Specifies where mkvtoolnix, ffmpeg and sed are located.  If omitted, we assume the tools are in the same location as this exe");
            Console.WriteLine(" -verbose");
            Console.WriteLine("          Displays output from conversion tools");
        }

        static void DirSearch(string sDir)
        {
            try
            {
                if (sDir == strFilePath)
                {
                    //First search for any MKV files in the directory
                    foreach (string f in Directory.GetFiles(sDir, "*.mkv"))
                    {
                        Console.WriteLine("-- MKV File Found: " + f);

                        //Initiate Convert
                        int result = Convert(f);
                    }
                }

                //Now if there are subdirectories, let's search those.
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    if (Debug)
                        Console.WriteLine("-- Searching Directory: " + d);

                    foreach (string f in Directory.GetFiles(d, "*.mkv"))
                    {
                        Console.WriteLine("--- MKV File Found: " + f);

                        //Initiate Convert
                        int result = Convert(f);
                    }

                    DirSearch(d);
                }
                
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        static int Convert(string strFileName)
        {
            String strNewFileName = System.IO.Path.GetFileNameWithoutExtension(strFileName) + "-ac3.mkv";
            String strDestPath = System.IO.Path.GetDirectoryName(strFileName);

            Console.WriteLine("--- Converting MKV: " + strFileName);


            //Let's search for the first DTS track
            Console.WriteLine("--- Looking for DTS Track");
            Process p = new Process();
            p.StartInfo.FileName = strMKVToolNixPath + "\\mkvmerge.exe";
            p.StartInfo.Arguments = "-i \"" + strFileName + "\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            //Parse the Output for audio (A_DTS)
            int intLocation = output.IndexOf("audio (A_DTS)");
             
            if (intLocation > 3)
                intLocation -= 3;
            else
            {
                Console.WriteLine("--- ERROR: No DTS Track Found - exiting");
                return 1;
            }

            String  strTrackID = output[intLocation].ToString();

            //Parse the Output for video (V_
            intLocation = 0;
            intLocation = output.IndexOf("video (V_");

            if (intLocation > 3)
                intLocation -= 3;
            else
            {
                Console.WriteLine("--- ERROR: No DTS Track Found - exiting");
                return 1;
            }

            String strVideoTrackID = output[intLocation].ToString();

            Console.WriteLine("--- DTS Track Found: " + strTrackID);

            if (Debug)
                Console.WriteLine(output);    

            //Get Track Info
            Console.WriteLine("--- Getting Information from Track #" + strTrackID);
            p = new Process();
            p.StartInfo.FileName = strMKVToolNixPath + "\\mkvinfo.exe";
            p.StartInfo.Arguments = "\"" + strFileName + "\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                Console.WriteLine("--- ERROR: Unable to Get Track Information, Exiting");
                return 1;
            }

            if (Debug)
                Console.WriteLine(output);    


            //Get the Timecodes
            Console.WriteLine("--- Getting Timecodes from Track #" + strTrackID);
            p = new Process();
            p.StartInfo.FileName = strMKVToolNixPath + "\\mkvextract.exe";
            p.StartInfo.Arguments = "timecodes_v2 \"" + strFileName + "\" " + strTrackID + ":tcfile.tc";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                Console.WriteLine("--- ERROR: Unable to Get Track Timecodes, Exiting");
                return 1;
            }

            if (Debug)
                Console.WriteLine(output);

            //Get Delay
            Console.WriteLine("--- Getting Delay from Track #" + strTrackID);
            p = new Process();
            p.StartInfo.FileName = strSEDPath + "\\sed.exe";
            p.StartInfo.Arguments = "-n \"2p\" tcfile.tc";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                Console.WriteLine("--- ERROR: Unable to Get Track Timecodes, Exiting");
                return 1;
            }

            if (Debug)
                Console.WriteLine(output);

            strDelay = output;

            //Pull out th e DTS source
            Console.WriteLine("--- Extracting DTS");
            p = new Process();
            p.StartInfo.FileName = strMKVToolNixPath + "\\mkvextract.exe";
            p.StartInfo.Arguments = "tracks \"" + strFileName + "\" " + strTrackID + ":dtsfile.dts";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                Console.WriteLine("--- ERROR: Unable to Extract DTS, Exiting");
                return 1;
            }

            if (Debug)
                Console.WriteLine(output);    


            //Convert DTS to AC3
            Console.WriteLine("--- Converting DTS to AC3");
            p = new Process();
            p.StartInfo.FileName = strFFMPEGPath + "\\ffmpeg.exe";
            p.StartInfo.Arguments = "-i dtsfile.dts -acodec ac3 -ac 6 -ab 448k ac3file.ac3";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                Console.WriteLine("--- ERROR: Unable to Convert DTS to AC3, Exiting");
                return 1;
            }

            if (Debug)
                Console.WriteLine(output);

            String strCMDArgments = "";

            //Merge AC3 as Second Track
            Console.WriteLine("--- Merging AC3 as 2nd Track in MKV");
            p = new Process();
            p.StartInfo.FileName = strMKVToolNixPath + "\\mkvmerge.exe";
            strCMDArgments = "--track-order 0:1,1:0 -o \"" + strDestPath + "\\" + strNewFileName + "\" -A --compression " + strVideoTrackID + ":none \"" + strFileName +"\" --default-track 0 --compression 0:none ac3file.ac3";

            if (strDelay != "0")
                strCMDArgments = strCMDArgments + " --sync 0:" + strDelay;

            p.StartInfo.Arguments = strCMDArgments;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                Console.WriteLine("--- ERROR: Unable to Merge AC3 Track, Exiting");
                return 1;
            }

            if (Debug)
                Console.WriteLine(output);    

 
            //Now clean up the files
            if (!KeepOriginal)
                File.Delete(strFileName);

            File.Delete("tcfile.tc");
            File.Delete("ac3file.ac3");
            File.Delete("dtsfile.dts");

            //Single the All Clear
            Console.WriteLine("--- SUCCESS: CONVERSION COMPLETED!");
            Console.WriteLine("---- Converted Filename: " + strNewFileName);
            Console.WriteLine("---- Destination of Converted File: " + strDestPath);
            return 0;
        }
    }
}
