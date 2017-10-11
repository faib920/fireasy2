#if NET35
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading;

namespace Fireasy.Data.Entity.Validation
{
    public sealed class ValidationContext : IServiceProvider
    {
        private string _displayName;
        private Dictionary<object, object> _items;
        private string _memberName;
        private object _objectInstance;
        private IServiceContainer _serviceContainer;
        private Func<Type, object> _serviceProvider;

        public ValidationContext(object instance)
            : this(instance, null, null)
        {
        }

        public ValidationContext(object instance, IDictionary<object, object> items)
            : this(instance, null, items)
        {
        }

        public ValidationContext(object instance, IServiceProvider serviceProvider, IDictionary<object, object> items)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (serviceProvider != null)
            {
                //<>c__DisplayClass0 class2;
                //this.InitializeServiceProvider(new Func<Type, object>(class2.<_ctor>b__1));
            }
            IServiceContainer parentContainer = serviceProvider as IServiceContainer;
            if (parentContainer != null)
            {
                this._serviceContainer = new ValidationContextServiceContainer(parentContainer);
            }
            else
            {
                this._serviceContainer = new ValidationContextServiceContainer();
            }
            if (items != null)
            {
                this._items = new Dictionary<object, object>(items);
            }
            else
            {
                this._items = new Dictionary<object, object>();
            }
            this._objectInstance = instance;
        }

        private string GetDisplayName()
        {
            return this.MemberName;
        }

        public object GetService(Type serviceType)
        {
            object service = null;
            if (this._serviceContainer != null)
            {
                service = this._serviceContainer.GetService(serviceType);
            }
            if ((service == null) && (this._serviceProvider != null))
            {
                service = this._serviceProvider(serviceType);
            }
            return service;
        }

        public void InitializeServiceProvider(Func<Type, object> serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this._displayName))
                {
                    this._displayName = this.GetDisplayName();
                    if (string.IsNullOrEmpty(this._displayName))
                    {
                        this._displayName = this.MemberName;
                        if (string.IsNullOrEmpty(this._displayName))
                        {
                            this._displayName = this.ObjectType.Name;
                        }
                    }
                }
                return this._displayName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                this._displayName = value;
            }
        }

        public IDictionary<object, object> Items
        {
            get
            {
                return this._items;
            }
        }

        public string MemberName
        {
            get
            {
                return this._memberName;
            }
            set
            {
                this._memberName = value;
            }
        }

        public object ObjectInstance
        {
            get
            {
                return this._objectInstance;
            }
        }

        public Type ObjectType
        {
            get
            {
                return this.ObjectInstance.GetType();
            }
        }

        public IServiceContainer ServiceContainer
        {
            get
            {
                if (this._serviceContainer == null)
                {
                    this._serviceContainer = new ValidationContextServiceContainer();
                }
                return this._serviceContainer;
            }
        }

        private class ValidationContextServiceContainer : IServiceContainer, IServiceProvider
        {
            private readonly object _lock;
            private IServiceContainer _parentContainer;
            private Dictionary<Type, object> _services;

            internal ValidationContextServiceContainer()
            {
                this._services = new Dictionary<Type, object>();
                this._lock = new object();
            }

            internal ValidationContextServiceContainer(IServiceContainer parentContainer)
            {
                this._services = new Dictionary<Type, object>();
                this._lock = new object();
                this._parentContainer = parentContainer;
            }

            public void AddService(Type serviceType, ServiceCreatorCallback callback)
            {
                this.AddService(serviceType, callback, true);
            }

            public void AddService(Type serviceType, object serviceInstance)
            {
                this.AddService(serviceType, serviceInstance, true);
            }

            public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
            {
                if (promote && (this._parentContainer != null))
                {
                    this._parentContainer.AddService(serviceType, callback, promote);
                }
                else
                {
                    object obj2 = null;
                    try
                    {
                        obj2 = this._lock;
                        Monitor.Enter(obj2);
                        if (this._services.ContainsKey(serviceType))
                        {
                            object[] args = new object[] { serviceType };
                            throw new ArgumentException();
                        }
                        this._services.Add(serviceType, callback);
                    }
                    finally
                    {
                        Monitor.Exit(obj2);
                    }
                }
            }

            public void AddService(Type serviceType, object serviceInstance, bool promote)
            {
                if (promote && (this._parentContainer != null))
                {
                    this._parentContainer.AddService(serviceType, serviceInstance, promote);
                }
                else
                {
                    object obj2 = null;
                    try
                    {
                        obj2 = this._lock;
                        Monitor.Enter(obj2);
                        if (this._services.ContainsKey(serviceType))
                        {
                            object[] args = new object[] { serviceType };
                            throw new ArgumentException();
                        }
                        this._services.Add(serviceType, serviceInstance);
                    }
                    finally
                    {
                        Monitor.Exit(obj2);
                    }
                }
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == null)
                {
                    throw new ArgumentNullException("serviceType");
                }
                object service = null;
                this._services.TryGetValue(serviceType, out service);
                if ((service == null) && (this._parentContainer != null))
                {
                    service = this._parentContainer.GetService(serviceType);
                }
                ServiceCreatorCallback callback = service as ServiceCreatorCallback;
                if (callback != null)
                {
                    service = callback(this, serviceType);
                }
                return service;
            }

            public void RemoveService(Type serviceType)
            {
                this.RemoveService(serviceType, true);
            }

            public void RemoveService(Type serviceType, bool promote)
            {
                object obj2 = null;
                try
                {
                    obj2 = this._lock;
                    Monitor.Enter(obj2);
                    if (this._services.ContainsKey(serviceType))
                    {
                        this._services.Remove(serviceType);
                    }
                }
                finally
                {
                    Monitor.Exit(obj2);
                }
                if (promote && (this._parentContainer != null))
                {
                    this._parentContainer.RemoveService(serviceType);
                }
            }
        }
    }
}
#endif