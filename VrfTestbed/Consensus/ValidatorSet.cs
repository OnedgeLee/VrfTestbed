using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Numerics;
using Bencodex;
using VrfTestbed.VrfLib;

namespace VrfTestbed.Consensus
{
    public class ValidatorSet : IEquatable<ValidatorSet>, IBencodable
    {
        public ValidatorSet()
            : this(new List<Validator>())
        {
        }

        public ValidatorSet(List<Validator> validators)
        {
            if (validators
                .Select(validators => validators.PublicKey)
                .Distinct()
                .Count() != validators.Count)
            {
                throw new ArgumentException("All public keys for validators must be unique.");
            }
            else if (validators.Any(validator => validator.Power == BigInteger.Zero))
            {
                throw new ArgumentException("All validators must have positive power.");
            }

            Validators = validators
                //.OrderBy(validator => validator.PublicKey) : Public key order have to be resolved
                .ToImmutableList();
        }

        public ValidatorSet(Bencodex.Types.IValue bencoded)
            : this(bencoded is Bencodex.Types.List list
                ? list
                : throw new ArgumentException(
                    $"Given {nameof(bencoded)} must be of type " +
                    $"{typeof(Bencodex.Types.List)}: {bencoded.GetType()}",
                    nameof(bencoded)))
        {
        }

        private ValidatorSet(Bencodex.Types.List bencoded)
            : this(bencoded.Select(elem => new Validator(elem)).ToList())
        {
        }

        public ImmutableList<Validator> Validators { get; }

        public ImmutableList<BlsPublicKey> PublicKeys => Validators.Select(
            validator => validator.PublicKey).ToImmutableList();

        public int TotalCount => Validators.Count;

        public BigInteger TotalPower => Validators.Aggregate(
            BigInteger.Zero, (total, next) => total + next.Power);

        public int TwoThirdsCount => TotalCount * 2 / 3;

        public BigInteger TwoThirdsPower => TotalPower * 2 / 3;

        public int OneThirdCount => TotalCount / 3;

        public BigInteger OneThirdPower => TotalPower / 3;

        public Bencodex.Types.IValue Bencoded =>
            new Bencodex.Types.List(Validators.Select(validator => validator.Bencoded));

        public Validator this[int index] => Validators[index];

        public int FindIndex(BlsPublicKey publicKey) => Validators.FindIndex(
            validator => validator.PublicKey.Equals(publicKey));

        public Validator? GetValidator(BlsPublicKey publicKey)
            => Validators.Find(validator => validator.PublicKey == publicKey);

        public ImmutableList<Validator> GetValidators(IEnumerable<BlsPublicKey> publicKeys)
            => (from publicKey in publicKeys select GetValidator(publicKey)).ToImmutableList();

        public BigInteger GetValidatorsPower(List<BlsPublicKey> publicKeys)
        {
            return GetValidators(publicKeys).Aggregate(
                BigInteger.Zero, (total, next) => total + next.Power);
        }

        public bool Contains(Validator validator) => Validators.Contains(validator);

        public bool ContainsPublicKey(BlsPublicKey publicKey) =>
            Validators.Any(validator => validator.PublicKey.Equals(publicKey));

        [Pure]
        public ValidatorSet Update(Validator validator)
        {
            var updated = Validators.ToList();

            updated.RemoveAll(v => v.PublicKey.Equals(validator.PublicKey));

            if (validator.Power == BigInteger.Zero)
            {
                return new ValidatorSet(updated);
            }
            else
            {
                updated.Add(validator);
                return new ValidatorSet(updated);
            }
        }

        public bool Equals(ValidatorSet? other) =>
            other is ValidatorSet validators && Validators.SequenceEqual(validators.Validators);

        public override bool Equals(object? obj) => obj is ValidatorSet other && Equals(other);

        public override int GetHashCode()
        {
            int hashCode = 17;
            foreach (Validator validator in Validators)
            {
                hashCode = unchecked(hashCode * (31 + validator.GetHashCode()));
            }

            return hashCode;
        }

        public Validator GetProposer(long height, int round)
        {
            return Validators.IsEmpty
                ? throw new InvalidOperationException(
                    "Cannot select a proposer from an empty list of validators.")
                : Validators[(int)((height + round) % Validators.Count)];
        }
    }
}
