using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ArmController.Integration
{
    /// <summary>
    /// An abstract interface for each of the modules.
    /// </summary>
    /// <remarks>New modules should inherit from this class and should be instantiated in <see cref="MainWindow.WindowLoaded"/>.</remarks>
    abstract class Module
    {

        /**************************************************************************************/
        /* Protected Members */
        /**************************************************************************************/

        /// <summary>
        /// A reference to the virtual bus shared between the modules.
        /// </summary>
        protected VirtualBus Bus { get; private set; }

        /**************************************************************************************/
        /* Public Methods */
        /**************************************************************************************/

        /// <summary>
        /// Requests that the module be initialized and begins communicating on the specified virtual bus.
        /// </summary>
        /// <param name="bus">The virtual bus to communicate on.</param>
        public void InitializeModule(VirtualBus bus)
        {
            Bus = bus;
            OnInitialize();
        }

        /// <summary>
        /// Requests that the module finalize its state in preparation to exit the program.
        /// </summary>
        public void FinalizeModule()
        {
            OnFinalize();
        }

        /**************************************************************************************/
        /* Protected Methods */
        /**************************************************************************************/

        /// <summary>
        /// Sends a message to the controller requesting that the program exit.
        /// </summary>
        protected void RequestStop()
        {
            Debug.WriteLine("Requesting that the program stops.");
            if (Bus != null)
            {
                Bus.Publish(BusNode.STOP_REQUESTED, true);
            }
        }

        /// <summary>
        /// An event handling method called when the controller has requested that the module initialize its state.
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// An event handling method called when the controller has requested that the module finalize its state.
        /// </summary>
        protected abstract void OnFinalize();

    }
}
