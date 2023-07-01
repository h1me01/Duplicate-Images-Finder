using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

namespace Duplicate_Images_Finder
{
    public static class FindImages
    {
        private static readonly string filesFolder = "AllFiles";
        private static readonly string duplicateFolder = "DuplicateFiles";
        private static readonly List<string> imageFiles = new List<string>();
        private static readonly List<string> duplicateImages = new List<string>();

        public static void SearchDuplicates()
        {
            Console.WriteLine("Initialize...");

            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            Console.WriteLine($"\nProcessing Images in {filesFolder}...\n");

            try
            {
                GetImageFiles();
                FindDuplicateImages();
                DisplayDuplicateImages();
                MoveDuplicateImages();
            }
            catch (Exception ex)
            {
                DisplayError(ex);
            }

            sw.Stop();
            Console.Write($"\nProcessing was successful (Runtime: {sw.Elapsed.TotalSeconds} sec.)");
            Console.ReadKey();
        }

        private static void GetImageFiles()
        {
            string[] extensions = { ".jpg", ".jpeg", ".png", ".gif" };

            foreach (var extension in extensions)
            {
                string[] files = Directory.GetFiles(filesFolder, "*" + extension, SearchOption.AllDirectories);
                imageFiles.AddRange(files);
            }

            if (imageFiles.Count == 0)
            {
                Console.WriteLine("No image files found in the specified folder.");
            }
        }

        private static void FindDuplicateImages()
        {
            HashSet<string> uniqueHashes = new HashSet<string>();
            duplicateImages.Clear();

            foreach (var file in imageFiles)
            {
                using (var image = Image.FromFile(file))
                {
                    string hash = GetImageHash(image);

                    if (!uniqueHashes.Add(hash))
                    {
                        duplicateImages.Add(file);
                    }
                }
            }
        }

        private static string GetImageHash(Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();
                using (var md5 = MD5.Create())
                {
                    byte[] hashBytes = md5.ComputeHash(imageBytes);
                    return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                }
            }
        }

        private static void DisplayDuplicateImages()
        {
            if (duplicateImages.Count > 0)
            {
                Console.WriteLine("\nShowing all Duplicates...\n");

                foreach (var file in duplicateImages)
                {
                    Console.WriteLine($"File: {file}");
                }
            }
            else
            {
                Console.WriteLine("No duplicate images found.");
            }
        }

        private static void MoveDuplicateImages()
        {
            if (duplicateImages.Count > 0)
            {
                Console.WriteLine($"\nDo you want to move the duplicates to the Folder: {duplicateFolder}? 1 for Yes, 2 for No");
                string input = Console.ReadLine();

                if (input == "1")
                {
                    Directory.CreateDirectory(duplicateFolder);

                    foreach (var file in duplicateImages)
                    {
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(duplicateFolder, fileName);
                        File.Move(file, destinationPath);
                    }

                    Console.WriteLine("Duplicate images moved to the specified folder.");
                }
                else
                {
                    Console.WriteLine("No action taken. Duplicate images were not moved.");
                }
            }
        }

        private static void DisplayError(Exception ex)
        {
            Console.WriteLine($"An Error has occurred: {ex.Message}");
            Console.ReadKey();
        }
    }
}