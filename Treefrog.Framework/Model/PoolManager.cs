﻿using System;
using System.Collections.Generic;
using Treefrog.Framework.Compat;

namespace Treefrog.Framework.Model
{
    public interface IPoolManager<TPool>
        where TPool : class, IResource
    {
        IResourceCollection<TPool> Pools { get; }
        void Reset ();
        TPool PoolFromItemKey (Guid key);
        bool Contains (Guid key);

        event EventHandler<ResourceEventArgs<TPool>> PoolAdded;
        event EventHandler<ResourceEventArgs<TPool>> PoolRemoved;
        event EventHandler<ResourceEventArgs<TPool>> PoolModified;
    }

    public abstract class PoolManager<TPool, TPoolItem> : IPoolManager<TPool>
        where TPool : class, IResource, IResourceManager<TPoolItem>
        where TPoolItem : IResource
    {
        private ResourceCollection<TPool> _pools;
        private Dictionary<Guid, EventHandler<ResourceEventArgs<TPoolItem>>> _poolResourceAddHandlers;
        private Dictionary<Guid, EventHandler<ResourceEventArgs<TPoolItem>>> _poolResourceRemoveHandlers;

        private Dictionary<Guid, TPool> _poolIndexMap;

        protected PoolManager ()
        {
            _pools = new ResourceCollection<TPool>();
            _poolResourceAddHandlers = new Dictionary<Guid, EventHandler<ResourceEventArgs<TPoolItem>>>();
            _poolResourceRemoveHandlers = new Dictionary<Guid, EventHandler<ResourceEventArgs<TPoolItem>>>();

            _poolIndexMap = new Dictionary<Guid, TPool>();

            _pools.ResourceAdded += HandleResourceAdded;
            _pools.ResourceRemoved += HandleResourceRemoved;
            _pools.ResourceModified += HandleResourceModified;
        }

        private void HandleResourceRemoved (object sender, ResourceEventArgs<TPool> e)
        {
            if (_poolResourceAddHandlers.ContainsKey(e.Uid)) {
                e.Resource.ResourceAdded -= _poolResourceAddHandlers[e.Uid];
                _poolResourceAddHandlers.Remove(e.Uid);
            }

            if (_poolResourceRemoveHandlers.ContainsKey(e.Uid)) {
                e.Resource.ResourceRemoved -= _poolResourceRemoveHandlers[e.Uid];
                _poolResourceRemoveHandlers.Remove(e.Uid);
            }

            List<Guid> removeQueue = new List<Guid>();
            foreach (var item in _poolIndexMap) {
                if (item.Value == e.Resource)
                    removeQueue.Add(item.Key);
            }

            foreach (Guid key in removeQueue)
                _poolIndexMap.Remove(key);

            OnPoolRemoved(e.Resource);
        }

        private void HandleResourceAdded (object sender, ResourceEventArgs<TPool> e)
        {
            _poolResourceAddHandlers[e.Uid] = (s, es) => { _poolIndexMap.Add(es.Uid, e.Resource); };
            _poolResourceRemoveHandlers[e.Uid] = (s, es) => { _poolIndexMap.Remove(es.Uid); };

            e.Resource.ResourceAdded += _poolResourceAddHandlers[e.Uid];
            e.Resource.ResourceRemoved += _poolResourceRemoveHandlers[e.Uid];

            foreach (TPoolItem item in e.Resource)
                _poolIndexMap.Add(item.Uid, e.Resource);

            OnPoolAdded(e.Resource);
        }

        private void HandleResourceModified (object sender, ResourceEventArgs<TPool> e)
        {
            OnPoolModified(e.Resource);
        }

        public virtual IResourceCollection<TPool> Pools
        {
            get { return _pools; }
        }

        public virtual IEnumerable<Guid> Keys
        {
            get { return _poolIndexMap.Keys; }
        }

        public event EventHandler<ResourceEventArgs<TPool>> PoolAdded;
        public event EventHandler<ResourceEventArgs<TPool>> PoolRemoved;
        public event EventHandler<ResourceEventArgs<TPool>> PoolModified;

        protected virtual void OnPoolAdded (TPool pool)
        {
            var ev = PoolAdded;
            if (ev != null)
                ev(this, new ResourceEventArgs<TPool>(pool));
        }

        protected virtual void OnPoolRemoved (TPool pool)
        {
            var ev = PoolRemoved;
            if (ev != null)
                ev(this, new ResourceEventArgs<TPool>(pool));
        }

        protected virtual void OnPoolModified (TPool pool)
        {
            var ev = PoolModified;
            if (ev != null)
                ev(this, new ResourceEventArgs<TPool>(pool));
        }

        public virtual void Reset ()
        {
            _pools.Clear();

            _poolResourceAddHandlers.Clear();
            _poolResourceRemoveHandlers.Clear();
            _poolIndexMap.Clear();
        }

        public virtual bool Contains (Guid key)
        {
            return _poolIndexMap.ContainsKey(key);
        }

        public virtual TPool PoolFromItemKey (Guid key)
        {
            TPool item;
            if (_poolIndexMap.TryGetValue(key, out item))
                return item;
            return null;
        }
    }
}
