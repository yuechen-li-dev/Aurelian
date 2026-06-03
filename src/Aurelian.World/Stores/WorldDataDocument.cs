using Aurelian.World.Units;

namespace Aurelian.World.Stores;

public sealed record WorldDataDocument(
    WorldDocument World,
    UnitNameStore Names,
    Transform2Store Transforms)
{
    public static WorldDataDocument FromWorld(WorldDocument world) =>
        new(world, UnitNameStore.Empty, Transform2Store.Empty);

    public WorldDataDocument WithWorld(WorldDocument world) => this with { World = world };

    public WorldDataDocument WithNames(UnitNameStore names) => this with { Names = names };

    public WorldDataDocument WithTransforms(Transform2Store transforms) => this with { Transforms = transforms };
}
