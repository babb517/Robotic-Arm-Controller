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

namespace RobotArmControl
{
   
    /// <summary>
    /// The virtual bus implements a publish / subscribe architecture to
    /// facilitate extensible communication between modules. Each module
    /// is responsible for subscribing to any relevant nodes, listening for
    /// any value-changed events, and publishing any results they may have.
    /// </summary>
    class VirtualBus
    {

        /************************************************************************************************************************************/
        /* Public Constants */
        /************************************************************************************************************************************/

        /// <summary>
        /// <summary>
        /// This enumeration stores known data points which may be queried
        /// using the virtual bus.
        /// </summary>
        public static Enum Nodes
        {
            // TODO: Populate the nodes with appropriate values.
        }

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
        public delegate void onValueChangedCallback(int node, Object value);

        /// <summary>
        /// A single entry in the node table.
        /// </summary>
        private class NodeEntry
        {
            public NodeEntry(Type type, Object value, List<VirtualBus.onValueChangedCallback> subscribers)
            {
                this.type = type;
                this.value = value;
                this.subscribers = subscribers;
            }

            public Object value { get; set; }
            public List<VirtualBus.onValueChangedCallback> subscribers { get; set; }
            public Type type { get; private set; }
        }

        /************************************************************************************************************************************/
        /* Private Members */
        /************************************************************************************************************************************/

        /// <summary>
        /// This is the set of all nodes active nodes in the virtual bus.
        /// </summary>
        private ConcurrentDictionary<int, NodeEntry> _nodes;


        /// <summary>
        /// A set of the nodes which have been recently updated with notifications that need to be sent.
        /// </summary>
        private Queue _updatedNodes;

        /// <summary>
        /// The thread that the notification are ran on.
        /// </summary>
        Thread _notifier;

        /************************************************************************************************************************************/
        /* Constructors */
        /************************************************************************************************************************************/
        public VirtualBus()
        {
            _updatedNodes = Queue.Synchronized(new Queue());
            _nodes = new ConcurrentDictionary<int, NodeEntry>();
            _notifier = new Thread(NotifyQueuedNodes);
        }

        /************************************************************************************************************************************/
        /* Public Methods */
        /************************************************************************************************************************************/
        /// <summary>
        /// Returns the current value of a node.
        /// </summary>
        /// <typeparam name="T">The type of the node to retrieve.</typeparam>
        /// <param name="node">The node to retrieve.</param>
        /// <returns>The value of the node cast to the parameterized type. Must be a serializable type.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the node doesn't exist.</exception>
        /// <exception cref="InvalidCastException">Thrown if the node can't be cast to the parameterized type.</exception>
        /// <exception cref="ArgumentException">Thrown if T is not a valid type.</exception>
        public T Get<T>(int node)
        {
            T value,copy;

            try
            {
                value = (T)_nodes[node].value;
            }
            catch (KeyNotFoundException e)
            {
                throw new KeyNotFoundException("Unable to get the value of node " + node + " as the node doesn't exist.", e);
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
        /// Returns the current value of a node.
        /// </summary>
        /// <typeparam name="T">The type of the object to attach to the node. Must be a serializable type.</typeparam>
        /// <param name="node">The node to retrieve.</param>
        /// <param name="defaultValue">A value to return in the event the node doesn't exist.</param>
        /// <returns>The value of the node cast to the parameterized type, or the defaultValue if it doesn't exist.</returns>
        /// <exception cref="InvalidCastException">Thrown if the node can't be cast to the parameterized type.</exception>
        /// <exception cref="ArgumentException">Thrown if T is not a valid type.</exception>
        public T Get<T>(int node, T defaultValue)
        {
            T value,copy;

            try
            {
                value = (T)_nodes[node].value;
            }
            catch (KeyNotFoundException e)
            {
                value = defaultValue;
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
        /// <param name="strict">Whether an exception should be thrown if the node exists. Otherwise the node is created.</param>
        /// <exception cref="KeyNotFoundException">Thrown if operating in strict mode and the node doesn't exist.</exception>
        /// <exception cref="ArgumentException">Thrown if the type of T is not valid or is not compatible with the declared type of the node.</exception>
        /// <returns>True if the already existed, false otherwise.</returns>
        public bool Publish<T>(int node, T value, bool strict = true) {
            bool ret = false;
            NodeEntry entry;
            T copy;

            if (!SerializeCopy(value,out copy))
                throw new ArgumentException("Cannot publish a value of type " + typeof(T).Name + " to node " + node + ". The type must be serializeable.");

            
            try
            {
                entry = _nodes[node];

                if (!entry.type.IsAssignableFrom(typeof(T)))
                    throw new ArgumentException("Cannot publish a value of type " + typeof(T).Name + " to a node " + node + " of type " + entry.type.Name + ". The types are not compatible.");

                lock (entry)
                {
                    entry.value = copy;
                }
            }
            catch (KeyNotFoundException e)
            {
                if (strict)
                    throw new KeyNotFoundException("Unable to attach value " + value + " to node " + node + " as the node doesn't exist.", e);
                else {
                    Create(node, copy);
                    ret = true;
                }
            }

            QueueNotification(node);

            return ret;
        }

        /// <summary>
        /// Attempts to create a new node on the virtual bus
        /// </summary>
        /// <typeparam name="T">The type of the object to attach to the node. Must be a serializable type.</typeparam>
        /// <param name="node">The identifier for the new node.</param>
        /// <param name="value">The initial value of the node.</param>
        /// <param name="strict">Whether to throw an exception if the node already exists.</param>
        /// <exception cref="ArgumentException">Thrown if operating in strict mode and the node already exists, or T is not a valid type.</exception>
        /// <returns>True if the node was created.</returns>
        public bool Create<T>(int node, T value = default(T), bool strict = true)
        {
            T copy;
            bool ret = true;

            if (!SerializeCopy(value, out copy))
                throw new ArgumentException("Cannot create node " + node + " of type " + typeof(T).Name + ". The type must be serializable.");

            NodeEntry newNode = new NodeEntry(typeof(T),value, new List<onValueChangedCallback>());

            if (!_nodes.TryAdd(node, newNode))
            {
                if (strict)
                    throw new ArgumentException("Unable to create node " + node + " as the node already exists.");
                else
                    ret = false;
            }
             
            _nodes[node].value = value;

            return ret;
        }


        /// <summary>
        /// Attempts to subscribe a value-changed listener to a node.
        /// </summary>
        /// <typeparam name="T">The type of the object to attach to the node. Must be a serializable type.</typeparam>
        /// <param name="node">The node to subscribe the listener to.</param>
        /// <param name="callback">The delegate to call when a node has been changed.</param>
        /// <param name="strict">Whether to throw an exception if the node doesn't exists. The node is created otherwise.</param>
        /// <exception cref="KeyNotFoundException">Thrown if operating in strict mode and the node doesn't exist.</exception>
        /// <exception cref="ArgumentException">Thrown if T is not a valid type.</exception>
        /// <returns>True if the node already existed, false if it was created.</returns>
        public bool Subscribe<T>(int node, onValueChangedCallback callback, bool strict = true) 
        {
            NodeEntry entry;

            if (!IsValidType(typeof(T)))
                throw new ArgumentException("Cannot subscribe to node " + node + " with type " + typeof(T).Name + ". The type must be serializable.");

            try
            {
                entry = _nodes[node];

                lock (entry)
                {
                    if (!entry.subscribers.Contains(callback))
                    {
                        entry.subscribers.Add(callback);
                    }
                }
            }
            catch (KeyNotFoundException e)
            {
                if (strict)
                    throw new KeyNotFoundException("Unable to subscribe to node " + node + " as the node doesn't exist.", e);
                else
                {
                    Create<T>(node);
                    entry = _nodes[node];
                    lock (entry)
                    {
                        if (!entry.subscribers.Contains(callback))
                        {
                            entry.subscribers.Add(callback);
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to unsubscribe a value-changed listener from a node.
        /// </summary>
        /// <param name="node">The node to unsubscribe the listener from.</param>
        /// <param name="callback">The listener to unsubscribe.</param>
        /// <param name="strict">Whether to throw an exception if the node doesn't exist or the listener wasn't subscribed to the node.</param>
        /// <exception cref="KeyNotFoundException">Thrown if operating in strict mode and the node doesn't exist.</exception>
        /// <exception cref="ArgumentException">Thrown if operating in strict mode and the callback wasn't registered to the node.</exception>
        /// <returns>True if successful, false otherwise.</returns>
        public bool Unsubscribe(int node, onValueChangedCallback callback, bool strict = true)
        {
            NodeEntry entry = null;

            try
            {
                entry = _nodes[node];
            }
            catch (KeyNotFoundException e)
            {
                if (strict)
                    throw new KeyNotFoundException("Unable to unsubscribe from node " + node + " as the node doesn't exist.", e);
                else
                    return false;
            }

            lock (entry)
            {
                if (!entry.subscribers.Remove(callback) && strict)
                    throw new ArgumentException("Unable to unsubscribe from node " + node + " as the delegate wasn't subscribed properly.");
            }

            return true;

        }


        /// <summary>
        /// Queues a node to be notified
        /// </summary>
        /// <param name="node">The node to call the listeners for.</param>
        /// <param name="strict">Whether we should throw an exception if the node doesn't exist.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the node doesn't exist.</exception>
        /// <returns>True if successful, false otherwise.</returns>
        public bool QueueNotification(int node, bool strict = true)
        {
            NodeEntry entry = null;

            try
            {
                entry = _nodes[node];
            }
            catch (KeyNotFoundException e)
            {
                if (strict)
                    throw new KeyNotFoundException("Unable to notify listeners on node " + node + " as the node doesn't exist.", e);
                else
                    return false;
            }

            // add the node to the queue and start the notifier if neccessary.
            _updatedNodes.Enqueue(node);
            if (!_notifier.IsAlive) _notifier.Start();

            return true;
        }


        /************************************************************************************************************************************/
        /* Private Methods */
        /************************************************************************************************************************************/

        /// <summary>
        /// Notifies all currently queued nodes.
        /// </summary>
        private void NotifyQueuedNodes()
        {
            int node;

            while (_updatedNodes.Count > 0)
            {
                node = (int)_updatedNodes.Dequeue();
                Notify(node);
            }
        }

        /// <summary>
        /// Calls any listeners for a node.
        /// </summary>
        /// <param name="node">The node to call the listeners for.</param>
        /// <param name="strict">Whether we should throw an exception if the node doesn't exist.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the node doesn't exist.</exception>
        /// <returns>True if successful, false otherwise. </returns>
        private bool Notify(int node, bool strict = true)
        {
            NodeEntry entry = null;

            try
            {
                entry = _nodes[node];
            }
            catch (KeyNotFoundException e)
            {
                if (strict)
                    throw new KeyNotFoundException("Unable to notify listeners on node " + node + " as the node doesn't exist.", e);
                else
                    return false;
            }

            lock (entry)
            {
                foreach (onValueChangedCallback c in entry.subscribers)
                {
                    c(node, entry.value);
                }
            }
            return true;
        }

        /// <summary>
        /// Determines if the provided type
        /// </summary>
        /// <param name="type"></param>
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
            else if (Object.ReferenceEquals(original,null))
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

    }
}
