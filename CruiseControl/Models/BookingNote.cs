namespace CruiseControl.Models;
public record BookingNote(
    bool CancellationNote,
    DateTime createDate,
    long NoteNum,
    string Notes,
    bool PopUp,
    bool SameFranchise,
    bool ShowNote,
    string StaffUsername,
    bool SystemNote,
    string Username,
    bool Valid
);
