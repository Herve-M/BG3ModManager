﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace DivinityModManager.Util
{
	public static class RecycleBinHelper
	{
		// Source: http://csharphelper.com/blog/2015/07/manage-the-recycle-bin-wastebasket-in-c/
		// Structure used by SHQueryRecycleBin.
		[StructLayout(LayoutKind.Sequential)]
		private struct SHQUERYRBINFO
		{
			public int cbSize;
			public long i64Size;
			public long i64NumItems;
		}

		// Get information from recycle bin.
		[DllImport("shell32.dll")]
		private static extern int SHQueryRecycleBin(string pszRootPath,
			ref SHQUERYRBINFO pSHQueryRBInfo);

		// Empty the recycle bin.
		[DllImport("shell32.dll")]
		static extern int SHEmptyRecycleBin(IntPtr hWnd,
			string pszRootPath, uint dwFlags);

		// Return the number of items in the recycle bin.

		// Note: In Windows 2000, you need to supply the root
		// directory to the call to SHQueryRecycleBin so to get
		// the total number of files in the recycle you must add
		// up the results for each disk. See:
		// http://www.pinvoke.net/default.aspx/shell32/SHQueryRecycleBin.html
		public static int NumberOfFilesInRecycleBin()
		{
			SHQUERYRBINFO sqrbi = new SHQUERYRBINFO();
			sqrbi.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));
			int hresult = SHQueryRecycleBin(string.Empty, ref sqrbi);
			return (int)sqrbi.i64NumItems;
		}

		// Delete a file or move it to the recycle bin.
		public static void DeleteFile(string filename, bool confirm,
			bool delete_permanently)
		{
			UIOption ui_option = UIOption.OnlyErrorDialogs;
			if (confirm) ui_option = UIOption.AllDialogs;

			RecycleOption recycle_option =
				recycle_option = RecycleOption.SendToRecycleBin;
			if (delete_permanently)
				recycle_option = RecycleOption.DeletePermanently;
			try
			{
				Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(filename, ui_option, recycle_option);
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Error deleting file.\n" + ex.ToString());
			}
		}

		// Empty the wastebasket.
		[Flags]
		private enum RecycleFlags : uint
		{
			SHERB_NOCONFIRMATION = 0x1,
			SHERB_NOPROGRESSUI = 0x2,
			SHERB_NOSOUND = 0x4
		}
		public static void EmptyWastebasket(bool show_progress,
			bool play_sound, bool confirm)
		{
			RecycleFlags options = 0;
			if (!show_progress) options =
				options | RecycleFlags.SHERB_NOPROGRESSUI;
			if (!play_sound) options =
			 options | RecycleFlags.SHERB_NOSOUND;
			if (!confirm) options =
		  options | RecycleFlags.SHERB_NOCONFIRMATION;

			try
			{
				SHEmptyRecycleBin(IntPtr.Zero, null, (uint)options);
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Error emptying wastebasket.\n" + ex.ToString());
			}
		}
	}
}
