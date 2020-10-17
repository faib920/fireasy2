using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Aliyun.MQ.Runtime.Internal.Util;

namespace Aliyun.MQ.Runtime.Pipeline
{
    public partial class RuntimePipeline : IDisposable
    {
        #region Private members

        bool _disposed;

        // The top-most handler in the pipeline.
        IPipelineHandler _handler;

        #endregion

        #region Properties

        public IPipelineHandler Handler
        {
            get { return _handler; }
        }

        #endregion

        #region Constructors

        public RuntimePipeline(IPipelineHandler handler) 
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            _handler = handler;
        }

        public RuntimePipeline(IList<IPipelineHandler> handlers)
        {
            if (handlers == null || handlers.Count == 0)
                throw new ArgumentNullException("handlers");
            
            foreach (var handler in handlers)
            {
                this.AddHandler(handler);
            }
        }

        #endregion

        #region Invoke methods
        
        public IResponseContext InvokeSync(IExecutionContext executionContext)
        {
            ThrowIfDisposed();

            _handler.InvokeSync(executionContext);            
            return executionContext.ResponseContext;
        }

        public IAsyncResult InvokeAsync(IAsyncExecutionContext executionContext)
        {
            ThrowIfDisposed();

            return _handler.InvokeAsync(executionContext);
        }

        #endregion

        #region Handler methods

        public void AddHandler(IPipelineHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            ThrowIfDisposed();

            var innerMostHandler = GetInnermostHandler(handler);

            if (_handler != null)
            {
                innerMostHandler.InnerHandler = _handler;
                _handler.OuterHandler = innerMostHandler;    
            }
            
            _handler = handler;

            SetHandlerProperties(handler);
        }
        
        public void AddHandlerAfter<T>(IPipelineHandler handler)
            where T : IPipelineHandler
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            ThrowIfDisposed();

            var type = typeof(T);
            var current = _handler;
            while (current != null)
            {
                if (current.GetType() == type)
                {
                    InsertHandler(handler, current);
                    SetHandlerProperties(handler);
                    return;
                }
                current = current.InnerHandler;
            }
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture, "Cannot find a handler of type {0}", type.Name));
        }
                        
        public void AddHandlerBefore<T>(IPipelineHandler handler)
            where T : IPipelineHandler
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            ThrowIfDisposed();

            var type = typeof(T);
            if (_handler.GetType() == type)
            {
                // Add the handler to the top of the pipeline
                AddHandler(handler);
                SetHandlerProperties(handler);
                return;
            }

            var current = _handler;
            while (current != null)
            {
                if (current.InnerHandler != null &&
                    current.InnerHandler.GetType() == type)
                {
                    InsertHandler(handler, current);
                    SetHandlerProperties(handler);
                    return;
                }
                current = current.InnerHandler;
            }

            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture, "Cannot find a handler of type {0}", type.Name));
        }

        public void RemoveHandler<T>()
        {
            ThrowIfDisposed();

            var type = typeof(T);

            IPipelineHandler previous = null;
            var current = _handler;

            while (current != null)
            {
                if (current.GetType() == type)
                {
                    // Cannot remove the handler if it's the only one in the pipeline
                    if (current == _handler && _handler.InnerHandler == null)
                    {
                        throw new InvalidOperationException(
                            "The pipeline contains a single handler, cannot remove the only handler in the pipeline.");
                    }

                    // current is the top, point top to current's inner handler
                    if (current == _handler)                    
                        _handler = current.InnerHandler;
                    

                    // Wireup outer handler to current's inner handler
                    if (current.OuterHandler != null)                    
                        current.OuterHandler.InnerHandler = current.InnerHandler;

                    // Wireup inner handler to current's outer handler
                    if (current.InnerHandler != null)
                        current.InnerHandler.OuterHandler = current.OuterHandler;

                    // Cleanup current
                    current.InnerHandler = null;
                    current.OuterHandler = null;

                    return;
                }

                previous = current;
                current = current.InnerHandler;
            }

            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture, "Cannot find a handler of type {0}", type.Name));
        }

        public void ReplaceHandler<T>(IPipelineHandler handler)
            where T : IPipelineHandler
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            ThrowIfDisposed();

            var type = typeof(T);
            IPipelineHandler previous = null;
            var current = _handler;
            while (current != null)
            {
                if (current.GetType() == type)
                {
                    // Replace current with handler.
                    handler.InnerHandler = current.InnerHandler;
                    handler.OuterHandler = current.OuterHandler;
                    if(previous != null)
                    {
                        // Wireup previous handler
                        previous.InnerHandler = handler;
                    }
                    else
                    {
                        // Current is the top, replace it.
                        _handler = handler;                        
                    }

                    if (current.InnerHandler != null)
                    {
                        // Wireup next handler
                        current.InnerHandler.OuterHandler = handler;
                    }
                    
                    // Cleanup current
                    current.InnerHandler = null;
                    current.OuterHandler = null;

                    SetHandlerProperties(handler);
                    return;
                }
                previous = current;
                current = current.InnerHandler;                
            }
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture, "Cannot find a handler of type {0}", type.Name));
        }

        private static void InsertHandler(IPipelineHandler handler, IPipelineHandler current)
        {
            var next = current.InnerHandler;
            current.InnerHandler = handler;
            handler.OuterHandler = current;
            
            if (next!=null)
            {
                var innerMostHandler = GetInnermostHandler(handler);
                innerMostHandler.InnerHandler = next;
                next.OuterHandler = innerMostHandler;
            }
        }

        private static IPipelineHandler GetInnermostHandler(IPipelineHandler handler)
        {
            Debug.Assert(handler != null);

            var current = handler;
            while (current.InnerHandler != null)
            {
                current = current.InnerHandler;
            }
            return current;
        }

        private void SetHandlerProperties(IPipelineHandler handler)
        {
            ThrowIfDisposed();
        }

        public List<IPipelineHandler> Handlers
        {
            get
            {
                return EnumerateHandlers().ToList();
            }
        }

        public IEnumerable<IPipelineHandler> EnumerateHandlers()
        {
            var handler = this.Handler;
            while(handler != null)
            {
                yield return handler;
                handler = handler.InnerHandler;
            }
        }

        #endregion

        #region Dispose methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                var handler = this.Handler;
                while (handler != null)
                {
                    var innerHandler = handler.InnerHandler;
                    var disposable = handler as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                    handler = innerHandler;
                }

                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        #endregion
    }
}
