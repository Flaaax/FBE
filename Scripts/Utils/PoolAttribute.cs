using System;

#nullable enable
namespace FBE.Scripts.Utils;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PoolAttribute(Type poolType) : Attribute
{
    public Type PoolType { get; } = poolType;
}