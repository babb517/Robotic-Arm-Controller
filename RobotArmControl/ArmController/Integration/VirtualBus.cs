using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Windows.Threading;

namespace ArmController.Integration
{
    #region Virtual Bus
    /// <summary>
    /// The virtual bus implements a publish / subscribe architecture to
    /// facilitate extensible communication between modules. Each module
    /// is responsible for subscribing to any relevant nodes, listening for
    /// any value-changed events, and publishing any results they may have.
    /// </summary>
    class VirtualBus
    {
        #region Constants
        /************************************************************************************************************************************/
        /* Public Constants */
        /************************************************************************************************************************************/

        #endregion Constants

        #region Types
        /************************************************************************************************************************************/
        /* Types */
        /************************************************************************************************************************************/
        /// <summary>
        /// A delegate used for subscribing to nodes in the virtual bus.
        /// This delegate is called when the value of the subscribed node has
        /// been changed.
        /// </summary>
        /// <param name="node">The node which has been changed.</param>
        /// <param name="value"></param>
        public delegate void OnValueChangedCallback(BusNode node, Object value);

        /// <summary>
        /// A single entry in the node table.
        /// </summary>
        private class NodeEntry
        {
            public NodeEntry(BusNode node, Object value, List<VirtualBus.OnValueChangedCallback> subscribers)
            {
                Node = node;
                Value = value;
                Subscribers = subscribers;
            }

            public BusNode Node { get; private set; }
            public Object Value { get; set; }
            public List<VirtualBus.OnValueChangedCallback> Subscribers { get; set; }
        }

        #endregion Types

        #region Private Members
        /************************************************************************************************************************************/
        /* Private Members */
        /************************************************************************************************************************************/

        /// <summary>
        /// This is the set of all nodes active nodes in the virtual bus.
        /// </summary>
        private ConcurrentDictionary<BusNode, NodeEntry> _nodes;


        /// <summary>
        /// A set of the nodes which have been recently updated with notifications that need to be sent.
        /// </summary>
        private Queue _updatedNodes;

        /// <summary>
        ///  A dispatcher used for notifying everything on the GUI thread.
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// An object used to track an active notification request.
        /// </summary>
        private DispatcherOperation _notification;

        #endregion Private Members

        #region Constructors
        /************************************************************************************************************************************/
        /* Constructors */
        /************************************************************************************************************************************/
        public VirtualBus(System.Windows.Threading.Dispatcher dispatcher)
        {
            Debug.Write("Initializing the virtual bus...\n");
            _updatedNodes = Queue.Synchronized(new Queue());
            _nodes = new ConcurrentDictionary<BusNode, NodeEntry>();
            _dispatcher = dispatcher;

            foreach (BusNode node in BusNode.Values)
            {
                if (!IsValidType(node.NodeType))
                    throw new ArgumentException("Cannot initialize a node of type '" + node.NodeType.Name + "' as the type is not serializable.");

                _nodes[node] = new NodeEntry(node, null, new List<VirtualBus.OnValueChangedCallback>());
            }
        }
        #endregion Constructors

        #region Public Methods
        /************************************************************************************************************************************/
        /* Public Methods */
        /************************************************************************************************************************************/
        /// <summary>
        /// Returns the current value of a node.
        /// </summary>
        /// <typeparam name="T">The type of the node to retrieve.</typeparam>
        /// <param name="node">The node to retrieve.</param>
        /// <returns>The value of the node cast to the parameterized type. Must be a serializable type.</returns>
        /// <exception cref="InvalidCastException">Thrown if the node can't be cast to the parameterized type.</exception>
        /// <exception cref="ArgumentException">Thrown if T is not a valid type.</exception>
        public T Get<T>(BusNode node)
        {
            T value, copy;

            try
            {
                object tmp = _nodes[node].Value;
                if (tmp != null)
                {
                    value = (T)(tmp);
                }
                else
                {
                    value = default(T);
                }
            }
            catch (InvalidCastException e)
            {
                throw e;
            }

            if (!SerializeCopy(value, out copy))
                throw new ArgumentException("Cannot get a value of type " + typeof(T).Name + " from node " + node + ". The type must be serializeable.");

            return copy;
        }

        /// <summary>
        /// Attempts to attach a value to a node in the virtual bus. Notifies subscribed parties if successful.
        /// </summary>
        /// <typeparam name="T">The type of the object to attach to the node. Must be a serializable type.</typeparam>
        /// <param name="node">The node to attach the value to.</param>
        /// <param name="value">The new value of the node.</param>
        /// <exception cref="ArgumentException">Thrown if the type of T is not valid or is not compatible with the declared type of the node.</exception>
        public void Publish<T>(BusNode node, T value)
        {
            NodeEntry entry;
            T copy;

            if (!SerializeCopy(value, out copy))
                throw new ArgumentException("Cannot publish a value of type " + typeof(T).Name + " to node " + node + ". The type must be serializeable.");


            entry = _nodes[node];

            if (!entry.Node.NodeType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("Cannot publish a value of type " + typeof(T).Name + " to a node " + node + " of type " + entry.Node.NodeType.Name + ". The types are not compatible.");

            lock (entry)
            {
                entry.Value = copy;
            }

            QueueNotification(node);
        }

        /// <summary>
        /// Attempts to subscribe a value-changed listener to a node.
        /// </summary>
        /// <typeparam name="T">The type of the object to attach to the node. Must be a serializable type.</typeparam>
        /// <param name="node">The node to subscribe the listener to.</param>
        /// <param name="callback">The delegate to call when a node has been changed.</param>
        public void Subscribe(BusNode node, OnValueChangedCallback callback)
        {
            NodeEntry entry;

            entry = _nodes[node];

            lock (entry)
            {
                if (!entry.Subscribers.Contains(callback))
                {
                    entry.Subscribers.Add(callback);
                }
            }
        }

        /// <summary>
        /// Attempts to unsubscribe a value-changed listener from a node.
        /// </summary>
        /// <param name="node">The node to unsubscribe the listener from.</param>
        /// <param name="callback">The listener to unsubscribe.</param>
        /// <param name="strict">Whether to throw an exception if something goes wrong.</param>
        /// <exception cref="ArgumentException">Thrown if operating in strict mode and the callback wasn't registered to the node.</exception>
        public void Unsubscribe(BusNode node, OnValueChangedCallback callback, bool strict = true)
        {
            NodeEntry entry = null;

            entry = _nodes[node];

            lock (entry)
            {
                if (!entry.Subscribers.Remove(callback) && strict)
                    throw new ArgumentException("Unable to unsubscribe from node " + node + " as the delegate wasn't subscribed properly.");
            }

        }


        /// <summary>
        /// Queues a node to be notified
        /// </summary>
        /// <param name="node">The node to call the listeners for.</param>
        public void QueueNotification(BusNode node)
        {
            lock (_updatedNodes)
            {
                if (!_updatedNodes.Contains(node))
                {
                    //Debug.WriteLine("Queueing notification...");
                    _updatedNodes.Enqueue(node);
                }

                // add the node to the queue and start the notifier if neccessary.
                if (_notification == null)
                {
                    _notification = _dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(NotifyNodes));
                }
            }

        }

        #endregion Public Methods

        #region Private Methods
        /************************************************************************************************************************************/
        /* Private Methods */
        /************************************************************************************************************************************/

        /// <summary>
        /// Notifies all currently queued nodes.
        /// </summary>
        private void NotifyNodes()
        {
            BusNode node;

            lock (_updatedNodes)
            {
                while (_updatedNodes.Count > 0)
                {
                    //Debug.WriteLine("Notifying nodes.");
                    node = (BusNode)_updatedNodes.Dequeue();
                    Notify(node);
                }

                _notification = null;
            }
        }

        /// <summary>
        /// Calls any listeners for a node.
        /// </summary>
        /// <param name="node">The node to call the listeners for.</param>
        private void Notify(BusNode node)
        {
            NodeEntry entry = null;

            entry = _nodes[node];

            // make a copy of the value.
            object copy;

            SerializeCopy(entry.Value, out copy);

            lock (entry)
            {
                foreach (OnValueChangedCallback c in entry.Subscribers)
                {
                    c(node, copy);
                }
            }

        }

        /// <summary>
        /// Determines if the provided type is valid.
        /// </summary>
        /// <param name="type">The type to check.</param>
        private static bool IsValidType(Type type)
        {
            return type.IsSerializable;
        }


        /// <summary>
        /// Performs a deep copy on the object by serializing an unserializing it.
        /// </summary>
        /// <typeparam name="T">The type of the object to copy. Must be serializable.</typeparam>
        /// <param name="original">The original object.</param>
        /// <param name="copy">The copy of the object. Passed in uniitialized.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private static bool SerializeCopy<T>(T original, out T copy)
        {
            IFormatter formatter;
            Stream stream;


            // Return false if the type isn't serializable.
            if (!IsValidType(typeof(T)))
            {
                copy = default(T);
                return false;
            }
            // Don't serialize a null value.
            else if (Object.ReferenceEquals(original, null))
            {
                copy = default(T);
            }
            // Serialize everything else.
            else
            {
                formatter = new BinaryFormatter();
                stream = new MemoryStream();

                formatter.Serialize(stream, original);
                stream.Seek(0, SeekOrigin.Begin);
                copy = (T)formatter.Deserialize(stream);
            }

            return true;

        }

        #endregion Private Methods
    }
    #endregion Virtual Bus
}
