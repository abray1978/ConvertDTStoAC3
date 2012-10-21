THIS APPLICATION IS PROVIDED AS IS.  AUTHOR IS NOT RESPONSIBLE FOR ANY DAMAGE OR ISSUES THAT IT MAY CAUSE.

This .NET application was built to allow me to basically convert DTS to AC3 using MKVTOOLNIX, FFMPEG all with SABNZBD.  

I used the logic from mkvdts2ac3.sh, written by Jake Wharton <jakewharton@gmail.com> & Chris Hoekstra <chris.hoekstra@gmail.com>

You will need to have FFMPEG, Sed for Windows and MKVTOOLNIX.

Usage:

ConvertDTStoAC3.exe -source <directory with your mkv files> -mkvtoolnixpath <path to mkvtoolnix files> -ffmpegpath <path to ffmpeg> -sedpath <path to sed for windows> 

Optional Command Line Arguments:

-verbose
	All output from each mkvtoolnix command and FFMPEG will be displayed

-keeporiginal
	The original MKV file will not be removed

This was written in .NET 4.0, so you will need that on your windows system for this to work.

To use this with sabnzbd, create a script similar to the following:

convertDTS2AC3.bat:

@ECHO off

CD /D %1
D:\Convert\ConvertDTStoAC3.exe -debug -mvktoolnixpath d:\Convert -ffmpegpath d:\Convert -sedpath d:\Convert -source %1
