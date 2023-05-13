using ImageMagick;

string inputDirectory = ".\\Input";
string outputDirectory = ".\\Output";
string[] processableExtensions = new[] { ".jpg", ".jpeg", ".png" };

// Check if the output directory exists, if not, create it.
if (!Directory.Exists(outputDirectory))
    Directory.CreateDirectory(outputDirectory);

string[] filesToProcess = Directory.GetFiles(inputDirectory, "*.*", SearchOption.AllDirectories);
int fileCount = filesToProcess.Length;

int processedCount = 0;
Progress<int> progress = new(count =>
{
    processedCount += count;
    Console.WriteLine($"Processed {processedCount} of {fileCount} files. Estimated time remaining: {TimeSpan.FromSeconds((fileCount - processedCount) * 2)}");
});

// Traverse all files in the input directory.
Parallel.ForEach(filesToProcess, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, filePath =>
{
    // Obtiene la ruta relativa del archivo de entrada en relación al directorio de entrada.
    string relativePath = Path.GetRelativePath(inputDirectory, filePath);

    // Create the output path by concatenating the path relative to the output directory.
    string outputFilePath = Path.Combine(outputDirectory, relativePath);

    // Create the necessary folders in the output path.
    Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

    if (processableExtensions.Contains(Path.GetExtension(filePath).ToLower()))
    {
        // Load the image using ImageMagick.
        using (MagickImage image = new(filePath))
        {
            image.Quality = 100;

            // Automatically orient the image if necessary.
            image.AutoOrient();

            // Resize the image to full hd if it is larger than that.
            if (image.BaseWidth >= image.BaseHeight && image.BaseWidth < 1920)
            {
                // If the difference between the base width and the new width is less than 50% of the base width, resize it (keeping the aspect ratio) with the adaptive resize method
                if (image.BaseWidth * 1.5 > 1920)
                {
                    image.AdaptiveResize(new MagickGeometry(1920, 1080)
                    {
                        IgnoreAspectRatio = false,
                        FillArea = true
                    });
                }
                // Otherwise, resize it (keeping the aspect ratio) with the normal resize method
                else
                {
                    image.Resize(new MagickGeometry(1920, 1080)
                    {
                        IgnoreAspectRatio = false,
                        FillArea = true
                    });
                }
            }
            else if (image.BaseHeight > image.BaseWidth && image.BaseHeight < 1920)
            {
                // If the difference between the base height and the new height is less than 50% of the base height, resize it (keeping the aspect ratio) with the adaptive resize method
                if (image.BaseHeight * 1.5 > 1920)
                {
                    image.AdaptiveResize(new MagickGeometry(1080, 1920)
                    {
                        IgnoreAspectRatio = false,
                        FillArea = true
                    });
                }
                // Otherwise, resize it (keeping the aspect ratio) with the normal resize method
                else
                {
                    image.Resize(new MagickGeometry(1080, 1920)
                    {
                        IgnoreAspectRatio = false,
                        FillArea = true
                    });
                }
            }

            // Convert the image to PNG and save it to the output path.
            image.Write(Path.ChangeExtension(outputFilePath, ".png"));
        };
    }
    else
    {
        // Copy the file to the output directory.
        File.Copy(filePath, outputFilePath);
    }

    ((IProgress<int>)progress).Report(1);
});

Console.WriteLine("Finished!");