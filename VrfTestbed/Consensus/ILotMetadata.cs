namespace VrfTestbed.Consensus
{
    public interface ILotMetadata
    {
        long Height { get; }

        int Round { get; }
    }
}