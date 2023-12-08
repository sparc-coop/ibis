﻿using PuppeteerSharp;
using File = Sparc.Blossom.Data.File;

namespace Ibis.Messages;

public class GetPageScreenshot : RealtimeFeature<MessageTextChanged>
{
    public GetPageScreenshot(IRepository<Room> rooms, IFileRepository<File> files, AzureOCR azureOCR)
    {
        Rooms = rooms;
        Files = files;
        AzureOCR = azureOCR;
    }

    public IRepository<Room> Rooms { get; }
    public IFileRepository<File> Files { get; }
    public AzureOCR AzureOCR { get; }

    public override async Task ExecuteAsync(MessageTextChanged notification)
    {
        //var room = await Rooms.FindAsync(notification.Message.RoomId);

        var fileUrl = await GetScreenshotAsync(notification);

        //await AzureOCR.MakeRequest();

        return;
    }

    public async Task<string> GetScreenshotAsync(MessageTextChanged notification)
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        var url = "https://www.google.com";
        //var file = ".\\somepage.jpg";

        var launchOptions = new LaunchOptions()
        {
            Headless = true
        };

        using (var browser = await Puppeteer.LaunchAsync(launchOptions))
        using (var page = await browser.NewPageAsync())
        {
            await page.GoToAsync(url);
            var fileData = await page.ScreenshotDataAsync();

            var fileName = "screenshot.png";
            var fileUrl = await UploadScreenshotToStorage(notification.Message.RoomId, fileName, fileData);

            return fileUrl;
        }
    }

    internal async Task<string> UploadScreenshotToStorage(string roomId, string fileName, byte[] bytes)
    {
        var previousFile = new File("screenshots", $"{roomId}/screenshot.png");
        await Files.DeleteAsync(previousFile);

        File file = new("screenshots", $"{roomId}/{fileName}", AccessTypes.Public, new MemoryStream(bytes));
        await Files.AddAsync(file);
        return file.Url!;
    }    
}
