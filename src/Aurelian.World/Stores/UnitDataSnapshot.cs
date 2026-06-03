using Aurelian.World.Units;

namespace Aurelian.World.Stores;

public sealed record UnitDataSnapshot(
    UnitId Id,
    UnitKindId Kind,
    UnitLogicRef? Logic,
    UnitName? Name,
    Transform2 Transform,
    IReadOnlyList<UnitId> ImmediateChildren);
