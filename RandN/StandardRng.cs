﻿using System;
using RandN.Rngs;

namespace RandN
{
    /// <summary>
    /// A cryptographically secure RNG with good performance.
    /// </summary>
    public sealed class StandardRng : IRng, ICryptoRng
    {
        private static readonly Factory _factory = new Factory();
        private readonly ChaCha _wrapped;

        private StandardRng(ChaCha wrapped) => _wrapped = wrapped;

        /// <summary>
        /// Creates a <see cref="StandardRng"/>.
        /// </summary>
        public static StandardRng Create()
        {
            var rng = ChaCha.GetChaCha8Factory().Create(ThreadLocalRng.Instance);
            return new StandardRng(rng);
        }

        /// <summary>
        /// Gets the <see cref="StandardRng"/> factory.
        /// </summary>
        public static Factory GetFactory() => _factory;

        /// <inheritdoc />
        public void Fill(Span<Byte> buffer) => _wrapped.Fill(buffer);

        /// <inheritdoc />
        public UInt32 NextUInt32() => _wrapped.NextUInt32();

        /// <inheritdoc />
        public UInt64 NextUInt64() => _wrapped.NextUInt64();

        /// <inheritdoc cref="IRngFactory{StandardRng}" />
        public sealed class Factory : IRngFactory<StandardRng>
        {
            internal Factory() { }

            /// <inheritdoc />
            public StandardRng Create() => StandardRng.Create();
        }
    }
}