namespace RandN
{
    /// <summary>
    /// A factory that produces Random Number Generators.
    /// </summary>
    public interface IRngFactory<TRng>
        where TRng : IRng
    {
        /// <summary>
        /// Creates a new <see cref="TRng"/>.
        /// </summary>
        /// <returns>A new <see cref="TRng"/> instance.</returns>
        TRng Create();
    }
}