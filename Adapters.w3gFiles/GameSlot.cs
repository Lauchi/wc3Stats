namespace Adapters.w3gFiles
{
    public class GameSlot
    {
        public int PlayerId { get; }
        public SlotUsage SlotUsage { get; }
        public bool IsHuman { get; }
        public int TeamNumber { get; }
        public Race Race { get; }

        public GameSlot(int playerId, SlotUsage slotUsage, bool isHuman, int teamNumber, Race race)
        {
            PlayerId = playerId;
            SlotUsage = slotUsage;
            IsHuman = isHuman;
            TeamNumber = teamNumber;
            Race = race;
        }
    }
}