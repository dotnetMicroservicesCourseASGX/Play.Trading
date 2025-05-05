using System;

namespace Play.Trading.Service.Exceptions;

[Serializable]
internal class UnknownItemException : Exception
{
    private Guid ItemId { get; }

    public UnknownItemException(Guid itemId)
    : base($"Unkown item {itemId}")
    {
        ItemId = itemId;
    }
}