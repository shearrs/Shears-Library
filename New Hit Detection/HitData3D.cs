using System.Collections.Generic;

public readonly struct HitData3D
{
    private readonly HitBody3D hitBody;
    private readonly HurtBody3D hurtBody;
    private readonly HitResult3D result;
    private readonly IReadOnlyCollection<IHitSubdata> data;

    public readonly HitBody3D HitBody => hitBody;
    public readonly HurtBody3D HurtBody => hurtBody;
    public readonly HitResult3D Result => result;
    public readonly IReadOnlyCollection<IHitSubdata> Data => data;

    public HitData3D(HitBody3D hitBody, HurtBody3D hurtBody,
        HitResult3D result, IReadOnlyCollection<IHitSubdata> data)
    {
        this.hitBody = hitBody;
        this.hurtBody = hurtBody;
        this.result = result;
        this.data = data;
    }

    public readonly bool TryGetData<T>(out T data) where T : IHitSubdata
    {
        if (this.data == null)
        {
            data = default;
            return false;
        }

        foreach (var hitData in this.data)
        {
            if (hitData is T typedData)
            {
                data = typedData;
                return true;
            }
        }

        data = default;
        return false;
    }
}
