﻿using System.Collections.Concurrent;
using System.Numerics;
using VrfTestbed.Consensus;

namespace VrfTestbed.VrfLib
{
    public class ProofSet
    {
        private IReadOnlyList<byte> _payload;
        private ConcurrentDictionary<BlsPublicKey, (Proof, BigInteger)> _proofs;
        private BlsSignature? _aggregatedSignature;
        private (BlsPublicKey, Proof, BigInteger)? _dominantProof;
        private BigInteger _totalPower;

        public ProofSet(IReadOnlyList<byte> payload)
        {
            _payload = payload;
            _proofs = new ConcurrentDictionary<BlsPublicKey, (Proof, BigInteger)>();
            _aggregatedSignature = null;
            _dominantProof = null;
            _totalPower = BigInteger.Zero;
        }

        public ProofSet(long height, int round)
            : this(new LotMetadata(height, round).ByteArray) { }

        public void Add(BlsPublicKey publicKey, Proof proof, BigInteger power)
        {
            if (proof.Verify(publicKey, _payload) && _proofs.TryAdd(publicKey, (proof, power)))
            {
                _aggregatedSignature = _aggregatedSignature is { } sig
                    ? sig.Aggregate(proof.Signature)
                    : proof.Signature;
                _dominantProof = null;
                _totalPower += power;
            }
        }

        public void Add(BlsPublicKey publicKey, BlsSignature signature, BigInteger power)
            => Add(publicKey, new Proof(signature), power);


        public void Add(IEnumerable<(BlsPublicKey, Proof, BigInteger)> proofs)
        {
            foreach (var proof in proofs)
            {
                Add(proof.Item1, proof.Item2, proof.Item3);
            }
        }

        public void Add(IEnumerable<(BlsPublicKey, BlsSignature, BigInteger)> proofs)
        {
            foreach (var proof in proofs)
            {
                Add(proof.Item1, proof.Item2, proof.Item3);
            }
        }


        public bool Verify()
            => _aggregatedSignature?.Verify(_proofs.Keys.ToArray(), _payload) ?? throw new Exception("Empty Proof");


        public (BlsPublicKey, Proof, BigInteger) DominantProof
        {
            get
            {
                if (_aggregatedSignature is null)
                {
                    throw new Exception("Empty Proof");
                }

                if (_dominantProof is { } dominantProof)
                {
                    return dominantProof;
                }
                else
                {
                    var selectedItem = _proofs.MaxBy(proof => proof.Value.Item1.Select(1, proof.Value.Item2, _totalPower));
                    _dominantProof = (selectedItem.Key, selectedItem.Value.Item1, selectedItem.Value.Item2);
                    return ((BlsPublicKey, Proof, BigInteger))_dominantProof;
                }
            }
        }

        public int Seed()
            => DominantProof.Item2.Seed();
    }
}
