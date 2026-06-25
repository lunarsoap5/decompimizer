using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RarcTools.GCRebuilder
{
    internal class GCRebuilder
    {
        // Based off of the GCRebuilder created by BSV [https://github.com/bsv798/gcrebuilder]
        [STAThread]
        public static int RebuildISO(string rootPath, string isoPath, bool useGameTOC)
        {
            try
            {
                BackendClass mf = new BackendClass();

                if (mf.IsRootPath(rootPath))
                {
                    if (useGameTOC)
                    {
                        mf.RootOpen(rootPath, true);
                    }
                    else
                    {
                        mf.RootOpen(rootPath, false);
                    }
                    mf.Rebuild(isoPath);
                }
                else
                {
                    Console.WriteLine(
                        "Supplied path: " + rootPath + " is not a valid root directory."
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return ex.HResult;
            }
            return 0;
        }

        public static int ExtractISO(string imagePath, string folderPath)
        {
            try
            {
                BackendClass mf = new BackendClass();

                if (BackendClass.IsImagePath(imagePath))
                {
                    mf.ImageOpen(imagePath);
                    mf.Export(folderPath);
                }
                else
                {
                    Console.WriteLine(
                        "Supplied path: " + imagePath + " is not a valid image directory."
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return ex.HResult;
            }

            return 0;
        }

        static void Usage()
        {
            Console.WriteLine("--extract|import|rebuild iso_path folder_path");
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
