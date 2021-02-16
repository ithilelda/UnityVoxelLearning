using System;

public static class GameDefines
{
    public const int CHUNK_BIT = 4;
    public const int CHUNK_SIZE = 1 << CHUNK_BIT;
    public const int CHUNK_MASK = CHUNK_SIZE - 1;
    public const int CHUNK_SIZE_SQUARED = CHUNK_SIZE * CHUNK_SIZE;
    public const int CHUNK_SIZE_CUBED = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;

}
