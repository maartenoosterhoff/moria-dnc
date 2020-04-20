namespace Moria.Core.Structures.Enumerations
{
    // The status of the customer bid.
    // Note: a received bid may still result in a rejected offer.
    public enum BidState
    {
        Received = 0, // the big was received successfully
        Rejected,     // the bid was rejected, or cancelled by the customer
        Offended,     // customer tried to sell an undesirable item
        Insulted,     // the store owner was insulted too many times by the bid
    };
}
