using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


var authoringKey = "304fd494640b4b77b681bd52860c445c";
var authoringURL = "https://sampleprojectcsharp.cognitiveservices.azure.com/";
var queryingURL = "https://evachatbot.azurewebsites.net";
var kbId="54184de1-2c7f-4fb7-8e25-d5187963acfa";

var client = new QnAMakerClient(new ApiKeyServiceClientCredentials(authoringKey))
{ Endpoint = authoringURL };

    
//CreateSampleKb(client).Wait();
UpdateKB(client,kbId).Wait();
PublishKb(client,kbId).Wait();

 static async Task<string> CreateSampleKb(IQnAMakerClient client)
{
    

    var file1 = new FileDTO
    {
        FileName = "Book 5",
        //FileUri = "https://filebin.net/2nt7gn13z6bjn3u0/Book_5.xlsx",

    };

    var createKbDto = new CreateKbDTO
    {
        Name = "QnA Maker .NET SDK Quickstart",
        Files = new List<FileDTO> { file1 }

    };

    var createOp = await client.Knowledgebase.CreateAsync(createKbDto);
    createOp = await MonitorOperation(client, createOp);

    return createOp.ResourceLocation.Replace("/knowledgebases/", string.Empty);
}


  static async Task PublishKb(IQnAMakerClient client, string kbId)
{
    await client.Knowledgebase.PublishAsync(kbId);
}


  static async Task UpdateKB(IQnAMakerClient client, string kbId)
{
    var uri = new System.Uri(@"C:\Users\abhey\source\repos\qna-maker-quickstart\Book 5.xlsx");
    var converted = uri.AbsoluteUri;
    
    string urlFile=@"C:\Users\abhey\source\repos\qna-maker-quickstart\Book 5.xlsx";
    var file1 = new FileDTO
    {
        FileName = "Book 5.xlsx",
        FileUri= urlFile,
        //FileUri = "https://filebin.net/2nt7gn13z6bjn3u0/Book_5.xlsx",

    };

     var updateOp = await client.Knowledgebase.UpdateAsync(kbId, new UpdateKbOperationDTO
    {
        // Create JSON of changes
        Add = new UpdateKbOperationDTOAdd
        {

            Files = new List<FileDTO> { file1 },

            QnaList = new List<QnADTO> {
                new QnADTO {
                    Questions = new List<string> {
                        "Current Time?",
                    },
                    Answer = "Its 11:15",
                },
            },
        },
        
        Update = null,
        Delete = null
    }); ;

    // Loop while operation is success
    updateOp = await MonitorOperation(client, updateOp);
}

 static async Task<Operation> MonitorOperation(IQnAMakerClient client, Operation operation)
{
    // Loop while operation is success
    for (int i = 0;
        i < 20 && (operation.OperationState == OperationStateType.NotStarted || operation.OperationState == OperationStateType.Running);
        i++)
    {
        Console.WriteLine("Waiting for operation: {0} to complete.", operation.OperationId);
        await Task.Delay(5000);
        operation = await client.Operations.GetDetailsAsync(operation.OperationId);
    }

    if (operation.OperationState != OperationStateType.Succeeded)
    {
        throw new Exception($"Operation {operation.OperationId} failed to completed.");
    }
    return operation;
}
