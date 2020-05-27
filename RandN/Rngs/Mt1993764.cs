﻿using System;
using RandN.RngHelpers;

namespace RandN.Rngs
{
    /// <summary>
    /// An implementation of Mersenne Twister, variant MT19937-64.
    /// Based on the pseudocode provided from https://en.wikipedia.org/wiki/Mersenne_twister
    /// and the original C code at http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/VERSIONS/C-LANG/mt19937-64.c
    /// </summary>
    public sealed class Mt1993764 : IRng
    {
        private static readonly Factory _factory = new Factory();

        private const Int32 WordSize = sizeof(UInt64) * 8; // 64 bits (w)
        private const Int32 RecurrenceDegree = 312; // (n)
        private const Int32 MiddleWord = 156; // (m)
        private const Int32 OneWordSeparationPoint = 31; // (r)

        private const UInt64 LowerMask = (1ul << OneWordSeparationPoint) - 1;
        private const UInt64 UpperMask = ~LowerMask;

        private const UInt64 TwistMatrixCoefficients = 0xB5026F5AA96619E9; // (a)

        // TGSFSR(R) tempering bitmasks
        private const UInt64 BMask = 0x71D67FFFEDA60000;
        private const UInt64 CMask = 0xFFF7EEE000000000;

        // TGSFSR(R) tempering bit shifts
        private const Int32 SShift = 17;
        private const Int32 TShift = 37;

        // Additional tempering bit shifts and masks
        private const Int32 LShift = 43;
        private const Int32 UShift = 29;
        private const UInt64 DMask = 0x5555555555555555;

        private const UInt64 F = 6364136223846793005;

        private readonly UInt64[] _state;
        private Int32 _index;

        private Mt1993764(UInt64[] state)
        {
            _state = state;
            _index = state.Length;
        }

        public static Mt1993764 Create(UInt64 seed)
        {
            // Init state from seed
            UInt64[] state = new UInt64[RecurrenceDegree];
            state[0] = seed;

            for (UInt32 i = 1; i < state.Length; i++)
            {
                state[i] = unchecked(F * (state[i - 1] ^ (state[i - 1] >> (WordSize - 2))) + i);
            }

            return new Mt1993764(state);
        }

        public static Mt1993764 Create(UInt64[] seed)
        {
            const UInt64 INITIAL_SEED = 19650218ul;
            const UInt64 F1 = 3935559000370003845ul;
            const UInt64 F2 = 2862933555777941757ul;

            var rng = Create(INITIAL_SEED);
            var state = rng._state;
            Int32 stateIndex = 1;
            Int32 seedIndex = 0;
            Int32 iterationCount = Math.Max(RecurrenceDegree, seed.Length);
            for (Int32 i = 0; i < iterationCount; i++)
            {
                state[stateIndex] = unchecked((state[stateIndex] ^ ((state[stateIndex - 1] ^ (state[stateIndex - 1] >> 62)) * F1)) + seed[seedIndex] + (UInt64)seedIndex);
                stateIndex += 1;
                seedIndex += 1;
                if (stateIndex >= state.Length)
                {
                    state[0] = state[state.Length - 1];
                    stateIndex = 1;
                }
                if (seedIndex >= seed.Length)
                    seedIndex = 0;
            }

            for (Int32 k = state.Length - 1; k > 0; k--)
            {
                 state[stateIndex] = unchecked((state[stateIndex] ^ ((state[stateIndex - 1] ^ (state[stateIndex - 1] >> 62)) * F2)) - (UInt64)stateIndex);
                stateIndex += 1;
                if (stateIndex >= state.Length)
                {
                    state[0] = state[state.Length - 1];
                    stateIndex = 1;
                }
            }

            state[0] = 1ul << 63;
            return rng;
        }

        public static Factory GetFactory() => _factory;

        public UInt32 NextUInt32() => Filler.NextUInt32ViaUInt64(this);

        public UInt64 NextUInt64()
        {
            if (_index == _state.Length)
                Twist();

            UInt64 y = _state[_index];
            y ^= (y >> UShift) & DMask;
            y ^= (y << SShift) & BMask;
            y ^= (y << TShift) & CMask;
            y ^= y >> LShift;

            _index++;

            return y;
        }

        public void Fill(Span<Byte> buffer) => Filler.FillBytesViaNext(this, buffer);

        private void Twist()
        {
            for (Int32 i = 0; i < _state.Length; i++)
            {
                UInt64 upper = _state[i] & UpperMask;
                UInt64 lower = _state[(i + 1) % RecurrenceDegree] & LowerMask;
                UInt64 x = upper | lower;

                UInt64 xA = x >> 1;
                if (x % 2 != 0)
                    xA ^= TwistMatrixCoefficients;

                _state[i] = _state[(i + MiddleWord) % RecurrenceDegree] ^ xA;
            }

            _index = 0;
        }

        public sealed class Factory : IReproducibleRngFactory<Mt1993764, UInt64>, IReproducibleRngFactory<Mt1993764, UInt64[]>
        {
            internal Factory() { }

            public Mt1993764 Create(UInt64 seed) => Mt1993764.Create(seed);

            public Mt1993764 Create(UInt64[] seed) => Mt1993764.Create(seed);

            public UInt64[] CreateSeed<TSeedingRng>(TSeedingRng seedingRng) where TSeedingRng : IRng
            {
                UInt64[] seed = new UInt64[RecurrenceDegree];
                Span<Byte> byteSeed = System.Runtime.InteropServices.MemoryMarshal.AsBytes<UInt64>(seed);
                seedingRng.Fill(byteSeed);
                return seed;
            }

            UInt64 IReproducibleRngFactory<Mt1993764, UInt64>.CreateSeed<TSeedingRng>(TSeedingRng seedingRng) => seedingRng.NextUInt64();
        }
    }
}
