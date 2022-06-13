using System.Text.Json.Serialization;

namespace Sandbox.TelegramBot.Models.Responses;

public abstract class BaseResponse
{
    [JsonIgnore]
    public bool Success { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorMessage { get; set; }


    public BaseResponse()
    {
    }
}
