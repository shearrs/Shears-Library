using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitRay2D
    {
        private readonly RaycastHit2D[] results;
        private readonly Dictionary<Collider2D, RaycastHit2D> validHits = new();
        private int hits;

        public RaycastHit2D[] Results => results;
        public IReadOnlyDictionary<Collider2D, RaycastHit2D> ValidHits => validHits;
        public int Hits => hits;

        public HitRay2D(int maxResults)
        {
            results = new RaycastHit2D[maxResults];
        }

        public void Cast(Vector2 origin, Vector2 direction, ContactFilter2D filter, float distance)
        {
            hits = Physics2D.Raycast(origin, direction, filter, results, distance);
        }

        public void AddValidHit(RaycastHit2D hit) => validHits.Add(hit.collider, hit);

        public void SetValidHit(Collider2D collider, RaycastHit2D hit) => validHits[collider] = hit;

        public void ClearValidHits() => validHits.Clear();
    }
}
