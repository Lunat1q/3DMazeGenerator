namespace MazeGeneratorCore
{
    public enum MazeGenerationType
    {
        Latest, // always take latest cell to continue
        Random, // take any previous cell to continue
        HalfHalf, // 50/50 Latest/Random
        MostlyLatest, // 75/25 Latest/Random
        MostlyRandom // 25/75 Latest/Random
    }
}