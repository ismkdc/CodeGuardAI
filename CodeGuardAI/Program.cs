using GenerativeAI;
using GenerativeAI.Clients;
using GenerativeAI.Types;

if (args.Length < 3)
{
    Console.WriteLine("Usage: <apiKey> <fileExtension> <folderPath>");
    return;
}

var apiKey = args[0];
var fileExtension = args[1];
var folderPath = args[2];

var googleAi = new GoogleAi(apiKey);
var googleModel = googleAi.CreateGenerativeModel("models/gemini-2.0-flash");

var fileClient = new FileClient(new GoogleAIPlatformAdapter(apiKey));
var files = Directory.GetFiles(folderPath, $"*.{fileExtension}", SearchOption.AllDirectories)
    .ToList();

var semaphore = new SemaphoreSlim(10);
var uploadTasks = new List<Task<FileUploadResult>>();

Console.WriteLine($"Found {files.Count} files to upload...");

var processedCount = 0;
var totalFiles = files.Count;

foreach (var file in files)
{
    await semaphore.WaitAsync();
    var uploadTask = UploadFileAsync(fileClient, file, semaphore);
    uploadTasks.Add(uploadTask);

    processedCount++;
    ShowProgress(processedCount, totalFiles);
}

var uploadResults = await Task.WhenAll(uploadTasks);

var generationConfig = new GenerationConfig
{
    Temperature = 1,
    TopP = 0.95,
    TopK = 40,
    MaxOutputTokens = 8192,
    ResponseMimeType = "text/plain",
};

var history = uploadResults
    .Select(uploaded => new Content
        {
            Role = "user",
            Parts =
            [
                new Part
                {
                    FileData = new FileData
                    {
                        MimeType = uploaded.MimeType,
                        FileUri = uploaded.FileUri
                    }
                }
            ]
        }
    ).ToList();

var chatSession = googleModel.StartChat(history, generationConfig);
const string prompt =
    "Analyze the source code files I provide and identify any potential security vulnerabilities. For each vulnerability, create a report containing the following details (DONT RETURN ANYTHING ELSE):\n\n[File Name] => <filename>\n[Vulnerable Line] => <line_of_code>\n[Vulnerability Description] => <brief explanation of the vulnerability>\n[Suggested Fix] => <recommended solution or code change>\n\nCarefully review the entire code and report any vulnerabilities that may pose a security risk. \n";
var result = await chatSession.GenerateContentAsync(prompt);

Console.WriteLine();
Console.WriteLine(result);


async Task<FileUploadResult> UploadFileAsync(FileClient fileClient, string filePath, SemaphoreSlim semaphore)
{
    try
    {
        var uploaded = await fileClient.UploadFileAsync(filePath);

        while (uploaded.State != FileState.ACTIVE)
        {
            await Task.Delay(1000);
            uploaded = await fileClient.GetFileAsync(uploaded.Name);
        }

        return new FileUploadResult
        {
            FileUri = uploaded.Uri,
            MimeType = uploaded.MimeType
        };
    }
    finally
    {
        semaphore.Release();
    }
}

void ShowProgress(int processed, int total)
{
    Console.CursorLeft = 0;
    var percentage = (double)processed / total * 100;
    var barWidth = 50;
    var progress = (int)(percentage / 2);
    Console.Write($"[{new string('#', progress)}{new string('-', barWidth - progress)}] {percentage:0.00}%");
}


public class FileUploadResult
{
    public string FileUri { get; set; }
    public string MimeType { get; set; }
}