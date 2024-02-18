using System.Runtime.Serialization;

namespace CatMessenger.Core.Message;

public enum ClickEvent
{
    [EnumMember(Value = "open_url")] OpenUrl,

    [EnumMember(Value = "run_command")] RunCommand,

    [EnumMember(Value = "suggest_command")]
    SuggestCommand,

    [EnumMember(Value = "copy")] Copy
}