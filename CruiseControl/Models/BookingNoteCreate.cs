using System.Text.Json.Serialization;

namespace CruiseControl.Models;
public record BookingNoteCreate(
    [property: JsonPropertyName("notes")] string Notes,
    [property: JsonPropertyName("popUp")] bool PopUp,
    [property: JsonPropertyName("createDate")] DateTime CreateDate,
    [property: JsonPropertyName("showNote")] bool ShowNote,
    [property: JsonPropertyName("systemNote")] bool SystemNote,
    [property: JsonPropertyName("noteTypeCode")] string NoteTypeCode,
    [property: JsonPropertyName("sameFranchise")] bool SameFranchise,
    [property: JsonPropertyName("cancellationNote")] bool CancellationNote,
    [property: JsonPropertyName("staffUserName")] string StaffUsername,
    [property: JsonPropertyName("username")] string Username
);
