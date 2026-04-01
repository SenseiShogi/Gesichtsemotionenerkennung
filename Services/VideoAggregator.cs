using System.Collections.Generic;
using System.Linq;
using Gesichtsemotionenerkennung.Models;

namespace Gesichtsemotionenerkennung.Services
{
    public class VideoAggregator
    {
        // Sammlung aller Frame-Kontexte
        private readonly List<UnifiedFrameContext> _contexts = new List<UnifiedFrameContext>();

        // Lock-Objekt für Thread-Sicherheit
        private readonly object _lock = new object();

        // Fügt einen Frame-Kontext hinzu (Thread-sicher)
        public void AddFrameContext(UnifiedFrameContext context)
        {
            lock (_lock)
            {
                _contexts.Add(context);
            }
        }

        // Gibt alle gespeicherten Frame-Kontexte zurück (Thread-sicher)
        public List<UnifiedFrameContext> GetAllContexts()
        {
            lock (_lock)
            {
                return _contexts.ToList();
            }
        }

        // Löscht alle gespeicherten Frame-Kontexte
        public void Clear()
        {
            lock (_lock)
            {
                _contexts.Clear();
            }
        }

        // Anzahl der gespeicherten Frame-Kontexte
        public int Count => _contexts.Count;
    }
}