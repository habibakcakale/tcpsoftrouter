using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace SharpOpen.Net
{
    public static class Implementation
    {
        private static Type _ConnectionImplementation;
        private static Type _ServerImplementation;

        public static Type GetConnectionImplementation()
        {
            return _ConnectionImplementation;
        }

        public static Type GetServerImplementation()
        {
            return _ServerImplementation;
        }

        private static void BaseCheck(Type implementation, Type baseType)
        {
            Type implementationBase = implementation.BaseType;
            while (implementationBase != null)
            {
                if (implementationBase == baseType) return;
            }
            throw new ArgumentException("The provided type does not implement " + baseType.FullName);
        }

        private static void TestImplementation(Type implementation, Type baseType)
        {
            if (implementation == null) throw new ArgumentNullException("connectionImplementor");
            BaseCheck(implementation, baseType);
            // try to create an instance
            try
            {
                Activator.CreateInstance(implementation);
            }
            catch (Exception activationException)
            {
                throw new ArgumentException("Could not create an instance of the type " + implementation.FullName, activationException);
            }
        }

        public static void SetConnectionImplementation(Type connectionImplementation)
        {
            TestImplementation(connectionImplementation, typeof(Connection));
            _ConnectionImplementation = connectionImplementation;
        }
        
        public static void SetServerImplementation(Type serverImplementation)
        {
            TestImplementation(serverImplementation, typeof(Server));
            _ServerImplementation = serverImplementation;
        }

        internal static Server CreateServer()
        {
            if (_ServerImplementation == null) return new Server();

            try
            {
                return Activator.CreateInstance(_ServerImplementation) as Server;
            }
            catch (Exception activationException)
            {
                throw new ArgumentException("Could not create an instance of the type " + _ServerImplementation.FullName, activationException);
            }
        }

        internal static Connection CreateConnection()
        {
            if (_ConnectionImplementation == null) return new Connection();

            try
            {
                return Activator.CreateInstance(_ConnectionImplementation) as Connection;
            }
            catch (Exception activationException)
            {
                throw new ArgumentException("Could not create an instance of the type " + _ConnectionImplementation.FullName, activationException);
            }
        }
    }
}
