namespace Semgus.Model.Smt
{
    /// <summary>
    /// An SMT object source for things specified by the user at run-time
    /// </summary>
    public class SmtUserDefinedSource : ISmtSource
    {
        /// <summary>
        /// The name of this source
        /// </summary>
        public SmtIdentifier Name { get; }

        /// <summary>
        /// Constructs a new user-defined source with the given name
        /// </summary>
        /// <param name="name"></param>
        private SmtUserDefinedSource(SmtIdentifier name)
        {
            Name = name;
        }

        /// <summary>
        /// Cache holding name/source pairs for already constructed sources
        /// </summary>
        private static IDictionary<SmtIdentifier, SmtUserDefinedSource> _cache = new Dictionary<SmtIdentifier, SmtUserDefinedSource>();

        /// <summary>
        /// Gets a user-defined source for the given identifier
        /// </summary>
        /// <param name="id">Identifier for source</param>
        /// <returns>The user-defined source with the given name</returns>
        public static SmtUserDefinedSource ForIdentifier(SmtIdentifier id)
        {
            if (!_cache.TryGetValue(id, out var source))
            {
                source = new SmtUserDefinedSource(id);
                _cache.Add(id, source);
            }
            return source;
        }

        /// <summary>
        /// Gets a user-defined source for the given filename
        /// </summary>
        /// <param name="name">Filename of source</param>
        /// <returns>The user-defined source for the given filename</returns>
        public static SmtUserDefinedSource ForFile(string name)
        {
            SmtIdentifier id = new(name, new SmtIdentifier.Index("file"), new SmtIdentifier.Index(name));
            return ForIdentifier(id);
        }

        /// <summary>
        /// Gets a user-defined source for the given stream name
        /// </summary>
        /// <param name="name">Stream name of source</param>
        /// <returns>The user-defined source for the given stream name</returns>
        public static SmtUserDefinedSource ForStream(string name)
        {
            SmtIdentifier id = new(name, new SmtIdentifier.Index("stream"), new SmtIdentifier.Index(name));
            return ForIdentifier(id);
        }
    }
}
